
import * as Triggers from "PosApi/Extend/Triggers/ProductTriggers";
import { ClientEntities } from "PosApi/Entities";

export default class PreSetQuantityTrigger extends Triggers.PreSetQuantityTrigger {
    /**
     * Executes the trigger functionality.
     * @param {Triggers.IPreSetQuantityTriggerOptions} options The options provided to the trigger.
     */
    public execute(options: Triggers.IPreSetQuantityTriggerOptions): Promise<ClientEntities.ICancelable> {
        this.context.logger.logVerbose("Executing PreProductSaleTrigger with options " + JSON.stringify(options) + " at " + new Date().getTime() + ".");
        sessionStorage.setItem("InitialTransQuantity","2");//options.cartLineQuantities[0].cartLine.Quantity.toString()
        return Promise.resolve({ canceled: false });
    }
}