import ko from "knockout";
import { ProxyEntities, ClientEntities } from "PosApi/Entities";
import { ICustomViewControllerContext, ICustomViewControllerBaseState } from "PosApi/Create/Views";
import { ObjectExtensions, StringExtensions } from "PosApi/TypeExtensions";
import { StockTake } from "../DataService2/DataServiceRequests.g";
import { Entities } from "../DataService2/DataServiceEntities.g";
import ConfirmMessageDialogModule from "../Controls/DialogControl/ConfirmMessageDialogModule";
import { IConfirmMessageDialogResult } from "../Controls/DialogControl/IConfirmMessageDialogResult";
import { HardwareStationDeviceActionRequest, HardwareStationDeviceActionResponse } from "PosApi/Consume/Peripherals";
import { ErrorHelper } from "../../ErrorHelper";


/**
 * The ViewModel for ItemLabelPrintWzrdView.
 */
export default class StockTakeJournalsViewModel {
    private _context: ICustomViewControllerContext;

    public searchText: ko.Observable<string>;
    public journalType: ko.Observable<number>;
    public axSCJournals: ko.ObservableArray<any>;
    public isEnable: ko.Computed<boolean>;
    public selectedJournalId: ko.Observable<string>;
    public selectedCMValue: ko.Observable<string>;
    public selectedLMValue: ko.Observable<string>;
    public axJournal: ko.Observable<ProxyEntities.StockCountJournal>;
    public selectedJournalID: ko.Observable<string>;
    
    public isShowDialog: ko.Observable<boolean>;
    public searchJournals: ko.ObservableArray<Entities.SCJournal>;

    public enableWizardNext: ko.Observable<boolean>;
    public enablePrintOk: ko.Computed<boolean>;
    public allowOffline: ko.Observable<boolean>;
    public scJournal: ko.Observable<Entities.SCJournal>;

    // Handlers to reset results
    public labelitemsQuantityHandler: (() => void);

    // Busy states
    //public isProductsModeBusy: ko.Observable<boolean>;
    //public isDialogModeBusy: ko.Observable<boolean>;
    private state: ICustomViewControllerBaseState;

    //Handlers to refresh list
    public refreshListHandler: (() => void);

    constructor(context: ICustomViewControllerContext, vState: ICustomViewControllerBaseState) {
        //super();

        this._context = context;
        this.state = vState;
        // selected Elements Array
        this.searchJournals = ko.observableArray([]);
        this.axSCJournals = ko.observableArray([]);

        // Intialize busy state to false by default
        //this.isProductsModeBusy = ko.observable(false);
        //this.isDialogModeBusy = ko.observable(false);

        this.allowOffline = ko.observable(false);   // forcefully hide import/export offline buttons
        this.isShowDialog = ko.observable(false);
        this.scJournal = ko.observable(null);
        this.axJournal = ko.observable(null);
        this.selectedJournalID = ko.observable("");
        this.journalType = ko.observable(0);
        this.isEnable = ko.computed(() => this.scJournal() != null);
        this.searchText = ko.observable("");
        this.selectedJournalId = ko.observable("");
        this.selectedCMValue = ko.observable("0");
        this.selectedLMValue = ko.observable("0");
        
        var self = this;
        this.journalType.subscribe(function (a) {
            self.searchSCJournals();
        }, this);
                
        //this.searchSCJournals();
        this._context.logger.logInformational("StockTakeJournalsViewModel: success");
    }

    //public loadDataPage(size: number, skip: number): Promise<Entities.SCJournal[]> {
    //    return this.searchSCJournals();
    //}

