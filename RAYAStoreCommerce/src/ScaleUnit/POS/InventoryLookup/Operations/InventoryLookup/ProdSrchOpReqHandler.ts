import { ExtensionOperationRequestType, ExtensionOperationRequestHandlerBase } from "PosApi/Create/Operations";
import ProdSrchOpResponse from "./ProdSrchOpResponse";
import ProdSrchOpRequest from "./ProdSrchOpRequest";
import { ClientEntities } from "PosApi/Entities";

export default class ProdSrchOpReqHandler extends ExtensionOperationRequestHandlerBase<ProdSrchOpResponse> {
    
    /**
     * Gets the supported request type.
     * @return {RequestType<TResponse>} The supported request type.
     */
    public supportedRequestType(): ExtensionOperationRequestType<ProdSrchOpResponse> {
        return ProdSrchOpRequest;
    }

    /**
     * Executes the request handler asynchronously.
     * @param {ProdSrchOpRequest<TResponse>} request The request.
     * @return {Promise<ICancelableDataResult<TResponse>>} The cancelable async result containing the response.
     */
    public executeAsync(request: ProdSrchOpRequest): Promise<ClientEntities.ICancelableDataResult<ProdSrchOpResponse>> {

        this.context.logger.logInformational("Log message from ProductSearchView executeAsync().", this.context.logger.getNewCorrelationId());        
        return new Promise((resolve: (value?: ClientEntities.ICancelableDataResult<ProdSrchOpResponse>) => void) => {

            setTimeout(resolve, 1000 /*milliseconds*/);
            this.context.navigator.navigate("ProductSearchView");

        });
        
    }
}