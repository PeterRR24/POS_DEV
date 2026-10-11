import ko from "knockout";
import * as NewView from "PosApi/Create/Views";
//import { IExtensionViewControllerContext } from "PosApi/Create/Views";
//import KnockoutExtensionViewControllerBase from "./BaseClasses/KnockoutExtensionViewControllerBase";
import ProductSearchViewModel from "./ProductSearchViewModel";
import { ArrayExtensions, ObjectExtensions, StringExtensions } from "PosApi/TypeExtensions";
import { CurrencyFormatter } from "PosApi/Consume/Formatters";
//import { DataList, IDataListState, SelectionMode } from "PosUISdk/Controls/DataList";
import * as Controls from "PosApi/Consume/Controls";
import { Entities } from "../DataService/DataServiceEntities.g";
//import { HeaderSplitView, IHeaderSplitViewState } from "PosUISdk/Controls/HeaderSplitView";
import { ClientEntities } from "PosApi/Entities";
import { AddItemToCartOperationRequest, AddItemToCartOperationResponse } from "PosApi/Consume/Cart";
import { ErrorHelper } from "../../ErrorHelper";

type ICancelableDataResult<TResult> = ClientEntities.ICancelableDataResult<TResult>;

/**
 * The controller for ProductSearchView.
 */
export default class ProductSearchView extends NewView.CustomViewControllerBase {
    //private _context: IExtensionViewControllerContext;

    public readonly viewModel: ProductSearchViewModel;
    //public readonly dataList: DataList<Entities.ProductExClass>;
    public paginatedDataList: Controls.IPaginatedDataList<Entities.ProductExClass>;
    //public readonly headerSplitView: HeaderSplitView;

