using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;

namespace RR.Commerce.Runtime.Utilities
{

    //public sealed class UtilitiesDataManager : DataManager
    //{
    //    private const int MaxCachedCollectionSize = 500;

    //    public UtilitiesDataManager(RequestContext context) : base(context)
    //    {
    //        this.DataStoreManagerInstance.RegisterDataStoreAccessor(DataStoreType.Database, new Func<IDataStore, RequestContext, DataStoreAccessor>(UtilitiesDatabaseAccessor.Instantiate), context);
    //        this.DataStoreManagerInstance.RegisterDataStoreAccessor(DataStoreType.L2Cache, new Func<IDataStore, RequestContext, DataStoreAccessor>(UtilitiesL2CacheAccessor.Instantiate), context);
    //    }

    //    public static UtilitiesDataManager Instantiate(RequestContext context)
    //    {
    //        return new UtilitiesDataManager(context);
    //    }

    //    public UtilitiesConfiguration GetUtilitiesConfiguration()
    //    {
            
    //        UtilitiesL2CacheAccessor cacheAccessor = (UtilitiesL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];

    //        bool found;
    //        bool updateL2Cache;
    //        UtilitiesConfiguration result = DataManager.GetDataFromCache(() => cacheAccessor.GetUtilitiesConfiguration(), out found, out updateL2Cache);

    //        if (!found)
    //        {
    //            UtilitiesDatabaseAccessor databaseAccessor = (UtilitiesDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
    //            result = databaseAccessor.GetUtilitiesConfiguration();
    //            if (result == null)
    //            {
    //                result = new UtilitiesConfiguration();
    //            }
    //            updateL2Cache = true;
    //        }

    //        if (updateL2Cache)
    //            cacheAccessor.PutUtilitiesConfiguration(result);

    //        return result;
    //    }

    //    public PagedResult<StoreSection> GetStoreSections()
    //    {
    //        UtilitiesL2CacheAccessor cacheAccessor = (UtilitiesL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
    //        bool mustUpdateOnMiss = DataStoreManager.DataStores[DataStoreType.L2Cache].Policy.MustUpdateOnMiss;

    //        PagedResult<StoreSection> result = cacheAccessor.GetStoreSections();
    //        bool flag = mustUpdateOnMiss & result == null;
    //        if (result == null)
    //        {
    //            UtilitiesDatabaseAccessor databaseAccessor = (UtilitiesDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
    //            result = databaseAccessor.GetStoreSections();
    //            flag &= result != null;
    //        }
    //        if (flag)
    //            cacheAccessor.PutStoreSections(result);

    //        return result;
    //    }

    //    public PagedResult<StoreShelf> GetStoreShelves(string sectionId)
    //    {
    //        UtilitiesL2CacheAccessor cacheAccessor = (UtilitiesL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
    //        bool mustUpdateOnMiss = DataStoreManager.DataStores[DataStoreType.L2Cache].Policy.MustUpdateOnMiss;

    //        PagedResult<StoreShelf> result = cacheAccessor.GetStoreShelves();
    //        bool flag = mustUpdateOnMiss & result == null;
    //        if (result == null)
    //        {
    //            UtilitiesDatabaseAccessor databaseAccessor = (UtilitiesDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
    //            result = databaseAccessor.GetStoreShelves();
    //            flag &= result != null;
    //        }
    //        if (flag)
    //            cacheAccessor.PutStoreShelves(result);

    //        if (String.IsNullOrEmpty(sectionId))
    //            return result;
    //        else
    //            return new PagedResult<StoreShelf>(result.Results.Where(s => String.Compare(s.SectionId, sectionId, StringComparison.OrdinalIgnoreCase) == 0).AsReadOnly());
    //    }

    //    // Data.Services\ChannelDataService.cs => GetChannelCategories
    //    public PagedResult<Category> GetCategoryHierarchy(long hierarchyId)
    //    {
    //        //TEST
    //        //hierarchyId = 22565421921;

    //        if (hierarchyId == 0)
    //            return null;

    //        UtilitiesL2CacheAccessor cacheAccessor = (UtilitiesL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];

    //        bool found;
    //        bool updateL2Cache;
    //        PagedResult<Category> result = DataManager.GetDataFromCache(() => cacheAccessor.GetCategoryHierarchy(hierarchyId), out found, out updateL2Cache);

    //        if (!found)
    //        {
    //            ChannelConfiguration channelConfig = this.Context.GetChannelConfiguration();
    //            string language = String.IsNullOrEmpty(channelConfig.DefaultLanguageId) ? channelConfig.CompanyLanguageId : channelConfig.DefaultLanguageId;

    //            UtilitiesDatabaseAccessor databaseAccessor = (UtilitiesDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
    //            result = databaseAccessor.GetGetCategoryHierarchy(hierarchyId, language);

    //            /* NU
    //            result = databaseAccessor.GetGetCategoryHierarchy(hierarchyId, QueryResultSettings.AllRecords);
    //            // Fill the image values
    //            this.FillCategoryImageProperty(request.ChannelId, result.Results, this.Context);
    //            // Fill in the category name translations
    //            this.FillCategoryNameTranslations(request.ChannelId, result.Results, this.Context);
    //            */

