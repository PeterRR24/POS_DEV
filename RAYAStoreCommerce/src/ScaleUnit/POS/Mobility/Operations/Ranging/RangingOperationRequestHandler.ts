import { ExtensionOperationRequestType, ExtensionOperationRequestHandlerBase } from "PosApi/Create/Operations";
import RangingOperationResponse from "./RangingOperationResponse";
import RangingOperationRequest from "./RangingOperationRequest";
import { ClientEntities } from "PosApi/Entities";
import { Ranging } from "../../DataService2/RangingDataServiceRequests.g";

export default class RangingOperationRequestHandler extends ExtensionOperationRequestHandlerBase<RangingOperationResponse> {
    
    /**
     * Gets the supported request type.
     * @return {RequestType<TResponse>} The supported request type.
     */
    public supportedRequestType(): ExtensionOperationRequestType<RangingOperationResponse> {
        return RangingOperationRequest;
    }

    /**
     * Executes the request handler asynchronously.
     * @param {PrintOperationRequest<TResponse>} request The request.
     * @return {Promise<ICancelableDataResult<TResponse>>} The cancelable async result containing the response.
     */
    public executeAsync(request: RangingOperationRequest): Promise<ClientEntities.ICancelableDataResult<RangingOperationResponse>> {

        this.context.logger.logInformational("Log message from RangingOperationRequestHandler executeAsync().", this.context.logger.getNewCorrelationId());        
        return new Promise((resolve: (value?: ClientEntities.ICancelableDataResult<RangingOperationResponse>) => void) => {

            let request: Ranging.UtilitiesConfigurationRequest<Ranging.UtilitiesConfigurationResponse>
                = new Ranging.UtilitiesConfigurationRequest<Ranging.UtilitiesConfigurationResponse>();
            this.context.runtime.executeAsync(request)
                .then((result: ClientEntities.ICancelableDataResult<Ranging.UtilitiesConfigurationResponse>) => {
                    setTimeout(resolve, 500 /*milliseconds*/);
                    this.context.navigator.navigate("ItemStoreLocationView", { isFaceing: result.data.result.RangingWithFaces });
                })

        });
        
    }
}