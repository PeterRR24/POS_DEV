using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using RR.HardwareStation.Models;

namespace RR.HardwareStation
{
    //
    // Summary:
    //     Parses a receipt into separate regions of content for printing.
    public sealed class TextLogoParser
    {
        private const string LegacyLogoGroup = "legacyLogo";

        private const string LogoGroup = "logo";

        private const string LogoRegex = "(?<logo>\\<L\\:[A-Za-z0-9\\+\\/\\=]*?\\>)|(?<legacyLogo>\\<L\\>)";

        private const string TagStart = "<L:";

        private const string TagEnd = ">";

        //
        // Summary:
        //     Initializes a new instance of the Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.TextLogoParser
        //     class.
        private TextLogoParser()
        {
        }

        //
        // Summary:
        //     Get the base64 encoded images bytes from a tag with <L:base64>.
        //
        // Parameters:
        //   logoText:
        //     Logo tag containing Base64-encoded image data.
        //
        // Returns:
        //     Bytes extracted from the logo image data.
        public static byte[] GetLogoImageBytes(string logoText)
        {
            if (!string.IsNullOrWhiteSpace(logoText) && logoText.StartsWith("<L:", StringComparison.Ordinal) && logoText.EndsWith(">", StringComparison.Ordinal))
            {
                string s = logoText.Substring("<L:".Length, logoText.Length - "<L:".Length - ">".Length);
                return Convert.FromBase64String(s);
            }

            return Array.Empty<byte>();
        }

        //
        // Summary:
        //     Splits a string into parts that are text, and parts that are embedded logos of
        //     the form <L:Base64>.
        //
        // Parameters:
        //   text:
        //     Text to parse.
        //
        // Returns:
        //     List of either text or logo strings.
        public static System.Collections.ObjectModel.ReadOnlyCollection<Models.TextPart> Parse(string text)
        {
            return TextParser.Parse(text);
        }

        //
        // Summary:
        //     Get logo images from a receipt text.
        //
        // Parameters:
        //   text:
        //     A receipt text.
        //
        // Returns:
        //     Tax part objects with mapped regular expression groups.
        internal static List<TextRegexGroupPart> GetLogos(string text)
        {
            MatchCollection source = Regex.Matches(text, "(?<logo>\\<L\\:[A-Za-z0-9\\+\\/\\=]*?\\>)|(?<legacyLogo>\\<L\\>)", RegexOptions.Compiled);
            IEnumerable<Match> source2 = Enumerable.Where(Enumerable.Cast<Match>(source), (Match m) => m.Success && m.Groups != null);
#pragma warning disable CA1851 // Possible multiple enumerations of 'IEnumerable' collection
            IEnumerable<Group> first = Enumerable.Select(source2, (Match m) => m.Groups["logo"]);
#pragma warning restore CA1851 // Possible multiple enumerations of 'IEnumerable' collection
#pragma warning disable CA1851 // Possible multiple enumerations of 'IEnumerable' collection
            IEnumerable<Group> second = Enumerable.Select(source2, (Match m) => m.Groups["legacyLogo"]);
#pragma warning restore CA1851 // Possible multiple enumerations of 'IEnumerable' collection
            return Enumerable.ToList(Enumerable.Select(Enumerable.Where(Enumerable.Concat(first, second), (Group g) => g?.Success ?? false), (Group g) => GetTextRegexGroupPart(g)));
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
            Models.TextPart textPart = new TextPart((!(group.Value == "<L>")) ? TextType.LogoWithBytes : TextType.LegacyLogo, group.Value);
            return new TextRegexGroupPart(group, textPart);
        }
    }
}
