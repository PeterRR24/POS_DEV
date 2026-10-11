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
    public sealed class LabelL2CacheAccessor : DataCacheAccessor
    {
        private LabelL2CacheAccessor(IDataStore backingStore, RequestContext context) : base(backingStore, context)
        {
        }

        internal static LabelL2CacheAccessor Instantiate(IDataStore dataStore, RequestContext context)
        {
            return new LabelL2CacheAccessor(dataStore, context);
        }

        protected override string GetKeyPattern()
        {
            return String.Format("{0}\\{1}\\", this.ChannelId, "LabelItem");
        }

        public PagedResult<LabelCode> GetLabelCodes()
        {
            string key = this.GenerateKey("LabelCodes");
            PagedResult<LabelCode> result = null;
            this.TryGetItem<PagedResult<LabelCode>>(key, out result);
            return result;
        }

        public void PutLabelCodes(PagedResult<LabelCode> result)
        {
            ThrowIf.Null<PagedResult<LabelCode>>(result, "result");
            string key = this.GenerateKey("LabelCodes");
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(5.0);
            this.PutItem<PagedResult<LabelCode>>(key, result, absoluteExpiration);
        }

        public ReadOnlyCollection<LabelProfile> GetLabelProfiles()
        {
            string key = this.GenerateKey("LabelProfiles");
            ReadOnlyCollection<LabelProfile> result = null;
            this.TryGetItem<ReadOnlyCollection<LabelProfile>>(key, out result);
            return result;
        }

        public void PutLabelProfiles(ReadOnlyCollection<LabelProfile> result)
        {
            ThrowIf.Null<ReadOnlyCollection<LabelProfile>>(result, "result");
            string key = this.GenerateKey("LabelProfiles");
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(5.0);
            this.PutItem<ReadOnlyCollection<LabelProfile>>(key, result, absoluteExpiration);
        }

        public ReadOnlyCollection<LabelProfileLine> GetLabelProfileLines()
        {
            string key = this.GenerateKey("LabelProfileLines");
            ReadOnlyCollection<LabelProfileLine> result = null;
            this.TryGetItem<ReadOnlyCollection<LabelProfileLine>>(key, out result);
            return result;
        }

        public void PutLabelProfileLines(ReadOnlyCollection<LabelProfileLine> result)
        {
            ThrowIf.Null<ReadOnlyCollection<LabelProfileLine>>(result, "result");
            string key = this.GenerateKey("LabelProfileLines");
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(5.0);
            this.PutItem<ReadOnlyCollection<LabelProfileLine>>(key, result, absoluteExpiration);
        }

        public ReadOnlyCollection<LabelFormat> GetLabelFormats()
        {
            string key = this.GenerateKey("LabelFormats");
            ReadOnlyCollection<LabelFormat> result = null;
            this.TryGetItem<ReadOnlyCollection<LabelFormat>>(key, out result);
            return result;
        }

        public void PutLabelFormats(ReadOnlyCollection<LabelFormat> result)
        {
            ThrowIf.Null<ReadOnlyCollection<LabelFormat>>(result, "result");
            string key = this.GenerateKey("LabelFormats");
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(5.0);
            this.PutItem<ReadOnlyCollection<LabelFormat>>(key, result, absoluteExpiration);
        }

        public ReadOnlyCollection<LabelLayout> GetLabelLayouts()
        {
            string key = this.GenerateKey("LabelLayouts");
            ReadOnlyCollection<LabelLayout> result = null;
            this.TryGetItem<ReadOnlyCollection<LabelLayout>>(key, out result);
            return result;
        }

        public void PutLabelLayouts(ReadOnlyCollection<LabelLayout> result)
        {
            ThrowIf.Null<ReadOnlyCollection<LabelLayout>>(result, "result");
            string key = this.GenerateKey("LabelLayouts");
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(5.0);
            this.PutItem<ReadOnlyCollection<LabelLayout>>(key, result, absoluteExpiration);
        }
    }*/
}
