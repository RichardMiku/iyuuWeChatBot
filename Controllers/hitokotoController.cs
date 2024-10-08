using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ricwxbot.Functions;

namespace ricwxbot.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class hitokotoController : ControllerBase
    {
        /// <summary>
        /// 获取hitokoto一言
        /// </summary>
        /// <returns>hitokoto一言</returns>
        /// <response code="200">
        /// {
        /// "id": 6989,
        /// "uuid": "3479e221-297f-429f-bada-b7e4bcb2e535",
        /// "hitokoto": "很多人在喧嚣声中登场，也有少数人在静默中退出。",
        /// "type": "d",
        /// "from": "单独中的洞见2，作家出版社",
        /// "from_who": "张方宇",
        /// "creator": "南婷婷",
        /// "creator_uid": 8367,
        /// "reviewer": 1044,
        /// "commit_from": "web",
        /// "created_at": "1611846976",
        /// "length": 23
        /// }
    /// </response>
    [HttpGet(Name ="hitokoto")]
        public async Task<IActionResult> Get()
        {
            hitokoto _hitokoto = new hitokoto();
            string result = await _hitokoto.hitokotoGetAsync();
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
