using RuntimeVariables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using SharedScrpits;
using System.Threading;

namespace Home.Models
{
    public class HomeScripts
    {
        Common_Share shareCommon = new Common_Share();
        public bool isDebug = false;


        [Conditional("DEBUG")]
        private void IsDebugCheck(ref bool isDebug)
        {
            isDebug = true;
        }


        private void setDebug()
        {
            IsDebugCheck(ref isDebug);
            GlobalVariables.MySession.isDebug = isDebug;
        }

        
        public bool debugStatus()
        {
            return GlobalVariables.MySession.isDebug;
        }


        private void setDEV()
        {
            //GlobalVariables.MySession.isDEV = (debugStatus() || getProjectName().ToLower().Contains("dev") ? true : false);
            GlobalVariables.MySession.isDEV = (getProjectName().ToLower().Contains("dev") ? true : false);
        }

        
        public bool devStatus()
        {
            return GlobalVariables.MySession.isDEV;
        }


        public void FetchEnvironment()
        {
            setDebug();
            setDEV();
        }


        public string getProjectName()
        {
            return new Get_Share().GetProjectName();
        }

    }
}