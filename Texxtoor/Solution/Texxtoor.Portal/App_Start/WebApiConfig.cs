namespace Texxtoor.Portal
{
    using System.Net.Http.Headers;
    using System.Web.Http;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Attribute routing.
            // http://www.asp.net/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2
            config.MapHttpAttributeRoutes();

            // Use camel case for JSON data.
            var jsonformatter = config.Formatters.JsonFormatter;
            jsonformatter.SerializerSettings = new JsonSerializerSettings();
            jsonformatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonformatter.SerializerSettings.Converters.Add(new IsoDateTimeConverter());
            jsonformatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // set JSON formatter as default
            config.Formatters.Remove(jsonformatter);
            config.Formatters.Insert(0, jsonformatter); 
        }
    }
}