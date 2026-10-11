import ko from "knockout";
import * as NewView from "PosApi/Create/Views";
import StockTakeJournalsViewModel from "./StockTakeJournalsViewModel";
//import { CurrencyFormatter } from "PosApi/Consume/Formatters";
import * as Controls from "PosApi/Consume/Controls";
import { Entities } from "../DataService2/DataServiceEntities.g";
import ConfirmMessageDialogModule from "../Controls/DialogControl/ConfirmMessageDialogModule";
import { IConfirmMessageDialogResult } from "../Controls/DialogControl/IConfirmMessageDialogResult";
import { ObjectExtensions } from "PosApi/TypeExtensions";

/**
 * The controller for ItemLabelPrintWzrdView.
 */
export default class StockTakeJournalsView extends NewView.CustomViewControllerBase {
    private _context: NewView.ICustomViewControllerContext;

    public readonly viewModel: StockTakeJournalsViewModel;
    public dataList: Controls.IDataList<Entities.SCJournal>;

    public arrJournalTypes: ko.ObservableArray<any>;
    public page1Visible: ko.Observable<boolean>;
    public page2Visible: ko.Observable<boolean>;
    public btnBackEnable: ko.Observable<boolean>;
    public btnNextEnable: ko.Observable<boolean>;
    public btnOKEnable: ko.Observable<boolean>;


