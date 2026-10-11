using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

using RR.Commerce.Contracts.Reporting;

namespace RR.Commerce.Runtime.Reporting
{
    public abstract class PrcBaseForm
    {
        private readonly RequestContext CTX;
        
        public PrcBaseForm(RequestContext ctx)
        {
            CTX = ctx;
        }

        public RequestContext Context { get { return CTX; } }

        public abstract object Header { get; }
        public abstract object DataSet { get; }
    }

    //

    public abstract class PrrBase : RorBase { }

    public abstract class RorBase
    {
        protected string NotNull(string value)
        {
            try { return value ?? String.Empty; }
            catch (ArgumentOutOfRangeException) { return String.Empty; }
            catch (NullReferenceException) { return String.Empty; }
        }

        protected decimal NotNull(decimal? value)
        {
            try { return value.HasValue ? value.Value : 0; }
            catch (ArgumentOutOfRangeException) { return 0; }
            catch (NullReferenceException) { return 0; }
        }

        protected double NotNull(double? value)
        {
            try { return value.HasValue ? value.Value : 0; }
            catch (ArgumentOutOfRangeException) { return 0; }
            catch (NullReferenceException) { return 0; }
        }

        protected int NotNull(int? value)
        {
            try { return value.HasValue ? value.Value : 0; }
            catch (ArgumentOutOfRangeException) { return 0; }
            catch (NullReferenceException) { return 0; }
        }
    }

    //

    public class RorStoreInformation : RorBase
    {
        private DeviceConfiguration _device;
        private OrgUnit _orgUinit;
        private Address _address;

        internal RorStoreInformation(RequestContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            _orgUinit = context.GetOrgUnit();
            if (_orgUinit != null)
                _address = _orgUinit.OrgUnitAddress;
            else
                _address = null;
            _device = context.GetDeviceConfiguration();
        }

        public string StoreId { get { return _device == null ? String.Empty : _device.StoreNumber; } }
        public string Name { get { return _orgUinit == null ? String.Empty : _orgUinit.Name; } }
        public string Address { get { return _address == null ? String.Empty : _address.FullAddress; } }
        public string Street { get { return _address == null ? String.Empty : _address.Street; } }
        public string Zip { get { return _address == null ? String.Empty : _address.ZipCode; } }
        public string City { get { return _address == null ? String.Empty : _address.City; } }
        public string County { get { return _address == null ? String.Empty : _address.County; } }
        public string State { get { return _address == null ? String.Empty : _address.State; } }
        public string Country { get { return String.Empty; } }
        public string Phone { get { return _device == null ? String.Empty : _device.StorePhone; } }
        public string TaxIdNumber { get { return _device == null ? String.Empty : _device.TaxIdNumber; } }
    }

    public class RorReportHost : RorBase, IReportHost
    {
        private DeviceConfiguration _device;

        internal RorReportHost(RequestContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            _device = context.GetDeviceConfiguration();
        }

        public string StoreId { get { return _device == null ? String.Empty : _device.StoreNumber; } }
        public string TerminalId { get { return _device == null ? String.Empty : _device.TerminalId; } }

        public ReportHostType HostType { get { return ReportHostType.Service; } }
    }

    public abstract class RohCore : IReportController
    {
        private RorStoreInformation _store = null;
        private RorReportHost _host = null;

        public RorStoreInformation Store
        {
            get
            {
                if (_store == null)
                    _store = new RorStoreInformation(Context);
                return _store;
            }
        }

        #region IReportController

        public abstract RequestContext Context { get; }

        public IReportHost Host
        {
            get
            {
                if (_host == null)
                    _host = new RorReportHost(Context);
                return _host;
            }
        }

        public virtual string TranslateText(string textId, string defaultText) { return defaultText; }

        public virtual object ExecuteCRT(object request, Type response)
        {
            MethodInfo method = typeof(RequestContext).GetMethod("Execute").MakeGenericMethod(new Type[] { response });
            return method.Invoke(this.Context, new object[] { request });
        }
        #endregion
    }
}
