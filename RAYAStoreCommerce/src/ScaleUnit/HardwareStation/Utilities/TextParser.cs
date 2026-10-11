using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RR.HardwareStation.Models;
using System.Linq;
namespace RR.HardwareStation
{
    //
    // Summary:
    //     Parses a text into separate regions of content for printing.
    public static class TextParser
    {
        //
        // Summary:
        //     The barcode regular expression.
        public const string BarcodeRegex = "<B: (.+?)>";

        //
        // Summary:
        //     Splits a string into parts that are text, and parts that are embedded special
        //     tags like images and logos.
        //
        // Parameters:
        //   text:
        //     Text to parse.
        //
        // Returns:
        //     List of either text or logo strings.
        public static ReadOnlyCollection<TextPart> Parse(string text)
        {
            List<TextPart> list = new List<TextPart>();
            if (string.IsNullOrEmpty(text))
            {
                list.Add(new TextPart(TextType.Text, text));
                return list.AsReadOnly();
            }

            List<TextRegexGroupPart> list2 = new List<TextRegexGroupPart>();
            list2.AddRange(TextLogoParser.GetLogos(text));
            list2.AddRange(TextImageParser.GetImages(text));
            int num = 0;
            foreach (TextRegexGroupPart item in Enumerable.OrderBy(list2, (TextRegexGroupPart tag) => tag.Group.Index))
            {
                if (item.Group.Index > num)
                {
                    list.Add(new TextPart(Models.TextType.Text, text.Substring(num, item.Group.Index - num)));
                }

                list.Add(item.TextPart);
                num = item.Group.Index + item.Group.Length;
            }

            if (num < text.Length)
            {
                list.Add(new TextPart(TextType.Text, text.Substring(num, text.Length - num)));
            }

            return list.AsReadOnly();
        }

        //
        // Summary:
        //     Extracts the base64 encoded logos/images bytes from a tag.
        //
        // Parameters:
        //   textPart:
        //     A text part containing Base64-encoded image data.
        //
        // Returns:
        //     Bytes extracted from the logo/image data.
        public static byte[] GetImageBytes(TextPart textPart)
        {
            byte[] array = null;
            switch (textPart.TextType)
            {
                case TextType.LogoWithBytes:
                    return TextLogoParser.GetLogoImageBytes(textPart.Value);
                case TextType.Image:
                    return TextImageParser.GetImageBytes(textPart.Value);
                default:
                    return Array.Empty<byte>();
            }
        }
    }
}
