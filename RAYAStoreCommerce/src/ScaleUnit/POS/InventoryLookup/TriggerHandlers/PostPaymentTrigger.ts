
import * as Triggers from "PosApi/Extend/Triggers/PaymentTriggers";

export default class PostPaymentTrigger extends Triggers.PostPaymentTrigger {

    public execute(options: Triggers.IPostPaymentTriggerOptions): Promise<void> {
        this.context.logger.logVerbose("Executing PostPaymentTrigger with options " + JSON.stringify(options) + " at " + new Date().getTime() + ".");

        sessionStorage.removeItem("RR_LessQtyVoiedItem");
        sessionStorage.removeItem("RR_InitialTransQuantity");
        sessionStorage.removeItem("RR_LessQtyRecallOrder");

        return Promise.resolve();
    }
}