/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace Contoso
{
    namespace Commerce.Runtime.ReceiptsSample
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.IO;
        using System.Threading.Tasks;
        using System.Xml;
        using E_Receipt_DLL;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Localization.Services.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using QRCoder;
        using static QRCoder.Base64QRCode;

        /// <summary>
        /// The extended service to get custom receipt field.
        /// </summary>
        /// <remarks>
        /// To print custom receipt fields on a receipt, one must handle <see cref="GetSalesTransactionCustomReceiptFieldServiceRequest"/>
        /// and <see cref="GetCustomReceiptFieldServiceResponse"/>. Here are several points about how to do this.
        /// 1. CommerceRuntime calls this request if and only if it needs to print values for some fields that are not supported by default. So make
        ///    sure your custom receipt fields have different names than existing ones. Adding a prefix in front of custom filed names would
        ///    be a good idea. The value of custom filed name should match the value you defined in AX, on Custom Filed page.
        /// 2. User should handle content-related formatting. This means that if you want to print "$ 10.00" instead of "10" on the receipt, 
        ///    you must generate "$ 10.00" by yourself. You can call <see cref="GetFormattedCurrencyServiceRequest"/> to do this. There are also some
        ///    other requests designed to format other types of values such as numbers and date time. Note, the user DO NOT need to worry about alignment,
        ///    the CommerceRuntime will take care of that.
        /// 3. If any exception happened when getting the value of custom receipt fields, CommerceRuntime will print empty value on the receipt and the
        ///    exceptions will be logged.
        /// 4. So far, only sales-transaction-based custom receipts are supported. This means you can do customization for receipts when checking out
        ///    a normal sales transaction or creating/picking up a customer order.
        /// </remarks>
        public class GetCustomReceiptFieldsService : IRequestHandlerAsync
        {
            /// <summary>
            /// Gets the supported request types.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetSalesTransactionCustomReceiptFieldServiceRequest),
                    };
                }
            }

            /// <summary>
            /// Executes the requests.
            /// </summary>
            /// <param name="request">The request parameter.</param>
            /// <returns>The GetReceiptServiceResponse that contains the formatted receipts.</returns>
            public async Task<Response> Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Type requestedType = request.GetType();

                if (requestedType == typeof(GetSalesTransactionCustomReceiptFieldServiceRequest))
                {
                    return await this.GetCustomReceiptFieldForSalesTransactionReceiptsAsync((GetSalesTransactionCustomReceiptFieldServiceRequest)request).ConfigureAwait(false);
                }

                throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
            }

            /// <summary>
            /// Gets the custom receipt filed for all transaction-based receipts, such as SalesReceipt, CustomerOrderReceipt, PickupReceipt, CreditCardReceipt, and so on.
            /// </summary>
            /// <param name="request">The service request to get custom receipt filed.</param>
            /// <returns>The value of custom receipt filed.</returns>
            private async Task<Response> GetCustomReceiptFieldForSalesTransactionReceiptsAsync(GetSalesTransactionCustomReceiptFieldServiceRequest request)
            {
                string receiptFieldName = request.CustomReceiptField;

                SalesOrder salesOrder = request.SalesOrder;
                SalesLine salesLine = request.SalesLine;
                TenderLine tenderLine = request.TenderLine;
                
                // Get the store currency.
                string currency = request.RequestContext.GetOrgUnit().Currency;
                
                string returnValue = string.Empty;
                switch (receiptFieldName)
                {
                    //case "TIPAMOUNT":
                    //    {
                    //        // FORMAT THE VALUE
                    //        decimal tipAmount = salesOrder == null ? 0 : (salesOrder.TotalAmount * 0.18m);
                    //        returnValue = await this.FormatCurrencyAsync(tipAmount, currency, request.RequestContext).ConfigureAwait(false);
                    //    }

                    //    break;

                    //case "ITEMNUMBER":
                    //    {
                    //        returnValue = salesLine == null ? string.Empty : "Custom_" + salesLine.ItemId;
                    //    }

                    //    break;

                    //case "TENDERID":
                    //    {
                    //        returnValue = tenderLine == null ? string.Empty : "Custom_" + tenderLine.TenderTypeId;
                    //    }

                    //    break;
                    
                    case "TAXINVOICEQR":
                        {
                           
                            string ReceiptURL;



                            if (!string.IsNullOrEmpty(salesOrder.CustomerId))
                            {

                                string custType = await GetCustomerType(request).ConfigureAwait(false);

                                // customer type is PERSON
                                if (custType == "1")
                                {

                                    //--------- customer receipt 
                                    if (request.ReceiptType == ReceiptType.SalesReceipt)
                                    {
                                        ReceiptURL = await ProcessReceipt(salesOrder, request).ConfigureAwait(false);

                                        returnValue =await getQR_codeX(ReceiptURL,request).ConfigureAwait(false);
                                    }
                                    //----- pickup receipt 
                                    else if (request.ReceiptType == ReceiptType.SalesOrderReceipt &&
                                        salesOrder.CustomerOrderMode == CustomerOrderMode.Pickup && salesOrder.Status == SalesStatus.Invoiced)
                                    {
                                        ReceiptURL = await ProcessReceipt(salesOrder, request).ConfigureAwait(false);

                                        returnValue =await getQR_codeX(ReceiptURL,request).ConfigureAwait(false);
                                    }
                                    //---- ----- pickup receipt  return
                                    else if (request.ReceiptType == ReceiptType.SalesOrderReceipt &&
                                      salesOrder.CustomerOrderMode == CustomerOrderMode.CustomerOrderCreateOrEdit
                                      && salesOrder.Status == SalesStatus.Created && salesOrder.NetAmount > 0)
                                    {
                                        ReceiptURL = await ProcessReceipt(salesOrder, request, true).ConfigureAwait(false);

                                        returnValue =await getQR_codeX(ReceiptURL,request).ConfigureAwait(false);
                                    }
                                    //----- pickup receipt 
                                    else if (request.ReceiptType == ReceiptType.PickupReceipt &&
                                    salesOrder.CustomerOrderMode == CustomerOrderMode.Pickup && salesOrder.Status == SalesStatus.Invoiced)
                                    {
                                        ReceiptURL = await ProcessReceipt(salesOrder, request).ConfigureAwait(false);

                                        returnValue =await getQR_codeX(ReceiptURL,request).ConfigureAwait(false);
                                    }


                                }
                            }
                            else
                            {
                                ReceiptURL = await ProcessReceipt(salesOrder, request).ConfigureAwait(false);

                                returnValue =await getQR_codeX(ReceiptURL,request).ConfigureAwait(false);
                            }


                            //  writelog(request.ReceiptType.ToString());
                            //   returnValue = request.ReceiptType.ToString() + "-" + salesOrder.CustomerOrderMode.ToString() + "-" + salesOrder.Status ;

                        }

                        break;
                }

                return new GetCustomReceiptFieldServiceResponse(returnValue);
            }

            /// <summary>
            /// Formats the currency to another currency.
            /// </summary>
            /// <param name="value">The digital value of the currency to be formatted.</param>
            /// <param name="currencyCode">The code of the target currency.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The formatted value of the currency.</returns>
            /// 
            private async Task<string> GetSO_TransNo(GetSalesTransactionCustomReceiptFieldServiceRequest request)
            {
                
                ThrowIf.Null(request, "GetsalesInvoiceReferUUID request");

                try
                {

                    string  returnTransNum = "", refeUUID = "";


                    ThrowIf.Null(request, "getReferenceRceiptUUID request");

                    var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        DatabaseSchema = "AX",
                        Select = new ColumnSet(new string[] { "RETURNTRANSACTIONID" }),
                        From = "RETAILTRANSACTIONSALESTRANS",
                        Where = "TRANSACTIONID = @trnsNum and transactionStatus != 1 and RETURNTRANSACTIONID != ''",
                        OrderBy = "CREATEDDATETIME Desc",
                    };

                    query.Parameters["@trnsNum"] = request.SalesOrder.Id;
                    //query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                    using (var databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        var extensionsResponse = await databaseContext.ReadEntityAsync<ExtensionsEntity>(query).ConfigureAwait(false);
                        ExtensionsEntity extensions = extensionsResponse.FirstOrDefault();

                        var receiptID = extensions != null ? extensions.GetProperty("RETURNTRANSACTIONID") : "";

                        if (receiptID != null)
                        {
                            returnTransNum = receiptID.ToString();
                        }
                        else
                        {
                            returnTransNum = "";
                        }

                    }


                    if (!string.IsNullOrEmpty(returnTransNum))
                    {
                        InvokeMethodRealtimeRequest extensionRequest = new InvokeMethodRealtimeRequest("getSalesInvoiceDetail", returnTransNum);
                        InvokeMethodRealtimeResponse response = await request.RequestContext.ExecuteAsync<InvokeMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);

                        ReadOnlyCollection<object> results = response.Result;
                        string resValue = (string)results[0];
                        // returnValue = salesOrder == null ? string.Empty : resValue;
                        string regis = string.IsNullOrEmpty(resValue) ? "" : resValue;

                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.LoadXml(regis);
                        string salesID = xmlDocument.DocumentElement.GetAttribute("SalesId");


                        var getCustomReceiptsRequest_local = new GetSalesOrderDetailsBySalesIdServiceRequest(salesID, SearchLocation.Local);
                        var getSalesOrderDetailsServiceResponse_local =
                            await request.RequestContext.ExecuteAsync<GetSalesOrderDetailsServiceResponse>(getCustomReceiptsRequest_local).ConfigureAwait(false);

                        refeUUID = string.IsNullOrEmpty(getSalesOrderDetailsServiceResponse_local.SalesOrder.Id) ? "" : getSalesOrderDetailsServiceResponse_local.SalesOrder.Id;

                        if (string.IsNullOrEmpty(refeUUID))
                        {
                            var getCustomReceiptsRequest = new GetSalesOrderDetailsBySalesIdRealtimeRequest(salesID);
                            var getSalesOrderDetailsServiceResponse =
                                await request.RequestContext.ExecuteAsync<GetSalesOrderDetailsRealtimeResponse>(getCustomReceiptsRequest).ConfigureAwait(false);

                            refeUUID = string.IsNullOrEmpty(getSalesOrderDetailsServiceResponse.SalesOrder.Id) ? "" : getSalesOrderDetailsServiceResponse.SalesOrder.Id;

                        }








                        return refeUUID;
                    }


                    return "";


                }
                catch (Exception ex)
                {
                    writelog(ex.Message + "--" + ex.InnerException);
                    return "";
                }

             
               
            }
            private async Task<string> FormatCurrencyAsync(decimal value, string currencyCode, RequestContext context)
            {
                GetRoundedValueServiceRequest roundingRequest = null;

                string currencySymbol = string.Empty;

                // Get the currency symbol.
                if (!string.IsNullOrWhiteSpace(currencyCode))
                {
                    var getCurrenciesDataRequest = new GetCurrenciesDataRequest(currencyCode, QueryResultSettings.SingleRecord);
                    var currencyResponse = await context.Runtime.ExecuteAsync<EntityDataServiceResponse<Currency>>(getCurrenciesDataRequest, context).ConfigureAwait(false);
                    Currency currency = currencyResponse.PagedEntityCollection.FirstOrDefault();
                    currencySymbol = currency.CurrencySymbol;
                }

                roundingRequest = new GetRoundedValueServiceRequest(value, currencyCode, 0, false);

                var roundedValueResponse = await context.ExecuteAsync<GetRoundedValueServiceResponse>(roundingRequest).ConfigureAwait(false);
                decimal roundedValue = roundedValueResponse.RoundedValue;

                var formattingRequest = new GetFormattedCurrencyServiceRequest(roundedValue, currencySymbol);
                var formattedValueResponse = await context.ExecuteAsync<GetFormattedContentServiceResponse>(formattingRequest).ConfigureAwait(false);
                string formattedValue = formattedValueResponse.FormattedValue;
                return formattedValue;
            }

            private async Task<string> GetCustomerType(GetSalesTransactionCustomReceiptFieldServiceRequest request)
            {
                string CustomerType = "";
                //CustAccount = "FW00100";


                ThrowIf.Null(request, "GetCustomerType request");

                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "crt",
                    Select = new ColumnSet(new string[] { "CUSTOMERTYPE" }),
                    From = "CUSTOMERSVIEW where ACCOUNTNUMBER = '" + request.SalesOrder.CustomerId + "'"

                };

                //query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                using (var databaseContext = new DatabaseContext(request.RequestContext))
                {
                    var extensionsResponse = await databaseContext.ReadEntityAsync<ExtensionsEntity>(query).ConfigureAwait(false);
                    ExtensionsEntity extensions = extensionsResponse.FirstOrDefault();

                    var receiptID = extensions != null ? extensions.GetProperty("CUSTOMERTYPE") : "";

                    if (receiptID != null)
                    {
                        CustomerType = receiptID.ToString();
                    }
                    else
                    {
                        //  writelog("ItemID: " + itemid + " BrickCode Not Found");
                        return "";
                    }

                }

                // }




                return CustomerType;
            }

            [Obsolete]
            private async Task<string> ProcessReceipt(SalesOrder salesOrder, GetSalesTransactionCustomReceiptFieldServiceRequest request, Boolean isReturn = false)
            {
                try
                {
                   
                    Receipt_CLS _receipt = new Receipt_CLS();
                    decimal TotalAmount = 0, TaxTotal = 0, TotalDisc = 0, TotalSalesAmount = 0, NetTotal = 0;
                    string PreviousUUID = "", ReferenceUUID = "", UUID = "", TrnsDateTime = "", ReceiptType = "";
                    var itemdata = new List<itemData> { };
                    // var extraReceiptDiscount = new List<decimal>() { };
                    var extraReceiptDiscountData = new List<discount>() { };
                    //  TrnsDateTime = _receipt.GetDateFormat(request.SalesOrder.TransactionDateTime.DateTime.AddHours(-2).AddMinutes(-10));
                  //  TrnsDateTime = _receipt.GetDateFormat(request.SalesOrder.TransactionDateTime.DateTime.AddHours(-3).AddMinutes(-10));
                    TrnsDateTime = _receipt.GetDateFormat(request.SalesOrder.CreatedDateTime.DateTime.AddHours(-3).AddMinutes(-10));
              
                    Boolean OrderHasCustomer = false;
                 
                    string ReceiptURL = "";
                 
                    foreach (SalesLine item in salesOrder.SalesLines)
                    {



                        if (!item.IsVoided)
                        {
                            
                            decimal parmUnitValue = 0, parmLineTax = 0, parmSalesTotal = 0,
                                parmNetTotal = 0, parmTotal = 0, parmItemDisc = 0, qty = 0;


                            parmUnitValue = Math.Round(Math.Abs(item.TaxAmount) > 0 ? Math.Abs(item.Price) / (1 + (item.TaxRatePercent / 100)) : Math.Abs(item.Price), 5);

                            qty = Convert.ToDecimal(String.Format("{0:0.00000}", Math.Abs(item.Quantity)));
                            parmItemDisc = Math.Round(Math.Abs(item.TaxAmount) > 0 ? Math.Abs(item.DiscountAmount) / (1 + (item.TaxRatePercent / 100)) : Math.Abs(item.DiscountAmount), 5);
                            parmSalesTotal = Math.Round((parmUnitValue * qty), 5);
                            parmNetTotal = Math.Round((parmSalesTotal - parmItemDisc), 5);
                            //, MidpointRounding.ToEven
                            parmLineTax = Math.Round((parmNetTotal * (item.TaxRatePercent / 100)), 5, MidpointRounding.ToEven);
                            parmTotal = Math.Round((parmNetTotal + parmLineTax), 5);

                            //var commercialDiscount = new List<decimal>() { Convert.ToDecimal(String.Format("{0:0.00000}", Line_Disc)) };
                            //var itemDiscount = new List<decimal>() { };

                            var commercialDiscountData = new List<discount>() { };
                            var itemDiscountData = new List<discount>() { };


                            TotalAmount += parmTotal;
                            TaxTotal += parmLineTax;
                            TotalDisc += parmItemDisc;
                            TotalSalesAmount += parmSalesTotal;
                            NetTotal += parmNetTotal;
                           
                            itemdata.Add(new itemData
                            {
                                internalCode = item.ItemId,
                                description = await GetItemDescription(request, item.ItemId).ConfigureAwait(false),
                                itemType = "GS1",
                                itemCode = string.IsNullOrEmpty(await GetBrickCode(request, item.ItemId).ConfigureAwait(false)) ? "99999999" : await GetBrickCode(request, item.ItemId).ConfigureAwait(false),
                               // itemCode = "99999999",
                                unitType = "EA",
                                quantity = qty,
                                unitPrice = parmUnitValue,
                                netSale = parmNetTotal,
                                totalSale = parmSalesTotal,
                                total = parmTotal,
                                commercialDiscountData = new List<discount>
                                          {
                                           new discount
                                           {
                                                                amount = parmItemDisc,
                                                                description = "Discount"
                                            }
                                          },
                                itemDiscountData = itemDiscountData,
                                valueDifference = Convert.ToDecimal(String.Format("{0:0.00000}", 0)),
                                taxableItems = new List<taxableItems>
                                          {
                                           new taxableItems
                                           {
                                                                taxType = "T1",
                                                                subType = "V010",
                                                                rate = Convert.ToDecimal(String.Format("{0:0.00}", item.TaxRatePercent)),
                                                                amount =  parmLineTax
                                            }
                                          }
                            });
                          
                        }


                    }
                 


                    PreviousUUID = await getPreviousRceiptUUID(request).ConfigureAwait(false);

                    // is pick up order return
                    if (request.SalesOrder.NetAmount > 0 && isReturn == true)
                    {
                        string SO_tranID = await GetSO_TransNo(request).ConfigureAwait(false);
                        ReferenceUUID = await getReferenceRceiptUUIDV1(request, SO_tranID).ConfigureAwait(false);
                    }
                    else
                    {
                        ReferenceUUID = request.SalesOrder.NetAmount > 0 ? await getReferenceRceiptUUID(request).ConfigureAwait(false) : "";
                    }
                    
                    ReceiptType = request.SalesOrder.NetAmount > 0 ? "Return" : "Sales";
                    
                    dynamic header = _receipt.GetHeader(TrnsDateTime, request.SalesOrder.ReceiptId, "", PreviousUUID, "", ReferenceUUID,
                         "EGP", "", "", Convert.ToDecimal(String.Format("{0:0.0}", 0)), Convert.ToDecimal(String.Format("{0:0.0}", 0)), Convert.ToDecimal(String.Format("{0:0.0}", 0)));
                    

                    var documentType = new documentType
                    {
                        receiptType = request.SalesOrder.NetAmount > 0 ? "r" : "s",
                        typeVersion = "1.2"
                    };
                   
                    OrderHasCustomer = string.IsNullOrEmpty(salesOrder.CustomerId) ? false : true;
                    string customerRegID = OrderHasCustomer ? await GetCustomerRegID_DB(request).ConfigureAwait(false) : "";
                    string CustomerTypeETA =  "P" ;
                  
                    var buyer = new buyer
                    {
                        type = CustomerTypeETA,
                        id = customerRegID,
                        name = string.IsNullOrEmpty(customerRegID) ? "" : await GetCustomerName_DB(request).ConfigureAwait(false),
                        mobileNumber = "",
                        paymentNumber = ""
                    };
                  


                    var receipt = new receipt
                    {
                        header = header,
                        documentType = documentType,
                        seller = _receipt.GetCompanyInfo(),
                        buyer = buyer,
                        itemData = itemdata,
                        totalSales = TotalSalesAmount,
                        totalCommercialDiscount = Convert.ToDecimal(String.Format("{0:0.00000}", TotalDisc)),
                        totalItemsDiscount = Convert.ToDecimal(String.Format("{0:0.00000}", 0)),
                        extraReceiptDiscountData = extraReceiptDiscountData,
                        netAmount = NetTotal,
                        feesAmount = Convert.ToDecimal(String.Format("{0:0.00000}", 0)),
                        totalAmount = TotalAmount,
                        taxTotals = new List<taxTotals>
                                {
                                    new taxTotals
                                    {
                                        taxType = "T1",
                                        amount =  TaxTotal
                                    }
                                },
                        paymentMethod = "C",
                        adjustment = Convert.ToDecimal(String.Format("{0:0.00000}", 0)),
                        contractor = _receipt.GetContractor(),
                        beneficiary = _receipt.GetBeneficiary()
                    };
                    
                    UUID = _receipt.GenerateUUID(receipt);
             

                    ReceiptURL = await GetPortalURL(request).ConfigureAwait(false) + "/receipts/search/" + UUID + "/share/" +
                      TrnsDateTime + "#Total:" + String.Format("{0:0.00000}", TotalAmount) +
                      ",IssuerRIN:" + _receipt.GetCompanyRIN();

                    dynamic header1 = _receipt.GetHeader(TrnsDateTime, request.SalesOrder.ReceiptId, UUID, PreviousUUID, "", ReferenceUUID,
                    "EGP", "", "", Convert.ToDecimal(String.Format("{0:0.0}", 0)), Convert.ToDecimal(String.Format("{0:0.0}", 0)), Convert.ToDecimal(String.Format("{0:0.0}", 0)));
                 
                    var receipt1 = new receipt
                    {
                        header = header1,
                        documentType = documentType,
                        seller = _receipt.GetCompanyInfo(),
                        buyer = buyer,
                        itemData = itemdata,
                        totalSales = TotalSalesAmount,
                        totalCommercialDiscount = Convert.ToDecimal(String.Format("{0:0.00000}", TotalDisc)),
                        totalItemsDiscount = Convert.ToDecimal(String.Format("{0:0.00000}", 0)),
                        extraReceiptDiscountData = extraReceiptDiscountData,
                        netAmount = NetTotal,
                        feesAmount = Convert.ToDecimal(String.Format("{0:0.00000}", 0)),
                        totalAmount = TotalAmount,
                        taxTotals = new List<taxTotals>
                                {
                                    new taxTotals
                                    {
                                        taxType = "T1",
                                        amount =  TaxTotal
                                    }
                                },
                        paymentMethod = "C",
                        adjustment = Convert.ToDecimal(String.Format("{0:0.00000}", 0)),
                        contractor = _receipt.GetContractor(),
                        beneficiary = _receipt.GetBeneficiary()
                    };
                  

                    using (var databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        ParameterSet parameters = new ParameterSet();
                        parameters["@nvc_UUID"] = UUID;
                        parameters["@nvc_PREVIOUSUUID"] = PreviousUUID;
                        parameters["@nvc_REFERENCEUUID"] = ReferenceUUID;
                        parameters["@nvc_DEVSERIALNUMBER"] = _receipt.GetSerialNumber();
                        parameters["@nvc_Receipt_URL"] = ReceiptURL;
                        parameters["@nvc_ReceiptIsResend"] = 0;
                        parameters["@nvc_Receipt_Status"] = 0;
                        parameters["@nvc_Receipt_DateTime"] = TrnsDateTime;
                        parameters["@nvc_Json_Receipt"] = _receipt.JsonSerialize(receipt1);
                        parameters["@nvc_TransactionId"] = request.SalesOrder.Id;
                        await databaseContext.ExecuteStoredProcedureNonQueryAsync("[ext].E_Receipt_Update", parameters, resultSettings: null).ConfigureAwait(false);
                    }

                    //  writelog(_receipt.Serialize(_receipt.JsonSerialize(receipt)));
                   
                    writelog(_receipt.JsonSerialize(receipt1));
                    // writelog(salesOrder.Comment);

                    //    writelog(_receipt.GenerateUUID(receipt));

                    //Insert into missing Receipt TB 
                    using (var databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        ParameterSet parameters = new ParameterSet();
                        parameters["@nvc_UUID"] = UUID;
                        parameters["@nvc_PREVIOUSUUID"] = PreviousUUID;
                        parameters["@nvc_REFERENCEUUID"] = ReferenceUUID;
                        parameters["@nvc_DEVSERIALNUMBER"] = _receipt.GetSerialNumber();
                        parameters["@nvc_Receipt_URL"] = ReceiptURL;
                        parameters["@nvc_TransactionId"] = request.SalesOrder.Id;
                        parameters["@nvc_ReceiptId"] = request.SalesOrder.ReceiptId;
                        parameters["@nvc_Store"] = request.SalesOrder.StoreId;
                        parameters["@nvc_Json_Receipt"] = _receipt.JsonSerialize(receipt1);
                        parameters["@nvc_Receipt_DateTime"] = TrnsDateTime;
                        parameters["@nvc_DataAreaId"] = request.SalesOrder.DataAreaId;
                        await databaseContext.ExecuteStoredProcedureNonQueryAsync("[ext].RR_RECEIPTS_BK_ST", parameters, resultSettings: null).ConfigureAwait(false);
                      
                    }
                   

                    return ReceiptURL;
                }
                catch (Exception xx)
                {

                    writelog(xx.Message + "-----" + xx.InnerException);
                    return "";
                }


            }

            private async Task<string> GetBrickCode(GetSalesTransactionCustomReceiptFieldServiceRequest request, string itemid, string variant = null)
            {
                string brickCode = "";
                //CustAccount = "FW00100";


                ThrowIf.Null(request, "GetBrickCode request");

                //if (string.IsNullOrEmpty(brickCode))
                //{
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "AX",
                    Select = new ColumnSet(new string[] { "R_BRICKCODE" }),
                    From = "INVENTTABLE where ITEMID  = '" + itemid + "'"

                };

                //    query.Parameters["@trnsNum"] = request.SalesOrder.Id;  '" + itemid + "'
                //query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                using (var databaseContext = new DatabaseContext(request.RequestContext))
                {
                    var extensionsResponse = await databaseContext.ReadEntityAsync<ExtensionsEntity>(query).ConfigureAwait(false);
                    ExtensionsEntity extensions = extensionsResponse.FirstOrDefault();

                    var receiptID = extensions != null ? extensions.GetProperty("R_BRICKCODE") : "";

                    if (receiptID != null)
                    {
                        brickCode = receiptID.ToString();
                    }
                    else
                    {
                        //  writelog("ItemID: " + itemid + " BrickCode Not Found");
                        return "99999999";
                    }

                }

                // }




                return brickCode;

            }

            private async Task<string> GetBrickCode_old1(GetSalesTransactionCustomReceiptFieldServiceRequest request, string itemid, string variant = null)
            {
                string brickCode = "";
                //CustAccount = "FW00100";


                ThrowIf.Null(request, "GetBrickCode request");

                //if (string.IsNullOrEmpty(brickCode))
                //{
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "AX",
                    Select = new ColumnSet(new string[] { "FRIENDLYNAME" }),
                    From = "ECORESCATEGORYTRANSLATION where ax.ECORESCATEGORYTRANSLATION.CATEGORY = (select top 1 CATEGORY from ax.ECORESPRODUCTCATEGORY where CategoryHierarchy=(select top 1 brickcode from ext.E_Invoice_TB) and PRODUCT = (select top 1 PRODUCT from ax.INVENTTABLE where ITEMID  = '" + itemid + "'))"

                };

                //    query.Parameters["@trnsNum"] = request.SalesOrder.Id;  '" + itemid + "'
                //query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                using (var databaseContext = new DatabaseContext(request.RequestContext))
                {
                    var extensionsResponse = await databaseContext.ReadEntityAsync<ExtensionsEntity>(query).ConfigureAwait(false);
                    ExtensionsEntity extensions = extensionsResponse.FirstOrDefault();

                    var receiptID = extensions != null ? extensions.GetProperty("FRIENDLYNAME") : "";

                    if (receiptID != null)
                    {
                        brickCode = receiptID.ToString();
                    }
                    else
                    {
                        //  writelog("ItemID: " + itemid + " BrickCode Not Found");
                        return "99999999";
                    }

                }

                // }




                return brickCode;

            }

            private async Task<string> GetBrickCode_old(GetSalesTransactionCustomReceiptFieldServiceRequest request, string itemid, string variant = null)
            {
                string brickCode = "";
                //CustAccount = "FW00100";


                ThrowIf.Null(request, "GetBrickCode request");



                //if (string.IsNullOrEmpty(brickCode))
                //{
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "AX",
                    Select = new ColumnSet(new string[] { "NAME" }),
                    From = "ECORESPRODUCTCATEGORY join ax.INVENTTABLE on ax.ECORESPRODUCTCATEGORY.PRODUCT = (select top 1 PRODUCT from ax.INVENTTABLE where ITEMID  = '" + itemid + "')  join ax.ecoresCategory on ax.ecoresCategory.RECID = ax.ECORESPRODUCTCATEGORY.CATEGORY where ax.ecoresCategory.PARENTCATEGORY = (select top 1 BrickCode from ext.E_INVOICE_TB )"

                };

                //    query.Parameters["@trnsNum"] = request.SalesOrder.Id;
                //query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                using (var databaseContext = new DatabaseContext(request.RequestContext))
                {
                    var extensionsResponse = await databaseContext.ReadEntityAsync<ExtensionsEntity>(query).ConfigureAwait(false);
                    ExtensionsEntity extensions = extensionsResponse.FirstOrDefault();

                    var receiptID = extensions != null ? extensions.GetProperty("NAME") : "";

                    if (receiptID != null)
                    {
                        brickCode = receiptID.ToString();
                    }
                    else
                    {
                        //  writelog("ItemID: " + itemid + " BrickCode Not Found");
                        return "99999999";
                    }

                }

                // }




                return brickCode;

            }

            private async Task<string> GetItemDescription(GetSalesTransactionCustomReceiptFieldServiceRequest request, string itemid)
            {
                ThrowIf.Null(request, "GetItemDescription request");

                string description = "";

                var query1 = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "AX",
                    Select = new ColumnSet(new string[] { "Name" }),
                    From = "ECORESPRODUCTTRANSLATION where ax.ECORESPRODUCTTRANSLATION.Product = (select top 1 PRODUCT from ax.INVENTTABLE where ITEMID  = '" + itemid + "') and ax.ECORESPRODUCTTRANSLATION.LanguageId = 'en-us'"

                };


                using (var databaseContext = new DatabaseContext(request.RequestContext))
                {
                    var extensionsResponse = await databaseContext.ReadEntityAsync<ExtensionsEntity>(query1).ConfigureAwait(false);
                    ExtensionsEntity extensions = extensionsResponse.FirstOrDefault();

                    var receiptID = extensions != null ? extensions.GetProperty("Name") : "";

                    if (receiptID != null)
                    {
                        description = receiptID.ToString();
                    }
                    else
                    {
                        description = "";
                    }

                }




                return description;
            }
            private async Task<string> GetCustomerName_DB(GetSalesTransactionCustomReceiptFieldServiceRequest request)
            {
                string CustomerName = "";
                //CustAccount = "FW00100";


                ThrowIf.Null(request, "GetCustomerName request");

                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "crt",
                    Select = new ColumnSet(new string[] { "NAME" }),
                    From = "CUSTOMERSVIEW where ACCOUNTNUMBER = '" + request.SalesOrder.CustomerId + "'"

                };

                //query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                using (var databaseContext = new DatabaseContext(request.RequestContext))
                {
                    var extensionsResponse = await databaseContext.ReadEntityAsync<ExtensionsEntity>(query).ConfigureAwait(false);
                    ExtensionsEntity extensions = extensionsResponse.FirstOrDefault();

                    var receiptID = extensions != null ? extensions.GetProperty("NAME") : "";

                    if (receiptID != null)
                    {
                        CustomerName = receiptID.ToString();
                    }
                    else
                    {
                        //  writelog("ItemID: " + itemid + " BrickCode Not Found");
                        return "";
                    }

                }

                // }




                return CustomerName;
            }
            private async Task<string> GetCustomerRegID_DB(GetSalesTransactionCustomReceiptFieldServiceRequest request)
            {
                string CustomerRegID = "";
                //CustAccount = "FW00100";


                ThrowIf.Null(request, "CustomerRegID request");

                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "AX",
                    Select = new ColumnSet(new string[] { "VATNUM" }),
                    From = "CUSTTABLE where ACCOUNTNUM = '" + request.SalesOrder.CustomerId + "'"

                };
                //'" + itemid + "'
                //    query.Parameters["@trnsNum"] = request.SalesOrder.Id;
                //query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                using (var databaseContext = new DatabaseContext(request.RequestContext))
                {
                    var extensionsResponse = await databaseContext.ReadEntityAsync<ExtensionsEntity>(query).ConfigureAwait(false);
                    ExtensionsEntity extensions = extensionsResponse.FirstOrDefault();

                    var receiptID = extensions != null ? extensions.GetProperty("VATNUM") : "";

                    if (receiptID != null)
                    {
                        CustomerRegID = receiptID.ToString();
                    }
                    else
                    {
                        //  writelog("ItemID: " + itemid + " BrickCode Not Found");
                        return "";
                    }

                }

                // }




                return CustomerRegID;
            }
            private async Task<string> getReferenceRceiptUUID(GetSalesTransactionCustomReceiptFieldServiceRequest request)
            {
                string ReferenceRceiptUUID = "", returnTransNum = "";


                ThrowIf.Null(request, "getReferenceRceiptUUID request");

                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "AX",
                    Select = new ColumnSet(new string[] { "RETURNTRANSACTIONID" }),
                    From = "RETAILTRANSACTIONSALESTRANS",
                    Where = "TRANSACTIONID = @trnsNum and transactionStatus != 1 and RETURNTRANSACTIONID != ''",
                    OrderBy = "CREATEDDATETIME Desc",
                };

                query.Parameters["@trnsNum"] = request.SalesOrder.Id;
                //query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                using (var databaseContext = new DatabaseContext(request.RequestContext))
                {
                    var extensionsResponse = await databaseContext.ReadEntityAsync<ExtensionsEntity>(query).ConfigureAwait(false);
                    ExtensionsEntity extensions = extensionsResponse.FirstOrDefault();

                    var receiptID = extensions != null ? extensions.GetProperty("RETURNTRANSACTIONID") : "";

                    if (receiptID != null)
                    {
                        returnTransNum = receiptID.ToString();
                    }
                    else
                    {
                        returnTransNum = "";
                    }

                }

            
            

                if (!string.IsNullOrEmpty(returnTransNum))
                {
                    var query_UUID = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        DatabaseSchema = "AX",
                        Select = new ColumnSet(new string[] { "UUID", "ReceiptId" }),
                        From = "RETAILTRANSACTIONTABLE",
                        Where = "TRANSACTIONID = @trnsNum "
                    };

                    query_UUID.Parameters["@trnsNum"] = returnTransNum;
                    //query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                    using (var databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        var extensionsResponse = await databaseContext.ReadEntityAsync<ExtensionsEntity>(query_UUID).ConfigureAwait(false);
                        ExtensionsEntity extensions = extensionsResponse.FirstOrDefault();

                        var receiptID = extensions != null ? extensions.GetProperty("ReceiptId") : "";
                        var receiptUUID = extensions != null ? extensions.GetProperty("UUID") : "";
                        if (receiptUUID != null)
                        {
                            ReferenceRceiptUUID = receiptUUID.ToString();
                        }
                        else
                        {
                            return "";
                        }

                    }
                }

                if (string.IsNullOrEmpty(ReferenceRceiptUUID))
                {

                    InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest("GetTrnReferenceUUID", returnTransNum);
                    InvokeExtensionMethodRealtimeResponse response = await request.RequestContext.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);
                    ReadOnlyCollection<object> results = response.Result;
                    string resValue = (string)results[0];
                    // returnValue = salesOrder == null ? string.Empty : resValue;
                    string regis = string.IsNullOrEmpty(resValue) ? "" : resValue;

                    if (regis != null)
                    {
                        ReferenceRceiptUUID = regis.ToString();
                    }
                    else
                    {
                        return "";
                    }

                }

                //if (string.IsNullOrEmpty(ReferenceRceiptUUID))
                //{

                //    //  GetOrderInvoiceRealtimeRequest getOrder = new GetOrderInvoiceRealtimeRequest(request.SalesOrder.CustomerId, returnTransNum);
                //    //   GetInvoiceRealtimeRequest requestInv = new GetInvoiceRealtimeRequest(request.SalesOrder.CustomerId, returnTransNum);

                //    //var getSalesOrderDetailsServiceResponse = await request.RequestContext.ExecuteAsync<GetSalesOrderDetailsServiceResponse>(getOrder).ConfigureAwait(false);
                //    //  var getSalesOrderDetailsServiceResponse = await request.RequestContext.ExecuteAsync<GetInvoiceRealtimeResponse>(requestInv).ConfigureAwait(false);


                //    ///  var getSalesOrderDetailsServiceResponse1 = await request.RequestContext.ExecuteAsync<GetSalesOrderDetailsServiceResponse>(requestInv).ConfigureAwait(false);

                //    var getCustomReceiptsRequest = new GetSalesOrderDetailsByTransactionIdServiceRequest(request.SalesOrder.Id, SearchLocation.Local);
                //    var getSalesOrderDetailsServiceResponse = await request.RequestContext.ExecuteAsync<GetSalesOrderDetailsServiceResponse>(getCustomReceiptsRequest).ConfigureAwait(false);

                //    if (getSalesOrderDetailsServiceResponse == null ||
                //getSalesOrderDetailsServiceResponse.SalesOrder == null)
                //    {
                //        throw new DataValidationException(
                //            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound,
                //            string.Format("Unable to get the sales order created. ID: {0}", request.SalesOrder.Id));
                //    }

                //    SalesOrder salesOrder = getSalesOrderDetailsServiceResponse.SalesOrder;

                //    ReferenceRceiptUUID = salesOrder.Id;
                //    //if (getSalesOrderDetailsServiceResponse.SalesOrder.SalesId != null)
                //    //{
                //    //    ReferenceRceiptUUID = getSalesOrderDetailsServiceResponse.SalesOrder.SalesId;
                //    //}
                //    //else
                //    //{
                //    //    return "";
                //    //}

                //}


                return ReferenceRceiptUUID;
            }
            private async Task<string> getReferenceRceiptUUIDV1(GetSalesTransactionCustomReceiptFieldServiceRequest request,string SO_TRN)
            {
                string ReferenceRceiptUUID = "";


                ThrowIf.Null(request, "getReferenceRceiptUUIDV1 request");

               

                if (!string.IsNullOrEmpty(SO_TRN))
                {
                    var query_UUID = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        DatabaseSchema = "AX",
                        Select = new ColumnSet(new string[] { "UUID", "ReceiptId" }),
                        From = "RETAILTRANSACTIONTABLE",
                        Where = "TRANSACTIONID = @trnsNum "
                    };

                    query_UUID.Parameters["@trnsNum"] = SO_TRN;
                    //query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                    using (var databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        var extensionsResponse = await databaseContext.ReadEntityAsync<ExtensionsEntity>(query_UUID).ConfigureAwait(false);
                        ExtensionsEntity extensions = extensionsResponse.FirstOrDefault();

                        var receiptID = extensions != null ? extensions.GetProperty("ReceiptId") : "";
                        var receiptUUID = extensions != null ? extensions.GetProperty("UUID") : "";
                        if (receiptUUID != null)
                        {
                            ReferenceRceiptUUID = receiptUUID.ToString();
                        }
                        else
                        {
                            return "";
                        }

                    }
                }

                if (string.IsNullOrEmpty(ReferenceRceiptUUID))
                {

                    InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest("GetTrnReferenceUUID", SO_TRN);
                    InvokeExtensionMethodRealtimeResponse response = await request.RequestContext.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);
                    ReadOnlyCollection<object> results = response.Result;
                    string resValue = (string)results[0];
                    // returnValue = salesOrder == null ? string.Empty : resValue;
                    string regis = string.IsNullOrEmpty(resValue) ? "" : resValue;

                    if (regis != null)
                    {
                        ReferenceRceiptUUID = regis.ToString();
                    }
                    else
                    {
                        return "";
                    }

                }

              


                return ReferenceRceiptUUID;
            }
            private async Task<string> getPreviousRceiptUUID(GetSalesTransactionCustomReceiptFieldServiceRequest request)
            {


                string PreviousRceiptUUID = "";


                ThrowIf.Null(request, "getPreviousRceiptUUID request");

                if (request.IsCopy)
                {
                    var query1 = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        DatabaseSchema = "AX",
                        Select = new ColumnSet(new string[] { "PreviousUUID", "ReceiptId" }),
                        From = "RETAILTRANSACTIONTABLE",
                        Where = "TRANSACTIONID = @trnsNum and  (Type = 2 or Type = 19)  and ENTRYSTATUS != 1 and (UUID != '' and UUID is not null)"
                    };

                    query1.Parameters["@trnsNum"] = request.SalesOrder.Id;
                    //query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                    using (var databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        var extensionsResponse = await databaseContext.ReadEntityAsync<ExtensionsEntity>(query1).ConfigureAwait(false);
                        ExtensionsEntity extensions = extensionsResponse.FirstOrDefault();

                        //  var receiptID = extensions != null ? extensions.GetProperty("ReceiptId") : "0";
                        var receiptUUID = extensions != null ? extensions.GetProperty("PreviousUUID") : "";

                        //customer.SetProperty("EMAILOPTIN", emailOptIn);

                        //  PreviousRceiptID = receiptID.ToString();
                        if (receiptUUID != null)
                        {
                            PreviousRceiptUUID = receiptUUID.ToString();
                        }
                        else
                        {
                            return "";
                        }

                    }
                }
                else
                {
                    var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        DatabaseSchema = "AX",
                        Select = new ColumnSet(new string[] { "UUID", "ReceiptId" }),
                        From = "RETAILTRANSACTIONTABLE",
                        Where = "TRANSACTIONID != @trnsNum and (Type = 2 or Type = 19)  and ENTRYSTATUS != 1 and (UUID != '' and UUID is not null)",
                        OrderBy = "CREATEDDATETIME Desc"
                    };

                    query.Parameters["@trnsNum"] = request.SalesOrder.Id;
                    //query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                    using (var databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        var extensionsResponse = await databaseContext.ReadEntityAsync<ExtensionsEntity>(query).ConfigureAwait(false);
                        ExtensionsEntity extensions = extensionsResponse.FirstOrDefault();

                        //  var receiptID = extensions != null ? extensions.GetProperty("ReceiptId") : "0";
                        var receiptUUID = extensions != null ? extensions.GetProperty("UUID") : "";

                        //customer.SetProperty("EMAILOPTIN", emailOptIn);

                        //  PreviousRceiptID = receiptID.ToString();
                        if (receiptUUID != null)
                        {
                            PreviousRceiptUUID = receiptUUID.ToString();
                        }
                        else
                        {
                            return "";
                        }

                    }
                }





                return PreviousRceiptUUID;
            }

            private async Task<string> GetPortalURL(GetSalesTransactionCustomReceiptFieldServiceRequest request)
            {
                string portalURL = "";
                ThrowIf.Null(request, "GetPortalURL request");

                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "ext",
                    Select = new ColumnSet(new string[] { "PORTALURL" }),
                    From = "E_INVOICE_TB"
                };

                //  query.Parameters["@trnsNum"] = request.SalesOrder.Id;
                //query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                using (var databaseContext = new DatabaseContext(request.RequestContext))
                {
                    var extensionsResponse = await databaseContext.ReadEntityAsync<ExtensionsEntity>(query).ConfigureAwait(false);
                    ExtensionsEntity extensions = extensionsResponse.FirstOrDefault();

                    //  var receiptID = extensions != null ? extensions.GetProperty("ReceiptId") : "0";
                    var receiptUUID = extensions != null ? extensions.GetProperty("PORTALURL") : "";

                    //customer.SetProperty("EMAILOPTIN", emailOptIn);

                    //  PreviousRceiptID = receiptID.ToString();
                    if (receiptUUID != null)
                    {
                        portalURL = receiptUUID.ToString();
                    }
                    else
                    {
                        return "";
                    }

                }

                return portalURL;
            }

            public async Task<string> getQR_codeX(string qrText, Request request)
            {

                if (string.IsNullOrEmpty(qrText))
                    return string.Empty;


                var qrCodeRequest = new EncodeQrCodeServiceRequest(qrText)
                {
                    Width = 200,
                    Height = 200
                };

                EncodeQrCodeServiceResponse qrCodeDataResponse = await request.RequestContext.ExecuteAsync<EncodeQrCodeServiceResponse>(qrCodeRequest).ConfigureAwait(false);

                return $"<I:{qrCodeDataResponse.QRCode}>";

            }
           

            public static void writelog(string message)

            {

                try
                {
                    string folderPath = @"C:\E_Receipt_Log";


                    //if (Directory.Exists(folderPath))

                    //{

                    //}

                    //The below code will create a folder if the folder is not exists in C#.Net.           

                    DirectoryInfo folder = Directory.CreateDirectory(folderPath);


                    string filename = DateTime.Now.ToString("yyyyMMdd");

                    //  string path = folderPath + @"\" + "Log" + ".txt";

                    string path = folderPath + @"\" + filename + ".txt";

                    FileInfo fi = new FileInfo(path);

                    if (fi.Exists)
                    {
                        using (StreamWriter sw = fi.AppendText())

                        {

                            sw.WriteLine("At:" + DateTime.Now.ToString() + "||" + message);
                            sw.WriteLine("-------------------------------------");

                        }
                    }
                    else
                    {
                        using (StreamWriter sw = fi.CreateText())

                        {

                            sw.WriteLine("At:" + DateTime.Now.ToString() + "||" + message);
                            sw.WriteLine("-------------------------------------");

                        }
                    }

                }
                catch (Exception)
                {

                }






            }
        }
    }
}
