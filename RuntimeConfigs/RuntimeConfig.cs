using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace RuntimeConfig
{
    public class GlobalConfig
    {
        private GlobalConfig()
        {
            //Validity = true;
            //WebServerIP = "sbs-intranet.specialtybolt.com";
            Connection_SBS = new SqlConnection("Data Source=192.100.100.46;Initial Catalog=SBS_dev;Persist Security Info=True;User ID=AppBot;Password=47Y@p88");
            //Connection_SBS = new SqlConnection("Data Source=192.100.100.46;Initial Catalog=SBS;Persist Security Info=True;User ID=AppBot;Password=47Y@p88");
            Connection_OS_BuySheet = new SqlConnection("Data Source=192.100.100.46;Initial Catalog=OS_BuySheet_dev;Persist Security Info=True;User ID=AppBot;Password=47Y@p88");
            //Connection_OS_BuySheet = new SqlConnection("Data Source=192.100.100.46;Initial Catalog=OS_BuySheet;Persist Security Info=True;User ID=AppBot;Password=47Y@p88");
            //ProjectName = "SBS_Intranet_dev";
            //SSRSReportUrl = "http://SBS-DB02-21:80/ReportServer/";
            SSRSReportUrl = "http://192.100.100.46/ReportServer/";
            SSRSdomain = "specialtybolt.com";
            SSRSuser = "SBS";
            SSRSpassword = "shipBIGscreen0*";
        }


        public static GlobalConfig MyConfig
        {
            get
            {
                return new GlobalConfig();
            }
        }


        //public bool Validity { get; set; }
        //public string WebServerIP { get; set; }
        public SqlConnection Connection_SBS { get; set; }
        public SqlConnection Connection_OS_BuySheet { get; set; }
        //public string ProjectName { get; set; }
        public string SSRSReportUrl { get; set; }
        public string SSRSdomain { get; set; }
        public string SSRSuser { get; set; }
        public string SSRSpassword { get; set; }

    }
}
