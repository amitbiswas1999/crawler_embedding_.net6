using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Net;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;


namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebCrawlerController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IAsyncEnumerable<string>>> Crawlwebsite(string url, int level)
        {
            List<string> web_url = new List<string>();
            web_url.Add(url);
            var doc = GetDocument(url);
            //var Links = ExtractLinks(doc);
            var all_links = FilteredLink(web_url, level);
            // Console.WriteLine(String.Join(Environment.NewLine, all_links));
            var logs= await Linktotext(all_links);
            return Ok(logs);
        }
        private List<String> FilteredLink(List<string> web_url, int level)
        {
            List<string> local_url = new List<string>();
            local_url.AddRange(web_url);

            while (level != 0)
            {
                foreach (var items in local_url.ToList())
                {
                    var docs = GetDocument(items);
                    var new_links = ExtractLinks(docs);
                    foreach (var links in new_links)
                    {
                        if (!web_url.Contains(links))
                        {
                            web_url.Add(links);
                            local_url.Add(links);
                        }
                        local_url.Remove(links);
                    }
                }

                level--;
            }
            return web_url;
        }

        private HtmlDocument GetDocument(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            // return doc; 
            return doc;
        }
        private string Gettext(HtmlDocument doc)
        {
            string str = doc.DocumentNode.InnerText.Trim();
            string cleanedText = Regex.Replace(str, @"\s+", " ");
            string outputtext = Regex.Replace(cleanedText, @"#\d+", "");
            return outputtext;
        }
        private List<string> ExtractLinks(HtmlDocument doc)
        {
            List<string> hrefTags = new List<string>();
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                HtmlAttribute att = link.Attributes["href"];
                hrefTags.Add(att.Value);
            }
            List<string> filteredList = hrefTags.Where(s => s.Contains("www.cloudcraftz")).ToList();
            return filteredList;
        }
        private async Task<List<string>> Linktotext(List<String> links)
        {
            JArray embeddingVector = new JArray();
            List<string>textmessage = new List<string>();

            foreach (var items in links)
            {
                var doc = GetDocument(items);
                var str = Gettext(doc);
                JToken embeddings = await texttoemb(str);
                JArray embeddingsArray = new JArray(embeddings);
                embeddingVector.Add(embeddingsArray);
                string[] substrings = items.Split('/');
                string elem = substrings[substrings.Length - 2];
                string txt = ".txt";
                string file_name = elem + txt;
                var status = await UpsertVector(elem, str, embeddingsArray);
                if (status == HttpStatusCode.OK)
                {
                    string Message = "Embedding upserted successfully : " + elem;
                    textmessage.Add(Message);
                }
                else
                {
                    string Message = "Error during upserting embedding : "   + elem;
                    textmessage.Add(Message);     
                }
            }
            return textmessage;
        }
        private async Task<JToken> texttoemb(string text)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://api.openai.com/");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer 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");

            // Set up the request body as a JSON string
            string requestBody = @"{
            ""input"": """ + text + @""",
            ""model"": ""text-embedding-ada-002""
            }";
         // Create the request content as a StringContent object
            var requestContent = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync("v1/embeddings", requestContent);            
            try
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject responseJson = JObject.Parse(responseBody);
                JToken embeddings = responseJson["data"][0]["embedding"];
                return embeddings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        static async Task<HttpStatusCode> UpsertVector(string id, string metadataText, JArray embvetcor)
        {
            // Set up the request payload
            var payload = new
            {
                vectors = new[]
                {
                    new
                    {
                        id = id,
                        values = embvetcor,
                        metadata = new
                        {
                            text = metadataText
                        }
                    }
                }
              };

            // Serialize the payload to a JSON string
            var json = JsonConvert.SerializeObject(payload);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://test1-5a3ac21.svc.us-west1-gcp-free.pinecone.io/vectors/upsert"),
                Headers =
                {
                    { "accept", "application/json" },
                    { "Api-Key", "76459ca8-165f-492f-a052-65c9c85071b8" },
                },
                 Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // Send the HTTP request and get the response
            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request);
                return response.StatusCode;
            }
        }

    }
}









