﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WMS_Database;
using WMS_Interface;

namespace WMS_Kernel
{
    public class PalletManagePresenter:BasePresenter<IPalletManageView>
    {
        WH_WareHouseBll bllWareHouse = new WH_WareHouseBll();
        View_CellBLL bllViewCell = new View_CellBLL();
        View_StockListBLL bllViewStockList = new View_StockListBLL();
        Plan_ListBll bllPlanList = new Plan_ListBll();
        PlanBll bllPlan = new PlanBll();
        View_PlanListBLL bllViewPlanList = new View_PlanListBLL();
         View_GoodsBLL bllView_Goods = new View_GoodsBLL();
         View_GoodsBLL bllViewGoods = new View_GoodsBLL();
         StockBll bllStock = new StockBll();
         Stock_ListBll bllStockList = new Stock_ListBll();
         WH_Station_LogicBLL bllStationLogic = new WH_Station_LogicBLL();
        WH_CellBll bllCell = new WH_CellBll();
         GoodsBll bllGoods = new GoodsBll();
        string currHouseName="";
        string currPalletPos="";
        string currPlanCode = "";
        public PalletManagePresenter(IPalletManageView view,IWMSFrame wmsFrame):base(view,wmsFrame)
        { }
        public override void Init()
        {
            //IniHouseList();
            IniTargetPos();
            IniPlanList();
        }
        public void IniTargetPos( )
        {
            List<string> palletSta = new List<string>();
            //if(houseName == "所有")
            //{
            //    palletSta.Add("所有");
            //    this.View.IniPalletPos(palletSta);
            //    return;
                    
            //}
            //WH_WareHouseModel house = bllWareHouse.GetModelByName(houseName);
            //if (house == null)
            //{
            //    this.View.ShowMessage("信息提示", "不存在此库房");
            //    return;
            //}
            //List<WH_Station_LogicModel> statinList = bllStationLogic.GetStationListByType(house.WareHouse_ID, EnumCellType.配盘工位.ToString());

           
            //if (statinList == null)
            //{
            //    this.View.ShowMessage("信息提示", "此库房不存在配盘工位，请检查配置！");
            //    return;
            //}
            //foreach (WH_Station_LogicModel station in statinList)
            //{
            //    palletSta.Add(station.WH_Station_Logic_Name);
            //}
            List<string> statinList = bllStationLogic.GetStationCellName();
            this.View.IniPalletPos(statinList);
        }

        private void IniHouseList()
        {
            List<WH_WareHouseModel> houseList = bllWareHouse.GetModelList("");
            this.View.IniHouseName(houseList);
        }

        private void IniPlanList()
        {
            List<string> planList = new List<string>();
            List<PlanMainModel> planModelList = bllPlan.GetRunPlanList();
            if (planModelList != null)
            {
                foreach (PlanMainModel plan in planModelList)
                 {
                     planList.Add(plan.Plan_Code);
                 }
            }
            this.View.IniPlanList(planList);
        }
        //private void IniPalletPos(string houseName)
        //{
        //    WH_WareHouseModel house = bllWareHouse.GetModelByName(houseName);
        //    if (house == null)
        //    {

