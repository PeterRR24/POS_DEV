
import * as Triggers from "PosApi/Extend/Triggers/ProductTriggers";
//import { ObjectExtensions } from "PosApi/TypeExtensions";
//import { ClientEntities } from "PosApi/Entities";

export default class PostSetQuantityTrigger extends Triggers.PostSetQuantityTrigger {
    /**
     * Executes the trigger functionality.
     * @param {Triggers.IPreSetQuantityTriggerOptions} options The options provided to the trigger.
     */
    public execute(options: Triggers.IPostSetQuantityTriggerOptions): Promise<void> {
        this.context.logger.logVerbose("Executing PostSetQuantityTrigger with options " + JSON.stringify(options) + " at " + new Date().getTime() + ".");
        sessionStorage.setItem("InitialTransQuantity", '2');//options.cartLineQuantities[0].cartLine.Quantity.toString()



        return Promise.resolve();
    }
}