using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace ricwxbot.dataWR
{
    public class jsonWR
    {
        public class userINFO
        {
            public string WXNAME { get; set; }
            public string SDTBUID { get; set; }
            public string SDTBUPASSWORD {  get; set; }
        }

        /// <summary>
        /// 写入用户智慧山商信息
        /// </summary>
        /// <param name="wxname">微信昵称</param>
        /// <param name="sdtbu_ID">智慧山商账号</param>
        /// <param name="sdtbu_PASSWD">智慧山商密码</param>
        public static void INFOJdataW(string wxname,string sdtbu_ID,string sdtbu_PASSWD)
        {
            // 创建一个Person对象
            userINFO usrI = new userINFO 
            {
                WXNAME = wxname,
                SDTBUID = sdtbu_ID,
                SDTBUPASSWORD = sdtbu_PASSWD
            };

            // 将对象序列化为JSON字符串
            string json = JsonConvert.SerializeObject(usrI, Formatting.Indented);

            // 将JSON字符串写入文件
            File.WriteAllText($"./usrdata/{wxname}.json", json);
        }

        /// <summary>
        /// 检查用户信息文件是否存在并读取文件
        /// </summary>
        /// <param name="wxname">微信昵称</param>
        /// <returns></returns>
        public static string INFOJdataR(string wxname)
        {
            // 指定要检查的文件路径
            string filePath = $"./usrdata/{wxname}.json";

            // 检查文件是否存在
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string content = reader.ReadToEnd(); // 读取整个文件
                    return content;
                }
            }
            else
            {
                string content = "您未绑定智慧山商，请先绑定！";
                return content;
            }
        }

        /// <summary>
        /// 绑定智慧山商
        /// </summary>
        /// <param name="wxname">微信昵称</param>
        /// <param name="bindMSG">绑定命令</param>
        /// <returns></returns>
        public static string bindSDTBU(string wxname,string bindMSG)
        {
            string input = bindMSG;
            // 正则表达式解释：
            // "绑定" - 匹配命令 "绑定"
            // @"\s+" - 匹配一个或多个空白字符
            // "(\p{L}+)" - 匹配一个或多个字母字符（包括中文）
            // @"\s+" - 再次匹配一个或多个空白字符
            // "(\p{L}+)" - 再次匹配一个或多个字母字符（包括中文）
            Regex regex = new Regex(@"信息绑定\s+([\p{L}\p{N}\p{P}\p{S}]+)\s+([\p{L}\p{N}\p{P}\p{S}]+)");
            Match match = regex.Match(input);

            if (match.Success)
            {
                string SDTBUaccount = match.Groups[1].Value; // 第一个参数 "123"
                string SDTBUpasswd = match.Groups[2].Value; // 第二个参数 "abc"
                INFOJdataW(wxname, SDTBUaccount, SDTBUpasswd);//写入智慧山商信息
                if (INFOcheck(wxname))
                {
                    return "绑定成功！";
                }
                else
                {
                    return "绑定失败";
                }
            }
            else
            {
                return "可能存在错误的输入信息";
            }
        }

        /// <summary>
        /// 判断消息开头是否为信息绑定
        /// </summary>
        /// <param name="msgcontent"></param>
        /// <returns></returns>
        public static bool BindRegex(string msgcontent)
        {
            string input = msgcontent;
            // 正则表达式解释：
            // "绑定" - 匹配命令 "绑定"
            // @"\s+" - 匹配一个或多个空白字符
            // "(\p{L}+)" - 匹配一个或多个字母字符（包括中文）
            // @"\s+" - 再次匹配一个或多个空白字符
            // "(\p{L}+)" - 再次匹配一个或多个字母字符（包括中文）
            Regex regex = new Regex(@"信息绑定\s+([\p{L}\p{N}\p{P}\p{S}]+)\s+([\p{L}\p{N}\p{P}\p{S}]+)");
            Match match = regex.Match(input);
            if (match.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public static bool INFOcheck(string fileName)
        {
            // 指定要检查的文件路径
            string filePath = $"./usrdata/{fileName}.json";

            // 检查文件是否存在
            if (File.Exists(filePath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 创建数据文件夹
        /// </summary>
        public static void datafolder()
        {
            // 指定要创建的文件夹的路径
            string folderPath = @"./usrdata";

            // 检查文件夹是否已存在
            if (!Directory.Exists(folderPath))
            {
                // 使用Directory.CreateDirectory方法创建文件夹
                Directory.CreateDirectory(folderPath);
                Console.WriteLine("文件夹已创建: " + folderPath);
            }
            else
            {
                Console.WriteLine("文件夹已存在: " + folderPath);
            }
        }
    }
}
