using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.Api.External.ActivityLogs
{
    public class LogonActivityClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public LogonActivityClient(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<IList<CustomerActivity>> GetLogonHistoryAsync(string userId, int maxCount = 10)
        {
            try
            {
                var logonHistory = await _httpClient.GetAsync($"{_baseUrl}&userId={userId}&activityType=Logon");
                var json1 = await logonHistory.Content.ReadAsStringAsync();
                var logons = JsonConvert.DeserializeObject<IList<CustomerActivity>>(json1);

                var autoLogonHistory = await _httpClient.GetAsync($"{_baseUrl}&userId={userId}&activityType=AutoLogon");
                var json2 = await autoLogonHistory.Content.ReadAsStringAsync();
                var autoLogons = JsonConvert.DeserializeObject<IList<CustomerActivity>>(json2);

                return logons
                    .Concat(autoLogons)
                    .OrderByDescending(l => l.Timestamp)
                    .Take(maxCount)
                    .ToList();
            }
            catch
            {
                return null;
            }
        }

        public static LogonActivityClient Create(string baseUrl)
        {
            return new LogonActivityClient(new HttpClient(), baseUrl);
        }
    }
}
