
import * as Triggers from "PosApi/Extend/Triggers/ProductTriggers";
import {  ProxyEntities } from "PosApi/Entities";
//import { Products } from "../dataservice/DataServiceRequests.g";
import { PriceOverrideOperationRequest,  } from "PosApi/Consume/Cart";
import { Entities } from "../DataService/DataServiceEntities.g";
export { ProxyEntities };
export { Entities };

export interface IProductSaleReturnDetailsExt extends Commerce.Client.Entities.IProductSaleReturnDetails {
    AvailableQuantity?: number;
  
}
//export class ProductSearchByTextResponse extends DataServiceResponse {
//    public result: Entities.ProductExClass[];
//}

export default class PostProductSaleTrigger extends Triggers.PostProductSaleTrigger {

    public execute(options: Triggers.IPostProductSaleTriggerOptions): Promise<void> {
        this.context.logger.logVerbose("Executing PostProductSaleTrigger with options " + JSON.stringify(options) + " at " + new Date().getTime() + ".");


        


        //let updateCartLines: Commerce.Proxy.Entities.CartLine[]= [];
        //let Cartline: ProxyEntities.CartLine[] = [];
        //let NewPrice: number = 0;
        let correlationId: string = '';
        //let correlationIDs: string = '';
        //let voidCartLines: Commerce.Proxy.Entities.CartLine[] = [];
        //let PSDs: IProductSaleReturnDetailsExt[] = options.productSaleDetails;
        //let KKAS: string = ''; 
        //let priceOverrideOperationRequest:  PriceOverrideOperationRequest<PriceOverrideOperationResponse>;
        //let Tran: ProxyEntities.Transaction;
        //let itemtype: Entities.ProductInfoClass;
        //let itemid: string = options.cart.CartLines[options.cart.CartLines.length - 1].ItemId;
        //let TranType: Number = options.cart.TransactionTypeValue;
        //let tranname: string = TranType.toString();
            //Commerce.Proxy.Entities.TransactionClass; //Commerce.Extensibility.ValueFormatterManager.apply(TransactionTypeFormatter:





        //let getstorehours: StoreHours.RequestValue<StoreHours.GetResponseValue> = null;
        //getstorehours = new StoreHours.RequestValue<StoreHours.GetResponseValue>(options.cart.CartLines[options.cart.CartLines.length - 1].ItemId);
        //this.context.runtime.executeAsync(getstorehours)
        //    .then((result: ClientEntities.ICancelableDataResult<StoreHours.GetResponseValue>): any => {
        //        //   let dat: Commerce.Proxy.Entities.StoreHoursClass = (<StoreHours.GetStoreDaysByStoreResponse>result.data).result;

        //        //let SampleCommerceProperties: ProxyEntities.CommerceProperty[] = result.data.result[0].ExtensionProperties.filter((extensionProperty: ProxyEntities.CommerceProperty) => {
        //        //    return extensionProperty.Key === "sampleExtensionProperty";

        //        //});
        //        // ErrorHelper.displayMessageAsync(this.context, result.data.result[0].ExtensionProperties[0].Value.StringValue);
        //        // ErrorHelper.displayMessageAsync(this.context, result.data.result[0].ExtensionProperties[0].Key.toString());

        //        // ErrorHelper.displayMessageAsync(this.context, result.data.result[0].ITEMBARCODE + "-" + result.data.result[0].DESCRIPTION);
        //        ErrorHelper.displayMessageAsync(this.context, result.data.result.toString());
        //    }).catch(err => {
        //        console.log(err);
        //        // ErrorHelper.displayMessageAsync(this.context, err);
        //    });
        
   


        //let itemQtyRequest: Products.GetProductAvailableQuantityRequest<Products.GetProductAvailableQuantityResponse> = null;

        //itemQtyRequest = new Products.GetProductAvailableQuantityRequest<Products.GetProductAvailableQuantityResponse>(options.cart.CartLines[options.cart.CartLines.length - 1].ProductId, options.cart.CartLines[options.cart.CartLines.length - 1].ItemId, '');
        //this.context.runtime.executeAsync(itemQtyRequest)
        //    .then((resp: ClientEntities.ICancelableDataResult<Products.GetProductAvailableQuantityResponse>): any => {
        //        if (!resp.canceled) {
        //          //  PSDs[i].AvailableQuantity = resp.data.result;
        //            ErrorHelper.displayMessageAsync(this.context, resp.data.result.toString());
        //        }
        //    });



        //let getstorehours: StoreHours.GetStoreDaysByStoreRequest<StoreHours.GetStoreDaysByStoreResponse> = null;
        //getstorehours = new StoreHours.GetStoreDaysByStoreRequest<StoreHours.GetStoreDaysByStoreResponse>(options.cart.CartLines[options.cart.CartLines.length - 1].ItemId);
        //this.context.runtime.executeAsync(getstorehours)
        //    .then((result: ClientEntities.ICancelableDataResult<StoreHours.GetStoreDaysByStoreResponse>): any => {
        //        //   let dat: Commerce.Proxy.Entities.StoreHoursClass = (<StoreHours.GetStoreDaysByStoreResponse>result.data).result;

        //        //let SampleCommerceProperties: ProxyEntities.CommerceProperty[] = result.data.result[0].ExtensionProperties.filter((extensionProperty: ProxyEntities.CommerceProperty) => {
        //        //    return extensionProperty.Key === "sampleExtensionProperty";

        //        //});
        //        // ErrorHelper.displayMessageAsync(this.context, result.data.result[0].ExtensionProperties[0].Value.StringValue);
        //        // ErrorHelper.displayMessageAsync(this.context, result.data.result[0].ExtensionProperties[0].Key.toString());

        //        // ErrorHelper.displayMessageAsync(this.context, result.data.result[0].ITEMBARCODE + "-" + result.data.result[0].DESCRIPTION);
        //        ErrorHelper.displayMessageAsync(this.context, result.data.result[0].toString());
        //    }).catch(err => {
        //        console.log(err);
        //        // ErrorHelper.displayMessageAsync(this.context, err);
        //    });
         
   
    

       

            if (options.cart.CartLines[options.cart.CartLines.length - 1].ItemId.charAt(0) === "%") {
                // StringExtensions.format(this.context.resources.getString('PRODUCT_QUANTITY_ZERO_ALERT'), "")


                var Desc = options.cart.CartLines[options.cart.CartLines.length - 1].ItemId.split('%');


                let UpatedPrice: number = (options.cart.CartLines[options.cart.CartLines.length - 1].Price * parseFloat(Desc[1])) / 100;

                this.context.runtime.executeAsync(new PriceOverrideOperationRequest(options.cart.CartLines[options.cart.CartLines.length - 1].LineId, UpatedPrice, ""))
                    .then(() => {

                        correlationId = this.context.logger.getNewCorrelationId();

                        this.context.runtime.executeAsync(new Commerce.RefreshCartClientRequest(correlationId))
                            .then((resp: any) => {

                                console.log(resp);

                            });
                    }).catch(err => {
                        console.log(err);
                    });

            }

     //   }//end new if


    
        
            return Promise.resolve();
        }






    
    //private GetProductsQuantity(PSDs: IProductSaleReturnDetailsExt[]): Promise<any> {
    //    let promises = [];
    //    let itemQtyRequest: Products.GetProductAvailableQuantityRequest<Products.GetProductAvailableQuantityResponse> = null;
    //    for (let i = 0; i < PSDs.length; i++) {
          
    //        if (PSDs[i].product.ProductTypeValue == 4 || PSDs[i].product.ProductTypeValue == 5) {
    //            itemQtyRequest = new Products.GetProductAvailableQuantityRequest<Products.GetProductAvailableQuantityResponse>(PSDs[i].product.RecordId, PSDs[i].product.ItemId, '');
    //            promises.push(this.context.runtime.executeAsync(itemQtyRequest)
    //                .then((resp: ClientEntities.ICancelableDataResult<Products.GetProductAvailableQuantityResponse>): any => {
    //                    if (!resp.canceled) {
    //                        PSDs[i].AvailableQuantity = resp.data.result;
    //                        return PSDs[i];
    //                    }
    //                }));
    //        }
    //    }
    //    return Promise.all(promises);
    //}




}
