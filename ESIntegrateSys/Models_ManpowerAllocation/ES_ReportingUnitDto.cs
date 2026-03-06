using System;

namespace ESIntegrateSys.Models_ManpowerAllocation
{
    /// <summary>
    /// 報工生產單位資料傳輸物件
    /// </summary>
    public class ES_ReportingUnitDto
    {
        /// <summary>
        /// 單位唯一識別碼
        /// </summary>
        public int 單位ID { get; set; }

        /// <summary>
        /// 單位名稱
        /// </summary>
        public string 單位名稱 { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool 啟用 { get; set; }
    }
}
