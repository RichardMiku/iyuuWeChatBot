using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ricwxbot.Process;
using System.Reflection;
using Newtonsoft.Json.Linq;
using ricwxbot.Functions;
using ricwxbot.strFunc;
using ricwxbot.dataWR;
using ricwxbot.sdtbuapi;

namespace ricwxbot.Controllers
{
    [ApiController]
    [Route("msgreceive")]
    public class MsgReceiveController : ControllerBase
    {
        private readonly static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        /// <summary>
        /// 用于接收消息的api
        /// </summary>
        /// <param name="msgReceive"></param>
        /// <returns>...</returns>
        /// <remarks>
        /// 用于接收wxbot消息的api，接收并处理消息
        /// </remarks>
        [HttpPost(Name = "ReceiveMSG")]
        public async Task<IActionResult> Post([FromForm] MsgReceive msgReceive)
        {
            //消息处理器
            //await _semaphore.WaitAsync();
            //try
            //{
            //    ProcessMessage prcMSG = new ProcessMessage();
            //    Thread.Sleep(3000);
            //    await prcMSG.ProcessMSG(msgReceive, JObject.Parse(msgReceive.Source));
            //}
            //finally
            //{
            //    _semaphore.Release();
            //}
            // 处理接收到的消息
            // 例如：保存到数据库或进行其他处理

            //功能-一言
            if (msgReceive.Content == "一言")
            {
                hitokoto _hitokoto = new hitokoto();
                string hitContent = await _hitokoto.hitokotoGetAsync();

                var response = new
                {
                    success = true,
                    data = new[]
                    {
                        new { type = "text", content = hitContent },
                    }
                };

                return new JsonResult(response);

            }

            if (msgReceive.Content == "你好")
            {
                var response = new
                {
                    success = true,
                    data = new[]
                    {
                        new { type = "text", content = msgReceive.Type },
                        new { type = "text", content = msgReceive.Content },
                        new { type = "text", content = msgReceive.Source },
                        new { type = "text", content = msgReceive.IsMentioned },
                        new { type = "text", content = msgReceive.IsMsgFromSelf }
                    }
                };

                return new JsonResult(response);
            }


            //菜单-主菜单
            if (msgReceive.Content == "菜单")
            {
                var response = new
                {
                    success = true,
                    data = new { type = "text", content = strMenu.menuTEXT() }
                };

                return new JsonResult(response);
                //return strMenu.strMENUProc(strMenu.menuTEXT());
            }

            //菜单-智慧山商
            if (msgReceive.Content == "智慧山商")
            {
                var response = new
                {
                    success = true,
                    data = new { type = "text", content = strMenu.smtSDTBU() }
                };

                return new JsonResult(response);
                //return strMenu.strMENUProc(strMenu.menuTEXT());
            }

            //菜单-功能实现
            if (msgReceive.Content == "功能信息")
            {
                var response = new
                {
                    success = true,
                    data = new { type = "text", content = strMenu.FuncDEVINFO() }
                };

                return new JsonResult(response);
                //return strMenu.strMENUProc(strMenu.menuTEXT());
            }

            //菜单-账号绑定-绑定说明
            if (msgReceive.Content == "账号绑定")
            {
                var response = new
                {
                    success = true,
                    data = new { type = "text", content = strMenu.INFObindMSG() }
                };

                return new JsonResult(response);
                //return strMenu.strMENUProc(strMenu.menuTEXT());
            }

            //功能-智慧山商-绑定
            if (jsonWR.BindRegex(msgReceive.Content))
            {
                JObject PerSource = JObject.Parse(msgReceive.Source);
                string PerWXname = (string)PerSource["from"]["payload"]["name"];
                jsonWR.bindSDTBU(PerWXname, msgReceive.Content);
                //if (jsonWR.INFOcheck(PerWXname))
                //{
                //    var response = new
                //    {
                //        success = true,
                //        data = new[]
                //        {
                //            new { type = "text", content = "绑定成功！" },
                //            new { type = "text", content = "您的绑定信息为"+jsonWR.INFOJdataR(PerWXname) },
                //        }
                //    };
                //    return new JsonResult(response);
                //}
                bool chEck = await sdtbuAPI.bindCHECK(PerWXname);
                if (chEck)
                {
                    var response = new
                    {
                        success = true,
                        data = new[]
                        {
                            new { type = "text", content = "绑定成功！" },
                            new { type = "text", content = "您的绑定信息为"+jsonWR.INFOJdataR(PerWXname) },
                        }
                    };
                    return new JsonResult(response);
                }
                else
                {
                    var response = new
                    {
                        success = true,
                        data = new { type = "text", content = "绑定失败!" }
                    };
                    return new JsonResult(response);
                }
            }

            //功能-智慧山商-个人信息
            if (msgReceive.Content == "个人信息")
            {
                JObject PerSource = JObject.Parse(msgReceive.Source);
                string PerWXname = (string)PerSource["from"]["payload"]["name"];
                //string responseSTR = jsonWR.INFOJdataR(PerWXname);
                if (jsonWR.INFOcheck(PerWXname))
                {
                    JObject STUID = JObject.Parse(jsonWR.INFOJdataR(PerWXname));
                    string USRINFO = await sdtbuAPI.usrINFO((string)STUID["SDTBUID"]);
                    var response = new
                    {
                        success = true,
                        data = new { type = "text", content = USRINFO  }
                    };
                    return new JsonResult(response);
                }
                else
                {
                    var response = new
                    {
                        success = true,
                        data = new { type = "text", content = "您未绑定智慧山商，请先绑定！" }
                    };
                    return new JsonResult(response);
                }
            }

            //功能-智慧山商-课表查询
            if (msgReceive.Content == "课表查询") 
            {
                JObject PerSource = JObject.Parse(msgReceive.Source);
                string PerWXname = (string)PerSource["from"]["payload"]["name"];
                if (jsonWR.INFOcheck(PerWXname))
                {
                    var data = new List<object>();
                    string[] coursesArray = await Cschedule.GetCourseSchedule(PerWXname);
                    foreach (string course in coursesArray)
                    {
                        // 创建课程信息的匿名对象，并添加到data列表中
                        var courseInfo = new
                        {
                            type = "text",
                            content = course
                        };
                        data.Add(courseInfo);
                    }
                    var response = new
                    {
                        success = true,
                        data = data.ToArray() // 将列表转换为数组
                    };

                    return new JsonResult(response);
                }
                else
                {
                    var response = new
                    {
                        success = true,
                        data = new { type = "text", content = "您未绑定智慧山商，请先绑定！" }
                    };
                    return new JsonResult(response);
                }
                
            }






            return Ok(new { message = "Message received successfully", msgReceive });
        }
    }
}
