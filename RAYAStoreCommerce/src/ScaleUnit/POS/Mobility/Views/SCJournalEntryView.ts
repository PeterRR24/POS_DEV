import ko from "knockout";
import * as NewView from "PosApi/Create/Views";
import { IDataList, IDataListOptions, DataListInteractionMode } from "PosApi/Consume/Controls";
import { Entities } from "../DataService2/DataServiceEntities.g";
import ScJournalEntryViewModel from "./SCJournalEntryViewModel";
import ConfirmMessageDialogModule from "../Controls/DialogControl/ConfirmMessageDialogModule";
import { IConfirmMessageDialogResult } from "../Controls/DialogControl/IConfirmMessageDialogResult";
import { ObjectExtensions } from "PosApi/TypeExtensions";

/**
 * The controller for ScJournalEntryView.
 */
export default class ScJournalEntryView extends NewView.CustomViewControllerBase {
    private _context: NewView.ICustomViewControllerContext;

    public readonly viewModel: ScJournalEntryViewModel;
    public dataList: IDataList<Entities.SCJournalEntry>;

    constructor(context: NewView.ICustomViewControllerContext, options?: any) {
        
        super(context, {
            title: context.resources.getString("STOCK_TAKE_ENTRY_PAGE_TITLE"),
            commandBar: {
                commands: [
                    {
                        name: "deleteEntry",
                        label: 'Delete',
                        icon: NewView.Icons.Delete,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.DeleteOperation();
                        }
                    }
                ]
            }
        });

        this._context = context;

        // Initialize the view model.
        this.viewModel = new ScJournalEntryViewModel(context, options);


    }

    public onReady(element: HTMLElement): void {
        ko.applyBindings(this, element);

        let dataListOptions: IDataListOptions<Entities.SCJournalEntry> = {
            interactionMode: DataListInteractionMode.MultiSelect, // InvokeOrMulti,
            //itemInvoked: this.viewModel.JournalEntryEditor.bind(this.viewModel),
            //selectionChanged: this.viewModel.setSelectedItems.bind(this.viewModel),
            data: this.viewModel.searchJournalEntry,
            columns: [
                {
                    title: this._context.resources.getString("ITEM_ID"),
                    ratio: 7,
                    collapseOrder: 10,
                    minWidth: 50,
                    computeValue: (rowData: Entities.SCJournalEntry): string => { return rowData.ItemId; }
                },
                {
                    title: this._context.resources.getString("SECTION"),
                    ratio: 10,
                    collapseOrder: 11,
                    minWidth: 50,
                    computeValue: (rowData: Entities.SCJournalEntry): string => { return rowData.SectionId; }
                },
                {
                    title: this._context.resources.getString("SHELF"),
                    ratio: 10,
                    collapseOrder: 12,
                    minWidth: 50,
                    computeValue: (rowData: Entities.SCJournalEntry): string => { return rowData.ShelfId; }
                },
                {
                    title: this._context.resources.getString("ITEM_DIM_CONFIG"),
                    ratio: 7,
                    collapseOrder: 1,
                    minWidth: 50,
                    computeValue: (rowData: Entities.SCJournalEntry): string => { return rowData.ConfigId; }
                },
                {
                    title: this._context.resources.getString("ITEM_DIM_SIZE"),
                    ratio: 7,
                    collapseOrder: 2,
                    minWidth: 50,
                    computeValue: (rowData: Entities.SCJournalEntry): string => { return rowData.InventSizeId; }
                },
                {
                    title: this._context.resources.getString("ITEM_DIM_COLOR"),
                    ratio: 7,
                    collapseOrder: 3,
                    minWidth: 50,
                    computeValue: (rowData: Entities.SCJournalEntry): string => { return rowData.InventColorId; }
                },
                {
                    title: this._context.resources.getString("ITEM_DIM_STYLE"),
                    ratio: 7,
                    collapseOrder: 4,
                    minWidth: 50,
                    computeValue: (rowData: Entities.SCJournalEntry): string => { return rowData.InventStyleId; }
                },
                {
                    title: this._context.resources.getString("UNIT"),
                    ratio: 5,
                    collapseOrder: 5,
                    minWidth: 50,
                    computeValue: (rowData: Entities.SCJournalEntry): string => { return rowData.Unit; }
                },
                {
                    title: this._context.resources.getString("STATUS"),
                    ratio: 10,
                    collapseOrder: 6,
                    minWidth: 80,
                    computeValue: (rowData: Entities.SCJournalEntry): string => { return rowData.Status; }
                },
                {
                    title: this._context.resources.getString("QUANTITY_COUNT"),
                    ratio: 10,
                    collapseOrder: 7,
                    minWidth: 80,
                    computeValue: (rowData: Entities.SCJournalEntry): string => { return rowData.QtyCount == undefined ? "" : Number(rowData.QtyCount).toFixed(2); }
                },
                {
                    title: this._context.resources.getString("COUNT_DATE"),
                    ratio: 10,
                    collapseOrder: 8,
                    minWidth: 80,
                    computeValue: (rowData: Entities.SCJournalEntry): string => { return rowData.CountedDate == undefined ? "" : rowData.CountedDate.toLocaleDateString(); }
                },
                {
                    title: this._context.resources.getString("DEVICE_ID"),
                    ratio: 10,
                    collapseOrder: 9,
                    minWidth: 80,
                    computeValue: (rowData: Entities.SCJournalEntry): string => { return rowData.DeviceId; }
                }
            ]
        };
        
        let dataListRootElem: HTMLDivElement = element.querySelector("#listScJournalEntryview") as HTMLDivElement;
        this.dataList = this.context.controlFactory.create(this.context.logger.getNewCorrelationId(), "DataList", dataListOptions, dataListRootElem);

        this.dataList.addEventListener("SelectionChanged", (eventData: { items: Entities.SCJournalEntry[] }) => {
            this.viewModel.setSelectedItems(eventData.items);

            //Update the command states to reflect the current selection state.
            this.state.commandBar.commands.forEach(
                command => command.canExecute = this.viewModel.enableDelete()                
            );
        });
        this.state.isProcessing = true;
        this.viewModel.searchSCJournalEntries().then((): void => {
            // Initialize the data list with what the view model loaded
            this.dataList.data = this.viewModel.searchJournalEntry();
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
        this.dataList.data = this.viewModel.searchJournalEntry();
    }

    /**
     * Search button click event handler.
     */
    public searchClickHandler(): void {
        this.viewModel.searchSCJournalEntries();
    }
    
    public DeleteOperation(): void {
        let dialog: ConfirmMessageDialogModule = new ConfirmMessageDialogModule();
        dialog.open(this._context.resources.getString("CONFIRM_DELETE_JOURNAL_ENTRY"))
            .then((result: IConfirmMessageDialogResult) => {
                if (result.selectedValue !== 'Closed') {
                    this.viewModel.deleteSCJournalEntries();
                }
            }).catch((reason: any) => {
                this._context.logger.logError("ScJournalEntryView deleteOperation confirmation error: " + JSON.stringify(reason));
            });
    }

}