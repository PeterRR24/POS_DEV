import ko from "knockout";
import * as NewView from "PosApi/Create/Views";
import { IDataList, IDataListOptions, DataListInteractionMode } from "PosApi/Consume/Controls";
import { Entities } from "../DataService2/RangingDataServiceEntities.g";
import ItemStoreLocationViewModel from "./ItemStoreLocationViewModel";
import ClearCommitOptionsDialogModule from "../Controls/DialogControl/ClearCommitOptionsDialogModule";
import { IClearCommitOptionsDialogResult } from "../Controls/DialogControl/IClearCommitOptionsDialogResult";
import ConfirmMessageDialogModule from "../Controls/DialogControl/ConfirmMessageDialogModule";
import { IConfirmMessageDialogResult } from "../Controls/DialogControl/IConfirmMessageDialogResult";
import { ArrayExtensions, StringExtensions, ObjectExtensions } from "PosApi/TypeExtensions";

/**
 * The controller for ItemStoreLocationView.
 */
export default class ItemStoreLocationView extends NewView.CustomViewControllerBase {
    private _context: NewView.ICustomViewControllerContext;

    public readonly viewModel: ItemStoreLocationViewModel;
    public dataList: IDataList<Entities.StoreLocationItem>;
    public showFaces: ko.Observable<boolean>;
    private listColumn: ko.ObservableArray<any>;

