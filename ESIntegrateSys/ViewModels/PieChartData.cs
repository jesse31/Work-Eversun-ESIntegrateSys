using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESIntegrateSys.ViewModels
{
    /// <summary>
    /// Pie 圖表資料的檢視模型，包含客戶、工單屬性、請求日期與請求數量等欄位。
    /// </summary>
    public class PieChartData
    {
        /// <summary>
        /// 客戶代號
        /// </summary>
        public string CustNo { get; set; }

        /// <summary>
        /// 工單屬性
        /// </summary>
        public int WoNoAttri { get; set; }

        /// <summary>
        /// 請求日期（不含時間部分）
        /// </summary>
        public DateTime ReqDate { get; set; }

        /// <summary>
        /// 該客戶在該日的請求數
        /// </summary>
        public int RequestCount { get; set; }

        /// <summary>
        /// 該日的總請求數
        /// </summary>
        public int TotalRequestCount { get; set; }

        /// <summary>
        /// 請求比例（格式為百分比字串，例如 "12.34%"）
        /// </summary>
        public string RequestRatioPercentage { get; set; }
    }

}