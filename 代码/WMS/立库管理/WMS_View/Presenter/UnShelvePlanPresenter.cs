﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WMS_Database;
using WMS_Interface;

namespace WMS_Kernel
{
    public class UnShelvePlanPresenter:BasePresenter<IUnShelvePlanView>
    {
       
        View_StockListBLL bllViewStockList = new View_StockListBLL();
        View_PlanListBLL bllViewPlanList = new View_PlanListBLL();
        Stock_ListBll bllStockList = new Stock_ListBll();
        Manage_ListBll bllManageList = new Manage_ListBll();
        WH_CellBll bllCell = new WH_CellBll();
        WH_Cell_ChildrenBll bllCellChild = new WH_Cell_ChildrenBll();
        View_CellBLL bllViewCell = new View_CellBLL();
        View_Plan_StockListBLL bllViewPlanStockList = new View_Plan_StockListBLL();
        ManageBll bllManage = new ManageBll();

        private static View_ManageStockListBLL bllViewManageStockList = new View_ManageStockListBLL();

        Func<UnShelveParams, ReturnObject> allowUnShelve = null;
        private static StockBll bllStock = new StockBll();

        PlanBll bllPlan = new PlanBll();
        Plan_ListBll bllPlanList = new Plan_ListBll();

        View_ManageBLL bllViewManage = new View_ManageBLL();
        View_ManageListBLL bllViewManageList = new View_ManageListBLL();
        View_PlanMainBLL bllViewPlanMain = new View_PlanMainBLL();
        WH_Station_LogicBLL bllStationLogic = new WH_Station_LogicBLL();
        WH_WareHouseBll bllWareHouse = new WH_WareHouseBll();
        private string currPlanCode = "";
        public UnShelvePlanPresenter(IUnShelvePlanView view,IWMSFrame wmsFrame):base(view,wmsFrame)
        { }
        public override void Init()
        {
            IniPlanList();
            IniHouseList();
            //GetUnShelveStation();
        }
        public void RegistUnShelve(Func<UnShelveParams, ReturnObject> unShelve)
        {
            this.allowUnShelve = unShelve;
        }
        public void IniPlanList()
        {
            List<View_PlanMainModel> planList = bllViewPlanMain.GetPlanListByStatus("2", EnumPlanStatus.执行中.ToString());//下架
            this.View.IniPlanList(planList);
        }
        public void IniUnShelveStationList (string houseName)
        {
            WH_WareHouseModel house = bllWareHouse.GetModelByName(houseName);
            if(house==null)
            {
                return;
            }
            string houseID = "";
            //if (houseName == "A1库房" || houseName == "A2库房" || houseName == "A3库房" || houseName == "A4库房" || houseName == "A5库房")
            //{
            //    houseID = "1-5库房";
            //}
            //else
            //{
                houseID = house.WareHouse_ID;
            //}
            List<WH_Station_LogicModel> staionList = bllStationLogic.GetModelListByHouseIDAndCellType(houseID, EnumCellType.下架工位.ToString());
            List<string> stationList = new List<string>();


            foreach (WH_Station_LogicModel station in staionList)
            {
                stationList.Add(station.WH_Station_Logic_Name);
            }
            this.View.IniTargetList(stationList);
        }
        public void QueryPlan(string planCode)
        {
            this.currPlanCode = planCode;
            //List<View_Plan_StockListModel> stockList = bllViewStockList.GetModelByPlanID(planID);
            List<View_PlanListModel> planList = bllViewPlanList.GetModelByPlanCode(planCode);

            ViewDataManager.UNSHELVEPALNDATA.PlanListData.Clear();
            if (planList == null || planList.Count == 0)
            {
                //this.View.ShowMessage("信息提示", "无此计划信息！");
                return;
            }
            foreach (View_PlanListModel stockModel in planList)
            {
                PlanListModel planModel = new PlanListModel();
                planModel.规格型号 = stockModel.Goods_Model;
                planModel.计划数量 = stockModel.Plan_List_Quantity;
                planModel.计量单位 = stockModel.Goods_Unit;
                planModel.完成数量 = stockModel.Plan_List_Finished_Quantity.ToString();
                planModel.物料编码 = stockModel.Goods_Code;
                planModel.物料名称 = stockModel.Goods_Name;
                planModel.下达数量 = stockModel.Plan_List_Ordered_Quantity.ToString();
                planModel.计划列表编码 = stockModel.Plan_List_ID;
                planModel.计划单号 = stockModel.Plan_ID;
                planModel.物料批次 = stockModel.Plan_List_Remark;
                if (stockModel.Plan_Create_Time!= null)
                {
                    planModel.计划创建时间 = stockModel.Plan_Create_Time.ToString();
                }
               
                ViewDataManager.UNSHELVEPALNDATA.PlanListData.Add(planModel);
            }

            
        }
        private void IniHouseList()
        {
            //List<WH_WareHouseModel> houseList = bllWareHouse.GetModelList("");
            //this.View.IniHouseName(houseList);
        }
        public void GetUnShelveStation(string houseName)
        {
            //if (palletCode.Trim() == "")
            //{
            //    this.View.ShowMessage("信息提示", "请选择指定货位下架！");
            //    return;
            //}
            //View_StockListModel stockListModel = bllViewStockList.GetModelByPalletCode(palletCode,EnumCellType.货位.ToString());
            //if(stockListModel == null)
            //{
            //    this.View.ShowMessage("信息提示", "此货位无库存！");
            //    return;
            //}
            WH_WareHouseModel house = bllWareHouse.GetModelByName(houseName);
            if (house == null)
            {
                this.View.ShowMessage("信息提示", "不存在此库房");
                return;
            }

            List<WH_Station_LogicModel> staionList = bllStationLogic.GetStationListByType(house.WareHouse_ID,EnumCellType.下架工位.ToString());
            List<string> stationList = new List<string>();


            foreach (WH_Station_LogicModel station in staionList)
            {
                stationList.Add(station.WH_Station_Logic_Name);
            }
            this.View.IniTargetList(stationList);

        }