    constructor(context: NewView.ICustomViewControllerContext, options?: any) {
        // Do not save in history
        super(context, {
            title: context.resources.getString("RANGING_PAGE_TITLE"),
            commandBar: {
                commands: [
                    {
                        name: "commitLocations",
                        label: context.resources.getString('UPDATE_HQ_BUTTON'),
                        icon: NewView.Icons.Accept,
                        isVisible: true,
                        canExecute: true,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.CommitOperation();
                        }
                    },
                    {
                        name: "clearLocations",
                        label: 'Clear',
                        icon: NewView.Icons.Cancel,
                        isVisible: true,
                        canExecute: true,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.ClearOperation();
                        }
                    },
                    {
                        name: "moveDownItem",
                        label: context.resources.getString('MOVE_DOWN_BUTTON'),
                        icon: NewView.Icons.PreviewPrevious,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.MoveOperation.bind(this, 'down');
                        }
                    },
                    {
                        name: "moveUpItem",
                        label: context.resources.getString('MOVE_UP_BUTTON'),
                        icon: NewView.Icons.PreviewNext,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.MoveOperation.bind(this, 'up');
                        }
                    },
                    {
                        name: "removeItem",
                        label: context.resources.getString('REMOVE_BUTTON'),
                        icon: NewView.Icons.Delete,
                        isVisible: true,
                        canExecute: false,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.RemoveOperation();
                        }
                    },
                    {
                        name: "printFacingReport",
                        label: context.resources.getString('PRINT_REPORT_BUTTON'),
                        icon: NewView.Icons.Invoice,
                        isVisible: true,
                        canExecute: true,
                        execute: (args: NewView.CustomViewControllerExecuteCommandArgs): void => {
                            this.PrintingOperation();
                        }
                    }
                ]
            }
        });

        this._context = context;
        let isFacing: boolean = options.isFaceing == 1;

        // Initialize the view model.
        this.viewModel = new ItemStoreLocationViewModel(context, options);

        this.listColumn = ko.observableArray([]);
        this.showFaces = ko.observable(false);
        this.showFaces(isFacing);

        if (isFacing) {
            this.listColumn(this.ColumnsWithFaces());
        } else {
            this.listColumn(this.ColumnsWithoutFaces());
        }
        
    }

    public onReady(element: HTMLElement): void {
        ko.applyBindings(this, element);
        
        let dataListOptions: Readonly<IDataListOptions<Entities.StoreLocationItem>> = {
            interactionMode: DataListInteractionMode.SingleSelect,
            data: this.viewModel.storeLocationItems(),
            columns: this.listColumn()
        };
        
        let dataListRootElem: HTMLDivElement = element.querySelector("#listItemStoreLocation") as HTMLDivElement;
        this.dataList = this.context.controlFactory.create(this.context.logger.getNewCorrelationId(), "DataList", dataListOptions, dataListRootElem);

        this.dataList.addEventListener("SelectionChanged", (eventData: { items: Entities.StoreLocationItem[] }) => {
            this.viewModel.setSelectedItem(eventData.items);

            ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'clearLocations').canExecute = this.viewModel.enableClear();
            ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'commitLocations').canExecute = this.viewModel.enableCommit();
            ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'moveDownItem').canExecute = this.viewModel.enableMoveDown();
            ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'moveUpItem').canExecute = this.viewModel.enableMoveUp();
            ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'removeItem').canExecute = this.viewModel.enableRemove();
            //ArrayExtensions.firstOrUndefined(this.state.commandBar.commands, (c) => c.name === 'printFacingReport').canExecute = this.viewModel.allowDelete();
        });

        //this.viewModel.searchStoreLacationItems().then((): void => {
        //    // Initialize the data list with what the view model loaded
        //    this.dataList.data = this.viewModel.storeLocationItems();
        //});
        this.viewModel.refreshListHandler = (() => {
            this.RefreshList();
        });
    }

    private RefreshList() {
        this.dataList.data = this.viewModel.storeLocationItems();
    }

    private ColumnsWithFaces():any[] {
        let columns: any[] = [];
        columns = [
            {
                title: this._context.resources.getString("LOCATION"),
                ratio: 15,
                collapseOrder: 3,
                minWidth: 50,
                computeValue: (rowData: Entities.StoreLocationItem): number => { return rowData.Location; }
            },
            {
                title: this._context.resources.getString("ITEM_ID"),
                ratio: 20,
                collapseOrder: 4,
                minWidth: 50,
                computeValue: (rowData: Entities.StoreLocationItem): string => { return rowData.ItemId; }
            },
            {
                title: this._context.resources.getString("ITEM_DESCRIPTION"),
                ratio: 35,
                collapseOrder: 2,
                minWidth: 50,
                computeValue: (rowData: Entities.StoreLocationItem): string => { return rowData.Description; }
            },
            {
                title: this._context.resources.getString("UNIT"),
                ratio: 10,
                collapseOrder: 1,
                minWidth: 50,
                computeValue: (rowData: Entities.StoreLocationItem): string => { return rowData.UnitId; }
            },
            {
                title: this._context.resources.getString("FACES"),
                ratio: 20,
                collapseOrder: 5,
                minWidth: 50,
                computeValue: (rowData: Entities.StoreLocationItem): number => { return rowData.FacingCount; }
            }
        ];
        return columns;
    }

    private ColumnsWithoutFaces(): any[] {
        let columns: any[] = [];
        columns = [
            {
                title: this._context.resources.getString("LOCATION"),
                ratio: 20,
                collapseOrder: 3,
                minWidth: 50,
                computeValue: (rowData: Entities.StoreLocationItem): number => { return rowData.Location; }
            },
            {
                title: this._context.resources.getString("ITEM_ID"),
                ratio: 20,
                collapseOrder: 4,
                minWidth: 50,
                computeValue: (rowData: Entities.StoreLocationItem): string => { return rowData.ItemId; }
            },
            {
                title: this._context.resources.getString("ITEM_DESCRIPTION"),
                ratio: 45,
                collapseOrder: 2,
                minWidth: 50,
                computeValue: (rowData: Entities.StoreLocationItem): string => { return rowData.Description; }
            },
            {
                title: this._context.resources.getString("UNIT"),
                ratio: 15,
                collapseOrder: 1,
                minWidth: 50,
                computeValue: (rowData: Entities.StoreLocationItem): string => { return rowData.UnitId; }
            }
        ];
        return columns;
    }

    public onShown() {
        
        $('.rootCategory').on('click', 'ul.category label', function (self, evt) {
            $('.rootCategory > .product > ul.category label').removeClass('accentBackground');
            $(self.currentTarget).addClass("accentBackground");
        });

        //let rooNodes: NodeListOf<HTMLScriptElement> = <NodeListOf<HTMLScriptElement>>document.querySelectorAll('ul.rootCategory ul.category');
        //for (var i = 0; i < rooNodes.length; i++) {
        //    rooNodes[i].addEventListener('click', function (e) {
        //        let element = <HTMLScriptElement>e.target;
        //        if (element && element.tagName.toLowerCase() === 'label') {
        //            let eleTags: NodeListOf<HTMLScriptElement> = <NodeListOf<HTMLScriptElement>>document.querySelectorAll('.rootCategory ul.category label');
        //            for (var i = 0; i < eleTags.length; i++) {
        //                eleTags[i].classList.remove("accentBackground");
        //            }
        //            element.classList.add("accentBackground");
        //        }
        //    });
        //}

        //document.getElementById("txtfiletoread").addEventListener("change", function (evt: any) {
        //    var input = evt.target;
        //    var reader = new FileReader();
        //    reader.onload = function () {
        //        console.log(reader.result);
        //    };
        //    reader.readAsText(input.files[0]);
        //});
        
        
    }
    
    public dispose(): void {
        ObjectExtensions.disposeAllProperties(this);
    }

    public LocationClickHandler(self, data, element): void {
        //var src: HTMLElement = element.currentTarget;
        //$(src).siblings('ul').slideToggle('slow');
        self.viewModel.sectionId(data.SectionId == undefined ? "" : data.SectionId);
        self.viewModel.shelfId(data.ShelfId == undefined ? "" : data.ShelfId);
        self.viewModel.searchStoreLacationItems();
    }

    public PlaceOperation(): void {
        this.viewModel.placeItemLocations(0, "");
    }

    public ClearOperation(): void {
        
        let dialog: ClearCommitOptionsDialogModule = new ClearCommitOptionsDialogModule(this.viewModel.shelfId());
        dialog.open(this._context.resources.getString("CLEAR_LOCATION_OPTION_SELECT_MESSAGE"), this._context.resources.getString("CLEAR_LOCATION_OPTIONS_DIALOG_TITLE"))
            .then((result: IClearCommitOptionsDialogResult) => {
                if (result.selectedValue !== '0') {
                    if (result.selectedValue == "1")
                        this.viewModel.clearLocations(result.selectedValue);
                    else {
                        let confirmDialog: ConfirmMessageDialogModule = new ConfirmMessageDialogModule();
                        confirmDialog.open(this._context.resources.getString("CONFIRM_CLEAR_LOCATION"))
                            .then((confirmResult: IConfirmMessageDialogResult) => {
                                if (confirmResult.selectedValue == "Yes") {
                                    this.viewModel.clearLocations(result.selectedValue);
                                }
                            }).catch((reason: any) => {
                                this._context.logger.logError("ItemStoreLocationView clearOperation options: " + JSON.stringify(reason));
                            });
                    }                    
                }
            }).catch((reason: any) => {
                this._context.logger.logError("ItemStoreLocationView clearOperation options: " + JSON.stringify(reason));
            });

        //var iframe = document.getElementById('iframe');
        //iframe.setAttribute("src", 'data:text/html;charset=utf-8,' + this.htmlStr());
        //iframe.setAttribute("srcdoc", this.htmlStr())
    }

    public CommitOperation(): void {
        
        let dialog1: ClearCommitOptionsDialogModule = new ClearCommitOptionsDialogModule(this.viewModel.shelfId());
        dialog1.open(this._context.resources.getString("COMMIT_LOCATION_OPTION_SELECT_MESSAGE"), this._context.resources.getString("COMMIT_LOCATION_OPTIONS_DIALOG_TITLE"))
            .then((result: IClearCommitOptionsDialogResult) => {
                if (result.selectedValue !== '0') {
                    this.viewModel.commitOption("");
                    this.viewModel.commitOption(result.selectedValue);
                }
            }).catch((reason: any) => {
                this._context.logger.logError("ItemStoreLocationView commitOperation options: " + JSON.stringify(reason));
            });
    }
    
    public MoveOperation(self, option: string): void {

        self.viewModel.moveLocations(option);   
        
    }
    
    public RemoveOperation(): void {
        let msg: string = StringExtensions.format(this._context.resources.getString("CONFIRM_REMOVE_LOCATION"), this.viewModel.selectedLocationItem().Description);
        let confirmDialog: ConfirmMessageDialogModule = new ConfirmMessageDialogModule();
        confirmDialog.open(msg)
            .then((confirmResult: IConfirmMessageDialogResult) => {
                if (confirmResult.selectedValue == "Yes") {
                    this.viewModel.removeLocations();
                }
            }).catch((reason: any) => {
                this._context.logger.logError("ItemStoreLocationView remove operation: " + JSON.stringify(reason));
            });
    }

    public PrintingOperation(): void{
        this.viewModel.printLocations();
    }
}