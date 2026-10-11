
import { ShowMessageDialogClientRequest, ShowMessageDialogClientResponse, IMessageDialogOptions } from "PosApi/Consume/Dialogs";
import { IExtensionContext } from "PosApi/Framework/ExtensionContext";
import { ClientEntities } from "PosApi/Entities";

/**
 * A class containing helper functions for error handling.
 */
export class ErrorHelper {

    /**
     * Get an error message corresponding to the provided reason object.
     * @param {IExtensionContext} context The extension context.
     * @param {any} reason The error/reason to display.
     * @returns {Promise<ClientEntities.ICancelable>} The promise representing the completion of displaying the error message.
     */
    public static displayLocalizedErrorAsync(context: IExtensionContext, errorObj: any): Promise<ClientEntities.ICancelable> {

        let reason: any = errorObj;
        let errorMsg: string = '';
        let errorCode: string = '';
        //var dlgTitle: string = this._context.resources.getString("ERROR_TITLE");

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
            
        } else if (typeof reason === "string") {
            errorMsg = reason;
        } else {
            errorMsg = "An unexpected error occurred.";
        }

        return ErrorHelper.displayMessageAsync(context, errorMsg);
    }

    /**
     * Displays the provided message to the client.
     * @param {IExtensionContext} context The extension context.
     * @param {any} reason The message to display.
     * @returns {Promise<ClientEntities.ICancelable>} The promise representing the completion of displaying the message.
     */
    public static displayMessageAsync(context: IExtensionContext, reason: string): Promise<ClientEntities.ICancelable> {
        let messageDialogOptions: IMessageDialogOptions = {
            message: reason
        };
        let messageRequest: ShowMessageDialogClientRequest<ShowMessageDialogClientResponse>
            = new ShowMessageDialogClientRequest(messageDialogOptions);

        return context.runtime.executeAsync(messageRequest);
    }

    /**
     * Displays an error message corresponding to the provided reason.
     * @param {IExtensionContext} context The extension context.
     * @param {any} reason The error/reason to display.
     * @returns {Promise<ClientEntities.ICancelable>} The promise representing the completion of displaying the error message.
     */
    public static displayErrorAsync(context: IExtensionContext, reason: any): Promise<ClientEntities.ICancelable> {
        let messageDialogOptions: IMessageDialogOptions;
        if (reason instanceof ClientEntities.ExtensionError) {
            messageDialogOptions = {
                message: reason.localizedMessage
            };
        } else if (reason instanceof Error) {
            messageDialogOptions = {
                message: reason.message
            };
        } else if (typeof reason === "string") {
            messageDialogOptions = {
                message: reason
            };
        } else {
            messageDialogOptions = {
                message: "An unexpected error occurred."
            };
        }

        let errorMessageRequest: ShowMessageDialogClientRequest<ShowMessageDialogClientResponse>
            = new ShowMessageDialogClientRequest(messageDialogOptions);

        return context.runtime.executeAsync(errorMessageRequest);
    }
}