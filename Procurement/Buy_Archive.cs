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

namespace Procurement
{
    public class BuyArchive_Program
    {
        //private SqlConnection OScon = GlobalConfig.MyConfig.Connection_OS_BuySheet;
        Common_Share shareCommon = new Common_Share();
        BuyNote_Program pgmBuyNote = new BuyNote_Program();
        BuySplits_Program pgmBuySplits = new BuySplits_Program();
        Buy_Fileter filterBuy = new Buy_Fileter();


        public List<BuyQuote> FetchQuotesOfOneBuy(int pm_viewID)
        {
            List<BuyQuote> quoteModel = new List<BuyQuote>();
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_viewID", SqlDbType = SqlDbType.Int, Value = pm_viewID}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_GetQuotesOfOneBuy", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                quoteModel = JsonConvert.DeserializeObject<List<BuyQuote>>(ResultModel.JsonData);

            return quoteModel;
        }


        public List<Buy> FetchArchiveBuys(List<Filters> pm_FilterList, string cluster)
        {
            shareCommon.CleanCache();
            List<Buy> buyList = new List<Buy>();
            int filterCount = 0;
            string pm_filters = "";
            bool writeSession = false;

            if (pm_FilterList == null)
            {
                pm_FilterList = filterBuy.getFilters(cluster, "BuyArchive");
                writeSession = true;
            }

            if (pm_FilterList == null)
                return buyList;

            foreach (var item in pm_FilterList)
            {
                if (String.IsNullOrEmpty(item.op))
                    continue;

                if (pm_filters != "")
                    pm_filters += " AND ";

                switch (item.value_type)
                {
                    case "text":
                        switch (item.op)
                        {
                            case "%":
                                pm_filters += item.key + " LIKE " + "'%" + item.value_s + "%'";
                                break;
                            case "!%":
                                pm_filters += item.key + " NOT LIKE " + "'%" + item.value_s + "%'";
                                break;
                            default:
                                pm_filters += item.key + item.op + "'" + item.value_s + "'";
                                break;
                        }
                        break;
                    case "date":
                        if (item.op != "~")
                            pm_filters += "CAST(" + item.key + " AS DATE)" + item.op + "'" + item.value_s + "'";
                        else
                            pm_filters += "CAST(" + item.key + " AS DATE) BETWEEN '" + item.value_s + "' AND '" + item.value_e + "'";
                        break;
                    default:
                        if (item.op != "~")
                            pm_filters += item.key + item.op + item.value_s;
                        else
                            pm_filters += item.key + " BETWEEN " + item.value_s + " AND " + item.value_e;
                        break;
                }

                filterCount++;
            }

            if (filterCount == 0)
                return buyList;


            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmFilters", SqlDbType = SqlDbType.VarChar, Value = pm_filters},
                new SqlParameter() {ParameterName = "@cluster", SqlDbType = SqlDbType.VarChar, Value = cluster}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_GetArchiveBuys1", wkParm);
            if (ResultModel.r == 1)
                buyList = JsonConvert.DeserializeObject<List<Buy>>(ResultModel.JsonData);

            var find = from data in buyList
                       select new BuySheet_BuyID() { viewID = data.viewID.ToString() + ", " };
            List<BuySheet_BuyID> pmFetch = find.ToList();
            string theseBuys = string.Join("", pmFetch.Select(a => a.viewID).ToArray()).Replace(",", " ");

            List<BuyFTB> FTB_List = FetchArchiveFTB(theseBuys);

            List<BuyNote> NotesList = pgmBuyNote.FetchAllNotes(theseBuys);
            List<BuySplit> SplitsList = pgmBuySplits.FetchAllSplits(theseBuys);
            if (writeSession)
            {
                switch (cluster)
                {
                    case "OS":
                        GlobalVariables.MySession.List_ArchiveBuy_OSNotes = NotesList;
                        GlobalVariables.MySession.List_ArchiveBuy_OSSplits = SplitsList;
                        break;
                    case "Domestic":
                        GlobalVariables.MySession.List_ArchiveBuy_DomesticNotes = NotesList;
                        GlobalVariables.MySession.List_ArchiveBuy_DomesticSplits = SplitsList;
                        break;
                    case "FTB":
                        GlobalVariables.MySession.List_ArchiveBuy_FTBNotes = NotesList;
                        GlobalVariables.MySession.List_ArchiveBuy_FTBSplits = SplitsList;
                        break;
                    default:
                        break;
                }
            }

            foreach (var item in buyList)
            {
                if (FTB_List != null)
                {
                    item.theFTB = FTB_List.Find(f => f.viewID == item.viewID);
                }

                if (NotesList != null)
                    item.noteCount = NotesList.FindAll(x => (x.viewID == item.viewID)).Count().ToString();
                else
                    item.noteCount = "0";

                if (SplitsList != null)
                    item.Splits = SplitsList.FindAll(x => x.viewID.Equals(item.viewID)).Count().ToString();
                else
                    item.Splits = "0";
            }

            return buyList;
        }


