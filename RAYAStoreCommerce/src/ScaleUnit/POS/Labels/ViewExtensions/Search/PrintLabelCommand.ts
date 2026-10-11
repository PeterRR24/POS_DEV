
import { IExtensionCommandContext } from "PosApi/Extend/Views/AppBarCommands";
import * as SearchView from "PosApi/Extend/Views/SearchView";
//import MessageDialog from "../../Controls/DialogControl/MessageDialog";

export default class PrintLabelCommand extends SearchView.ProductSearchExtensionCommandBase {
    /**
     * Creates a new instance of the NavigateToSimpleExtensionViewCommand class.
     * @param {IExtensionCommandContext<ProductDetailsView.IProductSearchToExtensionCommandMessageTypeMap>} context The command context.
     * @remarks The command context contains APIs through which a command can communicate with POS.
     */
    constructor(context: IExtensionCommandContext<SearchView.IProductSearchToExtensionCommandMessageTypeMap>) {
        super(context);

        this.id = "navigateToSimpleExtensionViewCommand";
        this.label = "RR Labels View";
        this.extraClass = "iconGo";
    }

    /**
     * Initializes the command.
     * @param {ProductDetailsView.IProductDetailsExtensionCommandState} state The state used to initialize the command.
     */
    protected init(state: SearchView.IProductSearchExtensionCommandState): void {
        this.canExecute = true;
        this.isVisible = true;
    }

    /**
     * Executes the command.
     */
    protected execute(): void {
        //MessageDialog.show(this.context, "ItemLabelPrintWzrdView");
        // this.context.navigator.navigate("ItemLabelPrintWzrdView");
        
    }
}