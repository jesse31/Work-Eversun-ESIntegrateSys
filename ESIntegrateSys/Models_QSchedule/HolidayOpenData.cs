using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESIntegrateSys.Models_QSchedule
{
    /// <summary>
    /// 假日資料與工作日計算工具。提供以假日清單為基礎的工作日計算方法。
    /// </summary>
    public class HolidayOpenData
    {
        /// <summary>
        /// 單一日的假日資料。
        /// </summary>
        public class HolidayData
        {
            /// <summary>
            /// 日期，格式為 yyyyMMdd（例如：20250206）。
            /// </summary>
            public string Date { get; set; }
            /// <summary>
            /// 星期文字描述（例如：星期一）。
            /// </summary>
            public string Week { get; set; }
            /// <summary>
            /// 是否為假日。true 表示是假日，false 表示非假日。
            /// </summary>
            public bool IsHoliday { get; set; }
            /// <summary>
            /// 假日的描述或備註（例如：連假名稱）。
            /// </summary>
            public string Description { get; set; }
        }
        /// <summary>
        /// 取得從指定基準日加上若干天後，且跳過假日的下一個工作日。
        /// 會檢查提供的假日清單（HolidayData），以 Date 屬性（yyyyMMdd）與 IsHoliday 標記判斷是否為假日；若目標日為假日則繼續往後推算。
        /// </summary>
        /// <param name="baseDate">起始基準日期。</param>
        /// <param name="holidays">假日清單，HolidayData.Date 格式為 yyyyMMdd；以此判斷是否為假日。</param>
        /// <param name="daysToAdd">要加上的天數（可為正整數）。</param>
        /// <returns>回傳計算後的下一個工作日（DateTime）。</returns>
        public static DateTime GetNextWorkingDay(DateTime baseDate, List<HolidayData> holidays, int daysToAdd)
        {
            // 加上N天
            DateTime targetDate = baseDate.AddDays(daysToAdd);

            // 不斷檢查目標日期是否為假日，若是則追加一天
            bool isHoliday = true;
            while (isHoliday)
            {
                // 查找目標日期是否為假日
                var holiday = holidays.FirstOrDefault(h => h.Date == targetDate.ToString("yyyyMMdd"));

                // 如果是空值或不是假日，則結束循環
                if (holiday == null || !holiday.IsHoliday)
                {
                    isHoliday = false;
                }
                else
                {
                    // 如果是假日，則目標日期增加一天
                    targetDate = targetDate.AddDays(1);
                }
            }

            return targetDate;
        }

    }
}