using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RR.Commerce.Contracts.Reporting
{
    public enum ReportHostType
    {
        HQ = 3,
        Service = 5
    }

    public interface IReportHost
    {
        string StoreId { get; }
        string TerminalId { get; }
        ReportHostType HostType { get; }
    }

    public interface IReportController
    {
        IReportHost Host { get; }

        string TranslateText(string textId, string defaultText);

        object ExecuteCRT(object request, Type response);
    }
}
