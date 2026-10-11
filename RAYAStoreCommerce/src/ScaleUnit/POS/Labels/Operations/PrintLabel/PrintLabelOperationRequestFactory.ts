import PrintLabelOperationResponse from "./PrintLabelOperationResponse";
import PrintLabelOperationRequest from "./PrintLabelOperationRequest";
import { ExtensionOperationRequestFactoryFunctionType, IOperationContext } from "PosApi/Create/Operations";
import { ClientEntities } from "PosApi/Entities";

let getOperationRequest: ExtensionOperationRequestFactoryFunctionType<PrintLabelOperationResponse> =
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
    ): Promise<ClientEntities.ICancelableDataResult<PrintLabelOperationRequest>> {

    let operationRequest: PrintLabelOperationRequest = new PrintLabelOperationRequest(correlationId);
    return Promise.resolve(<ClientEntities.ICancelableDataResult<PrintLabelOperationRequest>> {
        canceled: false,
        data: operationRequest
    });
};

export default getOperationRequest;