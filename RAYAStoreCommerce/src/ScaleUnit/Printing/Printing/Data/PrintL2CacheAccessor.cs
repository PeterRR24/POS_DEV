using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Printing
{/*
    public sealed class PrintL2CacheAccessor : DataCacheAccessor
    {
        private PrintL2CacheAccessor(IDataStore backingStore, RequestContext context) : base(backingStore, context)
        {
        }

        internal static PrintL2CacheAccessor Instantiate(IDataStore dataStore, RequestContext context)
        {
            return new PrintL2CacheAccessor(dataStore, context);
        }

        protected override string GetKeyPattern()
        {
            return String.Format("{0}\\{1}\\", this.ChannelId, "Printing");
        }

        public ReadOnlyCollection<PRPrinter> GetPrinters()
        {
            string key = this.GenerateKey("Printers");
            ReadOnlyCollection<PRPrinter> result = null;
            this.TryGetItem<ReadOnlyCollection<PRPrinter>>(key, out result);
            return result;
        }

        public void PutPrinters(ReadOnlyCollection<PRPrinter> result)
        {
            ThrowIf.Null<ReadOnlyCollection<PRPrinter>>(result, "result");
            string key = this.GenerateKey("Printers");
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(5.0);
            this.PutItem<ReadOnlyCollection<PRPrinter>>(key, result, absoluteExpiration);
        }

        public ReadOnlyCollection<PRTerminal> GetTerminals()
        {
            string key = this.GenerateKey("Terminals");
            ReadOnlyCollection<PRTerminal> result = null;
            this.TryGetItem<ReadOnlyCollection<PRTerminal>>(key, out result);
            return result;
        }

        public void PutTerminals(ReadOnlyCollection<PRTerminal> result)
        {
            ThrowIf.Null<ReadOnlyCollection<PRTerminal>>(result, "result");
            string key = this.GenerateKey("Terminals");
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(5.0);
            this.PutItem<ReadOnlyCollection<PRTerminal>>(key, result, absoluteExpiration);
        }

        public ReadOnlyCollection<StorePrinter> GetStorePrinters()
        {
            string key = this.GenerateKey("StorePrinters");
            ReadOnlyCollection<StorePrinter> result = null;
            this.TryGetItem<ReadOnlyCollection<StorePrinter>>(key, out result);
            return result;
        }

        public void PutStorePrinters(ReadOnlyCollection<StorePrinter> result)
        {
            ThrowIf.Null<ReadOnlyCollection<StorePrinter>>(result, "result");
            string key = this.GenerateKey("StorePrinters");
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(5.0);
            this.PutItem<ReadOnlyCollection<StorePrinter>>(key, result, absoluteExpiration);
        }

    }*/
}
