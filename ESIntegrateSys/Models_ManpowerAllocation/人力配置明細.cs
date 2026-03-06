using System; // 引入 System 命名空間
using System.Collections.Generic; // 引入集合相關命名空間
using System.ComponentModel.DataAnnotations; // 引入資料註解命名空間
using System.Linq; // 引入 LINQ 命名空間
using System.Web; // 引入 Web 相關命名空間

namespace ESIntegrateSys.Models_ManpowerAllocation // 定義命名空間：人力配置相關模型
{ // 命名空間開始
    /// <summary>
    /// 表示人力配置的明細資料模型（DTO）
    /// </summary>
    public class 人力配置明細 // 人力配置明細類別
    {
        /// <summary>
        /// 主鍵（字串）
        /// </summary>
        public string strPK { get; set; }

        /// <summary>
        /// 報工時顯示的生產單位名稱
        /// </summary>
        public string str報工生產單位名稱 { get; set; }

        /// <summary>
        /// 實際生產單位名稱
        /// </summary>
        public string str生產單位名稱 { get; set; }

        /// <summary>
        /// 班別名稱（例如：早班、晚班）
        /// </summary>
        public string str班別名稱 { get; set; }

        /// <summary>
        /// 出勤類型（例如：正式、臨時）
        /// </summary>
        public string str出勤類型 { get; set; }

        /// <summary>
        /// 作業人員工號
        /// </summary>
        public string str作業人員工號 { get; set; }

        /// <summary>
        /// 作業人員姓名
        /// </summary>
        public string str作業人員姓名 { get; set; }

        /// <summary>
        /// 備註欄位
        /// </summary>
        public string str備註 { get; set; }

        /// <summary>
        /// 報工生產單位的 GUID
        /// </summary>
        public string str報工生產單位GUID { get; set; }

        /// <summary>
        /// 是否為編輯或刪除（旗標）
        /// </summary>
        public Boolean blnIsEdDel { get; set; }

        /// <summary>
        /// 所屬部門名稱
        /// </summary>
        public string str部門 { get; set; }

        /// <summary>
        /// 最後修改者（使用者帳號或姓名）
        /// </summary>
        public string str修改者 { get; set; }

        /// <summary>
        /// 最後修改時間戳，用於並發控制（Nullable）
        /// </summary>
        public DateTime? dt最後修改時間 { get; set; }

        /// <summary>
        /// 目前正在編輯的使用者（若有值表示資料被鎖定）
        /// </summary>
        public string str目前編輯者 { get; set; }

        /// <summary>
        /// 編輯鎖定開始時間（Nullable）
        /// </summary>
        public DateTime? dt編輯鎖定時間 { get; set; }

        /// <summary>
        /// 前端傳入的時間戳，用於檢查並發
        /// </summary>
        [Display(Name = "時間戳")] // 前端顯示名稱註解
        public string str時間戳 { get; set; }

        /// <summary>
        /// 用於並發控制的時間戳字串（Base64 編碼）
        /// </summary>
        public string TimestampString // 並發控制用的時間戳字串屬性
        {
            get // TimestampString 的 getter
            {
                // 如果最後修改時間有值，則將其 Ticks 轉為位元組後做 Base64 編碼；否則回傳 null
                return dt最後修改時間.HasValue ? Convert.ToBase64String(BitConverter.GetBytes(dt最後修改時間.Value.Ticks)) : null; // 回傳 Base64 時間戳或 null
            } // getter 結束
        } // TimestampString 屬性結束
    } // 類別結束
} // 命名空間結束