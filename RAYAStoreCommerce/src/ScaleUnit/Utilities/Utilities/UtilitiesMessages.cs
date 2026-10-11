using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Utilities
{
    [DataContract]
    public class GetUtilitiesConfigurationDataRequest : DataRequest
    {
        public GetUtilitiesConfigurationDataRequest()
        {
        }
    }

    [DataContract]
    public sealed class GetCategoryHierarchyDataRequest : DataRequest
    {
        public GetCategoryHierarchyDataRequest(long hierarchyId)
        {
            this.HierarchyId = hierarchyId;
        }

        [DataMember]
        public long HierarchyId { get; }
    }

    [DataContract]
    public sealed class GetStoreSectionDataRequest : DataRequest
    {
        public GetStoreSectionDataRequest()
        {
        }
    }

    [DataContract]
    public sealed class GetStoreShelfDataRequest : DataRequest
    {
        public GetStoreShelfDataRequest(string sectionId)
        {
            this.SectionId = sectionId;
        }

        [DataMember]
        public string SectionId { get; }
    }
}
