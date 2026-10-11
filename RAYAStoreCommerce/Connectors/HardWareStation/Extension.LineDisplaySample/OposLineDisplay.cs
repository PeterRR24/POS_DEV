namespace Contoso
{
    namespace Commerce.HardwareStation.LineDisplaySample
    {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Interop.OposConstants;
    using Interop.OposLineDisplay;
    using Microsoft.Dynamics.Commerce.HardwareStation;
    using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
    using Microsoft.Dynamics.Commerce.Runtime.Handlers;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Class implements OPOS based line display for hardware station.
        /// </summary>
#pragma warning disable CS0618 // JUSTIFICATION: Pending migration to asynchronous APIs.
        public class OposLineDisplay : INamedRequestHandler
#pragma warning restore CS0618
        {
            private IOPOSLineDisplay oposLineDisplay;

            /// <summary>
            /// Gets the unique name for this request handler.
            /// </summary>
            public string HandlerName
            {
                get { return PeripheralType.Opos; }
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
                    typeof(DisplayTextLineDisplayDeviceRequest)
                };
                }
            }

            /// <summary>
            /// Represents the entry point for the line display device request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                ThrowIf.Null(request, nameof(request));

                switch (request)
                {
                    case DisplayTextLineDisplayDeviceRequest displayTextLineDisplayDeviceRequest:
                        if (displayTextLineDisplayDeviceRequest.Clear)
                        {
                            this.Clear();
                        }
                        this.DisplayText(displayTextLineDisplayDeviceRequest.Lines);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }

                return NullResponse.Instance;
            }

            /// <summary>
            /// Displays the text.
            /// </summary>
            /// <param name="lines">The lines to display.</param>
            private void DisplayText(IEnumerable<string> lines)
            {
                ThrowIf.Null(lines, "lines");

                var index = 0;

                try
                {
                    this.oposLineDisplay = OPOSDeviceManager<IOPOSLineDisplay>.Instance.AcquireDeviceHandle<OPOSLineDisplayClass>();

                    foreach (var line in lines)
                    {
                        var textToDisplay = line;

                        // hardcode for subtotal indent
                        if (index > 1)
                        {
                            index++;
                        }

                        if (this.oposLineDisplay.BinaryConversion == 2)
                        {
                            textToDisplay = OposHelper.ConvertToBCD(textToDisplay, this.oposLineDisplay.CharacterSet);
                        }

                        this.oposLineDisplay.DisplayTextAt(index++, 0, textToDisplay, (int)OPOSLineDisplayConstants.DISP_DT_NORMAL);
                        OposHelper.CheckResultCode(this, this.oposLineDisplay.ResultCode);
                    }
                }
                catch
                {
                    throw new PeripheralException(PeripheralException.LineDisplayError, "Display text error.");
                }
            }

            /// <summary>
            /// Clears this display.
            /// </summary>
            private void Clear()
            {
                try
                {
                    this.oposLineDisplay = OPOSDeviceManager<IOPOSLineDisplay>.Instance.AcquireDeviceHandle<OPOSLineDisplayClass>();

                    if (this.oposLineDisplay != null)
                    {
                        this.oposLineDisplay.ClearText();
                    }
                }
                catch
                {
                    throw new PeripheralException(PeripheralException.LineDisplayError, "Clear text error.");
                }
            }
        }
    }
}