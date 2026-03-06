using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ESIntegrateSys.Models_ManpowerAllocation
{
    /// <summary>
    /// 人力配置明細異動的資料傳輸物件 (DTO)，用於承載前端與後端之間的人力配置異動資料。
    /// </summary>
    public class 人力配置明細異動Dto
    {
        /// <summary>
        /// 主鍵 (字串)，通常為 GUID 或自訂編號。
        /// </summary>
        public string strPK { get; set; }
                
        /// <summary>
        /// 報工生產單位名稱（顯示用），為必填欄位。
        /// </summary>
        [Required(ErrorMessage = "報工生產單位名稱為必填欄位。")]
        public string str報工生產單位名稱 { get; set; }

        /// <summary>
        /// 實際生產單位名稱，為必填欄位。
        /// </summary>
        [Required(ErrorMessage = "生產單位名稱為必填欄位。")]
        public string str生產單位名稱 { get; set; }

        /// <summary>
        /// 班別名稱（例如：早班、晚班），為必填欄位。
        /// </summary>
        [Required(ErrorMessage = "班別名稱為必填欄位。")]
        public string str班別名稱 { get; set; }

        /// <summary>
        /// 出勤類型（例如：正式、臨時），為必填欄位。
        /// </summary>
        [Required(ErrorMessage = "出勤類型為必填欄位。")]
        public string str出勤類型 { get; set; }

        /// <summary>
        /// 作業人員工號，字數上限為 10。
        /// </summary>
        [Required(ErrorMessage = "作業人員工號為必填欄位。")]
        [StringLength(10, ErrorMessage = "作業人員工號長度不能超過10個字元。")]
        public string str作業人員工號 { get; set; }

        /// <summary>
        /// 作業人員姓名，字數上限為 50。
        /// </summary>
        [Required(ErrorMessage = "作業人員姓名為必填欄位。")]
        [StringLength(50, ErrorMessage = "作業人員姓名長度不能超過50個字元。")]
        public string str作業人員姓名 { get; set; }

        /// <summary>
        /// 備註欄位，字數上限為 4000。
        /// </summary>
        [StringLength(4000, ErrorMessage = "備註長度不能超過4000個字元。")]
        public string str備註 { get; set; }

        /// <summary>
        /// 建立者（使用者帳號或名稱）。
        /// </summary>
        public string str建立者 { get; set; }

        /// <summary>
        /// 最後修改者（使用者帳號或名稱）。
        /// </summary>
        public string str修改者 { get; set; }

        /// <summary>
        /// 報工生產單位的 GUID。
        /// </summary>
        public string str報工生產單位GUID { get; set; }

        /// <summary>
        /// 生產單位的 GUID。
        /// </summary>
        public string str生產單位GUID { get; set; }

        /// <summary>
        /// 出勤類型對應之 GUID。
        /// </summary>
        public string str出勤GUID { get; set; }

        /// <summary>
        /// 班別對應之 GUID。
        /// </summary>
        public string str班別GUID { get; set; }

        /// <summary>
        /// 是否為編輯或刪除的旗標。
        /// </summary>
        public Boolean blnIsEdDel { get; set; }

        /// <summary>
        /// 所屬部門名稱。
        /// </summary>
        public string str部門 { get; set; }

        /// <summary>
        /// 最後修改時間（Nullable），用於並發控制或顯示最後編輯時間。
        /// </summary>
        public DateTime? dt最後修改時間 { get; set; }

        /// <summary>
        /// 目前編輯者的帳號或名稱（若有值表示資料正在被其他使用者編輯）。
        /// </summary>
        public string str目前編輯者 { get; set; }

        /// <summary>
        /// 編輯鎖定開始時間（Nullable），用以標示資料被鎖定的起始時間。
        /// </summary>
        public DateTime? dt編輯鎖定時間 { get; set; }

        /// <summary>
        /// 前端傳入的時間戳字串，用於檢查並發衝突。
        /// </summary>
        [Display(Name = "時間戳")]
        public string str時間戳 { get; set; }

        /// <summary>
        /// 並發控制使用的時間戳字串（以最後修改時間的 Ticks 轉為 Base64 編碼）。
        /// 如果無最後修改時間則回傳 null。
        /// </summary>
        public string TimestampString
        {
            get
            {
                return dt最後修改時間.HasValue ? Convert.ToBase64String(BitConverter.GetBytes(dt最後修改時間.Value.Ticks)) : null;
            }
        }
    }
}