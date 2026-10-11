using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RR.HardwareStation.Models
{
    //
    // Summary:
    //     Used by the Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.TextParser
    //     to represent a matched region of content for printing.
    internal sealed class TextRegexGroupPart
    {
        //
        // Summary:
        //     Gets regular expression group matched a text part.
        public Group Group
        {
            get;
            private set;
        }

        //
        // Summary:
        //     Gets a matched text part.
        public TextPart TextPart
        {
            get;
            private set;
        }

        //
        // Summary:
        //     Initializes a new instance of the Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.TextRegexGroupPart
        //     class.
        //
        // Parameters:
        //   group:
        //     A regular expression group matched a text part.
        //
        //   textPart:
        //     A matched text part.
        public TextRegexGroupPart(Group group, TextPart textPart)
        {
            Group = group;
            TextPart = textPart;
        }
    }
}
