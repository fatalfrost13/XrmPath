using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Umbraco.Core.Logging;
using System.Net.Http.Headers;

namespace XrmPath.Web.Helpers.Utils
{
    public static class ApiUtility
    {
        static readonly HttpClient client = new HttpClient() { Timeout = new TimeSpan(0,5,0) }; // change from default 100s to 5 mins timeout on API calls
        public static async Task<T> ApiToModelAsync<T>(T source, string apiUrl)
        {
            if (!apiUrl.StartsWith("http://") && !apiUrl.StartsWith("https://"))
            {
                return (T)Activator.CreateInstance(typeof(T));
            }

            var response = await client.GetAsync(apiUrl).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var regionalDashboard = await response.Content.ReadAsAsync<T>();
                return regionalDashboard;
            }
            return default(T);
        }

        public static object ApiToModel<T>(string apiUrl)
        {
            try
            {
                if (!apiUrl.StartsWith("http://") && !apiUrl.StartsWith("https://"))
                {
                    return null;
                }

                var response = client.GetAsync(apiUrl).ConfigureAwait(false);

                DateTime start = DateTime.Now;
                var results = response.GetAwaiter().GetResult(); // api call
                DateTime end = DateTime.Now;
                TimeSpan elapsed = end - start;
                if (elapsed.TotalSeconds >= 100)
                    LogHelper.Info<string>($"XrmPath ApiUtility.ApiToModel({apiUrl}) took {elapsed.TotalSeconds} seconds");

                if (results.IsSuccessStatusCode)
                {
                    var regionalDashboard = results.Content.ReadAsAsync<T>();
                    return regionalDashboard.Result;
                }
                else
                {
                    LogHelper.Warn<string>($"XrmPath caught error on ApiUtility.ApiToModel(): Type:{typeof(T)} Url({apiUrl}) Error: {results.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn<string>($"XrmPath caught error on ApiUtility.ApiToModel(): Type:{typeof(T)} Url({apiUrl}) Error: {ex}");
            }
            return null;
        }
        internal static object ApiJsonToModel<T>(string apiUrl)
        {
            try
            {
                if (!apiUrl.StartsWith("http://") && !apiUrl.StartsWith("https://"))
                {
                    return null;
                }

                DateTime start = DateTime.Now;
                var response = client.GetAsync(apiUrl).ConfigureAwait(false);
                var results = response.GetAwaiter().GetResult(); // api call
                DateTime end = DateTime.Now;
                TimeSpan elapsed = end - start;
                if (elapsed.TotalSeconds >= 100)
                    LogHelper.Info<string>($"XrmPath ApiUtility.ApiJsonToModel({apiUrl}) took {elapsed.TotalSeconds} seconds");

                if (results.IsSuccessStatusCode)
                {
                    var resultString = results.Content.ReadAsStringAsync();
                    var apiObject = JsonConvert.DeserializeObject<T>(resultString.Result);
                    
                    return apiObject;
                }
                else
                {
                    LogHelper.Warn<string>($"XrmPath caught error on ApiUtility.ApiJsonToModel(): Type:{typeof(T)} Url({apiUrl}) Error: {results.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn<string>($"XrmPath caught error on ApiUtility.ApiJsonToModel(): Type:{typeof(T)} Url({apiUrl}) Error: {ex}");
            }
            return null;
        }
    }
}