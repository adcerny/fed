using Fed.Web.Portal.Models.Validation;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace Fed.Web.Portal.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetFriendlyErrorMessage(this Exception ex)
        {
            var httpEx = ex as HttpRequestException;

            if (httpEx == null)
                return ex.GetDefaultErrorMessage();

            const string fedServiceValidationError = "400 (Bad Request): ";

            if (!httpEx.Message.StartsWith(fedServiceValidationError))
                return ex.GetDefaultErrorMessage();

            try
            {
                var msg = httpEx.Message.Substring(fedServiceValidationError.Length);
                var error = JsonConvert.DeserializeObject<FedServiceError>(msg);
                return error.ErrorMessage ?? msg;
            }
            catch
            {
                return ex.GetDefaultErrorMessage();
            }

        }

        private static string GetDefaultErrorMessage(this Exception ex) => ex.Message;
    }
}