import ko from "knockout";
import * as NewView from "PosApi/Create/Views";
import ProductDimensionsViewModel from "./ProductDimensionsViewModel";
import { Entities } from "../DataService/DataServiceEntities.g";
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import { ArrayExtensions, ObjectExtensions, StringExtensions } from "PosApi/TypeExtensions";
//import * as Menu from "PosUISdk/Controls/Menu";
import * as Controls from "PosApi/Consume/Controls";
import { AddItemToCartOperationRequest, AddItemToCartOperationResponse } from "PosApi/Consume/Cart";
//import MessageDialog from "../Controls/DialogControl/MessageDialog";
import { ErrorHelper } from "../../ErrorHelper";

type ICancelableDataResult<TResult> = ClientEntities.ICancelableDataResult<TResult>;

export default class ProductDimensionsView extends NewView.CustomViewControllerBase {
    private _context: NewView.ICustomViewControllerContext;
    public readonly viewModel: ProductDimensionsViewModel;
    //public readonly headerSplitView: HeaderSplitView;
    public menu: Controls.IMenu;
    public menu2: Controls.IMenu;
    private _productDetailOptions: any;
    //public indeterminateWaitVisible: ko.Observable<boolean>;
    public localizedProductName: ko.Observable<string>;
    public localizedProductDescription: ko.Observable<string>;
    //private _isProductLoaded: ko.Observable<boolean>
    public _selectedVariants: ko.ObservableArray<any>;
    public filteredVariants: ko.ObservableArray<any>;
    public localOptions: ko.Observable<any>;
    public allowTrans: ko.Observable<boolean>;
    public frequentlySelectedVariant: ko.Observable<string>;
    public CanQuantityNegative: ko.Observable<boolean>;
    public alternateDimensions: ko.ObservableArray<any>;
    public menuAnchor: ko.Observable<HTMLElement>;
    public isMenuVisible: ko.Observable<boolean>;
    public isSizeVisible: ko.Observable<boolean>;
    public isColorVisible: ko.Observable<boolean>;
    public isStyleVisible: ko.Observable<boolean>;
    public isConfigurationVisible: ko.Observable<boolean>;
    public productSaleDetail: ClientEntities.IProductSaleReturnDetails[] = [];
    public totalVariants: number = 0;
    private isItemSelected: boolean;
    //private isSetDimensionMenu2: boolean;

    private readonly COLOR: string = 'Color';
    private readonly SIZE: string = 'Size';
    private readonly STYLE: string = 'Style';
    private readonly CONFIGURATION: string = 'Configuration';
    //private _productSubscription: any;

    //private _productNameField: string = "ProductName";
    //private _productDescriptionField: string = "Description";