    constructor(context: NewView.ICustomViewControllerContext) {
        // Do not save in history
        super(context, {
            title: context.resources.getString("MAX_PROD_SEARCH_PAGE_TITLE"),
            commandBar: {
                commands: [
                    {
                        name: "quickSale",
                        label: 'Sale now',
                        icon: NewView.Icons.Buy,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.QuickSale();
                        }
                    },
                    {
                        name: "addtosale",
                        label: 'Add item',
                        icon: NewView.Icons.Add,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.AddProductToCart();
                        }
                    },
                    {
                        name: "showMatrix",
                        label: 'Item details',
                        icon: NewView.Icons.PageList,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.navigateProdDetails();
                        }
                    },
                    {
                        name: "exportProds",
                        label: context.resources.getString('EXPORT_PRODUCT_BUTTON'),
                        icon: NewView.Icons.CloudUpload,
                        isVisible: true,
                        canExecute: true,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.exportOperation();
                        }
                    }
                ]
            }
        });

        // Initialize the view model.
        this.viewModel = new ProductSearchViewModel(context, this.state);

    }

    /**
     * Bind the html element with view controller.
     *
     * @param {HTMLElement} element DOM element.
     */
    public onReady(element: HTMLElement): void {
        ko.applyBindings(this, element);

        // Initialize datalist source
        let paginatedDataSource: Controls.IPaginatedDataSource<Entities.ProductEx> = {
            pageSize: this.viewModel.PageSize,
            loadDataPage: this.viewModel.loadDataPage.bind(this.viewModel)
        };

        // Initialize datalist options
        let dataListOptions: Readonly<Controls.IPaginatedDataListOptions<Entities.ProductEx>> = {
            columns: [
                {
                    title: this.context.resources.getString("PRODUCT_ID"),
                    ratio: 20,
                    collapseOrder: 1,
                    minWidth: 100,
                    computeValue: (rowData: Entities.ProductEx): string => { return rowData.ItemId; }
                },
                {
                    title: this.context.resources.getString("PRODUCT_NAME"),
                    ratio: 40,
                    collapseOrder: 2,
                    minWidth: 100,
                    computeValue: (rowData: Entities.ProductEx): string => { return rowData.ProductName; }
                },
                {
                    title: this.context.resources.getString("PRODUCT_TYPE"),
                    ratio: 20,
                    collapseOrder: 3,
                    minWidth: 100,
                    computeValue: (rowData: Entities.ProductEx): string => { return rowData.ProductType; }
                },
                {
                    title: this.context.resources.getString("PRODUCT_PRICE"),
                    ratio: 10,
                    collapseOrder: 4,
                    minWidth: 100,
                    computeValue: (rowData: Entities.ProductEx): string => { return CurrencyFormatter.toCurrency(rowData.Price); }
                },
                {
                    title: this.context.resources.getString("PRODUCT_AVAIL_QTY"),
                    ratio: 10,
                    collapseOrder: 5,
                    minWidth: 100,
                    computeValue: (rowData: Entities.ProductEx): string => { return rowData.Quantity.toString(); }
                }
            ],

            data: paginatedDataSource,
            interactionMode: Controls.DataListInteractionMode.MultiSelect,
        };

        // Initialize datalist
        let dataListRootElem: HTMLDivElement = element.querySelector("#productlistview") as HTMLDivElement;
        this.paginatedDataList = this.context.controlFactory.create(this.context.logger.getNewCorrelationId(), "PaginatedDataList", dataListOptions, dataListRootElem);
        this.paginatedDataList.addEventListener("ItemInvoked", (eventData: { item: Entities.ProductEx }) => {
            this.viewModel.itemClicked(eventData.item);
        });
        this.paginatedDataList.addEventListener("SelectionChanged", (eventData: { items: Entities.ProductEx[] }) => {
            this.viewModel.setSelectedItems(eventData.items);
            ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'quickSale').canExecute = this.viewModel.allowTrans();
            ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'addtosale').canExecute = this.viewModel.allowTrans();
            ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'showMatrix').canExecute = this.viewModel.showMatrix();
        });
    }

    /**
     * Called when the object is disposed.
     */
    public dispose(): void {
        ObjectExtensions.disposeAllProperties(this);
    }

    private reloadItemList(): void {
        this.paginatedDataList.reloadData();
        this.viewModel.setSelectedItems([]);
    }

    /**
    * Search button click event handler.
    */
    public searchClickHandler(): void {
        //this.viewModel.searchProduts();
        //this.viewModel.ProductSearchByText();
        this.reloadItemList();
    }

    public navigateProdDetails(): void {
        let items: Entities.ProductEx[] = this.viewModel.selectedProducts();
        if (items.length == 1) {
            if (items[0].IsMasterProduct == true && !items[0].IsKit)
                this.context.navigator.navigate("ProductDimensionsView", items[0]);
            else {
                let productDetailsOptions: ClientEntities.SimpleProductDetailsNavigationParameters
                    = new ClientEntities.SimpleProductDetailsNavigationParameters(items[0].Product.RecordId);
                this.context.navigator.navigateToPOSView("SimpleProductDetailsView", productDetailsOptions);
            }
        }
    }

    public QuickSale(isQuickSale: boolean = true) {
        let products: Entities.ProductEx[] = this.viewModel.selectedProducts();
        let selectedProducts: Entities.ProductEx[] = [];
        let productNames: string = '';

        if (this.viewModel.isBlockSale) {
            for (let idx = 0; idx < products.length; idx++) {
                if (products[idx].IsMasterProduct || products[idx].IsKit) {
                    selectedProducts.push(products[idx]);
                } else if (products[idx].AvailableQuantity > 0) {
                    selectedProducts.push(products[idx]);
                } else {
                    productNames += products[idx].ItemId + '::' + products[idx].ProductName + '\r\n'
                }
            }
            if (selectedProducts.length == 0) return;
        } else
            selectedProducts = products;
        
        //this.viewModel.isProductsModeBusy(true);
        this.state.isProcessing = true;
        var productSaleDetail: ClientEntities.IProductSaleReturnDetails[] = [];
        
        for (var i = 0; i < selectedProducts.length; i++) {
            productSaleDetail.push({
                quantity: 1,
                productId: selectedProducts[i].RecordId,
            });
        }
        // add all simple products to cart
        let addItemToCartOperationRequest: AddItemToCartOperationRequest<AddItemToCartOperationResponse>
            = new AddItemToCartOperationRequest<AddItemToCartOperationResponse>(productSaleDetail, this.context.logger.getNewCorrelationId());
        this.context.runtime.executeAsync(addItemToCartOperationRequest)
            .then((getgddItemToCartOperationResponse: ICancelableDataResult<AddItemToCartOperationResponse>) => {
                //this.viewModel.isProductsModeBusy(false);
                this.state.isProcessing = false;
                if (!getgddItemToCartOperationResponse.canceled) {
                    if (isQuickSale && StringExtensions.isEmpty(productNames))
                        this.context.navigator.navigateToPOSView("CartView");
                    else if (isQuickSale && !StringExtensions.isEmpty(productNames)) {
                        ErrorHelper.displayMessageAsync(this.context, StringExtensions.format(this.context.resources.getString('PRODUCT_QUANTITY_ZERO_ALERT'), productNames))
                            .then(() => {
                                this.context.navigator.navigateToPOSView("CartView");
                            });
                    } else if (!isQuickSale && !StringExtensions.isEmpty(productNames)) {
                        ErrorHelper.displayMessageAsync(this.context, StringExtensions.format(this.context.resources.getString('PRODUCT_QUANTITY_ZERO_ALERT'), productNames));
                    }
                }                
            }).catch((err: any) => {
                //this.viewModel.isProductsModeBusy(false);
                this.state.isProcessing = false;
                ErrorHelper.displayLocalizedErrorAsync(this.context, err);
            });
    }

    public exportOperation(): void{
        this.viewModel.ExportPorducts();
    }

    /**
         * Add to cart button handler
         *
         */
    public AddProductToCart() {
        this.QuickSale(false);
    }

    /**
     * Occurs when the element of the page is created.
     *
     * @param {HTMLElement} element DOM element.
     */
    public onCreated(element: HTMLElement) {



    }

    public onShown() {

    }


}
