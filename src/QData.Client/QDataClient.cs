using System;
using System.Collections.Generic;
using System.Reflection;
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

    public class QDataClient
    {

        private static HttpClient Client = new HttpClient();

        private static JsonSerializerSettings Settings = new JsonSerializerSettings();

        static QDataClient()
        {
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var dateTimeConverter = new IsoDateTimeConverter();
            // Default for IsoDateTimeConverter is yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK
            dateTimeConverter.DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm";


            Settings.Converters = new List<JsonConverter> { dateTimeConverter };
        }

        public IEnumerable<T> Get<T>(Uri accsessPoint, QDescriptor descriptor)
        {
            var json = JsonConvert.SerializeObject(descriptor, Settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            Task<HttpResponseMessage> response =
                Client.PostAsync(accsessPoint, content);

            if (response.Result.IsSuccessStatusCode)
            {
                string jsonContent = response.GetAwaiter().GetResult().Content.ReadAsStringAsync().Result;

                return JsonConvert.DeserializeObject<IEnumerable<T>>(jsonContent);
            }

            return null;
        }

        public IEnumerable<TP> GetProjection<TP>(Uri accsessPoint, QDescriptor descriptor) where TP : IProjection
        {
            var json = JsonConvert.SerializeObject(descriptor, Settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Task<HttpResponseMessage> response =
                Client.PostAsync(accsessPoint, content);
            if (response.Result.IsSuccessStatusCode)
            {
                string jsonContent = response.GetAwaiter().GetResult().Content.ReadAsStringAsync().Result;

                return JsonConvert.DeserializeObject<IEnumerable<TP>>(jsonContent);
            }

            return null;


        }

        public void Get(Uri accsessPoint, QDescriptor descriptor, Type returnType, object result)
        {
            var json = JsonConvert.SerializeObject(descriptor, Settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            Task<HttpResponseMessage> response =
                Client.PostAsync(accsessPoint, content);

            if (response.Result.IsSuccessStatusCode)
            {
                string jsonContent = response.GetAwaiter().GetResult().Content.ReadAsStringAsync().Result;

                var listType = typeof (IEnumerable<>);
                var targetType = listType.MakeGenericType(returnType);
                var res = JsonConvert.DeserializeObject(jsonContent, targetType);
                var methodInfo = result.GetType().GetTypeInfo().GetMethod("AddRange");
                methodInfo.Invoke(result, new object[] {res});
            }
            
        }
    }
}
