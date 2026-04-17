using RuntimeVariables;
using RuntimeConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Accounting
{
    public class Accounting_Fileter
    {
        //Get_Share shareGet = new Get_Share();
        //ModelGetter getModel = new ModelGetter();


        #region A/R
        /*
        public List<AR_Item> AR_parseFilters_Int(List<AR_Item> pmList_AR, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(AR_Item), "w");
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
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmList_AR;
        }


        public List<AR_Item> AR_parseFilters_DateTime(List<AR_Item> pmList_AR, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(AR_Item), "w");
            var property = Expression.Property(parameterExpression, pmFilter.key);

            DateTime? pmConstant_s = null;
            if (string.IsNullOrEmpty(pmFilter.value_s))
                return pmList_AR;
            pmConstant_s = Convert.ToDateTime(pmFilter.value_s);

            DateTime? pmConstant_e = null;
            if (!string.IsNullOrEmpty(pmFilter.value_e))
                pmConstant_e = Convert.ToDateTime(pmFilter.value_e);

            switch (pmFilter.op)
            {
                case ">":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmList_AR;
        }


        public List<AR_Item> AR_parseFilters_Decimal(List<AR_Item> pmList_AR, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(AR_Item), "w");
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
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmList_AR = pmList_AR.Where((Expression.Lambda<Func<AR_Item, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmList_AR;
        }


        public List<AR_Item> AR_parseFilters_String(List<AR_Item> pmList_AR, Filters pmFilter)
        {
            List<AR_Item> result = new List<AR_Item>();
            List<AR_Item> rejectResult = new List<AR_Item>();
            string pmConstant_s = pmFilter.value_s == null ? "" : pmFilter.value_s;
            //var getter = shareGet.GetPropertyGetter(typeof(AR_Item).ToString(), pmFilter.key);
            var getter = getModel.GetPropertyGetter(typeof(AR_Item).ToString(), pmFilter.key);

            switch (pmFilter.op)
            {
                case ">=":
                    result = pmList_AR.Where(i => (getter(i) as string).StartsWith(pmConstant_s)).ToList();
                    break;
                case "<=":
                    result = pmList_AR.Where(i => (getter(i) as string).EndsWith(pmConstant_s)).ToList();
                    break;
                case "%":
                    result = pmList_AR.Where(i => (getter(i) as string).ToLower().Contains(pmConstant_s.ToLower())).ToList();
                    break;
                case "!%":
                    rejectResult = pmList_AR.Where(i => (getter(i) as string).ToLower().Contains(pmConstant_s.ToLower())).ToList();
                    result = pmList_AR.Except(rejectResult).ToList();
                    break;
                case "=":
                    result = pmList_AR.Where(i => (getter(i) as string).Equals(pmConstant_s)).ToList();
                    break;
                case "!=":
                    result = pmList_AR.Where(i => (getter(i) as string) != pmConstant_s).ToList();
                    break;
                default:
                    result = pmList_AR;
                    break;
            }

            return result;
        }
        */

        public List<Filters> AR_getFilters()
        {
            List<Filters> FilterList = new List<Filters>();
            if (GlobalVariables.MySession.List_AccountsReceivable_Filters != null)
                FilterList = GlobalVariables.MySession.List_AccountsReceivable_Filters;

            if (FilterList == null || FilterList.Count() <= 0)
            {
                FilterList.Add(new Filters() { key = "invoice_period", key_name = "Invoice Period", op = "", value_type = "number", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "invoice_date", key_name = "Invoice Date", op = "", value_type = "date", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "location_id", key_name = "Location ID", op = "", value_type = "number", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "taker", key_name = "Taker", op = "", value_type = "text", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "customer_id", key_name = "Customer ID", op = "", value_type = "number", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "currency_id", key_name = "Currency ID", op = "", value_type = "number", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "default_paid_date", key_name = "Default Paid Date", op = "", value_type = "date", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "gui", key_name = "GUI", op = "", value_type = "text", value_s = "", value_e = "" });
            }

            return FilterList;
        }


        public void AR_PostFilter(List<Filters> pmFilterList)
        {
            GlobalVariables.MySession.List_AccountsReceivable_Filters = null;
            if (pmFilterList != null)
                GlobalVariables.MySession.List_AccountsReceivable_Filters = pmFilterList.ToList();
        }

        #endregion

        #region A/P
        /*
        public List<AP_Item> AP_parseFilters_Int(List<AP_Item> pmList_AP, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(AP_Item), "w");
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
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmList_AP;
        }


        public List<AP_Item> AP_parseFilters_DateTime(List<AP_Item> pmList_AP, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(AP_Item), "w");
            var property = Expression.Property(parameterExpression, pmFilter.key);

            DateTime? pmConstant_s = null;
            if (string.IsNullOrEmpty(pmFilter.value_s))
                return pmList_AP;
            pmConstant_s = Convert.ToDateTime(pmFilter.value_s);

            DateTime? pmConstant_e = null;
            if (!string.IsNullOrEmpty(pmFilter.value_e))
                pmConstant_e = Convert.ToDateTime(pmFilter.value_e);

            switch (pmFilter.op)
            {
                case ">":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmList_AP;
        }


        public List<AP_Item> AP_parseFilters_Decimal(List<AP_Item> pmList_AP, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(AP_Item), "w");
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
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmList_AP = pmList_AP.Where((Expression.Lambda<Func<AP_Item, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmList_AP;
        }


        public List<AP_Item> AP_parseFilters_String(List<AP_Item> pmList_AP, Filters pmFilter)
        {
            List<AP_Item> result = new List<AP_Item>();
            List<AP_Item> rejectResult = new List<AP_Item>();
            string pmConstant_s = pmFilter.value_s == null ? "" : pmFilter.value_s;
            //var getter = shareGet.GetPropertyGetter(typeof(AR_Item).ToString(), pmFilter.key);
            var getter = getModel.GetPropertyGetter(typeof(AR_Item).ToString(), pmFilter.key);

            switch (pmFilter.op)
            {
                case ">=":
                    result = pmList_AP.Where(i => (getter(i) as string).StartsWith(pmConstant_s)).ToList();
                    break;
                case "<=":
                    result = pmList_AP.Where(i => (getter(i) as string).EndsWith(pmConstant_s)).ToList();
                    break;
                case "%":
                    result = pmList_AP.Where(i => (getter(i) as string).ToLower().Contains(pmConstant_s.ToLower())).ToList();
                    break;
                case "!%":
                    rejectResult = pmList_AP.Where(i => (getter(i) as string).ToLower().Contains(pmConstant_s.ToLower())).ToList();
                    result = pmList_AP.Except(rejectResult).ToList();
                    break;
                case "=":
                    result = pmList_AP.Where(i => (getter(i) as string).Equals(pmConstant_s)).ToList();
                    break;
                case "!=":
                    result = pmList_AP.Where(i => (getter(i) as string) != pmConstant_s).ToList();
                    break;
                default:
                    result = pmList_AP;
                    break;
            }

            return result;
        }
        */

        public List<Filters> AP_getFilters()
        {
            List<Filters> FilterList = new List<Filters>();
            if (GlobalVariables.MySession.List_AccountsPayable_Filters != null)
                FilterList = GlobalVariables.MySession.List_AccountsPayable_Filters;

            if (FilterList == null || FilterList.Count() <= 0)
            {
                FilterList.Add(new Filters() { key = "invoice_period", key_name = "Invoice Period", op = "", value_type = "number", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "invoice_date", key_name = "Invoice Date", op = "", value_type = "date", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "receipt_number", key_name = "Receipt No", op = "", value_type = "number", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "po_number", key_name = "PO No", op = "", value_type = "number", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "supplier_id", key_name = "Customer ID", op = "", value_type = "number", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "currency_id", key_name = "Currency ID", op = "", value_type = "number", value_s = "", value_e = "" });
                //FilterList.Add(new Filters() { key = "default_paid_date", key_name = "Default Paid Date", op = "", value_type = "date", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "gui", key_name = "GUI", op = "", value_type = "text", value_s = "", value_e = "" });
            }

            return FilterList;
        }


        public void AP_PostFilter(List<Filters> pmFilterList)
        {
            GlobalVariables.MySession.List_AccountsPayable_Filters = null;
            if (pmFilterList != null)
                GlobalVariables.MySession.List_AccountsPayable_Filters = pmFilterList.ToList();
        }

        #endregion
    }
}
