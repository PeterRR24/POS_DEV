/**
* SAMPLE CODE NOTICE
* 
* THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
* OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
* THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
* NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/
namespace Contoso.Commerce.HardwareStation.Extension.WindowsPrinterSample
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Printing;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Dynamics.Commerce.HardwareStation;
    using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
    using Microsoft.Dynamics.Commerce.Runtime.Handlers;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;




    /// <summary>
    /// Class implements windows based printer driver for hardware station.
    /// </summary>
    /// 
#pragma warning disable CS0618 // JUSTIFICATION: Pending migration to asynchronous APIs.
    public class CustomWindowsPrinterDevice //: INamedRequestHandler
#pragma warning restore CS0618
    {
        private const string BarCodeRegEx = "<B: (.+?)>";
        private const string NormalTextMarker = "|1C";
        private const string BoldTextMarker = "|2C";
        private const string DoubleSizeNormalTextMarker = "|3C";
        private const string DoubleSizeBoldTextMarker = "|4C";

        private const string ExceptNormalMarkersRegEx = @"\|2C|\|3C|\|4C";
        private const string AllMarkersForSplitRegEx = @"(\|1C|\|2C|\|3C|\|4C)";

        private const float defaultDpiScalingFactor = 96f;

        private bool header = true;

        private RR.HardwareStation.Models.Barcode barCode = new RR.HardwareStation.Models.BarcodeCode39();

        private List<RR.HardwareStation.Models.TextPart> parts;
        private int printLine;

        /// <summary>
        /// Gets the esc marker.
        /// </summary>
        public static string EscMarker
        {
            get
            {
                return "&#x1B;";
            }
        }

        /// <summary>
        /// Font used for receipt.
        /// </summary>
        public static string TextFontName
        {
            get
            {
                return "Segoe UI Semibold";
            }
        }

        /// <summary>
        /// Default font size for receipt.
        /// </summary>
        public static int TextFontSize
        {
            get
            {
                return 9;
            }
        }

        /// <summary>
        /// Large font size for receipt.
        /// </summary>
        public static int DoubleSizeTextFontSize
        {
            get
            {
                return TextFontSize * 2;
            }
        }

        /// <summary>
        /// Gets the unique name for this request handler.
        /// </summary>
        public string HandlerName
        {
            get { return PeripheralType.Windows; }
        }

        /// <summary>
        /// Gets the collection of supported request types by this handler.
        /// </summary>
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(OpenPrinterDeviceRequest),
                    typeof(PrintPrinterDeviceRequest),
                    typeof(ClosePrinterDeviceRequest)
                };
            }
        }

        /// <summary>
        /// Gets or sets the printer name.
        /// </summary>
        protected string PrinterName { get; set; }

        /// <summary>
        /// Represents the entry point for the printer device request handler.
        /// </summary>
        /// <param name="request">The incoming request message.</param>
        /// <returns>The outgoing response message.</returns>
        public Response Execute(Request request)
        {
            ThrowIf.Null(request, nameof(request));

            Type requestType = request.GetType();

            if (requestType == typeof(OpenPrinterDeviceRequest))
            {
                var openRequest = (OpenPrinterDeviceRequest)request;

                this.Open(openRequest.DeviceName);
            }
            else if (requestType == typeof(PrintPrinterDeviceRequest))
            {
                var printRequest = (PrintPrinterDeviceRequest)request;

                this.Print(
                    printRequest.Header,
                    printRequest.Lines,
                    printRequest.Footer);
            }
            else if (requestType == typeof(ClosePrinterDeviceRequest))
            {
                // Do nothing. Just for support of the close request type.
            }
            else
            {
                throw new NotSupportedException(string.Format("Request '{0}' is not supported.", requestType));
            }

            return NullResponse.Instance;
        }

        private static bool DrawBitmapImage(PrintPageEventArgs e, byte[] defaultLogoBytes, float contentWidth, ref float y)
        {
            if (defaultLogoBytes == null || !defaultLogoBytes.Any())
                return true;

            using (MemoryStream ms = new MemoryStream(defaultLogoBytes))
            {
                var image = Image.FromStream(ms);

                if (y + image.Height >= e.MarginBounds.Height)
                {
                    // No more room - advance to next page
                    e.HasMorePages = true;
                    return false;
                }

                float center = ((contentWidth - image.Width) / 2.0f) + e.MarginBounds.Left;
                if (center < 0)
                {
                    center = 0;
                }

                float top = e.MarginBounds.Top + y;

                e.Graphics.DrawImage(image, center, top);

                y += image.Height;

                return true;
            }
        }

        /// <summary>
        /// Opens a peripheral.
        /// </summary>
        /// <param name="peripheralName">Name of the peripheral.</param>
        private void Open(string peripheralName)
        {
            this.PrinterName = peripheralName;
        }

        /// <summary>
        /// Print the data on printer.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="lines">The lines.</param>
        /// <param name="footer">The footer.</param>
        private void Print(string header, string lines, string footer)
        {
            string textToPrint = header + lines + footer;
            this.header = true;
            if (!string.IsNullOrWhiteSpace(textToPrint))
            {
                using (PrintDocument printDoc = new PrintDocument())
                {
                    printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                    printDoc.PrinterSettings.PrinterName = this.PrinterName;
                    string subString = textToPrint.Replace(EscMarker, string.Empty);
                    var printText = subString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                    this.parts = new List<RR.HardwareStation.Models.TextPart>();
                    foreach (var line in printText)
                    {
                        var lineParts = RR.HardwareStation.TextLogoParser.Parse(line);
                        if (lineParts != null)
                        {
                            this.parts.AddRange(lineParts);
                        }
                    }

                    this.printLine = 0;

                    printDoc.OriginAtMargins = false;
                    printDoc.DefaultPageSettings.Margins.Left = 0;
                    printDoc.DefaultPageSettings.Margins.Right = 0;
                    printDoc.DefaultPageSettings.Margins.Top = 0;
                    printDoc.DefaultPageSettings.Margins.Bottom = 0;

                    printDoc.PrintPage += new PrintPageEventHandler(this.PrintPageHandler);

                    try
                    {
                        printDoc.Print();
                    }
                    catch (InvalidPrinterException)
                    {
                        throw new PeripheralException(PeripheralException.PeripheralNotFound);
                    }
                }
            }
        }

        /// <summary>
        /// Prints the page.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PrintPageEventArgs" /> instance containing the event data.</param>
        private void PrintPageHandler(object sender, PrintPageEventArgs e)
        {
            const int LineHeight = 14;

            e.HasMorePages = false;
            using (Font textFont = new Font(TextFontName, TextFontSize, FontStyle.Regular))
            using (Font textFontBold = new Font(TextFontName, TextFontSize, FontStyle.Bold))
            using (Font doubleSizeTextFont = new Font(TextFontName, DoubleSizeTextFontSize, FontStyle.Regular))
            using (Font doubleSizeTextFontBold = new Font(TextFontName, DoubleSizeTextFontSize, FontStyle.Bold))
            {
                float y = 0;
                float dpiXRatio = e.Graphics.DpiX / defaultDpiScalingFactor;
                float dpiYRatio = e.Graphics.DpiY / defaultDpiScalingFactor;

                // This calculation isn't exactly the width of the rendered text.
                // All the calculations occurring in the rendering code of PrintTextLine.  It almost needs to run that code and use e.Graphics.MeasureString()
                // the first time to get the true contentWidth, then re-run the same logic using the true contentWidth for rendering.
                //
                // The rendering is close, but it's not 'exact' center due to the mismatch in estimated vs. true size
                float contentWidth = this.parts.Where(x => x.TextType == RR.HardwareStation.Models.TextType.Text)
                                                .Select(p => p.Value)
                                                .Max(str => str.Replace(NormalTextMarker, string.Empty)
                                                                .Replace(BoldTextMarker, string.Empty)
                                                                .Replace(DoubleSizeNormalTextMarker, string.Empty)
                                                                .Replace(DoubleSizeBoldTextMarker, string.Empty)
                                                                .Length)
                                        * dpiXRatio; // Line with max length = content width

                for (; this.printLine < this.parts.Count; this.printLine++)
                {
                    var part = this.parts[this.printLine];

                    if (part.TextType == RR.HardwareStation.Models.TextType.Text)
                    {
                        if (!this.PrintTextLine(
                                                e,
                                                LineHeight,
                                                textFontBold,//Arafa STD textFont
                                                textFontBold,
                                                doubleSizeTextFont,
                                                doubleSizeTextFontBold,
                                                dpiYRatio,
                                                contentWidth,
                                                dpiXRatio,
                                                part.Value,
                                                ref y))
                        {
                            return;
                        }
                    }
                    else if (part.TextType == RR.HardwareStation.Models.TextType.LogoWithBytes
                        || part.TextType == RR.HardwareStation.Models.TextType.Image
                        || part.TextType == RR.HardwareStation.Models.TextType.LegacyLogo)
                    {
                        byte[] imageBytes = RR.HardwareStation.TextParser.GetImageBytes(part);
                        if (!DrawBitmapImage(e, imageBytes, contentWidth, ref y))
                        {
                            return;
                        }
                    }
                }
            }
        }

        private bool PrintTextLine(
                        PrintPageEventArgs e,
                        int lineHeight,
                        Font textFont,
                        Font textFontBold,
                        Font doubleSizeTextFont,
                        Font doubleSizeTextFontBold,
                        float dpiYRatio,
                        float contentWidth,
                        float dpiXRatio,
                        string line,
                        ref float y)
        {
            float num = 0f;
            if (y + (float)lineHeight >= (float)e.MarginBounds.Height)
            {
                e.HasMorePages = true;
                return false;
            }
            int num2 = line.IndexOf("|2C", StringComparison.Ordinal);
            if (num2 >= 0)
            {
                string text = line;
                while (text.Length > 0)
                {
                    int num3 = text.IndexOf("|2C", StringComparison.Ordinal);
                    if (num3 >= 0)
                    {
                        string text2 = text.Substring(0, num3).Replace("|1C", string.Empty).Replace("|2C", string.Empty);
                        e.Graphics.DrawString(text2, textFont, Brushes.Black, num, y + (float)e.MarginBounds.Top);
                        num += (float)(text2.Length * 6);
                        num3 += 3;
                        text = text.Substring(num3, text.Length - num3);
                        num3 = text.IndexOf("|1C", StringComparison.Ordinal);
                        text2 = text.Substring(0, num3).Replace("|1C", string.Empty).Replace("|2C", string.Empty);
                        e.Graphics.DrawString(text2, textFontBold, Brushes.Black, num, y + (float)e.MarginBounds.Top);
                        num += (float)(text2.Length * 6);
                        text = text.Substring(num3, text.Length - num3);
                    }
                    else
                    {
                        text = text.Replace("|1C", string.Empty).Replace("|2C", string.Empty);
                        e.Graphics.DrawString(text, textFont, Brushes.Black, num, y + (float)e.MarginBounds.Top);
                        text = string.Empty;
                    }
                }
            }
            else
            {
                string text3 = string.Empty;
                string text4 = line.Replace("|1C", string.Empty);
                Match match = Regex.Match(text4, "<B: (.+?)>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                if (match.Success)
                {
                    text3 = match.Groups[1].ToString();
                    using (Image image = this.barCode.Create(text3, e.Graphics.DpiX, e.Graphics.DpiY))
                    {
                        if (image != null)
                        {
                            float num5 = (float)image.Height / dpiYRatio;
                            if (y + num5 >= (float)e.MarginBounds.Height)
                            {
                                e.HasMorePages = true;
                                return false;
                            }
                            float x2 = Math.Max((contentWidth - (float)(image.Width + 18) / dpiXRatio) / 2f + (float)e.MarginBounds.Left, 0f);
                            e.Graphics.DrawImage(image, x2, y + (float)e.MarginBounds.Top);
                            y += num5;
                        }
                    }
                }
                else
                {
                    if (header && text4.IndexOf("Total amounts") >= 0)
                    {
                        header = false;
                    }
                    if (header)
                    {
                        if (text4.IndexOf("Date.") >= 0)
                        {
                            text3 = text4.Substring(text4.IndexOf("Date."), text4.Length - text4.IndexOf("Date."));
                            text4 = text4.Remove(text4.IndexOf("Date."), text4.Length - text4.IndexOf("Date."));
                        }
                        if (text4.IndexOf("Time.") >= 0)
                        {
                            text3 = text4.Substring(text4.IndexOf("Time."), text4.Length - text4.IndexOf("Time."));
                            text4 = text4.Remove(text4.IndexOf("Time."), text4.Length - text4.IndexOf("Time."));
                        }
                    }
                    if (!header && text4.IndexOf(":") >= 0)
                    {
                        try
                        {
                            int startIndex = text4.IndexOf(":");
                            int num4 = text4.LastIndexOf(":.", startIndex);

                            text4 = ((num4 < 0) ? (startIndex >= 0 ? text4.Remove(text4.IndexOf(":") + 1, 14) : text4) : text4.Remove(text4.IndexOf(":.") + 1, 6));
                        }
                        catch (Exception ex)
                        {
                            //ignore formatting exception as temporary solution
                        }
                    }
                    e.Graphics.DrawString(text4, textFont, Brushes.Black, 0f, y + (float)e.MarginBounds.Top);
                    if (header && !string.IsNullOrEmpty(text3))
                    {
                        y += lineHeight;
                        text3 = text3.Trim();
                        e.Graphics.DrawString(text3, textFont, Brushes.Black, 0f, y + (float)e.MarginBounds.Top);
                    }
                }
            }

            y = y + lineHeight;
            return true;
        }
    }
}
