
import { ExtensionOperationRequestBase } from "PosApi/Create/Operations";
import ProdSrchOpResponse from "./ProdSrchOpResponse";

export default class ProdSrchOpRequest extends ExtensionOperationRequestBase<ProdSrchOpResponse> {
    
    constructor(correlationId: string) {
        super(5052, correlationId);
    }
}