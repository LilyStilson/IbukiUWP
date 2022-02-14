using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ibuki.Utils {
    public struct ProxyAuth {
        public string Address { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public static class Helpers {

        public static Uri AppendArgsToUri(Uri baseURI, params string[] args) {
            string relativeURL = "", resultURL = "";
            for(int i = 0; i < args.Length; i++) { 
                if (i == 0) {
                    relativeURL += $"?{args[i]}";
                } else {
                    relativeURL += $"&{args[i]}";
                }
            }

            resultURL = baseURI.AbsoluteUri + relativeURL;
            return new Uri(resultURL);
        }

        public static Uri FormatBooruUri(Uri baseURI, Dictionary<string, string> args) {
            string resultURL = Uri.UnescapeDataString(baseURI.AbsoluteUri);

            foreach (Match match in Regex.Matches(resultURL, @"{\s*(.*?)\s*}")) {       /// "{MATCH}"
                string token = match.Value.Replace("{", "").Replace("}", "");           /// "TOKEN"
                if (match.Value == "{}" || args[token] == "")
                    resultURL = resultURL
                        .Replace(match.Value, args[token])
                        .Remove(resultURL.IndexOf(token) - 2, 1);
                else
                    resultURL = resultURL.Replace(match.Value, args[token]);
            }

            return new Uri(resultURL);
        }

        /// <summary>
        /// Asyncronously gets contetnts of an HTML file at the address
        /// </summary>
        /// <param name="URL">Target URL</param>
        /// <returns>Contents of response as string</returns>
        public static async Task<string> GetHTML(string URL) {
            HttpClient Client = new HttpClient();
            Uri URI = new Uri(URL);

            HttpResponseMessage Response = await Client.GetAsync(URI);

            return await Response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="uri">Uri representation of target URL</param>
        /// <returns><inheritdoc/></returns>
        public static async Task<string> GetHTML(Uri uri) {
            HttpClient Client = new HttpClient();

            HttpResponseMessage Response = await Client.GetAsync(uri);

            return await Response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="proxyIP"></param>
        /// <returns><inheritdoc/></returns>
        /// <remarks>Use SOCKS5://88.198.24.108:1080 for testing</remarks>
        public static async Task<string> GetHTMLWithProxy(string URL) {
            /*
            HttpClient Client = new HttpClient(new HttpClientHandler() {
                Proxy = new WebProxy() {
                    Address = new Uri(proxyAuth.Address),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential() {
                        UserName = proxyAuth.Login,
                        Password = proxyAuth.Password
                    }
                }
            });*/

            HttpClient Client = new HttpClient(new HttpClientHandler() {
                Proxy = new WebProxy("http://136.243.211.104:80"),
                UseProxy = true,
            });

            Uri URI = new Uri(URL);

            HttpResponseMessage Response = await Client.GetAsync(URI);

            return await Response.Content.ReadAsStringAsync();
        }
    }
}
