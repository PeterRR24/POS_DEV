import * as Dialogs from "PosApi/Create/Dialogs";

import {
    ObjectExtensions
} from "PosApi/TypeExtensions";

import {
    ProxyEntities,
    ClientEntities
} from "PosApi/Entities";

import ko from "knockout";

import { Entities } from "../../DataService/DataServiceEntities.g";
import { Fintech } from "../../DataService/DataServiceRequests.g";

import { CurrencyFormatter } from "PosApi/Consume/Formatters";
export interface IFintechDialogResult {

    provider: number;

    plan: Entities.ProviderPlan;

    cash: number;

    originalTotal: number;

    financedAmount: number;

    interestAmount: number;

    financedTotal: number;

    newTotal: number;

    monthlyPayment: number;
}
export default class FintechDialog
    extends Dialogs.ExtensionTemplatedDialogBase {

    // ---------------------------------------------------------------------
    // Public properties used by the HTML bindings
    // ---------------------------------------------------------------------

    public providers: Entities.Provider[];

    public providerPlans: Entities.ProviderPlan[];

    public providerOptions: ko.ObservableArray<Entities.Provider>;

    public periodOptions: ko.ObservableArray<Entities.ProviderPlan>;

    public selectedProviderId: ko.Observable<number>;

    public selectedPeriodIndex: ko.Observable<number>;

    public selectedPeriod: ko.Observable<Entities.ProviderPlan>;

    public cash: ko.Observable<number>;

    public total: ko.Observable<number>;

    public financedAmount: ko.Computed<number>;

    public interestAmount: ko.Computed<number>;

    public newTotal: ko.Computed<number>;

    public monthlyPayment: ko.Computed<string>;

    public validationMessage: ko.Observable<string>;

    // ---------------------------------------------------------------------
    // Private state
    // ---------------------------------------------------------------------

    //private _cart: ProxyEntities.Cart;

    private _canceled: boolean;

    private _resolve: (result: ClientEntities.ICancelableDataResult<IFintechDialogResult>) => void;

    constructor() {

        super();

        this.providers = [];
        this.providerPlans = [];

        this.providerOptions =
            ko.observableArray<Entities.Provider>([]);

        this.periodOptions =
            ko.observableArray<Entities.ProviderPlan>([]);

        this.selectedProviderId =
            ko.observable<number>(undefined);

        this.selectedPeriodIndex =
            ko.observable<number>(-1);

        this.selectedPeriod =
            ko.observable<Entities.ProviderPlan>(undefined);

        this.cash =
            ko.observable<number>(0);

        this.total =
            ko.observable<number>(0);

        this.validationMessage =
            ko.observable<string>("");

        this.financedAmount = ko.computed(
            (): number => {

                let total: number = this.total();
                let cash: number = this.cash();

                let financed: number =
                    total - cash;

                return Math.max(
                    0,
                    this._roundToTwoDecimals(financed)
                );
            },
            this
        );

        this.interestAmount = ko.computed(
            (): number => {

                let plan: Entities.ProviderPlan =
                    this.selectedPeriod();

                if (!plan) {
                    return 0;
                }

                let financed: number =
                    this.financedAmount();

                let interest: number =
                    financed *
                    (plan.InterestPercent / 100);

                return this._roundToTwoDecimals(
                    interest
                );
            },
            this
        );

        this.newTotal = ko.computed(
            (): number => {

                let financed: number =
                    this.financedAmount();

                let interest: number =
                    this.interestAmount();

                let cash: number =
                    this.cash();

                return this._roundToTwoDecimals(
                    cash +
                    financed +
                    interest
                );
            },
            this
        );

        this._canceled = false;

        /*
         * Calculate monthly payment whenever Cash or Period changes.
         */
        this.monthlyPayment = ko.computed(
            (): string => {

                let plan: Entities.ProviderPlan =
                    this.selectedPeriod();

                if (!plan || plan.Months <= 0) {
                    return CurrencyFormatter.toCurrency(0);
                }

                let financed: number =
                    this.financedAmount();

                let interest: number =
                    this.interestAmount();

                let financedTotal: number =
                    financed + interest;

                let monthly: number =
                    financedTotal / plan.Months;

                return CurrencyFormatter.toCurrency(
                    this._roundToTwoDecimals(monthly)
                );
            },
            this
        );
    }

    // ---------------------------------------------------------------------
    // Dialog lifecycle
    // ---------------------------------------------------------------------

    public onReady(element: HTMLElement): void {

        ko.applyBindings(this, element);

    }

    /**
     * Opens the fintech dialog.
     */
    public open(
        cart: ProxyEntities.Cart,
        providers: Entities.Provider[]
    ): Promise<ClientEntities.ICancelableDataResult<IFintechDialogResult>> {

        //this._cart = cart;

        this.providers = providers || [];

        this.providerOptions(
            this.providers
        );

        /*
         * Total comes directly from the current cart.
         */
        this.total(
            cart ? cart.TotalAmount : 0
        );

        this.cash(0);

        this.selectedProviderId(undefined);

        this.selectedPeriod(undefined);

        this.periodOptions([]);

        this.validationMessage("");

        this._canceled = false;

        return new Promise<ClientEntities.ICancelableDataResult<IFintechDialogResult>>(
            (
                resolve: (
                    result: ClientEntities.ICancelableDataResult<IFintechDialogResult>
                ) => void
            ) => {

                this._resolve = resolve;

                let options:
                    Dialogs.ITemplatedDialogOptions = {

                    title: "Fintech",

                    onCloseX:
                        this.onCloseX.bind(this),

                    button1: {

                        id: "FintechCancelButton",

                        label: "Cancel",

                        onClick:
                            this._cancelClickHandler.bind(this)
                    },

                    button2: {

                        id: "FintechOkayButton",

                        label: "OK",

                        isPrimary: true,

                        onClick:
                            this._okayClickHandler.bind(this)
                    }
                };

                this.openDialog(options);
            }
        );
    }

    // ---------------------------------------------------------------------
    // Provider selection
    // ---------------------------------------------------------------------

    public onProviderChanged(
        providerId: number
    ): void {

        this.selectedProviderId(
            providerId
        );

        this.selectedPeriod(undefined);

        this.periodOptions([]);

        this.validationMessage("");

        if (ObjectExtensions.isNullOrUndefined(providerId)) {
            return;
        }

        let request:
            Fintech.GetFintechProviderPlansRequest<Fintech.GetFintechProviderPlansResponse> =
            new Fintech.GetFintechProviderPlansRequest(
                providerId
            );

        this.context.runtime.executeAsync(
            request
        ).then(
            (
                response:
                    ClientEntities.ICancelableDataResult<
                        Fintech.GetFintechProviderPlansResponse
                    >
            ) => {

                if (response.canceled) {
                    return;
                }

                let plans:
                    Entities.ProviderPlan[] =
                    response.data.result || [];

                this.providerPlans = plans;

                this.periodOptions(
                    plans
                );

            }
        );
    }

    // ---------------------------------------------------------------------
    // Period selection
    // ---------------------------------------------------------------------
    public onPeriodChanged(
        index: number
    ): void {

        if (index < 0 ||
            index >= this.providerPlans.length) {

            this.selectedPeriod(undefined);
            return;
        }

        this.selectedPeriod(
            this.providerPlans[index]
        );

        this.validationMessage("");
    }


   /* public onPeriodChanged(
        months: number
    ): void {

        let selectedPlan:
            Entities.ProviderPlan =
            ArrayExtensions.firstOrUndefined(
                this.providerPlans,
                (
                    plan: Entities.ProviderPlan
                ): boolean => {

                    return plan.Months === months;
                }
            );

        this.selectedPeriod(
            selectedPlan
        );

        this.validationMessage("");

    }*/

    // ---------------------------------------------------------------------
    // Cash
    // ---------------------------------------------------------------------

    public onCashChanged(
        cashValue: string
    ): void {

        let value:
            number =
            parseFloat(cashValue);

        if (isNaN(value)) {
            value = 0;
        }

        this.cash(value);

        this.validationMessage("");

    }

    // ---------------------------------------------------------------------
    // OK / Cancel
    // ---------------------------------------------------------------------

    private _okayClickHandler(): boolean {

        if (!this._validate()) {
            return false;
        }

        this._canceled = false;

        this._resolvePromise();

        return true;
    }

    private _cancelClickHandler(): boolean {

        this._canceled = true;

        this._resolvePromise();

        return true;
    }

    private onCloseX(): boolean {

        this._canceled = true;

        this._resolvePromise();

        return true;
    }

    private _resolvePromise(): void {

        if (ObjectExtensions.isFunction(this._resolve)) {

            this._resolve(
                {
                    canceled: this._canceled,
                    data: this._canceled
                        ? null
                        : {
                            provider:
                                this.selectedProviderId(),
                            originalTotal:
                                this.total(),
                            financedAmount:
                                this.financedAmount(),
                            interestAmount:
                                this.interestAmount(),
                            financedTotal:
                                this.total(),
                            monthlyPayment:
                                this.monthlyPayment(),
                            plan:
                                this.selectedPeriod(),

                            cash:
                                this.cash(),

                            newTotal:
                                this.newTotal()
                        }
                }
            );

            this._resolve = null;
        }
    }

    // ---------------------------------------------------------------------
    // Validation
    // ---------------------------------------------------------------------

    private _validate(): boolean {

        this.validationMessage("");

        let providerId:
            number =
            this.selectedProviderId();

        if (ObjectExtensions.isNullOrUndefined(providerId)) {

            this.validationMessage(
                "Please select a provider."
            );

            return false;
        }

        let selectedPlan:
            Entities.ProviderPlan =
            this.selectedPeriod();

        if (ObjectExtensions.isNullOrUndefined(selectedPlan)) {

            this.validationMessage(
                "Please select a payment period."
            );

            return false;
        }

        let cashAmount:
            number =
            this.cash();

        let cartTotal:
            number =
            this.total();

        if (cashAmount < 0) {

            this.validationMessage(
                "Cash cannot be negative."
            );

            return false;
        }

        if (cashAmount > cartTotal) {

            this.validationMessage(
                "Cash cannot be greater than the cart total."
            );

            return false;
        }

        if (
            selectedPlan.MinimumCash > 0 &&
            cashAmount < selectedPlan.MinimumCash
        ) {

            this.validationMessage(
                "Minimum cash required is " +
                selectedPlan.MinimumCash
            );

            return false;
        }

        let financedAmount:
            number =
            cartTotal - cashAmount;

        if (
            selectedPlan.MinimumFinance > 0 &&
            financedAmount < selectedPlan.MinimumFinance
        ) {

            this.validationMessage(
                "Minimum financed amount is " +
                selectedPlan.MinimumFinance
            );

            return false;
        }

        if (
            selectedPlan.MaximumFinance > 0 &&
            financedAmount > selectedPlan.MaximumFinance
        ) {

            this.validationMessage(
                "Maximum financed amount is " +
                selectedPlan.MaximumFinance
            );

            return false;
        }

        return true;
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------

    private _roundToTwoDecimals(
        value: number
    ): number {

        return Math.round(
            (value + 0) * 100
        ) / 100;
    }
}