    constructor(context: NewView.ICustomViewControllerContext) {
        
        super(context, {
            title: context.resources.getString("STOCK_TAKE_PAGE_TITLE"),
            commandBar: {
                commands: [
                    {
                        name: "addJournal",
                        label: context.resources.getString('GET_JOURNAL_BUTTON'),
                        icon: NewView.Icons.Add,
                        isVisible: true,
                        canExecute: true,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.AddAxJournalOperation();
                        }
                    },
                    {
                        name: "journalTransactions",
                        label: 'Transactions',
                        icon: NewView.Icons.LightningBolt,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.JournalTransactionsOperation();
                        }
                    },
                    {
                        name: "journalEntries",
                        label: context.resources.getString('ENTRIES_BUTTON'),
                        icon: NewView.Icons.PageList,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.JournalEntriesOperation();
                        }
                    },
                    {
                        name: "calculateJournal",
                        label: context.resources.getString('CALCULATE_BUTTON'),
                        icon: NewView.Icons.Quantity,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.JournalCalcOperation();
                        }
                    },
                    {
                        name: "journalAdjust",
                        label: context.resources.getString('ADJUST_BUTTON'),
                        icon: NewView.Icons.CheckList,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.JournalAdjustOperation();
                        }
                    },
                    {
                        name: "commitJournal",
                        label: 'Commit',
                        icon: NewView.Icons.Accept,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.JournalCommitOperation();
                        }
                    },
                    {
                        name: "deleteJournal",
                        label: 'Delete',
                        icon: NewView.Icons.Delete,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.deleteJournalOperation();
                        }
                    },
                    {
                        name: "exportJournal",
                        label: context.resources.getString('EXPORT_BUTTON'),
                        icon: NewView.Icons.CloudUpload,
                        isVisible: false,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.ExportJournalOperation();
                        }
                    },
                    {
                        name: "importJournal",
                        label: context.resources.getString('IMPORT_BUTTON'),
                        icon: NewView.Icons.Pickup,
                        isVisible: false,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.ImportJournalOperation();
                        }
                    },
                ]
            }
        });

        this._context = context;

        // Initialize the view model.
        this.viewModel = new StockTakeJournalsViewModel(context, this.state);
        this.page1Visible = ko.observable(false);
        this.page2Visible = ko.observable(false);
        this.btnBackEnable = ko.observable(false);
        this.btnNextEnable = ko.observable(false);
        this.btnOKEnable = ko.observable(false);

        this.arrJournalTypes = ko.observableArray([
            { JournalStatus: "Open", Value: 0 },
            { JournalStatus: "Close", Value: 1 }
        ]);

    }

    public onReady(element: HTMLElement): void {
        ko.applyBindings(this, element);
        
        let dataListOptions: Readonly<Controls.IDataListOptions<Entities.SCJournal>> = {
            columns: [
                {
                    title: this._context.resources.getString("JOURNAL_ID"),
                    ratio: 20,
                    collapseOrder: 8,
                    minWidth: 100,
                    computeValue: (rowData: Entities.SCJournal): string => { return rowData.JournalId; }
                },
                {
                    title: this._context.resources.getString("JOURNAL_DESCRIPTION"),
                    ratio: 25,
                    collapseOrder: 7,
                    minWidth: 100,
                    computeValue: (rowData: Entities.SCJournal): string => { return rowData.Description; }
                },
                {
                    title: this._context.resources.getString("JOURNAL_STATUS"),
                    ratio: 10,
                    collapseOrder: 6,
                    minWidth: 100,
                    computeValue: (rowData: Entities.SCJournal): string => { return rowData.Status.toString(); }
                },
                {
                    title: this._context.resources.getString("CALCULATE_MODE"),
                    ratio: 7,
                    collapseOrder: 5,
                    minWidth: 100,
                    computeValue: (rowData: Entities.SCJournal): string => { return rowData.CalcMode.toString(); }
                },
                {
                    title: this._context.resources.getString("LOCATION_MODE"),
                    ratio: 10,
                    collapseOrder: 4,
                    minWidth: 100,
                    computeValue: (rowData: Entities.SCJournal): string => { return rowData.LocationMode.toString(); }
                },
                {
                    title: this._context.resources.getString("CALCULATE_DATE"),
                    ratio: 10,
                    collapseOrder: 3,
                    minWidth: 100,
                    computeValue: (rowData: Entities.SCJournal): string => { return rowData.CreatedDate == undefined ? "" : rowData.CreatedDate.toLocaleDateString(); }
                },
                {
                    title: this._context.resources.getString("COMMIT_DATE"),
                    ratio: 10,
                    collapseOrder: 2,
                    minWidth: 100,
                    computeValue: (rowData: Entities.SCJournal): string => { return rowData.CommitDate == undefined ? "" : rowData.CommitDate.toLocaleDateString(); }
                },
                {
                    title: this._context.resources.getString("STAFF"),
                    ratio: 8,
                    collapseOrder: 1,
                    minWidth: 100,
                    computeValue: (rowData: Entities.SCJournal): string => { return rowData.StaffId; }
                }
            ],
            data: this.viewModel.searchJournals(),
            interactionMode: Controls.DataListInteractionMode.SingleSelect
        };

        let dataListRootElem: HTMLDivElement = element.querySelector("#listJournalview") as HTMLDivElement;
        this.dataList = this.context.controlFactory.create(this.context.logger.getNewCorrelationId(), "DataList", dataListOptions, dataListRootElem);

        this.dataList.addEventListener("SelectionChanged", (eventData: { items: Entities.SCJournal[] }) => {
            this.viewModel.setSelectedItems(eventData.items);
            // Update the command states to reflect the current selection state.
            this.state.commandBar.commands.forEach(
                command => command.canExecute = (
                    ["journalTransactions", "journalEntries", "calculateJournal", "journalAdjust", "commitJournal", "deleteJournal"]
                        .some(name => name == command.name && this.viewModel.isEnable()) || (command.name == "addJournal")
                )
            );
        });

        this.state.isProcessing = true;
        this.viewModel.searchSCJournals().then((): void => {
            // Initialize the data list with what the view model loaded
            this.dataList.data = this.viewModel.searchJournals();
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
        // Initialize the data list with what the view model loaded
        this.dataList.data = this.viewModel.searchJournals();
    }

    /**
     * Search button click event handler.
     */
    public searchClickHandler(): void {
        this.viewModel.searchSCJournals();
    }

    public AddAxJournalOperation(): void {
        this.page1Visible(true);
        this.page2Visible(false);
        this.btnBackEnable(false);
        this.btnNextEnable(false);
        this.btnOKEnable(false);
        this.viewModel.openJournalDialog();
    }

    public deleteJournalOperation(): void {
        let dialog: ConfirmMessageDialogModule = new ConfirmMessageDialogModule();
        dialog.open(this._context.resources.getString("CONFIRM_DELETE_JOURNAL"))
            .then((result: IConfirmMessageDialogResult) => {
                if (result.selectedValue !== 'Closed') {
                    this.viewModel.deleteJournal();
                }
            }).catch((reason: any) => {
                this._context.logger.logError("StockTakeJournalsView deleteJournalOperation confirmation: " + JSON.stringify(reason));
            });
    }

    public JournalCommitOperation(): void {
        this.viewModel.commitJournal();
        //let dialog: ConfirmMessageDialogModule = new ConfirmMessageDialogModule();
        //dialog.open("Are you sure you want to commit selected stock take journal?")
        //    .then((result: IConfirmMessageDialogResult) => {
        //        if (result.selectedValue !== 'Closed') {
        //            this.viewModel.commitJournal();
        //        }
        //    }).catch((reason: any) => {
        //        this._context.logger.logError("StockTakeJournalsView journal commit operation confirmation: " + JSON.stringify(reason));
        //    });
    }

    public JournalAdjustOperation(): void {
        //this.viewModel.adjustJournal();
        let dialog: ConfirmMessageDialogModule = new ConfirmMessageDialogModule();
        dialog.open(this._context.resources.getString("CONFIRM_ADJUST_JOURNAL"))
            .then((result: IConfirmMessageDialogResult) => {
                if (result.selectedValue !== 'Closed') {
                    this.viewModel.adjustJournal();
                }
            }).catch((reason: any) => {
                this._context.logger.logError("StockTakeJournalsView journal adjust operation confirmation: " + JSON.stringify(reason));
            });
    }

    public JournalCalcOperation(): void {
        //this.viewModel.calcualteJournal();
        let dialog: ConfirmMessageDialogModule = new ConfirmMessageDialogModule();
        dialog.open(this._context.resources.getString("CONFIRM_CALCULATE_JOURNAL"))
            .then((result: IConfirmMessageDialogResult) => {
                if (result.selectedValue !== 'Closed') {
                    this.viewModel.calcualteJournal();
                }
            }).catch((reason: any) => {
                this._context.logger.logError("StockTakeJournalsView journal calcualte operation confirmation: " + JSON.stringify(reason));
            });
    }

    public getJournal(self, data, e): void {

        self.viewModel.axJournal(data.Journal);
        self.viewModel.selectedJournalID(data.Journal.JournalId);
        self.viewModel.selectedCMValue("0");
        self.viewModel.selectedLMValue("0");
        self.btnNextEnable(true);

        let rooNodes: NodeListOf<HTMLScriptElement> = <NodeListOf<HTMLScriptElement>>document.querySelectorAll('div#divSCJournals div.listViewLine');
        for (var i = 0; i < rooNodes.length; i++) {
            rooNodes[i].classList.remove("accentBackground");
            rooNodes[i].classList.add("tablerow");
            let elements = <NodeListOf<HTMLInputElement>><unknown>rooNodes[i].getElementsByTagName('input');
            for (var j = 0; j < elements.length; j++) {
                let elmt = elements[j];
                if (elmt.type == "hidden" && elmt.id == data.Journal.RecordId) {
                    rooNodes[i].classList.add("accentBackground");
                    rooNodes[i].classList.remove("tablerow");
                }
            }
        }
    }

    public ImportJournalOperation(): void {
        document.getElementById('filePath').click();
    }

    public FileDialogHandler(self, evt): void {
        var data = evt.currentTarget.files[0];
        this.viewModel.importOfflineStockTake(data);
    }

    public ExportJournalOperation(): void {
        this.viewModel.exportJournal();
    }

    public NextHandler(): void {
        this.page1Visible(false);
        this.page2Visible(true);
        this.btnBackEnable(true);
        this.btnNextEnable(false);
        this.btnOKEnable(true);
    }

    public BackHandler(): void {
        this.page1Visible(true);
        this.page2Visible(false);
        this.btnBackEnable(false);
        this.btnNextEnable(true);
        this.btnOKEnable(false);
    }

    public OkDialogHandler(): void {

        this.viewModel.isShowDialog(false);
        this.viewModel.ImportJournal();
    }

    public CancelDialogHandler(): void {

        this.viewModel.isShowDialog(false);

    }

    public JournalTransactionsOperation(): void {

        this._context.navigator.navigate("ScJournalTransactionsView", this.viewModel.scJournal());
    }

    public JournalEntriesOperation(): void {
        let options = {
            key: "SCJournal",
            value: this.viewModel.scJournal()
        };
        this._context.navigator.navigate("ScJournalEntryView", options);
    }

}
