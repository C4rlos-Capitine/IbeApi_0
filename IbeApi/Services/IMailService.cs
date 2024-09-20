using IbeApi.Models;

namespace IbeApi.Services
{
    public interface IMailService
    {
        bool SendMail(MailData Mail_Data);
    }
}
