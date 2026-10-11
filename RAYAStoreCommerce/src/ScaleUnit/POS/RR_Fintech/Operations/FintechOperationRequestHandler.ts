import { ExtensionOperationRequestType, ExtensionOperationRequestHandlerBase } from "PosApi/Create/Operations";
import FintechOperationResponse from "./FintechOperationResponse";
import FintechOperationRequest from "./FintechOperationRequest";
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import { GetCurrentCartClientRequest, GetCurrentCartClientResponse } from "PosApi/Consume/Cart";
import { Fintech } from "../DataService/DataServiceRequests.g";
import FintechDialog, { IFintechDialogResult } from "../Controls/DialogControl/FintechDialog";
export default class FintechOperationHandler
    extends ExtensionOperationRequestHandlerBase<FintechOperationResponse> {

    public supportedRequestType(): ExtensionOperationRequestType<FintechOperationResponse> {
        return FintechOperationRequest;
    }

    public executeAsync(
        request: FintechOperationRequest
    ): Promise<ClientEntities.ICancelableDataResult<FintechOperationResponse>> {

        this.context.logger.logInformational(
            "FintechOperationHandler executeAsync().",
            request.correlationId
        );

        let getCurrentCartClientRequest:
            GetCurrentCartClientRequest<GetCurrentCartClientResponse> =
            new GetCurrentCartClientRequest(
                request.correlationId
            );

        return this.context.runtime.executeAsync(
            getCurrentCartClientRequest
        ).then(
            (
                getCurrentCartClientResponse:
                    ClientEntities.ICancelableDataResult<
                        GetCurrentCartClientResponse
                    >
            ) => {

                if (getCurrentCartClientResponse.canceled) {
                    return Promise.resolve(
                        <ClientEntities.ICancelableDataResult<FintechOperationResponse>>{
                            canceled: true,
                            data: null
                        }
                    );
                }

                let cart: ProxyEntities.Cart =
                    getCurrentCartClientResponse.data.result;

                this.context.logger.logInformational(
                    "Fintech cart total: " +
                    cart.TotalAmount,
                    request.correlationId
                );

                /*
                 * Get fintech providers.
                 */
                let getProvidersRequest:
                    Fintech.GetFintechProvidersRequest<Fintech.GetFintechProvidersResponse> =
                    new Fintech.GetFintechProvidersRequest();

                return this.context.runtime.executeAsync(
                    getProvidersRequest
                ).then(
                    (
                        getProvidersResponse:
                            ClientEntities.ICancelableDataResult<
                                Fintech.GetFintechProvidersResponse
                            >
                    ) => {

                        if (getProvidersResponse.canceled) {
                            return Promise.resolve(
                                <ClientEntities.ICancelableDataResult<FintechOperationResponse>>{
                                    canceled: true,
                                    data: null
                                }
                            );
                        }

                        let providers =
                            getProvidersResponse.data.result;

                        this.context.logger.logInformational(
                            "Fintech providers retrieved: " +
                            providers.length,
                            request.correlationId
                        );

                        /*
                         * Open fintech dialog.
                         */
                        let fintechDialog:
                            FintechDialog =
                            new FintechDialog();

                        return fintechDialog.open(
                            cart,
                            providers
                        ).then(
                            (
                                dialogResult:
                                    ClientEntities.ICancelableDataResult<IFintechDialogResult>
                            ) => {

                                if (dialogResult.canceled) {
                                    return Promise.resolve(
                                        <ClientEntities.ICancelableDataResult<FintechOperationResponse>>{
                                            canceled: true,
                                            data: null
                                        }
                                    );
                                }

                                return Promise.resolve(
                                    <ClientEntities.ICancelableDataResult<FintechOperationResponse>>{
                                        canceled: false,
                                        data:
                                            new FintechOperationResponse()
                                    }
                                );
                            }
                        );
                    }
                );
            }
        );
    }
}