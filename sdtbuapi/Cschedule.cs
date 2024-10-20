using Newtonsoft.Json.Linq;
using ricwxbot.dataWR;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ricwxbot.sdtbuapi
{
    public class Cschedule
    {
        /// <summary>
        /// 从环境变量中获取智慧山商api请求地址
        /// </summary>
        public static string sdtbuapiURL = Environment.GetEnvironmentVariable("ZHSS_API_URL");

        /// <summary>
        /// api获取课表信息
        /// </summary>
        /// <param name="wxname">微信昵称</param>
        /// <returns></returns>
        public static async Task<string[]> GetCourseSchedule(string wxname)
        {
            
            string JSONfileRead = jsonWR.INFOJdataR(wxname);
            JObject PsdtbuINFO = JObject.Parse(JSONfileRead);
            if (PsdtbuINFO != null)
            {
                var client = new HttpClient();
                var requestUrl = $"{sdtbuapiURL}/v1/class_schedule";

                var data = new
                {
                    username = (string)PsdtbuINFO["SDTBUID"],
                    password = (string)PsdtbuINFO["SDTBUPASSWORD"]
                };
                Console.WriteLine("获取课表信息json" + data);


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
                                //return ScheduleAnalyze(responseBody);
                                return ScheduleAnalyzeWeek(responseBody);
                            }
                            else
                            {
                                string[] result_false = { "获取失败，状态码不为200" };
                                Console.WriteLine(result_false);
                                return result_false;
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
                    string[] result_fl = { "获取失败，api响应错误" };
                    return result_fl;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                    string[] result_postfl = { "获取失败，post请求错误" };
                    Console.WriteLine(result_postfl);
                    return result_postfl;
                }
            }
            else
            {
                string[] result_INFOfl = { "获取失败，信息不存在！" };
                Console.WriteLine(result_INFOfl);
                return result_INFOfl;
            }
            // JSON 字符串



        }

        /// <summary>
        /// api获取下一节课信息
        /// </summary>
        /// <param name="wxname"></param>
        /// <returns></returns>
        public static async Task<string[]> GetCourseScheduleNext(string wxname)
        {

            string JSONfileRead = jsonWR.INFOJdataR(wxname);
            JObject PsdtbuINFO = JObject.Parse(JSONfileRead);
            if (PsdtbuINFO != null)
            {
                var client = new HttpClient();
                var requestUrl = $"{sdtbuapiURL}/v1/class_schedule";

                var data = new
                {
                    username = (string)PsdtbuINFO["SDTBUID"],
                    password = (string)PsdtbuINFO["SDTBUPASSWORD"]
                };
                Console.WriteLine("获取课表信息json" + data);


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
                                //return ScheduleAnalyze(responseBody);
                                return ScheduleAnalyzeWhatNext(responseBody);//课程解析接口
                            }
                            else
                            {
                                string[] result_false = { "获取失败，状态码不为200" };
                                Console.WriteLine(result_false);
                                return result_false;
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
                    string[] result_fl = { "获取失败，api响应错误" };
                    return result_fl;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                    string[] result_postfl = { "获取失败，post请求错误" };
                    Console.WriteLine(result_postfl);
                    return result_postfl;
                }
            }
            else
            {
                string[] result_INFOfl = { "获取失败，信息不存在！" };
                Console.WriteLine(result_INFOfl);
                return result_INFOfl;
            }
            // JSON 字符串



        }


        /// <summary>
        /// 解析获取的课表数据-课程详情式
        /// </summary>
        /// <param name="Class_json">课表数据</param>
        public static string[] ScheduleAnalyze(string Class_json)
        {
            string json = Class_json;
            List<string> rdata = new List<string>();
            JObject jsonObject = JObject.Parse(json);

            int code = (int)jsonObject["code"];
            string msg = (string)jsonObject["msg"];

            if (code == 200)
            {
                JArray coursesArray = (JArray)jsonObject["data"];
                foreach (JObject course in coursesArray)
                {
                    string courseName = (string)course["课程名称"];
                    string teacherName = (string)course["教师姓名"];
                    int startTime = (int)course["起始时间"];
                    int endTime = (int)course["结束时间"];
                    string location = (string)course["上课地点"];
                    int weekDay = (int)course["上课星期"];
                    rdata.Add($"课程名称: {courseName}\n教师姓名: {teacherName}\n起始时间: {startTime}\n结束时间: {endTime}\n上课地点: {location}\n上课星期: {weekDay}");
                    Console.WriteLine($"课程名称: {courseName}, 教师姓名: {teacherName}, 起始时间: {startTime}, 结束时间: {endTime}, 上课地点: {location}, 上课星期: {weekDay}");
                    
                }
                return rdata.ToArray();
            }
            else
            {
                string[] nfalse = { "课程解析错误！" };
                Console.WriteLine("无法获取课程数据或发生错误: " + msg);
                return nfalse;
            }
        }

        /// <summary>
        /// 解析获取的课表数据-周模式-去除课程详细信息，只保留上课星期
        /// </summary>
        /// <param name="Class_json">课表数据</param>
        /// <returns></returns>
        public static string[] ScheduleAnalyzeWeek(string Class_json)
        {
            string json = Class_json;
            List<string> rdata = new List<string>();
            JObject jsonObject = JObject.Parse(json);

            int code = (int)jsonObject["code"];
            string msg = (string)jsonObject["msg"];

            if (code == 200)
            {
                JArray coursesArray = (JArray)jsonObject["data"];

                string weekMonday = "星期一：\n";
                string weekTuesday = "星期二：\n";
                string weekWednesday = "星期三：\n";
                string weekThursday = "星期四：\n";
                string weekFriday = "星期五：\n";
                string weekSaturday = "星期六：\n";
                string weekSunday = "星期日：\n";

                foreach (JObject course in coursesArray)
                {
                    string courseName = (string)course["课程名称"];
                    int startTime = (int)course["起始时间"];
                    int endTime = (int)course["结束时间"];
                    string location = (string)course["上课地点"];
                    int weekDay = (int)course["上课星期"];

                    // 使用 switch 语句根据 weekDay 执行不同的操作
                    switch (weekDay)
                    {
                        case 1:
                            weekMonday += $"课程名称: {courseName}\n起始时间: {startTime}\n结束时间: {endTime}\n上课地点: {location}\n\n";
                            break;
                        case 2:
                            weekTuesday += $"课程名称: {courseName}\n起始时间: {startTime}\n结束时间: {endTime}\n上课地点: {location}\n\n";
                            break;
                        case 3:
                            weekWednesday += $"课程名称: {courseName}\n起始时间: {startTime}\n结束时间: {endTime}\n上课地点: {location}\n\n";
                            break;
                        case 4:
                            weekThursday += $"课程名称: {courseName}\n起始时间: {startTime}\n结束时间: {endTime}\n上课地点: {location}\n\n";
                            break;
                        case 5:
                            weekFriday += $"课程名称: {courseName}\n起始时间: {startTime}\n结束时间: {endTime}\n上课地点: {location}\n\n";
                            break;
                        case 6:
                            weekSaturday += $"课程名称: {courseName}\n起始时间: {startTime}\n结束时间: {endTime}\n上课地点: {location}\n\n";
                            break;
                        case 7:
                            weekSunday += $"课程名称: {courseName}\n起始时间: {startTime}\n结束时间: {endTime}\n上课地点: {location}\n\n";
                            break;
                        default:
                            break;
                    }

                    Console.WriteLine($"课程名称: {courseName}, 起始时间: {startTime}, 结束时间: {endTime}, 上课地点: {location}, 上课星期: {weekDay}\n");
                }

                rdata.Add("📖本周课程表📖");
                rdata.Add(weekMonday);
                rdata.Add(weekTuesday);
                rdata.Add(weekWednesday);
                rdata.Add(weekThursday);
                rdata.Add(weekFriday);
                rdata.Add(weekSaturday);
                rdata.Add(weekSunday);

                return rdata.ToArray();
            }
            else
            {
                string[] nfalse = { "课程解析错误！" };
                Console.WriteLine("无法获取课程数据或发生错误: " + msg);
                return nfalse;
            }
        }

        /// <summary>
        /// 解析获取的课表数据-下一节课信息
        /// </summary>
        /// <param name="Class_json">课程数据json</param>
        /// <returns></returns>
        public static string[] ScheduleAnalyzeWhatNext(string Class_json)
        {
            string json = Class_json;
            List<string> rdata = new List<string>();
            JObject jsonObject = JObject.Parse(json);

            int code = (int)jsonObject["code"];
            string msg = (string)jsonObject["msg"];

            if (code == 200)
            {
                JArray coursesArray = (JArray)jsonObject["data"];
                DateTime now = DateTime.Now;
                JObject nextCourse = null;

                foreach (JObject course in coursesArray)
                {
                    string courseName = (string)course["课程名称"];
                    string teacherName = (string)course["教师姓名"];
                    int startTime = (int)course["起始时间"];
                    int endTime = (int)course["结束时间"];
                    string location = (string)course["上课地点"];
                    int weekDay = (int)course["上课星期"];

                    // 获取课程的实际时间
                    (DateTime start, DateTime end) = GetRealTime(startTime, endTime, location);

                    // 判断当前时间是否在课程时间之前
                    if (now < start)
                    {
                        if (nextCourse == null || start < (DateTime)nextCourse["start"])
                        {
                            nextCourse = new JObject
                            {
                                ["课程名称"] = courseName,
                                ["教师姓名"] = teacherName,
                                ["起始时间"] = start,
                                ["结束时间"] = end,
                                ["上课地点"] = location,
                                ["上课星期"] = weekDay
                            };
                        }
                    }
                }

                if (nextCourse != null)
                {
                    rdata.Add($"下一节课：\n课程名称: {nextCourse["课程名称"]}\n教师姓名: {nextCourse["教师姓名"]}\n起始时间: {nextCourse["起始时间"]:HH:mm}\n结束时间: {nextCourse["结束时间"]:HH:mm}\n上课地点: {nextCourse["上课地点"]}\n上课星期: {nextCourse["上课星期"]}");
                }
                else
                {
                    rdata.Add("今天没有更多的课程了。");
                }

                return rdata.ToArray();
            }
            else
            {
                string[] nfalse = { "课程解析错误！" };
                Console.WriteLine("无法获取课程数据或发生错误: " + msg);
                return nfalse;
            }
        }

        /// <summary>
        /// 获取课程的实际时间，根据课程的起始时间、结束时间和上课地点
        /// </summary>
        /// <param name="startTime">起始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="location">上课地点</param>
        /// <returns></returns>
        private static (DateTime start, DateTime end) GetRealTime(int startTime, int endTime, string location)
        {
            DateTime start = DateTime.MinValue;
            DateTime end = DateTime.MinValue;

            // 解析教室信息
            bool isSpecialLocation = false;
            if (location.StartsWith("东校"))
            {
                string roomNumber = location.Substring(2); // 去掉“东校”前缀
                if (roomNumber.Length == 4)
                {
                    char building = roomNumber[0];
                    if (building == '3' || building == '5')
                    {
                        isSpecialLocation = true;
                    }
                }
            }
            else if (location.StartsWith("西校"))
            {
                string roomNumber = location.Substring(2); // 去掉“西校”前缀
                if (roomNumber.Length == 4)
                {
                    char building = roomNumber[0];
                    if (building == '1' || building == '2')
                    {
                        isSpecialLocation = true;
                    }
                }
            }

            switch (startTime)
            {
                case 1:
                    start = DateTime.Today.AddHours(7).AddMinutes(50);
                    end = DateTime.Today.AddHours(9).AddMinutes(20);
                    break;
                case 3:
                    start = DateTime.Today.AddHours(isSpecialLocation ? 10 : 9).AddMinutes(isSpecialLocation ? 10 : 50);
                    end = DateTime.Today.AddHours(isSpecialLocation ? 11 : 11).AddMinutes(isSpecialLocation ? 40 : 10);
                    break;
                case 5:
                    start = DateTime.Today.AddHours(14).AddMinutes(0);
                    end = DateTime.Today.AddHours(15).AddMinutes(30);
                    break;
                case 7:
                    start = DateTime.Today.AddHours(isSpecialLocation ? 16 : 15).AddMinutes(isSpecialLocation ? 20 : 50);
                    end = DateTime.Today.AddHours(isSpecialLocation ? 17 : 17).AddMinutes(isSpecialLocation ? 50 : 20);
                    break;
                case 9:
                    start = DateTime.Today.AddHours(19).AddMinutes(10);
                    end = DateTime.Today.AddHours(20).AddMinutes(40);
                    break;
            }

            return (start, end);
        }



        
        
    }
}
