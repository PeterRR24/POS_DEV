using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Utilities
{
    [DataContract]
    public sealed class GetProductVariantsByItemDataRequest : DataRequest
    {
        public GetProductVariantsByItemDataRequest(string itemId)
        {
            this.ItemId = itemId;
        }

        [DataMember]
        public string ItemId { get; }
    }

    [DataContract]
    public sealed class GetBarcodeForProductDataRequest : DataRequest
    {
        public GetBarcodeForProductDataRequest(string itemId, string inventDimId, string unitId)
        {
            this.ItemId = itemId;
            this.InventDimId = inventDimId;
            this.UnitId = unitId;
        }

        [DataMember]
        public string ItemId { get; }
        [DataMember]
        public string InventDimId { get; }
        [DataMember]
        public string UnitId { get; }
    }

    // Offline StockTake >>
    [DataContract]
    public sealed class GetBarcodeForItemRequest : Request
    {
        public GetBarcodeForItemRequest(IEnumerable<BarcodeItem> items)
        {
            this.Items = items;            
        }

        [DataMember]
        public IEnumerable<BarcodeItem> Items { get; }
        
    }

    [DataContract]
    public sealed class GetBarcodeForItemResponse : Response
    {
        public GetBarcodeForItemResponse(IEnumerable<BarcodeItem> barcodes)
        {
            this.Barcodes = barcodes;
        }

        [DataMember]
        public IEnumerable<BarcodeItem> Barcodes { get; }

    }
    // Offline StockTake <<

    [DataContract]
    public sealed class GetProductCategoryHierarchyDataRequest : DataRequest
    {
        public GetProductCategoryHierarchyDataRequest(long productId, long hierarchyId)
        {
            this.ProductId = productId;
            this.HierarchyId = hierarchyId;
        }

        [DataMember]
        public long ProductId { get; }
        [DataMember]
        public long HierarchyId { get; }
    }

    [DataContract]
    public sealed class GetItemIdsDataResponse : Response
    {
        public GetItemIdsDataResponse(IList<string> itemIds)
        {
            this.ItemIds = itemIds;
        }

        [DataMember]
        public IList<string> ItemIds { get; }
    }

    [DataContract]
    public sealed class GetCategoryHierarchyProductsDataRequest : Request
    {
        public GetCategoryHierarchyProductsDataRequest(long categoryId, long hierarchyId, QueryResultSettings settings)
        {
            this.CategoryId = categoryId;
            this.HierarchyId = hierarchyId;
            this.QueryResultSettings = settings;
        }

        [DataMember]
        public long CategoryId { get; set; }
        [DataMember]
        public long HierarchyId { get; set; }
    }

    [DataContract]
    public sealed class GetVariantIdForItemDataRequest : Request
    {
        public GetVariantIdForItemDataRequest(string itemId, string inventSizeId, string inventColorId, string inventStyleId, string configId)
        {
            this.ItemId = itemId;
            this.InventSizeId = inventSizeId;
            this.InventColorId = inventColorId;
            this.InventStyleId = inventStyleId;
            this.ConfigId = configId;
        }

        [DataMember]
        public string ItemId { get; }

        [DataMember]
        public string InventSizeId { get; }
        [DataMember]
        public string InventColorId { get; }
        [DataMember]
        public string InventStyleId { get; }
        [DataMember]
        public string ConfigId { get; }
    }

    [DataContract]
    public sealed class GetItemTrackingInfoDataRequest : Request
    {
        public GetItemTrackingInfoDataRequest(string itemId)
        {
            this.ItemId = itemId;
        }

        [DataMember]
        public string ItemId { get; }
    }

    [DataContract]
    public sealed class ProductSearchByTextRequest : Request
    {
        public ProductSearchByTextRequest(ProductSearchCriteria criteria, QueryResultSettings settings)
        {
            this.Criteria = criteria;

            this.QueryResultSettings = settings;
        }

        [DataMember]
        public ProductSearchCriteria Criteria { get; }
    }

    [DataContract]
    public class ProductSearchByTextResponse : Response
    {
        public ProductSearchByTextResponse() { }
        public ProductSearchByTextResponse(Microsoft.Dynamics.Commerce.Runtime.PagedResult<Product> searchResults)
        {
            this.QueryResults = searchResults;
        }

        [DataMember]
        public Microsoft.Dynamics.Commerce.Runtime.PagedResult<Product> QueryResults { get; }
    }

}