        public void QueryStockInfor(string goodsCode)
        {
            if (goodsCode.Trim() == "")
            {
                this.View.ShowMessage("信息提示", "请输入物料编码！");
                return;
            }
            List<View_StockListModel> stockList = bllViewStockList.GetModelListByGoodsCode(goodsCode,EnumCellType.货位.ToString());
            ViewDataManager.UNSHELVEPALNDATA.PalletInforData.Clear();
            if (stockList == null || stockList.Count == 0)
            {
                this.View.ShowMessage("信息提示", "库房无此物料信息！");
                return;
            }
            foreach (View_StockListModel stock in stockList)
            {
                PalletInfor palletInfor = new PalletInfor();
                palletInfor.库房 = stock.WareHouse_Name;
                palletInfor.存储货位 = stock.Cell_Name+"-"+stock.Cell_Chlid_Position;
                palletInfor.存储库区 = stock.Area_Name;
                palletInfor.更新时间 = stock.Stock_List_Update_Time.ToString();
                palletInfor.规格型号 = stock.Goods_Model;
                palletInfor.计量单位 = stock.Goods_Unit;
                palletInfor.入库时间 = stock.Stock_List_Entry_Time.ToString();
                //palletInfor.生产日期 = stock.Goods_ProduceDate;
                palletInfor.是否满盘 = stock.Stock_Full_Flag;
               
                palletInfor.数量 = stock.Stock_List_Quantity;
                palletInfor.托盘条码 = stock.Stock_Tray_Barcode;
                palletInfor.物料编码 = stock.Goods_Code;
                palletInfor.物料名称 = stock.Goods_Name;
                ViewDataManager.UNSHELVEPALNDATA.PalletInforData.Add(palletInfor);
            }

        }

