
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import { Entities } from "../../DataService2/DataServiceEntities.g";
import { Labels } from "../../DataService2/DataServiceRequests.g";
import { IExtensionCommandContext } from "PosApi/Extend/Views/AppBarCommands";
import * as SimpleProductDetailsView from "PosApi/Extend/Views/SimpleProductDetailsView";
import AddLabelDialogModule from "../../Controls/DialogControl/AddLabelDialogModule";
import { IAddLabelDialogResult } from "../../Controls/DialogControl/IAddLabelDialogResult";
import MessageDialog from "../../Controls/DialogControl/MessageDialog";
import { ObjectExtensions, StringExtensions } from "PosApi/TypeExtensions";
import ko from "knockout";

export default class PrintProductLabelCommand extends SimpleProductDetailsView.SimpleProductDetailsExtensionCommandBase {

    private _product: ProxyEntities.SimpleProduct;
    private _labelCodes: any;
    
    /**
     * Creates a new instance of the PrintProductLabelCommand class.
     * @param {IExtensionCommandContext<ISimpleProductDetailsToExtensionCommandMessageTypeMap>} context The command context.
     * @remarks The command context contains APIs through which a command can communicate with POS.
     */
    constructor(context: IExtensionCommandContext<SimpleProductDetailsView.ISimpleProductDetailsToExtensionCommandMessageTypeMap>) {
        super(context);

        this.id = "printLabelCommand";
        this.label = this.context.resources.getString("PRINT_LABEL_CMD_TEXT");
        this.extraClass = "iconGo"
        this._labelCodes = ko.observableArray([]);
        
        this.productChangedHandler = (data: SimpleProductDetailsView.SimpleProductDetailsProductChangedData): void => {
            this._productChanged(data.product);
        };
        this.canExecute = true;
        this.isVisible = true;
    }

    /**
     * Initializes the command.
     * @param {ISimpleProductDetailsExtensionCommandState} state The state used to initialize the command.
     */
    protected init(state: SimpleProductDetailsView.ISimpleProductDetailsExtensionCommandState): void {
        this._productChanged(state.product);
        this.GetLabelCodes();
    }

