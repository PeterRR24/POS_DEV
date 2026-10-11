import { ExtensionOperationRequestType, ExtensionOperationRequestHandlerBase } from "PosApi/Create/Operations";
import PrintLabelOperationResponse from "./PrintLabelOperationResponse";
import PrintLabelOperationRequest from "./PrintLabelOperationRequest";
import { ClientEntities } from "PosApi/Entities";

export default class PrintLabelOperationRequestHandler extends ExtensionOperationRequestHandlerBase<PrintLabelOperationResponse> {
    
    /**
     * Gets the supported request type.
     * @return {RequestType<TResponse>} The supported request type.
     */
    public supportedRequestType(): ExtensionOperationRequestType<PrintLabelOperationResponse> {
        return PrintLabelOperationRequest;
    }

    /**
     * Executes the request handler asynchronously.
     * @param {PrintOperationRequest<TResponse>} request The request.
     * @return {Promise<ICancelableDataResult<TResponse>>} The cancelable async result containing the response.
     */
    public executeAsync(request: PrintLabelOperationRequest): Promise<ClientEntities.ICancelableDataResult<PrintLabelOperationResponse>> {

        this.context.logger.logInformational("Log message from PrintOperationRequestHandler executeAsync().", this.context.logger.getNewCorrelationId());        
        return new Promise((resolve: (value?: ClientEntities.ICancelableDataResult<PrintLabelOperationResponse>) => void) => {

            setTimeout(resolve, 1000 /*milliseconds*/);
            this.context.navigator.navigate("ItemLabelPrintWzrdView");

        });
        
    }
}