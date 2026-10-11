import ko from "knockout";
import { ClientEntities } from "PosApi/Entities";
import { ICustomViewControllerContext } from "PosApi/Create/Views";
//import KnockoutExtensionViewModelBase from "./BaseClasses/KnockoutExtensionViewModelBase";
//import MessageDialog from "../Controls/DialogControl/MessageDialog";
import { ObjectExtensions, StringExtensions } from "PosApi/TypeExtensions";
import { StockTake } from "../DataService2/DataServiceRequests.g";
import { Entities } from "../DataService2/DataServiceEntities.g";
import { ErrorHelper } from "../../ErrorHelper";


/**
 * The ViewModel for ScJournalTransactionsView.
 */
export default class ScJournalEntryViewModel {
    private _context: ICustomViewControllerContext;

    public EntityType: string;
    public searchText: ko.Observable<string>;
    public scJournalEntry: ko.ObservableArray<Entities.SCJournalEntry>;
    public searchJournalEntry: ko.ObservableArray<Entities.SCJournalEntry>;
    public scJournal: Entities.SCJournal;
    public scJournalTrans: Entities.SCJournalTransaction;
    public enableDelete: ko.Computed<boolean>;

    // Busy states
    public isPageModeBusy: ko.Observable<boolean>;
    //Handlers to refresh list
    public refreshListHandler: (() => void);

    constructor(context: ICustomViewControllerContext, options?: any) {
        //super();

        this._context = context;

        // Intialize busy state to false by default
        this.isPageModeBusy = ko.observable(false);

        this.searchText = ko.observable("");
        this.searchJournalEntry = ko.observableArray([]);
        this.scJournalEntry = ko.observableArray([]);
        this.enableDelete = ko.computed(() => this.scJournalEntry().length > 0);

        this.EntityType = options.key;
        if (this.EntityType == "SCJournal") {
            this.scJournal = options.value;
            this.searchSCJournalEntries();
        } else {
            this.scJournalTrans = options.value;
            this.searchJournalTransEntries();
        }

    }


    public searchSCJournalEntries(): Promise<Entities.SCJournalEntry[]> {

        if (StringExtensions.isNullOrWhitespace(this.searchText())) {
            this.searchText("");
        }
        this.searchJournalEntry([]);
        let search: string = this.searchText();

        this.isPageModeBusy(true);
        let request: StockTake.SearchSCJournalEntryRequest<StockTake.SearchSCJournalEntryResponse>
            = new StockTake.SearchSCJournalEntryRequest<StockTake.SearchSCJournalEntryResponse>(this.scJournal.Id, search);

        return this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<StockTake.SearchSCJournalEntryResponse>) => {
                this.isPageModeBusy(false);
                if (!result.canceled) {
                    this.searchJournalEntry(result.data.result);
                    if (ObjectExtensions.isFunction(this.refreshListHandler)) {
                        this.refreshListHandler();
                    }
                    return this.searchJournalEntry();
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this._context.logger.logError("Stock Take Journal Transaction search error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    public searchJournalTransEntries() {

        this.searchJournalEntry([]);
        this.isPageModeBusy(true);
        let request: StockTake.SCJournalEntryRequest<StockTake.SCJournalEntryResponse>
            = new StockTake.SCJournalEntryRequest<StockTake.SCJournalEntryResponse>(this.scJournalTrans.Id);

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<StockTake.SCJournalEntryResponse>) => {
                this.isPageModeBusy(false);
                if (!result.canceled) {
                    this.searchJournalEntry(result.data.result);
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this._context.logger.logError("Stock Take Journal Transaction search error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    public deleteSCJournalEntries() {
        
        this.isPageModeBusy(true);
        let recIds: number[] = [];
        for (var i = 0; i < this.scJournalEntry().length; i++) {
            recIds.push(this.scJournalEntry()[i].Id);
        }
        let request: StockTake.DeleteSCJournalEntryRequest<StockTake.DeleteSCJournalEntryResponse>
            = new StockTake.DeleteSCJournalEntryRequest<StockTake.DeleteSCJournalEntryResponse>(recIds);

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<StockTake.DeleteSCJournalEntryResponse>) => {
                this.isPageModeBusy(false);
                if (this.EntityType == "SCJournal") {
                    this.searchSCJournalEntries();
                } else {
                    this.searchJournalTransEntries();
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this._context.logger.logError("Stock Take Journal Transaction search error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    public JournalEntryEditor(item: Entities.SCJournalEntry) {

    }

    /**
     * Sets the selected items upon which other actions can be performed.
     * @param {Entities.SCJournalEntry} items The SCJournalEntry.
     */
    public setSelectedItems(items: Entities.SCJournalEntry[]): void {
        this.scJournalEntry(items);
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
        this.displayClientMessage(errorMsg, dlgTitle);
    }

    private displayClientMessage(msg: string, dlgTitle: string): void {
        MessageDialog.show(this._context, msg, dlgTitle);
    }
    */
}