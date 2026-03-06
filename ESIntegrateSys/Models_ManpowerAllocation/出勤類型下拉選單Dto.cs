using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESIntegrateSys.Models_ManpowerAllocation
{
    public class 出勤類型下拉選單Dto
    {
        /// <summary>
        /// 出勤類型 GUID
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 出勤名稱
        /// </summary>
        public string Name { get; set; }
    }
}