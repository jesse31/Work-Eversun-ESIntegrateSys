using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ESIntegrateSys.Models;

namespace ESIntegrateSys.Models_QSchedule
{
    /// <summary>
    /// 表示上傳檔案記錄的資料模型，用於在報價系統中關聯檔案與報價項目。
    /// </summary>
    public class Record
    {
        /// <summary>
        /// 記錄流水號 (對應 ES_QuoteForSales.sno)
        /// </summary>
        public int sno { get; set; }

        /// <summary>
        /// 記錄名稱或標題
        /// </summary>
        public string Name { get; set; }

        // 其他字段

        /// <summary>
        /// 關聯的上傳檔案集合
        /// </summary>
        public virtual ICollection<ES_QuoteUploadFiles> Files { get; set; }
    }

}