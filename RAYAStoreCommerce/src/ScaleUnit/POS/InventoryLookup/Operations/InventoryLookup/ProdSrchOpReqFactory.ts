import ProdSrchOpResponse from "./ProdSrchOpResponse";
import ProdSrchOpRequest from "./ProdSrchOpRequest";
import { ExtensionOperationRequestFactoryFunctionType, IOperationContext } from "PosApi/Create/Operations";
import { ClientEntities } from "PosApi/Entities";

let getOperationRequest: ExtensionOperationRequestFactoryFunctionType<ProdSrchOpResponse> =
    /**
     * Gets an instance of PrintOperationRequest.
     * @param {number} operationId The operation Id.
     * @param {string[]} actionParameters The action parameters.
     * @param {string} correlationId A telemetry correlation ID, used to group events logged from this request together with the calling context.
     * @return {PrintOperationRequest<TResponse>} Instance of PrintOperationRequest.
     */
    function (
        context: IOperationContext,
        operationId: number,
        actionParameters: string[],
        correlationId: string
    ): Promise<ClientEntities.ICancelableDataResult<ProdSrchOpRequest>> {

        let operationRequest: ProdSrchOpRequest = new ProdSrchOpRequest(correlationId);
        return Promise.resolve(<ClientEntities.ICancelableDataResult<ProdSrchOpRequest>> {
        canceled: false,
        data: operationRequest
    });
};

export default getOperationRequest;