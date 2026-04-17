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
using System.Text;
using System.Threading.Tasks;

namespace Accounting
{
    public class CashLockbox_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();
        Convertor_Share shareConvertor = new Convertor_Share();


        public CashLockbox_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Cash Lockbox app now.";
            }
        }


        public SP_Return CashReceipts_ProcessWrapper(string assigned_date)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(37);
            if (ResultModel.r == 0)
                return ResultModel;

            string pm_sp = "P21Integration.prBRP_CashLockbox_ProcessWrapper";
            string pm_parm = "{ 'assigned_date': '" + assigned_date + "' }";
            pm_parm = pm_parm.Replace("'", "\"");

            ResultModel = shareCommon.AddScheduleForAPI(pm_sp, pm_parm, true);

            return ResultModel;
        }


        public SP_Return CashReceipts_ImportSheet(string thePath)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(37);
            if (ResultModel.r == 0)
                return ResultModel;

            DateTime startTime = DateTime.Now; //Current Date
            ResultModel.msg = "";
            ResultModel.JsonData += "<path>" + thePath + "</path>";

            SqlConnection SBScon = new SqlConnection(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);
            SqlCommand cmd = new SqlCommand("TRUNCATE TABLE P21Integration.cash_receipts_import_data", SBScon);
            var bulkCopy = new SqlBulkCopy(SBScon);
            ExcelWorksheet wkSheet;
            try
            {
                SBScon.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();

                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;   //To avoid showing message: "Please set the ExcelPackage.LicenseContext property"
                using (var excelPackage = new OfficeOpenXml.ExcelPackage())
                {
                    using (var stream = File.OpenRead(thePath))
                    {
                        excelPackage.Load(stream);
                    }
                    wkSheet = excelPackage.Workbook.Worksheets[0];

                    var dt = shareConvertor.ToDataTable(wkSheet);
                    var table = "P21Integration.cash_receipts_import_data";
                    bulkCopy.DestinationTableName = table;

                    bulkCopy.ColumnMappings.Add("Company Code", "company_code");
                    bulkCopy.ColumnMappings.Add("Reference", "reference");
                    bulkCopy.ColumnMappings.Add("Reference Key", "reference_key");
                    bulkCopy.ColumnMappings.Add("Document Date", "document_date");
                    bulkCopy.ColumnMappings.Add("Net due date", "net_due_date");
                    bulkCopy.ColumnMappings.Add("Document currency", "document_currency");
                    bulkCopy.ColumnMappings.Add("Amount in doc. curr.", "amount_in_doc_curr");
                    bulkCopy.ColumnMappings.Add("Discount amount", "discount_amount");
                    bulkCopy.WriteToServer(dt);

                    ResultModel.JsonData += "<Count>" + dt.Rows.Count + "</Count>";
                }
            }
            catch (Exception ex)
            {
                ResultModel.r = 0;
                ResultModel.msg = "<words>" + ex.ToString() + "</words>";
            }
            finally
            {
                if (SBScon.State != ConnectionState.Closed)
                    SBScon.Close();
                cmd.Dispose();
                bulkCopy.Close();
                bulkCopy = null;
                File.Delete(thePath);
            }

            TimeSpan span = startTime.Subtract(DateTime.Now);
            ResultModel.JsonData += "<ExecutionTime>" + span.ToString(@"mm\:ss") + "</ExecutionTime>";

            return ResultModel;
        }

    }
}
