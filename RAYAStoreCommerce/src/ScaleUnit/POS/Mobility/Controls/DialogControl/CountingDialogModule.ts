
import * as Dialogs from "PosApi/Create/Dialogs";
import { ObjectExtensions } from "PosApi/TypeExtensions";
import { ICountingDialogResult } from "./ICountingDialogResult";
import ko from "knockout";

type CountingDialogResolve = (value: ICountingDialogResult) => void;
type CountingDialogReject = (reason: any) => void;

export default class CountingDialogModule extends Dialogs.ExtensionTemplatedDialogBase {

    public quantity: any;
    public itemName: any;
    public comment: any;
    public isShowQty: any;
    private resolve: CountingDialogResolve;

    constructor(showQuantity: boolean) {
        super();
        this.quantity = ko.observable("");
        this.itemName = ko.observable("");
        this.comment = ko.observable("");
        this.isShowQty = ko.observable(showQuantity);
        
    }

    public onReady(el: HTMLElement): void {
        ko.applyBindings(this, el);
    }

    public open(name: string, quantity: string, title: string): Promise<ICountingDialogResult> {
        this.itemName(name);
        this.quantity(quantity);

        let promise: Promise<ICountingDialogResult> = new Promise((resolve: CountingDialogResolve, reject: CountingDialogReject) => {
            this.resolve = resolve;
            let option: Dialogs.ITemplatedDialogOptions = {
                title: title,
                subTitle: "",
                onCloseX: this.onCloseX.bind(this),
                button1: {
                    id: "Button1",
                    label: this.context.resources.getString("OK_BUTTON"),
                    isPrimary: true,
                    onClick: this.button1ClickHandler.bind(this)
                },
                button2: {
                    id: "Button2",
                    label: this.context.resources.getString("CANCEL_BUTTON"),
                    onClick: this.button2ClickHandler.bind(this)
                }
            };

            this.openDialog(option);
        });

        var self = this;
        this.quantity.subscribe(function () {
            self.enableOk();
        });
        this.comment.subscribe(function () {
            self.enableOk();
        });
        
        if (quantity == "" && this.comment() == "") {
            this.setButtonDisabledState("Button1", true);
        } else {
            this.setButtonDisabledState("Button1", false);
        }

        return promise;
    }

    public closeDialogClicked(): void {
        this.closeDialog();
        this.resolvePromise("Closed");
    }

    public enableOkClicked(): void {
        this.setButtonDisabledState("Button1", false);
        this.resolvePromise("Closed");
    }

    public enableOk(): boolean {
        let quantity: number = parseInt(this.quantity(), 10);
        let isNotNumber: boolean = (this.quantity() != quantity.toString()) || isNaN(quantity) || quantity <= 0;
        let noComment: boolean = this.comment().trim().length == 0;

        if ((this.isShowQty() && isNotNumber) || (!this.isShowQty() && noComment)) {
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
        this.resolvePromise("OK");
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
            this.resolve(<ICountingDialogResult>{
                quantity: parseInt(this.quantity()),
                comment: this.comment(),
                result: result
            });

            this.resolve = null;
        }
    }
}