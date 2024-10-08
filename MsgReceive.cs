namespace ricwxbot
{
    public class MsgReceive
    {
        // 功能类型
        /// <summary>
        /// 消息类型
        /// </summary>
        /// <example>text</example>
        public string Type { get; set; }

        // 传输的内容，可以是文本或文件
        // 使用object类型来支持String和Binary数据类型
        /// <summary>
        /// 传输的内容, 文本或传输的文件共用这个字段
        /// </summary>
        /// <example>你好</example>
        public string Content { get; set; }

        // 消息的相关发送方数据，JSON字符串
        /// <summary>
        /// 消息的相关发送方数据, JSON String
        /// </summary>
        /// <example>来源信息</example>
        public string Source { get; set; }

        // 该消息是否@我的消息
        /// <summary>
        /// 该消息是否@我的消息
        /// </summary>
        /// <example>0</example>
        public string IsMentioned { get; set; }

        // 是否是来自自己的消息
        /// <summary>
        /// 是否是来自自己的消息
        /// </summary>
        /// <example>0</example>
        public string IsMsgFromSelf { get; set; }

        //public IFormFile File { get; set; }
    }
}
