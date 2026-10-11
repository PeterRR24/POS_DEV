import ko from "knockout";
import * as NewView from "PosApi/Create/Views";
import ItemLabelPrintWzrdViewModel from "./ItemLabelPrintWzrdViewModel";
import { ArrayExtensions, ObjectExtensions } from "PosApi/TypeExtensions";
import { CurrencyFormatter } from "PosApi/Consume/Formatters";
import * as Controls from "PosApi/Consume/Controls";
import { Entities } from "../DataService2/DataServiceEntities.g";
import AddLabelDialogModule from "../Controls/DialogControl/AddLabelDialogModule";
import { IAddLabelDialogResult } from "../Controls/DialogControl/IAddLabelDialogResult";
import ConfirmMessageDialogModule from "../Controls/DialogControl/ConfirmMessageDialogModule";
import { IConfirmMessageDialogResult } from "../Controls/DialogControl/IConfirmMessageDialogResult";


/**
 * The controller for ItemLabelPrintWzrdView.
 */
export default class ItemLabelPrintWzrdView extends NewView.CustomViewControllerBase {
    //private _context: IExtensionViewControllerContext;

    public readonly viewModel: ItemLabelPrintWzrdViewModel;
    public paginatedDataList: Controls.IPaginatedDataList<Entities.LabelItem>;
    public fromDatePicker: Controls.IDatePicker;
    public toDatePicker: Controls.IDatePicker;
    public priceDatePicker: Controls.IDatePicker;
    public page0Visible: ko.Computed<boolean>;
    public page1Visible: ko.Computed<boolean>;
    public page2Visible: ko.Computed<boolean>;
    public page3Visible: ko.Computed<boolean>;
    public currentPage: ko.Observable<number>;
    public prevPage: ko.Observable<number>;
    public btnNextEnable: ko.Computed<boolean>;
    public btnBackEnable: ko.Computed<boolean>;
    public btnFinishEnable: ko.Computed<boolean>;
    public isProductHierarchy: ko.Computed<boolean>;
    public isLocationHierarchy: ko.Computed<boolean>;
    public isItemFilter: ko.Computed<boolean>;
    public isTree: ko.Computed<boolean>;
    //public selectedLblTypeValue: Observable<string>;
    public selectedLblTypeText: ko.Computed<string>;
    public selectedLblCodeText: ko.Computed<string>;
    public isShowWizard: ko.Observable<boolean>;
    public arrLabelTypes: ko.ObservableArray<any>;
    //private isTextSelected: ko.Observable<boolean>;

