using Fed.Api.External.XeroService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fed.AzureFunctions.Runner
{
    public static class FunctionsEnvironment
    {
        public static void LoadEnvironment()
        {
            using (var file = File.OpenText("Properties\\launchSettings.json"))
            {
                var reader = new JsonTextReader(file);
                var jObject = JObject.Load(reader);

                var variables = jObject
                .GetValue("dev")
                .SelectMany(profiles => profiles.Children())
                .SelectMany(profile => profile.Children<JProperty>())
                .ToList();

                foreach (var variable in variables)
                {
                    Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                }
            }
        }

        public static XeroSettings GetXeroSettings(string environment)
        {

            using (var file = File.OpenText("Properties\\launchSettings.json"))
            {
                var reader = new JsonTextReader(file);
                var jObject = JObject.Load(reader);

                var variables = jObject
                .GetValue(environment)
                .SelectMany(profiles => profiles.Children())
                .SelectMany(profile => profile.Children<JProperty>())
                .ToList();

                XeroSettings settings = new XeroSettings
                (
                    GetSetting(variables, "xero-consumer-key"),
                    GetSetting(variables, "xero-consumer-secret"),
                    GetSetting(variables, "xero-certificate"),
                    GetSetting(variables, "xero-certificate-password")
                );

                return settings;
            }
        }

        private static string GetSetting(List<JProperty> settings, string settingName) => settings.Where(s => s.Name == settingName).FirstOrDefault()?.Value.ToString();



    }
}
