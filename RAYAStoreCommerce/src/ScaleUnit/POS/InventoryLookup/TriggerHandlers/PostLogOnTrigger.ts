
import * as Triggers from "PosApi/Extend/Triggers/ApplicationTriggers";
import { ClientEntities } from "PosApi/Entities";
import { Products } from "../DataService/DataServiceRequests.g";

export default class PostLogOnTrigger extends Triggers.PostLogOnTrigger {

    public execute(options: Triggers.IPostLogOnTriggerOptions): Promise<void> {
        this.context.logger.logVerbose("Executing PostLogOnTrigger with options " + JSON.stringify(options) + " at " + new Date().getTime() + ".");
        sessionStorage.removeItem("RR_UtilitiesConfiguration");
        this.context.runtime.executeAsync(new Products.UtilitiesConfigurationRequest())
            .then((result: ClientEntities.ICancelableDataResult<Products.UtilitiesConfigurationResponse>): any =>  {
                if (!result.canceled) {
                    sessionStorage.setItem("RR_UtilitiesConfiguration", JSON.stringify(result.data.result));
                }
                return Promise.resolve();
            });

        return Promise.resolve();
    }
}