    constructor(context: NewView.ICustomViewControllerContext) {

        super(context, {
            title: context.resources.getString("MAX_LABEL_PAGE_TITLE"),
            commandBar: {
                commands: [
                    {
                        name: "importBarcodes",
                        label: context.resources.getString('CMD_IMPORT_BUTTON'),
                        icon: NewView.Icons.ProductList,
                        isVisible: true,
                        canExecute: true,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.importBarcodeOperation();
                        }
                    },
                    {
                        name: "changelabelQuantity",
                        label: context.resources.getString('CMD_EDIT_QUANTITY'),
                        icon: NewView.Icons.Edit,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.changeQuantityOperation();
                        }
                    },
                    {
                        name: "searchLabelByFilter",
                        label: context.resources.getString('CMD_PRINT_WIZARD_TEXT'),
                        icon: NewView.Icons.Filter,
                        isVisible: true,
                        canExecute: true,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.searchLabelByFilterOperation();
                        }
                    },
                    {
                        name: "searchLabelByBarcode",
                        label: context.resources.getString('CMD_PRINT_BY_BARCODE_TEXT'),
                        icon: NewView.Icons.Search,
                        isVisible: true,
                        canExecute: true,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.searchByBarcodeOperation();
                        }
                    },
                    {
                        name: "printlabelitems",
                        label: context.resources.getString('CMD_PRINT_TEXT'),
                        icon: NewView.Icons.Print,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.printLabelsOperation();
                        }
                    },
                    {
                        name: "deletelabelitems",
                        label: context.resources.getString('CMD_DELETE_TEXT'),
                        icon: NewView.Icons.Delete,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.deleteItemOperation();
                        }
                    },
                    {
                        name: "clearlabelitems",
                        label: context.resources.getString('CMD_CLEAR_TEXT'),
                        icon: NewView.Icons.ClearSelection,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.clearOperation();
                        }
                    }
                ]
            }
        });

        // Initialize the view model.
        this.viewModel = new ItemLabelPrintWzrdViewModel(context, this.state);
        
        this.isShowWizard = ko.observable(true);
        //this.isTextSelected = ko.observable(false);

        this.currentPage = ko.observable(0);
        this.prevPage = ko.observable(0);

        this.page0Visible = ko.computed(() => this.currentPage() === 0);
        this.page1Visible = ko.computed(() => this.currentPage() === 1);
        this.page2Visible = ko.computed(() => this.currentPage() === 2);
        this.page3Visible = ko.computed(() => this.currentPage() === 3);

        this.btnNextEnable = ko.computed(() => this.currentPage() != 3 && this.viewModel.enableWizardNext()); 
        this.btnBackEnable = ko.computed(() => this.currentPage() != 1);
        this.btnFinishEnable = ko.computed(() => this.currentPage() == 3);
        
        this.arrLabelTypes = ko.observableArray([
            //{ LabelType: "", Value: 2 },
            { LabelType: "Product", Value: 1 },
            { LabelType: "Shelf", Value: 0 }
        ]);

        //this.selectedLblTypeValue = ko.observable('0');
        var self = this;
        this.selectedLblTypeText = ko.computed(function () {
            return self.viewModel.labelType() === 1 ? 'Product' : (self.viewModel.labelType() === 0 ? 'Shelf' : '');
        }, this);
        this.selectedLblCodeText = ko.computed(function () {
            return (self.viewModel.selectedLC() == null || self.viewModel.selectedLC().Id == null)
                ? self.context.resources.getString("NONE")
                : self.viewModel.selectedLC().Code.toString() + ' : ' + self.viewModel.selectedLC().Description.toString();
        }, this);
        this.isProductHierarchy = ko.computed(() => this.viewModel.selectedOptionValue() === 'CategoryHierarchy');
        this.isLocationHierarchy = ko.computed(() => this.viewModel.selectedOptionValue() === 'LocationHierarchy');
        this.isItemFilter = ko.computed(() => this.viewModel.selectedOptionValue() === 'ItemFilter');
        this.isTree = ko.computed(function () {
            //return self.currentPage() === 2 && self.viewModel.selectedOptionValue() !== 'ItemFilter';
            return false;
        }, this);

    }

    /**
     * Bind the html element with view controller.
     *
     * @param {HTMLElement} element DOM element.
     */
    public onReady(element: HTMLElement): void {
        ko.applyBindings(this, element);
        // Initialize datalist source
        let paginatedDataSource: Controls.IPaginatedDataSource<Entities.LabelItem> = {
            pageSize: this.viewModel.PageSize,
            loadDataPage: this.viewModel.loadDataPage.bind(this.viewModel)
        };

        // Initialize datalist options
        let dataListOptions: Readonly<Controls.IPaginatedDataListOptions<Entities.LabelItem>> = {
            columns: [
                {
                    title: this.context.resources.getString("LABEL_ITEM_ID"),
                    ratio: 10,
                    collapseOrder: 9,
                    minWidth: 90,
                    computeValue: (rowData: Entities.LabelItem): string => { return rowData.ItemId; }
                },
                {
                    title: this.context.resources.getString("LABEL_ITEM_NAME"),
                    ratio: 28,
                    collapseOrder: 5,
                    minWidth: 100,
                    computeValue: (rowData: Entities.LabelItem): string => { return rowData.Name; }
                },
                {
                    title: this.context.resources.getString("LABEL_ITEM_PRICE_ORIGINAL"),
                    ratio: 8,
                    collapseOrder: 6,
                    minWidth: 90,
                    computeValue: (rowData: Entities.LabelItem): string => { return CurrencyFormatter.toCurrency(rowData.UnitPriceOld); }
                },
                {
                    title: this.context.resources.getString("LABEL_ITEM_PRICE"),
                    ratio: 8,
                    collapseOrder: 7,
                    minWidth: 90,
                    computeValue: (rowData: Entities.LabelItem): string => { return CurrencyFormatter.toCurrency(rowData.UnitPrice); }
                },
                {
                    title: this.context.resources.getString("LABEL_ITEM_DIM_COLOR"),
                    ratio: 10,
                    collapseOrder: 4,
                    minWidth: 90,
                    computeValue: (rowData: Entities.LabelItem): string => { return rowData.DimColorName || rowData.DimColorId; }
                },
                {
                    title: this.context.resources.getString("LABEL_ITEM_DIM_CONFIG"),
                    ratio: 10,
                    collapseOrder: 3,
                    minWidth: 90,
                    computeValue: (rowData: Entities.LabelItem): string => { return rowData.DimConfigName || rowData.DimConfigId; }
                },
                {
                    title: this.context.resources.getString("LABEL_ITEM_DIM_STYLE"),
                    ratio: 10,
                    collapseOrder: 2,
                    minWidth: 90,
                    computeValue: (rowData: Entities.LabelItem): string => { return rowData.DimStyleName || rowData.DimStyleId; }
                },
                {
                    title: this.context.resources.getString("LABEL_ITEM_DIM_SIZE"),
                    ratio: 10,
                    collapseOrder: 1,
                    minWidth: 90,
                    computeValue: (rowData: Entities.LabelItem): string => { return rowData.DimSizeName || rowData.DimSizeId; }
                },
                {
                    title: this.context.resources.getString("LABEL_ITEM_LABELS"),
                    ratio: 6,
                    collapseOrder: 8,
                    minWidth: 70,
                    computeValue: (rowData: Entities.LabelItem): string => { return rowData.NoOfLabels.toString(); }
                }
            ],
            
            data: paginatedDataSource,
            interactionMode: Controls.DataListInteractionMode.MultiSelect,
        };

        // Initialize datalist
        let dataListRootElem: HTMLDivElement = element.querySelector("#labelitemsview") as HTMLDivElement;
        this.paginatedDataList = this.context.controlFactory.create(this.context.logger.getNewCorrelationId(), "PaginatedDataList", dataListOptions, dataListRootElem);
        this.paginatedDataList.addEventListener("SelectionChanged", (eventData: { items: Entities.LabelItem[] }) => {
            this.viewModel.setSelectedItems(eventData.items);
            ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'changelabelQuantity').canExecute = this.viewModel.allowEdit();
            ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'deletelabelitems').canExecute = this.viewModel.allowDelete();
        });
        this.paginatedDataList.addEventListener("ItemInvoked", (eventData: { item: Entities.LabelItem }) => {
            this.viewModel.noOfLabelEditor(eventData.item);
        });

        // Initialize DatePicker options
        let datePickerOptios: Controls.IDatePickerOptions = { date: new Date(), enabled: true };
        // Initialize DatePicker
        //let divFromDate: HTMLDivElement = element.querySelector("#divFromDate") as HTMLDivElement;
        //let divToDate: HTMLDivElement = element.querySelector("#divToDate") as HTMLDivElement;
        let divPriceDate: HTMLDivElement = element.querySelector("#divPriceDate") as HTMLDivElement;
        //this.fromDatePicker = this.context.controlFactory.create(this.context.logger.getNewCorrelationId(), "DatePicker", datePickerOptios, divFromDate);
        //this.fromDatePicker.addEventListener("DateChanged", (eventData: { date: Date }) => {
        //    this.viewModel.fromDate(eventData.date);
        //});
        //this.toDatePicker = this.context.controlFactory.create(this.context.logger.getNewCorrelationId(), "DatePicker", datePickerOptios, divToDate);
        //this.toDatePicker.addEventListener("DateChanged", (eventData: { date: Date }) => {
        //    this.viewModel.toDate(eventData.date);
        //});
        this.priceDatePicker = this.context.controlFactory.create(this.context.logger.getNewCorrelationId(), "DatePicker", datePickerOptios, divPriceDate);
        this.priceDatePicker.addEventListener("DateChanged", (eventData: { date: Date }) => {
            this.viewModel.priceDate(eventData.date);
        });

        ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'clearlabelitems').canExecute = this.viewModel.allowClear();
        ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'printlabelitems').canExecute = this.viewModel.allowPrint();

        this.viewModel.labelitemsQuantityHandler = (() => {
            this.LabelItemQuantityDialogActivity();
        });
        this.viewModel.refreshLabelListHandler = (() => {
            this.reloadItemList();
            ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'clearlabelitems').canExecute = this.viewModel.allowClear();
            ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'printlabelitems').canExecute = this.viewModel.allowPrint();
        });
        
    }

    /**
     * Called when the object is disposed.
     */
    public dispose(): void {
        ObjectExtensions.disposeAllProperties(this);
    }

    /**
     * Occurs when the element of the page is created.
     *
     * @param {HTMLElement} element DOM element.
     */
    //public onCreated(element: HTMLElement) {
        
    //    this.viewModel.labelitemsQuantityHandler = (() => {
    //        this.LabelItemQuantityDialogActivity();
    //    });
    //    this.viewModel.refreshLabelListHandler = (() => {
    //        this.reloadItemList();
    //    });
    //}
    
    public onShown() {

        this.isShowWizard(false);
        $('.category ul').hide();
        //let nodeList: NodeListOf<HTMLScriptElement> = <NodeListOf<HTMLScriptElement>>document.querySelectorAll('.category ul');
        //for (var i = 0; i < nodeList.length; i++) {
        //    nodeList[i].style.display = 'none';
        //}
        //$('.rootCategory').on('click', 'label', function () {
        //    $('.rootCategory label').removeClass('accentBackground');
        //    $(this).addClass("accentBackground");
        //});
        let rooNodes: NodeListOf<HTMLScriptElement> = <NodeListOf<HTMLScriptElement>>document.querySelectorAll('.rootCategory');
        for (var i = 0; i < rooNodes.length; i++) {
            rooNodes[i].addEventListener('click', function (e) {
                let element = <HTMLScriptElement>e.target;
                if (element && element.tagName.toLowerCase() === 'label') {
                    let eleTags: NodeListOf<HTMLScriptElement> = <NodeListOf<HTMLScriptElement>>document.querySelectorAll('.rootCategory label');
                    for (var i = 0; i < eleTags.length; i++) {
                        eleTags[i].classList.remove("accentBackground");
                    }
                    element.classList.add("accentBackground");
                }
            });
        }

    }
    

    /**
     * Load data page callback.
     * @param {number} size The number of items to load.
     * @param {number} skip The number of items that were already loaded.
     * @returns {Promise<Entities.LabelItem[]>} The loaded items promise.
     */
    //private loadDataPage(size: number, skip: number): Promise<Entities.LabelItem[]> {
    //    let promise: Promise<any> = new Promise((resolve: (value?: any) => void) => {
    //        // Emulate delay of request to server.
    //        setTimeout(() => {
    //            this.context.logger.logInformational("dataListPageLoaded");
    //            let pageData: Entities.LabelItem[] = this.viewModel.searchItems();
    //            //let pageData: Promise<Entities.LabelItem[]> = this.viewModel.searchLabelItems(size, skip);
    //            resolve(pageData);
    //        }, 1000);
    //    });
    //    this.viewModel.Skip(skip);
    //    return promise;
    //}

    private reloadItemList(): void {
        this.paginatedDataList.reloadData();
        this.viewModel.selectedLabelItems([]);
    }
    /**
     * Search button click event handler.
     */
    public searchClickHandler(): void {
        this.reloadItemList();
    }

    //private fromDateChangedHandler(newDate: Date): void {
    //    this.viewModel.fromDate(newDate);
    //}

    //private toDateChangedHandler(newDate: Date): void {
    //    this.viewModel.toDate(newDate);
    //}

    //private priceDateChangedHandler(newDate: Date): void {
    //    this.viewModel.priceDate(newDate);
    //}
    
    public SelectCatHandler(self, data, element): void {
        //var src: HTMLElement = element.currentTarget;
        //$(src).siblings('ul').slideToggle('slow');
        self.viewModel.categoryId(data.value.RecordId);   // data.value.ParentCategory
        self.viewModel.hierarchyId(0);        // data.value.RecordId
        self.viewModel.enableWizardNext(true);
    }

    public ToggleCatHandler(self, data, element): void {
        var src: HTMLElement = element.currentTarget;
        $(src).siblings('ul').slideToggle('slow');
        //self.viewModel.categoryId(data.value.RecordId);   // data.value.ParentCategory
        //self.viewModel.hierarchyId(0);        // data.value.RecordId
        //self.viewModel.enableWizardNext(true);
    }

    public ToggleLocHandler(self, data, element): void {
        var src: HTMLElement = element.currentTarget;
        $(src).siblings('ul').slideToggle('slow');
        //self.viewModel.sectionId(data.SectionId);
        //self.viewModel.shelfId(data.ShelfId);
        //self.viewModel.enableWizardNext(true);
    }

    public SelectLocHandler(self, data, element): void {
        //var src: HTMLElement = element.currentTarget;
        //$(src).siblings('ul').slideToggle('slow');
        self.viewModel.sectionId(data.SectionId);
        self.viewModel.shelfId(data.ShelfId);
        self.viewModel.enableWizardNext(true);
    }
    
    //public selectLabelType() {
    //    this.currentPage(0);
    //}

    public AddItemHandler(): void {
        
        this.viewModel.calculateItemBarcode();
    }

    public OkHandler(): void {

        let no = this.prevPage() == 0 ? 1 : this.prevPage();
        this.currentPage(no);
        this.isShowWizard(true);
        this.viewModel.enableWizardNext(true);
        //this.viewModel.searchLabelItems();
    }

    public BackHandler(): void {

        var no = this.currentPage();
        this.prevPage(no - 1);
        this.currentPage(no - 1);
        this.viewModel.enableWizardNext(true);
    }

    public NextHandler(): void {

        var no = this.currentPage();
        this.prevPage(no + 1);
        this.currentPage(no + 1);

        if (this.currentPage() == 0) {
            
            this.currentPage(1);
        } else if (this.currentPage() == 2 && this.isProductHierarchy()) {

            this.viewModel.getCategoryList();
            this.viewModel.enableWizardNext(false);
            if (this.viewModel.categoryId() != null) {
                this.viewModel.enableWizardNext(true);
            }
        } else if (this.currentPage() == 2 && this.isLocationHierarchy()) {

            this.viewModel.getStoreSections();
            this.viewModel.getStoreShelves();
            this.viewModel.enableWizardNext(false);
            if (this.viewModel.sectionId() != "") {
                this.viewModel.enableWizardNext(true);
            }
        } else if (this.currentPage() == 2 && this.isItemFilter()) {
            this.viewModel.enableWizardNext(false);
            if (this.viewModel.itemFilterText() != "") {
                this.viewModel.enableWizardNext(true);
            }
            else {
                // focus on item filter
                let eleTags: HTMLScriptElement = <HTMLScriptElement>document.querySelector('input.item-filter');
                eleTags.focus();
            }
        } else {
            this.viewModel.enableWizardNext(false);
        }
    }

    public CancelWizardHandler(): void {

        this.isShowWizard(false);
    }

    public FinishHandler(): void {
        
        this.viewModel.calculateItemsOnWizard();
        this.isShowWizard(false);
    }

    public CancelPrintHandler(): void {

        this.viewModel.printStatDialog(false);

    }

    public OkPrintHandler(): void {

        this.viewModel.printSelectedLabelLayout();

    }

    public selectAllStatistics(mngr, element) {

        var chk: boolean = element.target.checked;
        var stats = mngr.viewModel.labelItemStatistics();

        for (var i: number = 0; i < stats.length; i++) {
            if (stats[i].IsEnable()) {
                stats[i].IsSelect(chk);
            }
        }
        mngr.viewModel.labelItemStatistics([]);
        mngr.viewModel.labelItemStatistics(stats);
        return true;
    }

    public clearOperation(): void {

        let dialog: ConfirmMessageDialogModule = new ConfirmMessageDialogModule();
        dialog.open(this.context.resources.getString("CONFIRM_CLEAR_LABEL_ITEMS"))
            .then((result: IConfirmMessageDialogResult) => {
                if (result.selectedValue !== 'Closed') {
                    this.viewModel.clear();
                    this.viewModel.searchText()
                }
            }).catch((reason: any) => {
                this.context.logger.logError("ItemLabelPrintWzrdView clearOperation confirmation: " + JSON.stringify(reason));
            });
        
    }

    public deleteItemOperation(): void {
        if (!this.viewModel.allowDelete())  return;
        let dialog: ConfirmMessageDialogModule = new ConfirmMessageDialogModule();
        dialog.open(this.context.resources.getString("CONFIRM_DELETE_LABEL_ITEM"))
            .then((result: IConfirmMessageDialogResult) => {
                if (result.selectedValue !== 'Closed') {
                    this.viewModel.deleteItem();
                }
            }).catch((reason: any) => {
                this.context.logger.logError("ItemLabelPrintWzrdView deleteItemOperation confirmation: " + JSON.stringify(reason));
            });
    }

    public printLabelsOperation(): void {
        if (this.viewModel.selectedLabelItems().length == 1) {
            this.viewModel.printSelectedLabels();
        }
        else if (this.viewModel.selectedLabelItems().length > 1) {
            this.viewModel.calculateStatistics();
        }
        else {
            this.viewModel.calculateStatistics();
        }        
    }
    
    public searchByBarcodeOperation(): void {
        this.viewModel.showWizard(false);
        this.showlabelPrintWizardOperation();
    }

    public searchLabelByFilterOperation(): void {
        this.viewModel.showWizard(true);
        this.showlabelPrintWizardOperation();
    }

    private showlabelPrintWizardOperation(): void {

        $('.category ul').hide();
        $(".rootCategory label").removeClass("accentBackground");

        let eleTags: NodeListOf<HTMLScriptElement> = <NodeListOf<HTMLScriptElement>>document.querySelectorAll('.rootCategory label');
        for (var i = 0; i < eleTags.length; i++) {
            eleTags[i].classList.remove("accentBackground");
        }

        this.isShowWizard(true);
        this.viewModel.resetWizardParameter();

        this.prevPage(1);
        this.currentPage(1);
        this.viewModel.enableWizardNext(true);
        this.viewModel.barCodeNotFoundMsg('');
        this.viewModel.itemBarCode('');
        this.viewModel.barCodeLabelQuantity('');
    }

    public changeQuantityOperation(): void {
        this.LabelItemQuantityDialogActivity();
    }

    private LabelItemQuantityDialogActivity(): void {
        var item: Entities.LabelItem = this.viewModel.selectedLabelItems()[0];
        if (item == null && !this.viewModel.allowEdit()) return;
        let dialog: AddLabelDialogModule = new AddLabelDialogModule(this.viewModel.labelType(), false);
        dialog.open(this.context.resources.getString('PRINT_LABEL_QTY'), item.Description, item.NoOfLabels + "", [])
            .then((result: IAddLabelDialogResult) => {

                if (result.selectedValue !== 'Closed') {

                    var quantity: number = parseInt(result.selectedValue);
                    if (isNaN(quantity)) {
                        this.context.logger.logError("Please enter numbers only!");
                        return;
                    }
                    if (quantity <= 0) {
                        this.context.logger.logError("The label printing quantity cannot be zero or negative");
                        return;
                    }
                    this.viewModel.updatelabelitem(quantity)
                }

            }).catch((reason: any) => {
                this.context.logger.logError("ItemLabelPrintWzrdView.openTemplatedDialog: " + JSON.stringify(reason));
            });

    }

    public importBarcodeOperation(): void {
        document.getElementById('barCodeFilePath').click();
    }

    public FileDialogHandler(self, evt): void {
        let file: any = evt.currentTarget.files[0];

        this.getFileData(file)
            .then(data => {
                if (typeof data === 'string' || data instanceof String) {
                    let ipStrs: string[] = data.split('\r\n');
                    console.log(ipStrs);
                    let barCodes: string[] = ipStrs.map(n => n.trim());
                    if (!barCodes.length)
                        return;

                    let dialog: AddLabelDialogModule = new AddLabelDialogModule(this.viewModel.labelType(), true, 0, true, false);

                    dialog.open(this.context.resources.getString('PRINT_LABEL_QTY'), "", "1", this.viewModel.labelCodes())
                        .then((result: IAddLabelDialogResult) => {
                            this.context.logger.logInformational("Dialog result: " + result.selectedValue);
                            if (result.selectedValue == 'Closed') {
                                return;
                            }
                            let labelCode: Entities.LabelCode = result.labelCode;
                            var labelType: number = parseInt(result.itemLabelType);
                            console.log(labelType);
                            let labelCalcParam: Entities.LabelItemCalculateParams = new Entities.LabelItemCalculateParams();
                            labelCalcParam.CodeRecId = isNaN(labelCode.Id) ? 0 : labelCode.Id;
                            labelCalcParam.PriceDate = result.priceDate;

                            this.viewModel.calculateImportBarcodes(labelType, barCodes, labelCalcParam);

                        }).catch((reason: any) => {
                            this.context.logger.logError("Dialog error: " + JSON.stringify(reason));
                        });

                }
            }).then(() => {
                (<HTMLInputElement>document.getElementById('barCodeFilePath')).value = '';
                
            });

    }

    private getFileData(file): Promise<any> {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.readAsText(file);
            reader.onload = () => resolve(reader.result);
            reader.onerror = error => reject(error);
        });
    }

}
