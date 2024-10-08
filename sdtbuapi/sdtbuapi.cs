using Newtonsoft.Json.Linq;
using ricwxbot.dataWR;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ricwxbot.sdtbuapi
{
    public class sdtbuAPI
    {
        /// <summary>
        /// 从环境变量中获取智慧山商api请求地址
        /// </summary>
        public static string sdtbuapiURL = Environment.GetEnvironmentVariable("ZHSS_API_URL");

        /// <summary>
        /// 获取个人信息
        /// </summary>
        /// <param name="STUID">学号</param>
        /// <returns></returns>
        public static async Task<string> usrINFO(string STUID)
        {
            var client = new HttpClient();
            var requestUrl = $"{sdtbuapiURL}/v1/user_info?uid={STUID}";

            try
            {
                // 发送GET请求
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                // 检查响应状态代码
                if (response.IsSuccessStatusCode)
                {
                    // 读取响应内容
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject keyValuePairs = JObject.Parse(responseBody);//相应内容为json
                    if (keyValuePairs != null)
                    {
                        if ((int)keyValuePairs["code"] == 200)
                        {
                            string USRinfo = $"ℹ个人信息\n" +
                                $"📓学号：{(string)keyValuePairs["data"]["学号"]}\n" +
                                $"🏷姓名：{(string)keyValuePairs["data"]["姓名"]}\n" +
                                $"⚧性别：{(string)keyValuePairs["data"]["性别"]}\n" +
                                $"🏫学院：{(string)keyValuePairs["data"]["学院"]}";
                            return USRinfo;
                        }
                        else
                        {
                            return "获取失败-解析错误！";
                        }
                    }
                }
                else
                {
                    return "获取失败-请求成功但未请求到信息！";
                }
                return "获取失败-发送请求错误！";
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return "获取失败-尝试请求错误！";
            }
        }

        /// <summary>
        /// 智慧山商绑定验证
        /// </summary>
        /// <param name="wxname">微信昵称</param>
        /// <returns></returns>
        public static async Task<bool> bindCHECK(string wxname)
        {
            string JSONfileRead = jsonWR.INFOJdataR(wxname);
            JObject PsdtbuINFO = JObject.Parse(JSONfileRead);
            if (PsdtbuINFO !=null)
            {
                var client = new HttpClient();
                var requestUrl = $"{sdtbuapiURL}/v1/exam_score";

                var data = new
                {
                    username = (string)PsdtbuINFO["SDTBUID"],
                    password = (string)PsdtbuINFO["SDTBUPASSWORD"]
                };
                Console.WriteLine("绑定信息json"+data);


                // 将数据转换为JSON字符串
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    // 发送POST请求
                    HttpResponseMessage response = await client.PostAsync(requestUrl, content);

                    // 检查响应状态代码
                    if (response.IsSuccessStatusCode)
                    {
                        // 读取响应内容
                        string responseBody = await response.Content.ReadAsStringAsync();
                        JObject keyValuePairs = JObject.Parse(responseBody);//相应内容为json
                        Console.WriteLine(responseBody);
                        if (keyValuePairs != null)
                        {
                            if ((int)keyValuePairs["code"] == 200)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        // 处理错误
                        Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                        // 可以进一步处理错误，例如读取错误信息
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Error details: " + errorResponse);
                    }
                    return false;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }
            // JSON 字符串
            

            
        }
    }
}
