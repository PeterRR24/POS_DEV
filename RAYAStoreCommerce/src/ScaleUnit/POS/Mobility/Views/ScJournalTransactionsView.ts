import ko from "knockout";
import * as NewView from "PosApi/Create/Views";
import { IDataList, IDataListOptions, DataListInteractionMode } from "PosApi/Consume/Controls";
import { Entities } from "../DataService2/DataServiceEntities.g";
import ScJournalTransactionsViewModel from "./ScJournalTransactionsViewModel";
import CountingDialogModule from "../Controls/DialogControl/CountingDialogModule";
import { ICountingDialogResult } from "../Controls/DialogControl/ICountingDialogResult";
import { ObjectExtensions } from "PosApi/TypeExtensions";

/**
 * The controller for ScJournalTransactionsView.
 */
export default class ScJournalTransactionsView extends NewView.CustomViewControllerBase {
    private _context: NewView.ICustomViewControllerContext;

    public readonly viewModel: ScJournalTransactionsViewModel;
    public dataList: IDataList<Entities.SCJournalTransaction>;

    constructor(context: NewView.ICustomViewControllerContext, options: Entities.SCJournal) {
        
        super(context, {
            title: context.resources.getString("JOURNAL_TRANSACTIONS_PAGE_TITLE"),
            commandBar: {
                commands: [
                    {
                        name: "transactionEntry",
                        label: context.resources.getString('ENTRIES_BUTTON'),
                        icon: NewView.Icons.PageList,
                        isVisible: true,
                        canExecute: true,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.EntriesOperation();
                        }
                    },
                    {
                        name: "clearTransaction",
                        label: 'Clear',
                        icon: NewView.Icons.Cancel,
                        isVisible: true,
                        canExecute: true,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.ClearTransOperation();
                        }
                    },
                    {
                        name: "countTransaction",
                        label: 'Count',
                        icon: NewView.Icons.Count,
                        isVisible: true,
                        canExecute: true,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.CountTransOperation();
                        }
                    }
                ]
            }
        });

        this._context = context;

        // Initialize the view model.
        this.viewModel = new ScJournalTransactionsViewModel(context, options);

    }

    public onReady(element: HTMLElement): void {
        ko.applyBindings(this, element);

        let dataListOptions: IDataListOptions<Entities.SCJournalTransaction> = {
            interactionMode: DataListInteractionMode.SingleSelect,
            data: this.viewModel.searchTransactions,
            columns: [
                {
                    title: this._context.resources.getString("ITEM_ID"),
                    ratio: 7,
                    collapseOrder: 10,
                    minWidth: 40,
                    computeValue: (rowData: Entities.SCJournalTransaction): string => { return rowData.ItemId; }
                },
                {
                    title: this._context.resources.getString("ITEM_NAME"),
                    ratio: 20,
                    collapseOrder: 11,
                    minWidth: 80,
                    computeValue: (rowData: Entities.SCJournalTransaction): string => { return rowData.ItemName; }
                },
                {
                    title: this._context.resources.getString("ITEM_DIM_CONFIG"),
                    ratio: 7,
                    collapseOrder: 1,
                    minWidth: 50,
                    computeValue: (rowData: Entities.SCJournalTransaction): string => { return rowData.ConfigId; }
                },
                {
                    title: this._context.resources.getString("ITEM_DIM_SIZE"),
                    ratio: 7,
                    collapseOrder: 2,
                    minWidth: 50,
                    computeValue: (rowData: Entities.SCJournalTransaction): string => { return rowData.InventSizeId; }
                },
                {
                    title: this._context.resources.getString("ITEM_DIM_COLOR"),
                    ratio: 7,
                    collapseOrder: 3,
                    minWidth: 50,
                    computeValue: (rowData: Entities.SCJournalTransaction): string => { return rowData.InventColorId; }
                },
                {
                    title: this._context.resources.getString("ITEM_DIM_STYLE"),
                    ratio: 7,
                    collapseOrder: 4,
                    minWidth: 50,
                    computeValue: (rowData: Entities.SCJournalTransaction): string => { return rowData.InventStyleId; }
                },
                {
                    title: this._context.resources.getString("UNIT"),
                    ratio: 5,
                    collapseOrder: 5,
                    minWidth: 40,
                    computeValue: (rowData: Entities.SCJournalTransaction): string => { return rowData.Unit; }
                },
                {
                    title: this._context.resources.getString("QUANTITY"),
                    ratio: 8,
                    collapseOrder: 6,
                    minWidth: 70,
                    computeValue: (rowData: Entities.SCJournalTransaction): string => { return Number(rowData.Quantity).toFixed(2); }
                },
                {
                    title: this._context.resources.getString("QUANTITY_COUNT"),
                    ratio: 8,
                    collapseOrder: 7,
                    minWidth: 70,
                    computeValue: (rowData: Entities.SCJournalTransaction): string => { return rowData.QtyCount == undefined ? "" : Number(rowData.QtyCount).toFixed(2); }
                },
                {
                    title: this._context.resources.getString("QUANTITY_ADJUST"),
                    ratio: 8,
                    collapseOrder: 8,
                    minWidth: 70,
                    computeValue: (rowData: Entities.SCJournalTransaction): string => { return rowData.QtyAdjust == undefined ? "" : Number(rowData.QtyAdjust).toFixed(2); }
                },
                {
                    title: this._context.resources.getString("COUNT_DATE"),
                    ratio: 8,
                    collapseOrder: 9,
                    minWidth: 60,
                    computeValue: (rowData: Entities.SCJournalTransaction): string => { return rowData.CountedDate == undefined ? "" : rowData.CountedDate.toLocaleDateString(); }
                },
                {
                    title: this._context.resources.getString("COMMENT"),
                    ratio: 8,
                    collapseOrder: 12,
                    minWidth: 70,
                    computeValue: (rowData: Entities.SCJournalTransaction): string => { return rowData.Comment; }
                }
            ]
        };
        
        let dataListRootElem: HTMLDivElement = element.querySelector("#listScJournalTransview") as HTMLDivElement;
        this.dataList = this.context.controlFactory.create(this.context.logger.getNewCorrelationId(), "DataList", dataListOptions, dataListRootElem);

        this.dataList.addEventListener("SelectionChanged", (eventData: { items: Entities.SCJournalTransaction[] }) => {
            this.viewModel.setSelectedItems(eventData.items);
            // Update the command states to reflect the current selection state.
            this.state.commandBar.commands.forEach(
                command => command.canExecute = (
                    ["transactionEntry", "clearTransaction", "countTransaction"]
                        .some(name => name == command.name && this.viewModel.isEnable())
                )
            );
        });
        this.state.isProcessing = true;
        this.viewModel.searchSCJournalTransactions().then((): void => {
            // Initialize the data list with what the view model loaded
            this.dataList.data = this.viewModel.searchTransactions();
            this.state.isProcessing = false;
        });
        this.viewModel.refreshListHandler = (() => {
            this.RefreshList();
        });
    }

    /**
     * Called when the object is disposed.
     */
    public dispose(): void {
        ObjectExtensions.disposeAllProperties(this);
    }

    private RefreshList() {
        this.dataList.data = this.viewModel.searchTransactions();
    }

    /**
     * Search button click event handler.
     */
    public searchClickHandler(): void {
        this.viewModel.searchSCJournalTransactions();
    }

    public CountTransOperation(): void {
        let item: Entities.SCJournalTransaction = this.viewModel.scJournalTransaction();
        let qty: string = isNaN(item.QtyCount) ? "" : item.QtyCount.toString();
        let dialog: CountingDialogModule = new CountingDialogModule(true);
        dialog.open(item.ItemName, qty, this._context.resources.getString("QUANTITY_COUNT_DIALOG_TITLE"))
            .then((result: ICountingDialogResult) => {
                if (result.result !== 'Closed') {                    
                    this.viewModel.countJournalTransactions(result.quantity, result.comment);
                }
            }).catch((reason: any) => {
                this._context.logger.logError("ScJournalTransactionsView.openTemplatedDialog: " + JSON.stringify(reason));
            });
    }

    public ClearTransOperation(): void {
        
        let item: Entities.SCJournalTransaction = this.viewModel.scJournalTransaction();
        let dialog: CountingDialogModule = new CountingDialogModule(false);
        dialog.open(item.ItemName, "", this._context.resources.getString("COMMENT_DIALOG_TITLE"))
            .then((result: ICountingDialogResult) => {
                if (result.result !== 'Closed') {
                    this.viewModel.clearJournalTransactions(result.comment);
                }
            }).catch((reason: any) => {
                this._context.logger.logError("ScJournalTransactionsView.openTemplatedDialog: " + JSON.stringify(reason));
            });
    }

    public EntriesOperation(): void {
        let options = {
            key: "SCJournalTransaction",
            value: this.viewModel.scJournalTransaction()
        };
        this._context.navigator.navigate("ScJournalEntryView", options);
    }

}