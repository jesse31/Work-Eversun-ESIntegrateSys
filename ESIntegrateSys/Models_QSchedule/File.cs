using System; // 使用 System 命名空間，提供基礎型別與系統功能
using System.Collections.Generic; // 使用泛型集合
using System.Linq; // 使用 LINQ 擴充方法
using System.Web; // 使用 Web 相關類別（保留以備用）
using System.IO; // 使用 IO 功能，例如檔案處理
using ESIntegrateSys.Models; // 引用專案的 Models 命名空間，提供 Record 類別等

namespace ESIntegrateSys.Models_QSchedule // 定義 Models_QSchedule 命名空間
{ // 命名空間開始
    /// <summary>
    /// 案例：報價上傳檔案的資料物件 (Entity)
    /// </summary>
    public class ES_QuoteUploadFiles // 公開類別：表示報價上傳檔案的資料表對應類別
    { // 類別區塊開始
        /// <summary>
        /// 資料表流水號 (主鍵)
        /// </summary>
        public int sno { get; set; } // 流水號屬性

        /// <summary>
        /// 對應的記錄 Id (外鍵)
        /// </summary>
        public int RecordId { get; set; } // 關聯的記錄編號

        /// <summary>
        /// 檔案儲存路徑（相對或絕對）
        /// </summary>
        public string FilePath { get; set; } // 檔案路徑

        /// <summary>
        /// 上傳時間
        /// </summary>
        public DateTime UploadTime { get; set; } // 檔案上傳時間

        /// <summary>
        /// 檔案名稱
        /// </summary>
        public string FileName { get; set; } // 檔案名稱

        /// <summary>
        /// 導航屬性：對應的 Record 物件
        /// </summary>
        public virtual Record Record { get; set; } // 導航屬性，虛擬以供延遲載入
    } // 類別區塊結束

} // 命名空間結束