    //            updateL2Cache &= result != null && result.Results.Count < MaxCachedCollectionSize;
    //        }

    //        if (updateL2Cache)
    //            cacheAccessor.PutCategoryHierarchy(hierarchyId, result);

    //        return result;
    //    }

    //    //NU
    //    private void FillCategoryImageProperty(long channelId, ReadOnlyCollection<Category> categories, RequestContext context)
    //    {
    //        const string ImagePropertyName = "IMAGE";

    //        if (categories != null && categories.Any())
    //        {
    //            // get category attributes and associated
    //            GetChannelCategoryAttributesByChannelIdDataRequest getChannelAttributesByChannelIdDataRequest = new GetChannelCategoryAttributesByChannelIdDataRequest(channelId, categories.Select(c => c.RecordId), QueryResultSettings.AllRecords);
    //            PagedResult<AttributeCategory> categoryAttributes = context.Runtime.Execute<EntityDataServiceResponse<AttributeCategory>>(getChannelAttributesByChannelIdDataRequest, context).PagedEntityCollection;

    //            if (categoryAttributes != null && categoryAttributes.Results.Any())
    //            {
    //                // figure out category image attributes and assign them to relevant category object
    //                var imageAttributes = categoryAttributes.Results.Where(ca => string.Compare(ca.Name, ImagePropertyName, System.StringComparison.OrdinalIgnoreCase) == 0);

    //                if (imageAttributes != null &&
    //                    imageAttributes.Any())
    //                {
    //                    var categoriesDictionary = categories.ToDictionary(c => c.RecordId);

    //                    foreach (var imageAttribute in imageAttributes)
    //                    {
    //                        categoriesDictionary[imageAttribute.Category].Images = (imageAttribute.CategoryAttributeValue as AttributeMediaLocationValue).Value;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    // NU
    //    private void FillCategoryNameTranslations(long channelId, IEnumerable<Category> categories, RequestContext context)
    //    {
    //        const string CategoryNameTranslationsCategoryId = "CATEGORY";

    //        var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
    //        {
    //            From = "CATEGORYNAMETRANSLATIONSVIEW",
    //            Where = "CHANNEL = @Channel",
    //            IsQueryByPrimaryKey = false,
    //            OrderBy = "FRIENDLYNAME"
    //        };

    //        query.Parameters["@Channel"] = channelId;

    //        Dictionary<long, IList<Category>> categoriesById = new Dictionary<long, IList<Category>>();
    //        foreach (var category in categories)
    //        {
    //            if (!categoriesById.ContainsKey(category.RecordId))
    //            {
    //                categoriesById[category.RecordId] = new List<Category>();
    //            }

    //            categoriesById[category.RecordId].Add(category);
    //        }

    //        using (var recordIdTableType = new RecordIdTableType(categoriesById.Keys, CategoryNameTranslationsCategoryId))
    //        {
    //            query.Parameters["@TVP_RECIDTABLETYPE"] = recordIdTableType;

    //            // Execute the query
    //            PagedResult<CategoryNameTranslation> results;
    //            using (DatabaseContext databaseContext = new DatabaseContext(context))
    //            {
    //                results = databaseContext.ReadEntity<CategoryNameTranslation>(query);
    //            }

    //            var nameTranslationsDictionary = new Dictionary<long, IList<TextValueTranslation>>();

    //            foreach (CategoryNameTranslation categoryNameTranslation in results.Results)
    //            {
    //                var currentCategoryId = (long)categoryNameTranslation.GetProperty(CategoryNameTranslationsCategoryId);
    //                if (!nameTranslationsDictionary.ContainsKey(currentCategoryId))
    //                {
    //                    nameTranslationsDictionary[currentCategoryId] = new List<TextValueTranslation>();
    //                }

    //                nameTranslationsDictionary[currentCategoryId].Add(new TextValueTranslation { Language = categoryNameTranslation.LanguageId, Text = categoryNameTranslation.FriendlyName });
    //            }

    //            // Populate the Category Name Translations in Category
    //            foreach (Category category in categories)
    //            {
    //                if (nameTranslationsDictionary.ContainsKey(category.RecordId))
    //                {
    //                    category.NameTranslations = nameTranslationsDictionary[category.RecordId];
    //                }
    //            }
    //        }
    //    }

    //    //

    //    public bool GetStoreLocationType(string sectionId, string shelfId)
    //    {
    //        UtilitiesL2CacheAccessor cacheAccessor = (UtilitiesL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];

    //        bool result = false;

    //        int type = cacheAccessor.GetStoreLocationType(sectionId, shelfId);
    //        if (type == 0)
    //        {
    //            UtilitiesDatabaseAccessor databaseAccessor = (UtilitiesDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
    //            result = databaseAccessor.GetStoreLocationType(sectionId, shelfId);
    //            cacheAccessor.PutStoreLocationType(sectionId, shelfId, result);
    //        }
    //        else
    //            result = type > 1;

    //        return result;
    //    }
    //}

}
