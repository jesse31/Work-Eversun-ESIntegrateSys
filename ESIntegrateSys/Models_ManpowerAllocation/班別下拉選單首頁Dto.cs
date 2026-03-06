using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESIntegrateSys.Models_ManpowerAllocation
{
    /// <summary>
    /// 班別下拉選單首頁 DTO
    /// </summary>
    public class 班別下拉選單首頁Dto
    {
        /// <summary>
        /// 班別ID
        /// </summary>
        public string shiftID { get; set; }
        /// <summary>
        /// 班別名稱
        /// </summary>
        public string Name { get; set; }
    }
}