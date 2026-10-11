
import * as Dialogs from "PosApi/Create/Dialogs";
import { ObjectExtensions } from "PosApi/TypeExtensions";
import { IAddLabelDialogResult } from "./IAddLabelDialogResult";
import * as Controls from "PosApi/Consume/Controls";
import ko from "knockout";

type AddLabelDialogResolve = (value: IAddLabelDialogResult) => void;
type AddLabelDialogReject = (reason: any) => void;

export default class AddLabelDialogModule extends Dialogs.ExtensionTemplatedDialogBase {
    public messagePassedToDialog: any;
    public userEnteredValue: any;
    public selectedLabelType: any;
    public showLabelType: boolean;
    private isVariantSelected: boolean;
    private showQuantity: boolean;
    private labelCodes: any;
    private selectedCode: any;
    private priceDate: any;
    private resolve: AddLabelDialogResolve;
    private priceDatePicker: Controls.IDatePicker;

    constructor(labelType: number, showLabelType: boolean, codeId: number = 0, isVariantSelected: boolean = true, showQuantity: boolean = true) {
        super();
        this.userEnteredValue = ko.observable("");
        this.selectedLabelType = ko.observable(labelType.toString());
        this.showLabelType = showLabelType || false;
        this.isVariantSelected = isVariantSelected;
        this.showQuantity = showQuantity;
        this.labelCodes = ko.observableArray([]);
        this.selectedCode = ko.observable(null);
        this.selectedCode({ Id: codeId });
        this.priceDate = ko.observable(null);

        let date: Date = new Date();
        let today: Date = new Date(date.getFullYear(), date.getMonth(), date.getDate());
        this.priceDate(today);

        var self = this;
        this.userEnteredValue.subscribe(function () {
            self.enableOk();
        });
        //this.priceDatePicker = new DatePicker.DatePicker({
        //    initialValue: new Date(),
        //    onChange: this.priceDateChangedHandler.bind(this)
        //});
        
    }

    public onReady(el: HTMLElement): void {
        ko.applyBindings(this, el);
        // Initialize DatePicker options
        let datePickerOptios: Controls.IDatePickerOptions = { date: new Date(), enabled: true };
        let divPriceDate: HTMLDivElement = el.querySelector("#divPriceDate") as HTMLDivElement;
        this.priceDatePicker = this.context.controlFactory.create(this.context.logger.getNewCorrelationId(), "DatePicker", datePickerOptios, divPriceDate);
        this.priceDatePicker.addEventListener("DateChanged", (eventData: { date: Date }) => {
            this.priceDate(eventData.date);
        });
    }

    public open(title: string, subTitle: string, quantity: string, labelCodes: any[]): Promise<IAddLabelDialogResult> {
        this.userEnteredValue(quantity);
        this.labelCodes(labelCodes);
        if (this.showLabelType && this.selectedCode().Id > 0) {
            this.selectedCode(labelCodes.filter(lbl => lbl.Id == this.selectedCode().Id)[0]);
        }
        let promise: Promise<IAddLabelDialogResult> = new Promise((resolve: AddLabelDialogResolve, reject: AddLabelDialogReject) => {
            this.resolve = resolve;
            let option: Dialogs.ITemplatedDialogOptions = {
                title: this.context.resources.getString('PRINT_LABEL_QTY'),
                subTitle: subTitle,
                onCloseX: this.onCloseX.bind(this),
                button1: {
                    id: "Button1",
                    label: this.context.resources.getString('OK_BUTTON_TEXT'),
                    isPrimary: true,
                    onClick: this.button1ClickHandler.bind(this)
                },
                button2: {
                    id: "Button2",
                    label: this.context.resources.getString('CANCEL_BUTTON_TEXT'),
                    onClick: this.button2ClickHandler.bind(this)
                }
            };

            this.openDialog(option);
        });
        if (this.showQuantity)
            this.setButtonDisabledState("Button1", true);
        else
            this.setButtonDisabledState("Button1", false);
        return promise;
    }

    //private priceDateChangedHandler(newDate: Date): void {
    //    this.priceDate(newDate);
    //}
    
    public closeDialogClicked(): void {
        this.closeDialog();
        this.resolvePromise("Closed");
    }

    public enableOkClicked(): void {
        this.setButtonDisabledState("Button1", false);
        this.resolvePromise("Closed");
    }

    public enableOk(): boolean {
        let quantity: number = parseInt(this.userEnteredValue());
        let isQuantity: boolean = (this.userEnteredValue() != quantity.toString()) || isNaN(quantity) || quantity <= 0;
        let isLabelType: boolean = this.selectedLabelType() == '0';
        if (isLabelType || isQuantity || !this.isVariantSelected) {
            this.setButtonDisabledState("Button1", true);
        } else {
            this.setButtonDisabledState("Button1", false);            
        }
        return true;
    }

    private onCloseX(): boolean {
        this.resolvePromise("Closed");
        return true;
    }

    private button1ClickHandler(): boolean {
        this.resolvePromise(this.userEnteredValue());
        return true;
    }

    private button2ClickHandler(): boolean {
        //this.resolvePromise("Canceled");
        this.resolvePromise("Closed");
        this.closeDialog();
        return false;
    }

    private resolvePromise(result: string): void {
        if (ObjectExtensions.isFunction(this.resolve)) {
            this.resolve(<IAddLabelDialogResult>{
                selectedValue: result,
                itemLabelType: this.selectedLabelType(),
                labelCode: this.selectedCode(),
                priceDate: this.priceDate()
            });

            this.resolve = null;
        }
    }
}