using Fed.Api.External.SendGridService.SendGridResponse;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Fed.Api.External.SendGridService
{
    public class SendGridService : IEmailService
    {
        private readonly string _apiKey;
        private readonly SendGridMarketingSettings _marketingSettings;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public SendGridService(
            string apiKey,
            SendGridMarketingSettings marketingSettings,
            HttpClient httpClient,
            ILogger logger)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _marketingSettings = marketingSettings ?? throw new ArgumentNullException(nameof(marketingSettings));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<bool> SendMessageAsync(Email msg)
        {
            try
            {
                var recipients = msg.ToAddresses.Select(e => new EmailAddress(e)).ToList();
                var email =
                    MailHelper.CreateSingleEmailToMultipleRecipients(
                        new EmailAddress(msg.FromAddress),
                        recipients,
                        msg.Subject,
                        msg.PlainText,
                        msg.HtmlText);

                if (msg.CCs != null && msg.CCs.Count > 0)
                {
                    var ccs = msg.CCs.Select(e => new EmailAddress(e)).ToList();
                    email.AddCcs(ccs);
                }

                if (msg.BCCs != null && msg.BCCs.Count > 0)
                {
                    var bccs = msg.BCCs.Select(e => new EmailAddress(e)).ToList();
                    email.AddBccs(bccs);
                }

                var client = new SendGridClient(_apiKey);
                var response = await client.SendEmailAsync(email);

                var success = response.StatusCode == (HttpStatusCode)202;

                if (!success)
                {
                    var responseText = await response.Body.ReadAsStringAsync();
                    _logger.LogError(
                        $"Sending email via SendGrid failed with status code {response.StatusCode} and message: {responseText}.");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "An error has been thrown when trying to send an email to {recipients} with subject '{subject}'. The error message is: {err}. Full exception: {ex}.",
                    msg.ToAddresses, msg.Subject, ex.Message, ex);

                return false;
            }
        }

        public async Task AddOrUpdateContact(string oldEmailAddress, string emailAddress, SendGridContact contact)
        {
            try
            {
                // We have to use a plain HttpClient in order to create/update a contact in SendGrid,
                // because the current SendGridClient and the latest SendGrid API do not work
                // together as expected. There is a bug with the API which doesn't allow a media type
                // with a UTF-8 charset in the request message.
                // See: https://github.com/sendgrid/sendgrid-csharp/issues/910
                if (!string.IsNullOrEmpty(oldEmailAddress) && string.Compare(oldEmailAddress, emailAddress, true) != 0)
                {
                    var allContactsResponse = await _httpClient.GetAsync("https://api.sendgrid.com/v3/marketing/contacts");

                    if (allContactsResponse.StatusCode.Equals(HttpStatusCode.OK))
                    {
                        var contacts = await allContactsResponse.Content.ReadAsStringAsync();
                        var contactsList = Newtonsoft.Json.JsonConvert.DeserializeObject<SendGridContactResponse>(contacts);
                        var sendGridContact = contactsList.result.FirstOrDefault(c => c.email.ToLower() == oldEmailAddress.ToLower());


                        if (sendGridContact != null)
                        {
                            _logger.LogInformation($"Sendgrid contact with id {sendGridContact.id} and email {oldEmailAddress} found.");

                            var deleteContactResponse = await _httpClient.DeleteAsync($"https://api.sendgrid.com/v3/marketing/contacts?ids={sendGridContact.id}");

                            var deleteContact = await deleteContactResponse.Content.ReadAsStringAsync();
                            if (deleteContactResponse.StatusCode.Equals(HttpStatusCode.Accepted))
                            {
                                var deleteContactJob = Newtonsoft.Json.JsonConvert.DeserializeObject<SendGridActionResponse>(deleteContact);
                                _logger.LogInformation($"DeleteContactJob with id {deleteContactJob.job_id} received.");

                                var jobResponse = await GetSendGridJobResponseAsync(deleteContactJob);
                                if (jobResponse != null && jobResponse.results.errored_count == 0)
                                {
                                    _logger.LogInformation($"Sendgrid contact with id {sendGridContact.id} and email {oldEmailAddress} removal status - {jobResponse.status}");
                                }
                                else
                                    _logger.LogInformation($"Error deleting sendgrid contact with id {sendGridContact.id} and email {oldEmailAddress}. Error was {jobResponse.results.errors_url}");
                            }
                            else
                            {
                                _logger.LogError($"Failed to delete contact with email address '{oldEmailAddress}'. Error message: {deleteContact}");
                            }

                        }
                        else
                            _logger.LogInformation($"SendGrid contact with email {oldEmailAddress} not found.");

                    }

                }

                emailAddress = emailAddress.ToLowerInvariant();

                var jsonBody = $"{{ \"list_ids\": [ \"{_marketingSettings.PrimaryContactListId}\" ], \"contacts\": [ {{ \"email\": \"{emailAddress}\", \"first_name\": \"{contact.FirstName}\", \"last_name\": \"{contact.LastName}\", \"custom_fields\": {{ \"{_marketingSettings.CompanyIdFieldId}\": \"{contact.CompanyId}\", \"{_marketingSettings.ContactIdFieldId}\": \"{contact.ContactId}\", \"{_marketingSettings.CompanyNameFieldId}\": \"{contact.CompanyName}\" }} }} ] }}";

                _logger.LogInformation("JSON Payload to SendGrid: {payload}", jsonBody);

                var request = new StringContent(jsonBody);
                request.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _httpClient.PutAsync(
                    "https://api.sendgrid.com/v3/marketing/contacts",
                    request);

                var content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(content) && response.StatusCode.Equals(HttpStatusCode.Accepted))
                {
                    var upsertJob = Newtonsoft.Json.JsonConvert.DeserializeObject<SendGridActionResponse>(content);
                    var jobResponse = await GetSendGridJobResponseAsync(upsertJob);
                    if (jobResponse != null && jobResponse.results.errored_count == 0)
                    {
                        _logger.LogInformation($"Sendgrid contact with email {emailAddress} add status - {jobResponse.status}");
                    }
                }


                if (!response.StatusCode.Equals(HttpStatusCode.Accepted))
                    throw new Exception($"Failed to create or update contact with email address '{emailAddress}'. Error message: {content}");
            }
            catch (Exception ex)
            {
                _logger.LogError("An error has been thrown when trying to create or update a contact: {err}", ex.Message);
                throw;
            }
        }

        public async Task UnsubscribeFromGroupAsync(string emailAddress, string groupId)
        {
            try
            {
                emailAddress = emailAddress.ToLowerInvariant();

                var client = new SendGridClient(_apiKey);
                var response =
                    await client.RequestAsync(
                        SendGridClient.Method.POST,
                        urlPath: $"asm/groups/{groupId}/suppressions",
                        requestBody: $"{{ \"recipient_emails\": [ \"{emailAddress}\" ] }}");

                var content = await response.Body.ReadAsStringAsync();

                if (!response.StatusCode.Equals(HttpStatusCode.Created))
                    throw new Exception($"Failed to unsubscribe email address '{emailAddress}'. Error message: {content}");

            }
            catch (Exception ex)
            {
                _logger.LogError("An error has been thrown when trying to unsubscribe an email address: {err}", ex.Message);

                throw;
            }
        }

        public async Task RemoveSupressionFromGroupAsync(string emailAddress, string groupId)
        {
            try
            {
                emailAddress = emailAddress.ToLowerInvariant();

                var client = new SendGridClient(_apiKey);
                var response =
                    await client.RequestAsync(
                        SendGridClient.Method.DELETE,
                        urlPath: $"asm/groups/{groupId}/suppressions/{emailAddress}");

                var content = await response.Body.ReadAsStringAsync();

                if (!response.StatusCode.Equals(HttpStatusCode.NoContent))
                    throw new Exception($"Failed to remove the suppresion for email address '{emailAddress}'. Error message: {content}");

            }
            catch (Exception ex)
            {
                _logger.LogError("An error has been thrown when trying to remove the supression an email address: {err}", ex.Message);

                throw;
            }
        }

        private async Task<SendGridJobResponse> GetSendGridJobResponseAsync(SendGridActionResponse action)
        {
            var jobResponse = await _httpClient.GetAsync($"https://api.sendgrid.com/v3/marketing/contacts/imports/{action.job_id}");
            if (jobResponse.StatusCode.Equals(HttpStatusCode.OK))
            {
                var job = await jobResponse.Content.ReadAsStringAsync();
                var jobResult = Newtonsoft.Json.JsonConvert.DeserializeObject<SendGridJobResponse>(job);
                return jobResult;
            }
            return null;
        }
    }
}
