using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ESIntegrateSys.Models;
using ESIntegrateSys.ViewModels;

namespace ESIntegrateSys.Models_QSchedule
{
    /// <summary>
    /// 產生報表用的 Pie Chart 資料查詢類別，提供取得前五名每日請求比率的功能。
    /// </summary>
    public class IndexPieChart
    {
        private readonly ESIntegrateSysEntities db;

        /// <summary>
        /// 建構子：使用指定的資料庫上下文建立 <see cref="IndexPieChart"/> 實例。
        /// </summary>
        /// <param name="dbContext">ESIntegrateSys 的 Entity Framework 資料庫上下文。</param>
        public IndexPieChart(ESIntegrateSysEntities dbContext)
        {
            db = dbContext;
        }

        /// <summary>
        /// 查詢並回傳用於 Pie Chart 的資料清單（前五筆每日請求比率）。
        /// 結果包含客戶代號、工單屬性、請求日期、該日期的請求數、該日期總請求數及請求比例百分比。
        /// </summary>
        /// <returns>回傳一個 <see cref="List{PieChartData}"/>，代表 Pie Chart 的資料集。</returns>
        public List<PieChartData> PieChart()
        {
            var query = @"
                                WITH DailyRequests AS (
                                    SELECT CustNo, WoNoAttri, CAST(RequDate AS date) AS ReqDate, COUNT(*) AS RequestCount
                                    FROM ES_QuoteForSales	
                                    GROUP BY CustNo, WoNoAttri, CAST(RequDate AS date)
                                ),
                                TotalRequests AS (
                                    SELECT CAST(RequDate AS date) AS ReqDate, COUNT(*) AS TotalRequestCount
                                    FROM ES_QuoteForSales
                                    GROUP BY CAST(RequDate AS date)
                                )
                                SELECT top(5)
                                    d.CustNo,
                                    d.WoNoAttri,
                                    d.ReqDate, 
                                    d.RequestCount, 
                                    t.TotalRequestCount, 
                                    FORMAT((CAST(d.RequestCount AS FLOAT) / t.TotalRequestCount) * 100, 'N2') + '%' AS RequestRatioPercentage
                                FROM DailyRequests d
                                JOIN TotalRequests t ON d.ReqDate = t.ReqDate
                                ORDER BY d.ReqDate, d.CustNo; ";

            var results = db.Database.SqlQuery<PieChartData>(query).ToList();
            return results;
        } 
        




    }
}