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

namespace Procurement
{
    public class BuyNote_Program
    {
        //private SqlConnection OScon = GlobalConfig.MyConfig.Connection_OS_BuySheet;
        Common_Share shareCommon = new Common_Share();
        BuyOthers_Program pgmBuyOthers = new BuyOthers_Program();


        public List<BuyNote> FetchAllNotes(string pm_viewID)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<BuyNote> noteList = new List<BuyNote>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_viewID", SqlDbType = SqlDbType.VarChar, Value = pm_viewID}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_GetNotes1", wkParm);

            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                noteList = JsonConvert.DeserializeObject<List<BuyNote>>(ResultModel.JsonData);
            /*
            List<BuyNote> noteList = null;
            SqlCommand cmd = new SqlCommand("sbs_sp_OSBUY_GetNotes1", GlobalConfig.MyConfig.Connection_OS_BuySheet) { CommandType = CommandType.StoredProcedure, CommandTimeout = 0 };

            try
            {
                cmd.Parameters.AddWithValue("@pm_viewID", pm_viewID);
                var adap = new SqlDataAdapter(cmd);
                var dset = new DataSet();
                adap.Fill(dset);
                noteList = JsonConvert.DeserializeObject<List<BuyNote>>(dset.Tables[0].Rows[0][0].ToString());
                if (noteList == null)
                    noteList = new List<BuyNote>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (GlobalConfig.MyConfig.Connection_OS_BuySheet.State != ConnectionState.Closed) { GlobalConfig.MyConfig.Connection_OS_BuySheet.Close(); }
                cmd.Dispose();
            }
            */
            return noteList;
        }


        public List<BuyNote> GetNotesForOneBuy(int pm_viewID, string pm_EntryID)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<BuyNote> noteList = new List<BuyNote>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@entryId", SqlDbType = SqlDbType.VarChar, Value = pm_EntryID}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_GetNotes2", wkParm);

            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                noteList = JsonConvert.DeserializeObject<List<BuyNote>>(ResultModel.JsonData);

                foreach (var item in noteList)
                    item.viewID = pm_viewID;
            }
            /*
            SqlCommand cmd = new SqlCommand("sbs_sp_OSBUY_GetNotes2", GlobalConfig.MyConfig.Connection_OS_BuySheet) { CommandType = CommandType.StoredProcedure, CommandTimeout = 0 };
            try
            {
                cmd.Parameters.AddWithValue("@entryId", pm_EntryID);
                if (GlobalConfig.MyConfig.Connection_OS_BuySheet != null && GlobalConfig.MyConfig.Connection_OS_BuySheet.State == ConnectionState.Closed)
                    GlobalConfig.MyConfig.Connection_OS_BuySheet.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        BuyNote theNotes = new BuyNote();
                        theNotes.viewID = pm_viewID;
                        theNotes.entryID = pm_EntryID;
                        theNotes.notesID = (int)reader["notesID"];
                        theNotes.notes = reader["notes"].ToString();
                        theNotes.user = reader["user"].ToString().Replace("SPECIALTYBOLT\\", "");
                        theNotes.createDate = (DateTime)reader["createDate"];
                        theNotes.read = reader["read"].ToString();
                        NotesList.Add(theNotes);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (GlobalConfig.MyConfig.Connection_OS_BuySheet.State != ConnectionState.Closed) { GlobalConfig.MyConfig.Connection_OS_BuySheet.Close(); }
                cmd.Dispose();
            }
            */
            return noteList;
        }


        public SP_Return UpdateOneNote(string cluster, BuyNote noteModel)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            switch (cluster)
            {
                case "OS":
                    ResultModel = shareCommon.checkAuthorized(19);
                    break;
                case "Domestic":
                    ResultModel = shareCommon.checkAuthorized(31);
                    break;
                case "FTB":
                    ResultModel = shareCommon.checkAuthorized(107);
                    break;
                default:
                    ResultModel.r = 0;
                    ResultModel.msg = "Unavailable Cluster.";
                    break;
            }
            if (ResultModel.r == 0)
                return ResultModel;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@entryId", SqlDbType = SqlDbType.VarChar, Value = noteModel.entryID},
                new SqlParameter() {ParameterName = "@notesId", SqlDbType = SqlDbType.Int, Value = noteModel.notesID},
                new SqlParameter() {ParameterName = "@notes", SqlDbType = SqlDbType.VarChar, Value = noteModel.notes}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_UpdateNotes", wkParm, true);
            /*
            SqlCommand cmd = new SqlCommand("sbs_sp_OSBUY_UpdateNotes", GlobalConfig.MyConfig.Connection_OS_BuySheet) { CommandType = CommandType.StoredProcedure, CommandTimeout = 0 };
            try
            {
                cmd.Parameters.AddWithValue("@entryId", noteModel.entryID);
                cmd.Parameters.AddWithValue("@notesId", noteModel.notesID);
                cmd.Parameters.AddWithValue("@notes", noteModel.notes);
                if (GlobalConfig.MyConfig.Connection_OS_BuySheet != null && GlobalConfig.MyConfig.Connection_OS_BuySheet.State == ConnectionState.Closed)
                    GlobalConfig.MyConfig.Connection_OS_BuySheet.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ResultModel.r = 0;
                ResultModel.msg = ex.ToString();
            }
            finally
            {
                if (GlobalConfig.MyConfig.Connection_OS_BuySheet.State != ConnectionState.Closed) { GlobalConfig.MyConfig.Connection_OS_BuySheet.Close(); }
                cmd.Dispose();
            }
            */
            if (ResultModel.r == 1)
                regenerateNotes(cluster, noteModel.entryID, noteModel.viewID);

            return ResultModel;
        }


        public SP_Return MarkAsReadNotesForOneBuy(string cluster, string pm_EntryID, int pm_viewID)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Buy> BuyList = new List<Buy>();
            List<BuyNote> NoteList = new List<BuyNote>();

            string codePoint = "1F5CE";//🗎
            int code = int.Parse(codePoint, System.Globalization.NumberStyles.HexNumber);
            string unicodeString = char.ConvertFromUtf32(code);

            switch (cluster)
            {
                case "OS":
                    BuyList = GlobalVariables.MySession.List_Buy_OSSheet;
                    NoteList = GlobalVariables.MySession.List_Buy_OSNotes;
                    break;
                case "Domestic":
                    BuyList = GlobalVariables.MySession.List_Buy_DomesticSheet;
                    NoteList = GlobalVariables.MySession.List_Buy_DomesticNotes;
                    break;
                case "FTB":
                    BuyList = GlobalVariables.MySession.List_Buy_FTBSheet;
                    NoteList = GlobalVariables.MySession.List_Buy_FTBNotes;
                    break;
                default:
                    break;
            }

            NoteList = NoteList.Where(n => n.viewID == pm_viewID).ToList();
            if (NoteList.Count() <= 0)
                return ResultModel;

            //double check
            switch (cluster)
            {
                case "OS":
                    ResultModel = shareCommon.checkAuthorized(19);
                    break;
                case "Domestic":
                    ResultModel = shareCommon.checkAuthorized(31);
                    break;
                case "FTB":
                    ResultModel = shareCommon.checkAuthorized(107);
                    break;
                default:
                    ResultModel.r = 0;
                    ResultModel.msg = "Unavailable Cluster.";
                    break;
            }
            if (ResultModel.r == 0)
                return ResultModel;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@entryId", SqlDbType = SqlDbType.VarChar, Value = pm_EntryID}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_UpdateBuyNotes2", wkParm, true);

            if (ResultModel.r == 1)
            {
                shareCommon.CleanCache();

                BuyList.Where(b => b.viewID == pm_viewID).FirstOrDefault().noteCount = unicodeString + NoteList.Count().ToString();

                switch (cluster)
                {
                    case "OS":
                        GlobalVariables.MySession.List_Buy_OSSheet = null;
                        GlobalVariables.MySession.List_Buy_OSSheet = BuyList;
                        break;
                    case "Domestic":
                        GlobalVariables.MySession.List_Buy_DomesticSheet = null;
                        GlobalVariables.MySession.List_Buy_DomesticSheet = BuyList;
                        break;
                    case "FTB":
                        GlobalVariables.MySession.List_Buy_FTBSheet = null;
                        GlobalVariables.MySession.List_Buy_FTBSheet = BuyList;
                        break;
                    default:
                        ResultModel.r = 0;
                        ResultModel.msg = "Unavailable Cluster.";
                        break;
                }
            }
            /*
            SqlCommand cmd = new SqlCommand("sbs_sp_OSBUY_UpdateBuyNotes2", GlobalConfig.MyConfig.Connection_OS_BuySheet) { CommandType = CommandType.StoredProcedure, CommandTimeout = 0 };
            try
            {
                cmd.Parameters.AddWithValue("@entryId", pm_EntryID);
                if (GlobalConfig.MyConfig.Connection_OS_BuySheet != null && GlobalConfig.MyConfig.Connection_OS_BuySheet.State == ConnectionState.Closed)
                    GlobalConfig.MyConfig.Connection_OS_BuySheet.Open();
                cmd.ExecuteNonQuery();

                shareCommon.CleanCache();

                BuyList.Where(b => b.viewID == pm_viewID).FirstOrDefault().noteCount = unicodeString + NoteList.Count().ToString();

                switch (cluster)
                {
                    case "OS":
                        GlobalVariables.MySession.List_Buy_OSSheet = null;
                        GlobalVariables.MySession.List_Buy_OSSheet = BuyList;
                        break;
                    case "Domestic":
                        GlobalVariables.MySession.List_Buy_DomesticSheet = null;
                        GlobalVariables.MySession.List_Buy_DomesticSheet = BuyList;
                        break;
                    case "FTB":
                        GlobalVariables.MySession.List_Buy_FTBSheet = null;
                        GlobalVariables.MySession.List_Buy_FTBSheet = BuyList;
                        break;
                    default:
                        ResultModel.r = 0;
                        ResultModel.msg = "Unavailable Cluster.";
                        break;
                }
            }
            catch (Exception ex)
            {
                ResultModel.r = 0;
                ResultModel.msg = ex.ToString();
            }
            finally
            {
                if (GlobalConfig.MyConfig.Connection_OS_BuySheet.State != ConnectionState.Closed) { GlobalConfig.MyConfig.Connection_OS_BuySheet.Close(); }
                cmd.Dispose();
            }
            */
            return ResultModel;
        }


        public SP_Return CreateNewNote(string cluster, BuyNote noteModel, string sender)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            if (sender == "BuyCreate")
            {
                switch (cluster)
                {
                    case "OS":
                        ResultModel = shareCommon.checkAuthorized(18);
                        break;
                    case "Domestic":
                        ResultModel = shareCommon.checkAuthorized(30);
                        break;
                    case "FTB":
                        ResultModel = shareCommon.checkAuthorized(106);
                        break;
                    default:
                        ResultModel.r = 0;
                        ResultModel.msg = "Unavailable Cluster.";
                        break;
                }
            }
            else if (sender == "BuySheet")
            {
                switch (cluster)
                {
                    case "OS":
                        ResultModel = shareCommon.checkAuthorized(19);
                        break;
                    case "Domestic":
                        ResultModel = shareCommon.checkAuthorized(31);
                        break;
                    case "FTB":
                        ResultModel = shareCommon.checkAuthorized(107);
                        break;
                    default:
                        ResultModel.r = 0;
                        ResultModel.msg = "Unavailable Cluster.";
                        break;
                }
            }
            else
            {
                ResultModel.r = 0;
                ResultModel.msg = "Invalid sender.";
            }

            if (ResultModel.r == 0)
                return ResultModel;
            /*
            SqlCommand cmd = new SqlCommand("sbs_sp_OSBUY_SaveNotes", GlobalConfig.MyConfig.Connection_OS_BuySheet) { CommandType = CommandType.StoredProcedure, CommandTimeout = 0 };
            try
            {
                cmd.Parameters.AddWithValue("@entryId", noteModel.entryID);
                cmd.Parameters.AddWithValue("@notes", noteModel.notes);
                cmd.Parameters.AddWithValue("@user", GlobalVariables.MySession.Account);
                if (GlobalConfig.MyConfig.Connection_OS_BuySheet != null && GlobalConfig.MyConfig.Connection_OS_BuySheet.State == ConnectionState.Closed)
                    GlobalConfig.MyConfig.Connection_OS_BuySheet.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ResultModel.r = 0;
                ResultModel.msg = ex.ToString();
            }
            finally
            {
                if (GlobalConfig.MyConfig.Connection_OS_BuySheet.State != ConnectionState.Closed) { GlobalConfig.MyConfig.Connection_OS_BuySheet.Close(); }
                cmd.Dispose();
            }
            */
            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@entryId", SqlDbType = SqlDbType.VarChar, Value = noteModel.entryID},
                new SqlParameter() {ParameterName = "@notes", SqlDbType = SqlDbType.VarChar, Value = noteModel.notes},
                new SqlParameter() {ParameterName = "@user", SqlDbType = SqlDbType.VarChar, Value = GlobalVariables.MySession.Account}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_SaveNotes", wkParm, true);

            if (ResultModel.r == 1 && noteModel.viewID > 0)
                regenerateNotes(cluster, noteModel.entryID, noteModel.viewID);

            return ResultModel;
        }


        public SP_Return DeleteOneNote(string cluster, BuyNote noteModel)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            switch (cluster)
            {
                case "OS":
                    ResultModel = shareCommon.checkAuthorized(19);
                    break;
                case "Domestic":
                    ResultModel = shareCommon.checkAuthorized(31);
                    break;
                case "FTB":
                    ResultModel = shareCommon.checkAuthorized(107);
                    break;
                default:
                    ResultModel.r = 0;
                    ResultModel.msg = "Unavailable Cluster.";
                    break;
            }
            if (ResultModel.r == 0)
                return ResultModel;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@entryId", SqlDbType = SqlDbType.VarChar, Value = noteModel.entryID},
                new SqlParameter() {ParameterName = "@notesId", SqlDbType = SqlDbType.Int, Value = noteModel.notesID}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_DeleteNotes", wkParm, true);
            /*
            SqlCommand cmd = new SqlCommand("sbs_sp_OSBUY_DeleteNotes", GlobalConfig.MyConfig.Connection_OS_BuySheet) { CommandType = CommandType.StoredProcedure, CommandTimeout = 0 };
            try
            {
                cmd.Parameters.AddWithValue("@entryId", noteModel.entryID);
                cmd.Parameters.AddWithValue("@notesId", noteModel.notesID);
                if (GlobalConfig.MyConfig.Connection_OS_BuySheet != null && GlobalConfig.MyConfig.Connection_OS_BuySheet.State == ConnectionState.Closed)
                    GlobalConfig.MyConfig.Connection_OS_BuySheet.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ResultModel.r = 0;
                ResultModel.msg = ex.ToString();
            }
            finally
            {
                if (GlobalConfig.MyConfig.Connection_OS_BuySheet.State != ConnectionState.Closed) { GlobalConfig.MyConfig.Connection_OS_BuySheet.Close(); }
                cmd.Dispose();
            }
            */
            if (ResultModel.r == 1)
                regenerateNotes(cluster, noteModel.entryID, noteModel.viewID);

            return ResultModel;
        }


        public void regenerateNotes(string cluster, string pm_entryID, int pm_viewID)
        {
            List<Buy> BuyList = new List<Buy>();
            Buy thisBuy = pgmBuyOthers.FetchOneBuy(pm_viewID);
            List<BuyNote> NotesListAllBuy = new List<BuyNote>();
            List<BuyNote> newNotesThisBuy = GetNotesForOneBuy(pm_viewID, pm_entryID);
            shareCommon.CleanCache();

            string codePoint = "1F5CE";//🗎
            int code = int.Parse(codePoint, System.Globalization.NumberStyles.HexNumber);
            string unicodeString = char.ConvertFromUtf32(code);

            switch (cluster)
            {
                case "OS":
                    NotesListAllBuy = GlobalVariables.MySession.List_Buy_OSNotes;
                    BuyList = GlobalVariables.MySession.List_Buy_OSSheet;
                    break;
                case "Domestic":
                    NotesListAllBuy = GlobalVariables.MySession.List_Buy_DomesticNotes;
                    BuyList = GlobalVariables.MySession.List_Buy_DomesticSheet;
                    break;
                case "FTB":
                    NotesListAllBuy = GlobalVariables.MySession.List_Buy_FTBNotes;
                    BuyList = GlobalVariables.MySession.List_Buy_FTBSheet;
                    break;
                default:
                    break;
            }

            NotesListAllBuy.RemoveAll(n => n.viewID == pm_viewID);
            NotesListAllBuy.AddRange(newNotesThisBuy);

            if (newNotesThisBuy == null || newNotesThisBuy.Count() <= 0)
                thisBuy.noteCount = unicodeString + "0";
            else
            {
                if (thisBuy.noteCount == "y")
                    thisBuy.noteCount = newNotesThisBuy.Count().ToString() + "/n";
                else
                    thisBuy.noteCount = unicodeString + newNotesThisBuy.Count().ToString();
            }
            BuyList.RemoveAll(n => n.viewID == pm_viewID);
            BuyList.Add(thisBuy);

            switch (cluster)
            {
                case "OS":
                    GlobalVariables.MySession.List_Buy_OSNotes = null;
                    GlobalVariables.MySession.List_Buy_OSNotes = NotesListAllBuy;
                    GlobalVariables.MySession.List_Buy_OSSheet = null;
                    GlobalVariables.MySession.List_Buy_OSSheet = BuyList;
                    break;
                case "Domestic":
                    GlobalVariables.MySession.List_Buy_DomesticNotes = null;
                    GlobalVariables.MySession.List_Buy_DomesticNotes = NotesListAllBuy;
                    GlobalVariables.MySession.List_Buy_DomesticSheet = null;
                    GlobalVariables.MySession.List_Buy_DomesticSheet = BuyList;
                    break;
                case "FTB":
                    GlobalVariables.MySession.List_Buy_FTBNotes = null;
                    GlobalVariables.MySession.List_Buy_FTBNotes = NotesListAllBuy;
                    GlobalVariables.MySession.List_Buy_FTBSheet = null;
                    GlobalVariables.MySession.List_Buy_FTBSheet = BuyList;
                    break;
                default:
                    break;
            }
        }

    }
}
