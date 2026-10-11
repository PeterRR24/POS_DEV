import ko from "knockout";
import { ProxyEntities, ClientEntities } from "PosApi/Entities";
import { ICustomViewControllerContext, ICustomViewControllerBaseState } from "PosApi/Create/Views";
//import KnockoutExtensionViewModelBase from "./BaseClasses/KnockoutExtensionViewModelBase";
import { Products } from "../DataService/DataServiceRequests.g";
import { Entities } from "../DataService/DataServiceEntities.g";
//import MessageDialog from "../Controls/DialogControl/MessageDialog";
import { StringExtensions } from "PosApi/TypeExtensions";
import { ErrorHelper } from "../../ErrorHelper";

/**
 * The ViewModel for ProductSearchViewModel.
 */
export default class ProductSearchViewModel {
    private _context: ICustomViewControllerContext;
    private state: ICustomViewControllerBaseState;
    public readonly PageSize: number = 50;// 1000;

    // Busy states
    //public isProductsModeBusy: ko.Observable<boolean>;
    public searchProducts: ko.ObservableArray<Entities.ProductEx>;
    public selectedProducts: ko.ObservableArray<Entities.ProductEx>;
    public searchText: ko.Observable<string>;
    public allowTrans: ko.Observable<boolean>;
    public showMatrix: ko.Observable<boolean>;
    public isBlockSale: boolean;

    constructor(context: ICustomViewControllerContext, vState: ICustomViewControllerBaseState) {
        //super();

        this._context = context;
        this.state = vState;
        //this.isProductsModeBusy = ko.observable(false);
        this.searchProducts = ko.observableArray([]);
        this.selectedProducts = ko.observableArray([]);
        this.searchText = ko.observable("");
        //this.searchProduts();
        //this.ProductSearchByText();
        this.allowTrans = ko.observable(false);
        this.showMatrix = ko.observable(false);
        this.isBlockSale = false;
        this.getConfiguration();
        this._context.logger.logInformational("ProductSearchViewModel: success");
    }

    /**
     * Load data page callback.
     * @param {number} size The number of items to load.
     * @param {number} skip The number of items that were already loaded.
     * @returns {Promise<Entities.ProductEx[]>} The loaded items promise.
     */
    public loadDataPage(size: number, skip: number): Promise<Entities.ProductEx[]> {
        let timeOut: number = skip == 0 ? 0 : 200;
        let promise: Promise<any> = new Promise((resolve: (value?: any) => void) => {
            // Emulate delay of request to server.
            setTimeout(() => {
                this._context.logger.logInformational("dataListPageLoaded");
                let pageData: Promise<Entities.ProductEx[]> = this.ProductSearchByText(size, skip);
                resolve(pageData);
            }, timeOut);
        });
        return promise;
    }

    /*
    public searchProduts() {
        this.isProductsModeBusy(true);

        //let RR: any = Commerce;
        //let ChannelConfig: Commerce.Proxy.Entities.ChannelConfigurationClass = JSON.parse(RR.ApplicationStorage.getItem(RR.ApplicationStorageIDs.CHANNEL_CONFIGURATION_KEY));

        let projectionDomain: Entities.ProjectionDomain = new Entities.ProjectionDomain();
        projectionDomain.ChannelId = 0;

        let productSearchCriteria: ProxyEntities.ProductSearchCriteria = new ProxyEntities.ProductSearchCriteriaClass();
        productSearchCriteria.Context = projectionDomain;
        productSearchCriteria.SearchCondition = !StringExtensions.isEmptyOrWhitespace(this.searchText()) ? this.searchText() : "0";
        productSearchCriteria.DataLevelValue = 1;

        let request: Products.GetProductSearchRequest<Products.GetProductSearchResponse>
            = new Products.GetProductSearchRequest<Products.GetProductSearchResponse>(productSearchCriteria, 1);

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Products.GetProductSearchResponse>) => {
                this.isProductsModeBusy(false);
                if (!result.canceled) {
                    this.searchProducts(result.data.result);
                }
            })
            .catch((reason: any): void => {
                this.isProductsModeBusy(false);
                this._context.logger.logError("Product search error: " + JSON.stringify(reason));
                this.handleErrorObject(reason);
            });

    }
    */

