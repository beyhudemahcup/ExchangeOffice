using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace ExchangeOffice
{
    internal class APIClass
    {
        public class Root
        {
            //open exchange rate ID
            //3255c05823ed41c081b0a85cf6349d6e
            public Rate rates { get; set; }
            public long timestamp;
            public string license;
        }

        public class Rate
        {
            //those values comes from a website that provides APIs
            public double INR { get; set; }
            public double JPY { get; set; }
            public double USD { get; set; }
            public double EUR { get; set; }
            public double NZD { get; set; }
            public double CAD { get; set; }
            public double ISK { get; set; }
            public double PHP { get; set; }
            public double DKK { get; set; }
            public double CZK { get; set; }
        }

        public static async Task<Root> GetData<T>(string url)
        {
            var root = new Root();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(1);
                    HttpResponseMessage httpResponseMessage = await client.GetAsync(url);

                    if(httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var responseString = await httpResponseMessage.Content.ReadAsStringAsync();
                        var responseObject = JsonConvert.DeserializeObject<Root>(responseString);

                        MessageBox.Show($"Timestamp: {responseObject.timestamp}, Information: {MessageBoxButton.OK}");
                   
                        return responseObject;
                    }
                    return root;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return root;
            }
        }
    }
}
