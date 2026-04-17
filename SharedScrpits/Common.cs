using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuntimeConfig;
using RuntimeVariables;

namespace SharedScrpits
{
    public class Common_Share
    {
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        //Get_Share shareGet = new Get_Share();


        public ViewBagPageInfo GetPageInfo(int pm_MenuID)
        {
            Login item = GlobalVariables.MySession.LoginMenuList.Where(w => w.menu_id == pm_MenuID).First();
            AppendLog(new ActionLog() { keys = item.menu_name });
            return new ViewBagPageInfo() { Title = item.menu_name, PageName = item.menu_name.Replace(" ", ""), Path = item.menu_path };
        }


        public bool isAuthorized(int pm_MenuID)
        {
            Login theMenuItem = GlobalVariables.MySession.LoginMenuList.FirstOrDefault(m => m.menu_id == pm_MenuID);
            return ((theMenuItem == null || !string.IsNullOrWhiteSpace(theMenuItem.kind)) ? false : true);
        }


        public SP_Return checkAuthorized(int pm_MenuID)
        {
            bool authorized = isAuthorized(pm_MenuID);

            SP_Return resultModel = new SP_Return();
            resultModel.r = authorized == true ? 1 : 0;
            resultModel.msg = authorized == true ? "" : "You don't have permission to access this action.";
            resultModel.JsonData = "";

            return resultModel;
        }

        
        public bool isReportAuthorized(int pm_report_id)
        {
            ReportItem theReportItem = GlobalVariables.MySession.LoginReportList.FirstOrDefault(r => r.report_id == pm_report_id);
            return ((theReportItem == null || string.IsNullOrWhiteSpace(theReportItem.run) || theReportItem.rpt_chk_read) ? false : true);
        }
        

        public SP_Return checkReportAuthorized(int pm_report_id)
        {
            bool authorized = isReportAuthorized(pm_report_id);

            SP_Return resultModel = new SP_Return();
            resultModel.r = authorized == true ? 1 : 0;
            resultModel.msg = authorized == true ? "" : "You don't have permission to access this report.";
            resultModel.JsonData = "";

            return resultModel;
        }


        public SP_Return ExecSP(SqlConnection pmConnection, string pmSP, List<SqlParameter> pmParm = null, Boolean pmExecOnly = false)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            SqlCommand cmd = new SqlCommand(pmSP, pmConnection) { CommandType = CommandType.StoredProcedure, CommandTimeout = 0 };

            try
            {
                cmd.Parameters.Clear();
                if (pmParm != null)
                    cmd.Parameters.AddRange(pmParm.ToArray());

                if (pmConnection != null && pmConnection.State == ConnectionState.Closed)
                    pmConnection.Open();

                if (pmExecOnly)
                    cmd.ExecuteNonQuery();
                else
                    ResultModel.JsonData = (string)cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                ResultModel.r = 0;
                ResultModel.msg = ex.ToString();
            }
            finally
            {
                //if (pmConnection.State != ConnectionState.Closed) { pmConnection.Close(); }
                cmd.Dispose();
            }

            return ResultModel;
        }


