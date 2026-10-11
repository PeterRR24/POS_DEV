using System;
using System.IO;
using System.Drawing;

namespace RR.Commerce.Runtime.Reporting
{
    public static class ObjectEncoder
    {
        // Image
        public static byte[] EncodeImage(Image value)
        {
            if (value != null)
            {
                using (MemoryStream buffer = new MemoryStream())
                {
                    value.Save(buffer, value.RawFormat);
                    return buffer.ToArray();
                }
            }
            return null;
        }

        public static Image DecodeImage(byte[] value)
        {
            if (value != null)
            {
                using (MemoryStream buffer = new MemoryStream(value))
                {
                    return Image.FromStream(buffer);
                }
            }
            return null;
        }


        public static Image TryDecodeImage(byte[] value)
        {
            try
            {
                return DecodeImage(value);
            }
            catch
            {
                return null;
            }
        }

        public static Image DecodeImageAX(byte[] value)
        {
            if (value != null)
            {
                using (MemoryStream buffer = new MemoryStream(value))
                {
                    using (Bitmap bitmap = new Bitmap(buffer))
                    {
                        return new Bitmap(bitmap);
                    }
                }
            }
            return null;
        }

        public static Image TryDecodeImageAX(byte[] value)
        {
            try
            {
                return DecodeImageAX(value);
            }
            catch
            {
                return null;
            }
        }
    }
}
