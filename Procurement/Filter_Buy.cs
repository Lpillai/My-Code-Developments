using RuntimeVariables;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Procurement
{
    public class Buy_Fileter
    {
        //Get_Share shareGet = new Get_Share();
        //ModelGetter getModel = new ModelGetter();

        /*
        public List<Buy> parseFilters_Int(List<Buy> pmBuyList, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(Buy), "w");
            var property = Expression.Property(parameterExpression, pmFilter.key);

            int pmConstant_s = 0;
            if (!string.IsNullOrEmpty(pmFilter.value_s))
                pmConstant_s = Int32.Parse(pmFilter.value_s);

            int pmConstant_e = 0;
            if (!string.IsNullOrEmpty(pmFilter.value_e))
                pmConstant_e = Int32.Parse(pmFilter.value_e);

            switch (pmFilter.op)
            {
                case ">":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmBuyList;
        }


        public List<Buy> parseFilters_DateTime(List<Buy> pmBuyList, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(Buy), "w");
            var property = Expression.Property(parameterExpression, pmFilter.key);

            DateTime? pmConstant_s = null;
            if (string.IsNullOrEmpty(pmFilter.value_s))
                return pmBuyList;
            pmConstant_s = Convert.ToDateTime(pmFilter.value_s);

            DateTime? pmConstant_e = null;
            if (!string.IsNullOrEmpty(pmFilter.value_e))
                pmConstant_e = Convert.ToDateTime(pmFilter.value_e);

            switch (pmFilter.op)
            {
                case ">":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmBuyList;
        }


        public List<Buy> parseFilters_Decimal(List<Buy> pmBuyList, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(Buy), "w");
            var property = Expression.Property(parameterExpression, pmFilter.key);

            decimal pmConstant_s = 0;
            if (!string.IsNullOrEmpty(pmFilter.value_s))
                pmConstant_s = Decimal.Parse(pmFilter.value_s);

            decimal pmConstant_e = 0;
            if (!string.IsNullOrEmpty(pmFilter.value_e))
                pmConstant_e = Decimal.Parse(pmFilter.value_e);

            switch (pmFilter.op)
            {
                case ">":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Buy, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmBuyList;
        }


        public List<Buy> parseFilters_String(List<Buy> pmBuyList, Filters pmFilter)
        {
            List<Buy> result = new List<Buy>();
            List<Buy> rejectResult = new List<Buy>();
            string pmConstant_s = pmFilter.value_s == null ? "" : pmFilter.value_s;
            //var getter = shareGet.GetPropertyGetter(typeof(Buy).ToString(), pmFilter.key);
            var getter = getModel.GetPropertyGetter(typeof(Buy).ToString(), pmFilter.key);
            
            //var type = Type.GetType(typeof(Buy).ToString());
            ////var member = type.GetProperty(pmFilter.key);
            //var member = pgmSP.GetProp(type, pmFilter.key);
            //var parm = Expression.Parameter(type, "s");
            //var propertyExp = Expression.Property(parm, pmFilter.key);
            
            switch (pmFilter.op)
            {
                case ">=":
                    result = pmBuyList.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).StartsWith(pmConstant_s)).ToList();
                    break;
                case "<=":
                    result = pmBuyList.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).EndsWith(pmConstant_s)).ToList();
                    break;
                case "%":
                    result = pmBuyList.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).ToLower().Contains(pmConstant_s.ToLower())).ToList();
                    break;
                case "!%":
                    rejectResult = pmBuyList.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).ToLower().Contains(pmConstant_s.ToLower())).ToList();
                    result = pmBuyList.Except(rejectResult).ToList();
                    break;
                case "=":
                    //result = pmBuyList.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).Equals(pmConstant_s)).ToList();
                    result = pmBuyList.FindAll(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).Equals(pmConstant_s));

                    //var equal = Expression.Equal(Expression.Property(parm, pmFilter.key), Expression.Constant(pmConstant_s, member.PropertyType));
                    //var lambda_eq = Expression.Lambda<Func<Buy, bool>>(equal, parm);
                    //result = pmBuyList.AsQueryable().Where(lambda_eq).ToList();

                    break;
                case "!=":
                    result = pmBuyList.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)) != pmConstant_s).ToList();
                    //var not_equal = Expression.NotEqual(Expression.Property(parm, pmFilter.key), Expression.Constant(pmConstant_s, member.PropertyType));
                    //var lambda_ne = Expression.Lambda<Func<Buy, bool>>(not_equal, parm);
                    //result = pmBuyList.AsQueryable().Where(lambda_ne).ToList();
                    break;
                default:
                    result = pmBuyList;
                    break;
            }

            //result = pmBuyList.FindAll(f => f.theFTB.procure == "Mandy");

            return result;
        }
        */

        public List<Filters> getFilters(string cluster, string pm_Source)
        {
            List<Filters> FilterList = new List<Filters>();

            switch (cluster)
            {
                case "OS":
                    if (pm_Source == "BuySheet" && GlobalVariables.MySession.List_Buy_OSFilters != null)
                        FilterList = GlobalVariables.MySession.List_Buy_OSFilters;
                    else
                    if (pm_Source == "BuyArchive" && GlobalVariables.MySession.List_ArchiveBuy_OSFilters != null)
                        FilterList = GlobalVariables.MySession.List_ArchiveBuy_OSFilters;
                    break;
                case "Domestic":
                    if (pm_Source == "BuySheet" && GlobalVariables.MySession.List_Buy_DomesticFilters != null)
                        FilterList = GlobalVariables.MySession.List_Buy_DomesticFilters;
                    else
                    if (pm_Source == "BuyArchive" && GlobalVariables.MySession.List_ArchiveBuy_DomesticFilters != null)
                        FilterList = GlobalVariables.MySession.List_ArchiveBuy_DomesticFilters;
                    break;
                case "FTB":
                    if (pm_Source == "BuySheet" && GlobalVariables.MySession.List_Buy_FTBFilters != null)
                        FilterList = GlobalVariables.MySession.List_Buy_FTBFilters;
                    else
                    if (pm_Source == "BuyArchive" && GlobalVariables.MySession.List_ArchiveBuy_FTBFilters != null)
                        FilterList = GlobalVariables.MySession.List_ArchiveBuy_FTBFilters;
                    break;
                default:
                    break;
            }

            if (FilterList == null || FilterList.Count() <= 0)
            {
                FilterList.Add(new Filters() { key = "viewID", key_name = "Buy ID", op = "", value_type = "int", value_s = "", value_e = "" });
                //FilterList.Add(new Filters() { key = "viewID", key_name = "Buy ID", op = "", value_type = "number", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "ItemID", key_name = "Part Number", op = "", value_type = "text", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "date_created", key_name = "Date Created", op = "", value_type = "date", value_s = "", value_e = "" });
                if (cluster != "FTB")
                    FilterList.Add(new Filters() { key = "planner", key_name = "Planner", op = "", value_type = "text", value_s = "", value_e = "" });
                else
                {
                    FilterList.Add(new Filters() { key = "creator", key_name = "Requester", op = "", value_type = "text", value_s = "", value_e = "" });
                    FilterList.Add(new Filters() { key = "filter_sourcing", key_name = "Sourced By", op = "", value_type = "text", value_s = "", value_e = "" });
                    FilterList.Add(new Filters() { key = "filter_buyer", key_name = "Buyer", op = "", value_type = "text", value_s = "", value_e = "" });
                }
                FilterList.Add(new Filters() { key = "buy_to_loc", key_name = "Buy to Location", op = "", value_type = "text", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "vendor_approve", key_name = "Vendor Approve", op = "", value_type = "text", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "PO_no", key_name = "PO Number", op = "", value_type = "text", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "PPAP", key_name = "PPAP", op = "", value_type = "text", value_s = "", value_e = "" });
                if (cluster != "FTB")
                    FilterList.Add(new Filters() { key = "program", key_name = "Program", op = "", value_type = "text", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "TruePrimarySupplier", key_name = "True Primary Supplier", op = "", value_type = "text", value_s = "", value_e = "" });
                //FilterList.Add(new Filters() { key = "supplier_1", key_name = "Supplier 1", op = "", value_type = "text", value_s = "", value_e = "" });
                //FilterList.Add(new Filters() { key = "supplier_2", key_name = "Supplier 2", op = "", value_type = "text", value_s = "", value_e = "" });
                if (cluster != "FTB")
                    FilterList.Add(new Filters() { key = "creator", key_name = "Creator", op = "", value_type = "text", value_s = "", value_e = "" });
            }

            return FilterList;
        }


        public void PostFilter(List<Filters> pmFilterList, string cluster, string back)
        {
            switch (cluster)
            {
                case "OS":
                    if (back == "BuySheet")
                    {
                        GlobalVariables.MySession.List_Buy_OSFilters = null;
                        if (pmFilterList != null)
                            GlobalVariables.MySession.List_Buy_OSFilters = pmFilterList.ToList();
                    }
                    else
                    if (back == "BuyArchive")
                    {
                        GlobalVariables.MySession.List_ArchiveBuy_OSFilters = null;
                        if (pmFilterList != null)
                            GlobalVariables.MySession.List_ArchiveBuy_OSFilters = pmFilterList.ToList();
                    }
                    break;
                case "Domestic":
                    if (back == "BuySheet")
                    {
                        GlobalVariables.MySession.List_Buy_DomesticFilters = null;
                        if (pmFilterList != null)
                            GlobalVariables.MySession.List_Buy_DomesticFilters = pmFilterList.ToList();
                    }
                    else
                    if (back == "BuyArchive")
                    {
                        GlobalVariables.MySession.List_ArchiveBuy_DomesticFilters = null;
                        if (pmFilterList != null)
                            GlobalVariables.MySession.List_ArchiveBuy_DomesticFilters = pmFilterList.ToList();
                    }
                    break;
                case "FTB":
                    if (back == "BuySheet")
                    {
                        GlobalVariables.MySession.List_Buy_FTBFilters = null;
                        if (pmFilterList != null)
                            GlobalVariables.MySession.List_Buy_FTBFilters = pmFilterList.ToList();
                    }
                    else
                    if (back == "BuyArchive")
                    {
                        GlobalVariables.MySession.List_ArchiveBuy_FTBFilters = null;
                        if (pmFilterList != null)
                            GlobalVariables.MySession.List_ArchiveBuy_FTBFilters = pmFilterList.ToList();
                    }
                    break;
                default:
                    break;
            }
        }


        public List<Filters> SetFilterForItemID(string cluster, string pm_ItemID)
        {
            List<Filters> tmpFilters = new List<Filters>();
            List<Filters> FilterList = new List<Filters>();

            switch (cluster)
            {
                case "OS":
                    tmpFilters = GlobalVariables.MySession.List_ArchiveBuy_OSFilters;
                    GlobalVariables.MySession.List_ArchiveBuy_OSFilters = null;
                    FilterList = getFilters(cluster, "BuyArchive").Where(i => i.key == "ItemID").Select(i => { i.op = "="; i.value_s = pm_ItemID; return i; }).ToList();
                    GlobalVariables.MySession.List_ArchiveBuy_OSFilters = tmpFilters;
                    break;
                case "Domestic":
                    tmpFilters = GlobalVariables.MySession.List_ArchiveBuy_DomesticFilters;
                    GlobalVariables.MySession.List_ArchiveBuy_DomesticFilters = null;
                    FilterList = getFilters(cluster, "BuyArchive").Where(i => i.key == "ItemID").Select(i => { i.op = "="; i.value_s = pm_ItemID; return i; }).ToList();
                    GlobalVariables.MySession.List_ArchiveBuy_DomesticFilters = tmpFilters;
                    break;
                case "FTB":
                    tmpFilters = GlobalVariables.MySession.List_ArchiveBuy_FTBFilters;
                    GlobalVariables.MySession.List_ArchiveBuy_FTBFilters = null;
                    FilterList = getFilters(cluster, "BuyArchive").Where(i => i.key == "ItemID").Select(i => { i.op = "="; i.value_s = pm_ItemID; return i; }).ToList();
                    GlobalVariables.MySession.List_ArchiveBuy_FTBFilters = tmpFilters;
                    break;
                default:
                    break;
            }

            return FilterList;
        }

    }
}
