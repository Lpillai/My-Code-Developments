using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RuntimeVariables;
using RuntimeConfig;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P21
{
    public class Integration_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();
        Convertor_Share shareConvertor = new Convertor_Share();
        FileActions_Share shareFileActions = new FileActions_Share();


        public Integration_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for P21 Integration app now.";
            }
        }
        
        #region Item Maintenance

        public item_net_weight Fetch_item_net_weight(string pm_item_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            item_net_weight theItem = new item_net_weight();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_item_id", SqlDbType = SqlDbType.VarChar, Value = pm_item_id}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prItem_NetWeight_Get", wkParm);
            if (ResultModel.r == 1)
                theItem = JsonConvert.DeserializeObject<item_net_weight>(ResultModel.JsonData);

            return theItem;
        }


        public async Task<SP_Return> SaveItemNetWeightAsync(item_net_weight pmItem)
        {
            SP_Return ResultModel = new SP_Return() { r = -1, msg = "", JsonData = "" };
            string pm_parm = "";

            //double check
            ResultModel = shareCommon.checkAuthorized(75);
            if (ResultModel.r == 0)
                return ResultModel;

            if (pmItem.weight == null)
                pmItem.weight = pmItem.kilograms * 2.20462;
            if (pmItem.kilograms == null)
                pmItem.kilograms = pmItem.weight * 0.453592;
            pmItem.P21URL = (!isDev ? 1 : 2);
            pm_parm = JsonConvert.SerializeObject(pmItem);

            ResultModel = await shareCommon.callSBS_API_Post("Warehouse/Update_Item_NetWeight_Async", pm_parm);

            return ResultModel;
        }


        public SP_Return GetExampleSheet_PGID()
        {
            string fullPath = shareFileActions.GetUploadFolder() + "example_pgid.xlsx";
            SP_Return ExportModel = new SP_Return() { r = 1, msg = "", JsonData = fullPath };
            System.IO.FileInfo wkFile = new System.IO.FileInfo(fullPath);
            string version = "20231117";

            try
            {
                if (wkFile.Exists)
                    wkFile.Delete();

                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(wkFile))
                {
                    var workSheet = package.Workbook.Worksheets.Add(version);
                    workSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    //header
                    workSheet.Cells[1, 1].Value = "item_id";
                    workSheet.Cells[1, 2].Value = "location_id";
                    workSheet.Cells[1, 3].Value = "product_group_id";

                    //data row 1
                    workSheet.Cells[2, 1].Value = "example item 1";
                    workSheet.Cells[2, 2].Value = "1";
                    workSheet.Cells[2, 3].Value = "10783";

                    //data row 2
                    workSheet.Cells[3, 1].Value = "example item 2";
                    workSheet.Cells[3, 2].Value = "13";
                    workSheet.Cells[3, 3].Value = "10913";

                    workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                    workSheet.View.FreezePanes(2, 1);
                    package.Save();
                }
            }
            catch (Exception ex)
            {
                ExportModel.r = 0;
                ExportModel.msg = ex.ToString();
            }

            return ExportModel;
        }


        public SP_Return UpdateProductGroupID(string pm_Path)
        {
            SqlConnection SBScon = new SqlConnection(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            int theURL = !isDev ? 1 : 2;

            //double check
            ResultModel = shareCommon.checkAuthorized(84);
            if (ResultModel.r == 0)
                return ResultModel;

            string queue_id = null;
            string wkPath = null;
            var bulkCopy = new SqlBulkCopy(SBScon);
            ExcelWorksheet wkSheet;

            //To avoid showing message: "Please set the ExcelPackage.LicenseContext property"
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var excelPackage = new OfficeOpenXml.ExcelPackage())
            {
                try
                {
                    SBScon.Open();
                    using (var stream = File.OpenRead(pm_Path))
                    {
                        excelPackage.Load(stream);
                    }
                    wkSheet = excelPackage.Workbook.Worksheets[0];

                    var dt = shareConvertor.ToDataTable(wkSheet);
                    bulkCopy.DestinationTableName = "P21Integration.inv_loc_pgid";
                    bulkCopy.ColumnMappings.Add("item_id", "item_id");
                    bulkCopy.ColumnMappings.Add("location_id", "location_id");
                    bulkCopy.ColumnMappings.Add("product_group_id", "pgid");
                    bulkCopy.WriteToServer(dt);

                    ResultModel = shareCommon.ExecSP(SBScon, "P21Integration.prIntranet_inv_loc_pgid_SetQueueID");
                    if (ResultModel.r == 1)
                    {
                        queue_id = ResultModel.JsonData;
                        if (queue_id != "-1")
                        {
                            wkPath = "@pm_query='P21Integration/Update_Item_PGID?P21URL=" + theURL.ToString() + "&queue_id=" + queue_id + "&mail_to=" + GlobalVariables.MySession.Email + "', @out_result=''";
                            ResultModel = shareCommon.AddScheduleForAPI("SBS.z.prSBS_API_GET2", wkPath, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //throw ex;
                    ResultModel.r = 0;
                    ResultModel.msg = ex.ToString();
                }
                finally
                {
                    if (SBScon.State != ConnectionState.Closed)
                        SBScon.Close();
                    bulkCopy.Close();
                    bulkCopy = null;
                    File.Delete(pm_Path);
                }
            }

            return ResultModel;
        }


        public SP_Return GetExampleSheet_ABCClass()
        {
            string fullPath = shareFileActions.GetUploadFolder() + "example_abc_class.xlsx";
            SP_Return ExportModel = new SP_Return() { r = 1, msg = "", JsonData = fullPath };
            System.IO.FileInfo wkFile = new System.IO.FileInfo(fullPath);
            string version = "20241115";

            try
            {
                if (wkFile.Exists)
                    wkFile.Delete();

                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(wkFile))
                {
                    var workSheet = package.Workbook.Worksheets.Add(version);
                    workSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    //header
                    workSheet.Cells[1, 1].Value = "item_id";
                    workSheet.Cells[1, 2].Value = "location_id";
                    workSheet.Cells[1, 3].Value = "purchase_class";

                    //data row 1
                    workSheet.Cells[2, 1].Value = "example item 1";
                    workSheet.Cells[2, 2].Value = "1";
                    workSheet.Cells[2, 3].Value = "AA";

                    //data row 2
                    workSheet.Cells[3, 1].Value = "example item 2";
                    workSheet.Cells[3, 2].Value = "13";
                    workSheet.Cells[3, 3].Value = "B";

                    workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                    workSheet.View.FreezePanes(2, 1);
                    package.Save();
                }
            }
            catch (Exception ex)
            {
                ExportModel.r = 0;
                ExportModel.msg = ex.ToString();
            }

            return ExportModel;
        }


        public SP_Return UpdateABCClass(string pm_Path)
        {
            SqlConnection SBScon = new SqlConnection(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            int theURL = !isDev ? 1 : 2;

            //double check
            ResultModel = shareCommon.checkAuthorized(115);
            if (ResultModel.r == 0)
                return ResultModel;

            string queue_id = null;
            string wkPath = null;
            var bulkCopy = new SqlBulkCopy(SBScon);
            ExcelWorksheet wkSheet;

            //To avoid showing message: "Please set the ExcelPackage.LicenseContext property"
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var excelPackage = new OfficeOpenXml.ExcelPackage())
            {
                try
                {
                    SBScon.Open();
                    using (var stream = File.OpenRead(pm_Path))
                    {
                        excelPackage.Load(stream);
                    }
                    wkSheet = excelPackage.Workbook.Worksheets[0];

                    var dt = shareConvertor.ToDataTable(wkSheet);
                    bulkCopy.DestinationTableName = "P21Integration.inv_loc_abcclass";
                    bulkCopy.ColumnMappings.Add("item_id", "item_id");
                    bulkCopy.ColumnMappings.Add("location_id", "location_id");
                    bulkCopy.ColumnMappings.Add("purchase_class", "purchase_class");
                    bulkCopy.WriteToServer(dt);

                    ResultModel = shareCommon.ExecSP(SBScon, "P21Integration.prIntranet_inv_loc_abcclass_SetQueueID");
                    if (ResultModel.r == 1)
                    {
                        queue_id = ResultModel.JsonData;
                        if (queue_id != "-1")
                        {
                            wkPath = "@pm_query='P21Integration/Update_Item_ABCClass?P21URL=" + theURL.ToString() + "&queue_id=" + queue_id + "&mail_to=" + GlobalVariables.MySession.Email + "', @out_result=''";
                            ResultModel = shareCommon.AddScheduleForAPI("SBS.z.prSBS_API_GET2", wkPath, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //throw ex;
                    ResultModel.r = 0;
                    ResultModel.msg = ex.ToString();
                }
                finally
                {
                    if (SBScon.State != ConnectionState.Closed)
                        SBScon.Close();
                    bulkCopy.Close();
                    bulkCopy = null;
                    File.Delete(pm_Path);
                }
            }

            return ResultModel;
        }


        public SP_Return GetExampleSheet_LeadTime()
        {
            string fullPath = shareFileActions.GetUploadFolder() + "example_lead_time.xlsx";
            SP_Return ExportModel = new SP_Return() { r = 1, msg = "", JsonData = fullPath };
            System.IO.FileInfo wkFile = new System.IO.FileInfo(fullPath);
            string version = "20241122";

            try
            {
                if (wkFile.Exists)
                    wkFile.Delete();

                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(wkFile))
                {
                    var workSheet = package.Workbook.Worksheets.Add(version);
                    workSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    //header
                    workSheet.Cells[1, 1].Value = "item_id";
                    workSheet.Cells[1, 2].Value = "supplier_id";
                    workSheet.Cells[1, 3].Value = "lead_time";

                    //data row 1
                    workSheet.Cells[2, 1].Value = "example item 1";
                    workSheet.Cells[2, 2].Value = "12345";
                    workSheet.Cells[2, 3].Value = "50";

                    //data row 2
                    workSheet.Cells[3, 1].Value = "example item 2";
                    workSheet.Cells[3, 2].Value = "54321";
                    workSheet.Cells[3, 3].Value = "100";

                    workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                    workSheet.View.FreezePanes(2, 1);
                    package.Save();
                }
            }
            catch (Exception ex)
            {
                ExportModel.r = 0;
                ExportModel.msg = ex.ToString();
            }

            return ExportModel;
        }


        public SP_Return UpdateLeadTime(string pm_Path)
        {
            SqlConnection SBScon = new SqlConnection(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            int theURL = !isDev ? 1 : 2;

            //double check
            ResultModel = shareCommon.checkAuthorized(116);
            if (ResultModel.r == 0)
                return ResultModel;

            string queue_id = null;
            string wkPath = null;
            var bulkCopy = new SqlBulkCopy(SBScon);
            ExcelWorksheet wkSheet;

            //To avoid showing message: "Please set the ExcelPackage.LicenseContext property"
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var excelPackage = new OfficeOpenXml.ExcelPackage())
            {
                try
                {
                    SBScon.Open();
                    using (var stream = File.OpenRead(pm_Path))
                    {
                        excelPackage.Load(stream);
                    }
                    wkSheet = excelPackage.Workbook.Worksheets[0];

                    var dt = shareConvertor.ToDataTable(wkSheet);
                    bulkCopy.DestinationTableName = "P21Integration.inventory_supplier_leadtime";
                    bulkCopy.ColumnMappings.Add("item_id", "item_id");
                    bulkCopy.ColumnMappings.Add("supplier_id", "supplier_id");
                    bulkCopy.ColumnMappings.Add("lead_time", "supplier_sort_code");
                    bulkCopy.WriteToServer(dt);

                    ResultModel = shareCommon.ExecSP(SBScon, "P21Integration.prIntranet_inventory_supplier_leadtime_SetQueueID");
                    if (ResultModel.r == 1)
                    {
                        queue_id = ResultModel.JsonData;
                        if (queue_id != "-1")
                        {
                            wkPath = "@pm_query='P21Integration/Update_Item_LeadTime?P21URL=" + theURL.ToString() + "&queue_id=" + queue_id + "&mail_to=" + GlobalVariables.MySession.Email + "', @out_result=''";
                            ResultModel = shareCommon.AddScheduleForAPI("SBS.z.prSBS_API_GET2", wkPath, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //throw ex;
                    ResultModel.r = 0;
                    ResultModel.msg = ex.ToString();
                }
                finally
                {
                    if (SBScon.State != ConnectionState.Closed)
                        SBScon.Close();
                    bulkCopy.Close();
                    bulkCopy = null;
                    File.Delete(pm_Path);
                }
            }

            return ResultModel;
        }

        #endregion
        
    }
}
