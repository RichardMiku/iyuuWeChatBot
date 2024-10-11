using ricwxbot.Functions;
using ricwxbot.Process.ricwxbotapi;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace ricwxbot.Process
{
    public class ProcessMessage
    {
        ricapi api =new ricapi();
        public async Task<object> ProcessMSG(MsgReceive msgReceive,JObject msgSource)
        {
            string msgFROMperson = "";
            string msgFROMgroup = null;
            try
            {
                msgFROMperson = (string)msgSource["from"]["payload"]["name"];
                msgFROMgroup = (string)msgSource["room"]["payload"]["topic"];
            }
            catch (Exception ex)
            {
                //await api.apiSendMessage("text", "##", ex.Message);
            }

            //构造发送文本消息的方法，自动判断发送对象
            async void apisendmsg(string msgtype, string msgcontent)
            {
                if (msgFROMgroup != null)
                {
                    await api.apiSendGroupMessage("text", msgFROMgroup, msgcontent);
                }
                else
                {
                    await api.apiSendMessage("text", msgFROMperson, msgcontent);
                }
            }

            //实现一言功能的消息处理
            if (msgReceive.Content == "一言")
            {
                hitokoto _hitokoto = new hitokoto();
                string hitContent = await _hitokoto.hitokotoGetAsync();
                //if (msgFROMgroup != null)
                //{
                //    await api.apiSendGroupMessage("text", msgFROMgroup, hitContent);
                //}
                //else
                //{
                //    await api.apiSendMessage("text", msgFROMperson, hitContent);
                //}
                apisendmsg("text", hitContent);
            }

            //实现菜单功能的消息处理
            if (msgReceive.Content == "菜单")
            {
                
            }

            return new { Message = "Message processed successfully." };
        }
    }
}
