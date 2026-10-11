using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RR.HardwareStation.Models;
using System.Linq;

namespace RR.HardwareStation
{
    //
    // Summary:
    //     Extracts images data from a receipt text.
    internal static class TextImageParser
    {
        private const string ImageGroup = "image";

        private const string ImageRegex = "(?<image><I:(.+?)>)";

        private const string TagStart = "<I:";

        private const string TagEnd = ">";

        //
        // Summary:
        //     Get the base64 encoded images bytes from a tag with <I:base64>.
        //
        // Parameters:
        //   imageText:
        //     Image tag containing Base64-encoded image data.
        //
        // Returns:
        //     Bytes extracted from the image data.
        public static byte[] GetImageBytes(string imageText)
        {
            if (!string.IsNullOrWhiteSpace(imageText) && imageText.StartsWith("<I:", StringComparison.Ordinal) && imageText.EndsWith(">", StringComparison.Ordinal))
            {
                string s = imageText.Substring("<I:".Length, imageText.Length - "<I:".Length - ">".Length);
                return Convert.FromBase64String(s);
            }

            return Array.Empty<byte>();
        }

        //
        // Summary:
        //     Get an images from a receipt text.
        //
        // Parameters:
        //   text:
        //     A receipt text.
        //
        // Returns:
        //     Tax part objects with mapped regular expression groups.
        internal static IReadOnlyCollection<TextRegexGroupPart> GetImages(string text)
        {
            MatchCollection source = Regex.Matches(text, "(?<image><I:(.+?)>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return Enumerable.ToList(Enumerable.Select(Enumerable.Where(Enumerable.Select(Enumerable.Where(Enumerable.Cast<Match>(source), (Match m) => m.Success && m.Groups != null), (Match m) => m.Groups["image"]), (Group g) => g?.Success ?? false), (Group g) => GetTextRegexGroupPart(g)));
        }

        //
        // Summary:
        //     Get a text part object with mapped a regulared expression group.
        //
        // Parameters:
        //   group:
        //     Regular expression group.
        //
        // Returns:
        //     A text part object with mapped a regulared expression group.
        private static TextRegexGroupPart GetTextRegexGroupPart(Group group)
        {
            TextPart textPart = new TextPart(TextType.Image, group.Value);
            return new TextRegexGroupPart(group, textPart);
        }
    }
}
