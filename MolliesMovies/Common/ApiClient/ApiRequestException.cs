using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MolliesMovies.Common.ApiClient
{
    public class ApiRequestException : Exception
    {
        public ApiRequestException(HttpStatusCode? statusCode, Exception inner) : base("API request failed", inner)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode? StatusCode { get; }
        
        public object Content { get; private set; }

        public static async Task<ApiRequestException> FromResponseAsync(HttpResponseMessage response, Exception inner = null)
        {
            var content = await response.Content.ReadAsStringAsync();
            var exception = new ApiRequestException(response.StatusCode, inner);
            try
            {
                exception.Content = JsonConvert.DeserializeObject(content);
            }
            catch (Exception)
            {
                exception.Content = content;
            }

            return exception;
        }
    }
}