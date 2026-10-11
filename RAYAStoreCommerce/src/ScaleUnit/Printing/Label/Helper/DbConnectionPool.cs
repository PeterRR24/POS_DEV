
namespace RR.Commerce.Runtime.Printing
{
    using System;
    using System.Reflection;

    using Microsoft.Dynamics.Commerce.Runtime;

    internal class DbConnectionPool
    {
        private const string PoolSizeFieldName = "connectionpoolsize";
        private const int poolSizeFieldValue = 2000;

        public static void SetSize(RequestContext context)
        {
            var dbProvider = context.Runtime.Configuration.Composition.DatabaseProvider;
            var numObj = GetPrivatePropertyValue(dbProvider, PoolSizeFieldName);
            if (numObj != null && Convert.ToInt32(numObj) != poolSizeFieldValue)
            {
                context.SetProperty("DBPool", numObj);
                SetPrivatePropertyValue(dbProvider, PoolSizeFieldName, poolSizeFieldValue);
            }
        }

        public static void ReSetSize(RequestContext context)
        {
            var defaultPoolSize = Convert.ToInt32(context.GetProperty("DBPool"));

            if (defaultPoolSize == 0)
                return;
            
            var dbProvider = context.Runtime.Configuration.Composition.DatabaseProvider;
            var numObj = GetPrivatePropertyValue(dbProvider, PoolSizeFieldName);
            if (numObj != null && Convert.ToInt32(numObj) == poolSizeFieldValue)
                SetPrivatePropertyValue(dbProvider, PoolSizeFieldName, defaultPoolSize);
        }

        private static object GetPrivatePropertyValue<T>(T obj, string propertyName)
        {
            object retVal = null;
            if (obj == null)
                return retVal;
            foreach (FieldInfo fi in obj.GetType().GetRuntimeFields())
            {
                if (fi.Name.ToLower().Contains(propertyName.ToLower()))
                {
                    retVal = fi.GetValue(obj);
                    break;
                }
            }
            return retVal;
        }

        private static void SetPrivatePropertyValue<T>(T obj, string propertyName, object newValue)
        {
            if (obj == null) return;
            foreach (FieldInfo fi in obj.GetType().GetRuntimeFields())
            {
                if (fi.Name.ToLower().Contains(propertyName.ToLower()))
                {
                    fi.SetValue(obj, newValue);
                    break;
                }
            }
        }
    }
}
