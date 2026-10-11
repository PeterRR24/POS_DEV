import ko from "knockout";
import { ClientEntities } from "PosApi/Entities";
import { ICustomViewControllerContext } from "PosApi/Create/Views";
import { ObjectExtensions, StringExtensions } from "PosApi/TypeExtensions";
import { StockTake } from "../DataService2/DataServiceRequests.g";
import { Entities } from "../DataService2/DataServiceEntities.g";
import { ErrorHelper } from "../../ErrorHelper";

/**
 * The ViewModel for ScJournalTransactionsView.
 */
export default class ScJournalTransactionsViewModel {
    private _context: ICustomViewControllerContext;

    public searchText: ko.Observable<string>;
    public scJournalTransaction: ko.Observable<Entities.SCJournalTransaction>;
    public searchTransactions: ko.ObservableArray<Entities.SCJournalTransaction>;
    public scJournal: Entities.SCJournal;
    public isEnable: ko.Computed<boolean>;

    // Busy states
    public isPageModeBusy: ko.Observable<boolean>;
    //Handlers to refresh list
    public refreshListHandler: (() => void);

    constructor(context: ICustomViewControllerContext, options?: Entities.SCJournal) {
        //super();

        this._context = context;

        // Intialize busy state to false by default
        this.isPageModeBusy = ko.observable(false);

        this.searchText = ko.observable("");
        this.searchTransactions = ko.observableArray([]);
        this.scJournalTransaction = ko.observable(null);
        this.isEnable = ko.computed(() => this.scJournalTransaction() != null);
        this.scJournal = options;

        this.searchSCJournalTransactions();

    }

    public searchSCJournalTransactions(): Promise<Entities.SCJournalTransaction[]> {

        if (StringExtensions.isNullOrWhitespace(this.searchText())) {
            this.searchText("");
        }
        this.searchTransactions([]);
        let search: string = this.searchText();
        
        this.isPageModeBusy(true);
        let request: StockTake.SearchJournalTransactionRequest<StockTake.SearchJournalTransactionResponse>
            = new StockTake.SearchJournalTransactionRequest<StockTake.SearchJournalTransactionResponse>(this.scJournal.Id, search);

        return this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<StockTake.SearchJournalTransactionResponse>) => {
                this.isPageModeBusy(false);
                if (!result.canceled) {
                    this.searchTransactions(result.data.result);
                    if (ObjectExtensions.isFunction(this.refreshListHandler)) {
                        this.refreshListHandler();
                    }
                    return this.searchTransactions();
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this._context.logger.logError("Stock Take Journal Transaction search error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    public countJournalTransactions(quantity: number, comment: string) {

        let recId: number = this.scJournalTransaction().Id;
        let list: Entities.SCJournalTransaction[] = this.searchTransactions();
        let item: Entities.SCJournalTransaction = null;

        this.isPageModeBusy(true);
        let request: StockTake.CountTransactionRequest<StockTake.CountTransactionResponse>
            = new StockTake.CountTransactionRequest<StockTake.CountTransactionResponse>(recId, quantity, comment);

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<StockTake.CountTransactionResponse>) => {
                this.isPageModeBusy(false);
                if (!result.canceled) {
                    item = result.data.result;
                    for (var i = 0; i < list.length; i++) {
                        if (item.Id == list[i].Id && item.RecordId == list[i].RecordId) {
                            list[i] = item;
                            break;
                        }
                    }
                    this.searchTransactions([]);
                    this.searchTransactions(list);
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this._context.logger.logError("Stock Take Journal Transaction Count operation error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    public clearJournalTransactions(comment: string) {

        let recId: number = this.scJournalTransaction().Id;
        let list: Entities.SCJournalTransaction[] = this.searchTransactions();
        let item: Entities.SCJournalTransaction = null;

        this.isPageModeBusy(true);
        let request: StockTake.ClearTransactionRequest<StockTake.ClearTransactionResponse>
            = new StockTake.ClearTransactionRequest<StockTake.ClearTransactionResponse>(recId, comment);

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<StockTake.ClearTransactionResponse>) => {
                this.isPageModeBusy(false);
                if (!result.canceled) {
                    item = result.data.result;
                    for (var i = 0; i < list.length; i++) {
                        if (item.Id == list[i].Id && item.RecordId == list[i].RecordId) {
                            list[i] = item;
                            break;
                        }
                    }
                    this.searchTransactions([]);
                    this.searchTransactions(list);
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this._context.logger.logError("Stock Take Journal Transaction Clear operation error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    /**
     * Sets the selected items upon which other actions can be performed.
     * @param {Entities.SCJournalTransaction} items The SCJournalTransaction.
     */
    public setSelectedItems(items: Entities.SCJournalTransaction[]): void {
        //this.selectedTransactions(items);
        if (items.length)
            this.scJournalTransaction(items[0]);
        else
            this.scJournalTransaction(null);
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