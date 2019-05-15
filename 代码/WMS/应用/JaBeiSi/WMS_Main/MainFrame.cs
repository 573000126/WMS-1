﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraTab;
using DevExpress.XtraEditors;
using WMS_Interface;
using WMS_Kernel;
using LicenceManager;
using System.Configuration;
using CommonMoudle;
using WMS_Database;
using DevExpress.Utils;
namespace WMS_Main
{
    public partial class MainFrame : ChildViewBase, IWMSFrame, ILicenseNotify
    {
        private delegate void DelegateDispLog(string logSrc, string category,string level, string content);//委托，显示日志
        private LicenseMonitor licenseMonitor = null;
        MainPresenter mainPresenter = new MainPresenter();
        public MainFrame()
        {
            InitializeComponent();
        }
        public void SetRoleLevel(int roleLevel,string userName)
        {
            this.RoleLevel = roleLevel;
            this.CurrentUser = userName;
            this.bsi_CurrentUser.Caption = userName;
        }
        public  void Init()
        {
           
         
            Console.SetOut(new TextBoxWriter(this.richTextBoxLog));
            InitTabbedMDI();
            
            this.mainPresenter.Init(this);
         
        }
        private void MainView_Load(object sender, EventArgs e)
        {
         
            this.ribbon_Title.ApplicationCaption = LoginView.WMSName;
            string licenseFile = AppDomain.CurrentDomain.BaseDirectory + @"\JCJLicense.lic";
            this.licenseMonitor = new LicenseMonitor(this, 2000, licenseFile, "zzkeyFT1");
            if (!this.licenseMonitor.StartMonitor())
            {
                throw new Exception("license 监控失败");
            }
            string reStr = "";
            if (!this.licenseMonitor.IslicenseValid(ref reStr))
            {
                MessageBox.Show(reStr);
                return;
            }
        }
        #region ILicenseNotify接口实现
        public void ShowWarninfo(string info)
        {
           
            WriteLog("程序主框架", "", EnumLoglevel.提示.ToString(), info);
        }
        public void LicenseInvalid(string warnInfo)
        {

            AbortApp();
            //nodeMonitorView.AbortApp();
            //this.bt_StartSystem.Enabled = false;
            //LogModel log = new LogModel("其它", warnInfo, EnumLoglevel.警告);
            //logView.GetLogrecorder().AddLog(log);
        }
        private delegate void AbortAppDelegate(bool enabled);
     

        private void EnabledApp(bool enabled)
        {
            if (this.ribbon_Title.InvokeRequired)
            {
                AbortAppDelegate disenable = new AbortAppDelegate(EnabledApp);
                this.ribbon_Title.Invoke(disenable,new object[1]{enabled});
            }
            else
            {
                this.ribbon_Title.Enabled = enabled;
            }
        }
        
        public void AbortApp()
        {
            EnabledApp(false);
        }
        public void LicenseReValid(string noteInfo)
        {
            EnabledApp(true);
            WriteLog("程序主框架", "", EnumLoglevel.提示.ToString(), noteInfo);
          
        }
        #endregion
        #region 实现IWMS
    
        public  void SetVersion(string version)
        { 
        
        }
        public void SetTabChangeEvent(EventHandler tabChangeEventHandler)
        {
            this.xtraTabbedMdiManager1.SelectedPageChanged += tabChangeEventHandler;
        }

        public  void WriteLog(string logSrc, string category, string level, string content)
        {
            if (this.richTextBoxLog.InvokeRequired)
            {
                DelegateDispLog delegateLog = new DelegateDispLog(WriteLog);
                this.Invoke(delegateLog, new object[4] { logSrc, category, level, content });
            }
            else
            {

                richTextBoxLog.AppendText(string.Format("[{0:yyyy-MM-dd HH:mm:ss.fff}]{1},{2},{3}", DateTime.Now.ToString(), logSrc, category, content) + Environment.NewLine);

                string[] newlines = new string[richTextBoxLog.Lines.Length];
                Array.Copy(richTextBoxLog.Lines, richTextBoxLog.Lines.Length - newlines.Count(), newlines, 0, newlines.Count());
                richTextBoxLog.Lines = newlines;
                richTextBoxLog.Select(richTextBoxLog.Text.Length, 0);
                richTextBoxLog.ScrollToCaret();

                if (this.richTextBoxLog.Lines.Length > 600)//600行数据
                {
                    this.richTextBoxLog.Clear();
                }
                SysLogModel logModel = new SysLogModel();
                logModel.SysLog_ID = Guid.NewGuid().ToString();
                logModel.SysLog_Level = level;
                logModel.SysLog_Content = content;
                logModel.SysLog_Source = logSrc;
                logModel.SysLog_Time = DateTime.Now;
                this.mainPresenter.AddDBLog(logModel);

            }
        }

        public  void ShowView(Form tabForm, bool isTab)
        {
            if (isTab == true)
            {
                tabForm.MdiParent = this;

                tabForm.Show();
            }
            else
            {
                tabForm.ShowDialog();
            }
            this.xtraTabbedMdiManager1.SelectedPage = this.xtraTabbedMdiManager1.Pages[tabForm];
        }
        public  bool AddTitlePage(string pageName,ref string restr)
        {
          
            if (this.ribbon_Title.Pages.GetPageByName(pageName) != null)
            {
                restr = "存在同名标题页！";
                return false;
            }
            DevExpress.XtraBars.Ribbon.RibbonPage addPage = new DevExpress.XtraBars.Ribbon.RibbonPage();
            addPage.Name = pageName;
            addPage.Text = pageName;
            this.ribbon_Title.Pages.Add(addPage);
            restr = "标题页添加成功！";
            return true;
        }
        public  bool AddGroup(string pageName, string groupName, ref string restr)
        {
            DevExpress.XtraBars.Ribbon.RibbonPage page = this.ribbon_Title.Pages.GetPageByName(pageName);
            if (page == null)
            {
                restr = "不存在[" + page + "]标题页！";
                return false;
            }

            if (page.Groups.GetGroupByName(groupName) != null)
            {
                restr = "存在同名组！";
                return false;
            }
            DevExpress.XtraBars.Ribbon.RibbonPageGroup group = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            group.Name = groupName;
            group.Text = groupName;
            page.Groups.Add(group);
            this.ribbon_Title.DrawGroupCaptions = DefaultBoolean.False;
            restr = "添加组成功！";
            return true;
        }

