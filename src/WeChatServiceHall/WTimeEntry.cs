using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HitRefresh.HitGeneralServices.WeChatServiceHall
{
    /// <summary>
    /// 用于解析微信课表条目的记录(事件记录)
    /// </summary>
    public record WTimeEntry
    {
        /// <summary>
        /// 对应的日期
        /// </summary>
        [JsonPropertyName("rq")]
        public string Date { get; set; }
        /// <summary>
        /// 星期，1-7的整数
        /// </summary>
        [JsonPropertyName("xqj")]
        public string DayOfWeek { get; set; }
    }
}
