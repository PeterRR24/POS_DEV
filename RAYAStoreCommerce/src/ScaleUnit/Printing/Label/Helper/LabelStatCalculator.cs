using System;
using System.Text;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using System.Threading.Tasks;

namespace RR.Commerce.Runtime.Printing
{
    internal class LabelStatCalculator
    {
        private RequestContext Context;
        private HardwareProfile profile = null;
        private PRTerminal terminal = null;
        private StorePrinter storePrinter = null;

        public LabelStatCalculator(RequestContext context)
        {
            ThrowIf.Null(context, "context");

            Context = context;
        }

        private async Task<LabelLayout> GetLayout(LabelItemStatistic item)
        {
            LabelLayout layout = null;

            //Test
            if (item.LayoutRecId == -1)
            {
                layout = new LabelLayout();
                layout.Id = -1;
                layout.LayoutId = "Test";
                layout.LabelType = item.LabelType;
                layout.Template = "Test";
                layout.Template1 = "Test1";
                layout.Template2 = "Test2";
                layout.Template3 = "Test3";
                return layout;
            }
            //

            if (item.LayoutRecId != 0)
            {
                GetLabelLayoutDataRequest request = new GetLabelLayoutDataRequest(item.LayoutRecId);
                layout = (await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<LabelLayout>>(request).ConfigureAwait(false)).Entity;
                if (layout != null)
                {
                    if (!layout.Inactive)
                    {
                        if (layout.LabelType != item.LabelType)
                            item.AddError("Invalid Label Layout Type.");
                    }
                    else
                        item.AddError(string.Format("Label Layout '{0}' is inactive.", layout.LayoutId));
                }
                else
                    item.AddError(string.Format("Invalid Label Layout Id '{0}'.", item.LayoutRecId));
            }
            else
                item.AddError("Label Layout must be specified.");

            return layout;
        }

        private async Task<PRPrinter> GetPrinter(LabelItemStatistic item)
        {
            PRPrinter printer = null;

            //Test
            if (item.PrinterRecId == -1)
            {
                printer = new PRPrinter();
                printer.Id = -1;
                printer.PrinterId = "Test";
                printer.LabelPrinter = LabelPrinterTemplate.Template2;
                //printer.SystemPrinter = SystemPrinterType.Default;
                printer.WindowsPrinter = WindowsPrinterType.Custom;
                printer.WindowsPrinterName = "Test";
                return printer;
            }
            //

            if (item.PrinterRecId != 0)
            {
                GetPRPrinterDataRequest request = new GetPRPrinterDataRequest(item.PrinterRecId);
                printer = (await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<PRPrinter>>(request).ConfigureAwait(false)).Entity;
                if (printer == null)
                    item.AddError("Printer Name must be specified.");
            }
            else
                item.AddError("Printer Name must be specified.");

            return printer;
        }

        public async Task Validate(LabelItemStatistic item)
        {
            await Validate(item, LabelPrinterTemplate.None).ConfigureAwait(false);
        }

        public async Task Validate(LabelItemStatistic item, LabelPrinterTemplate printerTemplate)
        {
            LabelLayout layout = await GetLayout(item).ConfigureAwait(false);
            if (printerTemplate == LabelPrinterTemplate.None)
            {
                PRPrinter printer = await GetPrinter(item).ConfigureAwait(false);
                await Validate(item, layout, printer).ConfigureAwait(false);
            }
            else
                Validate(item, layout, printerTemplate);
        }

        private async Task Validate(LabelItemStatistic item, LabelLayout layout, PRPrinter printer)
        {
            if (layout != null)
            {
                item.LayoutId = layout.LayoutId;
                item.LayoutName = layout.Name;

                if (printer != null)
                {
                    item.LayoutText = FindLabelTemplate(layout, printer.LabelPrinter);
                    if (String.IsNullOrEmpty(item.LayoutText))
                        item.AddError(string.Format("Label Layout '{0}' template is not defined.", layout.LayoutId));

                    item.PrinterName = await GetPrinterName(printer).ConfigureAwait(false);
                    if (String.IsNullOrEmpty(item.PrinterName))
                        item.AddError("Printer Name must be specified.");

                    item.LabelPrinter = printer.LabelPrinter != LabelPrinterTemplate.None;
                }
            }
        }

