using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESIntegrateSys.Models_QSchedule
{
    /// <summary>
    /// 台灣假日判斷工具類別。
    /// </summary>
    public class TWHoliday
    {
        /// <summary>
        /// 判斷指定日期是否為假日（含固定節日）。
        /// </summary>
        /// <param name="date">要判斷的日期（DateTime）。</param>
        /// <returns>若為假日則回傳 <c>true</c>，否則回傳 <c>false</c>。</returns>
        public bool IsHoliday(DateTime date)
        {
            // 固定的假日列表 (可以根據需要添加假日)
            List<DateTime> holidays = new List<DateTime>
            {
                new DateTime(date.Year, 1, 1),  // 元旦
                new DateTime(date.Year, 12, 25), // 聖誕節
                // 其他固定假日
            };

            return holidays.Contains(date.Date);
        }

    }
}