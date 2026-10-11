
import * as Triggers from "PosApi/Extend/Triggers/ProductTriggers";
import { ClientEntities } from "PosApi/Entities";
import { Products } from "../DataService/DataServiceRequests.g";
import { PriceOverrideOperationRequest } from "PosApi/Consume/Cart";


export interface IProductSaleReturnDetailsExt extends Commerce.Client.Entities.IProductSaleReturnDetails {
    AvailableQuantity?: number;
}

export default class PostProductSaleTrigger extends Triggers.PostProductSaleTrigger {


    public execute(options: Triggers.IPostProductSaleTriggerOptions): Promise<void> {
        this.context.logger.logVerbose("Executing PostProductSaleTrigger with options " + JSON.stringify(options) + " at " + new Date().getTime() + ".");



        let correlationId: string = '';
        let voidCartLines: Commerce.Proxy.Entities.CartLine[] = [];
        let PSDs: IProductSaleReturnDetailsExt[] = options.productSaleDetails;
        let products: string = ''; 



       



            if (options.cart.CartLines[options.cart.CartLines.length-1].Description.charAt(0) === "%") {

                var Desc = options.cart.CartLines[options.cart.CartLines.length - 1].Description.split('%');


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
         

           // }
           
        




            this.GetProductsQuantity(PSDs).then((results: IProductSaleReturnDetailsExt[]) => {





                for (let idx = 0; idx < results.length; idx++) {
                    if (!(results[idx].AvailableQuantity > 0)) {

                        for (let cl = 0; cl < options.cart.CartLines.length; cl++) {




                            if (options.cart.CartLines[cl].ProductId == results[idx].product.RecordId) {
                                voidCartLines.push(options.cart.CartLines[cl]);
                                let item = options.cart.CartLines[cl].ItemId + '::' + options.cart.CartLines[cl].Description;


                                if (products.indexOf(item) == -1) {
                                    products += item + '\r\n'
                                }
                            }
                        }
                    }
                }

                //if (voidCartLines.length !== 0) {
                //    this.context.runtime.executeAsync(new Products.VoidCartLinesRequest(options.cart.Id, voidCartLines))
                //        .then(() => {
                //            correlationId = this.context.logger.getNewCorrelationId();
                //            this.context.runtime.executeAsync(new Commerce.RefreshCartClientRequest(correlationId))
                //                .then((resp: any) => {
                //                    console.log(resp);
                //                    ErrorHelper.displayMessageAsync(this.context, StringExtensions.format(this.context.resources.getString('PRODUCT_QUANTITY_ZERO_ALERT'), products));
                //                });
                //        });
                //}

            }).catch(err => {
                console.log(err);
            });

            return Promise.resolve();
        }

   

    

    //private ChangeCartLinePrice(CartLineUpdate: ProxyEntities.CartLine, newPrice: number): Promise<any> {
    
    //    let promises = [];
    //    let priceOverrideOperationRequest: PriceOverrideOperationRequest<PriceOverrideOperationResponse> ;



    
    //    priceOverrideOperationRequest = new PriceOverrideOperationRequest<PriceOverrideOperationResponse>(CartLineUpdate.LineId, newPrice, '');
    //    promises.push(this.context.runtime.executeAsync(priceOverrideOperationRequest)
    //        .then((resp: ClientEntities.ICancelableDataResult<PriceOverrideOperationResponse>): any => {
    //                    if (!resp.canceled) {
    //                     //   PSDs[i].AvailableQuantity = resp.data.result;
    //                        return CartLineUpdate;
    //                    }
    //                }));
      
      
    //    return Promise.all(promises);
    //} 




    
    private GetProductsQuantity(PSDs: IProductSaleReturnDetailsExt[]): Promise<any> {
        let promises = [];
        let itemQtyRequest: Products.GetProductAvailableQuantityRequest<Products.GetProductAvailableQuantityResponse> = null;

        for (let i = 0; i < PSDs.length; i++) {

            if (PSDs[i].product.ProductTypeValue == 4 || PSDs[i].product.ProductTypeValue == 5) {
                itemQtyRequest = new Products.GetProductAvailableQuantityRequest<Products.GetProductAvailableQuantityResponse>(PSDs[i].product.RecordId, PSDs[i].product.ItemId, '');
                promises.push(this.context.runtime.executeAsync(itemQtyRequest)
                    .then((resp: ClientEntities.ICancelableDataResult<Products.GetProductAvailableQuantityResponse>): any => {
                        if (!resp.canceled) {
                            PSDs[i].AvailableQuantity = resp.data.result;
                            return PSDs[i];
                        }
                    }));
            }
        }
        return Promise.all(promises);
    }




}
