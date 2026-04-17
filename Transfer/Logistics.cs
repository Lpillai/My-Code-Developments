using Newtonsoft.Json;
using RuntimeVariables;
using RuntimeConfig;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transfer
{
    public class Logistics_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();


        public Logistics_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Logistics app now.";
            }
        }


        public List<Transfer_Preload> FetchOpenPreloads(int pmP21URL, int pmFrom, string pmTo)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Transfer_Preload> thePreloads = new List<Transfer_Preload>();
            string pmJSON = "{'pmP21URL':" + pmP21URL.ToString() + ", 'pmFrom':" + pmFrom.ToString() + ", 'pmTo':" + pmTo + ", 'pmDate':'" + DateTime.Now.ToString("yyyy-MM-dd") + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prP21_GetPreloadList", wkParm);
            if (ResultModel.r == 1)
                thePreloads = JsonConvert.DeserializeObject<List<Transfer_Preload>>(ResultModel.JsonData);

            return thePreloads;
        }


        public Transfer_ShipmentData FetchCustomsClearanceInformation(string pmPickTickets)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            Transfer_ShipmentData theInformation = new Transfer_ShipmentData();
            //string pmJSON = "{'pmP21URL':" + pmP21URL.ToString() + ", 'pmPickTickets':'" + pmPickTickets + "'}";
            string pmJSON = "{'pmPickTickets':'" + pmPickTickets + "'}";
            pmJSON = pmJSON.Replace(" ", "").Replace(";", "");
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            //ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prP21_GetCustomsClearanceInformation", wkParm);
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prP21_InternationalShipmentData", wkParm);
            if (ResultModel.r == 1)
            {
                theInformation = JsonConvert.DeserializeObject<Transfer_ShipmentData>(ResultModel.JsonData);
            }

            return theInformation;
        }


        #region Open Transfers

        public List<Transfer_PlannedRecptDate> FetchOpenTransfersList()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Transfer_PlannedRecptDate> theList = new List<Transfer_PlannedRecptDate>();
            int i = 0;

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prP21_transfer_hdr_list");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theList = JsonConvert.DeserializeObject<List<Transfer_PlannedRecptDate>>(ResultModel.JsonData);

                foreach (var item in theList)
                {
                    i++;
                    item.no = i;
                }
            }

            return theList;
        }


        public SP_Return UpdatePlannedRecptDate(List<Transfer_PlannedRecptDate_Override> pmTransfer)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string json_parm = "";

            //double check
            ResultModel = shareCommon.checkAuthorized(25);
            if (ResultModel.r == 0)
                return ResultModel;

            List<Transfer_PlannedRecptDate_Override> pmUpdate = pmTransfer.Where(w => w.override_planned_recpt_date >= DateTime.Today || !string.IsNullOrWhiteSpace(w.tracking_no)).ToList();
            if (pmUpdate.Count() == 0)
            {
                ResultModel.r = 0;
                ResultModel.msg = "No data updated.";
                return ResultModel;
            }

            json_parm = "{'user': '" + GlobalVariables.MySession.Account + "', 'Override': " + JsonConvert.SerializeObject(pmUpdate) + "}";
            json_parm = json_parm.Replace("'", "\"");
            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prP21_transfer_UpdatePlannedRecptDate", wkParm);
            if (ResultModel.r == 1 && !String.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

        #endregion

        #region TW Logistics

        public Shipping_Orders FetchShippingOrders(int pmP21URL, int pmFrom, string pmTo, DateTime pmDate)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            Shipping_Orders theOrders = new Shipping_Orders();
            int i;
            string pmJSON = "{'pmP21URL':" + pmP21URL.ToString() + ", 'pmFrom':" + pmFrom.ToString() + ", 'pmTo':'" + pmTo + "', 'pmDate':'" + pmDate.ToString("yyyy-MM-dd") + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prP21_GetShippingOrders", wkParm);
            if (ResultModel.r == 1)
            {
                theOrders = JsonConvert.DeserializeObject<Shipping_Orders>(ResultModel.JsonData);

                if (theOrders != null)
                    theOrders.shift_days = 7;
                else
                {
                    theOrders = new Shipping_Orders();
                    theOrders.preloads = new List<ShippingOrder_Preload>();
                    theOrders.pos = new List<ShippingOrder_PurchaseOrder>();
                    theOrders.shift_days = 0;
                }

                theOrders.P21URL = pmP21URL;
                theOrders.required_date = pmDate;
                theOrders.from_loc = pmFrom;
                for (i = 0; i < theOrders.preloads.Count(); i++)
                    theOrders.preloads[i].no = i + 1;
                for (i = 0; i < theOrders.pos.Count(); i++)
                    theOrders.pos[i].no = i + 1;
            }

            return theOrders;
        }


        public async Task<SP_Return> GoExpectedShipDateModifying(Shipping_Orders pm_orders)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<SP_Return> workModel = new List<SP_Return>();
            Shipping_Orders wkOrders = new Shipping_Orders()
            {
                P21URL = pm_orders.P21URL,
                required_date = pm_orders.required_date,
                shift_days = pm_orders.shift_days,
                from_loc = pm_orders.from_loc,
                preloads = pm_orders.preloads.Where(w => w.chk == true).ToList(),
                pos = pm_orders.pos.Where(w => w.chk == true).ToList()
            };

            if ((wkOrders.preloads.Count() <= 0 && wkOrders.pos.Count() <= 0) || wkOrders.shift_days <= 0)
            {
                ResultModel.r = 0;
                ResultModel.msg = "Invalid parameters.";
            }
            else
            {
                foreach (var item in wkOrders.preloads)
                {
                    item.requested_date = item.requested_date.AddDays(wkOrders.shift_days);
                    item.expedite_date = item.expedite_date.AddDays(wkOrders.shift_days);
                    item.expected_date = item.expected_date.AddDays(wkOrders.shift_days);
                    item.expected_ship_date = item.expected_ship_date.AddDays(wkOrders.shift_days);
                }
                foreach (var item in wkOrders.pos)
                {
                    item.expected_date = item.expected_date.AddDays(wkOrders.shift_days);
                    item.expected_ship_date = item.expected_ship_date.AddDays(wkOrders.shift_days);
                }

                ResultModel = await shareCommon.callSBS_API_Post("Transfer/ExpectedShipDateAsync", JsonConvert.SerializeObject(wkOrders));
                if (ResultModel.r == 1)
                {
                    ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);
                    workModel = JsonConvert.DeserializeObject<List<SP_Return>>(ResultModel.JsonData);
                    foreach (var item in workModel)
                        ResultModel.msg += "\r\n\r\n" + (item.r == 1 ? item.msg : item.JsonData);
                }
            }

            return ResultModel;
        }

        #endregion

    }
}
