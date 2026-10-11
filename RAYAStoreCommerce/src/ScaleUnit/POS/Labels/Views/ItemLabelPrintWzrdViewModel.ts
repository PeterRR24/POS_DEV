import ko from "knockout";
import { ProxyEntities, ClientEntities } from "PosApi/Entities";
import { ICustomViewControllerContext, ICustomViewControllerBaseState } from "PosApi/Create/Views";
import { ObjectExtensions, StringExtensions } from "PosApi/TypeExtensions";
import { Labels } from "../DataService2/DataServiceRequests.g";
import { Entities } from "../DataService2/DataServiceEntities.g";
import { HardwareStationDeviceActionRequest, HardwareStationDeviceActionResponse } from "PosApi/Consume/Peripherals";
//import MessageDialog from "../Controls/DialogControl/MessageDialog";
import ConfirmMessageDialogModule from "../Controls/DialogControl/ConfirmMessageDialogModule";
import { IConfirmMessageDialogResult } from "../Controls/DialogControl/IConfirmMessageDialogResult";
import { ErrorHelper } from "../../ErrorHelper";

/**
 * The ViewModel for ItemLabelPrintWzrdView.
 */
export default class ItemLabelPrintWzrdViewModel {
    private _context: ICustomViewControllerContext;
    private state: ICustomViewControllerBaseState;
    public readonly PageSize: number = 100;

    public searchText: ko.Observable<string>;
    public clearLabelItemSelection: ko.Observable<boolean>;
    public labelType: ko.Observable<number>;
    public labelItemStatistics: ko.ObservableArray<any>;
    public categories: ko.ObservableArray<ProxyEntities.Category>;
    public categoryTree: ko.ObservableArray<any>;
    public allowPrint: ko.Computed<boolean>;
    public allowClear: ko.Computed<boolean>;
    public allowCommit: ko.Observable<boolean>;
    public allowDelete: ko.Computed<boolean>;
    public allowEdit: ko.Computed<boolean>;
    public printStatDialog: ko.Observable<boolean>;
    public storeSections: ko.ObservableArray<Entities.StoreSection>;
    public storeShelves: ko.ObservableArray<Entities.StoreShelf>;
    public locationTree: ko.ObservableArray<any>;
    public showWizard: ko.Observable<boolean>;
    public itemBarCode: ko.Observable<string>;
    public barCodeLabelQuantity: ko.Observable<string>;
    public barCodeNotFoundMsg: ko.Observable<string>;
    //public criteria: Observable<Entities.LabelItemCalculateCriteria>;

    // Wizard parameters
    public itemFilterText: ko.Observable<string>;
    public itemQuantity: ko.Observable<number>;
    public selectedOptionValue: ko.Observable<string>;
    public categoryId: ko.Observable<number>;
    public hierarchyId: ko.Observable<number>;
    public sectionId: ko.Observable<string>;
    public shelfId: ko.Observable<string>;
    public selectedQtySource: ko.Observable<string>;
    public changeTracking: ko.Observable<string>;
    public searchItems: ko.ObservableArray<Entities.LabelItem>;
    public selectedLabelItems: ko.ObservableArray<Entities.LabelItem>;
    public labelCodes: ko.ObservableArray<Entities.LabelCode>;
    public selectedLC: ko.Observable<Entities.LabelCode>;
    public fromDate: ko.Observable<Date>;
    public toDate: ko.Observable<Date>;
    public priceDate: ko.Observable<Date>;

    public enableWizardNext: ko.Observable<boolean>;
    public enablePrintOk: ko.Computed<boolean>;
    public enablePrintCancel: ko.Observable<boolean>;

    public labelItem: ko.Observable<any>;
    private labelCalcParam: Entities.LabelItemCalculateParams;

    // Handlers to reset results
    public labelitemsQuantityHandler: (() => void);

    // Handlers to refresh list
    public refreshLabelListHandler: (() => void);

    // Busy states
    //public isProductsModeBusy: ko.Observable<boolean>;

