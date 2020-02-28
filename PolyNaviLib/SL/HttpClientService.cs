﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PolyNaviLib.SL
{
    public static class HttpClientService
    {
        public static async Task<string> GetResponseAsync(HttpClient client, string uri, CancellationToken cts)
        {
            try
            {
                var response = await client.GetAsync(uri, cts);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }
    }
}