        public async Task<HttpResponseMessage> callSBS_API_Get(string address, string parms) //WebApi Client libraries
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            //Uri theURL = new Uri("https://localhost:44334");
            //address = "/api/" + address + "?" + parms;
            Uri theURL = new Uri("http://sbs-intranet.specialtybolt.com/");
            //address = "/SBS_API/api/" + address + "?" + parms;
            address = "/SBS_API/api/" + address + (string.IsNullOrWhiteSpace(parms) ? "" : "?" + parms);
            HttpResponseMessage response = new HttpResponseMessage();

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(6);
                client.BaseAddress = theURL;
                client.DefaultRequestHeaders.Connection.Add("keep-alive");
                client.DefaultRequestHeaders.Add("Keep-Alive", "600");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //HttpResponseMessage response = await client.GetAsync(address, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                response = await client.GetAsync(address, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                /*
                if (!response.IsSuccessStatusCode)
                    ResultModel.r = 0;
                ResultModel.msg = "StatusCode = " + ((int)response.StatusCode).ToString() + ", " + response.ReasonPhrase;
                using (StreamReader stream = new StreamReader(response.Content.ReadAsStreamAsync().Result, Encoding.UTF8))
                {
                    ResultModel.JsonData = stream.ReadToEnd();
                }
                */
            }

            //return ResultModel;
            return response;
        }


        public async Task<SP_Return> callSBS_API_Post(string address, string request) //WebApi Client libraries
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            Uri theURL = new Uri("http://sbs-intranet.specialtybolt.com/");
            //Uri theURL = new Uri("https://localhost:44334");
            //Uri theURL = new Uri("http://" + GlobalConfig.MyConfig.WebServerIP + "/");
            address = "/SBS_API/api/" + address;
            //address = "/api/" + address;

            using (HttpClient client = new HttpClient())
            {
                //var sp = ServicePointManager.FindServicePoint(theURL);
                //sp.ConnectionLeaseTimeout = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;    //change the max connection lifetime
                //sp.MaxIdleTime = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;   //change the idle connection timeout
                client.Timeout = TimeSpan.FromMinutes(6);

                client.BaseAddress = theURL;
                client.DefaultRequestHeaders.Connection.Add("keep-alive");
                client.DefaultRequestHeaders.Add("Keep-Alive", "600");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent requestContent = new StringContent(request, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(address, requestContent);
                if (!response.IsSuccessStatusCode)
                    ResultModel.r = 0;
                ResultModel.msg = "StatusCode = " + ((int)response.StatusCode).ToString() + ", " + response.ReasonPhrase;
                using (StreamReader stream = new StreamReader(response.Content.ReadAsStreamAsync().Result, Encoding.UTF8))
                    ResultModel.JsonData = stream.ReadToEnd();
            }

            return ResultModel;
        }


        public Func<object, object> GetPropertyGetter(string typeName, string propertyName)
        {
            Type t = Type.GetType(typeName);
            PropertyInfo pi = t.GetProperty(propertyName);
            MethodInfo getter = pi.GetGetMethod();

            DynamicMethod dm = new DynamicMethod("GetValue", typeof(object), new Type[] { typeof(object) }, typeof(object), true);
            ILGenerator lgen = dm.GetILGenerator();

            lgen.Emit(OpCodes.Ldarg_0);
            lgen.Emit(OpCodes.Call, getter);

            if (getter.ReturnType.GetTypeInfo().IsValueType)
            {
                lgen.Emit(OpCodes.Box, getter.ReturnType);
            }

            lgen.Emit(OpCodes.Ret);
            return dm.CreateDelegate(typeof(Func<object, object>)) as Func<object, object>;
        }


        public SP_Return AddScheduleForAPI(string pm_sp, string pm_parm, bool pm_isParm)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            if (string.IsNullOrEmpty(pm_sp))
            {
                ResultModel.r = 0;
                ResultModel.msg = "No command was in.";
                return ResultModel;
            }

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_sp", SqlDbType = SqlDbType.NVarChar, Value = pm_sp},
                new SqlParameter() {ParameterName = "@pm_parm", SqlDbType = SqlDbType.NVarChar, Value = pm_parm},
                new SqlParameter() {ParameterName = "@pm_z_usr", SqlDbType = SqlDbType.VarChar, Value = GlobalVariables.MySession.Account},
                new SqlParameter() {ParameterName = "@pm_isParm", SqlDbType = SqlDbType.Bit, Value = pm_isParm}
            };
            ResultModel = ExecSP(GlobalConfig.MyConfig.Connection_SBS, "SBS.z.prP21_API_NewSchedule", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public void CleanCache()
        {
            MemoryCache cache = MemoryCache.Default;
            List<string> cacheKeys = cache.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
                cache.Remove(cacheKey);
        }


        public void AppendLog(ActionLog pmLog)
        {
            if (!string.IsNullOrWhiteSpace(GlobalVariables.MySession.Account))
            {
                pmLog.field_name = GlobalVariables.MySession.Account;

                List<SqlParameter> wkParm = new List<SqlParameter>()
                {
                    new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = JsonConvert.SerializeObject(pmLog)}
                };
                ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_ActionLog", wkParm, true);
            }
        }

    }
}
