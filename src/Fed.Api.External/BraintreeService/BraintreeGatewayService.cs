using Braintree;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Core.Models;
using Fed.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Api.External.BraintreeService
{
    public class BraintreeGatewayService : IPaymentGatewayService
    {
        private readonly BraintreeGateway _gateway;
        private readonly IPaymentsHandler _paymentsHandler;
        private readonly ILogger _logger;
        private readonly string _merchantAccountId;

        public BraintreeGatewayService(
            string environment,
            string merchantId,
            string merchantAccountId,
            string publicKey,
            string privateKey,
            IPaymentsHandler paymentsHandler,
            ILogger logger)
        {
            environment = environment ?? throw new ArgumentNullException(nameof(environment));
            merchantId = merchantId ?? throw new ArgumentNullException(nameof(merchantId));
            _merchantAccountId = merchantAccountId ?? throw new ArgumentNullException(nameof(merchantAccountId));
            publicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
            privateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
            _paymentsHandler = paymentsHandler;
            _logger = logger;

            _gateway = new BraintreeGateway
            {
                Environment = Braintree.Environment.ParseEnvironment(environment),
                MerchantId = merchantId,
                PublicKey = publicKey,
                PrivateKey = privateKey
            };
        }

        public async Task<string> GetClientToken()
        {
            var clientToken = await _gateway.ClientToken.GenerateAsync();
            return clientToken;
        }

        public async Task<CardTransaction> ProcessPayment(PaymentRequest payment)
        {
            CardTransaction cardTransaction = await CreateCardTransaction(payment);
            var payResult = payment.Amount > 0 ? await Pay(payment, cardTransaction) :
                                                 await Refund(payment, cardTransaction);
            return cardTransaction;
        }

        private async Task<Result<Transaction>> Pay(PaymentRequest payment, CardTransaction cardTransaction)
        {

            Result<Transaction> payResult = null;
            try
            {
                _logger.LogInformation($"Attempting payment £{payment.Amount} for card transaction {cardTransaction.Id}");
                payResult = Sale(payment, cardTransaction.Id);
            }
            catch (Exception ex)
            {
                cardTransaction.ErrorMessage = ex.Message;
                _logger.LogError($"Exception occurred for card transaction {cardTransaction.Id}.  Exception was {ex.Message}");
                await _paymentsHandler.ExecuteAsync(new UpdateCommand<CardTransaction>(cardTransaction.Id, cardTransaction));
            }

            if (payResult.IsSuccess())
            {
                await UpdateSuccess(cardTransaction, payResult.Target.Amount, payResult.Target.ProcessorResponseCode, payResult.Target.ProcessorResponseText, CardTransactionStatus.Paid);
            }
            else
            {
                if (payResult.Errors?.DeepAll().Count > 0)
                {
                    ValidationError error = payResult.Errors.DeepAll().First();
                    await UpdateFailure(cardTransaction, error.Message, null, null);
                }
                else if (payResult.Transaction != null)
                {
                    await UpdateFailure(cardTransaction, payResult.Transaction.Status.ToString(), payResult.Transaction.ProcessorResponseCode, payResult.Transaction.ProcessorResponseText);
                }
            }
            payment.Status = payResult.IsSuccess() ? CardTransactionStatus.Paid : CardTransactionStatus.PayFailed;
            return payResult;
        }
        private async Task<Result<Transaction>> Refund(PaymentRequest payment, CardTransaction cardTransaction)
        {

            Result<Transaction> refundResult = null;
            var refundAmout = Math.Abs(payment.Amount);

            try
            {
                _logger.LogInformation($"Attempting refund of £{refundAmout} for card transaction {cardTransaction.Id}");
                refundResult = Refund(payment, cardTransaction.Id);
            }
            catch (Exception ex)
            {
                cardTransaction.ErrorMessage = ex.Message;
                _logger.LogError($"Exception occurred for card transaction {cardTransaction.Id}.  Exception was {ex.Message}");
                await _paymentsHandler.ExecuteAsync(new UpdateCommand<CardTransaction>(cardTransaction.Id, cardTransaction));
            }

            if (refundResult.IsSuccess())
            {
                await UpdateSuccess(cardTransaction, -refundResult.Target.Amount, refundResult.Target.ProcessorResponseCode, refundResult.Target.ProcessorResponseText, CardTransactionStatus.Refunded);
            }
            else
            {
                if (refundResult.Errors?.DeepAll().Count > 0)
                {
                    ValidationError error = refundResult.Errors.DeepAll().First();
                    await UpdateFailure(cardTransaction, error.Message, null, null);
                }
                else if (refundResult.Transaction != null)
                {
                    await UpdateFailure(cardTransaction, refundResult.Transaction.Status.ToString(), refundResult.Transaction.ProcessorResponseCode, refundResult.Transaction.ProcessorResponseText);
                }
            }
            payment.Status = refundResult.IsSuccess() ? CardTransactionStatus.Refunded : CardTransactionStatus.PayFailed;
            return refundResult;
        }

        public CardToken CreateCard(Contact contact, CardPaymentRequest command)
        {
            Guid cardTokenId = Guid.NewGuid();
            CreditCard card = null;

            if (!BraintreeCustomerExists(contact.Id.ToString()))
            {
                var result = CreateCustomer(contact, cardTokenId.ToString(), command);
                if (result.IsSuccess())
                    card = result.Target.CreditCards[0];
                else
                {
                    throw new InvalidOperationException(result.Message);
                }
            }
            else
            {
                var result = CreatePaymentMethod(contact, cardTokenId.ToString(), command);
                if (result.IsSuccess())
                    card = (CreditCard)result.Target;
                else
                {
                    throw new InvalidOperationException(result.Message);
                }
            }

            if (card == null)
                return null;

            var cardToken = new CardToken(
                cardTokenId,
                contact.Id,
                int.Parse(card.ExpirationMonth),
                int.Parse(card.ExpirationYear),
                card.MaskedNumber,
                command.CardHolderName,
                command.AddressLine1,
                command.Postcode,
                true);

            return cardToken;
        }

        public async Task UpdateCardTransaction(CardTransaction cardTransaction)
        {
            Transaction transaction = GetTransaction(cardTransaction.Id);

            if (transaction == null)
            {
                await UpdateFailure(cardTransaction, "Not processed", null, null);
            }
            else if (transaction.Status == TransactionStatus.SETTLED || transaction.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT)
            {
                await UpdateSuccess(cardTransaction, transaction.Amount, transaction.ProcessorResponseCode, transaction.ProcessorResponseText, CardTransactionStatus.Paid);
            }
            else
            {
                await UpdateFailure(cardTransaction, transaction.Status.ToString(), transaction.ProcessorResponseCode, transaction.ProcessorResponseText);
            }
        }

        private Result<Braintree.Customer> CreateCustomer(
            Contact contact, string token, CardPaymentRequest command)
        {
            var request = new CustomerRequest
            {
                Id = contact.Id.ToString(),
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Email = contact.Email,
                PaymentMethodNonce = command.PaymentMethodNonce,
                DeviceData = command.DeviceData,
                CreditCard = new CreditCardRequest
                {
                    Token = token,
                    CardholderName = command.CardHolderName,
                    BillingAddress = new CreditCardAddressRequest
                    {
                        StreetAddress = command.AddressLine1,
                        PostalCode = command.Postcode
                    },
                    Options = new CreditCardOptionsRequest
                    {
                        VerifyCard = true,
                        MakeDefault = true,
                        VerificationMerchantAccountId = _merchantAccountId
                    }
                }

            };
            var response = _gateway.Customer.Create(request);
            return response;
        }

        private Result<Braintree.PaymentMethod> CreatePaymentMethod(
            Contact contact,
            string token,
            CardPaymentRequest command)
        {
            var request = new Braintree.PaymentMethodRequest
            {
                CustomerId = contact.Id.ToString(),
                Token = token,
                CardholderName = command.CardHolderName,
                PaymentMethodNonce = command.PaymentMethodNonce,
                DeviceData = command.DeviceData,
                BillingAddress = new PaymentMethodAddressRequest
                {
                    StreetAddress = command.AddressLine1,
                    PostalCode = command.Postcode
                },
                Options = new PaymentMethodOptionsRequest
                {
                    VerifyCard = true,
                    MakeDefault = true,
                    VerificationMerchantAccountId = _merchantAccountId
                }
            };

            var response = _gateway.PaymentMethod.Create(request);

            return response;
        }

        public CardToken UpdateCustomer(
            string existingId,
            Guid contactId)
        {
            var cardTokenId = Guid.NewGuid();
            Braintree.Customer customer = null;

            try
            {
                customer = _gateway.Customer.Find(existingId);
            }
            catch
            {
                return null;
            }

            var card = customer.PaymentMethods?.SingleOrDefault(p => p.IsDefault.GetValueOrDefault()) as CreditCard;

            if (card == null)
                return null;

            var updatePaymentMethodRequest = new Braintree.PaymentMethodRequest
            {
                Token = cardTokenId.ToString()
            };
            var updatePaymentResponse = _gateway.PaymentMethod.Update(card.Token, updatePaymentMethodRequest);

            var updateCustomerRequest = new CustomerRequest
            {
                Id = contactId.ToString()
            };
            var updateCustomerResponse = _gateway.Customer.Update(existingId, updateCustomerRequest);

            CardToken token = new CardToken
            (
                cardTokenId,
                contactId,
                int.Parse(card.ExpirationMonth),
                int.Parse(card.ExpirationYear),
                card.MaskedNumber,
                card.CardholderName,
                card.BillingAddress?.StreetAddress,
                card.BillingAddress?.PostalCode,
                true
            );
            return token;
        }

        private bool BraintreeCustomerExists(string id)
        {
            try
            {
                Braintree.Customer customer = _gateway.Customer.Find(id);
                return true;
            }
            catch (Braintree.Exceptions.NotFoundException)
            {
                return false;
            }
        }

        public bool DeleteCardToken(CardToken token)
        {
            try
            {
                var response = _gateway.PaymentMethod.Delete(token.Id.ToString());
                return response.IsSuccess();
            }
            catch
            {
                return false;
            }

        }

        private void UpdateCardTokenFromResponse(CardToken token, CreditCard card)
        {
            token.ObscuredCardNumber = card.MaskedNumber;
            token.ExpiresYear = int.Parse(card.ExpirationYear);
            token.ExpiresMonth = int.Parse(card.ExpirationMonth);
            token.CardHolderFullName = card.CardholderName;
        }

        private async Task<CardTransaction> CreateCardTransaction(PaymentRequest payment)
        {
            CardTransaction cardTransaction = new CardTransaction(Guid.NewGuid(),
                                                                  payment.CardTransactionBatchId,
                                                                  payment.DeliveryId,
                                                                  payment.CardTokenId,
                                                                  CardTransactionStatus.Requested,
                                                                  DateTime.Now.ToBritishTime(),
                                                                  null,
                                                                  payment.Amount,
                                                                  null,
                                                                  null,
                                                                  null,
                                                                  null);

            await _paymentsHandler.ExecuteAsync(new CreateCommand<CardTransaction>(cardTransaction));
            return cardTransaction;
        }

        private async Task UpdateSuccess(CardTransaction cardTransaction, decimal? amountCaptured, string responseCode, string responseText, CardTransactionStatus status)
        {
            _logger.LogInformation($"Successful {(amountCaptured > 0 ? "payment" : "refund")} of £{Math.Abs(amountCaptured.Value)} for card transaction {cardTransaction.Id}");
            cardTransaction.Status = status;
            cardTransaction.AmountCaptured = amountCaptured;
            cardTransaction.ResponseCode = responseCode;
            cardTransaction.ResponseText = responseText;
            cardTransaction.TimeModified = DateTime.Now.ToBritishTime();
            await _paymentsHandler.ExecuteAsync(new UpdateCommand<CardTransaction>(cardTransaction.Id, cardTransaction));
        }

        private async Task UpdateFailure(CardTransaction cardTransaction, string errorMessage, string responseCode, string responseText)
        {
            _logger.LogWarning($"Unsuccessful payment for card transaction {cardTransaction.Id}.  Error message was {errorMessage}.  Response code was {responseCode}");
            cardTransaction.Status = CardTransactionStatus.PayFailed;
            cardTransaction.ErrorMessage = errorMessage;
            cardTransaction.ResponseCode = responseCode;
            cardTransaction.ResponseText = responseText;
            cardTransaction.TimeModified = DateTime.Now.ToBritishTime();
            await _paymentsHandler.ExecuteAsync(new UpdateCommand<CardTransaction>(cardTransaction.Id, cardTransaction));
        }

        private Result<Transaction> Sale(PaymentRequest payment, Guid cardTransactionId)
        {
            var request = new TransactionRequest
            {
                PaymentMethodToken = payment.CardTokenId.ToString(),
                Amount = payment.Amount,
                OrderId = cardTransactionId.ToString(),
                MerchantAccountId = _merchantAccountId,

                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            var response = _gateway.Transaction.Sale(request);

            return response;
        }

        private Result<Transaction> Refund(PaymentRequest payment, Guid cardTransactionId)
        {

            var request = new TransactionSearchRequest().OrderId.Is(payment.OriginalCardTransactionId.ToString());
            var transaction = _gateway.Transaction.Search(request).FirstItem;

            var refundRequest = new TransactionRefundRequest
            {
                Amount = Math.Abs(payment.Amount),
                OrderId = cardTransactionId.ToString()
            };

            var response = _gateway.Transaction.Refund(
                transaction.Id,
                refundRequest
            );

            return response;
        }


        private Transaction GetTransaction(Guid cardTransactionId)
        {
            var request = new TransactionSearchRequest().
                OrderId.Is(cardTransactionId.ToString());

            ResourceCollection<Transaction> transactions = _gateway.Transaction.Search(request);

            if (transactions.Ids.Count == 0)
                return null;

            return transactions.FirstItem;
        }
    }
}