    constructor(context: ICustomViewControllerContext, baseState: ICustomViewControllerBaseState) {
        
        this._context = context;
        this.state = baseState;
        let date: Date = new Date();
        let today: Date = new Date(date.getFullYear(), date.getMonth(), date.getDate());
        // selected Elements Array
        this.selectedLabelItems = ko.observableArray([]);
        this.searchItems = ko.observableArray([]);
        this.labelItem = ko.observable({});
        // Intialize busy state to false by default
        //this.isProductsModeBusy = ko.observable(false);

        this.labelItemStatistics = ko.observableArray([]);

        //var clearLabelItemSelectionExtender: ObservableExtender = { notify: "always" };
        //this.clearLabelItemSelection = ko.observable(false);
        //this.clearLabelItemSelection.extend(clearLabelItemSelectionExtender);

        this.categories = ko.observableArray([]);
        this.categoryTree = ko.observableArray([]);
        this.storeSections = ko.observableArray([]);
        this.storeShelves = ko.observableArray([]);
        this.locationTree = ko.observableArray([]);
        this.labelCodes = ko.observableArray([]);
        this.labelType = ko.observable(1);
        this.allowPrint = ko.computed(() => this.labelType() != 2);
        this.allowClear = ko.computed(() => this.labelType() != 2);
        this.allowDelete = ko.computed(() => this.selectedLabelItems().length > 0);
        this.allowCommit = ko.observable(false);
        this.showWizard = ko.observable(false);
        this.itemBarCode = ko.observable('');
        this.barCodeLabelQuantity = ko.observable('');
        this.barCodeNotFoundMsg = ko.observable('');
        this.allowEdit = ko.computed(() => this.selectedLabelItems().length == 1);
        this.searchText = ko.observable("");
        this.printStatDialog = ko.observable(false);
        this.labelCalcParam = new Entities.LabelItemCalculateParams();
        this.selectedLC = ko.observable(null);
        // Initialize wizard parameters
        //this.criteria = ko.observable(new Entities.LabelItemCalculateCriteriaClass());
        //this.criteria().SearchOption = "CategoryHierarchy";
        this.selectedOptionValue = ko.observable("CategoryHierarchy");
        this.selectedQtySource = ko.observable("Available");
        this.changeTracking = ko.observable("None");
        this.fromDate = ko.observable(null);
        this.toDate = ko.observable(null);
        this.priceDate = ko.observable(today);
        this.itemFilterText = ko.observable("");
        this.itemQuantity = ko.observable(0);
        this.categoryId = ko.observable(null);
        this.hierarchyId = ko.observable(null);
        this.sectionId = ko.observable("");
        this.shelfId = ko.observable("");
        var self = this;
        this.labelType.subscribe(function (a) {
            self.updateLabelList();
        }, this);
        
        this.enableWizardNext = ko.observable(true);
        this.itemFilterText.subscribe(function () {
            if (self.itemFilterText() == "") {
                self.enableWizardNext(false);
            } else {
                self.enableWizardNext(true);
            }
        }, this);
        this.enablePrintCancel = ko.observable(false);
        this.enablePrintOk = ko.computed(() => 
            !(this.labelItemStatistics().every((element) => {
                return element.IsSelect() == false;
            })) && this.enablePrintCancel()
        );

        //this.getConfiguration();
        this.getLabelCodes();
        //this.searchLabelItems(this.PageSize);
        this._context.logger.logInformational("ItemLabelPrintWzrdViewModel: success");
    }

    /**
     * Load data page callback.
     * @param {number} size The number of items to load.
     * @param {number} skip The number of items that were already loaded.
     * @returns {Promise<Entities.LabelItem[]>} The loaded items promise.
     */
    public loadDataPage(size: number, skip: number): Promise<Entities.LabelItem[]> {
        let timeOut: number = skip == 0 ? 0 : 1000;
        let promise: Promise<any> = new Promise((resolve: (value?: any) => void) => {
            // Emulate delay of request to server.
            setTimeout(() => {
                this._context.logger.logInformational("dataListPageLoaded");
                let pageData: Promise<Entities.LabelItem[]> = this.searchLabelItems(size, skip);
                resolve(pageData);
            }, timeOut);
        });
        return promise;
    }