        public void UnShelveTask(string planCode,string planlistCode,string num,string palletCode,string houseName, string unshelveStationName)
        {
            //查看当前是否已经有此托盘条码的上架管理任务
            View_Manage_ListModel manage = bllViewManageList.GetModelByPalletCodeAndTaskType(palletCode, EnumManageTaskType.下架.ToString(), EnumManageTaskStatus.待执行.ToString());
            if(manage!=null)
            {
                //this.WmsFrame.WriteLog("下架逻辑", "", "提示", "当前托盘下架任务已经下发！");
                this.View.ShowMessage("信息提示", "当前托盘下架任务已经下发！");
                return;
            }
            if (CommonMoudle.TaskHandleMethod.IsOrderNumBiggerThanPlan(planlistCode, num) == true)
            {
                if(this.View.AskMessage("询问？","当前计划物料下达数量已经大于计划数量，您确定还要下达吗？")!=0)
                {
                    return;
                }
            }

            string restr = "";

            string manageID = "";

            //if(CheckMaterialNum(ref restr) == false)//先不加个数校验
            //{
            //    this.View.ShowMessage("信息提示", restr);
            //    return ;
            //}

            ReturnObject allowCreateTask = new ReturnObject();
            allowCreateTask.Status = true;
            if (this.allowUnShelve != null)
            {
                WH_WareHouseModel house = bllWareHouse.GetModelByName(houseName);
                if (house == null)
                {
                    this.View.ShowMessage("信息提示", "库房获取失败！");
                    return;
                }
                UnShelveParams unshelveParams = new UnShelveParams();
                unshelveParams.WareHouseCode = house.WareHouse_Code;
                unshelveParams.PalletCode = palletCode;

                allowCreateTask = this.allowUnShelve(unshelveParams);
            }
            if (allowCreateTask.Status == false)
            {
                this.View.ShowMessage("信息提示", allowCreateTask.Describe);
                return;
            }

            if (CommonMoudle.TaskHandleMethod.CreateUnshelveManageTask(planCode, palletCode, houseName, unshelveStationName, ref manageID, ref restr) == false)
            {
                this.WmsFrame.WriteLog("下架逻辑", "", "提示", restr);
                return;
            }
            if (CommonMoudle.TaskHandleMethod.UpdatePlanOrderedNum(manageID) == false)
            {
                this.WmsFrame.WriteLog("下架逻辑", "", "提示", "更新计划下达数据数量失败：计划编码：" + planCode + "，托盘号：" +palletCode);
                return;
            }
           
            //下架管理任务生成完毕后需要更新货位状态,计划状态根据管理任务状态更新
            if(UpateCellStatus(palletCode, EnumGSOperate.出库, EnumGSTaskStatus.锁定) == false)
            {
                return;
            }
            this.WmsFrame.WriteLog("下架逻辑", "", "提示", restr);
            QueryPlan(this.currPlanCode);
        }

        private bool CheckMaterialNum(ref string restr)
        {
            Dictionary<string, float> materialNum = new Dictionary<string, float>();
            for (int i = 0; i <  ViewDataManager.UNSHELVEPALNDATA.PalletInforData.Count; i++)  
            {
                PalletInfor trayGoodsModel = ViewDataManager.UNSHELVEPALNDATA.PalletInforData[i];
                if (materialNum.Keys.Contains(trayGoodsModel.物料编码) == false)
                {
                    materialNum[trayGoodsModel.物料编码] = float.Parse(trayGoodsModel.数量);
                    continue;
                }
                materialNum[trayGoodsModel.物料编码] += float.Parse(trayGoodsModel.数量);
            }
            foreach (KeyValuePair<string, float> keyValue in materialNum)
            {
                int planNum = GetPlanMateriNum(keyValue.Key);
                if (keyValue.Value > planNum)
                {
                    restr = "物料：" + keyValue.Key + "超出计划剩余的数量（计划数量-下达数量）";
                    return false;
                }
            }
            return true;

        }
        private int GetPlanMateriNum(string materialCode)
        {
            foreach (PlanListModel planDetail in ViewDataManager.UNSHELVEPALNDATA.PlanListData)
            {
                if (planDetail.物料编码 == materialCode)
                {
                    return (int.Parse(planDetail.计划数量) - int.Parse(planDetail.下达数量));
                }
            }
            return 0;
        }
     
        private bool UpateCellStatus(string palletCode, EnumGSOperate cellOperStatus, EnumGSTaskStatus taskStatus)
        {
            View_StockListModel stockModel = bllViewStockList.GetModelByPalletCode(palletCode,EnumCellType.货位.ToString());
            if (stockModel == null)
            {
                this.WmsFrame.WriteLog("下架逻辑", "", "提示", "更新货位状态时，没有找到所选物料库存！");
                return false;
            }
            WH_Cell_ChildrenModel cellChildModel = bllCellChild.GetModel(stockModel.Cell_Child_ID);
            if (cellChildModel == null)
            {
                this.WmsFrame.WriteLog("下架逻辑", "", "提示", "更新货位状态时，没有找到所选物料货位！");
                return false;
            }
            cellChildModel.Cell_Child_Run_Status = taskStatus.ToString();
            cellChildModel.Cell_Child_Operate_Type = cellOperStatus.ToString();
            bllCellChild.Update(cellChildModel);
            this.WmsFrame.WriteLog("下架逻辑", "", "提示", "更新货位状态成功！");
            return true;
        }
        
            
    }
}
