using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CryptoTradingBot_MVP
{
    public class APICoinmarketcapData
    {
        private static string API_KEY = "92a21b09-8b4a-43ae-a693-94bd6733cd28";
        public string Start(string symbol)
        {
            try
            {
                return makeAPICall(symbol);
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
            }
            return "";
        }

        static string makeAPICall(string symbol)
        {
            var URL = new UriBuilder("https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest");
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["symbol"] = symbol;

            URL.Query = queryString.ToString();

            var client = new WebClient();
            client.Headers.Add("X-CMC_PRO_API_KEY", API_KEY);
            client.Headers.Add("Accepts", "application/json");

            var res = client.DownloadString(URL.ToString());
            dynamic result_parsed = JObject.Parse(res);
            
            JObject json = JObject.Parse(res);
            var price_ = json.SelectToken($"data.{symbol.ToUpper()}.quote.USD.price");

            return price_ != null ? price_.ToString() : null;

        }
    }
}