    public searchSCJournals(): Promise<Entities.SCJournal[]> {

        if (StringExtensions.isNullOrWhitespace(this.searchText())) {
            this.searchText("");
        }
        this.searchJournals([]);
        let search: string = this.searchText();
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        let request: StockTake.SearchJournalRequest<StockTake.SearchJournalResponse>
            = new StockTake.SearchJournalRequest<StockTake.SearchJournalResponse>(this.journalType(), search);

        return this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<StockTake.SearchJournalResponse>) => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                if (!result.canceled) {
                    this.searchJournals(result.data.result);
                    if (ObjectExtensions.isFunction(this.refreshListHandler)) {
                        this.refreshListHandler();
                    }
                    return this.searchJournals();
                }
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Stock Take Journal search error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    //private updateList(): void {
    //    if (ObjectExtensions.isFunction(this.refreshListHandler)) {
    //        this.refreshListHandler();
    //    }
    //}
    public openJournalDialog() {
        
        var journals = [];
        let request: StockTake.AXJournalRequest<StockTake.AXJournalResponse>
            = new StockTake.AXJournalRequest<StockTake.AXJournalResponse>();
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;

        let allowSelect: boolean = false;

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<StockTake.AXJournalResponse>) => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                if (!result.canceled) {
                    let stats: ProxyEntities.StockCountJournal[] = result.data.result;
                    for (var i: number = 0; i < stats.length; i++) {
                        
                        journals.push({ Journal: stats[i], IsSelect: ko.observable(allowSelect) });

                    }
                    this.axSCJournals(journals);
                    this.isShowDialog(true);
                }
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Get Journal error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    public ImportJournal(): void {
        let requestParam: Entities.SCJournalRequest = new Entities.SCJournalRequest();
        requestParam.CalcMode = parseInt(this.selectedCMValue(), 10);
        requestParam.LocationMode = parseInt(this.selectedLMValue(), 10);
        requestParam.RecordId = this.axJournal().RecordId;
        requestParam.JournalId = this.axJournal().JournalId;
        requestParam.Description = this.axJournal().Description;

        let request: StockTake.JournalImportRequest<StockTake.JournalImportResponse>
            = new StockTake.JournalImportRequest<StockTake.JournalImportResponse>(requestParam);
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<StockTake.JournalImportResponse>) => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                if (!result.canceled) {
                    //let journal: Entities.SCJournal = result.data.result;
                    //let journals: Entities.SCJournal[] = this.searchJournals();
                    //journals.push(journal);
                    //this.searchJournals([]);
                    //this.searchJournals(journals);
                    this.searchSCJournals();
                }
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Import Journal error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    public adjustJournal(): void {
        let request: StockTake.JournalAdjustRequest<StockTake.JournalAdjustResponse>
            = new StockTake.JournalAdjustRequest<StockTake.JournalAdjustResponse>(this.scJournal().Id);
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<StockTake.JournalAdjustResponse>) => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Get Journal error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
    }

    public calcualteJournal(): void {
        let request: StockTake.JournalCalculateRequest<StockTake.JournalCalculateResponse>
            = new StockTake.JournalCalculateRequest<StockTake.JournalCalculateResponse>(this.scJournal().Id, true);
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<StockTake.JournalCalculateResponse>) => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Get Journal error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
    }

    public commitJournal(): void {

        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        let entryRequest: StockTake.SearchSCJournalEntryRequest<StockTake.SearchSCJournalEntryResponse>
            = new StockTake.SearchSCJournalEntryRequest<StockTake.SearchSCJournalEntryResponse>(this.scJournal().Id, "");
        this._context.runtime.executeAsync(entryRequest)
            .then((resp: ClientEntities.ICancelableDataResult<StockTake.SearchSCJournalEntryResponse>) => {
                if (!resp.canceled) {
                    let entries = resp.data.result;
                    let doCommit: boolean = false;
                    for (let i = 0; i < entries.length; i++) {
                        if (entries[i].StatusValue != 0) {
                            doCommit = true;
                        }
                    }
                    if (doCommit) {
                        let dialog: ConfirmMessageDialogModule = new ConfirmMessageDialogModule();
                        dialog.open(this._context.resources.getString("CONFIRM_COMMIT_JOURNAL"))
                            .then((result: IConfirmMessageDialogResult) => {
                                if (result.selectedValue !== 'Closed') {

                                    let commitRequest: StockTake.JournalCommitRequest<StockTake.JournalCommitResponse>
                                        = new StockTake.JournalCommitRequest<StockTake.JournalCommitResponse>(this.scJournal().Id);
                                    this._context.runtime.executeAsync(commitRequest)
                                        .then((result: ClientEntities.ICancelableDataResult<StockTake.JournalCommitResponse>) => {
                                            //this.isProductsModeBusy(false);
                                            this.state.isProcessing = false;
                                            if (!result.canceled) {
                                                this.searchSCJournals();
                                            }
                                        })
                                        .catch((reason: any): void => {
                                            //this.isProductsModeBusy(false);
                                            this.state.isProcessing = false;
                                            this._context.logger.logError("Commit Journal error: " + JSON.stringify(reason));
                                            ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
                                        });
                                } else {
                                    //this.isProductsModeBusy(false);
                                    this.state.isProcessing = false;
                                }
                            });
                    }
                    else {
                        ErrorHelper.displayMessageAsync(this._context, this._context.resources.getString("JOURNAL_CALCULATE_VALIDATION"));
                        //this.isProductsModeBusy(false);
                        this.state.isProcessing = false;
                    }
                } else
                    //this.isProductsModeBusy(false);
                    this.state.isProcessing = false;
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Stock Take Journal entry error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
    }

    public deleteJournal(): void {

        let request: StockTake.DeleteJournalRequest<StockTake.DeleteJournalResponse>
            = new StockTake.DeleteJournalRequest<StockTake.DeleteJournalResponse>(this.scJournal());
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<StockTake.DeleteJournalResponse>) => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                if (!result.canceled) {
                    this.searchSCJournals();
                }
            })
            .catch((reason: any): void => {
                //this.isProductsModeBusy(false);
                this.state.isProcessing = false;
                this._context.logger.logError("Delete Journal error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
    }
    
    public importOfflineStockTake(file: any): void {
        let me = this;
        //me.isProductsModeBusy(true);
        this.state.isProcessing = true;
        this.getXml(file)
            .then(data => {
                //console.log(data);                
                let requestObj = { "xmlData": data };
                let importRequest: HardwareStationDeviceActionRequest<HardwareStationDeviceActionResponse> =
                    new HardwareStationDeviceActionRequest("FILEOPERATION", "ImportStockTakeFromXML", requestObj);

                me._context.runtime.executeAsync(importRequest)
                    .then((resp: any): any => {
                        let stItems: Entities.StockTakeItem[] = resp.data.response.StockTakeItems;
                        me._context.runtime.executeAsync(new StockTake.CountItemsRequest<StockTake.CountItemsResponse>(stItems))
                            .then((result: ClientEntities.ICancelableDataResult<StockTake.CountItemsResponse>) => {
                                //me.isProductsModeBusy(false);
                                this.state.isProcessing = false;
                                if (!result.canceled) {
                                    console.log(result.data.result);
                                    ErrorHelper.displayMessageAsync(me._context, me._context.resources.getString("JOURNAL_IMPORT_SUCCESS_MESSAGE"));
                                }
                            })
                            .catch((reason: any): void => {
                                //me.isProductsModeBusy(false);
                                this.state.isProcessing = false;
                                me._context.logger.logError("Count inport journal items error: " + JSON.stringify(reason));
                                ErrorHelper.displayLocalizedErrorAsync(me._context, reason);
                            });
                    })
                    .catch((reason: any): void => {
                        //me.isProductsModeBusy(false);
                        this.state.isProcessing = false;
                        me._context.logger.logError("Convert xml to object error: " + JSON.stringify(reason));
                        ErrorHelper.displayLocalizedErrorAsync(me._context, reason);
                    });

            }
        );
    }

    public exportJournal(): void {
        //let arr = [];
        let transactions: Entities.SCJournalTransaction[] = [];
        let sections: Entities.StoreSection[] = [];
        let shelve: Entities.StoreShelf[] = [];
        let journals = this.searchJournals();
        //let trans: number = 0;
        let isTrans: boolean = false;
        let isSection: boolean = false;
        let isShelve: boolean = false;
        let isDone: boolean = false;
        //this.isProductsModeBusy(true);
        this.state.isProcessing = true;
        let me = this;
        let rr:any = { commerce: Commerce };

        new rr.commerce.AsyncQueue().enqueue(function () {

            var asyncResult = new rr.commerce.VoidAsyncResult();

            // Getting Journal Transactions
            for (let i = 0; i < journals.length; i++) {
                setTimeout(function () {
                        me._context.runtime.executeAsync(new StockTake.SearchJournalTransactionRequest<StockTake.SearchJournalTransactionResponse>(journals[i].Id, ''))
                            .then((result: ClientEntities.ICancelableDataResult<StockTake.SearchJournalTransactionResponse>) => {
                                if (!result.canceled) {
                                    transactions = transactions.concat(result.data.result);
                                }
                                isTrans = false;
                                isDone = false;
                                if (i + 1 == journals.length)
                                    isTrans = true;
                                if (isTrans && isSection && isShelve)
                                    isDone = true;

                                if (isDone)
                                    asyncResult.resolve();
                            })
                            .catch((reason: any): void => {
                                //me.isProductsModeBusy(false);
                                me.state.isProcessing = false;
                                me._context.logger.logError("Stock Take Journal Transaction search error: " + JSON.stringify(reason));
                                ErrorHelper.displayLocalizedErrorAsync(me._context, reason);
                                asyncResult.reject(reason);
                            })
                    }, 50);
            }

            // Getting Store Sections
            me._context.runtime.executeAsync(new StockTake.GetStoreSectionsRequest<StockTake.GetStoreSectionsResponse>())
                .then((result: ClientEntities.ICancelableDataResult<StockTake.GetStoreSectionsResponse>) => {
                    if (!result.canceled) {
                        sections = result.data.result;                        
                    }
                    isSection = true;
                    if (isTrans && isSection && isShelve)
                        isDone = true;
                    if (isDone)
                        asyncResult.resolve();
                })
                .catch((reason: any): void => {
                    //me.isProductsModeBusy(false);
                    me.state.isProcessing = false;
                    me._context.logger.logError("Getting store section's error: " + JSON.stringify(reason));
                    ErrorHelper.displayLocalizedErrorAsync(me._context, reason);
                    asyncResult.reject(reason);
                });

            // Getting Store Shelve
            me._context.runtime.executeAsync(new StockTake.GetStoreShelvesRequest<StockTake.GetStoreShelvesResponse>(''))
                .then((result: ClientEntities.ICancelableDataResult<StockTake.GetStoreShelvesResponse>) => {
                    if (!result.canceled) {
                        shelve = result.data.result;
                    }
                    isShelve = true;
                    if (isTrans && isSection && isShelve)
                        isDone = true;
                    if (isDone)
                        asyncResult.resolve();
                })
                .catch((reason: any): void => {
                    //me.isProductsModeBusy(false);
                    me.state.isProcessing = false;
                    me._context.logger.logError("Getting store section's error: " + JSON.stringify(reason));
                    ErrorHelper.displayLocalizedErrorAsync(me._context, reason);
                    asyncResult.reject(reason);
                });

            return asyncResult;

        }).enqueue(function () {

            var asyncResult = new rr.commerce.VoidAsyncResult();
            let items: Entities.BarcodeItem[] = [];
            
            for (let i = 0; i < transactions.length; i++) {
                items.push(new Entities.BarcodeItem({
                    InventDimId: transactions[i].InventDimId,
                    ItemId: transactions[i].ItemId,
                    RecordId: transactions[i].RecordId,
                    UnitId: transactions[i].Unit
                }));
            }
            
            me._context.runtime.executeAsync(new StockTake.GetBarcodeForItemRequest<StockTake.GetBarcodeForItemResponse>(items))
                .then((result: ClientEntities.ICancelableDataResult<StockTake.GetBarcodeForItemResponse>) => {
                    if (!result.canceled) {
                        items = result.data.result;
                        for (let j = 0; j < items.length; j++) {
                            if (items[j].Barcode != null || items[j].Barcode != "") {
                                transactions.forEach((element, index) => {
                                    if (element.RecordId === items[j].RecordId) {
                                        transactions[index].Barcode = items[j].Barcode;
                                    }
                                });
                            }
                            if (j + 1 == items.length)
                                asyncResult.resolve();
                        }
                    }
                    
                })
                .catch((reason: any): void => {
                    //me.isProductsModeBusy(false);
                    me.state.isProcessing = false;
                    me._context.logger.logError("Getting store section's error: " + JSON.stringify(reason));
                    ErrorHelper.displayLocalizedErrorAsync(me._context, reason);
                    asyncResult.reject(reason);
                });
            return asyncResult;

            }).enqueue(function () {

                var asyncResult = new rr.commerce.VoidAsyncResult();
                
                let requestObj = {
                    "FileName": "",
                    "SCJournals": JSON.stringify(journals),
                    "SCJournalTransactions": JSON.stringify(transactions),
                    "StoreSections": JSON.stringify(sections),
                    "StoreSectionShelve": JSON.stringify(shelve)
                };
                let exportRequest: HardwareStationDeviceActionRequest<HardwareStationDeviceActionResponse> =
                    new HardwareStationDeviceActionRequest("FILEOPERATION", "Export", requestObj);

                me._context.runtime.executeAsync(exportRequest)
                    .then((resp: any): any => {
                        console.log(JSON.stringify(resp));
                        let respMsg: string = '';
                        if (resp.data.response == true)
                            respMsg = me._context.resources.getString("JOURNAL_EXPORT_SUCCESS_MESSAGE");
                        else
                            respMsg = me._context.resources.getString("JOURNAL_EXPORT_FAIL_MESSAGE");
                        //me.isProductsModeBusy(false);
                        me.state.isProcessing = false;
                        ErrorHelper.displayMessageAsync(me._context, respMsg);
                        asyncResult.resolve();
                    })
                    .catch((reason: any): void => {
                        //me.isProductsModeBusy(false);
                        me.state.isProcessing = false;
                        me._context.logger.logError("Getting journal export error: " + JSON.stringify(reason));
                        ErrorHelper.displayLocalizedErrorAsync(me._context, reason);
                        asyncResult.reject(reason);
                    });

                return asyncResult;

            }).run();
        
    }
    
    public getXml(file): Promise<any> {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.readAsText(file, "UTF-8");
            reader.onload = () => resolve(reader.result);
            reader.onerror = error => reject(error);
        });
    }
    //public getBase64(file): Promise<any> {
    //    return new Promise((resolve, reject) => {
    //        const reader = new FileReader();
    //        reader.readAsDataURL(file);
    //        reader.onload = () => resolve(reader.result);
    //        reader.onerror = error => reject(error);
    //    });
    //}

    /**
     * Sets the selected items upon which other actions can be performed.
     * @param {Entities.SCJournal} items The items.
     */
    public setSelectedItems(items: Entities.SCJournal[]): void {
        if (items.length)
            this.scJournal(items[0]);
        else
            this.scJournal(null);
    }
    /*
    private handleError(error: any): void {
        var rsError: Entities.RetailServerError = error || {};
        if (error.length) {
            rsError = error[0] || {};
        }
        var dlgTitle: string = rsError.name;
        var errorMsg: string = rsError._externalLocalizedErrorMessage;
        if (StringExtensions.isNullOrWhitespace(dlgTitle) || StringExtensions.isEmpty(dlgTitle)) {
            dlgTitle = this._context.resources.getString("ERROR_TITLE");
        }
        if (StringExtensions.isNullOrWhitespace(errorMsg) || StringExtensions.isEmpty(errorMsg)) {
            errorMsg = JSON.stringify(rsError);
        }
        // LICENSE
        if (rsError._exceptionType == "LicenseException" && rsError._errorCode == "Microsoft_Dynamics_Commerce_Runtime_InvalidOperation") {
            dlgTitle = rsError._exceptionType;
            errorMsg = this._context.resources.getString("NO_MOBILITY_LICENSE");
        }
        this.displayClientMessage(errorMsg, dlgTitle);
    }

    private displayClientMessage(msg: string, dlgTitle: string): void {
        MessageDialog.show(this._context, msg, dlgTitle);
    }
       */ 
}