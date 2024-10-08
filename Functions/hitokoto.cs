using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ricwxbot.Functions
{
    public class hitokoto
    {
        public string hitokotoContent {  get; set; }
        private JObject hitokotoJsonContent;

        /// <summary>
        /// 一言获取
        /// </summary>
        /// <returns></returns>
        public async Task<string> hitokotoGetAsync()
        {
            //var tasks = new[]
            //{
            //    hitokotoGet()
            //};

            //var results = await Task.WhenAll(tasks);

            //foreach (var result in results)
            //{
            //    hitokotoContent = result;
            //    hitokotoJsonContent = JObject.Parse(hitokotoContent);
            //}
            Thread.Sleep(3000);
            hitokotoContent = await hitokotoGet();
            hitokotoJsonContent = JObject.Parse(hitokotoContent);
            string hcontent = (string)hitokotoJsonContent["hitokoto"] + "\n" +
                "作者：" + (string)hitokotoJsonContent["from_who"] + "\n" +
                "出处：" + (string)hitokotoJsonContent["from"];

            return hcontent;
        }

        /// <summary>
        /// 获取一言
        /// </summary>
        /// <returns></returns>
        public async Task<string> hitokotoGet()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    //Thread.Sleep(3000);
                    //await Task.Delay(3000);
                    // 创建一个新的HttpRequestMessage对象
                    var request = new HttpRequestMessage(HttpMethod.Get, "https://v1.hitokoto.cn/");
                    // 设置用户代理
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; .NET4.0C; .NET4.0E; rv:11.0) like Gecko");
                    //获取内容
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        return responseBody;
                    }
                    else
                    {
                        Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                        return "{\"hitokoto\": \"啊哦！获取失败了！\"}";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return "{\"hitokoto\": \"啊哦！获取失败了！\"}";
                }
            }
        }
    }
}
