using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESIntegrateSys.Models_ManpowerAllocation
{
    /// <summary>
    /// 報工生產單位下拉選單 DTO
    /// </summary>
    public class 報工生產單位下拉選單Dto
    {
        /// <summary>
        /// 報工生產單位 GUID
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 報工生產單位名稱
        /// </summary>
        public string Name { get; set; }
    }
}