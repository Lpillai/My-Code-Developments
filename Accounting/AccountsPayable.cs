using Newtonsoft.Json;
using OfficeOpenXml;
using RuntimeVariables;
using RuntimeConfig;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Accounting
{
    public class AP_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();
        Filters_Share shareFilters = new Filters_Share();
        //Accounting_Fileter filterAccounting = new Accounting_Fileter();


        public AP_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Accounts Payable app now.";
            }
        }


        public List<AP_Item> FetchAP_Archive(string pm_invoice_no = "", int? pm_customer_id = null, string pm_gui_no = "")
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<AP_Item> theAP = new List<AP_Item>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            { 
                new SqlParameter() {ParameterName = "@pm_invoice_no", SqlDbType = SqlDbType.VarChar, Value = pm_invoice_no},
                new SqlParameter() {ParameterName = "@pm_customer_id", SqlDbType = SqlDbType.Decimal, Value = pm_customer_id},
                new SqlParameter() {ParameterName = "@pm_gui_no", SqlDbType = SqlDbType.VarChar, Value = pm_gui_no}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_GetList_AR_Archive", wkParm);
            if (ResultModel.r == 1)
                theAP = JsonConvert.DeserializeObject<List<AP_Item>>(ResultModel.JsonData);

            return theAP;
        }


        public List<AP_Detail> FetchAP_Details(int pm_receipt_number = 0)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<AP_Detail> theAP = new List<AP_Detail>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_receipt_number", SqlDbType = SqlDbType.Int, Value = pm_receipt_number}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_GetList_AP_Details", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theAP = JsonConvert.DeserializeObject<List<AP_Detail>>(ResultModel.JsonData);

            return theAP;
        }


        public List<AP_Item> GetAccountsPayableList(int pm_location_id)
        {
            if (GlobalVariables.MySession.List_AccountsPayable == null)
                FetchAccountsPayableList(pm_location_id);

            if (GlobalVariables.MySession.List_AccountsPayable == null)
                return null;

            List<AP_Item> theList = GlobalVariables.MySession.List_AccountsPayable
                .Where(w => w.location_id == pm_location_id)
                .OrderBy(i => i.invoice_no)//.ThenBy(i => i.requested_date).ThenBy(i => i.location_id).ThenBy(i => i.order_no).ThenBy(i => i.line_no).ThenBy(i => i.release_no)
                .ToList();

            if (GlobalVariables.MySession.List_AccountsPayable_Filters != null)
            {
                List<Filters> FilterList = GlobalVariables.MySession.List_AccountsPayable_Filters;

                foreach (var item in FilterList)
                {
                    if (String.IsNullOrEmpty(item.op))
                        continue;
                    /*
                    Type Type_AP = typeof(AP_Item);
                    PropertyInfo piInstance = Type_AP.GetProperty(item.key);

                    if (piInstance.PropertyType == typeof(int))
                        theList = filterAccounting.AP_parseFilters_Int(theList, item);
                    else
                    if (piInstance.PropertyType == typeof(DateTime))
                        theList = filterAccounting.AP_parseFilters_DateTime(theList, item);
                    else
                    if (piInstance.PropertyType == typeof(decimal))
                        theList = filterAccounting.AP_parseFilters_Decimal(theList, item);
                    else
                        theList = filterAccounting.AP_parseFilters_String(theList, item);
                    */
                    switch (item.value_type)
                    {
                        case "int":
                            theList = shareFilters.parseFilters_Int<AP_Item>(theList, item);
                            break;
                        case "number":
                            theList = shareFilters.parseFilters_Decimal<AP_Item>(theList, item);
                            break;
                        case "date":
                            theList = shareFilters.parseFilters_DateTime<AP_Item>(theList, item);
                            break;
                        default:
                            theList = shareFilters.parseFilters_String<AP_Item>(theList, item);
                            break;
                    }
                }
            }

            return theList;
        }


        public void FetchAccountsPayableList(int pm_location_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<AP_Item> theAP = new List<AP_Item>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_location_id", SqlDbType = SqlDbType.Decimal, Value = pm_location_id}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_GetList_AccountsPayable", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theAP = JsonConvert.DeserializeObject<List<AP_Item>>(ResultModel.JsonData);
                theAP.Select(s => { s.override_paid_date = s.default_paid_date; return s; }).ToList();

                if (GlobalVariables.MySession.List_AccountsPayable == null)
                    GlobalVariables.MySession.List_AccountsPayable = theAP;
                else
                {
                    GlobalVariables.MySession.List_AccountsPayable.RemoveAll(w => w.location_id == pm_location_id);
                    GlobalVariables.MySession.List_AccountsPayable.AddRange(theAP);
                }
            }

            //return theAP;
        }


        public SP_Return UpdateAccountsPayable(List<AP_Item> pm_AP)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string pmJSON = "";

            //double check
            ResultModel = shareCommon.checkAuthorized(121);
            if (ResultModel.r == 0)
                return ResultModel;

            var find = from data in pm_AP
                       where String.IsNullOrEmpty(data.gui) == false
                        || String.IsNullOrEmpty(data.z_memo) == false
                        || data.override_paid_date != data.default_paid_date
                       select data;
            List<AP_Item> pmUpdate = find.ToList();
            //pmUpdate.AddRange(pm_AR.Where(w => w.chk_archive).Select(s => { s.z_status = "A"; return s; }).ToList());
            //List<AR_Item> pmUpdate = find.Where(w => w.chk_archive).Select(s => { s.z_status = "A"; return s; }).ToList();
            foreach (var item in pmUpdate)
            {
                item.z_status = item.chk_archive ? "A" : "";
                item.default_paid_date = item.override_paid_date;
            }
            int location_id = pmUpdate.First().location_id;

            if (pmUpdate.Count() == 0)
            {
                ResultModel.r = 0;
                ResultModel.msg = "No data updated.";
                return ResultModel;
            }

            pmJSON = "{'pm_usr':'" + GlobalVariables.MySession.Account + "', 'theGUI':" + JsonConvert.SerializeObject(pmUpdate) + "}";
            pmJSON = pmJSON.Replace("'", "\"");
            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_AP_Management", wkParm);
            if (ResultModel.r == 1)
            {
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);
                if (ResultModel.r == 1)
                    FetchAccountsPayableList(location_id);
            }

            return ResultModel;
        }

    }
}