    constructor(context: NewView.ICustomViewControllerContext, options: any) {
        // Do not save in history
        super(context, {
            title: context.resources.getString("MAX_ILKP_PAGE_TITLE"),
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
                    }
                ]
            }
        });

        this._context = context;

        // Initialize the view model.
        this.viewModel = new ProductDimensionsViewModel(context);
        
        //this.indeterminateWaitVisible = ko.observable(true);

        this._productDetailOptions = ko.observable(options);
        this.localizedProductName = ko.observable("");
        this.localizedProductDescription = ko.observable("");
        //this._isProductLoaded = ko.observable(false);
        this._selectedVariants = ko.observableArray([]);
        this.filteredVariants = ko.observableArray([]);
        this.localOptions = ko.observable({});
        this.allowTrans = ko.observable(false);
        this.frequentlySelectedVariant = ko.observable("");
        this.CanQuantityNegative = ko.observable(false);
        this.alternateDimensions = ko.observableArray([]);
        this.menuAnchor = ko.observable(null);
        this.isMenuVisible = ko.observable(false);
        this.isSizeVisible = ko.observable(false);
        this.isColorVisible = ko.observable(false);
        this.isStyleVisible = ko.observable(false);
        this.isConfigurationVisible = ko.observable(false);
        this.isItemSelected = false;
        //this.isSetDimensionMenu2 = false;

        if (!ObjectExtensions.isNullOrUndefined(this._productDetailOptions().RecordId)) {
            this.viewModel.getProductDimensions(this._productDetailOptions().RecordId);
            this.state.isProcessing = true;
            this.viewModel.getProductDetails([this._productDetailOptions().RecordId], true)
                .then((products: Entities.ProductExClass[]) => {
                    // this._productDetailOptions().product = products[0].Product;
                    // this.localizeProductProperties(products[0]);
                    this.loadVariantList();
                    //this._isProductLoaded(true);
                    this.state.isProcessing = false;
                });
        }
    }

    public onReady(element: HTMLElement): void {
        ko.applyBindings(this, element);

        let menuOptions: Controls.IMenuOptions = {
            directionalHint: Controls.DirectionalHint.BottomCenter,
            type: 'button',
            commands: [
                {
                    id: 'Color',
                    label: 'Color',
                    isVisible: true
                },
                {
                    id: 'Style',
                    label: 'Style',
                    isVisible: true
                },
                {
                    id: 'Size',
                    label: 'Size',
                    isVisible: true
                },
                {
                    id: 'Configuration',
                    label: 'Configuration',
                    isVisible: true
                }
            ]
        };
        let menuElem: HTMLDivElement = element.querySelector("#buttonInteractionMenu") as HTMLDivElement;
        let menuElem2: HTMLDivElement = element.querySelector("#buttonInteractionMenu2") as HTMLDivElement;
        this.menu = this.context.controlFactory.create(this.context.logger.getNewCorrelationId(), "Menu", menuOptions, menuElem);
        this.menu2 = this.context.controlFactory.create(this.context.logger.getNewCorrelationId(), "Menu", menuOptions, menuElem2);
        this.menu.addEventListener("CommandInvoked", (eventData: { id: string }) => {
            this.switchDimensionHandler(eventData.id, this, null);
        });
        this.menu2.addEventListener("CommandInvoked", (eventData: { id: string }) => {
            this.switchDimensionHandler(eventData.id, this, null);
        });
        
    }

    public loadVariantList() {
        //this.indeterminateWaitVisible(true);
        this.state.isProcessing = true;
        var color = "", size = "", style = "", config = "";

        if (this.viewModel.bDimension1()) {
            color = "", size = "", style = "", config = "";
        }

        if (this.viewModel.bDimension2()) {
            color = "", size = "", style = "", config = "";
        }

        let dimValue: string = '';
        if (this.viewModel.bDimension3()) {
            let dimList3 = this.viewModel.dimensionList3()[0];
            dimValue = dimList3.text;
            switch (dimList3.dimension) {
                case this.COLOR:
                    color = dimValue;
                    break;
                case this.SIZE:
                    size = dimValue;
                    break;
                case this.STYLE:
                    style = dimValue;
                    break;
                case this.CONFIGURATION:
                    config = dimValue;
                    break;
                default:
                    break;
            }
        }

        if (this.viewModel.bDimension4()) {
            let dimList4 = this.viewModel.dimensionList4()[0];
            dimValue = dimList4.text;
            switch (dimList4.dimension) {
                case this.COLOR:
                    color = dimValue;
                    break;
                case this.SIZE:
                    size = dimValue;
                    break;
                case this.STYLE:
                    style = dimValue;
                    break;
                case this.CONFIGURATION:
                    config = dimValue;
                    break;
                default:
                    break;
            }
        }
        this.localOptions({ Color: color, Size: size, Style: style, Config: config });
        this.viewModel.LoadFilterVariants(color, size, style, config);
        this.loadFilterVariants();
    }

    private loadFilterVariants() {
        var list = this.viewModel.dimensionFilterVariantList(), len1 = list.length;
        var firstList = this.viewModel.dimensionList1(), len2 = firstList.length;
        //var filter2 = this.viewModel.dimensionList2();
        var filterList = [], me = this;

        for (var i = 0; i < len2; i++) {

            var groupHeaderItem = firstList[i], secondlevelItems = [];
            let gpHeaderName: string = groupHeaderItem.dimension; // == this.CONFIGURATION ? 'ConfigId' : groupHeaderItem.dimension + 'Id';
            let gpHeaderNameID: string = groupHeaderItem.dimension == this.CONFIGURATION ? 'ConfigId' : groupHeaderItem.dimension + 'Id';
            //secondlevelItems = list.filter((c, i, a) => { return c[groupHeaderItem.dimension] === groupHeaderItem.text; });
            for (var x = 0; x < len1; x++) {
                if (list[x][gpHeaderName] === groupHeaderItem.text || list[x][gpHeaderNameID] === groupHeaderItem.text) {
                    secondlevelItems.push(list[x]);
                }
            }

            if (me.viewModel.bDimension2()) {
                switch (me.viewModel.dimensionName2()) {
                    case this.COLOR:
                        secondlevelItems.sort(me.SortByColor);
                        break;
                    case this.SIZE:
                        secondlevelItems.sort(me.SortBySize);
                        break;
                    case this.STYLE:
                        secondlevelItems.sort(me.SortByStyle);
                        break;
                    case this.CONFIGURATION:
                        secondlevelItems.sort(me.SortByConfig);
                        break;
                    default:
                        break;
                }
            }
            var groupItems = { groupTxt: groupHeaderItem.text, groupVal: this.viewModel.encodeInputStr(groupHeaderItem.text), itemList: secondlevelItems, secondFilter: me.viewModel.dimensionList2(), b_filter2: me.viewModel.bDimension2() };
            filterList.push(groupItems);
        }
        this.filteredVariants(filterList);
        //this.indeterminateWaitVisible(false);
        this.state.isProcessing = false;
    }

    public dimensionHandler(data: any) {

        this.clearDimensionSelection(data);
        var temp = this.localOptions();
        let dimValue: string = data.text;
        switch (data.dimension) {
            case this.COLOR:
                this.localOptions({ Color: dimValue, Size: temp.Size, Style: temp.Style, Config: temp.Config });
                break;
            case this.SIZE:
                this.localOptions({ Color: temp.Color, Size: dimValue, Style: temp.Style, Config: temp.Config });
                break;
            case this.STYLE:
                this.localOptions({ Color: temp.Color, Size: temp.Size, Style: dimValue, Config: temp.Config });
                break;
            case this.CONFIGURATION:
                this.localOptions({ Color: temp.Color, Size: temp.Size, Style: temp.Style, Config: dimValue });
                break;
            default:
                break;
        }
        temp = this.localOptions();
        this.viewModel.LoadFilterVariants(temp.Color, temp.Size, temp.Style, temp.Config);
        this.loadFilterVariants();
    }

    public variantHandler(model: this, action: string, xelement: string, yelement: string, data: Entities.ProductExVariant, ev: Event) {

        if (model.viewModel.isBlockSale) {
            // block negative sale
            if (!(data.Quantity > 0) || (data.Quantity == data.TransQuantity))
                return;
        }

        var filterList: any[] = [];
        var bool: boolean = false;//, qty: number = data.Quantity - data.TransQuantity;

        //if (qty <= 0 && !model.CanQuantityNegative()) {
        //    Commerce.ViewModelAdapter.displayMessage("Quantity become negative!", MessageType.Info, MessageBoxButtons.Default);
        //    return;
        //}

        $.each(model.filteredVariants(), function (index, item) {

            var idx = item.itemList.indexOf(data);
            if (idx != -1) {
                data.TransQuantity = action === "add" ? data.TransQuantity + 1 : 0;
                item.itemList[idx] = data;
            }
            if (data.TransQuantity > 0) bool = true;
            var newItem = { groupTxt: item.groupTxt, groupVal: item.groupVal, itemList: item.itemList, secondFilter: item.secondFilter, b_filter2: item.b_filter2 };
            filterList.push(newItem);
        });
        model.filteredVariants([]);
        model.filteredVariants(filterList);
        model.allowTrans(bool);
        model.frequentlySelectedVariant(data.VariantId);

        let $elementx = $("#" + xelement);
        let $elementy: any;
        if (model.viewModel.bDimension2())
            $elementy = $("#" + yelement);

        $elementx.removeClass("accentBackground").addClass("secondaryBackgroundColor");
        //ko.applyBindingsToNode($elementx[0], { groupVal: data[model.viewModel.dimensionName1()] }, model);
        if (model.viewModel.bDimension2()) {
            $elementy.removeClass("accentBackground").addClass("secondaryBackgroundColor");
            //ko.applyBindingsToNode($elementy[0], data[model.viewModel.dimensionName2()], model);
        }

        setTimeout(() => {
            $elementx.addClass("accentBackground").removeClass("secondaryBackgroundColor");
            //ko.applyBindingsToNode($elementx[0], { groupVal: data[model.viewModel.dimensionName1()] }, model);
            if (model.viewModel.bDimension2()) {
                $elementy.addClass("accentBackground").removeClass("secondaryBackgroundColor");
                //ko.applyBindingsToNode($elementy[0], data[model.viewModel.dimensionName2()], model);
            }
        }, 150);

        if (!this.isItemSelected) {
            ArrayExtensions.firstOrUndefined(model.state.commandBar.commands, (c) => c.name === 'quickSale').canExecute = model.allowTrans();
            ArrayExtensions.firstOrUndefined(model.state.commandBar.commands, (c) => c.name === 'addtosale').canExecute = model.allowTrans();
        }
        this.isItemSelected = true;
    }

    private clearDimensionSelection(data: any) {
        var objArray = [];
        switch (data.dimension) {
            case this.viewModel.dimensionName1():
                objArray = this.viewModel.dimensionList1();
                this.viewModel.dimensionList1([]);
                for (var i = 0; i < objArray.length; i++) {
                    objArray[i].isSelected = objArray[i].value == data.value ? true : false;
                }
                this.viewModel.dimensionList1(objArray);
                break;
            case this.viewModel.dimensionName2():
                objArray = this.viewModel.dimensionList2();
                this.viewModel.dimensionList2([]);
                for (var i = 0; i < objArray.length; i++) {
                    objArray[i].isSelected = objArray[i].value == data.value ? true : false;
                }
                this.viewModel.dimensionList2(objArray);
                break;
            case this.viewModel.dimensionName3():
                objArray = this.viewModel.dimensionList3();
                this.viewModel.dimensionList3([]);
                for (var i = 0; i < objArray.length; i++) {
                    objArray[i].isSelected = objArray[i].value == data.value ? true : false;
                }
                this.viewModel.dimensionList3(objArray);
                break;
            case this.viewModel.dimensionName4():
                objArray = this.viewModel.dimensionList4();
                this.viewModel.dimensionList4([]);
                for (var i = 0; i < objArray.length; i++) {
                    objArray[i].isSelected = objArray[i].value == data.value ? true : false;
                }
                this.viewModel.dimensionList4(objArray);
                break;
            default:
                break;
        }
    }

    public dimensionOrderHandler(data: string, isSwitch: boolean) {

        let control = document.getElementById("dimensionFlyout")['winControl'],
            arr: any = [], objArray = [],
            dochange = false;

        if (data == "apply") {
            if (!isSwitch) {
                var list = $("#dimensionNameList h4");
                for (var i = 0; i < list.length; i++) {
                    var item = list[i];
                    arr.push({ name: item.innerText });
                    if (!dochange) {
                        dochange = !(item.innerText == this.viewModel.dimensions()[i].name)
                    }
                }
                this.viewModel.dimensions(arr);
            } else {
                arr = this.viewModel.dimensions();
                dochange = true;
            }

            if (dochange) {

                for (var i = 0; i < arr.length; i++) {
                    switch (arr[i].name) {
                        case this.viewModel.dimensionName1():
                            switch (i) {
                                case 0:
                                    this.viewModel.dimensionList1(this.viewModel.dimList1());
                                    break;
                                case 1:
                                    this.viewModel.dimensionList2(this.viewModel.dimList1());
                                    break;
                                case 2:
                                    this.viewModel.dimensionList3(this.viewModel.dimList1());
                                    break;
                                case 3:
                                    this.viewModel.dimensionList4(this.viewModel.dimList1());
                                    break;
                            }
                            break;
                        case this.viewModel.dimensionName2():
                            switch (i) {
                                case 0:
                                    this.viewModel.dimensionList1(this.viewModel.dimList2());
                                    break;
                                case 1:
                                    this.viewModel.dimensionList2(this.viewModel.dimList2());
                                    break;
                                case 2:
                                    this.viewModel.dimensionList3(this.viewModel.dimList2());
                                    break;
                                case 3:
                                    this.viewModel.dimensionList4(this.viewModel.dimList2());
                                    break;
                            }
                            break;
                        case this.viewModel.dimensionName3():
                            switch (i) {
                                case 0:
                                    this.viewModel.dimensionList1(this.viewModel.dimList3());
                                    break;
                                case 1:
                                    this.viewModel.dimensionList2(this.viewModel.dimList3());
                                    break;
                                case 2:
                                    this.viewModel.dimensionList3(this.viewModel.dimList3());
                                    break;
                                case 3:
                                    this.viewModel.dimensionList4(this.viewModel.dimList3());
                                    break;
                            }
                            break;
                        case this.viewModel.dimensionName4():
                            switch (i) {
                                case 0:
                                    this.viewModel.dimensionList1(this.viewModel.dimList4());
                                    break;
                                case 1:
                                    this.viewModel.dimensionList2(this.viewModel.dimList4());
                                    break;
                                case 2:
                                    this.viewModel.dimensionList3(this.viewModel.dimList4());
                                    break;
                                case 3:
                                    this.viewModel.dimensionList4(this.viewModel.dimList4());
                                    break;
                            }
                            break;
                    }
                }

                for (var x = 0; x < arr.length; x++) {
                    objArray = [];
                    switch (x) {
                        case 0:
                            this.viewModel.dimensionName1(arr[x].name)
                            break;
                        case 1:
                            this.viewModel.dimensionName2(arr[x].name)
                            this.viewModel.dimensionList2(this.viewModel.dimensionList2().sort(this.viewModel.SortByName));
                            break;
                        case 2:
                            this.viewModel.dimensionName3(arr[x].name)
                            objArray = this.viewModel.dimensionList3();
                            this.viewModel.dimensionList3([]);
                            for (var i = 0; i < objArray.length; i++) {
                                objArray[i].isSelected = i == 0 ? true : false;
                            }
                            this.viewModel.dimensionList3(objArray);
                            break;
                        case 3:
                            this.viewModel.dimensionName4(arr[x].name)
                            objArray = this.viewModel.dimensionList4();
                            this.viewModel.dimensionList4([]);
                            for (var i = 0; i < objArray.length; i++) {
                                objArray[i].isSelected = i == 0 ? true : false;
                            }
                            this.viewModel.dimensionList4(objArray);
                            break;
                    }
                }
                this.viewModel.dimList1(this.viewModel.dimensionList1());
                this.viewModel.dimList2(this.viewModel.dimensionList2());
                this.viewModel.dimList3(this.viewModel.dimensionList3());
                this.viewModel.dimList4(this.viewModel.dimensionList4());
                this.loadVariantList();
            }
        }
        control.hide();
    }

    public dimensionPositioHandler(data: any, src: any, ev: Event) {
        var btn = $(ev.currentTarget),
            item = btn.parents('.dimension-grid');

        if (data == 'up') {
            var prev = item.prev(); if (prev.length == 0) return;

            prev.css('z-index', 999).css('position', 'relative').animate({ top: item.outerHeight(true) }, 250);
            item.css('z-index', 1000).css('position', 'relative').animate({ top: '-' + prev.outerHeight(true) }, 300, function () {
                prev.css('z-index', '').css('top', '').css('position', '');
                item.css('z-index', '').css('top', '').css('position', '');
                item.insertBefore(prev);
            });

        } else {
            var next = item.next(); if (next.length == 0) return;

            next.css('z-index', 999).css('position', 'relative').animate({ top: '-' + item.outerHeight(true) }, 250);
            item.css('z-index', 1000).css('position', 'relative').animate({ top: next.outerHeight(true) }, 300, function () {
                next.css('z-index', '').css('top', '').css('position', '');
                item.css('z-index', '').css('top', '').css('position', '');
                item.insertAfter(next);
            });

        }
    }

    // Show the flyout for all dimensions
    public showDimensionPopup() {
        this.viewModel.dimensions(this.viewModel.dimensions());
        let btn = document.getElementById("popupButton");
        let winControl = document.getElementById("dimensionFlyout")['winControl'];
        winControl.show(btn);
    }

    public switchDimensionHandler(data: any, self: any, ev: any) {
        var oldValue = $('#hidDimension').val(),
            newValue = data,
            arr = self.viewModel.dimensions();
        //flyOut = document.getElementById("alternateDimensionFlyout");

        self.viewModel.dimensions([]);
        for (var i = 0; i < arr.length; i++) {
            if (arr[i].name == oldValue) {
                arr[i].name = newValue;
            } else if (arr[i].name == newValue) {
                arr[i].name = oldValue;
            }
        }
        self.viewModel.dimensions(arr);
        self.dimensionOrderHandler('apply', true);

        //flyOut.hide();

    }

    /**
     * Callback for menu command.
     * @param {IMenuCommandClickArgs} args Menu command arguments.
     */
    //public menuCommandClick(args: Menu.IMenuCommandClickArgs): void {
    //    this.context.logger.logInformational("menuCommandClick: " + args.id);
    //}

    // Show the flyout for alternate dimensions
    public alternateDimensionPopup(dimension: string, self: this, data: any, ev) {
        var arr = self.viewModel.dimensions();//,
        self.isMenuVisible(false);
        self.isColorVisible(false);
        self.isSizeVisible(false);
        self.isStyleVisible(false);
        self.isConfigurationVisible(false);
        self.alternateDimensions([]);
        self.alternateDimensions(arr.filter(function (item) { return item.name !== dimension }));
        for (var i = 0; i < self.alternateDimensions().length; i++) {
            switch (self.alternateDimensions()[i].name) {
                case self.COLOR:
                    self.isColorVisible(true);
                    break;
                case self.SIZE:
                    self.isSizeVisible(true);
                    break;
                case self.STYLE:
                    self.isStyleVisible(true);
                    break;
                case self.CONFIGURATION:
                    self.isConfigurationVisible(true);
                    break;
            }
        }
        
        self.showAvilableDimentions(self, self.menu2);
        self.menu.show(<HTMLElement>event.currentTarget);
        $('#hidDimension').val(dimension);
    }

    public alternateDimensionPopup2(dimension: string, self: this, data: any, ev) {
        var arr = self.viewModel.dimensions();//,
        self.isMenuVisible(false);
        self.isColorVisible(false);
        self.isSizeVisible(false);
        self.isStyleVisible(false);
        self.isConfigurationVisible(false);
        self.alternateDimensions([]);
        self.alternateDimensions(arr.filter(function (item) { return item.name !== dimension }));
        for (var i = 0; i < self.alternateDimensions().length; i++) {
            switch (self.alternateDimensions()[i].name) {
                case self.COLOR:
                    self.isColorVisible(true);
                    break;
                case self.SIZE:
                    self.isSizeVisible(true);
                    break;
                case self.STYLE:
                    self.isStyleVisible(true);
                    break;
                case self.CONFIGURATION:
                    self.isConfigurationVisible(true);
                    break;
            }
        }
        
        self.showAvilableDimentions(self, self.menu2);
        self.menu2.show(<HTMLElement>event.currentTarget);
        $('#hidDimension').val(dimension);
    }

    private showAvilableDimentions(self: this, menu: Controls.IMenu) {
        ArrayExtensions.firstOrUndefined(menu.menuCommands, (c) => c.id === 'Color').isVisible = self.isColorVisible();
        ArrayExtensions.firstOrUndefined(menu.menuCommands, (c) => c.id === 'Style').isVisible = self.isStyleVisible();
        ArrayExtensions.firstOrUndefined(menu.menuCommands, (c) => c.id === 'Size').isVisible = self.isSizeVisible();
        ArrayExtensions.firstOrUndefined(menu.menuCommands, (c) => c.id === 'Configuration').isVisible = self.isConfigurationVisible();
    }

    /**
         * Localize the product name and description in case there are translations availalbe in AX that match current app language set.
         */
    //public localizeProductProperties(product: ProxyEntities.Product) {
    //    var lProductName: string;
    //    var lProductDescription: string;

    //    if (!ObjectExtensions.isNullOrUndefined(product.ProductProperties)
    //        && ArrayExtensions.hasElements(product.ProductProperties)) {
    //        var translatedProperties: ProxyEntities.ProductPropertyTranslation[];
    //        translatedProperties = product.ProductProperties.filter((value: ProxyEntities.ProductPropertyTranslation) => {
    //            return !StringExtensions.isNullOrWhitespace(value.TranslationLanguage)
    //                && !StringExtensions.isNullOrWhitespace(Windows.Globalization.ApplicationLanguages.primaryLanguageOverride)
    //                && (!StringExtensions.compare(value.TranslationLanguage, Windows.Globalization.ApplicationLanguages.primaryLanguageOverride, true));
    //        });
    //        if (!ObjectExtensions.isNullOrUndefined(translatedProperties)
    //            && ArrayExtensions.hasElements(translatedProperties)) {
    //            lProductName = this.getProductProperty(translatedProperties[0].TranslatedProperties, this._productNameField);
    //            lProductDescription = this.getProductProperty(translatedProperties[0].TranslatedProperties, this._productDescriptionField);
    //        }
    //    }

    //    if (StringExtensions.isNullOrWhitespace(lProductName)) {
    //        lProductName = product.ProductName;
    //    }

    //    if (StringExtensions.isNullOrWhitespace(lProductDescription)) {
    //        lProductDescription = product.Description;
    //    }
    //    //if (product.Rules) {
    //    //    this.CanQuantityNegative(product.Rules.CanQuantityBecomeNegative);
    //    //}
    //    this.localizedProductName(lProductName);
    //    this.localizedProductDescription(lProductDescription);
    //}

    //private getProductProperty(productProperties: ProxyEntities.ProductProperty[], propertyName: string): string {
    //    var propertValue: string;

    //    var result = $.grep(productProperties, function (e: ProxyEntities.ProductProperty) { return (!StringExtensions.compare(e.KeyName, propertyName, true)); }, false);

    //    if (ArrayExtensions.hasElements(result)) {
    //        propertValue = result[0].ValueString;
    //    }

    //    return propertValue;
    //}

    public variantListSelectionChanged(items) {
        var variants: number[] = [];
        items.forEach(function (list, ind, _ary) {
            variants.push(list.DistinctVariant);
        });
        //var numItemsSelected = variants.length;
        this._selectedVariants(variants);
    }

    ///**
    // * Add to cart button handler
    // *
    // */
    public QuickSale(isQuickSale: boolean = true) {
        //this.indeterminateWaitVisible(true);
        this.state.isProcessing = true;
        this.processCartDetails(isQuickSale);
    }

    public processCartDetails(isQuickSale: boolean) {
        var self = this;
        $.each(this.viewModel.productVariantList(), function (index, item: Entities.ProductExVariant) {
            if (item.TransQuantity > 0) {
                var dimensions: ProxyEntities.ProductDimension[] = [];
                var quantity: number = 0;
                let color: string = StringExtensions.isEmpty(item.Color) || StringExtensions.isNullOrWhitespace(item.Color) ? item.ColorId : item.Color;
                let size: string = StringExtensions.isEmpty(item.Size) || StringExtensions.isNullOrWhitespace(item.Size) ? item.SizeId : item.Size;
                let style: string = StringExtensions.isEmpty(item.Style) || StringExtensions.isNullOrWhitespace(item.Style) ? item.StyleId : item.Style;
                let configuration: string = StringExtensions.isEmpty(item.Configuration) || StringExtensions.isNullOrWhitespace(item.Configuration) ? item.ConfigId : item.Configuration;
                if (!StringExtensions.isEmptyOrWhitespace(color)) {
                    dimensions.push({ DimensionTypeValue: 1, DimensionValue: { RecordId: 0, Value: color } });
                }
                if (!StringExtensions.isEmptyOrWhitespace(size)) {
                    dimensions.push({ DimensionTypeValue: 3, DimensionValue: { RecordId: 0, Value: size } });
                }
                if (!StringExtensions.isEmptyOrWhitespace(style)) {
                    dimensions.push({ DimensionTypeValue: 4, DimensionValue: { RecordId: 0, Value: style } });
                }
                if (!StringExtensions.isEmptyOrWhitespace(configuration)) {
                    dimensions.push({ DimensionTypeValue: 2, DimensionValue: { RecordId: 0, Value: configuration } });
                }
                quantity += item.TransQuantity;
                if (!ObjectExtensions.isNullOrUndefined(self.viewModel.product())) {
                    var selectedProductDimension: ProxyEntities.ProductDimension[] = [];
                    // get first dimesions record id
                    return self.viewModel.getDimensionValues(self.viewModel.product().RecordId, dimensions[0].DimensionTypeValue, null, null)
                        .then((productDimensions: ProxyEntities.ProductDimensionValue[]) => {
                            for (let i = 0; i < productDimensions.length; i++) {
                                if (dimensions[0].DimensionValue.Value == productDimensions[i].Value) {
                                    selectedProductDimension.push({
                                        DimensionTypeValue: dimensions[0].DimensionTypeValue,
                                        DimensionValue: productDimensions[i]
                                    });
                                    break;
                                }
                            }
                            if (dimensions.length > 1) {
                                // get second dimesions record id
                                return self.viewModel.getDimensionValues(self.viewModel.product().RecordId, dimensions[1].DimensionTypeValue, null, selectedProductDimension)
                                    .then((productDimensions2: ProxyEntities.ProductDimensionValue[]) => {
                                        for (let i = 0; i < productDimensions2.length; i++) {
                                            if (dimensions[1].DimensionValue.Value == productDimensions2[i].Value) {
                                                selectedProductDimension.push({
                                                    DimensionTypeValue: dimensions[1].DimensionTypeValue,
                                                    DimensionValue: productDimensions2[i]
                                                });
                                                break;
                                            }
                                        }
                                        // if dimensions are three
                                        if (dimensions.length > 2) {
                                            // get third dimesions record id
                                            return self.viewModel.getDimensionValues(self.viewModel.product().RecordId, dimensions[2].DimensionTypeValue, null, selectedProductDimension)
                                                .then((productDimensions3: ProxyEntities.ProductDimensionValue[]) => {
                                                    for (let i = 0; i < productDimensions3.length; i++) {
                                                        if (dimensions[2].DimensionValue.Value == productDimensions3[i].Value) {
                                                            selectedProductDimension.push({
                                                                DimensionTypeValue: dimensions[2].DimensionTypeValue,
                                                                DimensionValue: productDimensions3[i]
                                                            });
                                                            break;
                                                        }
                                                    }
                                                    // if dimensions are more then 3
                                                    if (dimensions.length > 3) {
                                                        // get third dimesions record id
                                                        return self.viewModel.getDimensionValues(self.viewModel.product().RecordId, dimensions[3].DimensionTypeValue, null, selectedProductDimension)
                                                            .then((productDimensions4: ProxyEntities.ProductDimensionValue[]) => {
                                                                for (let i = 0; i < productDimensions4.length; i++) {
                                                                    if (dimensions[3].DimensionValue.Value == productDimensions4[i].Value) {
                                                                        selectedProductDimension.push({
                                                                            DimensionTypeValue: dimensions[3].DimensionTypeValue,
                                                                            DimensionValue: productDimensions4[i]
                                                                        });
                                                                        break;
                                                                    }
                                                                }
                                                                return self.AddItemToCart(selectedProductDimension, isQuickSale, quantity);

                                                            }).catch((err: any) => {
                                                                //self.indeterminateWaitVisible(false);
                                                                self.state.isProcessing = false;
                                                                return Promise.resolve();
                                                            });
                                                    }
                                                    else
                                                        return self.AddItemToCart(selectedProductDimension, isQuickSale, quantity);

                                                }).catch((err: any) => {
                                                    //self.indeterminateWaitVisible(false);
                                                    self.state.isProcessing = false;
                                                    return Promise.resolve();
                                                });
                                        }
                                        else
                                            return self.AddItemToCart(selectedProductDimension, isQuickSale, quantity);

                                    }).catch((err: any) => {
                                        //self.indeterminateWaitVisible(false);
                                        self.state.isProcessing = false;
                                        return Promise.resolve();
                                    });
                            } else {
                                return self.AddItemToCart(selectedProductDimension, isQuickSale, quantity);
                            }
                        }).catch((err: any) => {
                            //self.indeterminateWaitVisible(false);
                            self.state.isProcessing = false;
                            return Promise.resolve();
                        });
                }
                else
                    return null;
            }
            else
                return null;
        });
    }

    private AddItemToCart(selectedProductDimension: ProxyEntities.ProductDimension[], isQuickSale: boolean, quantity: number): Promise<any> {
        var self = this;
        // get Variants by dimnesion details
        return self.viewModel.getProductVariantDetails(self.viewModel.product().RecordId, selectedProductDimension)
            .then((simpleProducts: ProxyEntities.SimpleProduct[]) => {
                self.productSaleDetail.push({
                    product: simpleProducts[0],
                    quantity: quantity,
                    productId: simpleProducts[0].RecordId,
                });

                // get count of selected variants
                if (self.totalVariants == 0) {
                    for (var i = 0; i < self.viewModel.productVariantList().length; i++) {
                        if (self.viewModel.productVariantList()[i].TransQuantity > 0)
                            self.totalVariants += 1;
                    }
                }

                if (self.productSaleDetail.length == self.totalVariants) {
                    // add all simple products to cart
                    let addItemToCartOperationRequest: AddItemToCartOperationRequest<AddItemToCartOperationResponse> = new AddItemToCartOperationRequest<AddItemToCartOperationResponse>(self.productSaleDetail, self._context.logger.getNewCorrelationId());
                    return self._context.runtime.executeAsync(addItemToCartOperationRequest)
                        .then((getgddItemToCartOperationResponse: ICancelableDataResult<AddItemToCartOperationResponse>) => {
                            //self.indeterminateWaitVisible(false);
                            self.state.isProcessing = false;
                            if (!getgddItemToCartOperationResponse.canceled && isQuickSale) {
                                self._context.navigator.navigateToPOSView("CartView");
                            } else {
                                self._context.navigator.navigateBack();
                            }
                        }).catch((err: any) => {
                            //self.indeterminateWaitVisible(false);
                            self.state.isProcessing = false;
                            //self.viewModel.handleErrorObject(err);
                            ErrorHelper.displayLocalizedErrorAsync(self._context, err);
                        });
                }

                return Promise.resolve();

            }).catch((err: any) => {
                //this.indeterminateWaitVisible(false);
                self.state.isProcessing = false;
                return Promise.resolve();
            });
    }


    /**
         * Add to cart button handler
         *
         */
    public AddProductToCart() {
        this.QuickSale(false);
    }

    private SortByColor(x: any, y: any): any {
        return ((x.Color == y.Color) ? 0 : ((x.Color > y.Color) ? 1 : -1));
    }

    private SortByStyle(x: any, y: any): any {
        return ((x.Style == y.Style) ? 0 : ((x.Style > y.Style) ? 1 : -1));
    }

    private SortBySize(x: any, y: any): any {
        return ((x.Size == y.Size) ? 0 : ((x.Size > y.Size) ? 1 : -1));
    }

    private SortByConfig(x: any, y: any): any {
        return ((x.Configuration == y.Configuration) ? 0 : ((x.Configuration > y.Configuration) ? 1 : -1));
    }


    /**
     * Occurs when the element of the page is created.
     *
     * @param {HTMLElement} element DOM element.
     */
    public onCreated(element: HTMLElement) {



    }

    /**
     * Called when the object is disposed.
     */
    public dispose(): void {
        ObjectExtensions.disposeAllProperties(this);
    }

    public onShown() {
        //this._productSubscription = this.viewModel.product.subscribe((newValue) => {
        //}, this);
    }
}