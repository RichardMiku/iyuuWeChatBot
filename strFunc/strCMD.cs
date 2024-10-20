namespace ricwxbot.strFunc
{
    public class strCMD
    {
        /// <summary>
        /// 判断消息是否为菜单命令
        /// </summary>
        /// <param name="CMDmenu">命令内容</param>
        /// <returns></returns>
        public static bool strCMDmenu(string CMDmenu)
        {
            bool isCMD = CMDmenu == "菜单" || CMDmenu == "/menu"
                || CMDmenu == "帮助" || CMDmenu == "/help"
                || CMDmenu == "命令" || CMDmenu == "/cmd"
                || CMDmenu == "功能";
            return isCMD;
        }

        /// <summary>
        /// 判断消息是否为课表查询命令
        /// </summary>
        /// <param name="CMDmenu">命令内容</param>
        /// <returns></returns>
        public static bool strCMDclass(string CMDmenu)
        {
            bool isCMD = CMDmenu == "课表查询" || CMDmenu == "/class"
                || CMDmenu == "课程查询" || CMDmenu == "/course"
                || CMDmenu == "课程表查询" || CMDmenu == "/timetable"
                ||CMDmenu== "本周课表" || CMDmenu == "/thisweek"
                || CMDmenu == "我的课表" || CMDmenu == "/myclass";
            return isCMD;
        }

        /// <summary>
        /// 判断消息是否为下节课表查询命令
        /// </summary>
        /// <param name="CMDmenu">命令内容</param>
        /// <returns></returns>
        public static bool strCMDNextClass(string CMDmenu)
        {
            bool isCMD = CMDmenu == "下节课" || CMDmenu == "/nextclass"
                || CMDmenu == "下一节课" || CMDmenu == "/nextcourse"
                || CMDmenu == "下节课程" 
                || CMDmenu == "下一节课程" || CMDmenu == "/nextcourse";
            return isCMD;
        }
    }
}
