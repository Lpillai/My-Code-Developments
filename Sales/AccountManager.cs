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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sales
{
    public class AccountManager_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();
        Filters_Share shareFilters = new Filters_Share();
        //Sales_Fileter filterSales = new Sales_Fileter();


        public AccountManager_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Account Manager app now.";
            }
        }


        public void SetOpenSalesOrder(string pm_taker_group)
        {
            //GlobalVariables.MySession.List_SO_OpenOrder = null;
            GlobalVariables.MySession.List_SO_OpenOrder = FetchOpenSalesOrder(pm_taker_group);
        }


        public List<SO_open_order> GetTakerOrder(string pm_taker)
        {
            List<SO_open_order> theList = GlobalVariables.MySession.List_SO_OpenOrder
                .Where(w => w.taker == pm_taker || pm_taker == "All")
                .OrderBy(i => i.taker).ThenBy(i => i.requested_date).ThenBy(i => i.location_id).ThenBy(i => i.order_no).ThenBy(i => i.line_no).ThenBy(i => i.release_no)
                .ToList();

            if (GlobalVariables.MySession.List_SO_OpenOrder_Filters != null)
            {
                List<Filters> FilterList = GlobalVariables.MySession.List_SO_OpenOrder_Filters;

                foreach (var item in FilterList)
                {
                    if (String.IsNullOrEmpty(item.op))
                        continue;
                    /*
                    Type Type_SO = typeof(SO_open_order);
                    PropertyInfo piInstance = Type_SO.GetProperty(item.key);
                    if (piInstance.PropertyType == typeof(int))
                        theList = filterSales.parseFilters_Int(theList, item);
                    else
                    if (piInstance.PropertyType == typeof(DateTime))
                        theList = filterSales.parseFilters_DateTime(theList, item);
                    else
                    if (piInstance.PropertyType == typeof(decimal))
                        theList = filterSales.parseFilters_Decimal(theList, item);
                    else
                        theList = filterSales.parseFilters_String(theList, item);
                    */
                    switch (item.value_type)
                    {
                        case "int":
                            theList = shareFilters.parseFilters_Int<SO_open_order>(theList, item);
                            break;
                        case "number":
                            theList = shareFilters.parseFilters_Decimal<SO_open_order>(theList, item);
                            break;
                        case "date":
                            theList = shareFilters.parseFilters_DateTime<SO_open_order>(theList, item);
                            break;
                        default:
                            theList = shareFilters.parseFilters_String<SO_open_order>(theList, item);
                            break;
                    }
                }
            }

            return theList;
        }


        public List<SO_open_order> FetchOpenSalesOrder(string pm_taker_group)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<SO_open_order> theOrders = new List<SO_open_order>();
            int wkP21URL = (!isDev ? 1 : 2);
            string pmJSON = "{'pmP21URL':" + wkP21URL.ToString() + ",'pm_taker_group':'" + pm_taker_group + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prP21_GetOpenSalesOrders", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theOrders = JsonConvert.DeserializeObject<List<SO_open_order>>(ResultModel.JsonData);
                //(from o in theOrders select o).ToList().ForEach(x => x.grp = 0);
            }

            return theOrders;
        }


        public SO_item_inquiry FetchItemInquiry(int pmP21URL, string pm_item_id, int pm_loc_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            SO_item_inquiry theItem = new SO_item_inquiry();
            string pmJSON = "{'pmP21URL':" + pmP21URL.ToString() + ",'pm_item_id':'" + pm_item_id + "','pm_loc_id':" + pm_loc_id.ToString() + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prP21_GetItemInquiry", wkParm);
            if (ResultModel.r == 1)
            {
                theItem = JsonConvert.DeserializeObject<SO_item_inquiry>(ResultModel.JsonData);
            }

            return theItem;
        }


        public MemoryStream SheetToExcel_OpenSalesOrder(string pm_taker)
        {
            List<SO_open_order> theExcelData = GetTakerOrder(pm_taker);
            var stream = new MemoryStream();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(pm_taker);
                workSheet.Cells["A1"].LoadFromCollection(theExcelData, true);

                workSheet.DeleteColumn(13); //qty_available
                workSheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(7).Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(12).Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Column(12).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(13).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(14).Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Column(14).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(15).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(15).Style.Numberformat.Format = "#,##0";
                workSheet.Column(16).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(16).Style.Numberformat.Format = "#,##0";
                workSheet.Column(17).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(17).Style.Numberformat.Format = "#,##0";
                workSheet.Column(19).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(20).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(20).Style.Numberformat.Format = "#,##0";
                workSheet.DeleteColumn(18); //qty_invoiced
                workSheet.DeleteColumn(9); //inv_mast_uid
                workSheet.DeleteColumn(5); //ship2_id

                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                workSheet.View.FreezePanes(2, 1);
                //workSheet.Cells[workSheet.Dimension.Address].AutoFilter = true;
                package.Save();
            }
            stream.Position = 0;

            return stream;
        }

    }
}
