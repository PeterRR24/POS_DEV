import * as Triggers from "PosApi/Extend/Triggers/ProductTriggers";
import { StringExtensions } from "PosApi/TypeExtensions";
import { ClientEntities } from "PosApi/Entities";
import { Products } from "../DataService/DataServiceRequests.g";
import { Entities } from "../DataService/DataServiceEntities.g";
import { ErrorHelper } from "../../ErrorHelper";

export default class PostSetQuantityTrigger extends Triggers.PostSetQuantityTrigger {

    private isBlockSale: boolean;

    public execute(options: Triggers.IPostSetQuantityTriggerOptions): Promise<void> {
        this.context.logger.logVerbose("Executing PostSetQuantityTrigger with options " + JSON.stringify(options) + " at " + new Date().getTime() + ".");

        this.getConfiguration();
        if (!this.isBlockSale || (options.cart.CartTypeValue == 3 && options.cart.CustomerOrderModeValue != 3))
            return Promise.resolve();

        let correlationId: string = '';
        let cart: Commerce.Proxy.Entities.Cart = options.cart;
        let updateCartLine: Commerce.Proxy.Entities.CartLine[] = options.cartLines;
        let tempCartLine: Commerce.Proxy.Entities.CartLine[] = [];
        let products: string = '';
        let availableQuantity: number = 0;
        let updateQty: number = 0;
        let transactionQty: number = 0;

        let rrCommerce: any = Commerce;
        let deviceConfig = new Commerce.Proxy.Entities.DeviceConfigurationClass(JSON.parse(rrCommerce.ApplicationStorage.getItem(rrCommerce.ApplicationStorageIDs.DEVICE_CONFIGURATION_KEY)));
        let aggregateItem: boolean = deviceConfig.AllowItemsAggregation;
        let channelId: number = deviceConfig.ChannelId;

        this.context.runtime.executeAsync(new Products.GetSimpleProductRequest(channelId, updateCartLine[0].ProductId))
            .then((resp: any) => {
                let SP: Commerce.Proxy.Entities.SimpleProduct = resp.data.result;
                if (SP.ItemTypeValue !== 2) {
                    let itemQtyRequest: Products.GetProductAvailableQuantityRequest<Products.GetProductAvailableQuantityResponse>
                        = new Products.GetProductAvailableQuantityRequest<Products.GetProductAvailableQuantityResponse>(updateCartLine[0].ProductId, updateCartLine[0].ItemId, '');
                    this.context.runtime.executeAsync(itemQtyRequest)
                        .then((resp: ClientEntities.ICancelableDataResult<Products.GetProductAvailableQuantityResponse>): any => {
                            if (!resp.canceled) {
                                availableQuantity = resp.data.result;
                                transactionQty = updateCartLine[0].Quantity;
                                if (!aggregateItem) {
                                    tempCartLine = cart.CartLines;
                                    let mapLines = tempCartLine.reduce((prev, next) => {
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
                                    var matchedLines = aggregateCartLine.filter(cl => cl.ProductId == updateCartLine[0].ProductId);
                                    if (matchedLines.length)
                                        transactionQty = matchedLines[0].Quantity;
                                    //console.log(matchedLines);
                                }
                                if (transactionQty > availableQuantity) {
                                    updateQty = parseInt(sessionStorage.getItem("RR_InitialTransQuantity"), 10);
                                    updateCartLine[0].Quantity = updateQty;
                                    products = updateCartLine[0].ItemId + '::' + updateCartLine[0].Description + '\r\n';
                                    this.context.runtime.executeAsync(new Products.UpdateCartLinesRequest(cart.Id, updateCartLine))
                                        .then(() => {
                                            correlationId = this.context.logger.getNewCorrelationId();
                                            this.context.runtime.executeAsync(new Commerce.RefreshCartClientRequest(correlationId))
                                                .then((resp: any) => {
                                                    console.log(resp);
                                                    ErrorHelper.displayMessageAsync(this.context, StringExtensions.format(this.context.resources.getString('PRODUCT_QUANTITY_LESS_ALERT'), products, availableQuantity));
                                                });
                                        });
                                }

                            }
                        });
                }
            });
        
        return Promise.resolve();
    }

    private getConfiguration(): void {
        let str_configuration: string = sessionStorage.getItem("RR_UtilitiesConfiguration");
        let configuration: Entities.UtilitiesConfiguration = JSON.parse(str_configuration);
        this.isBlockSale = false;
        if (!Commerce.ObjectExtensions.isNullOrUndefined(configuration))
            this.isBlockSale = configuration.BlockSales == 1;
    }

}