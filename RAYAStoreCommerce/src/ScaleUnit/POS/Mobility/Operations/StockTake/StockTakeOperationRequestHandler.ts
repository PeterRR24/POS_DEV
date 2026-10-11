import { ExtensionOperationRequestType, ExtensionOperationRequestHandlerBase } from "PosApi/Create/Operations";
import StockTakeOperationResponse from "./StockTakeOperationResponse";
import StockTakeOperationRequest from "./StockTakeOperationRequest";
import { ClientEntities } from "PosApi/Entities";

export default class StockTakeOperationRequestHandler extends ExtensionOperationRequestHandlerBase<StockTakeOperationResponse> {
    
    /**
     * Gets the supported request type.
     * @return {RequestType<TResponse>} The supported request type.
     */
    public supportedRequestType(): ExtensionOperationRequestType<StockTakeOperationResponse> {
        return StockTakeOperationRequest;
    }

    /**
     * Executes the request handler asynchronously.
     * @param {PrintOperationRequest<TResponse>} request The request.
     * @return {Promise<ICancelableDataResult<TResponse>>} The cancelable async result containing the response.
     */
    public executeAsync(request: StockTakeOperationRequest): Promise<ClientEntities.ICancelableDataResult<StockTakeOperationResponse>> {

        this.context.logger.logInformational("Log message from PrintOperationRequestHandler executeAsync().", this.context.logger.getNewCorrelationId());        
        return new Promise((resolve: (value?: ClientEntities.ICancelableDataResult<StockTakeOperationResponse>) => void) => {

            setTimeout(resolve, 1000 /*milliseconds*/);
            this.context.navigator.navigate("StockTakeJournalsView");

        });
        
    }
}