    public ProductSearchByText(size: number, skip: number): Promise<Entities.ProductEx[]> {

        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        let RR: any = Commerce;
        let ChannelConfig: Commerce.Proxy.Entities.ChannelConfigurationClass = JSON.parse(RR.ApplicationStorage.getItem(RR.ApplicationStorageIDs.CHANNEL_CONFIGURATION_KEY));

        let projectionDomain: Entities.ProjectionDomain = new Entities.ProjectionDomain();
        projectionDomain.ChannelId = ChannelConfig.RecordId;

        let productSearchCriteria: ProxyEntities.ProductSearchCriteria = new ProxyEntities.ProductSearchCriteriaClass();
        productSearchCriteria.Context = projectionDomain;
        productSearchCriteria.SearchCondition = !StringExtensions.isEmptyOrWhitespace(this.searchText()) ? this.searchText() : "0";
        productSearchCriteria.DataLevelValue = 1;

        let request: Products.ProductSearchByTextRequest<Products.ProductSearchByTextResponse>
            = new Products.ProductSearchByTextRequest<Products.ProductSearchByTextResponse>(productSearchCriteria, size, skip);

        return this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Products.ProductSearchByTextResponse>): Entities.ProductEx[] => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                if (!result.canceled) {
                    this.searchProducts(result.data.result);
                    return result.data.result;
                } else return [];
            })
            .catch((reason: any): any[] => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Product search error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
                return [];
            });
    }

    public itemClicked(item: Entities.ProductEx): void {
        if (item.IsMasterProduct == true && !item.IsKit)
            this._context.navigator.navigate("ProductDimensionsView", item);
        else {
            //var productDetailsOptions = {
            //    productId: item.Product.RecordId,
            //    isSelectionMode: false,
            //    correlationId: StringExtensions.EMPTY
            //};
            let productDetailsOptions: ClientEntities.SimpleProductDetailsNavigationParameters
                = new ClientEntities.SimpleProductDetailsNavigationParameters(item.Product.RecordId);
            this._context.navigator.navigateToPOSView("SimpleProductDetailsView", productDetailsOptions);
        }
    }
    
    /**
     * Sets the selected items upon which other actions can be performed.
     */
    public setSelectedItems(items: Entities.ProductEx[]): void {
        this.selectedProducts(items);
        if (items.length == 0) {
            this.allowTrans(false);
            this.showMatrix(false);
        }
        else if (items.length == 1) {
            this.allowTrans(true);
            this.showMatrix(true);
        }
        else {
            this.allowTrans(true);
            this.showMatrix(false);
        }
    }

    public ExportPorducts(): void {
        let products: Entities.ProductEx[] = this.searchProducts();
        let requestObj = {
            "FileName": "",
            "Products": JSON.stringify(products)
        };
        let exportRequest: Commerce.HardwareStationDeviceActionRequest<Commerce.HardwareStationDeviceActionResponse> =
            new Commerce.HardwareStationDeviceActionRequest("FILEOPERATION", "ExportProducts", requestObj);

        this._context.runtime.executeAsync(exportRequest)
            .then((resp: any): any => {
                console.log(JSON.stringify(resp));
                let respMsg: string = '';
                if (resp.data.response == true)
                    respMsg = this._context.resources.getString("PRODUCT_EXPORT_SUCCESS_MESSAGE");
                else
                    respMsg = this._context.resources.getString("PRODUCT_EXPORT_FAIL_MESSAGE");
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                ErrorHelper.displayMessageAsync(this._context, respMsg);
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    private getConfiguration(): void {
        let str_configuration: string = sessionStorage.getItem("RR_UtilitiesConfiguration");
        let configuration: Entities.UtilitiesConfiguration = JSON.parse(str_configuration);
        this.isBlockSale = false;
        if (!Commerce.ObjectExtensions.isNullOrUndefined(configuration))
            this.isBlockSale = configuration.BlockSales == 1;
    }

}