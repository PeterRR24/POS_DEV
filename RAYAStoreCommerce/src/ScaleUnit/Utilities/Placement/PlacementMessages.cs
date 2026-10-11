using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Utilities
{
    [DataContract]
    public sealed class GetStoreLocationItemsServiceRequest : Request
    {
        public GetStoreLocationItemsServiceRequest(string sectionId, string shelfId, QueryResultSettings settings)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
            this.QueryResultSettings = settings;
        }

        [DataMember]
        public string SectionId { get; }
        [DataMember]
        public string ShelfId { get; }
    }

    [DataContract]
    public sealed class GetStoreLocationItemsDataRequest : Request
    {
        public GetStoreLocationItemsDataRequest(string sectionId, string shelfId, QueryResultSettings settings)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
            this.QueryResultSettings = settings;
        }

        public GetStoreLocationItemsDataRequest(GetStoreLocationItemsServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            this.SectionId = request.SectionId;
            this.ShelfId = request.ShelfId;
            this.QueryResultSettings = request.QueryResultSettings;
        }

        [DataMember]
        public string SectionId { get; }
        [DataMember]
        public string ShelfId { get; }
    }

    [DataContract]
    public sealed class GetStoreLocationItemDataRequest : Request
    {
        public GetStoreLocationItemDataRequest(string sectionId, string shelfId, int location)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
            this.Location = location;
        }

        [DataMember]
        public string SectionId { get; }
        [DataMember]
        public string ShelfId { get; }
        [DataMember]
        public int Location { get; }
    }

    [DataContract]
    public sealed class GetStoreLocationTypeDataRequest : Request
    {
        public GetStoreLocationTypeDataRequest(string sectionId, string shelfId)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
        }

        [DataMember]
        public string SectionId { get; set; }
        [DataMember]
        public string ShelfId { get; set; }
    }

    [DataContract]
    public sealed class GetStoreLocationItemIdsServiceRequest : Request
    {
        public GetStoreLocationItemIdsServiceRequest(string sectionId, string shelfId) : this(sectionId, shelfId, QueryResultSettings.AllRecords)
        {
        }

        public GetStoreLocationItemIdsServiceRequest(string sectionId, string shelfId, QueryResultSettings setting)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
            this.QueryResultSettings = setting;
        }

        [DataMember]
        public string SectionId { get; set; }
        [DataMember]
        public string ShelfId { get; set; }
    }

    [DataContract]
    public sealed class GetStoreLocationItemIdsDataRequest : Request
    {
        /*
        public GetStoreLocationItemIdsDataRequest(string sectionId, string shelfId, bool local)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
            this.Local = local;
        }
        */

        public GetStoreLocationItemIdsDataRequest(GetStoreLocationItemIdsServiceRequest request, bool local)
        {
            ThrowIf.Null(request, "request");
            this.SectionId = request.SectionId;
            this.ShelfId = request.ShelfId;
            this.QueryResultSettings = request.QueryResultSettings;
            this.Local = local;
        }

        [DataMember]
        public string SectionId { get; set; }
        [DataMember]
        public string ShelfId { get; set; }
        [DataMember]
        public bool Local { get; set; }
    }

    /*
    [DataContract]
    public sealed class GetStoreLocationItemNosServiceRequest : Request
    {
        public GetStoreLocationItemNosServiceRequest(string sectionId, string shelfId, QueryResultSettings setting)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
            this.QueryResultSettings = setting;
        }

        [DataMember]
        public string SectionId { get; set; }
        [DataMember]
        public string ShelfId { get; set; }
    }


    [DataContract]
    public sealed class GetStoreLocationItemNosDataRequest : Request
    {
       public GetStoreLocationItemNosDataRequest(GetStoreLocationItemNosServiceRequest request, bool local)
        {
            ThrowIf.Null(request, "request");
            this.SectionId = request.SectionId;
            this.ShelfId = request.ShelfId;
            this.QueryResultSettings = request.QueryResultSettings;
            this.Local = local;
        }

        [DataMember]
        public string SectionId { get; set; }
        [DataMember]
        public string ShelfId { get; set; }
        [DataMember]
        public bool Local { get; set; }
    }
    */

    [DataContract]
    public sealed class GetItemStoreLocationsServiceRequest : Request
    {
        public GetItemStoreLocationsServiceRequest(string itemId)
        {
            this.ItemId = itemId;
        }

        public GetItemStoreLocationsServiceRequest(string itemId, string sectionId, string shelfId)
        {
            this.ItemId = itemId;
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
        }

        [DataMember]
        public string ItemId { get; }
        [DataMember]
        public string SectionId { get; set; }
        [DataMember]
        public string ShelfId { get; set; }
    }

    [DataContract]
    public sealed class GetItemStoreLocationsDataRequest : Request
    {
        public GetItemStoreLocationsDataRequest(string itemId, string sectionId, string shelfId, bool local)
        {
            this.ItemId = itemId;
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
            this.Local = local;
        }

        public GetItemStoreLocationsDataRequest(GetItemStoreLocationsServiceRequest request, bool local)
        {
            ThrowIf.Null(request, "request");
            this.ItemId = request.ItemId;
            this.SectionId = request.SectionId;
            this.ShelfId = request.ShelfId;
            this.Local = local;
        }

        [DataMember]
        public string ItemId { get; }
        [DataMember]
        public string SectionId { get; set; }
        [DataMember]
        public string ShelfId { get; set; }
        [DataMember]
        public bool Local { get; set; }
    }

    [DataContract]
    public sealed class GetItemStoreLocationsResponse : Response
    {
        public GetItemStoreLocationsResponse(IList<ItemStoreLocation> locations)
        {
            this.Locations = locations;
        }

        [DataMember]
        public IList<ItemStoreLocation> Locations { get; }
    }

    [DataContract]
    public sealed class ClearStoreLocationItemsDataRequest : Request
    {
        public ClearStoreLocationItemsDataRequest(string sectionId, string shelfId)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
        }

        [DataMember]
        public string SectionId { get; }
        [DataMember]
        public string ShelfId { get; }
    }

    [DataContract]
    public sealed class CommitStoreLocationItemsServiceRequest : Request
    {
        public CommitStoreLocationItemsServiceRequest(string sectionId, string shelfId)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
        }

        [DataMember]
        public string SectionId { get; }
        [DataMember]
        public string ShelfId { get; }
    }

    [DataContract]
    public sealed class CommitStoreLocationItemsRealtimeRequest : RealtimeRequest
    {
        public CommitStoreLocationItemsRealtimeRequest(string storeId, string sectionId, string shelfId, IEnumerable<StoreLocationItem> items)
        {
            this.StoreId = storeId;
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
            this.Items = items.AsReadOnly();
        }

        [DataMember]
        public string StoreId { get; }

        [DataMember]
        public string SectionId { get; }

        [DataMember]
        public string ShelfId { get; }

        [DataMember]
        public ReadOnlyCollection<StoreLocationItem> Items { get; }
    }

    [DataContract]
    public sealed class PlaceStoreLocationItemsDataRequest : Request
    {
        public PlaceStoreLocationItemsDataRequest(string sectionId, string shelfId, IEnumerable<StoreLocationItem> items)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
            this.Items = items.AsReadOnly();
        }

        [DataMember]
        public string SectionId { get; }
        [DataMember]
        public string ShelfId { get; }
        [DataMember]
        public ReadOnlyCollection<StoreLocationItem> Items { get; }
    }

    [DataContract]
    public sealed class PlaceStoreLocationItemDataRequest : Request
    {
        public PlaceStoreLocationItemDataRequest(string sectionId, string shelfId, int location, string itemId, int facingCount)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
            this.Location = location;
            this.ItemId = itemId;
            this.FacingCount = facingCount;
        }

        public PlaceStoreLocationItemDataRequest(string sectionId, string shelfId, int location, StoreLocationItem item)
        {
            ThrowIf.Null(item, "item");
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
            this.Location = location;
            this.ItemId = item.ItemId;
            this.InventDimId = item.InventDimId;
        }

        [DataMember]
        public string SectionId { get; }
        [DataMember]
        public string ShelfId { get; }
        [DataMember]
        public int Location { get; }
        [DataMember]
        public string ItemId { get; }
        [DataMember]
        public string InventDimId { get; }
        [DataMember]
        public int FacingCount { get; }
    }

    [DataContract]
    public sealed class RemoveStoreLocationItemDataRequest : Request
    {
        public RemoveStoreLocationItemDataRequest(string sectionId, string shelfId, int location)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
            this.Location = location;
        }

        [DataMember]
        public string SectionId { get; }
        [DataMember]
        public string ShelfId { get; }
        [DataMember]
        public int Location { get; }
    }

    [DataContract]
    public sealed class MoveStoreLocationItemServiceRequest : Request
    {
        public MoveStoreLocationItemServiceRequest(string sectionId, string shelfId, int oldLocation, int newLocation)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
            this.OldLocation = oldLocation;
            this.NewLocation = newLocation;
        }

        [DataMember]
        public string SectionId { get; }
        [DataMember]
        public string ShelfId { get; }
        [DataMember]
        public int OldLocation { get; }
        [DataMember]
        public int NewLocation { get; }
    }

    [DataContract]
    public sealed class GetItemStoreLocationFacesDataRequest : Request
    {
        public GetItemStoreLocationFacesDataRequest(string itemId, string inventDimId)
        {
            this.ItemId = itemId;
            this.InventDimId = inventDimId;
        }

        public GetItemStoreLocationFacesDataRequest(string itemId, string inventDimId, string sectionId, string shelfId)
        {
            this.ItemId = itemId;
            this.InventDimId = inventDimId;
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
        }

        [DataMember]
        public string ItemId { get; }
        [DataMember]
        public string InventDimId { get; set; }
        [DataMember]
        public string SectionId { get; set; }
        [DataMember]
        public string ShelfId { get; set; }
    }

    [DataContract]
    public sealed class GetItemStoreLocationFacesResponse : Response
    {
        public GetItemStoreLocationFacesResponse(int faces)
        {
            this.Faces = faces;
        }

        [DataMember]
        public int Faces { get; }
    }
}
