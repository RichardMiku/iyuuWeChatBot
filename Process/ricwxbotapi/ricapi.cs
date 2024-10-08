using Newtonsoft.Json;
using ricwxbot.Functions;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace ricwxbot.Process.ricwxbotapi
{
    public class ricapi
    {
        private static string Personal_TOKEN = Environment.GetEnvironmentVariable("LOGIN_API_TOKEN");
        private static string SEND_API_URL = Environment.GetEnvironmentVariable("SEND_API_URL");
        private static string httpUrl = @$"{SEND_API_URL}/webhook/msg/v2?token={Personal_TOKEN}";
        //private readonly ILogger<ricapi> _logger;
        private readonly ILogger<loginfo> _logger;

        //public ricapi(ILogger<ricapi> logger)
        //{
        //    _logger = logger;
        //}

        #region 群消息
        /// <summary>
        /// 多线程发送消息api，防卡顿
        /// </summary>
        /// <param name="msgtype">消息类型</param>
        /// <param name="toUSER">发送对象</param>
        /// <param name="msgcontent">消息内容</param>
        /// <returns></returns>
        public async Task apiSendGroupMessage(string msgtype, string toUSER, string msgcontent)
        {
            var tasks = new[]
            {
                ApiSendGroupMessage(msgtype,true,toUSER,msgcontent),
            };

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                Console.WriteLine($"Message sent successfully: {result}");
            }
        }

        /// <summary>
        /// 源_发送消息api
        /// </summary>
        /// <param name="msgtype">消息类型</param>
        /// <param name="toUSER">发送对象</param>
        /// <param name="msgcontent">消息内容</param>
        /// <returns></returns>
        public async Task<bool> ApiSendGroupMessage(string msgtype, bool isroom, string toUSER, string msgcontent)
        {
            var posturl = httpUrl;
            var databody = new
            {
                to = toUSER,
                isRoom = isroom,
                data = new[]
                    {
                        new { type = msgtype, content = msgcontent },
                    }
            };
            string jsondatabody = JsonConvert.SerializeObject(databody);
            var jsonContent = new StringContent(jsondatabody, Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync(posturl, jsonContent);
                    return response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }

        }
        #endregion

        #region 个人消息
        /// <summary>
        /// 多线程发送消息api，防卡顿
        /// </summary>
        /// <param name="msgtype">消息类型</param>
        /// <param name="toUSER">发送对象</param>
        /// <param name="msgcontent">消息内容</param>
        /// <returns></returns>
        public async Task apiSendMessage(string msgtype, string toUSER, string msgcontent)
        {
            var tasks = new[]
            {
                ApiSendMessage(msgtype, toUSER,msgcontent),
            };

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                Console.WriteLine($"Message sent successfully: {result}");
                Console.WriteLine($"Message Context: {msgcontent}");
            }
        }

        /// <summary>
        /// 源_发送消息api
        /// </summary>
        /// <param name="msgtype">消息类型</param>
        /// <param name="toUSER">发送对象</param>
        /// <param name="msgcontent">消息内容</param>
        /// <returns></returns>
        public async Task<bool> ApiSendMessage(string msgtype, string toUSER, string msgcontent)
        {
            var posturl = httpUrl;
            var databody = new
            {
                to = toUSER,
                data = new[]
                    {
                        new { type = msgtype, content = msgcontent },
                    }
            };
            string jsondatabody = JsonConvert.SerializeObject(databody);
            var jsonContent = new StringContent(jsondatabody, Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync(posturl, jsonContent);
                    return response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }

        }
        #endregion

    }
}
