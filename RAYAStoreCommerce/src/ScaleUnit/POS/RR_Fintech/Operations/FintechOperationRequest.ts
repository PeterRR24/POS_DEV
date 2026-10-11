
import { ExtensionOperationRequestBase } from "PosApi/Create/Operations";
import FintechOperationResponse from "./FintechOperationResponse";

export default class FintechOperationRequest extends ExtensionOperationRequestBase<FintechOperationResponse> {
    
    constructor(correlationId: string) {
        super(5010, correlationId);
    }
}