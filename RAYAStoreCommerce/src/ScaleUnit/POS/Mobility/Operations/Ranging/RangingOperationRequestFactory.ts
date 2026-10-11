import RangingOperationResponse from "./RangingOperationResponse";
import RangingOperationRequest from "./RangingOperationRequest";
import { ExtensionOperationRequestFactoryFunctionType, IOperationContext } from "PosApi/Create/Operations";
import { ClientEntities } from "PosApi/Entities";

let getOperationRequest: ExtensionOperationRequestFactoryFunctionType<RangingOperationResponse> =
    /**
     * Gets an instance of RangingOperationRequest.
     * @param {number} operationId The operation Id.
     * @param {string[]} actionParameters The action parameters.
     * @param {string} correlationId A telemetry correlation ID, used to group events logged from this request together with the calling context.
     * @return {RangingOperationRequest<TResponse>} Instance of PrintOperationRequest.
     */
    function (
        context: IOperationContext,
        operationId: number,
        actionParameters: string[],
        correlationId: string
    ): Promise<ClientEntities.ICancelableDataResult<RangingOperationRequest>> {

        let operationRequest: RangingOperationRequest = new RangingOperationRequest(correlationId);
        return Promise.resolve(<ClientEntities.ICancelableDataResult<RangingOperationRequest>> {
        canceled: false,
        data: operationRequest
    });
};

export default getOperationRequest;