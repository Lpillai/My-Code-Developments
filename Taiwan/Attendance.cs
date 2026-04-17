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

namespace Taiwan
{
    public class Attendance_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();
        FileActions_Share shareFileActions = new FileActions_Share();
        Get_Share shareGet = new Get_Share();


        public Attendance_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Taiwan Attendance app now.";
            }
        }


        #region Day off

        public List<tw_LeaveSetting> FetchList_TW_LeaveSettings()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<tw_LeaveSetting> theList = new List<tw_LeaveSetting>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prGetList_LeaveSetting");
            if (ResultModel.r == 1)
                theList = JsonConvert.DeserializeObject<List<tw_LeaveSetting>>(ResultModel.JsonData);

            return theList;
        }


        public tw_LeaveAttachment Get_TW_Attachment(int pm_attach_uid)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            tw_LeaveAttachment theAttach = new tw_LeaveAttachment();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_attach_uid", SqlDbType = SqlDbType.Int, Value = pm_attach_uid}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prGet_LeaveAttachment", wkParm);
            if (ResultModel.r == 1)
                theAttach = JsonConvert.DeserializeObject<tw_LeaveAttachment>(ResultModel.JsonData);

            return theAttach;
        }


        public string Upload_TW_LeaveTaking_Attachment(string pm_filePath, int pm_apply_uid)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            MemoryStream data = new MemoryStream();
            using (Stream str = File.OpenRead(pm_filePath))
            {
                str.CopyTo(data);
            }
            data.Seek(0, SeekOrigin.Begin);
            byte[] wkBuf = new byte[data.Length];
            data.Read(wkBuf, 0, wkBuf.Length);
            shareFileActions.deleteWhenUnlock(new System.IO.FileInfo(pm_filePath));

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_apply_uid", SqlDbType = SqlDbType.Int, Value = pm_apply_uid},
                new SqlParameter() {ParameterName = "@pm_attach_file", SqlDbType = SqlDbType.VarBinary, Value = wkBuf},
                new SqlParameter() {ParameterName = "@pm_attach_name", SqlDbType = SqlDbType.NVarChar, Value = Path.GetFileNameWithoutExtension(pm_filePath)},
                new SqlParameter() {ParameterName = "@pm_attach_ext", SqlDbType = SqlDbType.VarChar, Value = Path.GetExtension(pm_filePath)}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prManage_Attachment_Upload", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel.msg;
        }


        public SP_Return Delete_TW_LeaveTaking_Attachment(int pm_attach_uid)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(70);
            if (ResultModel.r == 0)
                return ResultModel;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_attach_uid", SqlDbType = SqlDbType.Int, Value = pm_attach_uid}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prManage_Attachment_Delete", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public tw_day_apply Get_TW_OneLeave(int pm_apply_uid)
        {
            if (GlobalVariables.MySession.List_TW_DayApply == null)
                FetchList_TW_LeaveTaking(GlobalVariables.MySession.Account);
            return GlobalVariables.MySession.List_TW_DayApply.Find(f => f.apply_uid == pm_apply_uid);
        }


        public tw_day_available Get_TW_OneAvailable(string pm_account_id, int pm_day_year, int pm_day_id)
        {
            if (GlobalVariables.MySession.List_TW_DayAvailable == null)
                FetchList_TW_LeaveTaking(pm_account_id);
            return GlobalVariables.MySession.List_TW_DayAvailable.Find(f => f.account_id == pm_account_id && f.day_id == pm_day_id && f.day_year == pm_day_year);
        }


        public tw_LeaveTaking FetchList_TW_LeaveTaking(string pm_account_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            tw_LeaveTaking theList = new tw_LeaveTaking();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_account_id", SqlDbType = SqlDbType.NVarChar, Value = pm_account_id}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prGetList_LeaveTaking", wkParm);
            if (ResultModel.r == 1)
            {
                theList = JsonConvert.DeserializeObject<tw_LeaveTaking>(ResultModel.JsonData);
                //theList.theCards = null;
                //theList.theCards = FetchList_TW_CardStamp(pm_account_id);
                GlobalVariables.MySession.List_TW_DayApply = null;
                GlobalVariables.MySession.List_TW_DayApply = theList.theApplys;
                GlobalVariables.MySession.List_TW_DayAvailable = null;
                GlobalVariables.MySession.List_TW_DayAvailable = theList.theAvailable;
            }

            return theList;
        }


        public SP_Return Update_TW_LeaveTaking(tw_day_apply pm_apply)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            if (pm_apply.day_hours <= 0)
            {
                ResultModel.r = 0;
                ResultModel.msg = "無效的請假時數\r\nThe day off hour must be greater than zero. ";
            }
            if (pm_apply.day_start >= pm_apply.day_end)
            {
                ResultModel.r = 0;
                ResultModel.msg = "無效的請假起迄時間點\r\nThe day off end must be greater than day off start. ";
            }
            if ((pm_apply.day_hours*2) % 1 != 0)
            {
                ResultModel.r = 0;
                ResultModel.msg = "請假時間須以半小時為單位\r\nThe day off unit must be in units of half an hour. ";
            }
            if (ResultModel.r == 0)
                return ResultModel;

            //double check
            ResultModel = shareCommon.checkAuthorized(70);
            if (ResultModel.r == 0)
                return ResultModel;

            if (pm_apply.z_status_chk)
                pm_apply.z_status = "D";
            if (pm_apply.account_id == null)
                pm_apply.account_id = GlobalVariables.MySession.Account;
            pm_apply.z_usr = GlobalVariables.MySession.Account;
            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJson", SqlDbType = SqlDbType.NVarChar, Value = JsonConvert.SerializeObject(pm_apply)}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prManage_LeaveTaking", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return Update_TW_Available(tw_day_available pm_available)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(73);
            if (ResultModel.r == 0)
                return ResultModel;

            if (pm_available.z_status_chk)
                pm_available.z_status = "D";
            //if (pm_available.account_id == null)
            //    pm_available.account_id = GlobalVariables.MySession.Account;
            pm_available.z_usr = GlobalVariables.MySession.Account;
            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJson", SqlDbType = SqlDbType.NVarChar, Value = JsonConvert.SerializeObject(pm_available)}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prManage_Available", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);
            if (ResultModel.r == 1)
                GlobalVariables.MySession.List_TW_DayAvailable = null;

            return ResultModel;
        }


        public SP_Return AddCardStamp(DateTime pmDate)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string JsonString = "{'pmStamp':'" + pmDate.ToString("yyyy-MM-dd HH:mm:00.000") + "', 'z_usr':'" + GlobalVariables.MySession.Account + "'}";
            JsonString = JsonString.Replace("'", "\"");

            //double check
            ResultModel = shareCommon.checkAuthorized(70);
            if (ResultModel.r == 0)
                return ResultModel;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJson", SqlDbType = SqlDbType.VarChar, Value = JsonString}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prAdd_CardStamp", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public List<tw_day_HR_apply> FetchList_TW_HR_LeaveApply()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<tw_day_HR_apply> theList = new List<tw_day_HR_apply>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prGetList_HR_LeaveApplication");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theList = JsonConvert.DeserializeObject<List<tw_day_HR_apply>>(ResultModel.JsonData);

            return theList;
        }

        #endregion

        #region Calendar

        public List<tw_Calendar> FetchList_TW_Calendar()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<tw_Calendar> theList = new List<tw_Calendar>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prGetList_Holiday");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theList = JsonConvert.DeserializeObject<List<tw_Calendar>>(ResultModel.JsonData);
            }

            return theList;
        }


        public SP_Return Update_TW_Calendar(tw_Calendar theHoliday)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(92);
            if (ResultModel.r == 0)
                return ResultModel;

            theHoliday.z_usr = GlobalVariables.MySession.Account;
            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJson", SqlDbType = SqlDbType.NVarChar, Value = JsonConvert.SerializeObject(theHoliday)}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prManage_Calendar", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

        #endregion

        #region Card Stamp

        public string Get_TW_AssignedDate()
        {
            List<CodeValue> theList = shareGet.FetchCodeList("TW_Card").ToList();
            string theDate = theList.Where(w => w.Code == "1").First().Value;
            return theDate;
        }


        public tw_CardStamp Get_TW_OneCardStamp(string pm_card_cd, DateTime pm_card_stamp)
        {
            if (GlobalVariables.MySession.List_TW_CardStamp == null)
                FetchList_TW_CardStamp(pm_card_stamp);

            tw_CardStamp test = GlobalVariables.MySession.List_TW_CardStamp.Find(f => f.card_cd == pm_card_cd && f.card_stamp == pm_card_stamp);
            return test;
        }


        public List<tw_CardStamp> FetchList_TW_CardStamp(DateTime pm_theDate)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<tw_CardStamp> theList = new List<tw_CardStamp>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_theDate", SqlDbType = SqlDbType.Date, Value = pm_theDate}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prGetList_CardStamp", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theList = JsonConvert.DeserializeObject<List<tw_CardStamp>>(ResultModel.JsonData);
                GlobalVariables.MySession.List_TW_CardStamp = null;
                GlobalVariables.MySession.List_TW_CardStamp = theList;
            }

            return theList;
        }


        public SP_Return Update_TW_CardStamp(tw_CardStamp theStamp)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(72);
            if (ResultModel.r == 0)
                return ResultModel;

            theStamp.z_usr = GlobalVariables.MySession.Account;
            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJson", SqlDbType = SqlDbType.NVarChar, Value = JsonConvert.SerializeObject(theStamp)}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prManage_CardStamp", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return ImportCardStamp(List<string> pm_paths, string pm_assigned_date)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string jsonString = "";

            //double check
            ResultModel = shareCommon.checkAuthorized(72);
            if (ResultModel.r == 0)
                return ResultModel;

            List<tw_CardStamp> theStamps = new List<tw_CardStamp>();
            foreach (string aPath in pm_paths)
            {
                string[] lines = File.ReadAllLines(aPath);

                foreach (string aStamp in lines)
                {
                    string[] stampData = aStamp.Split(' ');
                    theStamps.Add(new tw_CardStamp { card_cd = int.Parse(stampData[2]).ToString(), card_stamp = DateTime.ParseExact(stampData[0] + stampData[1], "yyyyMMddHHmm", null), z_status = (int.Parse(stampData[3]) == 7 ? "O" : "") });
                }
            }

            if (theStamps.Count <= 0)
            {
                ResultModel.r = 0;
                ResultModel.msg = "No data found.";
            }
            else
            {
                jsonString = JsonConvert.SerializeObject(theStamps);
                jsonString = "{'z_usr':'" + GlobalVariables.MySession.Account + "', 'stamps': " + jsonString + "}";
                jsonString = jsonString.Replace("'", "\"");
                List<SqlParameter> wkParm = new List<SqlParameter>()
                {
                    new SqlParameter() {ParameterName = "@pmJson", SqlDbType = SqlDbType.VarChar, Value = jsonString}
                };
                ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prImport_CardStamp", wkParm);
                if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                    ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);
                
                DateTime ValidatedDate;
                if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(pm_assigned_date) && DateTime.TryParse(pm_assigned_date, out ValidatedDate))
                {
                    jsonString = ResultModel.msg;

                    wkParm.Clear();
                    wkParm = null;
                    wkParm = new List<SqlParameter>()
                    {
                        new SqlParameter() {ParameterName = "@pmCluster", SqlDbType = SqlDbType.VarChar, Value = "TW_Card"},
                        new SqlParameter() {ParameterName = "@pmCode", SqlDbType = SqlDbType.Int, Value = 1},
                        new SqlParameter() {ParameterName = "@pmValue", SqlDbType = SqlDbType.NVarChar, Value = pm_assigned_date}
                    };
                    ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prSet_CodeStore", wkParm);
                    if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                        ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

                    if (ResultModel.r == 1)
                    {
                        ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prMatch_CardStamp", null);
                        if (ResultModel.r != 1)
                            ResultModel.msg = jsonString + "\r\n" + ResultModel.msg;
                        else
                            ResultModel.msg = jsonString;
                    }
                }
            }

            return ResultModel;
        }

        #endregion

        #region Approval

        public List<tw_LeaveApproval> FetchList_TW_LeaveApproval()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<tw_LeaveApproval> theList = new List<tw_LeaveApproval>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_account_id", SqlDbType = SqlDbType.NVarChar, Value = GlobalVariables.MySession.Account}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prGetList_LeaveApproval", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theList = JsonConvert.DeserializeObject<List<tw_LeaveApproval>>(ResultModel.JsonData);
                GlobalVariables.MySession.List_TW_DayAproval = null;
                GlobalVariables.MySession.List_TW_DayAproval = theList;
            }

            return theList;
        }


        public tw_day_available LoadApprovalToAvaiable(tw_LeaveApproval pmApply)
        {
            //tw_day_available theAvailable = new tw_day_available() { day_id = pmApply.day_id, day_year = pmApply.day_year, day_name = pmApply.day_name, available_hours = pmApply.day_hours };
            tw_day_available theAvailable = new tw_day_available() { account_id = pmApply.account_id, day_id = pmApply.day_id, day_year = pmApply.day_year, day_name = pmApply.day_name, available_start = pmApply.day_start, available_end = pmApply.day_end, available_hours = pmApply.day_hours };

            switch (theAvailable.day_id)
            {
                case 6: //喪假，百日內8天、6天、3天。
                    theAvailable.available_hours = 0;
                    break;
                case 7: //婚假，自結婚日(戶政機關登記結婚之日)前10日起3個月內分次請休8天。
                    theAvailable.available_hours = 8;
                    break;
                case 9: //陪產假，於配偶分娩之當日及其前後合計15日期間內，擇其中之7日請假。
                    theAvailable.available_hours = 7;
                    break;
                default:
                    break;
            }
            theAvailable.available_end = theAvailable.available_start.AddDays(Decimal.ToDouble(theAvailable.available_hours));

            return theAvailable;
        }


        public SP_Return Update_LeaveApproval(string pm_apply_uid, int pm_approval, string pm_reason)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(71);
            if (ResultModel.r == 0)
                return ResultModel;

            int testUid = 0;
            if (string.IsNullOrWhiteSpace(pm_apply_uid) || !int.TryParse(pm_apply_uid, out testUid))
            {
                ResultModel.r = 0;
                ResultModel.msg = "UID 有誤\r\nWrong UID.";
                return ResultModel;
            }
            if (pm_approval == -1 && string.IsNullOrWhiteSpace(pm_reason))
            {
                ResultModel.r = 0;
                ResultModel.msg = "請輸入退回原因\r\nPlease enter rejection reason.";
                return ResultModel;
            }

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_apply_uid", SqlDbType = SqlDbType.Int, Value = testUid},
                new SqlParameter() {ParameterName = "@pm_approval", SqlDbType = SqlDbType.Int, Value = pm_approval},
                new SqlParameter() {ParameterName = "@pm_reason", SqlDbType = SqlDbType.NVarChar, Value = pm_reason},
                new SqlParameter() {ParameterName = "@pm_usr", SqlDbType = SqlDbType.NVarChar, Value = GlobalVariables.MySession.Account}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prManage_LeaveApproval", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

        #endregion

        #region Calculation and Inform

        public List<tw_CardStamp_Matching> FetchList_TW_CardStamp_Matching()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<tw_CardStamp_Matching> theList = new List<tw_CardStamp_Matching>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prMatch_CardStamp2");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theList = JsonConvert.DeserializeObject<List<tw_CardStamp_Matching>>(ResultModel.JsonData);
            }

            return theList;
        }


        public SP_Return Inform_TW_CardStamp_Matching(List<tw_CardStamp_Matching> pm_Stamps)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<tw_CardStamp_Matching> theStamps = pm_Stamps.Where(w => w.chk == true).ToList();
            string JsonString = JsonConvert.SerializeObject(theStamps);
            JsonString = "{\"z_usr\":\"" + GlobalVariables.MySession.Account + "\", \"pm_Stamps\":" + JsonString + "}";
            //JsonString = JsonString.Replace("{[{", "{\"z_usr\":\"" + GlobalVariables.MySession.Account + "\", \"pm_Stamps\":[{");

            //double check
            ResultModel = shareCommon.checkAuthorized(93);
            if (ResultModel.r == 0)
                return ResultModel;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJson", SqlDbType = SqlDbType.NVarChar, Value = JsonString}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prInform_CardStamp", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

        #endregion

        #region Reports

        public MemoryStream GetDayOffList()
        {
            var stream = new MemoryStream();
            tw_DayOffList theList = FetchList_DayOffList();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add("請假明細");
                workSheet.Cells["A1"].LoadFromCollection(theList.theOffs, true);
                workSheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(1).Style.Numberformat.Format = "@";
                workSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(6).Style.Numberformat.Format = "yyyy/MM/dd hh:MM";
                workSheet.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(7).Style.Numberformat.Format = "yyyy/MM/dd hh:MM";
                workSheet.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(9).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                workSheet.View.FreezePanes(2, 1);

                workSheet = null;
                workSheet = package.Workbook.Worksheets.Add("剩餘特休");
                workSheet.Cells["A1"].LoadFromCollection(theList.theUsable, true);
                workSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(3).Style.Numberformat.Format = "yyyy/MM/dd";
                workSheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(4).Style.Numberformat.Format = "yyyy/MM/dd";
                workSheet.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(6).Style.Numberformat.Format = "yyyy/MM/dd";
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                workSheet.View.FreezePanes(2, 1);

                package.Save();
            }
            stream.Position = 0;

            return stream;
        }


        public tw_DayOffList FetchList_DayOffList()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            tw_DayOffList theList = new tw_DayOffList();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "reporting.tw_current_ym_day_off");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theList = JsonConvert.DeserializeObject<tw_DayOffList>(ResultModel.JsonData);

            return theList;
        }


        #endregion

    }
}
