using System;
using System.Collections.Generic;
using System.Text;

namespace HitRefresh.HitGeneralServices
{
    /// <summary>
    /// 用户的校园角色
    /// </summary>
    public enum HitRole
    {
        /// <summary>
        /// 不是校园的成员
        /// </summary>
        NotMember = 0,
        /// <summary>
        /// 本科生
        /// </summary>
        Undergraduate = 1,
        /// <summary>
        /// 留学生
        /// </summary>
        InternationalStudent,
        /// <summary>
        /// 硕士
        /// </summary>
        Master = 2,
        /// <summary>
        /// 博士
        /// </summary>
        Doctor = 3,
        /// <summary>
        /// 老师(员工)
        /// </summary>
        Teacher = 4,
        /// <summary>
        /// 其它，未识别的
        /// </summary>
        Else = 5
    }
}
