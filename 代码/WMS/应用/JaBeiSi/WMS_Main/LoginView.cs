﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using WMS_Interface;
using WMS_Kernel;
using System.Configuration;
using DevExpress.XtraSplashScreen;

namespace WMS_Main
{
    public partial class LoginView : ChildViewBase,IAddUserView
    {
        private AddUserPresenter presenter = null;
        private SplashScreenManager _loadForm;
        public static string WMSName = "佳贝斯-WMS立库管理系统";//系统名称配置
        public LoginView()
        {
            InitializeComponent();
            this.presenter = new AddUserPresenter(this);
            DatabaseCfg();
            InitTabbedMDI();
            this.lc_WMSName.Text = WMSName;
          
        }

        public override void Init(IWMSFrame wmsFrame)
        {
            base.Init(wmsFrame); 
        }

       
        protected SplashScreenManager LoadForm
        {
            get
            {
                if (_loadForm == null)
                {
                    this._loadForm = new SplashScreenManager(this, typeof(WaitForm1), true, true);
                    this._loadForm.ClosingDelay = 0;
                }
                return _loadForm;
            }
        }
 

        public void ShowWaitForm()
        {
            bool flag = !this.LoadForm.IsSplashFormVisible;
            if (flag)
            {
                this.LoadForm.ShowWaitForm();
            }
        }

        public void HideWaitForm()
        {
            bool isSplashFormVisible = this.LoadForm.IsSplashFormVisible;
            if (isSplashFormVisible)
            {
                this.LoadForm.CloseWaitForm();
            }
        }

        #region 私有方法
        private void DatabaseCfg()
        {
            string dbSrc = ConfigurationManager.AppSettings["DBSource"];
                string dbAPPSrc = ConfigurationManager.AppSettings["WMS_ERP_Database_Path"];
            //CtlDBAccess.DBUtility.PubConstant.ConnectionString = string.Format(@"{0}Initial Catalog=ACEcams;User ID=sa;Password=123456;", dbSrc);
            string dbConn1 = string.Format(@"{0}Initial Catalog=JBSWmsDB;User ID=sa;Password=Aa123456;", dbSrc);
            string dbConn2 = string.Format(@"{0}Initial Catalog=JBS_APPDB;User ID=sa;Password=Aa123456;", dbAPPSrc);

            WMS_Database.PubConstant.SetConnectStr(dbConn1);
            JBS_APPDB.PubConstant.SetConnectStr(dbConn2);
        }
        //public void ShowMessage(string title, string content)
        //{
        //    DevExpress.XtraEditors.XtraMessageBox.Show(content, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

        //}
        #endregion
        private void sBtn_Cancel_Click(object sender, EventArgs e)
        {
            this.txtEdit_UserName.Text = string.Empty;
            this.txtEdit_UserPassword.Text = string.Empty;
            this.Close();
            
        }
        private void InitTabbedMDI()
        {
            DevExpress.UserSkins.BonusSkins.Register();
            DevExpress.Skins.SkinManager.EnableFormSkins();
            //Dark Side,Visual Studio 2013 Blue,Office 2007 Black,Office 2010 Silver,Office 2007 Blue,DevExpress Dark Style,DevExpress Style,Office 2016 Black
            this.defaultLookAndFeel1.LookAndFeel.SetSkinStyle("Stardust");
          
        }
        private void sBtn_Sure_Click(object sender, EventArgs e)
        {
         
            User user = new User();
            user.UserName = this.txtEdit_UserName.Text;
            user.UserPassword = this.txtEdit_UserPassword.Text;
            int roleLevel = 0;
            if(!this.presenter.CheckLoginUser(user,ref roleLevel))
            {
                this.ShowMessage("信息提示", "该用户不存在，请重新输入用户名称！");
                return;
            }
            if (!this.presenter.CheckLoginPassword(user))
            {
                this.ShowMessage("信息提示", "用户密码不正确，请重新输入用户密码！");
                return;
            }
            this.Hide();
            this.ShowWaitForm();
            MainFrame mainView = new MainFrame();
            mainView.SetRoleLevel(roleLevel,user.UserName);
            mainView.Init();
            this.HideWaitForm();
            mainView.Show();
           
          
        }

        private void LoginView_Load(object sender, EventArgs e)
        {
            DatabaseCfg();
        }
 

 
    }
}