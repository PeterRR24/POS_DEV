using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;

using RR.Commerce.Contracts.Reporting;
using RR.Commerce.Runtime.Printing;
using RR.Commerce.Runtime.Utilities;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RR.Commerce.Runtime.Reporting
{
    public sealed class LabelItemFormatRequestHandler : SingleAsyncRequestHandler<LabelItemFormatRequest>
    {
        private const string DirectPrinterName = "DIRECT";
        private const string SharpShooterPrinterName = "SHARPSHOOTER";
        //private static readonly Dictionary<string, CultureInfo> ISOCurrenciesToCultureMap = 
        //    CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(c => new { c, new RegionInfo(c.LCID).ISOCurrencySymbol })
        //    .GroupBy(x => x.ISOCurrencySymbol).ToDictionary(g => g.Key, g => g.First().c, StringComparer.OrdinalIgnoreCase);

        protected override async Task<Response> Process(LabelItemFormatRequest request)
        {
            ThrowIf.Null(request.Stat, "stat");

            if (!String.IsNullOrEmpty(request.Stat.ErrorText))
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidOperation, request.Stat.ErrorText);
            if (String.IsNullOrEmpty(request.Stat.LayoutText))
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ValueOutOfRange, ResTextPR.Text("ERR_LABEL_LAYOUT"));

#if !DEBUG
            if (request.Mobility)
                await Mobilityicense.Check(request.RequestContext).ConfigureAwait(false);
            else
                await LabelLicense.Check(request.RequestContext).ConfigureAwait(false);
#endif

            if (request.Item != null)
            {
                if (request.Direct)
                    return ProcessItemDirect(request.RequestContext, request.Stat, request.Item, request.ItemSale);
                else
                    return await ProcessItem(request.RequestContext, request.Stat, request.Item, request.ItemSale).ConfigureAwait(false);
            }
            else
                return await ProcessBatch(request.RequestContext, request.Stat).ConfigureAwait(false);
        }

        private async Task<LabelItemFormatResponse> ProcessBatch(RequestContext requestContext, LabelItemStatistic stat)
        {
            List<IFormItemLabel> labels = new List<IFormItemLabel>();

            IEnumerable<LabelItem> items = (await requestContext.ExecuteAsync<EntityDataServiceResponse<LabelItem>>(new LabelItemFormatDataRequest(stat)).ConfigureAwait(false)).PagedEntityCollection.Results;
            
            foreach (LabelItem item in items)
                await BufferLabelItem(requestContext, labels, item, null, stat.LabelPrinter).ConfigureAwait(false);

            return ProcessFormat(requestContext, stat, labels);
        }

        private async Task<LabelItemFormatResponse> ProcessItem(RequestContext requestContext, LabelItemStatistic stat, LabelItem item, LabelItemSale itemSale)
        {
            List<IFormItemLabel> labels = new List<IFormItemLabel>();

            await BufferLabelItem(requestContext, labels, item, itemSale, stat.LabelPrinter).ConfigureAwait(false);

            return ProcessFormat(requestContext, stat, labels);
        }

        private async Task BufferLabelItem(RequestContext requestContext, IList<IFormItemLabel> labels, LabelItem item, LabelItemSale itemSale, bool labelPrinter)
        {
            if (item.NoOfLabels > 0)
            {
                if (labelPrinter)
                {
                    labels.Add(new FormItemLabel(item, itemSale, item.NoOfLabels));
                }
                else
                {
                    FormItemLabel itemLabel = new FormItemLabel(item, itemSale, 1);
                    if (item.DataRecId != 0)
                    {
                        LabelData labelData = (await requestContext.ExecuteAsync<SingleEntityDataServiceResponse<LabelData>>(new GetLabelDataDataRequest(item.DataRecId)).ConfigureAwait(false)).Entity;
                        if (labelData != null)
                        {
                            itemLabel.SetImage(labelData.Image);
                            itemLabel.SetQRCode(labelData.QRCode);
                        }
                    }
                    for (int index = 0; index < item.NoOfLabels; index++)
                        labels.Add(itemLabel);
                }
            }
        }

        private LabelItemFormatResponse ProcessFormat(RequestContext requestContext, LabelItemStatistic stat, IEnumerable<IFormItemLabel> labels)
        {
            if (labels.Any())
            {
                PrinterJob job = new PrinterJob();
                job.PrinterName = stat.PrinterName;

                if (stat.LabelPrinter)
                {
                    job.Document = FormatLabelsTxt(labels, stat);
                    job.PrinterType = DirectPrinterName;
                }
                else
                {
                    ChannelConfiguration channelConfig = requestContext.GetChannelConfiguration();
                    string cultureName = channelConfig.DefaultLanguageId;   // string.IsNullOrEmpty(channelConfig.DefaultLanguageId) ? channelConfig.CompanyLanguageId : channelConfig.DefaultLanguageId;
                    if (string.IsNullOrEmpty(cultureName))
                    {
                        string currencyCode = channelConfig.Currency;
                        if (!string.IsNullOrEmpty(currencyCode) && TryGetCurrencySymbol(currencyCode, out RegionInfo regionInfo))
                            cultureName = regionInfo.Name;
                        else if (!string.IsNullOrEmpty(channelConfig.CompanyLanguageId)) 
                            cultureName = channelConfig.CompanyLanguageId;
                        
                        //if (!string.IsNullOrEmpty(currencyCode) && ISOCurrenciesToCultureMap.TryGetValue(currencyCode, out CultureInfo cultureInfo))
                        //    cultureName = cultureInfo.Name;
                    }
                    string template = stat.LayoutText;
                    template = template.Replace("ToString(&quot;C&quot;)", "ToString(&quot;C&quot;, System.Globalization.CultureInfo.GetCultureInfo(&quot;" + cultureName + "&quot;))");
                    job.Template = template.Replace(" encoding=\"utf-16\"", "");
                    job.LabelData = LabelPrinterFormat.SerializeItems(labels);
                    job.PrinterType = SharpShooterPrinterName;
                }

                return new LabelItemFormatResponse(job);
            }
            else
                return new LabelItemFormatResponse();
        }

        private LabelItemFormatResponse ProcessItemDirect(RequestContext requestContext, LabelItemStatistic stat, LabelItem item, LabelItemSale itemSale)
        {
            LabelPrinterTask task = LabelPrinterFormat.Format(stat.LayoutText, new LabelItemVariable(new FormItemLabel(item, itemSale, item.NoOfLabels)));

            return new LabelItemFormatResponse(task);
        }

        /* 
         * Moved in HWS
         * 
        private string FormatLabelsRss(RequestContext requestContext, IEnumerable<IFormItemLabel> labels, LabelItemStatistic itemStat, string cultureName)
        {
            PrcBaseForm form;
            if (itemStat.LabelType == LabelLayoutType.Product)
            {
                PrcLabelProduct prod = new PrcLabelProduct(requestContext);
                prod.Build(itemStat.LayoutRecId, cultureName);
                prod.Generate(labels);
                form = prod;
            }
            else
            {
                PrcLabelShelf shelf = new PrcLabelShelf(requestContext);
                shelf.Build(itemStat.LayoutRecId, cultureName);
                shelf.Generate(labels);
                form = shelf;
            }
            using (ReportingEngine RM = new ReportingEngine())
            {
                //cultureName = "en-GB";
                string template = itemStat.LayoutText;
                template = template.Replace("ToString(&quot;C&quot;)", "ToString(&quot;C&quot;, System.Globalization.CultureInfo.GetCultureInfo(&quot;" + cultureName + "&quot;))");
                return RM.Format(template, form.DataSet, form.Header);
            }
        }
        */
        
        private static bool TryGetCurrencySymbol(string ISOCurrencySymbol, out RegionInfo regionInfo)
        {

            regionInfo = CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .Where(c => !c.IsNeutralCulture)
                .Select(culture =>
                {
                    try
                    {
                        return new RegionInfo(culture.Name);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(ri =>
                {
                    return ri != null &&
                    string.Equals(ri.ISOCurrencySymbol, ISOCurrencySymbol, StringComparison.InvariantCultureIgnoreCase);
                })
                .FirstOrDefault();
            return regionInfo != null;
        }

        private string FormatLabelsTxt(IEnumerable<IFormItemLabel> labels, LabelItemStatistic itemStat)
        {
            List<LabelPrinterTask> tasks = new List<LabelPrinterTask>();
            foreach (IFormItemLabel label in labels)
            {
                LabelPrinterTask task = LabelPrinterFormat.Format(itemStat.LayoutText, new LabelItemVariable(label));
                if (task != null && task.Quantity > 0)
                    tasks.Add(task);
            }
            if (tasks.Count <= 0)
                return null;
            else
                return LabelPrinterFormat.ToXml(tasks);
        }
    }
}