    /**
     * Executes the command.
     */
    protected execute(): void {
        
        let selectedLabelType: number = ObjectExtensions.isNullOrUndefined(sessionStorage.getItem("LabelType")) ? 0 : parseInt(sessionStorage.getItem("LabelType"));
        let codeRecId: number = ObjectExtensions.isNullOrUndefined(sessionStorage.getItem("CodeRecId")) ? 0 : parseInt(sessionStorage.getItem("CodeRecId"));
        let isVatiantSel: boolean = !(this._product.ProductTypeValue === ProxyEntities.ProductType.Master);
        let dialog: AddLabelDialogModule = new AddLabelDialogModule(selectedLabelType, true, codeRecId, isVatiantSel);
        dialog.open(this.context.resources.getString('PRINT_LABEL_QTY'), this._product.Description, "", this._labelCodes())
            .then((result: IAddLabelDialogResult) => {
                this.context.logger.logInformational("Dialog result: " + result.selectedValue);
                if (result.selectedValue == 'Closed') {
                    return;
                }
                var quantity: number = parseInt(result.selectedValue);
                var labelType: number = parseInt(result.itemLabelType);
                if (isNaN(quantity)) {
                    this.context.logger.logError("Please enter numbers only!");
                    return;
                }
                if (quantity <= 0) {
                    this.context.logger.logError("The label printing quantity cannot be zero or negative");
                    return;
                }
                if (isNaN(labelType)) {
                    this.context.logger.logError("Please enter numbers only!");
                    return;
                }

                this.isProcessing = true;
                let calculateItem: Entities.LabelItemCalculateItem = new Entities.LabelItemCalculateItem();
                let labelCalcParam: Entities.LabelItemCalculateParams;
                let labelCode: Entities.LabelCode = result.labelCode;
                calculateItem.ItemId = this._product.ItemId;                
                calculateItem.UnitOfMeasure = this._product.DefaultUnitOfMeasure;
                calculateItem.Quantity = quantity;

                if (ObjectExtensions.isNullOrUndefined(labelCode)) {
                    labelCode = new Entities.LabelCode();
                }
                labelCalcParam = new Entities.LabelItemCalculateParams();
                labelCalcParam.CodeRecId = isNaN(labelCode.Id) ? 0 : labelCode.Id;
                labelCalcParam.PriceDate = result.priceDate;

                if (this._product.ProductTypeValue === ProxyEntities.ProductType.Variant && labelType == 1) {
                    calculateItem.ProductId = this._product.RecordId;
                }
                
                sessionStorage.setItem("CodeRecId", labelCalcParam.CodeRecId.toString());
                sessionStorage.setItem("LabelType", labelType.toString());

                let request: Labels.CalculateLabelItemRequest<Labels.CalculateLabelItemResponse>
                    = new Labels.CalculateLabelItemRequest<Labels.CalculateLabelItemResponse>(labelType, labelCalcParam, [calculateItem], null, 0);

                this.context.runtime.executeAsync(request)
                    .then((result: ClientEntities.ICancelableDataResult<Labels.CalculateLabelItemResponse>) => {
                        this.isProcessing = false;
                        if (!result.canceled) {
                            if (result.data.result > 0) {
                                this.displayClientMessage(StringExtensions.format(this.context.resources.getString("CALCUALTE_ITEM_RESULT_PASS"), this._product.Name), "Information");
                            } else {
                                this.displayClientMessage(StringExtensions.format(this.context.resources.getString("CALCUALTE_ITEM_RESULT_FAIL"), this._product.Name), "Information");
                            }
                        }

                    })
                    .catch((reason: any): void => {
                        this.isProcessing = false;
                        this.context.logger.logError("Calculate label item error: " + JSON.stringify(reason));
                        this.handleError(reason);
                    });

            }).catch((reason: any) => {
                this.context.logger.logError("Dialog error: " + JSON.stringify(reason));
                this.handleError(reason);
            });

    }

    public GetLabelCodes(): void {
        
        let request: Labels.GetLabelCodeRequest<Labels.GetLabelCodeResponse>
            = new Labels.GetLabelCodeRequest<Labels.GetLabelCodeResponse>();
        this.isProcessing = true;

        this.context.runtime.executeAsync(request)
            .then((result: ClientEntities.ICancelableDataResult<Labels.GetLabelCodeResponse>) => {
                this.isProcessing = false;
                if (!result.canceled) {
                    this._labelCodes(result.data.result);
                }
            })
            .catch((reason: any): void => {
                this.isProcessing = false;
                this.context.logger.logError("Label code get error: " + JSON.stringify(reason));
                this.handleError(reason);
            });
    }

    private handleError(error: any): void {
        var rsError: Entities.RetailServerError = error || {};
        if (error.length) {
            rsError = error[0] || {};
        }
        var dlgTitle: string = rsError.name;
        var errorMsg: string = rsError._externalLocalizedErrorMessage;
        if (StringExtensions.isNullOrWhitespace(dlgTitle) || StringExtensions.isEmpty(dlgTitle)) {
            dlgTitle = this.context.resources.getString("ERROR_TITLE");
        }
        if (StringExtensions.isNullOrWhitespace(errorMsg) || StringExtensions.isEmpty(errorMsg)) {
            errorMsg = JSON.stringify(rsError);
        }
        this.displayClientMessage(errorMsg, dlgTitle);
    }

    private displayClientMessage(msg: string, dlgTitle: string): void {
        MessageDialog.show(this.context, msg, dlgTitle);
    }

    /**
     * Handles the product changed message by sending a message indicating whether or not the command can be executed.
     * @param {ProxyEntities.SimpleProduct} product The product.
     */
    private _productChanged(product: ProxyEntities.SimpleProduct): void {
        this._product = product;
        //let isMasterProduct: boolean = product.ProductTypeValue === ProxyEntities.ProductType.Master;
        //this.canExecute = !isMasterProduct;
    }
}

