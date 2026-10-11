import ko from "knockout";
import { ClientEntities } from "PosApi/Entities";
import { ICustomViewControllerContext } from "PosApi/Create/Views";
import { ObjectExtensions, StringExtensions } from "PosApi/TypeExtensions";
import { Ranging } from "../DataService2/RangingDataServiceRequests.g";
import { Entities } from "../DataService2/RangingDataServiceEntities.g";
import ProcessingDialogModule from "../Controls/DialogControl/ProcessingDialogModule";
import { IProcessingDialogResult } from "../Controls/DialogControl/IProcessingDialogResult";
import { HardwareStationDeviceActionRequest, HardwareStationDeviceActionResponse } from "PosApi/Consume/Peripherals";
import { ErrorHelper } from "../../ErrorHelper";


/**
 * The ViewModel for ItemStoreLocationView.
 */
export default class ItemStoreLocationViewModel {
    private _context: ICustomViewControllerContext;

    public EntityType: string;
    public searchText: ko.Observable<string>;
    public storeLocationItems: ko.ObservableArray<Entities.StoreLocationItem>;
    public selectedLocationItem: ko.Observable<Entities.StoreLocationItem>;
    public storeSections: ko.ObservableArray<Entities.StoreSection>;
    public storeShelves: ko.ObservableArray<Entities.StoreShelf>;
    public locationTree: ko.ObservableArray<any>;
    public sectionId: ko.Observable<string>;
    public shelfId: ko.Observable<string>;
    public enableRemove: ko.Observable<boolean>;
    public enableClear: ko.Observable<boolean>;
    // Commit
    public enableCommit: ko.Observable<boolean>;
    public commitOption: ko.Observable<string>;
    private stopCommit: ko.Observable<boolean>;
    // Move
    public enableMoveUp: ko.Observable<boolean>;
    public enableMoveDown: ko.Observable<boolean>;

    //public section: Entities.StoreSection;
    //public shelt: Entities.StoreShelf;

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
        this.storeLocationItems = ko.observableArray([]);
        this.storeSections = ko.observableArray([]);
        this.storeShelves = ko.observableArray([]);
        this.locationTree = ko.observableArray([]);
        this.selectedLocationItem = ko.observable(null);
        //this.searchStoreLacationItems();
        this.sectionId = ko.observable("");
        this.shelfId = ko.observable("");
        this.enableRemove = ko.observable(false);        
        this.enableClear = ko.observable(true);

