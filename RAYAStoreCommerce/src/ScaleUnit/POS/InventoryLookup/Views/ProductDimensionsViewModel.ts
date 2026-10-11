import ko from "knockout";
import { ProxyEntities, ClientEntities } from "PosApi/Entities";
import { ICustomViewControllerContext } from "PosApi/Create/Views";
//import KnockoutExtensionViewModelBase from "./BaseClasses/KnockoutExtensionViewModelBase";
import { Entities } from "../DataService/DataServiceEntities.g";
import { Products } from "../DataService/DataServiceRequests.g";
import { ObjectExtensions, ArrayExtensions, StringExtensions } from "PosApi/TypeExtensions";
//import MessageDialog from "../Controls/DialogControl/MessageDialog";
import { ErrorHelper } from "../../ErrorHelper";

/**
 * The ViewModel for ProductDimensionView.
 */
export default class ProductDimensionsViewModel {
    private _context: ICustomViewControllerContext;

    // Common variables for both product details and kit details
    public product: ko.Observable<ProxyEntities.Product>;
    public simpleProduct: ko.Observable<ProxyEntities.SimpleProduct>;
    public dimensionValue: ko.ObservableArray<any>;
    public selectedVariant: ko.Observable<ProxyEntities.ProductVariant>;
    public productVariantList: ko.ObservableArray<Entities.ProductExVariant>;
    public dimensionList: ProxyEntities.ProductDimensionSet[]
    public dimensionFilterVariantList: ko.ObservableArray<Entities.ProductExVariant>;
    public productName: ko.Observable<string>;

    // Variables for product dimensions
    public bDimensionInfiFilled: ko.Observable<boolean>;
    public bDimension1: ko.Observable<boolean>;
    public bDimension2: ko.Observable<boolean>;
    public bDimension3: ko.Observable<boolean>;
    public bDimension4: ko.Observable<boolean>;

    public dimensionName1: ko.Observable<string>;
    public dimensionName2: ko.Observable<string>;
    public dimensionName3: ko.Observable<string>;
    public dimensionName4: ko.Observable<string>;

    public dimensionList1: ko.ObservableArray<any>;
    public dimensionList2: ko.ObservableArray<any>;
    public dimensionList3: ko.ObservableArray<any>;
    public dimensionList4: ko.ObservableArray<any>;

    public dimList1: ko.ObservableArray<any>;
    public dimList2: ko.ObservableArray<any>;
    public dimList3: ko.ObservableArray<any>;
    public dimList4: ko.ObservableArray<any>;

    public dimensions: ko.ObservableArray<any>;
    public isBlockSale: boolean;
    public static ProductVideoPropertyTypeValue: number = 40;
    private ChannelID: number;

    constructor(context: ICustomViewControllerContext) {
        //super();
        this._context = context;

        // Common variables for both product details and kit details
        this.product = ko.observable(new ProxyEntities.ProductClass());
        this.simpleProduct = ko.observable(new ProxyEntities.SimpleProductClass());

        // Variables for product details
        this.dimensionValue = ko.observableArray([]);

        this.dimensionFilterVariantList = ko.observableArray<Entities.ProductExVariant>([]);
        this.productVariantList = ko.observableArray<Entities.ProductExVariant>([]);
        this.dimensionList = [];
        this.selectedVariant = ko.observable(null);
        this.productName = ko.observable("");
        // Variables for product dimensions
        this.bDimensionInfiFilled = ko.observable(false);
        this.bDimension1 = ko.observable(false);
        this.bDimension2 = ko.observable(false);
        this.bDimension3 = ko.observable(false);
        this.bDimension4 = ko.observable(false);

        this.dimensionName1 = ko.observable("");
        this.dimensionName2 = ko.observable("");
        this.dimensionName3 = ko.observable("");
        this.dimensionName4 = ko.observable("");

        this.dimensionList1 = ko.observableArray([]);
        this.dimensionList2 = ko.observableArray([]);
        this.dimensionList3 = ko.observableArray([]);
        this.dimensionList4 = ko.observableArray([]);

        this.dimList1 = ko.observableArray([]);
        this.dimList2 = ko.observableArray([]);
        this.dimList3 = ko.observableArray([]);
        this.dimList4 = ko.observableArray([]);

        this.dimensions = ko.observableArray([]);
        this.isBlockSale = false;
        this.ChannelID = 0;
        this.getConfiguration();
        this._context.logger.logInformational("ProductDimensionsViewModel: success");
    }

