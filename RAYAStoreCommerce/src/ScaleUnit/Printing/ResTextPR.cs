using System;
using System.Text;
using System.Resources;
using System.Globalization;
using System.Reflection;

namespace RR.Commerce.Runtime.Printing
{
    public static class ResTextPR
    {
        private static ResourceManager manager;
        private static CultureInfo culture = null;

        public static ResourceManager Manager
        {
            get
            {
                if (object.ReferenceEquals(manager, null))
                    manager = new ResourceManager("RR.Commerce.Runtime.Printing.ResTextPR", typeof(ResTextPR).GetTypeInfo().Assembly);
                return manager;
            }
        }

        public static CultureInfo Culture
        {
            get { return culture; }
            set { culture = value; }
        }

        public static string Text(string key)
        {
            return Manager.GetString(key);
        }

        public static string Text(string key, params object[] par)
        {
            return String.Format(Culture, Manager.GetString(key), par);
        }
    }
}
