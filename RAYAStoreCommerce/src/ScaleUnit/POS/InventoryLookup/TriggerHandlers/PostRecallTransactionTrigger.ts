
import * as Triggers from "PosApi/Extend/Triggers/TransactionTriggers";
import { StringExtensions } from "PosApi/TypeExtensions";
import { ClientEntities } from "PosApi/Entities";
import { Products } from "../DataService/DataServiceRequests.g";
import { Entities } from "../DataService/DataServiceEntities.g";
import { ErrorHelper } from "../../ErrorHelper";

export default class PostRecallTransactionTrigger extends Triggers.PostRecallTransactionTrigger {

    private isBlockSale: boolean;

    public execute(options: Triggers.IPostRecallTransactionTriggerOptions): Promise<void> {
        this.context.logger.logVerbose("Executing PostRecallTransactionTrigger with options " + JSON.stringify(options) + " at " + new Date().getTime() + ".");
        let me = this;
        this.getConfiguration();
        if (!this.isBlockSale || (options.cart.CartTypeValue == 3 && options.cart.CustomerOrderModeValue != 3))
            return Promise.resolve();

        sessionStorage.setItem("RR_LessQtyRecallOrder", "0");
        let cartLines: Commerce.Proxy.Entities.CartLine[] = options.cart.CartLines;
        let products: string = '';
        let strQuantity: string = '';
        let rrCommerce: any = Commerce;
        let deviceConfig = new Commerce.Proxy.Entities.DeviceConfigurationClass(JSON.parse(rrCommerce.ApplicationStorage.getItem(rrCommerce.ApplicationStorageIDs.DEVICE_CONFIGURATION_KEY)));
        let aggregateItem: boolean = deviceConfig.AllowItemsAggregation;
        let prodIds: number[] = cartLines.filter(obj => !obj.IsVoided).map(cl => cl.ProductId);
        return me.context.runtime.executeAsync(new Products.GetSimpleProductsRequest(deviceConfig.ChannelId, prodIds))
            .then((resp: ClientEntities.ICancelableDataResult<Products.GetSimpleProductsResponse>): any => {
                if (!resp.canceled) {
                    let RequestPSDs: Entities.IProductSaleReturnDetailsExt[] = [];
                    let PSDs: Entities.IProductSaleReturnDetailsExt[] = [];
                    for (let cnt = 0; cnt < resp.data.result.length; cnt++) {
                        let PSD: Entities.IProductSaleReturnDetailsExt = { quantity: 0, product: resp.data.result[cnt] };
                        // PSD.product = resp.data.result[cnt];
                        RequestPSDs.push(PSD);
                    }
                    for (let i = 0; i < RequestPSDs.length; i++) {
                        if (RequestPSDs[i].product.ItemTypeValue !== 2) {
                            PSDs.push(RequestPSDs[i]);
                        }
                    }
                    if (PSDs.length == 0)
                        return Promise.resolve({ canceled: false });
                    else
                        return PSDs;
                }
            }).then((PSDs: Entities.IProductSaleReturnDetailsExt[]): any => {

                return me.GetProductsQuantity(PSDs);

            }).then((results: Entities.IProductSaleReturnDetailsExt[]): any => {

                for (let j = 0; j < results.length; j++) {
                    let availableQuantity: number = results[j].AvailableQuantity;
                    if (!(availableQuantity > 0)) {

                        for (let cl = 0; cl < cartLines.length; cl++) {
                            if (!cartLines[cl].IsVoided && cartLines[cl].ProductId == results[j].product.RecordId) {
                                //voidCartLines.push(cartLines[cl]);
                                let item = cartLines[cl].ItemId + '::' + results[j].product.Description;
                                if (products.indexOf(item) == -1) {
                                    products += item + '\r\n';
                                    strQuantity += availableQuantity + ', ';
                                }
                            }
                        }
                    } else if (!aggregateItem) {

                        let lines = cartLines;
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
                        for (let acl = 0; acl < aggregateCartLine.length; acl++) {
                            cartLine = aggregateCartLine[acl];
                            if (cartLine.ProductId == results[j].product.RecordId && cartLine.Quantity > availableQuantity) {
                                let item = cartLine.ItemId + '::' + results[j].product.Description;
                                if (products.indexOf(item) == -1) {
                                    products += item + '\r\n';
                                    strQuantity += availableQuantity + ', ';
                                }
                                cartLine.Quantity = results[j].quantity;
                                //voidCartLines.push(cartLine);
                            }
                        }
                    } else {

                        let cartLine: Commerce.Proxy.Entities.CartLine = null;
                        for (let cl = 0; cl < cartLines.length; cl++) {
                            cartLine = cartLines[cl];
                            if (!cartLine.IsVoided && cartLine.ProductId == results[j].product.RecordId && cartLine.Quantity > availableQuantity) {
                                let item = cartLine.ItemId + '::' + results[j].product.Description;
                                if (products.indexOf(item) == -1) {
                                    products += item + '\r\n';
                                    strQuantity += availableQuantity + ', ';
                                }
                                cartLine.Quantity = availableQuantity;
                                //updateCartLine.push(cartLine);
                            }
                        }
                    }
                }

            }).then((): any => {

                if (products.length > 0) {
                    sessionStorage.setItem("RR_LessQtyRecallOrder", "1");
                    return ErrorHelper.displayMessageAsync(me.context, StringExtensions.format(me.context.resources.getString('PRODUCT_QUANTITY_LESS_ALERT'), products, strQuantity));
                } else
                    return Promise.resolve();

            }).catch(err => {
                console.log(err);
                return ErrorHelper.displayMessageAsync(me.context, JSON.stringify(err));
            });

        //return Promise.resolve();
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