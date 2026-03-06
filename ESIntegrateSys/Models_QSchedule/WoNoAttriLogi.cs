using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using ESIntegrateSys.Models;

namespace ESIntegrateSys.Models_QSchedule
{
    /// <summary>
    /// 依據業務填寫的需求日期、填寫時間排序，由需求日期及工單性質判斷
    /// 1. 工單性質為標準工單，同日期可安排10個機種，超過10個機種告警業務主管
    ///2. 工單性質為ECN預估或ECN實測，同日期可安排5個機種，超過5個機種告警業務主管
    ///3. 工單性質為預估工時，同日期可安排5個機種，超過5個機種告警業務主管
    ///1	標準工單  ;2	預估工時 ; 3	ECN預估  ;4	ECN實測
    /// </summary>    

    public class WoNoAttriLogi
    {
        private readonly ESIntegrateSysEntities db;

        /// <summary>
        /// 建構子：使用指定的資料庫上下文來建立 <see cref="WoNoAttriLogi"/> 的實例。
        /// </summary>
        /// <param name="dbContext">ESIntegrateSys 的 Entity Framework 資料庫上下文。</param>
        public WoNoAttriLogi(ESIntegrateSysEntities dbContext)
        {
            db = dbContext;
        }

        /// <summary>
        /// 工單屬性編號（1 = 標準工單, 2 = 預估工時, 3 = ECN預估, 4 = ECN實測）。
        /// </summary>
        public int AttriNo { get; set; }

        /// <summary>
        /// 要求日期（僅使用日期部分進行比較）。
        /// </summary>
        public DateTime RqDate { get; set; }

        const int Attri1 = 10;
        const int Attri2 = 5;
        
        /// <summary>
        /// 根據設定的工單屬性與要求日期判斷當日是否仍可排入新機種。
        /// 若超過該類別當日上限則回傳 <c>false</c>，否則回傳 <c>true</c>。
        /// </summary>
        /// <returns>可排入回傳 <c>true</c>；超過上限回傳 <c>false</c>。</returns>
        public bool Logi()
        {
            int result = db.ES_QuoteForSales.Where(d => d.WoNoAttri == AttriNo && 
                DbFunctions.TruncateTime(d.RequDate) == DbFunctions.TruncateTime(RqDate)).Count();
            switch (AttriNo)
            {
                case 1:
                    if (result > 10)
                    {
                        return false;
                    }
                    break;
                case 2:
                case 3:
                case 4:
                    if (result > 5)
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }
    }
}