        //this.GetStoreSections();
        //this.GetStoreShelves();
        this.GetStoreSectionAndShelves();
        // Move
        this.enableMoveUp = ko.observable(false);
        this.enableMoveDown = ko.observable(false);
        // Commit
        this.enableCommit = ko.observable(true);
        this.commitOption = ko.observable("");
        this.stopCommit = ko.observable(false);
        var self = this;
        this.commitOption.subscribe(function () {
            self.commitLocations();
        });
        //this.searchStoreLacationItems();
    }

    public searchStoreLacationItems(): Promise<Entities.StoreLocationItem[]> {

        if (this.sectionId() == "" || this.sectionId() == null)
            return this.storeLocationItems([]);
        if (this.shelfId() == "" || this.shelfId() == null)
            return this.storeLocationItems([]);

        this.isPageModeBusy(true);
        let request: Ranging.SelectLocationItemRequest<Ranging.SelectLocationItemResponse>
            = new Ranging.SelectLocationItemRequest<Ranging.SelectLocationItemResponse>(this.sectionId(), this.shelfId());

        return this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Ranging.SelectLocationItemResponse>) => {
                this.isPageModeBusy(false);
                if (!result.canceled) {
                    this.storeLocationItems(result.data.result);
                    if (ObjectExtensions.isFunction(this.refreshListHandler)) {
                        this.refreshListHandler();
                    }
                    return this.storeLocationItems();
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this._context.logger.logError("Get store location item error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    //public locationItemEditor(item: Entities.StoreLocationItem): void {

    //}

    /**
     * Sets the selected items upon which other actions can be performed.
     * @param {Entities.StoreLocationItem} items The StoreLocationItem.
     */
    public setSelectedItem(item: Entities.StoreLocationItem[]): void {
        this.selectedLocationItem(item[0]);        
        this.enableRemove(item.length > 0);
        this.enableMoveUp(false);
        this.enableMoveDown(false);
        if (item.length > 0) {
            let len: number = this.storeLocationItems().length;
            let isLast: boolean = this.storeLocationItems()[len - 1].ItemId == item[0].ItemId;
            let isFIrst: boolean = this.storeLocationItems()[0].ItemId == item[0].ItemId;
            this.enableMoveUp(!isFIrst);
            this.enableMoveDown(!isLast);
        }
        
    }

    public placeItemLocations(location: number, itemId: string) {
        let request: Ranging.PlaceLocationItemRequest<Ranging.PlaceLocationItemResponse>
            = new Ranging.PlaceLocationItemRequest<Ranging.PlaceLocationItemResponse>(this.sectionId(), this.shelfId(), location, itemId);
        this.isPageModeBusy(true);

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Ranging.PlaceLocationItemResponse>) => {
                this.isPageModeBusy(false);
                if (!result.canceled) {
                    this.searchStoreLacationItems();
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this._context.logger.logError("Placing item locations error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
    }

    public clearLocations(option: string): void {

        let shelve: string, section: string;
        if (option == "1") {
            section = this.sectionId();
            shelve = this.shelfId();
        } else if (option == "2") {
            section = this.sectionId();
            shelve = "";
        } else {
            section = "";
            shelve = "";
        }

        let request: Ranging.ClearLocationItemRequest<Ranging.ClearLocationItemResponse>
            = new Ranging.ClearLocationItemRequest<Ranging.ClearLocationItemResponse>(section, shelve);
        this.isPageModeBusy(true);

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Ranging.ClearLocationItemResponse>) => {
                this.isPageModeBusy(false);
                if (!result.canceled) {
                    this.searchStoreLacationItems();
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this._context.logger.logError("Clear locations error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
    }

    private commitLocations(): void {

        let option: string = this.commitOption();
        if (["1", "2", "3"].indexOf(option) == -1)
            return;

        let dialog: ProcessingDialogModule = new ProcessingDialogModule();
        let shelves: Entities.StoreShelf[] = [];
        let shelve: string, section: string;
        this.stopCommit(false);

        if (option == "1") {
            dialog = null;
            shelve = this.shelfId();
            section = this.sectionId();
        } else if (option == "2") {
            shelves = this.storeShelves().filter(ss => ss.SectionId == this.sectionId());
            this.commitAllForSection(dialog, shelves, 0);
            return;
        } else {
            shelves = this.storeShelves();
            this.commitAllForSection(dialog, shelves, 0);
            return;
        }

        this.isPageModeBusy(true);

        let request: Ranging.CommitLocationItemRequest<Ranging.CommitLocationItemResponse> =
            new Ranging.CommitLocationItemRequest<Ranging.CommitLocationItemResponse>(section, shelve);
        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Ranging.CommitLocationItemResponse>) => {
                this.isPageModeBusy(false);
                if (!result.canceled) {
                    this.searchStoreLacationItems();
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this._context.logger.logError("Update HQ locations error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
        
    }

    private commitAllForSection(dialog: ProcessingDialogModule, shelves: Entities.StoreShelf[], i: number): void {

        if (i == shelves.length) {
            $("#processText").html(this._context.resources.getString("COMMIT_LOCATION_SUCCESS_MESSAGE"));
            return;
        }
        let msg: string = StringExtensions.format(this._context.resources.getString("COMMIT_LOCATION_PROCESS_MESSAGE"), shelves[i].SectionName);
        console.log(shelves[i].SectionName);
        if (i == 0) {
            this.isPageModeBusy(true);
            dialog.open(msg, this._context.resources.getString("COMMIT_LOCATION_PROCESS_DIALOG_TITLE"))
                .then((result: IProcessingDialogResult) => {
                    if (result.selectedValue == "Done") {
                        this.isPageModeBusy(false);
                        this.searchStoreLacationItems();
                        this.stopCommit(true);
                    }
                }).catch((reason: any) => {
                    this._context.logger.logError("ItemStoreLocationViewModel update HQ processing dialog error: " + JSON.stringify(reason));
                });
        }
        else if (i > 0) {
            $("#processText").html(msg);
        }        
        if (this.stopCommit()) {
            return;
        }
        
        let request: Ranging.CommitLocationItemRequest<Ranging.CommitLocationItemResponse>
            = new Ranging.CommitLocationItemRequest<Ranging.CommitLocationItemResponse>(shelves[i].SectionId, shelves[i].ShelfId);
        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Ranging.CommitLocationItemResponse>) => {
                if (!result.canceled) {
                    if (this.stopCommit()) {
                        this.isPageModeBusy(false);
                        this.searchStoreLacationItems();
                    } else
                        this.commitAllForSection(dialog, shelves, ++i);
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this.searchStoreLacationItems();
                this._context.logger.logError("Items location update to HQ error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
        
    }
    
    public removeLocations() {

        let item: Entities.StoreLocationItem = this.selectedLocationItem();
        let request: Ranging.RemoveLocationItemRequest<Ranging.RemoveLocationItemResponse>
            = new Ranging.RemoveLocationItemRequest<Ranging.RemoveLocationItemResponse>(this.sectionId(), this.shelfId(), item.Location);
        this.isPageModeBusy(true);

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Ranging.RemoveLocationItemResponse>) => {
                this.isPageModeBusy(false);
                if (!result.canceled) {
                    this.searchStoreLacationItems();
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this._context.logger.logError("Removing locations error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
    }

    public printLocations(): void {

        this.isPageModeBusy(true);
        let count: number = 0;
        let itemList: Entities.StoreLocationItem[] = [];
        let shelf: Entities.StoreShelf = null;
        let request: Ranging.SelectLocationItemRequest<Ranging.SelectLocationItemResponse> = null;
        for (let idx = 0; idx < this.storeShelves().length; idx++) {
            shelf = this.storeShelves()[idx];
            request = new Ranging.SelectLocationItemRequest<Ranging.SelectLocationItemResponse>(shelf.SectionId, shelf.ShelfId);
            this._context.runtime.executeAsync(request)
                .then((result: ClientEntities.ICancelableDataResult<Ranging.SelectLocationItemResponse>) => {
                    count += 1;
                    //console.log("index : " + count);
                    if (!result.canceled) {
                        itemList = itemList.concat(result.data.result);
                        //console.log(result.data.result);
                    }
                    console.log(itemList);
                    if (count === (this.storeShelves().length)) {
                        this.isPageModeBusy(false);
                        this.processPrint(itemList);
                    }
                })
                .catch((reason: any): void => {
                    this.isPageModeBusy(false);
                    this._context.logger.logError("Get store location item error: " + JSON.stringify(reason));
                    ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
                });
        }

        
    }

    private processPrint(items: Entities.StoreLocationItem[]): void {

        if (items.length == 0)
            return;
        this.isPageModeBusy(true);
        let commerce: any = Commerce;
        let storeNumber: string = commerce.ApplicationContext.Instance.storeNumber;
        let requestObj = {"storeName": storeNumber, "strObject": JSON.stringify(items) };
        let hsRequest: HardwareStationDeviceActionRequest<HardwareStationDeviceActionResponse> =
            new HardwareStationDeviceActionRequest("FACEING", "PrintFacing", requestObj);

        this._context.runtime.executeAsync(hsRequest)
            .then((resp: any): any => {
                console.log(resp);
                this.isPageModeBusy(false);
            })
            .catch((reason: any): void => {
                console.log(reason);
                this.isPageModeBusy(false);
            });
    }

    public moveLocations(option: string) {

        let item: Entities.StoreLocationItem = this.selectedLocationItem();
        let upItem: Entities.StoreLocationItem = null,
            downItem: Entities.StoreLocationItem = null,
            allItems: Entities.StoreLocationItem[] = this.storeLocationItems(),
            itemsLen: number = allItems.length,
            newLocation: number = 0;

        if (itemsLen == 1) return;

        for (let i = 0; i < itemsLen; i++) {
            if (allItems[i].ItemId == item.ItemId) {
                if (i != 0) {
                    upItem = allItems[i - 1];
                }
                if (i != itemsLen - 1) {
                    downItem = allItems[i + 1];
                }
                break;
            }
        }
        if (option == 'up') {
            newLocation = upItem.Location;
        }
        else {
            newLocation = downItem.Location;
        }
        let request: Ranging.MoveLocationItemRequest<Ranging.MoveLocationItemResponse>
            = new Ranging.MoveLocationItemRequest<Ranging.MoveLocationItemResponse>(this.sectionId(), this.shelfId(), item.Location, newLocation);
        this.isPageModeBusy(true);

        this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Ranging.MoveLocationItemResponse>) => {
                this.isPageModeBusy(false);
                if (!result.canceled) {
                    this.searchStoreLacationItems();
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this._context.logger.logError("Moving locations error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });
    }

    public GetStoreSectionAndShelves(): void {

        this.GetStoreSections().then(() => {
            this.GetStoreShelves().then((result: Entities.StoreShelf[]) => {
                this.arrangeLocation(result);
            });
        });
    }

    private GetStoreSections(): Promise<Entities.StoreSection[]> {

        if (this.storeSections().length > 0) return this.storeSections([]);

        let request: Ranging.GetStoreSectionsRequest<Ranging.GetStoreSectionsResponse>
            = new Ranging.GetStoreSectionsRequest<Ranging.GetStoreSectionsResponse>();
        this.isPageModeBusy(true);

        return this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Ranging.GetStoreSectionsResponse>) => {
                this.isPageModeBusy(false);
                if (!result.canceled) {
                    this.storeSections(result.data.result);
                    //this.arrangeLocation(null);
                    return this.storeSections();
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this._context.logger.logError("Getting store section's error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    private GetStoreShelves(sectionId: string = ''): Promise<Entities.StoreShelf[]> {

        if (this.storeShelves().length > 0) return this.storeShelves([]);

        let request: Ranging.GetStoreShelvesRequest<Ranging.GetStoreShelvesResponse>
            = new Ranging.GetStoreShelvesRequest<Ranging.GetStoreShelvesResponse>(sectionId);
        this.isPageModeBusy(true);

        return this._context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Ranging.GetStoreShelvesResponse>) => {
                this.isPageModeBusy(false);
                if (!result.canceled) {
                    this.storeShelves(result.data.result);
                    this.arrangeLocation(result.data.result);
                    return this.storeShelves();
                }
            })
            .catch((reason: any): void => {
                this.isPageModeBusy(false);
                this._context.logger.logError("Geting section's shalve error: " + JSON.stringify(reason));
                ErrorHelper.displayLocalizedErrorAsync(this._context, reason);
            });

    }

    private arrangeLocation(arry: Entities.StoreShelf[]): void {
        var secs = this.storeSections(),
            shelves = this.storeShelves(),
            storeShelves = [],
            sections = [];

        if (this.storeSections().length == 0 || this.storeShelves().length == 0) {
            return;
        }

        for (var i = 0; i < secs.length; ++i) {
            var section: Entities.StoreSection = secs[i];
            sections.push({
                SectionId: section.SectionId,
                SectionName: section.SectionName,
                shelves: []
            });
            for (var j = 0; j < shelves.length; ++j) {
                var shelve: Entities.StoreShelf = shelves[j];
                if (section.SectionId == shelve.SectionId) {
                    sections[i].shelves.push(shelve);
                    storeShelves.push(shelve);
                }
            }
        }

        //roots.push({ Id: 0, Name: 'ALL', sections: sections });
        this.locationTree(sections);
        this.storeShelves([]);
        this.storeShelves(storeShelves);
    }
    
    /**
     * Displays an error message corresponding to the provided reason.
     * @param {any} reason The error/reason to display.
     */
    /*
    private handleError(errorObj: any): void {

        let reason: any = errorObj;
        let errorMsg: string = '';
        let errorCode: string = '';
        var dlgTitle: string = this._context.resources.getString("ERROR_TITLE");

        if (errorObj instanceof Array) {
            if (errorObj.length) {
                reason = errorObj[0];
                errorCode = errorObj[0]._errorCode;
            }
        } else {
            errorCode = errorObj._errorCode || '';
        }
        if (reason instanceof ClientEntities.ExtensionError) {
            errorMsg = reason.localizedMessage;
        } else if (reason instanceof Error) {
            errorMsg = reason.message;
            if (errorMsg == '' && errorCode != '') {
                let rrcom: any = Commerce;
                errorMsg = rrcom.ViewModelAdapter.getResourceString(errorCode);
            }
            if (reason.name != '')
                dlgTitle = reason.name;
        } else if (typeof reason === "string") {
            errorMsg = reason;
        } else {
            errorMsg = "An unexpected error occurred.";
        }
        
        this.displayClientMessage(errorMsg, dlgTitle);
    }

    private displayClientMessage(msg: string, dlgTitle: string): void {
        MessageDialog.show(this._context, msg, dlgTitle);
    }
    */
}