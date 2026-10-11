using System;

namespace RR.Commerce.Runtime.Utilities
{
    public static class DataReaderHelper
    {
        public static DateTime? GetDateTime(object value)
        {
            if (value == null)
                return null;
            DateTime date = (DateTime)value;
            bool defaultDate = date.Year == 1900 && date.Month == 1 && date.Day == 1;
            if (defaultDate)
                return null;
            else
                return date;
        }
    }
}
