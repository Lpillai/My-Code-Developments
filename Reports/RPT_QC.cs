using Microsoft.Reporting.WebForms;
using RuntimeVariables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports
{
    public class RPT_QC_Program
    {
        Reports_Program pgmReports = new Reports_Program();


        public MemoryStream Get_QC_Sample(string rpt, string pm_po_no, string pm_line_no, string pm_unit_wgt, string pm_vendor_lot)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("po_no", pm_po_no));
            parms.Add(new ReportParameter("line_no", pm_line_no));
            parms.Add(new ReportParameter("unit_wgt", pm_unit_wgt));
            parms.Add(new ReportParameter("vendor_lot", pm_vendor_lot));
            parms.Add(new ReportParameter("z_usr", GlobalVariables.MySession.Account));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }


        public MemoryStream Get_QC_SampleProcess(string rpt, string pm_po_no, string pm_line_no)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("po_no", pm_po_no));
            parms.Add(new ReportParameter("line_no", pm_line_no));
            parms.Add(new ReportParameter("z_usr", GlobalVariables.MySession.Account));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }

    }
}
