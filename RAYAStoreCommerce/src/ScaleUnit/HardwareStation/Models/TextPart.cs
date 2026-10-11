using System;
using System.Collections.Generic;
using System.Text;

namespace RR.HardwareStation.Models
{
    //
    // Summary:
    //     Indicates the type of text present on the printed receipt. Used by the Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.TextLogoParser
    //     to break the page into regions of separate content for printing.
    public enum TextType
    {
        //
        // Summary:
        //     Text is sent directly to the printer for printing.
        Text,
        //
        // Summary:
        //     Logo with bytes contains embedded base-64 image data, in the format <L:base64data>.
        LogoWithBytes,
        //
        // Summary:
        //     Legacy format for receipt, using the <L> tag. Will load a default image from
        //     disk where ever this tag is embedded.
        LegacyLogo,
        //
        // Summary:
        //     Image with bytes contains embedded base-64 image data, in the format <I:base64data>.
        Image
    }


    //
    // Summary:
    //     Used by the Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.TextLogoParser
    //     to represent a region of content for printing.
    public class TextPart
    {
        //
        // Summary:
        //     Gets or sets the type of content contained in this text part.
        public TextType TextType
        {
            get;
            set;
        }

        //
        // Summary:
        //     Gets or sets the value of the content for this text part.
        public string Value
        {
            get;
            set;
        }

        //
        // Summary:
        //     Initializes a new instance of the Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.TextPart
        //     class.
        //
        // Parameters:
        //   type:
        //     Type of content contained by this text part.
        //
        //   value:
        //     Value of the content for this text part.
        public TextPart(TextType type, string value)
        {
            TextType = type;
            Value = value;
        }

        //
        // Summary:
        //     Converts this text part into a string representation.
        //
        // Returns:
        //     Text showing the type and value in a single string.
        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", TextType.ToString(), Value);
        }
    }
}
