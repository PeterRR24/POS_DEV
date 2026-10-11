
import * as Dialogs from "PosApi/Create/Dialogs";
import { ObjectExtensions } from "PosApi/TypeExtensions";
import { IConfirmMessageDialogResult } from "./IConfirmMessageDialogResult";
import ko from "knockout";

type ConfirmMessageDialogResolve = (value: IConfirmMessageDialogResult) => void;
type ConfirmMessageDialogReject = (reason: any) => void;

export default class ConfirmMessageDialogModule extends Dialogs.ExtensionTemplatedDialogBase {
    public confirmMessage: any;
    public userEnteredValue: any;
    
    private resolve: ConfirmMessageDialogResolve;
    private visibleReprint: any;

    constructor(showReprint: boolean = false) {
        super();
        this.confirmMessage = ko.observable("");
        this.visibleReprint = ko.observable(false);
        this.visibleReprint(showReprint);
    }

    public onReady(el: HTMLElement): void {
        ko.applyBindings(this, el);
    }

    public open(message: string, title: string = this.context.resources.getString('CONFIRM_DIALOG_TITLE'), subTitle: string = ""): Promise<IConfirmMessageDialogResult> {
        this.confirmMessage(message);

        let promise: Promise<IConfirmMessageDialogResult> = new Promise((resolve: ConfirmMessageDialogResolve, reject: ConfirmMessageDialogReject) => {
            this.resolve = resolve;
            let option: Dialogs.ITemplatedDialogOptions = {
                title: title,
                subTitle: subTitle,
                onCloseX: this.onCloseX.bind(this),
                button1: {
                    id: "btnYes",
                    label: this.context.resources.getString('YES_BUTTON'),
                    isPrimary: true,
                    onClick: this.btnYesClickHandler.bind(this)
                },
                button2: {
                    id: "btnNo",
                    label: this.context.resources.getString('NO_BUTTON'),
                    onClick: this.btnNoClickHandler.bind(this)
                }
            };
            
            this.openDialog(option);
        });

        this.setButtonDisabledState("btnYes", false);
        return promise;
    }
    
    private onCloseX(): boolean {
        this.resolvePromise("Closed");
        return true;
    }

    private btnYesClickHandler(): boolean {
        this.resolvePromise("Yes");
        return true;
    }

    private btnNoClickHandler(): void {
        this.resolvePromise("Closed");
        this.closeDialog();
    }

    public reprintClickHandler(): void {
        this.resolvePromise("Reprint");
        this.closeDialog();
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