    public LoadFilterVariants(color?: string, size?: string, style?: string, configuration?: string) {

        var bColor: boolean, bSize: boolean, bStyle: boolean, bConfig: boolean;
        var objArrray: Entities.ProductExVariant[] = [], len = this.productVariantList().length;
        this.dimensionFilterVariantList([]);

        for (var i = 0; i < len; i++) {

            var obj = this.productVariantList()[i];

            bColor = color == "" ? true : (obj.ColorId == color);
            bSize = size == "" ? true : (obj.SizeId == size);
            bStyle = style == "" ? true : (obj.StyleId == style);
            bConfig = configuration == "" ? true : (obj.ConfigId == configuration);

            if (bColor && bSize && bStyle && bConfig) {
                objArrray.push(obj);
            }
        }
        this.dimensionFilterVariantList(objArrray);
    }

    public SetActiveVariant(variantId: number) {
        if (this.product().IsMasterProduct) {
            this.selectedVariant(this.product().CompositionInformation.VariantInformation.Variants.filter((v) => v.DistinctProductVariantId == variantId)[0]);
        }
    }

    public getProductDetails(productIds: number[], arrangeProducts?: boolean): Promise<Entities.ProductExClass[]> {
        //let RR: any = Commerce;
        //let ChannelConfig: Commerce.Proxy.Entities.ChannelConfigurationClass = JSON.parse(RR.ApplicationStorage.getItem(RR.ApplicationStorageIDs.CHANNEL_CONFIGURATION_KEY));
        let projectionDomain: Entities.ProjectionDomain = new Entities.ProjectionDomain();
        projectionDomain.ChannelId = this.ChannelID;

        var searchProductCriteria: ProxyEntities.ProductSearchCriteria = {
            Context: projectionDomain,
            Ids: productIds,
            DataLevelValue: 4,
            SkipVariantExpansion: false
        };

        let request: Products.ProductSearchByTextRequest<Products.ProductSearchByTextResponse>
            = new Products.ProductSearchByTextRequest<Products.ProductSearchByTextResponse>(searchProductCriteria, 10, 0);
        //let request: Products.GetProductSearchRequest<Products.GetProductSearchResponse>
        //    = new Products.GetProductSearchRequest<Products.GetProductSearchResponse>(searchProductCriteria, 1);

        return this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Products.ProductSearchByTextResponse>): Entities.ProductExClass[] => {
                if (!result.canceled) {
                    this.product(result.data.result[0].Product)
                    this.fillDimensionInfo(result.data.result[0].Product)
                    let description: string = this.product().Name || this.product().ProductName || this.product().Description;
                    description = StringExtensions.format("{0} : {1}", this.product().ItemId, description);
                    this.productName(description);
                    return result.data.result;
                }
                return [];
            })
            .catch((reason: any): Entities.ProductExClass[] => {
                this._context.logger.logError("Product search error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
                return [];
            });
    }

    public getDimensionValues(RecordId: number, dimension: number, kitVariantResolutionContext: any, matchingDimensionValues: ProxyEntities.ProductDimension[]): Promise<ProxyEntities.ProductDimensionValue[]> {
        let request: Products.GetDimensionValuesRequest<Products.GetDimensionValuesResponse>
            = new Products.GetDimensionValuesRequest<Products.GetDimensionValuesResponse>(this.ChannelID, RecordId, dimension, kitVariantResolutionContext, matchingDimensionValues);
        return this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Products.GetDimensionValuesResponse>) => {
                if (!result.canceled) {
                    return result.data.result;
                }
                return null
            })
            .catch((reason: any): ProxyEntities.ProductDimensionValue[] => {
                this._context.logger.logError("Product dimension error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
                return null;
            });
    }

    public getProductVariantDetails(recordId: number, matchingDimensionValues: ProxyEntities.ProductDimension[]): Promise<ProxyEntities.SimpleProduct[]> {
        // let projectionDomain: Entities.ProjectionDomain = new Entities.ProjectionDomain();
        // projectionDomain.ChannelId = 0;
        

        let request: Products.GetProductVariantRequest<Products.GetProductVariantResponse>
            = new Products.GetProductVariantRequest<Products.GetProductVariantResponse>(this.ChannelID, recordId, matchingDimensionValues);
        return this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Products.GetProductVariantResponse>): ProxyEntities.SimpleProduct[] => {
                if (!result.canceled) {
                    return result.data.result;
                }
                return null
            })
            .catch((reason: any): ProxyEntities.SimpleProduct[] => {
                this._context.logger.logError("Product variant error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
                return null;
            });
    }

    private getConfiguration(): void {
        let str_configuration: string = sessionStorage.getItem("RR_UtilitiesConfiguration");
        let configuration: Entities.UtilitiesConfiguration = JSON.parse(str_configuration);
        this.isBlockSale = false;
        let RR: any = Commerce;
        let ChannelConfig: Commerce.Proxy.Entities.ChannelConfigurationClass = JSON.parse(RR.ApplicationStorage.getItem(RR.ApplicationStorageIDs.CHANNEL_CONFIGURATION_KEY));
        this.ChannelID = ChannelConfig.RecordId;
        if (!ObjectExtensions.isNullOrUndefined(configuration))
            this.isBlockSale = configuration.BlockSales == 1;
    }

    public fillDimensionInfo(product: ProxyEntities.Product) {

        if (product.IsMasterProduct) {
            var variantInformation: ProxyEntities.ProductVariantInformation = product.CompositionInformation.VariantInformation;
            //var Dimensions: ProxyEntities.ProductDimensionSet[] = variantInformation.Dimensions;
            var variants: Entities.ProductExVariant[] = [];
            var Dimensions: ProxyEntities.ProductDimensionSet[] = this.dimensionList;
            let objArray: any = [];

            for (var i = 0; i < variantInformation.Variants.length; i++) {
                variants.push(new Entities.ProductExVariantClass(variantInformation.Variants[i]));
            }
            this.productVariantList(variants);

            for (var dimensionCount = 0; dimensionCount < Dimensions.length; dimensionCount++) {

                this.dimensions.push({ name: Dimensions[dimensionCount].DimensionKey });

                switch (dimensionCount) {
                    case 0:
                        this.bDimension1(true);
                        this.dimensionName1(Dimensions[dimensionCount].DimensionKey);
                        objArray = [];
                        for (var i = 0; i < Dimensions[dimensionCount].DimensionValues.length; i++) {
                            objArray.push(
                                {
                                    dimension: Dimensions[dimensionCount].DimensionKey,
                                    text: Dimensions[dimensionCount].DimensionValues[i].DimensionValue,
                                    value: this.encodeInputStr(Dimensions[dimensionCount].DimensionValues[i].DimensionValue),    // this.getDimensionId(Dimensions[dimensionCount].DimensionKey, Dimensions[dimensionCount].DimensionValues[i].DimensionValue.toString()),
                                    isSelected: i == 0 ? true : false,
                                    idx: i
                                });
                        }
                        this.dimensionList1(objArray);
                        break;
                    case 1:
                        this.bDimension2(true);
                        this.dimensionName2(Dimensions[dimensionCount].DimensionKey);
                        objArray = [];
                        for (var i = 0; i < Dimensions[dimensionCount].DimensionValues.length; i++) {
                            objArray.push(
                                {
                                    dimension: Dimensions[dimensionCount].DimensionKey,
                                    text: Dimensions[dimensionCount].DimensionValues[i].DimensionValue,
                                    value: this.encodeInputStr(Dimensions[dimensionCount].DimensionValues[i].DimensionValue),    // this.getDimensionId(Dimensions[dimensionCount].DimensionKey, Dimensions[dimensionCount].DimensionValues[i].DimensionValue.toString()),
                                    isSelected: i == 0 ? true : false,
                                    idx: i
                                });
                        }
                        objArray = objArray.sort(this.SortByName);
                        this.dimensionList2(objArray);
                        break;
                    case 2:
                        this.bDimension3(true);
                        this.dimensionName3(Dimensions[dimensionCount].DimensionKey);
                        objArray = [];
                        for (var i = 0; i < Dimensions[dimensionCount].DimensionValues.length; i++) {
                            objArray.push(
                                {
                                    dimension: Dimensions[dimensionCount].DimensionKey,
                                    text: Dimensions[dimensionCount].DimensionValues[i].DimensionValue,
                                    value: this.encodeInputStr(Dimensions[dimensionCount].DimensionValues[i].DimensionValue),    // this.getDimensionId(Dimensions[dimensionCount].DimensionKey, Dimensions[dimensionCount].DimensionValues[i].DimensionValue.toString()),
                                    isSelected: i == 0 ? true : false,
                                    idx: i
                                });
                        }
                        this.dimensionList3(objArray);
                        break;
                    case 3:
                        this.bDimension4(true);
                        this.dimensionName4(Dimensions[dimensionCount].DimensionKey);
                        objArray = [];
                        for (var i = 0; i < Dimensions[dimensionCount].DimensionValues.length; i++) {
                            objArray.push(
                                {
                                    dimension: Dimensions[dimensionCount].DimensionKey,
                                    text: Dimensions[dimensionCount].DimensionValues[i].DimensionValue,
                                    value: this.encodeInputStr(Dimensions[dimensionCount].DimensionValues[i].DimensionValue),    // this.getDimensionId(Dimensions[dimensionCount].DimensionKey, Dimensions[dimensionCount].DimensionValues[i].DimensionValue.toString()),
                                    isSelected: i == 0 ? true : false,
                                    idx: i
                                });
                        }
                        this.dimensionList4(objArray);
                        break;
                    default:
                        break;
                }
            }

            this.bDimensionInfiFilled(true);
            this.dimList1(this.dimensionList1());
            this.dimList2(this.dimensionList2());
            this.dimList3(this.dimensionList3());
            this.dimList4(this.dimensionList4());
        }

    }

    public getProductDimensions(recordId: number): void{

        let request: Products.GetDimensionValuesRequest<Products.GetDimensionValuesResponse> = null;
        let strArr: ProxyEntities.ProductDimensionValueSet[] = [];
        let dim: string = '';
        let self = this;
        for (let i = 1; i < 5; i++) {
            request = new Products.GetDimensionValuesRequest<Products.GetDimensionValuesResponse>(this.ChannelID, recordId, i, null, []);
            this._context.runtime.executeAsync(request)
                .then((result: ClientEntities.ICancelableDataResult<Products.GetDimensionValuesResponse>) => {
                    if (!result.canceled) {
                        let resp: ProxyEntities.ProductDimensionValue[] = result.data.result;
                        if (resp.length) {
                            strArr = [];
                            switch (i) {
                                case 1: dim = 'Color'; break;
                                case 2: dim = 'Configuration'; break;
                                case 3: dim = 'Size'; break;
                                case 4: dim = 'Style'; break;
                            }
                            for (let idx = 0; idx < resp.length; idx++) {
                                if (resp[idx].RecordId != 0)
                                    strArr.push(
                                        new ProxyEntities.ProductDimensionValueSetClass({
                                            DimensionValue: resp[idx].Value, VariantSet: []
                                        }));
                            }
                            if (strArr.length) {
                                self.dimensionList.push(
                                    new ProxyEntities.ProductDimensionSetClass({
                                        DimensionKey: dim, DimensionValues: strArr
                                    }));
                                console.log(self.dimensionList);
                            }
                        }
                    }
                })
                .catch((reason: any) => {
                    this._context.logger.logError("Product dimension error: " + JSON.stringify(reason));
                    //ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
                });
        }
    }

    public SortByName(x: any, y: any): any {
        return ((x.text == y.text) ? 0 : ((x.text > y.text) ? 1 : -1));
    }

    public getDimensionId(dimensionType: string, dimensionName: string): string {

        var product: ProxyEntities.Product = this.product();
        var variantCollection: ProxyEntities.ProductVariant[] = product.CompositionInformation.VariantInformation.Variants;
        var dimensionId: string = "", len = variantCollection.length;

        switch (dimensionType) {
            case "Color":
                for (var i = 0; i < len; i++) {
                    if (variantCollection[i].Color === dimensionName) {
                        dimensionId = variantCollection[i].ColorId;
                        break;
                    }
                }
                break;
            case "Size":
                for (var i = 0; i < len; i++) {
                    if (variantCollection[i].Size === dimensionName) {
                        dimensionId = variantCollection[i].SizeId;
                        break;
                    }
                }
                break;
            case "Style":
                for (var i = 0; i < len; i++) {
                    if (variantCollection[i].Style === dimensionName) {
                        dimensionId = variantCollection[i].StyleId;
                        break;
                    }
                }
                break;
            case "Configuration":
                for (var i = 0; i < len; i++) {
                    if (variantCollection[i].Configuration === dimensionName) {
                        dimensionId = variantCollection[i].ConfigId;
                        break;
                    }
                }
                break;
            default:
                break;
        }
        return dimensionId;
    }

    public getVariantId(product: ProxyEntities.Product, dimension: any): number {
        var variantId: number = -1;
        var variantCollection: ProxyEntities.ProductVariant[] = [];

        if (product.IsMasterProduct && !product.IsKit) {
            variantCollection = product.CompositionInformation.VariantInformation.Variants;

            // check if all dimensions are selected
            for (var dimensionCount = 0; dimensionCount < product.CompositionInformation.VariantInformation.Dimensions.length; dimensionCount++) {
                if (!ObjectExtensions.isNullOrUndefined(dimension[dimensionCount].SelectedOption())) {
                    var tempVariantCollection: ProxyEntities.ProductVariant[] = [];
                    variantCollection.forEach((variant) => {
                        if (variant[dimension[dimensionCount].SelectedOption().dimension] == dimension[dimensionCount].SelectedOption().value) {
                            tempVariantCollection.push(variant);
                        }
                    });

                    variantCollection = tempVariantCollection;
                }
                else {
                    break;
                }
            }
            if (ArrayExtensions.hasElements(variantCollection) && variantCollection.length == 1) {
                variantId = variantCollection[0].DistinctProductVariantId;
            }
        } else {
            variantId = product.RecordId;
        }

        return variantId;
    }

    public encodeInputStr(str):string {
        str = str.replace(/\//g, "-");
        str = str.replace(/\\/g, "_");
        str = str.replace(/ /g, "_");
        return str;
    }

    /**
     * Get localized dimension string.
     *
     * @param {string} dimensionType - Type of dimension
     * @return {string} The localized dimension string.
     */
    public GetDimensionTypeString(dimensionType: string): string {
        var OptionCaption: string = "";
        if (!StringExtensions.isNullOrWhitespace(dimensionType)) {
            switch (dimensionType) {
                case "COLOR":
                    OptionCaption = "Select a color";
                    break;
                case "SIZE":
                    OptionCaption = "Select a size";
                    break;
                case "STYLE":
                    OptionCaption = "Select a style";
                    break;
                case "CONFIGURATION":
                    OptionCaption = "Select a configuration";
                    break;
                default:
                    break;
            }
        }
        return OptionCaption;
    }
}