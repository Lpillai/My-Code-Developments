using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeVariables
{
    public class ModelGetter
    {
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
    }
}
