using Fed.Core.Converters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Fed.Web.Service.Config
{
    public static class JsonConfig
    {
        public static void MvcConfig(MvcNewtonsoftJsonOptions config)
        {
            config.SerializerSettings.Converters.Add(new DateJsonConverter());

            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new DateJsonConverter());
                return settings;
            };
        }
    }
}