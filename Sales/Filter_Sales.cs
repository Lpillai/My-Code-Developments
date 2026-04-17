using RuntimeVariables;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sales
{
    public class Sales_Fileter
    {
        //Get_Share shareGet = new Get_Share();
        //ModelGetter getModel = new ModelGetter();

        /*
        public List<SO_open_order> parseFilters_Int(List<SO_open_order> pmList_SO, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(SO_open_order), "w");
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
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmList_SO;
        }


        public List<SO_open_order> parseFilters_DateTime(List<SO_open_order> pmList_SO, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(SO_open_order), "w");
            var property = Expression.Property(parameterExpression, pmFilter.key);

            DateTime? pmConstant_s = null;
            if (string.IsNullOrEmpty(pmFilter.value_s))
                return pmList_SO;
            pmConstant_s = Convert.ToDateTime(pmFilter.value_s);

            DateTime? pmConstant_e = null;
            if (!string.IsNullOrEmpty(pmFilter.value_e))
                pmConstant_e = Convert.ToDateTime(pmFilter.value_e);

            switch (pmFilter.op)
            {
                case ">":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmList_SO;
        }


        public List<SO_open_order> parseFilters_Decimal(List<SO_open_order> pmList_SO, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(SO_open_order), "w");
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
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmList_SO = pmList_SO.Where((Expression.Lambda<Func<SO_open_order, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmList_SO;
        }


        public List<SO_open_order> parseFilters_String(List<SO_open_order> pmList_SO, Filters pmFilter)
        {
            List<SO_open_order> result = new List<SO_open_order>();
            List<SO_open_order> rejectResult = new List<SO_open_order>();
            string pmConstant_s = pmFilter.value_s == null ? "" : pmFilter.value_s;
            //var getter = shareGet.GetPropertyGetter(typeof(SO_open_order).ToString(), pmFilter.key);
            var getter = getModel.GetPropertyGetter(typeof(SO_open_order).ToString(), pmFilter.key);

            switch (pmFilter.op)
            {
                case ">=":
                    result = pmList_SO.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).StartsWith(pmConstant_s)).ToList();
                    break;
                case "<=":
                    result = pmList_SO.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).EndsWith(pmConstant_s)).ToList();
                    break;
                case "%":
                    result = pmList_SO.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).ToLower().Contains(pmConstant_s.ToLower())).ToList();
                    break;
                case "!%":
                    rejectResult = pmList_SO.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).ToLower().Contains(pmConstant_s.ToLower())).ToList();
                    result = pmList_SO.Except(rejectResult).ToList();
                    break;
                case "=":
                    result = pmList_SO.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).Equals(pmConstant_s)).ToList();
                    break;
                case "!=":
                    result = pmList_SO.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)) != pmConstant_s).ToList();
                    break;
                default:
                    result = pmList_SO;
                    break;
            }

            return result;
        }
        */

        public List<Filters> getFilters()
        {
            List<Filters> FilterList = new List<Filters>();
            if (GlobalVariables.MySession.List_SO_OpenOrder_Filters != null)
                FilterList = GlobalVariables.MySession.List_SO_OpenOrder_Filters;

            if (FilterList == null || FilterList.Count() <= 0)
            {
                FilterList.Add(new Filters() { key = "location_id", key_name = "Location ID", op = "", value_type = "number", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "ship2_id", key_name = "Ship To ID", op = "", value_type = "number", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "requested_date", key_name = "Requested Date", op = "", value_type = "date", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "item_id", key_name = "Item ID", op = "", value_type = "text", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "customer_part_number", key_name = "Customer Part Number", op = "", value_type = "text", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "release_date", key_name = "Release Date", op = "", value_type = "date", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "disposition", key_name = "Disp", op = "", value_type = "text", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "open_qty", key_name = "Open Qty", op = "", value_type = "number", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "line_feed", key_name = "Line Feed", op = "", value_type = "text", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "line_station", key_name = "Line Station", op = "", value_type = "text", value_s = "", value_e = "" });
            }

            return FilterList;
        }


        public void PostFilter(List<Filters> pmFilterList)
        {
            GlobalVariables.MySession.List_SO_OpenOrder_Filters = null;
            if (pmFilterList != null)
                GlobalVariables.MySession.List_SO_OpenOrder_Filters = pmFilterList.ToList();
        }

        /*
        public List<Filters> SetFilterForItemID(string pm_ItemID)
        {
            List<Filters> tmpFilters = GlobalVariables.MySession.List_SO_OpenOrder_Filters;
            GlobalVariables.MySession.List_SO_OpenOrder_Filters = null;

            List<Filters> FilterList = getFilters().Where(i => i.key == "ItemID").Select(i => { i.op = "="; i.value_s = pm_ItemID; return i; }).ToList();
            GlobalVariables.MySession.List_SO_OpenOrder_Filters = tmpFilters;

            return FilterList;
        }
        */
    }
}