        public  bool AddButtonItem(string pageName, string groupName, string itemName, Image itemImage, ItemClickEventHandler callBack, ref string restr)
        {
            DevExpress.XtraBars.Ribbon.RibbonPage page = this.ribbon_Title.Pages.GetPageByName(pageName);
            if (page == null)
            {
                restr = "不存在[" + page + "]标题页！";
                return false;
            }
            DevExpress.XtraBars.Ribbon.RibbonPageGroup group = page.Groups.GetGroupByName(groupName);
            if (group == null)
            {
                restr = "不存在[" + groupName + "]组！";
                return false;
            }
            DevExpress.XtraBars.BarButtonItem buttonItem = new BarButtonItem();

            buttonItem.Name = itemName;
            buttonItem.Caption = itemName;
            buttonItem.ImageOptions.LargeImage = itemImage;
            //buttonItem.ImageOptions.Image = itemImage;
            //buttonItem.RibbonStyle = ((DevExpress.XtraBars.Ribbon.RibbonItemStyles)(((DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large | DevExpress.XtraBars.Ribbon.RibbonItemStyles.SmallWithText)
            //| DevExpress.XtraBars.Ribbon.RibbonItemStyles.SmallWithoutText)));
            buttonItem.ItemClick += callBack;
            group.ItemLinks.Add(buttonItem);
            restr = "添加菜单项成功！";
            return true;
        }

        public  string CurrentUser { get; set; }
        public int RoleLevel { get; set; }
        #endregion
        #region 私有方法
        //private void DatabaseCfg()
        //{
        //    string dbSrc = ConfigurationManager.AppSettings["DBSource"];
        //    //CtlDBAccess.DBUtility.PubConstant.ConnectionString = string.Format(@"{0}Initial Catalog=ACEcams;User ID=sa;Password=123456;", dbSrc);
        //    string dbConn1 = string.Format(@"{0}Initial Catalog=JBSWmsDB;User ID=sa;Password=123456;", dbSrc);
        //   // string dbConn1 = string.Format(@"{0}Initial Catalog=WMSDB2;User ID=AoyouWmsSA;Password=Aa123456;", dbSrc);
        //    WMS_Database.PubConstant.SetConnectStr(dbConn1);
        //}
        private void InitTabbedMDI()
        {
            DevExpress.UserSkins.BonusSkins.Register();
            DevExpress.Skins.SkinManager.EnableFormSkins();
            //Dark Side,Visual Studio 2013 Blue,Office 2007 Black,Office 2010 Silver,Office 2007 Blue,DevExpress Dark Style,DevExpress Style,Office 2016 Black
            this.defaultLookAndFeel1.LookAndFeel.SetSkinStyle("Stardust");
            this.xtraTabbedMdiManager1.MdiParent = this;

        }

        private void FormCloseEventHandler(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Form f = (Form)sender;
                f.Visible = false;
            }

        }
        private void LoadApp()
        {
            //AYUI.BatteryView battery = new AYUI.BatteryView(this);
            //battery.FormClosing += OnFormCloseEventHandler;

            //AYUI.PalletView pallet = new AYUI.PalletView(this);
            //pallet.FormClosing += OnFormCloseEventHandler;

        }


        #endregion

      
   
       
        private void barBtnItem_RoleMana_ItemClick(object sender, ItemClickEventArgs e)
        {
            //if (roleView == null)
            //{
            //    roleView = new RoleManaView();
            //}
             
            //this.ShowTab(roleView);
        }
       
        private void MainView_FormClosing(object sender, FormClosingEventArgs e)
        {

            DialogResult result = DevExpress.XtraEditors.XtraMessageBox.Show("您确定要退出系统么？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                //this.xtraTabbedMdiManager1.Pages.Clear();
                //System.Environment.Exit(0);
                Application.ExitThread();
                //Application.Exit();

            }
            else
            {
                e.Cancel = true;
            }
        }

  
        private void batBtnItem_Stop_ItemClick(object sender, ItemClickEventArgs e)
        {
            //string restr = "";
            //AddTitlePage("系统",ref restr);
            //AddGroup("系统", "系统配置", ref restr);
            //AddButtonItem("系统", "系统配置", "修改密码", this.imageCollection1.Images[0], null, ref restr);
        }

        private void batBtnItem_Modify_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void 清空日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxLog.Clear();
        }

        private void bti_About_ItemClick(object sender, ItemClickEventArgs e)
        {
            AboutView av = new AboutView(LoginView.WMSName);
            string sysVersion = "1.0.15";
            string aboutStr = "版本：" + sysVersion + " \r\n \r\n"
                + "日期：" +"2019-5-12" + "\r\n \r\n"
                + "（Copyright）深圳捷创嘉智能物流装备有限公司";
               av.SetVersion(aboutStr);
            av.ShowDialog();
        }
    }
}