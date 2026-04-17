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
    public class AR_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();
        Filters_Share shareFilters = new Filters_Share();
        //FileActions_Share shareFileActions = new FileActions_Share();
        //Accounting_Fileter filterAccounting = new Accounting_Fileter();


        public AR_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Accounts Receivable app now.";
            }
        }

        #region Accounts Receivable 

        public List<AR_Item> FetchAR_Archive(string pm_invoice_no = "", int? pm_customer_id = null, string pm_gui_no = "")
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<AR_Item> theAR = new List<AR_Item>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            { 
                new SqlParameter() {ParameterName = "@pm_invoice_no", SqlDbType = SqlDbType.VarChar, Value = pm_invoice_no},
                new SqlParameter() {ParameterName = "@pm_customer_id", SqlDbType = SqlDbType.Decimal, Value = pm_customer_id},
                new SqlParameter() {ParameterName = "@pm_gui_no", SqlDbType = SqlDbType.VarChar, Value = pm_gui_no}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_GetList_AR_Archive", wkParm);
            if (ResultModel.r == 1)
                theAR = JsonConvert.DeserializeObject<List<AR_Item>>(ResultModel.JsonData);

            return theAR;
        }


        public List<AR_Detail> FetchAR_Details(string pm_invoice_no = "")
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<AR_Detail> theAR = new List<AR_Detail>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_invoice_no", SqlDbType = SqlDbType.VarChar, Value = pm_invoice_no}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_GetList_AR_Details", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theAR = JsonConvert.DeserializeObject<List<AR_Detail>>(ResultModel.JsonData);

            return theAR;
        }


        //public List<AR_Item> GetAccountsReceivableList(int pm_location_id, string pm_taker_group)
        public List<AR_Item> GetAccountsReceivableList(int pm_location_id)
        {
            if (GlobalVariables.MySession.List_AccountsReceivable == null)
                //FetchAccountsReceivableList(pm_location_id, pm_taker_group);
                FetchAccountsReceivableList(pm_location_id);

            if (GlobalVariables.MySession.List_AccountsReceivable == null)
                return null;

            List<AR_Item> theList = GlobalVariables.MySession.List_AccountsReceivable
                //.Where(w => w.location_id == pm_location_id && w.taker_group == pm_taker_group)
                .Where(w => w.location_id == pm_location_id)
                .OrderBy(i => i.invoice_no)//.ThenBy(i => i.requested_date).ThenBy(i => i.location_id).ThenBy(i => i.order_no).ThenBy(i => i.line_no).ThenBy(i => i.release_no)
                .ToList();

            if (GlobalVariables.MySession.List_AccountsReceivable_Filters != null)
            {
                List<Filters> FilterList = GlobalVariables.MySession.List_AccountsReceivable_Filters;

                foreach (var item in FilterList)
                {
                    if (String.IsNullOrEmpty(item.op))
                        continue;
                    /*
                    Type Type_AR = typeof(AR_Item);
                    PropertyInfo piInstance = Type_AR.GetProperty(item.key);

                    if (piInstance.PropertyType == typeof(int))
                        theList = filterAccounting.AR_parseFilters_Int(theList, item);
                    else
                    if (piInstance.PropertyType == typeof(DateTime))
                        theList = filterAccounting.AR_parseFilters_DateTime(theList, item);
                    else
                    if (piInstance.PropertyType == typeof(decimal))
                        theList = filterAccounting.AR_parseFilters_Decimal(theList, item);
                    else
                        theList = filterAccounting.AR_parseFilters_String(theList, item);
                    */
                    switch (item.value_type)
                    {
                        case "int":
                            theList = shareFilters.parseFilters_Int<AR_Item>(theList, item);
                            break;
                        case "number":
                            theList = shareFilters.parseFilters_Decimal<AR_Item>(theList, item);
                            break;
                        case "date":
                            theList = shareFilters.parseFilters_DateTime<AR_Item>(theList, item);
                            break;
                        default:
                            theList = shareFilters.parseFilters_String<AR_Item>(theList, item);
                            break;
                    }
                }
            }

            return theList;
        }


        //public void FetchAccountsReceivableList(int pm_location_id, string pm_taker_group)
        public void FetchAccountsReceivableList(int pm_location_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<AR_Item> theAR = new List<AR_Item>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_location_id", SqlDbType = SqlDbType.Decimal, Value = pm_location_id}
                //,new SqlParameter() {ParameterName = "@pm_taker_group", SqlDbType = SqlDbType.VarChar, Value = pm_taker_group}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_GetList_AccountsReceivable", wkParm);
            if (ResultModel.r == 1)
            {
                theAR = JsonConvert.DeserializeObject<List<AR_Item>>(ResultModel.JsonData);
                //theAR.Select(s => { s.taker_group = pm_taker_group; return s; }).ToList();
                theAR.Select(s => { s.override_paid_date = s.default_paid_date; return s; }).ToList();

                if (GlobalVariables.MySession.List_AccountsReceivable == null)
                    GlobalVariables.MySession.List_AccountsReceivable = theAR;
                else
                {
                    //GlobalVariables.MySession.List_AccountsReceivable.RemoveAll(w => w.location_id == pm_location_id && w.taker_group == pm_taker_group);
                    GlobalVariables.MySession.List_AccountsReceivable.RemoveAll(w => w.location_id == pm_location_id);
                    GlobalVariables.MySession.List_AccountsReceivable.AddRange(theAR);
                }
            }

            //return theAR;
        }


        public SP_Return ImportData_ASMLTW(string pm_Path)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(62);
            if (ResultModel.r == 0)
                return ResultModel;

            List<AR_Item> AR_asmltw = new List<AR_Item>();
            string json_parm = "";
            ExcelWorksheet wkSheet;

            //To avoid showing message: "Please set the ExcelPackage.LicenseContext property"
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var excelPackage = new OfficeOpenXml.ExcelPackage())
            {
                try
                {
                    using (var stream = File.OpenRead(pm_Path))
                    {
                        excelPackage.Load(stream);
                    }
                    wkSheet = excelPackage.Workbook.Worksheets[0];

                    for (int rowNum = 5; rowNum <= wkSheet.Dimension.End.Row; rowNum++)
                    {
                        AR_asmltw.Add(new AR_Item()
                        {
                            po_no = wkSheet.Cells[rowNum, 3].Value.ToString().Substring(2),
                            gui = wkSheet.Cells[rowNum, 1].Value.ToString()
                        });
                    }

                    json_parm = JsonConvert.SerializeObject(AR_asmltw.Select(s => new { s.po_no, s.gui }));
                    json_parm = "{'pm_usr': '" + GlobalVariables.MySession.Account + "', 'asmltw': " + json_parm + "}";
                    json_parm = json_parm.Replace("'", "\"");

                    List<SqlParameter> wkParm = new List<SqlParameter>()
                    {
                        new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
                    };
                    ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_AR_Management_ASMLTW", wkParm);
                    if (ResultModel.r == 1)
                        ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);
                }
                catch (Exception ex)
                {
                    //throw ex;
                    ResultModel.r = 0;
                    ResultModel.msg = ex.ToString();
                }
                finally
                {
                    File.Delete(pm_Path);
                }
            }

            return ResultModel;
        }


        public SP_Return UpdateAccountsReceivable(List<AR_Item> pm_AR)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string pmJSON = "";

            //double check
            ResultModel = shareCommon.checkAuthorized(62);
            if (ResultModel.r == 0)
                return ResultModel;

            var find = from data in pm_AR
                       where !string.IsNullOrEmpty(data.gui)
                       || !string.IsNullOrEmpty(data.z_memo)
                       //|| data.chk_archive
                       || data.override_paid_date != data.default_paid_date
                       || data.z_status != data.progress.ToString() 
                       select data;
            List<AR_Item> pmUpdate = find.ToList();
            //pmUpdate.AddRange(pm_AR.Where(w => w.chk_archive).Select(s => { s.z_status = "A"; return s; }).ToList());
            //List<AR_Item> pmUpdate = find.Where(w => w.chk_archive).Select(s => { s.z_status = "A"; return s; }).ToList();
            foreach (var item in pmUpdate)
            {
                item.default_paid_date = item.override_paid_date;
                //item.z_status = item.chk_archive ? "A" : "";
                item.z_status = item.progress_name == "Archive" ? "A" : item.progress.ToString();
            }

            if (pmUpdate.Count() == 0)
            {
                ResultModel.r = 0;
                ResultModel.msg = "No data updated.";
                return ResultModel;
            }

            int location_id = pmUpdate.First().location_id;
            //string taker_group = pmUpdate.First().taker_group;
            pmJSON = "{'pm_usr':'" + GlobalVariables.MySession.Account + "', 'theGUI':" + JsonConvert.SerializeObject(pmUpdate) + "}";
            pmJSON = pmJSON.Replace("'", "\"");
            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_AR_Management", wkParm);
            if (ResultModel.r == 1)
            {
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);
                if (ResultModel.r == 1)
                    //FetchAccountsReceivableList(location_id, taker_group);
                    FetchAccountsReceivableList(location_id);
            }

            return ResultModel;
        }

        #endregion

        #region Attachment 

        public List<AccountingAttachment> Fetch_Accounting_Attachment(string pm_accounting_type, int pm_accounting_uid)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<AccountingAttachment> theAttach = new List<AccountingAttachment>();
            string JsonString = "{'accounting_type':'" + pm_accounting_type + "', 'accounting_uid':" + pm_accounting_uid.ToString() + "}";

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = JsonString.Replace("'", "\"")}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_GetList_AccountingAttachment", wkParm);
            GlobalVariables.MySession.List_AccountingAttachment = null;
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theAttach = JsonConvert.DeserializeObject<List<AccountingAttachment>>(ResultModel.JsonData);
                foreach (var item in theAttach)
                    //item.attach_tag = string.Join(System.Environment.NewLine, item.tags);
                    item.attach_tag = (item.attach_tag == null ? "" : item.attach_tag.Replace("\r\n", "<br>"));

                GlobalVariables.MySession.List_AccountingAttachment = theAttach;
            }

            //theAttach.Insert(0, new AccountingAttachment() { z_status = "A" });
            theAttach.Add(new AccountingAttachment() { z_status = "A" });
            return theAttach;
        }


        public string Update_Accounting_Attachment(List<AccountingAttachment> pm_files, string pm_accounting_type, int pm_accounting_uid)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string ResultString = "";
            string delimiter = ",";
            List<SqlParameter> wkParm = null;

            foreach (var theFile in pm_files)
            {
                if (theFile.z_status != "A" && theFile.z_status != "D")
                    continue;

                MemoryStream data = new MemoryStream();

                if (theFile.z_status == "D")
                {
                    wkParm = new List<SqlParameter>()
                    {
                        new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = JsonConvert.SerializeObject(theFile)},
                        new SqlParameter() {ParameterName = "@pm_attach_file", SqlDbType = SqlDbType.VarBinary, Value = DBNull.Value}
                    };
                }
                else
                if (theFile.z_status == "A")
                {
                    //using (Stream str = File.OpenRead(theFile.attach_path))
                    using (Stream str = theFile.attachedFile.InputStream)
                    {
                        str.CopyTo(data);
                    }
                    data.Seek(0, SeekOrigin.Begin);
                    byte[] wkBuf = new byte[data.Length];
                    data.Read(wkBuf, 0, wkBuf.Length);
                    //shareFileActions.deleteWhenUnlock(new System.IO.FileInfo(theFile.attach_path));

                    theFile.accounting_type = pm_accounting_type;
                    theFile.accounting_uid = pm_accounting_uid;
                    theFile.attach_name = Path.GetFileNameWithoutExtension(theFile.attachedFile.FileName);
                    theFile.attach_ext = Path.GetExtension(theFile.attachedFile.FileName);
                    theFile.tags.Sort();
                    theFile.attach_tag = string.Join(delimiter, theFile.tags);
                    //theFile.attach_path = null;
                    theFile.tags = null;

                    wkParm = new List<SqlParameter>()
                    {
                        new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = JsonConvert.SerializeObject(theFile)},
                        new SqlParameter() {ParameterName = "@pm_attach_file", SqlDbType = SqlDbType.VarBinary, Value = wkBuf}
                    };
                }

                ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_AccountingAttachmentManagement", wkParm);
                if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                {
                    ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);
                    if (theFile.z_status == "A")
                        ResultString += "\"" + Path.GetFileName(theFile.attachedFile.FileName) + "\" " + ResultModel.msg + System.Environment.NewLine;
                    else
                    if (theFile.z_status == "D")
                        ResultString += "\"" + Path.GetFileName(theFile.attach_name) + "\" " + ResultModel.msg + System.Environment.NewLine;
                    else
                        ResultString = "Warning";

                    if (pm_accounting_type == "AR")
                        GlobalVariables.MySession.List_AccountsReceivable.Find(f => f.invoice_no == pm_accounting_uid).att_count = Fetch_Accounting_Attachment(pm_accounting_type, pm_accounting_uid).Count() - 1;
                }
                else
                {
                    if (theFile.z_status == "A")
                        ResultString = "\"" + Path.GetFileName(theFile.attachedFile.FileName) + "\" uploading failed." + System.Environment.NewLine + ResultModel.msg;
                    else
                    if (theFile.z_status == "D")
                        ResultString = "\"" + Path.GetFileName(theFile.attach_name) + "\" deleting failed." + System.Environment.NewLine + ResultModel.msg;
                    else
                        ResultString = "Error";
                }

                data.Dispose();
                data = null;
                wkParm = null;
            }

            return ResultString;
        }

        #endregion
    }
}
