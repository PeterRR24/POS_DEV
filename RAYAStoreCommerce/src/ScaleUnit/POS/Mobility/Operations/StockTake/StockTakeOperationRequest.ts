
import { ExtensionOperationRequestBase } from "PosApi/Create/Operations";
import StockTakeOperationResponse from "./StockTakeOperationResponse";

export default class StockTakeOperationRequest extends ExtensionOperationRequestBase<StockTakeOperationResponse> {
    
    constructor(correlationId: string) {
        super(5050, correlationId);
    }
}