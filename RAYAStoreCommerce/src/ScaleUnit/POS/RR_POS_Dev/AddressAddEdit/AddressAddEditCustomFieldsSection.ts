/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */



import { AddressAddEditCustomControlBase, IAddressAddEditCustomControlState, IAddressAddEditCustomControlContext } from "PosApi/Extend/Views/AddressAddEditView";
//import { ObjectExtensions, StringExtensions } from "PosApi/TypeExtensions";
//import * as Controls from "PosApi/Consume/Controls";
import { KKAS } from "../DataService/DataServiceRequests.g";
import { ClientEntities, ProxyEntities } from "PosApi/Entities";
import { ErrorHelper } from "../ErrorHelper";
import { Entities } from "../DataService/DataServiceEntities.g";
import ko from "knockout";

//import { GetCurrentCartClientRequest, GetCurrentCartClientResponse, AddItemToCartOperationResponse } from "PosApi/Consume/Cart";
//import { PreSaveCustomerAddressTrigger } from "PosApi/Extend/Triggers/CustomerTriggers";
export { ProxyEntities };
export { Entities };
//type ICancelableDataResult<TResult> = ClientEntities.ICancelableDataResult<TResult>;

/**
 * This is extension custom control for address add/edit view. It adds to the page 2 new fields: notes and toggle for residential address.
 */
export default class AddressAddEditCustomFieldsSection extends AddressAddEditCustomControlBase {
    private static readonly TEMPLATE_ID: string = "Microsoft_Pos_Extensibility_Samples_AddressAddEditCustomFieldsSection";
    // public toggleSwitch: Controls.IDataListOptions<>;
   
    constructor(id: string, context: IAddressAddEditCustomControlContext) {
        super(id, context);




    }

    /**
     * Binds the control to the specified element.
     * @param {HTMLElement} element The element to which the control should be bound.
     */
    public onReady(element: HTMLElement): void {
        //  this.context.logger.logInformational("--------------------------------------------------------------- " + this.address.City + "   " + this.address.State);
        //$("#city").val(this.address.City);
        if (this.address.State != "" && this.address.State != null && this.address.State != undefined) {
           
            let getstorehours: KKAS.GetAllCity<KKAS.GetResponseValueV1> = null;
            getstorehours = new KKAS.GetAllCity<KKAS.GetResponseValueV1>(this.address.State);
            this.context.runtime.executeAsync(getstorehours).then((result: ClientEntities.ICancelableDataResult<KKAS.GetResponseValueV1>): any => {
                for (var i = 0; i < result.data.result.length; i++) {
                    $('#city').append(new Option(result.data.result[i].City, result.data.result[i].City));

                }
            }).catch(err => {
                console.log(err);
                ErrorHelper.displayMessageAsync(this.context, err);
            });
         
        }

        let rr = this.context;
        let adress = this.address;
        $(document).ready(function () {
            $(document).change(function () {
                $("#cityTabRead").parent("div").hide();
                $("#cityTabRead").hide();
                $("#stateTabRead").parent("div").hide();
                $("#stateTabRead").hide();
            })
            
            
            
            

            $('#city').css({ "margin-bottom": "20px" })
            $('#state').css({ "margin-bottom": "20px" })
            $("#stateTabRead").parent("div").after($("#customDivCity"));
            $("#stateTabRead").parent("div").after($("#customDivState"));
            $("#cityTabRead").parent("div").hide();
            $("#cityTabRead").hide();
            $("#stateTabRead").parent("div").hide();
            $("#stateTabRead").hide();

            if (adress.State != "") {
               // rr.logger.logInformational("--------------------------------------------------------------- " + adress.City + "   " + adress.State);
                $("#city > option").remove();
                $("#state > option").remove();
                $("#city").append(new Option(adress.City, adress.City));
                $("#state").append(new Option(adress.State, adress.State));
               
            }

            let getState: KKAS.GetAllState<KKAS.GetResponseValueV2> = null;
            getState = new KKAS.GetAllState<KKAS.GetResponseValueV2>();
            rr.runtime.executeAsync(getState)
                .then((result: ClientEntities.ICancelableDataResult<KKAS.GetResponseValueV2>): any => {
                    rr.logger.logInformational("rerer " + JSON.stringify(result));
                    //$("#state > option").remove();
                    //$('#state').append('<option value="this.address.State"></option>');

                    for (var i = 0; i < result.data.result.length; i++) {
                        $('#state').append(new Option(result.data.result[i].State, result.data.result[i].State));
                    }


                }).catch(err => {
                    console.log(err);
                    ErrorHelper.displayMessageAsync(rr, err);
                });

            $("#state").on('change', function () {
                $("#city > option").remove();
                $('#city').append('<option value=""></option>');
                $('#city').attr("required", true);

               
                if ($("#state").val() != "") {
                    let getstorehours: KKAS.GetAllCity<KKAS.GetResponseValueV1> = null;
                    getstorehours = new KKAS.GetAllCity<KKAS.GetResponseValueV1>($("#state").val());
                    rr.runtime.executeAsync(getstorehours)
                        .then((result: ClientEntities.ICancelableDataResult<KKAS.GetResponseValueV1>): any => {

                            for (var i = 0; i < result.data.result.length; i++) {
                                $('#city').append(new Option(result.data.result[i].City, result.data.result[i].City));
                            }


                        }).catch(err => {
                            console.log(err);
                            ErrorHelper.displayMessageAsync(rr, err);
                        });
                }
                
                  
                
            });


        });

        ko.applyBindingsToNode(element, {

            template: {
                name: AddressAddEditCustomFieldsSection.TEMPLATE_ID,
                data: this
            }

        });
    }


    public init(state: IAddressAddEditCustomControlState): void {
        this.isVisible = true;


        //$('#city').val(this.address.City)

        //let getstorehours: KKAS.GetAllCity<KKAS.GetResponseValueV1> = null;
        //getstorehours = new KKAS.GetAllCity<KKAS.GetResponseValueV1>(this.address.State);
        //this.context.runtime.executeAsync(getstorehours)
        //    .then((result: ClientEntities.ICancelableDataResult<KKAS.GetResponseValueV1>): any => {

        //        for (var i = 0; i < result.data.result.length; i++) {
        //            $('#city').append(new Option(result.data.result[i].City, result.data.result[i].City));

        //        }




        //    }).catch(err => {
        //        console.log(err);
        //        ErrorHelper.displayMessageAsync(this.context, err);
        //    });

    }






}