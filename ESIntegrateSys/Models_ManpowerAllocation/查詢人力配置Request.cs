using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESIntegrateSys.Models_ManpowerAllocation
{
    /// <summary>
    /// 查詢人力配置請求物件，用於封裝查詢條件與分頁資訊。
    /// </summary>
    public class 查詢人力配置Request
    {
        /// <summary>
        /// 查詢條件，包含欲查詢的人力配置明細。
        /// </summary>
        public 人力配置明細 查詢條件 { get; set; }

        /// <summary>
        /// 分頁頁碼，從 1 開始。
        /// </summary>
        public int page { get; set; }

        /// <summary>
        /// 每頁筆數。
        /// </summary>
        public int pageSize { get; set; }
    }
}