using RuntimeVariables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SharedScrpits
{
    public class Filters_Share
    {
        /*
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


        public List<T> parseFilters_String<T>(List<T> pmBuyList, Filters pmFilter)
        {
            List<T> result = new List<T>();
            List<T> rejectResult = new List<T>();
            string pmConstant_s = pmFilter.value_s ?? "";

            // Pass the type of T to the property getter helper
            var getter = GetPropertyGetter(typeof(T).ToString(), pmFilter.key);

            switch (pmFilter.op)
            {
                case ">=":
                    result = pmBuyList.Where(i => (getter(i)?.ToString() ?? "").StartsWith(pmConstant_s)).ToList();
                    break;
                case "<=":
                    result = pmBuyList.Where(i => (getter(i)?.ToString() ?? "").EndsWith(pmConstant_s)).ToList();
                    break;
                case "%":
                    result = pmBuyList.Where(i => (getter(i)?.ToString() ?? "").ToLower().Contains(pmConstant_s.ToLower())).ToList();
                    break;
                case "!%":
                    rejectResult = pmBuyList.Where(i => (getter(i)?.ToString() ?? "").ToLower().Contains(pmConstant_s.ToLower())).ToList();
                    result = pmBuyList.Except(rejectResult).ToList();
                    break;
                case "=":
                    result = pmBuyList.FindAll(i => (getter(i)?.ToString() ?? "").Equals(pmConstant_s));
                    break;
                case "!=":
                    result = pmBuyList.Where(i => (getter(i)?.ToString() ?? "") != pmConstant_s).ToList();
                    break;
                default:
                    result = pmBuyList;
                    break;
            }

            return result;
        }
        */
        public List<T> parseFilters_String<T>(List<T> pmBuyList, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(T), "w");
            var property = Expression.Property(parameterExpression, pmFilter.key);

            // We use a coalescing expression to ensure we are working with a string
            var nullCheckExpr = Expression.Coalesce(property, Expression.Constant(""));

            string pmConstant_s = pmFilter.value_s ?? "";
            var constantExpr = Expression.Constant(pmConstant_s);

            Expression binaryBody;

            switch (pmFilter.op)
            {
                case ">=": // StartsWith
                    binaryBody = Expression.Call(nullCheckExpr, typeof(string).GetMethod("StartsWith", new[] { typeof(string) }), constantExpr);
                    break;

                case "<=": // EndsWith
                    binaryBody = Expression.Call(nullCheckExpr, typeof(string).GetMethod("EndsWith", new[] { typeof(string) }), constantExpr);
                    break;

                case "%": // Contains (Case Insensitive)
                    var toLowerExpr = Expression.Call(nullCheckExpr, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
                    var lowerConstant = Expression.Constant(pmConstant_s.ToLower());
                    binaryBody = Expression.Call(toLowerExpr, typeof(string).GetMethod("Contains", new[] { typeof(string) }), lowerConstant);
                    break;

                case "!%": // Not Contains (Case Insensitive)
                    var toLowerExprNot = Expression.Call(nullCheckExpr, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
                    var lowerConstantNot = Expression.Constant(pmConstant_s.ToLower());
                    var containsExpr = Expression.Call(toLowerExprNot, typeof(string).GetMethod("Contains", new[] { typeof(string) }), lowerConstantNot);
                    binaryBody = Expression.Not(containsExpr);
                    break;

                case "=": // Equals
                    binaryBody = Expression.Call(nullCheckExpr, typeof(string).GetMethod("Equals", new[] { typeof(string) }), constantExpr);
                    break;

                case "!=": // Not Equal
                    binaryBody = Expression.NotEqual(nullCheckExpr, constantExpr);
                    break;

                default:
                    return pmBuyList;
            }

            var lambda = Expression.Lambda<Func<T, bool>>(binaryBody, parameterExpression);
            return pmBuyList.Where(lambda.Compile()).ToList();
        }


        public List<T> parseFilters_Int<T>(List<T> pmBuyList, Filters pmFilter)
        {
            // Use typeof(T) to create the expression parameter dynamically
            var parameterExpression = Expression.Parameter(typeof(T), "w");
            var property = Expression.Property(parameterExpression, pmFilter.key);

            int pmConstant_s = 0;
            if (!string.IsNullOrEmpty(pmFilter.value_s))
                pmConstant_s = Int32.Parse(pmFilter.value_s);

            int pmConstant_e = 0;
            if (!string.IsNullOrEmpty(pmFilter.value_e))
                pmConstant_e = Int32.Parse(pmFilter.value_e);

            Expression binaryBody;

            switch (pmFilter.op)
            {
                case ">":
                    binaryBody = Expression.GreaterThan(property, Expression.Constant(pmConstant_s));
                    break;
                case ">=":
                    binaryBody = Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s));
                    break;
                case "<":
                    binaryBody = Expression.LessThan(property, Expression.Constant(pmConstant_s));
                    break;
                case "<=":
                    binaryBody = Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s));
                    break;
                case "!=":
                    binaryBody = Expression.NotEqual(property, Expression.Constant(pmConstant_s));
                    break;
                case "~":
                    // Range logic: (Property >= s) AND (Property <= e)
                    var startExpr = Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s));
                    var endExpr = Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e));
                    binaryBody = Expression.AndAlso(startExpr, endExpr);
                    break;
                default:
                    binaryBody = Expression.Equal(property, Expression.Constant(pmConstant_s));
                    break;
            }

            var lambda = Expression.Lambda<Func<T, bool>>(binaryBody, parameterExpression);
            return pmBuyList.Where(lambda.Compile()).ToList();
        }


        public List<T> parseFilters_Decimal<T>(List<T> pmBuyList, Filters pmFilter)
        {
            // Use typeof(T) to make the expression parameter dynamic
            var parameterExpression = Expression.Parameter(typeof(T), "w");
            var property = Expression.Property(parameterExpression, pmFilter.key);

            decimal pmConstant_s = 0;
            if (!string.IsNullOrEmpty(pmFilter.value_s))
                pmConstant_s = Decimal.Parse(pmFilter.value_s);

            decimal pmConstant_e = 0;
            if (!string.IsNullOrEmpty(pmFilter.value_e))
                pmConstant_e = Decimal.Parse(pmFilter.value_e);

            // Helper to build the lambda: Expression.Lambda<Func<T, bool>>
            Func<Expression, Expression, BinaryExpression> operation;

            switch (pmFilter.op)
            {
                case ">":
                    operation = Expression.GreaterThan;
                    break;
                case ">=":
                    operation = Expression.GreaterThanOrEqual;
                    break;
                case "<":
                    operation = Expression.LessThan;
                    break;
                case "<=":
                    operation = Expression.LessThanOrEqual;
                    break;
                case "!=":
                    operation = Expression.NotEqual;
                    break;
                case "~":
                    // Special case for Range: Property >= Start AND Property <= End
                    var left = Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s));
                    var right = Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e));
                    var rangeExpr = Expression.AndAlso(left, right);
                    return pmBuyList.Where(Expression.Lambda<Func<T, bool>>(rangeExpr, parameterExpression).Compile()).ToList();
                default:
                    operation = Expression.Equal;
                    break;
            }

            // Compile and execute the expression
            var body = operation(property, Expression.Constant(pmConstant_s));
            var lambda = Expression.Lambda<Func<T, bool>>(body, parameterExpression);

            return pmBuyList.Where(lambda.Compile()).ToList();
        }


        public List<T> parseFilters_DateTime<T>(List<T> pmBuyList, Filters pmFilter)
        {
            // Use typeof(T) for dynamic object mapping
            var parameterExpression = Expression.Parameter(typeof(T), "w");
            var property = Expression.Property(parameterExpression, pmFilter.key);

            if (string.IsNullOrEmpty(pmFilter.value_s))
                return pmBuyList;

            DateTime pmConstant_s = Convert.ToDateTime(pmFilter.value_s);

            // Create a constant expression for the filter value
            // Note: We convert the property to match the constant type to prevent crashing on Nullable types
            var propExpression = Expression.Convert(property, typeof(DateTime));

            Expression binaryBody;

            switch (pmFilter.op)
            {
                case ">":
                    binaryBody = Expression.GreaterThan(propExpression, Expression.Constant(pmConstant_s));
                    break;
                case ">=":
                    binaryBody = Expression.GreaterThanOrEqual(propExpression, Expression.Constant(pmConstant_s));
                    break;
                case "<":
                    binaryBody = Expression.LessThan(propExpression, Expression.Constant(pmConstant_s));
                    break;
                case "<=":
                    binaryBody = Expression.LessThanOrEqual(propExpression, Expression.Constant(pmConstant_s));
                    break;
                case "!=":
                    binaryBody = Expression.NotEqual(propExpression, Expression.Constant(pmConstant_s));
                    break;
                case "~":
                    if (string.IsNullOrEmpty(pmFilter.value_e)) return pmBuyList;
                    DateTime pmConstant_e = Convert.ToDateTime(pmFilter.value_e);

                    var startExpr = Expression.GreaterThanOrEqual(propExpression, Expression.Constant(pmConstant_s));
                    var endExpr = Expression.LessThanOrEqual(propExpression, Expression.Constant(pmConstant_e));
                    binaryBody = Expression.AndAlso(startExpr, endExpr);
                    break;
                default:
                    binaryBody = Expression.Equal(propExpression, Expression.Constant(pmConstant_s));
                    break;
            }

            var lambda = Expression.Lambda<Func<T, bool>>(binaryBody, parameterExpression);
            return pmBuyList.Where(lambda.Compile()).ToList();
        }

    }
}
