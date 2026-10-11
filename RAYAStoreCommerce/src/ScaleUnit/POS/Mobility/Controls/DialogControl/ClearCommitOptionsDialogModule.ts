
import * as Dialogs from "PosApi/Create/Dialogs";
import { ObjectExtensions } from "PosApi/TypeExtensions";
import ko from "knockout";
import { IClearCommitOptionsDialogResult } from "./IClearCommitOptionsDialogResult";

type ClearCommitOptionsDialogResolve = (value: IClearCommitOptionsDialogResult) => void;
type ClearCommitOptionsDialogReject = (reason: any) => void;

export default class ClearCommitOptionsDialogModule extends Dialogs.ExtensionTemplatedDialogBase {
    public message: any;
    public userEnteredValue: any;

    private resolve: ClearCommitOptionsDialogResolve;
    private shelf: any;
    private optionValue: any;

    constructor(shelf: string) {
        super();
        this.message = ko.observable("");
        this.optionValue = ko.observable("0");
        this.shelf = ko.observable(shelf);
    }

    public onReady(el: HTMLElement): void {
        ko.applyBindings(this, el);
    }

    public open(message: string, title: string = "Dialog title", subTitle: string = ""): Promise<IClearCommitOptionsDialogResult> {
        this.message(message);

        let promise: Promise<IClearCommitOptionsDialogResult> = new Promise((resolve: ClearCommitOptionsDialogResolve, reject: ClearCommitOptionsDialogReject) => {
            this.resolve = resolve;
            let option: Dialogs.ITemplatedDialogOptions = {
                title: title,
                subTitle: subTitle,
                onCloseX: this.onCloseX.bind(this),
                button1: {
                    id: "ok",
                    label: this.context.resources.getString("OK_BUTTON"),
                    isPrimary: true,
                    onClick: this.okClickHandler.bind(this)
                },
                button2: {
                    id: "cancel",
                    label: this.context.resources.getString("CANCEL_BUTTON"),
                    onClick: this.cancelClickHandler.bind(this)
                }
            };

            this.openDialog(option);
        });
        console.log(this.shelf);
        var self = this;
        this.optionValue.subscribe(function () {
            self.enableOk();
        });
        this.setButtonDisabledState("ok", (this.optionValue() == "0"));
        return promise;
    }

    public enableOk(): void {
        this.setButtonDisabledState("ok", (this.optionValue() == "0"));
    }

    private onCloseX(): boolean {
        this.resolvePromise("0");
        return true;
    }

    private okClickHandler(): boolean {

        this.resolvePromise(this.optionValue());
        this.closeDialog();
        return true;
    }

    private cancelClickHandler(): boolean {
        //this.resolvePromise("Canceled");
        this.resolvePromise("0");
        this.closeDialog();
        return false;
    }

    private resolvePromise(result: string): void {
        if (ObjectExtensions.isFunction(this.resolve)) {
            this.resolve(<IClearCommitOptionsDialogResult>{
                selectedValue: result
            });

            this.resolve = null;
        }
    }
}