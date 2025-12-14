using System.Threading.Tasks;

namespace Fed.Api.External.SendGridService
{
    public interface IEmailService
    {
        Task<bool> SendMessageAsync(Email msg);
    }
}
