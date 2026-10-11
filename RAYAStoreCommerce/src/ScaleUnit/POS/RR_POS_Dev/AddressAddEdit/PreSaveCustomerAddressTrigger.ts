/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

import * as Triggers from "PosApi/Extend/Triggers/CustomerTriggers";
import { CancelableTriggerResult } from "PosApi/Extend/Triggers/Triggers";

/**
 * Example implementation of an PreCustomerSaveTrigger trigger that logs to the console.
 */
export default class PreSaveCustomerAddressTrigger extends Triggers.PreSaveCustomerAddressTrigger {


    execute(options: Triggers.IPreSaveCustomerAddressTriggerOptions):
        Promise<CancelableTriggerResult<Triggers.IPreSaveCustomerAddressTriggerOptions>> {

        //if (!ObjectExtensions.isNullOrUndefined(options.address.City))
        //{
        //    return Promise.reject({ canceled: true, options });
        //}
    
        options.address.City = $('#city').val();
        options.address.State = $('#state').val();
        return Promise.resolve({ canceled: false, options });
        
       
    }

   
}