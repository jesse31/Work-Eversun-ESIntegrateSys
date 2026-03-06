using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESIntegrateSys.Models_ManpowerAllocation
{
    /// <summary>
    /// 班別下拉選單 DTO
    /// </summary>
    public class 班別下拉選單Dto
    {
        /// <summary>
        /// 班別 GUID
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 班別名稱
        /// </summary>
        public string Name { get; set; }
    }
}