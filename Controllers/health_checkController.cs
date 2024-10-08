using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ricwxbot.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class health_checkController : ControllerBase
    {
        /// <summary>
        /// 机器人启动前的健康检查，使机器人能正常连接收消息api
        /// </summary>
        /// <returns>successful</returns>
        [HttpGet(Name = "healthcheck")]
        public IActionResult Get()
        {
            return Ok("Successful");
        }
    }
}
