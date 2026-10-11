using System;
using System.Text;

namespace RR.Commerce.Runtime.Utilities
{
    public static class BitEncoder
    {
        public static bool Get(int dat, byte bit)
        {
            if (bit < 0 || bit >= 32) return false;
            return (dat & (1 << bit)) != 0;
        }

        public static int Set(int dat, byte bit, bool val)
        {
            if (bit < 0 || bit >= 32) return dat;
            if (val)
                return dat | (1 << bit);
            else
                return dat & ~(1 << bit);
        }
    }
}
