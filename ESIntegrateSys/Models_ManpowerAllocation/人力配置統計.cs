using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESIntegrateSys.Models_ManpowerAllocation
{
    /// <summary>
    /// 人力配置的統計資料模型，包含各類出勤人數與備註欄位。
    /// </summary>
    public class 人力配置統計
    {
        /// <summary>生產單位名稱（顯示用）</summary>
        public string str生產單位 { get; set; }

        /// <summary>班別名稱（例如：早班、晚班）</summary>
        public string str班別名稱 { get; set; }

        /// <summary>間接出勤人數</summary>
        public int int間接出勤人數 { get; set; }

        /// <summary>線外出勤人數</summary>
        public int int線外出勤人數 { get; set; }

        /// <summary>直接出勤人數</summary>
        public int int直接出勤人數 { get; set; }

        /// <summary>留停人數（含請假、休假等狀態）</summary>
        public int int留停人數 { get; set; }

        /// <summary>備註欄位</summary>
        public string str備註 { get; set; }
    }
}