using IbeApi.Models;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using System.Data.SqlClient;
using IbeApi.Controllers;
using Microsoft.AspNetCore.Routing.Template;

namespace IbeApi.Services
{
    public class MailService : IMailService
    {
        private readonly string _connectionString;
        private readonly ILogger<MailController> _logger;
        MailSettings Mail_Settings = null;
        public MailService(IOptions<MailSettings> options, IConfiguration configuration, ILogger<MailController> logger)
        {
            Mail_Settings = options.Value;
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }
        public bool SendMail(MailData Mail_Data)
        {
            if (Mail_Data.auth == 1) { 
                return useTWOStepsAuth(Mail_Data);
            }
            if (Mail_Data.password.Length>0)
            {
                return SendConfirm(Mail_Data);
            }
            var newpassword = UpdatePassword(Mail_Data.EmailToId);
            if (newpassword == "0") { 
                return false;
            }

                try
                {
                //MimeMessage - a class from Mimekit
                MimeMessage email_Message = new MimeMessage();
                MailboxAddress email_From = new MailboxAddress(Mail_Settings.Name, Mail_Settings.EmailId);
                email_Message.From.Add(email_From);
                MailboxAddress email_To = new MailboxAddress(Mail_Data.EmailToName, Mail_Data.EmailToId);
                email_Message.To.Add(email_To);
                email_Message.Subject = Mail_Data.EmailSubject;
                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = "<h1 style='color: blue;'>Olá "+Mail_Data.name+"!</h1><p style='font-size: 16px;'>Sua nova senha é: "+ newpassword + "</p>\"\r\n";

                //emailBodyBuilder.TextBody = Mail_Data.EmailBody;
                email_Message.Body = emailBodyBuilder.ToMessageBody();
                //this is the SmtpClient class from the Mailkit.Net.Smtp namespace, not the System.Net.Mail one
                SmtpClient MailClient = new SmtpClient();
                MailClient.Connect(Mail_Settings.Host, Mail_Settings.Port, Mail_Settings.UseSSL);
                MailClient.Authenticate(Mail_Settings.EmailId, Mail_Settings.Password);
                MailClient.Send(email_Message);
                MailClient.Disconnect(true);
                MailClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error {ex}", ex);
                return false;
            }
        }
        private bool SendConfirm(MailData Mail_Data)
        {


            try
            {
                //MimeMessage - a class from Mimekit
                MimeMessage email_Message = new MimeMessage();
                MailboxAddress email_From = new MailboxAddress(Mail_Settings.Name, Mail_Settings.EmailId);
                email_Message.From.Add(email_From);
                MailboxAddress email_To = new MailboxAddress(Mail_Data.EmailToName, Mail_Data.EmailToId);
                email_Message.To.Add(email_To);
                email_Message.Subject = Mail_Data.EmailSubject;
                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = "<h1 style='color: blue;'>Olá " + Mail_Data.name + "!</h1><p style='font-size: 16px;'>Sua inscrição foi bem sucedida. Autentique - se com a sua senha: " + Mail_Data.password + " e email</p>\"\r\n";

                //emailBodyBuilder.TextBody = Mail_Data.EmailBody;
                email_Message.Body = emailBodyBuilder.ToMessageBody();
                //this is the SmtpClient class from the Mailkit.Net.Smtp namespace, not the System.Net.Mail one
                SmtpClient MailClient = new SmtpClient();
                MailClient.Connect(Mail_Settings.Host, Mail_Settings.Port, Mail_Settings.UseSSL);
                MailClient.Authenticate(Mail_Settings.EmailId, Mail_Settings.Password);
                MailClient.Send(email_Message);
                MailClient.Disconnect(true);
                MailClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                // Exception Details
                return false;
            }
        }
        private bool useTWOStepsAuth(MailData Mail_Data)
        {
            try
            {
                //MimeMessage - a class from Mimekit
                MimeMessage email_Message = new MimeMessage();
                MailboxAddress email_From = new MailboxAddress(Mail_Settings.Name, Mail_Settings.EmailId);
                email_Message.From.Add(email_From);
                MailboxAddress email_To = new MailboxAddress(Mail_Data.EmailToName, Mail_Data.EmailToId);
                email_Message.To.Add(email_To);
                email_Message.Subject = "codigo: "+ Mail_Data.getCodigo();

                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = "<div><h1 style='color: blue;'>Olá " + Mail_Data.name + "!</h1><p style='font-size: 16px;'>O seu Código de autenticação: " + Mail_Data.getCodigo() + " </p></div>\"\r\n";
                //emailBodyBuilder.TextBody = Mail_Data.EmailBody;
                email_Message.Body = emailBodyBuilder.ToMessageBody();
                //this is the SmtpClient class from the Mailkit.Net.Smtp namespace, not the System.Net.Mail one
                SmtpClient MailClient = new SmtpClient();
                MailClient.Connect(Mail_Settings.Host, Mail_Settings.Port, Mail_Settings.UseSSL);
                MailClient.Authenticate(Mail_Settings.EmailId, Mail_Settings.Password);
                MailClient.Send(email_Message);
                MailClient.Disconnect(true);
                MailClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                // Exception Details
                return false;
            }
        }
        private string UpdatePassword(string email)
        {
            

        
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Email or new password parameter is null or empty.");
                return "Email and password cannot be null or empty.";
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                _logger.LogInformation("Database connection opened.");

                const string sql = @"
            UPDATE GBICANDI 
            SET PASSWORD = @PASSWORD 
            WHERE EMAIL = @EMAIL;";

                using (var command = new SqlCommand(sql, connection))
                {
                    string newpassword = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0).ToString();
                    command.Parameters.AddWithValue("@PASSWORD", newpassword);
                    command.Parameters.AddWithValue("@EMAIL", email);

                    _logger.LogInformation("SQL command prepared: {sql}", sql);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        _logger.LogInformation("Password updated successfully for {email}", email);
                        return newpassword;
                    }
                    else
                    {
                        _logger.LogWarning("No user found with email {email}.", email);
                        return "0";
                    }
                }
            }
        }
    }


}
