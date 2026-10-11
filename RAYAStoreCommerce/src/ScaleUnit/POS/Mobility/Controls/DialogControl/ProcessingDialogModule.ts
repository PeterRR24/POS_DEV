
import * as Dialogs from "PosApi/Create/Dialogs";
import { ObjectExtensions } from "PosApi/TypeExtensions";
import { IProcessingDialogResult } from "./IProcessingDialogResult";
import ko from "knockout";

type ProcessingDialogResolve = (value: IProcessingDialogResult) => void;
type ProcessingDialogReject = (reason: any) => void;

export default class ProcessingDialogModule extends Dialogs.ExtensionTemplatedDialogBase {
    private displayMessage: any;
    //public isOpen: boolean;
    private resolve: ProcessingDialogResolve;

    constructor() {
        super();
        this.displayMessage = ko.observable("");
        //this.isOpen = false;
        //var self = this;
        //this.displayMessage.subscribe(function () {
        //    self.enableButton();
        //});
    }

    public onReady(el: HTMLElement): void {
        ko.applyBindings(this, el);
    }

    public open(message: string, title: string = "Processing dialog", subTitle: string = ""): Promise<IProcessingDialogResult> {
        this.displayMessage(message);

        let promise: Promise<IProcessingDialogResult> = new Promise((resolve: ProcessingDialogResolve, reject: ProcessingDialogReject) => {
            this.resolve = resolve;
            let option: Dialogs.ITemplatedDialogOptions = {
                title: title,
                subTitle: subTitle,
                //onCloseX: this.onCloseX.bind(this),
                button1: {
                    id: "Button1",
                    label: this.context.resources.getString("CLOSE_BUTTON"),
                    isPrimary: true,
                    onClick: this.button1ClickHandler.bind(this)
                }
            };

            this.openDialog(option);
        });
        //this.isOpen = true;
        //this.setButtonDisabledState("Button1", true);
        return promise;
    }

    //private enableButton(): void{
    //    if (this.displayMessage() == "Successfully done!!!") {
    //        this.setButtonDisabledState("Button1", false);
    //        this.setButtonDisabledState("Button2", true);
    //    }
    //    else {
    //        this.setButtonDisabledState("Button1", true);
    //        this.setButtonDisabledState("Button2", false);
    //    }
    //}

    private button1ClickHandler(): boolean {
        //if (this.displayMessage() == "Successfully done!!!") {
        //    this.resolvePromise("Done");
        //}
        //else {
        //    this.resolvePromise("Cancel");
        //}
        this.resolvePromise("Done");
        this.closeDialog();
        return true;
    }

    //private Button2ClickHandler(): boolean {
    //    this.resolvePromise("C");
    //    this.closeDialog();
    //    return false;
    //}

    private resolvePromise(result: string): void {
        //this.isOpen = false;
        if (ObjectExtensions.isFunction(this.resolve)) {
            this.resolve(<IProcessingDialogResult>{
                selectedValue: result
            });

            this.resolve = null;
        }
    }
}