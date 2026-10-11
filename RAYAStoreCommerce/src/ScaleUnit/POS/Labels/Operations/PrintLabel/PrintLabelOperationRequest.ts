
import { ExtensionOperationRequestBase } from "PosApi/Create/Operations";
import PrintLabelOperationResponse from "./PrintLabelOperationResponse";

export default class PrintLabelOperationRequest extends ExtensionOperationRequestBase<PrintLabelOperationResponse> {
    
    constructor(correlationId: string) {
        super(5000, correlationId);
    }
}