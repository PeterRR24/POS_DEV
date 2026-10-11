
import * as Dialogs from "PosApi/Create/Dialogs";
import { ObjectExtensions } from "PosApi/TypeExtensions";
import ko from "knockout";
import { IConfirmMessageDialogResult } from "./IConfirmMessageDialogResult";

type ConfirmMessageDialogResolve = (value: IConfirmMessageDialogResult) => void;
type ConfirmMessageDialogReject = (reason: any) => void;

export default class ConfirmMessageDialogModule extends Dialogs.ExtensionTemplatedDialogBase {
    public confirmMessage: any;
    public userEnteredValue: any

    private resolve: ConfirmMessageDialogResolve;

    constructor() {
        super();
        this.confirmMessage = ko.observable("");
        this.userEnteredValue = ko.observable("");
    }

    public onReady(el: HTMLElement): void {
        ko.applyBindings(this, el);
    }

    public open(message: string, title: string = "Confirmation dialog", subTitle: string = ""): Promise<IConfirmMessageDialogResult> {
        this.confirmMessage(message);

        let promise: Promise<IConfirmMessageDialogResult> = new Promise((resolve: ConfirmMessageDialogResolve, reject: ConfirmMessageDialogReject) => {
            this.resolve = resolve;
            let option: Dialogs.ITemplatedDialogOptions = {
                title: title,
                subTitle: subTitle,
                onCloseX: this.onCloseX.bind(this),
                button1: {
                    id: "Button1",
                    label: this.context.resources.getString("YES_BUTTON"),
                    isPrimary: true,
                    onClick: this.button1ClickHandler.bind(this)
                },
                button2: {
                    id: "Button2",
                    label: this.context.resources.getString("NO_BUTTON"),
                    onClick: this.button2ClickHandler.bind(this)
                }
            };

            this.openDialog(option);
        });

        this.setButtonDisabledState("Button1", false);
        return promise;
    }
    
    private onCloseX(): boolean {
        this.resolvePromise("Closed");
        return true;
    }

    private button1ClickHandler(): boolean {
        this.resolvePromise("Yes");
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
            this.resolve(<IConfirmMessageDialogResult>{
                selectedValue: result
            });

            this.resolve = null;
        }
    }
}