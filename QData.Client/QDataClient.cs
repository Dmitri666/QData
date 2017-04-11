using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QData.Client
{
    using System.Net.Http;
    using System.Net.Http.Headers;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using Qdata.Json.Contract;

    using QData.Common;

    public class QDataClient<TM> where TM : IModelEntity
    {
       

        public IEnumerable<TM> Get(Uri accsessPoint, QDescriptor descriptor)
        {
            using (var client = new HttpClient())
            {
                var dateTimeConverter = new IsoDateTimeConverter();
                // Default for IsoDateTimeConverter is yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK
                dateTimeConverter.DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm";

                var settings = new JsonSerializerSettings();
                settings.Converters = new List<JsonConverter> { dateTimeConverter };

                var json = JsonConvert.SerializeObject(descriptor, settings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (
                    Task<HttpResponseMessage> response =
                        client.PostAsync(accsessPoint, content))
                {
                    if (response.Result.IsSuccessStatusCode)
                    {
                        string jsonContent = response.GetAwaiter().GetResult().Content.ReadAsStringAsync().Result;

                        return JsonConvert.DeserializeObject<IEnumerable<TM>>(jsonContent);
                    }

                    return null;
                }
            }

            return null;
        }

        public IEnumerable<TP> Get<TP>(Uri accsessPoint, QDescriptor descriptor) where TP : IProjection
        {
            using (var client = new HttpClient())
            {
                var dateTimeConverter = new IsoDateTimeConverter();
                // Default for IsoDateTimeConverter is yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK
                dateTimeConverter.DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm";

                var settings = new JsonSerializerSettings();
                settings.Converters = new List<JsonConverter> { dateTimeConverter };

                var json = JsonConvert.SerializeObject(descriptor, settings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (
                    Task<HttpResponseMessage> response =
                        client.PostAsync(accsessPoint, content))
                {
                    if (response.Result.IsSuccessStatusCode)
                    {
                        string jsonContent = response.GetAwaiter().GetResult().Content.ReadAsStringAsync().Result;

                        return JsonConvert.DeserializeObject<IEnumerable<TP>>(jsonContent);
                    }

                    return null;
                }
            }

            return null;
        }

        public void Get(Uri accsessPoint, QDescriptor descriptor, Type returnType, object result)
        {
            using (var client = new HttpClient())
            {
                var dateTimeConverter = new IsoDateTimeConverter();
                // Default for IsoDateTimeConverter is yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK
                dateTimeConverter.DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm";

                var settings = new JsonSerializerSettings();
                settings.Converters = new List<JsonConverter> { dateTimeConverter };

                var json = JsonConvert.SerializeObject(descriptor, settings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (
                    Task<HttpResponseMessage> response =
                        client.PostAsync(accsessPoint, content))
                {
                    if (response.Result.IsSuccessStatusCode)
                    {
                        string jsonContent = response.GetAwaiter().GetResult().Content.ReadAsStringAsync().Result;

                        var listType = typeof(IEnumerable<>);
                        var targetType = listType.MakeGenericType(returnType);
                        var res = JsonConvert.DeserializeObject(jsonContent, targetType);
                        var methodInfo = result.GetType().GetMethod("AddRange");
                        methodInfo.Invoke(result, new object[] { res });
                    }


                }
            }


        }
    }
}
