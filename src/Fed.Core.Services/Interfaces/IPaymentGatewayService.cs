using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Models;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IPaymentGatewayService
    {
        Task<CardTransaction> ProcessPayment(PaymentRequest payment);

        CardToken CreateCard(Contact contact, CardPaymentRequest command);

        bool DeleteCardToken(CardToken cardToken);

        Task UpdateCardTransaction(CardTransaction cardTransaction);

        Task<string> GetClientToken();
    }
}