
import { ExtensionOperationRequestBase } from "PosApi/Create/Operations";
import RangingOperationResponse from "./RangingOperationResponse";

export default class RangingOperationRequest extends ExtensionOperationRequestBase<RangingOperationResponse> {
    
    constructor(correlationId: string) {
        super(5051, correlationId);
    }
}