
import * as Triggers from "PosApi/Extend/Triggers/ProductTriggers";
import { StringExtensions } from "PosApi/TypeExtensions";
import { ClientEntities } from "PosApi/Entities";
import { Products } from "../DataService/DataServiceRequests.g";
import { Entities } from "../DataService/DataServiceEntities.g";
import { ErrorHelper } from "../../ErrorHelper";
import { PriceOverrideOperationRequest } from "PosApi/Consume/Cart";

export default class PostProductSaleTrigger extends Triggers.PostProductSaleTrigger {

    private isBlockSale: boolean;

    public execute(options: Triggers.IPostProductSaleTriggerOptions): Promise<void> {
        this.context.logger.logVerbose("Executing PostProductSaleTrigger with options " + JSON.stringify(options) + " at " + new Date().getTime() + ".");

        this.getConfiguration();
        if (options.cart.CartLines[options.cart.CartLines.length - 1].Description.charAt(0) === "%") {

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


        if (!this.isBlockSale || (options.cart.CartTypeValue == 3 && options.cart.CustomerOrderModeValue != 3))
            return Promise.resolve();
        //MessageDialog.show(this.context, options.cart.TransactionTypeValue.toString(),"");
        //let TranType: Number = options.cart.TransactionTypeValue;
        //let tranname: string = TranType.toString();
        if (options.cart.TransactionTypeValue == 19) 
              return Promise.resolve();

            let RequestPSDs: Entities.IProductSaleReturnDetailsExt[] = options.productSaleDetails;
            let PSDs: Entities.IProductSaleReturnDetailsExt[] = [];
            for (let i = 0; i < RequestPSDs.length; i++) {
                if (RequestPSDs[i].product.ItemTypeValue !== 2) { // RequestPSDs[i].product.ItemServiceTypeValue == 1
                    PSDs.push(RequestPSDs[i]);
                }
            }
            if (PSDs.length == 0)
                return Promise.resolve();

            let correlationId: string = '';
            let voidCartLines: Commerce.Proxy.Entities.CartLine[] = [];
            let updateCartLine: Commerce.Proxy.Entities.CartLine[] = [];
            let cart: Commerce.Proxy.Entities.Cart = options.cart;
            let tempCartLine: Commerce.Proxy.Entities.CartLine[] = cart.CartLines;
            //let sp: Commerce.Proxy.Entities.SimpleProduct;
            let products: string = '';
            let strQuantity: string = '';

            let rrCommerce: any = Commerce;
            let deviceConfig = new Commerce.Proxy.Entities.DeviceConfigurationClass(JSON.parse(rrCommerce.ApplicationStorage.getItem(rrCommerce.ApplicationStorageIDs.DEVICE_CONFIGURATION_KEY)));
            let aggregateItem: boolean = deviceConfig.AllowItemsAggregation;

            this.GetProductsQuantity(PSDs).then((results: Entities.IProductSaleReturnDetailsExt[]): any => {

                for (let idx = 0; idx < results.length; idx++) {
                    let availableQuantity: number = results[idx].AvailableQuantity;
                    if (!(availableQuantity > 0)) {

                        for (let cl = 0; cl < cart.CartLines.length; cl++) {
                            if (!cart.CartLines[cl].IsVoided && cart.CartLines[cl].ProductId == results[idx].product.RecordId) {
                                voidCartLines.push(cart.CartLines[cl]);
                                let item = cart.CartLines[cl].ItemId + '::' + cart.CartLines[cl].Description;
                                if (products.indexOf(item) == -1) {
                                    products += item + '\r\n';
                                }
                            }
                        }
                    } else if (!aggregateItem) {

                        let lines = tempCartLine;
                        let mapLines = lines.reduce((prev, next) => {
                            if (next.ProductId in prev) {
                                if (!next.IsVoided) {
                                    prev[next.ProductId].Quantity += next.Quantity;
                                    prev[next.ProductId].LineId = next.LineId;
                                }
                            } else {
                                prev[next.ProductId] = next;
                            }
                            return prev;
                        }, {});
                        let aggregateCartLine = Object.keys(mapLines).map(id => mapLines[id]);
                        //console.log(aggregateCartLine);

                        let cartLine: Commerce.Proxy.Entities.CartLine = null;
                        //let itemQty: number = 0;
                        for (let cl = 0; cl < aggregateCartLine.length; cl++) {
                            cartLine = aggregateCartLine[cl];
                            if (cartLine.ProductId == results[idx].product.RecordId && cartLine.Quantity > availableQuantity) {
                                let item = cartLine.ItemId + '::' + cartLine.Description;
                                if (products.indexOf(item) == -1) {
                                    products += item + '\r\n';
                                    strQuantity += availableQuantity + ', ';
                                }
                                cartLine.Quantity = results[idx].quantity;
                                voidCartLines.push(cartLine);
                            }
                        }
                    } else {

                        let cartLine: Commerce.Proxy.Entities.CartLine = null;
                        for (let cl = 0; cl < cart.CartLines.length; cl++) {
                            cartLine = cart.CartLines[cl];
                            if (!cartLine.IsVoided && cartLine.ProductId == results[idx].product.RecordId && cartLine.Quantity > availableQuantity) {
                                let item = cartLine.ItemId + '::' + cartLine.Description;
                                if (products.indexOf(item) == -1) {
                                    products += item + '\r\n';
                                    strQuantity += availableQuantity + ', ';
                                }
                                cartLine.Quantity = availableQuantity;
                                updateCartLine.push(cartLine);
                            }
                        }
                    }
                }

                if (voidCartLines.length !== 0) {
                    let lineId: string[] = [];
                    for (let vcl: number = 0; vcl < voidCartLines.length; vcl++) {
                        lineId.push(voidCartLines[vcl].LineId);
                    }
                    let tempLineId: string[] = JSON.parse(sessionStorage.getItem("RR_LessQtyVoiedItem")) || [];
                    lineId = lineId.concat(tempLineId);
                    sessionStorage.setItem("RR_LessQtyVoiedItem", JSON.stringify(lineId));

                    this.context.runtime.executeAsync(new Products.VoidCartLinesRequest(cart.Id, voidCartLines))
                        .then(() => {
                            correlationId = this.context.logger.getNewCorrelationId();
                            this.context.runtime.executeAsync(new Commerce.RefreshCartClientRequest(correlationId))
                                .then((resp: any) => {
                                    console.log(resp);
                                    ErrorHelper.displayMessageAsync(this.context, StringExtensions.format(this.context.resources.getString('PRODUCT_QUANTITY_ZERO_ALERT'), products));
                                });
                        });
                }
                if (updateCartLine.length !== 0) {
                    strQuantity = strQuantity.substring(0, strQuantity.length - 2);
                    this.context.runtime.executeAsync(new Products.UpdateCartLinesRequest(cart.Id, updateCartLine))
                        .then(() => {
                            correlationId = this.context.logger.getNewCorrelationId();
                            this.context.runtime.executeAsync(new Commerce.RefreshCartClientRequest(correlationId))
                                .then((resp: any) => {
                                    console.log(resp);
                                    ErrorHelper.displayMessageAsync(this.context, StringExtensions.format(this.context.resources.getString('PRODUCT_QUANTITY_LESS_ALERT'), products, strQuantity));
                                });
                        });
                }
            }).catch(err => {
                console.log(err);
            });

            return Promise.resolve();
        
    }
    
    private GetProductsQuantity(PSDs: Entities.IProductSaleReturnDetailsExt[]): Promise<any> {
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

    private getConfiguration(): void {
        let str_configuration: string = sessionStorage.getItem("RR_UtilitiesConfiguration");
        let configuration: Entities.UtilitiesConfiguration = JSON.parse(str_configuration);
        this.isBlockSale = false;
        if (!Commerce.ObjectExtensions.isNullOrUndefined(configuration))
            this.isBlockSale = configuration.BlockSales == 1;
    }

}