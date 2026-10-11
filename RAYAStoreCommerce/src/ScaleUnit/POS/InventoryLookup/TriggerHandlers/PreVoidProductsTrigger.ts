
import * as Triggers from "PosApi/Extend/Triggers/ProductTriggers";
import { StringExtensions } from "PosApi/TypeExtensions";
import { ClientEntities } from "PosApi/Entities";
import { ErrorHelper } from "../../ErrorHelper";

export default class PreVoidProductsTrigger extends Triggers.PreVoidProductsTrigger {
    /**
     * Executes the trigger functionality.
     * @param {Triggers.IPreVoidProductsTriggerOptions} options The options provided to the trigger.
     */
    public execute(options: Triggers.IPreVoidProductsTriggerOptions): Promise<ClientEntities.ICancelable> {
        this.context.logger.logVerbose("Executing PreVoidProductsTrigger with options " + JSON.stringify(options) + " at " + new Date().getTime() + ".");
        if (options.cart.CartTypeValue == 3 && options.cart.CustomerOrderModeValue != 3) {
            return Promise.resolve({ canceled: false });
        }
        let cartLines: Commerce.Proxy.Entities.CartLine[] = options.cartLines;
        let lessQtyVoiedLineIds: string[] = JSON.parse(sessionStorage.getItem("RR_LessQtyVoiedItem")) || [];
        let products: string = '';
        for (let i: number = 0; i < cartLines.length; i++) {
            if (lessQtyVoiedLineIds.indexOf(cartLines[i].LineId) != -1) {
                let item = cartLines[i].ItemId + '::' + cartLines[i].Description;
                if (products.indexOf(item) == -1) {
                    products += item + '\r\n';
                }
                ErrorHelper.displayMessageAsync(this.context, StringExtensions.format(this.context.resources.getString('PRODUCT_QUANTITY_ZERO_ALERT'), products));
                return Promise.resolve({ canceled: true });
            }
        }
        return Promise.resolve({ canceled: false });
    }
}