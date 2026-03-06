using System;

namespace ESIntegrateSys.Models_ManpowerAllocation
{
    /// <summary>
    /// 報工生產單位權限資料傳輸物件
    /// </summary>
    public class MP_ProductionUnitPermissionDto
    {
        /// <summary>
        /// 權限唯一識別碼
        /// </summary>
        public int 權限ID { get; set; }

        /// <summary>
        /// 單位識別碼
        /// </summary>
        public int 單位ID { get; set; }

        /// <summary>
        /// 使用者工號
        /// </summary>
        public string 使用者ID { get; set; }

        /// <summary>
        /// 權限授予日期
        /// </summary>
        public DateTime 授權日期 { get; set; }

        /// <summary>
        /// 權限到期日（可選）
        /// </summary>
        public DateTime? 到期日 { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string 備註 { get; set; }
        
        #region 關聯資料
        
        /// <summary>
        /// 報工生產單位名稱
        /// </summary>
        public string UnitName { get; set; }
        
        /// <summary>
        /// 員工工號（同使用者ID）
        /// </summary>
        public string EmpId { get; set; }
        
        /// <summary>
        /// 員工姓名
        /// </summary>
        public string EmpName { get; set; }
        
        #endregion
    }
}