    public searchLabelItems(size: number, skip: number): Promise<Entities.LabelItem[]> {

        if (StringExtensions.isNullOrWhitespace(this.searchText())) {
            this.searchText("");
        }
        //if (!this.labelType()) {
        //    this.searchItems([]);
        //    return;
        //}
        let search: string = this.searchText();
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        let request: Labels.GetLabelItemRequest<Labels.GetLabelItemResponse>
            = new Labels.GetLabelItemRequest<Labels.GetLabelItemResponse>(this.labelType(), search, size, skip);

        return this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Labels.GetLabelItemResponse>): Entities.LabelItem[] => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                if (!result.canceled) {
                    this.searchItems(result.data.result);
                    return result.data.result;
                } else return [];
            })
            .catch((reason: any): any[] => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Print label search error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
                return [];
            });

    }
    
    private updateLabelQuantity(): void {
        if (ObjectExtensions.isFunction(this.labelitemsQuantityHandler)) {
            this.labelitemsQuantityHandler();
        }
    }

    private updateLabelList(): void {
        if (ObjectExtensions.isFunction(this.refreshLabelListHandler)) {
            this.refreshLabelListHandler();
        }
    }

    public resetWizardParameter(): void {

        this.selectedOptionValue("CategoryHierarchy");
        this.selectedQtySource("Available");
        this.changeTracking("None");
        this.itemFilterText("");
        this.itemQuantity(0);
        this.categoryId(null);
        this.hierarchyId(null);
        this.sectionId("");
        this.shelfId("");
        this.getLabelCodes();
        //this.selectedLC(null);
    }

    /**
     * Clears search parameters (including refiners).
     */
    public clearParameters(): void {
        this.searchText("");
        //this.labelType(1);
    }

    /**
     * Get all label item categories.
     * @return {IAsyncResult<Entities.Category[]>} The async result.
     */
    public getCategoryList(): void {

        if (this.categories().length > 0) return;

        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        let request: Labels.GetCategoryHierarchyRequest<Labels.GetCategoryHierarchyResponse>
            = new Labels.GetCategoryHierarchyRequest<Labels.GetCategoryHierarchyResponse>();

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Labels.GetCategoryHierarchyResponse>) => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                if (!result.canceled) {
                    this.categories(result.data.result);
                    this.arrangeCategories(result.data.result);
                }
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Getting category error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
        
    }
    
    public calculateItem(labelType: number, calculateItems: Entities.LabelItemCalculateItem[]) {
        
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        let request: Labels.CalculateLabelItemRequest<Labels.CalculateLabelItemResponse>
            = new Labels.CalculateLabelItemRequest<Labels.CalculateLabelItemResponse>(labelType, this.labelCalcParam, calculateItems, null, 0);
        
        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Labels.CalculateLabelItemResponse>) => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Print label calculate error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    public deleteItem(): void {

        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        let completeCount: number = 0;
        for (var i = 0; i < this.selectedLabelItems().length; i++) {
            let delRequest: Labels.LabelItemDeleteRequest<Labels.EmptyOrNullResponse>
                = new Labels.LabelItemDeleteRequest<Labels.EmptyOrNullResponse>(this.selectedLabelItems()[i]);
            this._context.runtime.executeAsync(delRequest)
                .then(() => {
                    completeCount += 1;
                    if (this.selectedLabelItems().length == completeCount) {
                        //this.isProductsModeBusy(false);
                        this.state.isProcessing = false;
                        this.updateLabelList();
                        this.selectedLabelItems([]);
                    }                    
                })
                .catch(() => {
                    //this.isProductsModeBusy(false);
                    this.state.isProcessing = false;
                    if (completeCount != 0) this.updateLabelList();
                });
        }
        
    }

    public printSelectedLabels(idx: number = 0): void {
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        let item: Entities.LabelItem = this.selectedLabelItems()[idx];
        let labelFormatRequest: Labels.GetLabelItemFormatRequest<Labels.GetLabelsFormatResponse>
            = new Labels.GetLabelItemFormatRequest<Labels.GetLabelsFormatResponse>(item.Id, item.NoOfLabels);
        this._context.runtime.executeAsync(labelFormatRequest)
            .then((result: ClientEntities.ICancelableDataResult<Labels.GetLabelsFormatResponse>) => {
                if (!result.canceled) {
                    let _commerce: any = Commerce;
                    let printerJob: Entities.PrinterJob = result.data.result;
                    let printRequest = {};
                    if (printerJob.PrinterType == "SHARPSHOOTER") {
                        let deviceConfig = new Commerce.Proxy.Entities.DeviceConfigurationClass(JSON.parse(_commerce.ApplicationStorage.getItem(_commerce.ApplicationStorageIDs.DEVICE_CONFIGURATION_KEY)));
                        let header = "CultureName=" + deviceConfig.CultureName + ";Currency=" + deviceConfig.Currency + ";Store=" + deviceConfig.StoreNumber + ";Terminal=" + deviceConfig.TerminalId + ";LabelType=" + this.labelType();
                        printRequest = {
                            DeviceName: printerJob.PrinterName,
                            DeviceType: printerJob.PrinterType,
                            Lines: printerJob.LabelData,
                            Header: JSON.stringify(header),
                            Footer: printerJob.Template
                        };
                    } else {
                        printRequest = {
                            DeviceName: printerJob.PrinterName,
                            DeviceType: printerJob.PrinterType,
                            Lines: printerJob.Document
                        };
                    }
                    let hsRequest: HardwareStationDeviceActionRequest<HardwareStationDeviceActionResponse> =
                        new HardwareStationDeviceActionRequest("PRINTER", "Print", [printRequest]);
                    //let _commerce: any = Commerce;
                    let oldTimeOut: number;
                    if (_commerce.ApplicationSession.instance.posConfiguration) {
                        oldTimeOut = _commerce.ApplicationSession.instance.posConfiguration.connectionTimeout;
                        _commerce.ApplicationSession.instance.posConfiguration._connectionTimeout = 1800;
                    }
                    this._context.runtime.executeAsync(hsRequest)
                        .then((): void => {
                            if (_commerce.ApplicationSession.instance.posConfiguration) {
                                _commerce.ApplicationSession.instance.posConfiguration._connectionTimeout = oldTimeOut;
                            }
                            //this.isProductsModeBusy(false);
                            this.state.isProcessing = false;
                            let confirmDialog: ConfirmMessageDialogModule = new ConfirmMessageDialogModule(true);
                            let confrmMsg: string = StringExtensions.format(this._context.resources.getString("CONFIRM_LABEL_PRINT_COMPLETE"),
                                this._context.resources.getString("ITEM_TXT"), item.Description || item.Name);
                            confirmDialog.open(confrmMsg)
                                .then((result: IConfirmMessageDialogResult) => {
                                    if (result.selectedValue == 'Reprint') {
                                        this.printSelectedLabels(idx);
                                    }
                                    else if (result.selectedValue !== 'Closed') {
                                        //this.isProductsModeBusy(true);
                                        this.state.isProcessing = true;
                                        let commitRequest: Labels.CommitLabelItemRequest<Labels.EmptyOrNullResponse>
                                            = new Labels.CommitLabelItemRequest<Labels.EmptyOrNullResponse>(item.Id);
                                        this._context.runtime.executeAsync(commitRequest)
                                            .then((): void => {
                                                //this.isProductsModeBusy(false);
                                                this.state.isProcessing = false;
                                                idx = idx + 1;
                                                if (idx < this.selectedLabelItems().length) {
                                                    this.printSelectedLabels(idx);
                                                } else {
                                                    this.updateLabelList();
                                                    this.printStatDialog(false);
                                                }
                                            });
                                    } else {
                                        if (idx > 0)
                                            this.updateLabelList();
                                    } 
                                }).catch((reason: any) => {
                                    this._context.logger.logError("Selected label item printOperation complete confirmation: " + JSON.stringify(reason));
                                    //this.isProductsModeBusy(false);
                                    this.state.isProcessing = false;
                                });
                        })
                        .catch((reason: any): void => {
                            if (_commerce.ApplicationSession.instance.posConfiguration) {
                                _commerce.ApplicationSession.instance.posConfiguration._connectionTimeout = oldTimeOut;
                            }
                            //this.isProductsModeBusy(false);
                            this.state.isProcessing = false;
                            this._context.logger.logError("Print label execute error: " + JSON.stringify(reason));
                            ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
                        });
                    //_commerce.Config.connectionTimeout = oldTimeOut;

                }
            })            
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Print label execute error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
    }

    public printSelectedLabelLayout(): void {
        let rrCommerce: any = Commerce;
        if (rrCommerce.ApplicationStorage.getItem(rrCommerce.ApplicationStorageIDs.HARDWARE_STATION_FEATURE_ENABLE) != "true") {
            ErrorHelper.displayMessageAsync(this._context, this._context.resources.getString("HARDWARE_STATION_REQUIRED_FOR_PRINT_OPERATION"));
            return;
        }
        this.enablePrintCancel(false);
        var stats: Entities.LabelItemStatistic[] = [];
        for (var i: number = 0; i < this.labelItemStatistics().length; i++) {
            if (this.labelItemStatistics()[i].IsSelect()) {
                stats[i] = this.labelItemStatistics()[i].statistic;
            }
        }
        this.printLayouts(0, stats);
    }

    private printLayouts(idx: number, stats: Entities.LabelItemStatistic[]): void {
        
        var itemStat: Entities.LabelItemStatistic = stats[idx];
        itemStat.LabelRecIds = '';
        let labelIds: number[] = [];
        if (this.selectedLabelItems().length > 1) {
            this.selectedLabelItems().forEach((item) => { return labelIds.push(item.Id) });
            itemStat.LabelRecIds = labelIds.toString();
        } 
        let request: Labels.GetLabelsFormatRequest<Labels.GetLabelsFormatResponse>
            = new Labels.GetLabelsFormatRequest<Labels.GetLabelsFormatResponse>(itemStat);
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Labels.GetLabelsFormatResponse>) => {

                if (!result.canceled) {
                    let printerJob: Entities.PrinterJob = result.data.result;
                    let _commerce: any = Commerce;
                    let printRequest = {};
                    if (printerJob.PrinterType == "SHARPSHOOTER") {
                        let deviceConfig = new Commerce.Proxy.Entities.DeviceConfigurationClass(JSON.parse(_commerce.ApplicationStorage.getItem(_commerce.ApplicationStorageIDs.DEVICE_CONFIGURATION_KEY)));
                        let header = "CultureName=" + deviceConfig.CultureName + ";Currency=" + deviceConfig.Currency + ";Store=" + deviceConfig.StoreNumber + ";Terminal=" + deviceConfig.TerminalId + ";LabelType=" + this.labelType();
                        printRequest = {
                            DeviceName: printerJob.PrinterName,
                            DeviceType: printerJob.PrinterType,
                            Lines: printerJob.LabelData,
                            Header: JSON.stringify(header),
                            Footer: printerJob.Template
                        };
                    } else {
                        printRequest = {
                            DeviceName: printerJob.PrinterName,
                            DeviceType: printerJob.PrinterType,
                            Lines: printerJob.Document
                        };
                    }
                    let hsRequest: HardwareStationDeviceActionRequest<HardwareStationDeviceActionResponse> =
                        new HardwareStationDeviceActionRequest("PRINTER", "Print", [printRequest]);
                    //let _commerce: any = Commerce;
                    let oldTimeOut: number;
                    if (_commerce.ApplicationSession.instance.posConfiguration) {
                        oldTimeOut = _commerce.ApplicationSession.instance.posConfiguration.connectionTimeout;
                        _commerce.ApplicationSession.instance.posConfiguration._connectionTimeout = 3600;
                    }
                    this._context.runtime.executeAsync(hsRequest)
                        .then((): void => {
                            if (_commerce.ApplicationSession.instance.posConfiguration) {
                                _commerce.ApplicationSession.instance.posConfiguration._connectionTimeout = oldTimeOut;
                            }
                            //this.isProductsModeBusy(false);
                            this.state.isProcessing = false;
                            let confirmDialog: ConfirmMessageDialogModule = new ConfirmMessageDialogModule(true);
                            let confrmMsg: string = StringExtensions.format(this._context.resources.getString("CONFIRM_LABEL_PRINT_COMPLETE"),
                                this._context.resources.getString("LAYOUT_TXT"), itemStat.LayoutName || itemStat.LayoutId);
                            confirmDialog.open(confrmMsg)
                                .then((result: IConfirmMessageDialogResult) => {
                                    if (result.selectedValue == 'Reprint') {
                                        this.printLayouts(idx, stats);  // re-print layout items
                                    }
                                    else if (result.selectedValue !== 'Closed') {
                                        //this.isProductsModeBusy(false);
                                        this.state.isProcessing = true;
                                        if (this.selectedLabelItems().length > 1) {
                                            // commit selected item
                                            let commitRequest: Labels.CommitLabelItemsRequest<Labels.EmptyOrNullResponse>
                                                = new Labels.CommitLabelItemsRequest<Labels.EmptyOrNullResponse>(labelIds);
                                            this._context.runtime.executeAsync(commitRequest)
                                                .then((): void => {
                                                    this.state.isProcessing = false;
                                                    this.updateLabelList();
                                                    this.printStatDialog(false);
                                                });
                                        } else {
                                            let commitRequest: Labels.GetLabelsCommitRequest<Labels.EmptyOrNullResponse>
                                                = new Labels.GetLabelsCommitRequest<Labels.EmptyOrNullResponse>(itemStat);
                                            this._context.runtime.executeAsync(commitRequest)
                                                .then((): void => {
                                                    //this.isProductsModeBusy(false);
                                                    this.state.isProcessing = false;
                                                    idx = idx + 1;
                                                    if (idx < stats.length) {
                                                        this.printLayouts(idx, stats);  // print next layout items
                                                    } else {
                                                        this.updateLabelList();
                                                        this.printStatDialog(false);
                                                    }
                                                });
                                        }
                                        
                                    } else {
                                        if (idx > 0) {
                                            this.updateLabelList();
                                        }
                                        this.printStatDialog(false);
                                    }
                                }).catch((reason: any) => {
                                    this._context.logger.logError("ItemLabelPrintWzrdViewModel complete printOperation confirmation: " + JSON.stringify(reason));
                                    //this.isProductsModeBusy(false);
                                    this.state.isProcessing = false;
                                });

                        })
                        .catch((reason: any): void => {
                            if (_commerce.ApplicationSession.instance.posConfiguration) {
                                _commerce.ApplicationSession.instance.posConfiguration._connectionTimeout = oldTimeOut;
                            }
                            //this.isProductsModeBusy(false);
                            this.state.isProcessing = false;
                            this.enablePrintCancel(true);
                            this._context.logger.logError("Print label execute error: " + JSON.stringify(reason));
                            ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
                        });
                    //_commerce.Config.connectionTimeout = oldTimeOut;

                }
            }).then((): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this.enablePrintCancel(true);
                this._context.logger.logError("Print label execute error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });        
    }

    public calculateStatistics(): void {

        var itemStats = [];
        let request: Labels.GetCalcStatRequest<Labels.GetCalcStatResponse>
            = new Labels.GetCalcStatRequest<Labels.GetCalcStatResponse>(this.labelType());
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        let allowSelect: boolean = false;
        let canCalculateAndPrint = this.selectedLabelItems().length > 1;
        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Labels.GetCalcStatResponse>) => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                if (!result.canceled) {
                    let stats: Entities.LabelItemStatistic[] = result.data.result;
                    for (var i: number = 0; i < stats.length; i++) {
                        allowSelect = StringExtensions.isNullOrWhitespace(stats[i].ErrorText);
                        itemStats.push({ statistic: stats[i], IsSelect: ko.observable(allowSelect), IsEnable: ko.observable(allowSelect) });
                        
                    }
                    this.labelItemStatistics(itemStats);
                    if (!canCalculateAndPrint) {
                        this.printStatDialog(true);
                        this.enablePrintCancel(true);
                    }
                }
            })
            .then(() => {
                if (canCalculateAndPrint)
                    this.printSelectedLabelLayout();
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Print label calculate Statistics error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
        
    }

    public clear(): void {
        /*
        var printContents: HTMLDivElement = document.getElementById('section-to-print') as HTMLDivElement;
        printContents.innerHTML = 'direct print to printer from mPOS';
        window.print();
        */
        let request: Labels.GetClearRequest<Labels.EmptyOrNullResponse>
            = new Labels.GetClearRequest<Labels.EmptyOrNullResponse>(this.labelType());
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Labels.EmptyOrNullResponse>) => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                if (!result.canceled) {
                    //this.labelType(2);
                    //this.searchLabelItems(this.PageSize);
                    this.updateLabelList();
                }
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError(" Clear execute error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
        
    }

    public calculateItemsOnWizard(): void {

        var itemCalculateCriteria = new Entities.LabelItemCalculateCriteria();
        itemCalculateCriteria.SearchOption = this.selectedOptionValue();
        itemCalculateCriteria.Quantity = this.itemQuantity();
        itemCalculateCriteria.CategoryId = isNaN(this.categoryId()) ? 0 : this.categoryId();
        itemCalculateCriteria.HierarchyId = isNaN(this.hierarchyId()) ? 0 : this.hierarchyId();
        itemCalculateCriteria.SectionId = StringExtensions.isNullOrWhitespace(this.sectionId()) ? "" : this.sectionId();
        itemCalculateCriteria.ShelfId = StringExtensions.isNullOrWhitespace(this.shelfId()) ? "" : this.shelfId();
        itemCalculateCriteria.ItemFilter = this.itemFilterText();
        itemCalculateCriteria.QtySource = this.selectedQtySource();
        itemCalculateCriteria.ChangeTracking = this.changeTracking();
        itemCalculateCriteria.FromDate = null;
        itemCalculateCriteria.ToDate = null;
        if (this.selectedQtySource() == "OrderReceived") {
            itemCalculateCriteria.FromDate = this.fromDate() != null ? this.fromDate() : new Date();
            itemCalculateCriteria.ToDate = this.toDate() != null ? this.toDate() : new Date();
        }
        if (ObjectExtensions.isNullOrUndefined(this.selectedLC())){
            this.selectedLC = ko.observable(new Entities.LabelCode());
        }
        this.labelCalcParam.CodeRecId = isNaN(this.selectedLC().Id) ? 0 : this.selectedLC().Id;
        this.labelCalcParam.PriceDate = this.DateWithTime(this.priceDate());
        
        this.wizardItemCalculate(itemCalculateCriteria, 0);
    }

    private wizardItemCalculate(itemCalculateCriteria: Entities.LabelItemCalculateCriteria, pageNumber: number) {

        let request: Labels.CalculateLabelItemRequest<Labels.CalculateLabelItemResponse>
            = new Labels.CalculateLabelItemRequest<Labels.CalculateLabelItemResponse>(this.labelType(), this.labelCalcParam, null, itemCalculateCriteria, pageNumber);
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        let _commerce: any = Commerce;
        let requestDelay: number = pageNumber == 0 ? 10 : 500;
        let oldTimeOut: number;
        if (_commerce.ApplicationSession.instance.posConfiguration) {
            oldTimeOut = _commerce.ApplicationSession.instance.posConfiguration.connectionTimeout;
            _commerce.ApplicationSession.instance.posConfiguration._connectionTimeout = 900;
        }
        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Labels.CalculateLabelItemResponse>) => {
                if (_commerce.ApplicationSession.instance.posConfiguration) {
                    _commerce.ApplicationSession.instance.posConfiguration._connectionTimeout = oldTimeOut;
                }
                // Emulate delay of request to server.
                setTimeout(() => {
                    //this.isProductsModeBusy(false);
                    this.state.isProcessing = false;
                    if (!result.canceled) {
                        if (result.data.result >= 0) {
                            //this.searchLabelItems(this.PageSize);
                            this.updateLabelList();
                        } else {
                            ++pageNumber;
                            this.wizardItemCalculate(itemCalculateCriteria, pageNumber);
                        }
                    }
                }, requestDelay);
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Calculate item on wizard error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
        
    }
    
    public noOfLabelEditor(item: Entities.LabelItem) {

        this.labelItem(item);
        //this.selectedLC(this.labelCodes().filter(lc => lc.Id == item.CodeRecId)[0]);
        this.updateLabelQuantity();
    }

    //private getConfiguration(): void {
    //    let request: Labels.UtilitiesConfigurationRequest<Labels.UtilitiesConfigurationResponse>
    //        = new Labels.UtilitiesConfigurationRequest<Labels.UtilitiesConfigurationResponse>();
    //    //this.isProductsModeBusy(true);
    //    this.state.isProcessing = true;

    //    this._context.runtime.executeAsync(request)
    //        .then((result: ClientEntities.ICancelableDataResult<Labels.UtilitiesConfigurationResponse>) => {
    //            //this.isProductsModeBusy(false);
    //            this.state.isProcessing = false;
    //            if (!result.canceled) {
    //                let config: Entities.UtilitiesConfiguration = result.data.result;
    //                let showWizard: boolean = config.DisableLabelWizard == 0;
    //                this.showWizard(showWizard);
    //            }
    //        })
    //        .catch((reason: any): void => {
    //            //this.isProductsModeBusy(false);
    //            this.state.isProcessing = false;
    //            this._context.logger.logError("Getting store section's error: " + JSON.stringify(reason));
    //            ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
    //        });
    //}

    public getLabelCodes(): void {
        if (this.labelCodes().length > 0) return;

        let request: Labels.GetLabelCodeRequest<Labels.GetLabelCodeResponse>
            = new Labels.GetLabelCodeRequest<Labels.GetLabelCodeResponse>();
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Labels.GetLabelCodeResponse>) => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                if (!result.canceled) {
                    this.labelCodes(result.data.result);
                }
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Getting store section's error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
    }

    public getStoreSections() {

        if (this.storeSections().length > 0) return;

        let request: Labels.GetStoreSectionsRequest<Labels.GetStoreSectionsResponse>
            = new Labels.GetStoreSectionsRequest<Labels.GetStoreSectionsResponse>();
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Labels.GetStoreSectionsResponse>) => {
                //this.isProductsModeBusy(false);                
                this.state.isProcessing = false;
                if (!result.canceled) {
                    this.storeSections(result.data.result);
                    this.arrangeLocation(null);
                }
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Getting store section's error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    public getStoreShelves(sectionId: string = '') {

        if (this.storeShelves().length > 0) return;

        let request: Labels.GetStoreShelvesRequest<Labels.GetStoreShelvesResponse>
            = new Labels.GetStoreShelvesRequest<Labels.GetStoreShelvesResponse>(sectionId);
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        
        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Labels.GetStoreShelvesResponse>) => {
                //this.isProductsModeBusy(false); 
                this.state.isProcessing = false;
                if (!result.canceled) {
                    this.storeShelves(result.data.result);
                    this.arrangeLocation(result.data.result);
                }
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Geting section's shalve error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
        
    }

    public updatelabelitem(quantity: number) {

        var item: Entities.LabelItem = this.selectedLabelItems()[0];
        item.NoOfLabels = quantity;
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;

        let upRequest: Labels.LabelItemUpdateRequest<Labels.LabelItemUpdateResponse>
            = new Labels.LabelItemUpdateRequest<Labels.LabelItemUpdateResponse>(item);

        this._context.runtime.executeAsync(upRequest)
            .then(() => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this.updateLabelList();
                this.selectedLabelItems([]);
            })
            .catch(() => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
            });
        
    }

    public labelQuantityCancelHandler() {
        this.labelItem({});
    }

    public calculateImportBarcodes(labelType: number, barCodes: string[], labelCalcParam: Entities.LabelItemCalculateParams): void {

        let calculateItem: Entities.LabelItemCalculateItem;
        let items: Entities.LabelItemCalculateItem[] = [];
        for (let i = 0; i < barCodes.length; i++) {
            if (barCodes[i] != '') {
                calculateItem = new Entities.LabelItemCalculateItem();
                calculateItem.ProductId = 0;
                calculateItem.BarCode = barCodes[i];
                calculateItem.Quantity = 1;
                items.push(calculateItem);
            }            
        }
        let request: Labels.CalculateLabelItemRequest<Labels.CalculateLabelItemResponse>
            = new Labels.CalculateLabelItemRequest<Labels.CalculateLabelItemResponse>(labelType, labelCalcParam, items, null, 0);
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Labels.CalculateLabelItemResponse>) => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this.updateLabelList();
                this.selectedLabelItems([]);
            })
            .catch((reason: any): void => {
                this._context.logger.logError("Calculate label item error: " + JSON.stringify(reason));
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
            });
    }

    public calculateItemBarcode(): void {

        this.barCodeNotFoundMsg('');
        let codeRecId: number = 0;
        if (this.selectedLC() == undefined) codeRecId = 0;
        else codeRecId = this.selectedLC().Id;

        if (StringExtensions.isEmptyOrWhitespace(this.itemBarCode())) {
            this.barCodeNotFoundMsg(this._context.resources.getString("ENTER_BARCODE_ALERT"));
            return;
        }
        if (StringExtensions.isEmptyOrWhitespace(this.barCodeLabelQuantity())) {
            this.barCodeNotFoundMsg(this._context.resources.getString("ENTER_LABEL_QUANTIRY_ALERT"));
            return;
        }
        if (Commerce.NumberExtensions.isNullOrNaN(parseInt(this.barCodeLabelQuantity()))) {
            this.barCodeNotFoundMsg(this._context.resources.getString("ENTER_LABEL_QUANTIRY_ALERT"));
            return;
        }
        if (codeRecId == 0) {
            this.barCodeNotFoundMsg(this._context.resources.getString("SELECT_LABEL_CODE_ALERT"));
            return;
        }
        let calculateItem: Entities.LabelItemCalculateItem = new Entities.LabelItemCalculateItem();
        calculateItem.ProductId = 0;
        calculateItem.BarCode = this.itemBarCode();
        calculateItem.Quantity = parseInt(this.barCodeLabelQuantity());
        
        this.labelCalcParam.CodeRecId = codeRecId;
        this.labelCalcParam.PriceDate = this.DateWithTime(this.priceDate());
        
        let request: Labels.CalculateLabelItemRequest<Labels.CalculateLabelItemResponse>
            = new Labels.CalculateLabelItemRequest<Labels.CalculateLabelItemResponse>(this.labelType(), this.labelCalcParam, [calculateItem], null, 0);
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Labels.CalculateLabelItemResponse>) => {
                if (!result.canceled) {
                    if (result.data.result == 0) {
                        this.barCodeNotFoundMsg(StringExtensions.format(this._context.resources.getString("BARCODE_NOT_FOUND"), this.itemBarCode()));
                        //this.isProductsModeBusy(false);
                        this.state.isProcessing = false;
                    } else {
                        this.updateLabelList();
                        this.selectedLabelItems([]);
                        this.itemBarCode('');
                        this.barCodeLabelQuantity('');
                    }
                }
            })
            .catch((reason: any): void => {
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
            });
    }

    /**
     * Sets the selected items upon which other actions can be performed.
     * @param {Entities.LabelItem} items The items.
     */
    public setSelectedItems(items: Entities.LabelItem[]): void {
        this.selectedLabelItems(items);
        
    }

    /**
     * Create a category list to be displayed in the heirarchy
     */
    private arrangeCategories(arry: ProxyEntities.Category[]): void {
        var roots = [], children = {};

        for (var i = 0, len = arry.length; i < len; ++i) {
            var item = arry[i];
            var p = item.ParentCategory;
            var target = !p ? roots : (children[p] || (children[p] = []));
            target.push({ value: item });
        }

        var findChildren = function (parentCategory) {
            if (children[parentCategory.value.RecordId]) {
                parentCategory.children = children[parentCategory.value.RecordId];
                for (var i = 0, len = parentCategory.children.length; i < len; ++i) {
                    findChildren(parentCategory.children[i]);
                }
            } else {
                parentCategory.children = [];
            }
        };

        for (var i = 0, ltd = roots.length; i < ltd; ++i) {
            findChildren(roots[i]);
        }

        this.categoryTree(roots);
    }

    private arrangeLocation(arry: Entities.StoreShelf[]): void {
        var sections = this.storeSections(),
            shelves = this.storeShelves(),
            roots = [],
            parents = [];

        if (this.storeSections().length == 0 || this.storeShelves().length == 0) {
            return;
        }

        for (var i = 0; i < sections.length; ++i) {
            var section: Entities.StoreSection = sections[i];
            parents.push({
                SectionId: section.SectionId,
                SectionName: section.SectionName,
                shelves: []
            });
            for (var j = 0; j < shelves.length; ++j) {
                var shelve: Entities.StoreShelf = shelves[j];
                if (section.SectionId == shelve.SectionId) {
                    parents[i].shelves.push(shelve);
                }
            }
        }

        roots.push({ Id: 0, Name: this._context.resources.getString("ALL"), sections: parents });
        this.locationTree(roots);
    }

    private DateWithTime(date: Date): Date {
        let tmpDate = new Date(Date.now());
        let hrs = tmpDate.getHours();
        let mins = tmpDate.getMinutes();
        let secs = tmpDate.getSeconds();
        let newDate: Date = new Date(date.getFullYear(), date.getMonth(), date.getDate(), hrs, mins, secs, 0);
        return newDate;
    }
}