        //        return;
        //    }
        //    List<WH_Station_LogicModel> logicStaion = bllStationLogic.GetAllStation(house.WareHouse_ID);
        //    this.View.IniPalletPos(logicStaion);
        //}
        public void QueryPalletInfo(string palletCode,string palletCellName)
        {
            List<View_StockListModel> stockList = bllViewStockList.GetPalletStock(palletCode, palletCellName);
            ViewDataManager.PALLETMANAGEDATA.PalletInforData.Clear();
            if(stockList==null)
            {
                return;
            }
            foreach (View_StockListModel stock in stockList)
            {
                PalletGoodsListModel pallet = new PalletGoodsListModel();
                //pallet.保质期 = stock.Goods_Shelf_Life.ToString();
                pallet.单位 = stock.Goods_Unit;
                pallet.规格型号 = stock.Goods_Model;
                pallet.托盘条码 = stock.Stock_Tray_Barcode;
                //pallet.生产日期 = (DateTime)stock.Goods_ProduceDate;
                pallet.数量 = int.Parse(stock.Stock_List_Quantity);
                pallet.物料编码 = stock.Goods_Code;
                //pallet.计划列表编号 = stock.Plan_List_ID;

                //Plan_ListModel planListModel = bllPlanList.GetModel(stock.Plan_List_ID);
                //if(planListModel !=null )
                //{
                //    pallet.计划单号 = planListModel.Plan_ID;
                //}
                ViewDataManager.PALLETMANAGEDATA.PalletInforData.Add(pallet);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="houseName">库房名称</param>
        /// <param name="palletPos">配盘工位</param>
        public void QueryPallet( string palletPos, string planCode)
        {
            //this.currHouseName = houseName;
            this.currPalletPos = palletPos;
            this.currPlanCode = planCode;
            string cellChildID = "";
            //if (houseName == "所有")
            //{

            //    cellChildID = "所有";
            //}
            //else
            //{


                //WH_WareHouseModel house = bllWareHouse.GetModelByName(houseName);
                //if (house == null)
                //{

                //    return;
                //}
                //WH_CellModel station = bllStationLogic.GetModelByHouseIDAndCellName(house.WareHouse_ID, palletPos);
                            WH_CellModel station =bllCell.GetStationByName(palletPos);
                if (station == null)
                {
                    this.View.ShowMessage("信息提示", "配盘工位获取失败！");
                    return;
                }

                cellChildID = station.Cell_ID;
            
         

        
            List<View_StockListModel> stockList = bllViewStockList.GetModelList(cellChildID, planCode);

            //var p1 = stockList.Distinct(sl=>sl.Stpcl);  

            ViewDataManager.PALLETMANAGEDATA.PalletList.Clear();
            if (stockList == null)
            {
                return;
            }

            List<View_StockListModel> distinctPallet = new List<View_StockListModel>();//去除重复数据
            foreach (View_StockListModel vsm in stockList)
            {
                var existPallet = distinctPallet.Where(s => s.Stock_Tray_Barcode == vsm.Stock_Tray_Barcode);
                if (existPallet == null || existPallet.Count() == 0)
                {
                    distinctPallet.Add(vsm);
                }
            }


            foreach (View_StockListModel stock in distinctPallet)
            {
                PalletListData pallet = new PalletListData();
                if (planCode != "所有")
                {
                    View_PlanListModel planList = bllViewPlanList.GetModelByPlanListID(stock.Plan_List_ID);
                    if (planList == null)
                    {
                        return;
                    }
                    if (stock.Plan_List_ID == "-1")
                    {
                        pallet.按计划配盘 = "否";
                    }
                    else
                    {
                        pallet.按计划配盘 = "是";

                        pallet.计划单号 = planList.Plan_Code;
                    }
                }
                else
                {
                    pallet.按计划配盘 = "否";
                    pallet.计划单号 = "-1";
                }
              
           
                pallet.配盘时间 = stock.Stock_List_Entry_Time.ToString();
                pallet.托盘条码 = stock.Stock_Tray_Barcode;
                pallet.配盘工位名称 = stock.Cell_Name;
                ViewDataManager.PALLETMANAGEDATA.PalletList.Add(pallet);

            }
        }

        public void ShowGoods(string goodsInfor)
        {
            List<View_GoodsModel> goodsList = bllView_Goods.GetModelByGoosInfo(goodsInfor);
            ViewDataManager.PALLETMANAGEDATA.PalletGoodsData.Clear();
            if (goodsList == null)
            {
                return;
            }

            //ViewDataManager.GOODSVIEWDATA.GoodsListData.Clear();
            foreach (View_GoodsModel goods in goodsList)
            {
                GoodsDataModel gsdModel = new GoodsDataModel();
                gsdModel.单位 = goods.Goods_Unit;
                gsdModel.启用 = goods.Goods_Flag;
                gsdModel.物料编码 = goods.Goods_Code;
                gsdModel.物料类别 = goods.Goods_Category;
                gsdModel.物料名称 = goods.Goods_Name;
                gsdModel.物料内部编码 = goods.Goods_ID.ToString();

                ViewDataManager.PALLETMANAGEDATA.PalletGoodsData.Add(gsdModel);
            }
        }

        public void AddTrayGoods(string trayCode, int goodsNum, string goodsCode)
        {
            if (trayCode.Trim() == "")
            {
                this.View.ShowMessage("信息提示", "请输入托盘条码！");
                return;
            }
            if (goodsNum == 0)
            {
                this.View.ShowMessage("信息提示", "请输入物料配盘数量！");
                return;
            }
            if (IsExistPalletGoods(goodsCode) == true)
            {
                this.View.ShowMessage("信息提示", "此物料已经在配盘中！");
                return;
            }
            View_GoodsModel goodsModel = bllViewGoods.GetModelByGoodsCode(goodsCode);
            if (goodsModel == null)
            {
                return;
            }
            PalletGoodsListModel tglm = new PalletGoodsListModel();
            tglm.单位 = goodsModel.Goods_Unit;
            tglm.规格型号 = goodsModel.Goods_Model;
            tglm.托盘条码 = trayCode;
            //tglm.生产日期 = createDatetime;
            tglm.数量 = goodsNum;
            tglm.物料编码 = goodsCode;

            ViewDataManager.PALLETMANAGEDATA.PalletInforData.Add(tglm);
        }
        public void DeleteTrayGoods(string goodsCode)
        {
            for (int i = 0; i < ViewDataManager.PALLETMANAGEDATA.PalletInforData.Count; i++)
            {
                PalletGoodsListModel tglm = ViewDataManager.PALLETMANAGEDATA.PalletInforData[i];
                if (tglm.物料编码 == goodsCode)
                {
                    ViewDataManager.PALLETMANAGEDATA.PalletInforData.Remove(tglm);
                    break;
                }

            }
        }

        public void TrayConfirm( bool isFull, string palletCode, string recCellName)
        {
            try
            {
                if (ViewDataManager.PALLETMANAGEDATA.PalletInforData.Count == 0)
                {
                    this.View.ShowMessage("信息提示", "请添加配盘物料！");
                    return;
                } 
                //WH_WareHouseModel house = bllWareHouse.GetModelByName(houseName);
                //if (house == null)
                //{
                   
                //    return ;
                //}
                WH_CellModel cell = bllCell.GetStationByName(recCellName);

                //WH_Station_LogicModel cell = bllStationLogic.GetStationByName(house.WareHouse_ID,recCellName);
                if (cell == null)
                {
                    this.View.ShowMessage("信息提示", "配盘地点错误！");
                    return;
                }
                StockModel stockModel = bllStock.GetModelByTrayCode(palletCode);
                if (stockModel == null)
                {
                    this.View.ShowMessage("信息提示", "此托盘条码不在库存中！");
                    return;
                }
                stockModel.Cell_Child_ID = cell.Cell_ID;
                stockModel.Stock_Tray_Barcode = palletCode;
                if (isFull == true)
                {
                    stockModel.Stock_Full_Flag = "1";
                }
                else
                {
                    stockModel.Stock_Full_Flag = "0";
                }
              
                bllStock.Update(stockModel);
                Stock_ListModel stockListTemp = bllStockList.GetModelByPalletCode(palletCode);
                if(stockListTemp == null)
                {
                    this.View.ShowMessage("信息提示", "此托盘中没有物料！");
                    return;
                }
                bllStockList.DeleteByStockID(stockModel.Stock_ID);
                for (int i = 0; i < ViewDataManager.PALLETMANAGEDATA.PalletInforData.Count; i++)
                {
                    Stock_ListModel stockList = new Stock_ListModel();
                    stockList.Stock_List_ID = Guid.NewGuid().ToString();
                    stockList.Stock_ID = stockModel.Stock_ID;
                    PalletGoodsListModel trayGoodsModel = ViewDataManager.PALLETMANAGEDATA.PalletInforData[i];
                    GoodsModel goods = bllGoods.GetModelByCode(trayGoodsModel.物料编码);
                    if (goods == null)
                    {
                        continue;
                    }
                    stockList.Goods_ID = goods.Goods_ID;
                    stockList.Plan_List_ID = stockListTemp.Plan_List_ID;
                    stockList.Stock_List_Box_Barcode = palletCode;
                    stockList.Stock_List_Entry_Time = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    stockList.Stock_List_Quantity = trayGoodsModel.数量.ToString();
                    bllStockList.Add(stockList);

                }
                this.View.ShowMessage("信息提示", "配盘成功！");
                QueryPallet( currPalletPos, currPlanCode);
            }
            catch (Exception ex)
            {
                this.View.ShowMessage("信息提示", "配盘失败！" + ex.Message);
            }

        }

        public void CancelPallet(string palletCode)
        {
            try
            {
                string restr ="";
                StockModel stock = bllStock.GetModelByTrayCode(palletCode);
                if(stock == null)
                {
                    return;
                }
              
                List<Stock_ListModel> stockList = bllStockList.GetListByStockID(stock.Stock_ID);
                if(stockList == null)
                {
                     this.WmsFrame.WriteLog("配盘管理","","错误","计划下达数量更新错误！");
                    return;
                }
             
                foreach(Stock_ListModel slModel in stockList)
                {
                    if (slModel.Plan_List_ID == "-1")
                    {
                        continue;
                    }
                    if (UpdatePlanNum(slModel.Plan_List_ID, slModel.Goods_ID, int.Parse(slModel.Stock_List_Quantity), ref restr) == false)
                    {
                        this.View.ShowMessage("信息提示", "取消配盘是失败！"+ restr);
                        return;
                    }
                }
                bllStock.Delete(stock.Stock_ID);
                this.View.ShowMessage("信息提示", "取消配盘成功！");
                ViewDataManager.PALLETMANAGEDATA.PalletInforData.Clear();
                QueryPallet( currPalletPos, currPlanCode);

            }
            catch(Exception ex)
            {
                this.View.ShowMessage("信息提示", "取消配盘失败！" + ex.Message);
            }
        }

        private bool UpdatePlanNum(string planListID, string materialCode, int materialNum, ref string restr)
        {

            Plan_ListModel planListModel = bllPlanList.GetModel(planListID);
            if (planListModel == null)
            {
                restr = "计划列表编码错误：" + planListID;
                return false;
            }
            int orderNum = 0;
            if (planListModel.Plan_List_Ordered_Quantity.Trim() != "")
            {
                orderNum = int.Parse(planListModel.Plan_List_Ordered_Quantity);
            }
            orderNum -= materialNum;
            planListModel.Plan_List_Ordered_Quantity = orderNum.ToString();
            bllPlanList.Update(planListModel);
            return true;
        }
        private bool IsExistPalletGoods(string goodsCode)
        {
            foreach (PalletGoodsListModel goods in ViewDataManager.PALLETMANAGEDATA.PalletInforData)
            {
                if (goods.物料编码 == goodsCode)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