        public List<BuyFTB> FetchArchiveFTB(string pm_viewID)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<BuyFTB> FTB_List = null;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_viewID", SqlDbType = SqlDbType.VarChar, Value = pm_viewID}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_GetFTB_List", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                FTB_List = JsonConvert.DeserializeObject<List<BuyFTB>>(ResultModel.JsonData);

            return FTB_List;
        }


        public List<Buy_ArchiveItem> FetchArchiveItem(string pm_item_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Buy_ArchiveItem> archiveList = new List<Buy_ArchiveItem>();
            string pmJSON = "";
            pmJSON = "{'pm_item_id':'" + pm_item_id + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_GetArchiveBuys_Rates", wkParm);
            if (ResultModel.r == 1)
                archiveList = JsonConvert.DeserializeObject<List<Buy_ArchiveItem>>(ResultModel.JsonData);

            if (archiveList == null)
                archiveList = new List<Buy_ArchiveItem>();
            else
            {
                var find = from data in archiveList
                           select new BuySheet_BuyID() { viewID = data.viewID.ToString() + "," };
                List<BuySheet_BuyID> pmFetch = find.ToList();
                string theseBuys = string.Join("", pmFetch.Select(a => a.viewID).ToArray()).Replace(",", " ");
                List<BuyNote> NotesList = pgmBuyNote.FetchAllNotes(theseBuys);

                List<BuySplit> SplitsList = pgmBuySplits.FetchAllSplits(theseBuys);
                foreach (var item in archiveList)
                {
                    if (NotesList != null)
                        item.noteCount = NotesList.FindAll(x => (x.viewID == item.viewID)).Count().ToString();
                    else
                        item.noteCount = "0";

                    if (SplitsList != null)
                        item.Splits = SplitsList.FindAll(x => x.viewID.Equals(item.viewID)).Count().ToString();
                    else
                        item.Splits = "0";
                }
            }

            return archiveList;
        }


        public MemoryStream GetArchiveItemSheet(string pm_archive)
        {
            List<Buy_ArchiveItem> archiveList = JsonConvert.DeserializeObject<List<Buy_ArchiveItem>>(pm_archive);
            string itemName = archiveList[0].ItemID;
            var stream = new MemoryStream();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add(itemName);
                workSheet.Cells["A1"].LoadFromCollection(archiveList, true);
                workSheet.DeleteColumn(2); //Entry ID
                workSheet.DeleteColumn(workSheet.Dimension.End.Column); //splits count
                workSheet.DeleteColumn(workSheet.Dimension.End.Column); //notes count

                workSheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(9).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(10).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(12).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(13).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(14).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(15).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(16).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(17).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(18).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(19).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(20).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                workSheet.Column(8).Style.Numberformat.Format = "#,##0.00";
                workSheet.Column(9).Style.Numberformat.Format = "#,##0.00";
                workSheet.Column(12).Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Column(13).Style.Numberformat.Format = "#,##0.00";
                workSheet.Column(15).Style.Numberformat.Format = "#,##0";
                workSheet.Column(16).Style.Numberformat.Format = "#,##0.00000";
                workSheet.Column(17).Style.Numberformat.Format = "#,##0.0";
                workSheet.Column(18).Style.Numberformat.Format = "#,##0.00";
                workSheet.Column(19).Style.Numberformat.Format = "#,##0";
                workSheet.Column(20).Style.Numberformat.Format = "#,##0";

                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                workSheet.View.FreezePanes(2, 4);
                package.Save();
            }
            stream.Position = 0;

            return stream;
        }

    }
}
