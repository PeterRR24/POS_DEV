
import { ShowMessageDialogClientRequest, ShowMessageDialogClientResponse, IMessageDialogOptions } from "PosApi/Consume/Dialogs";
import { IExtensionContext } from "PosApi/Framework/ExtensionContext";
import { ClientEntities } from "PosApi/Entities";

export default class MessageDialog {
    public static show(context: IExtensionContext, message: string, dlgTitle: string): Promise<void> {
        let promise: Promise<void> = new Promise<void>((resolve: () => void, reject: (reason?: any) => void) => {
            let messageDialogOptions: IMessageDialogOptions = {
                title: dlgTitle,
                message: message,
                showCloseX: true, // this property will return "Close" as result when "X" is clicked to close dialog.
                button1: {
                    id: "Button1Close",
                    label: context.resources.getString('OK_BUTTON_TEXT'),
                    result: "OKResult"
                }
            };

            let dialogRequest: ShowMessageDialogClientRequest<ShowMessageDialogClientResponse> =
                new ShowMessageDialogClientRequest<ShowMessageDialogClientResponse>(messageDialogOptions);

            context.runtime.executeAsync(dialogRequest).then((result: ClientEntities.ICancelableDataResult<ShowMessageDialogClientResponse>) => {
                if (!result.canceled) {
                    context.logger.logInformational("MessageDialog result: " + result.data.result.dialogResult);
                    resolve();
                }
            }).catch((reason: any) => {
                context.logger.logError(JSON.stringify(reason));
                reject(reason);
            });
        });

        return promise;
    }
}