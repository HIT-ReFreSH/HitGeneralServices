using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HitRefresh.HitGeneralServices
{
    /// <summary>
    /// 获取用户校园信息的选项
    /// </summary>
    public class HitInfoProviderOptions
    {
        /// <summary>
        /// 用于存放Id的Claim的Type，默认为<see cref="ClaimTypes.NameIdentifier"/>
        /// </summary>
        public string IdClaimType { get; set; } = ClaimTypes.NameIdentifier;
        /// <summary>
        /// 用于存放姓名的Claim的Type，默认为<see cref="ClaimTypes.Name"/>
        /// </summary>
        public string NameClaimType { get; set; } = ClaimTypes.Name;
        /// <summary>
        /// 用于存放认证方式的Claim的Type，默认为<see cref="ClaimTypes.AuthenticationMethod"/>
        /// </summary>
        public string AuthMethodClaimType { get; set; } = ClaimTypes.AuthenticationMethod;
    }
}