        private void Validate(LabelItemStatistic item, LabelLayout layout, LabelPrinterTemplate printerTemplate)
        {
            if (layout != null)
            {
                item.LayoutId = layout.LayoutId;
                item.LayoutName = layout.Name;

                item.LayoutText = FindLabelTemplate(layout, printerTemplate);
                if (String.IsNullOrEmpty(item.LayoutText))
                    item.AddError(string.Format("Label Layout '{0}' template is not defined.", layout.LayoutId));

                item.LabelPrinter = printerTemplate != LabelPrinterTemplate.None;
            }
        }

        private async Task<string> GetPrinterName(PRPrinter printer)
        {
            //if (printer.WindowsPrinter == WindowsPrinterType.None)
            //    return FindTerminalPrinterName(printer.SystemPrinter);
            //else
            //    return FindWindowsPrinterName(printer.WindowsPrinter, printer.WindowsPrinterName);
            return await FindStorePrinterName(printer.WindowsPrinter, printer.WindowsPrinterName).ConfigureAwait(false);
        }

        private async Task<string> FindTerminalPrinterName(SystemPrinterType printer)
        {
            if (profile == null)
            {
                GetHardwareProfileDataRequest request = new GetHardwareProfileDataRequest(this.Context.GetTerminal().HardwareProfile, QueryResultSettings.AllRecords);
                profile = (await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<HardwareProfile>>(request).ConfigureAwait(false)).Entity;
            }

            if (profile != null)
            {
                HardwareProfilePrinter printer1 = null, printer2 = null;
                if (profile.Printers.Count > 0)
                {
                    printer1 = profile.Printers[0];
                    if (profile.Printers.Count > 1)
                        printer2 = profile.Printers[1];
                }

                if ((printer == SystemPrinterType.Default || printer == SystemPrinterType.Printer1) &&
                     printer1 != null && printer1.DeviceType == DeviceType.WindowsPrinter && !String.IsNullOrEmpty(printer1.DeviceName))
                    return printer1.DeviceName;

                if ((printer == SystemPrinterType.Default || printer == SystemPrinterType.Printer2) &&
                     printer2 != null && printer2.DeviceType == DeviceType.WindowsPrinter && !String.IsNullOrEmpty(printer2.DeviceName))
                    return printer2.DeviceName;
            }

            return null;
        }

        private async Task<string> FindWindowsPrinterName(WindowsPrinterType printer, string custom)
        {
            if (printer == WindowsPrinterType.Custom)
                return custom;

            if (terminal == null)
            {
                GetPRTerminalDataRequest request = new GetPRTerminalDataRequest(this.Context.GetTerminal().TerminalId);
                terminal = (await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<PRTerminal>>(request).ConfigureAwait(false)).Entity;
            }

            if (terminal != null)
            {
                switch (printer)
                {
                    case WindowsPrinterType.Terminal1: return terminal.WindowsPrinterName1;
                    case WindowsPrinterType.Terminal2: return terminal.WindowsPrinterName2;
                    case WindowsPrinterType.Terminal3: return terminal.WindowsPrinterName3;
                }
            }

            return null;
        }

        private async Task<string> FindStorePrinterName(WindowsPrinterType printer, string custom)
        {
            if (printer == WindowsPrinterType.Custom)
                return custom;

            if (storePrinter == null)
            {
                GetStorePrinterDataRequest request = new GetStorePrinterDataRequest(this.Context.GetPrincipal().ChannelId);
                storePrinter = (await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<StorePrinter>>(request).ConfigureAwait(false)).Entity;
            }
            if (storePrinter != null)
            {
                switch (printer)
                {
                    case WindowsPrinterType.Terminal1: return storePrinter.WindowsPrinterName1;
                    case WindowsPrinterType.Terminal2: return storePrinter.WindowsPrinterName2;
                    case WindowsPrinterType.Terminal3: return storePrinter.WindowsPrinterName3;
                }
            }
            return null;
        }

        private string FindLabelTemplate(LabelLayout layout, LabelPrinterTemplate printerTemplate)
        {
            switch (printerTemplate)
            {
                case LabelPrinterTemplate.Template1: return layout.Template1;
                case LabelPrinterTemplate.Template2: return layout.Template2;
                case LabelPrinterTemplate.Template3: return layout.Template3;
            }
            return layout.Template;
        }
    }
}
