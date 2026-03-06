using System;

namespace ESIntegrateSys.Models_MGun
{
    /// <summary>
    /// 物料槍維修記錄資料模型。
    /// </summary>
    public class RGunDataListDto
    {
        /// <summary>
        /// 維修記錄序號。
        /// </summary>
        public int Sno { get; set; }

        /// <summary>
        /// 物料槍設備編號。
        /// </summary>
        public string MaterialGun_Eno { get; set; }

        /// <summary>
        /// 物料槍序號。
        /// </summary>
        public string MaterialGun_Sno { get; set; }

        /// <summary>
        /// 送修日期。
        /// </summary>
        public DateTime RepairDate { get; set; }

        /// <summary>
        /// 維修完成時間。
        /// </summary>
        public DateTime MaintenanceTime { get; set; }

        /// <summary>
        /// 故障描述。
        /// </summary>
        public string BadDescription { get; set; }

        /// <summary>
        /// 送修人員編號。
        /// </summary>
        public string RepairUserId { get; set; }

        /// <summary>
        /// 維修人員編號。
        /// </summary>
        public string MaintenanceUserId { get; set; }

        /// <summary>
        /// 送修人員姓名。
        /// </summary>
        public string rName { get; set; }

        /// <summary>
        /// 維修人員姓名。
        /// </summary>
        public string mName { get; set; }

        /// <summary>
        /// 維修結果。
        /// </summary>
        public string MaintenanceResult { get; set; }

        /// <summary>
        /// 維修分類。
        /// </summary>
        public string Classification { get; set; }

        /// <summary>
        /// 備註。
        /// </summary>
        public string Mark { get; set; }

        /// <summary>
        /// 其他維修結果說明。
        /// </summary>
        public string Other { get; set; }

        /// <summary>
        /// 更換項目名稱。
        /// </summary>
        public string ChangeItemName { get; set; }

        /// <summary>
        /// 更換項目編號。
        /// </summary>
        public string ChangeItemNo { get; set; }

        /// <summary>
        /// 維修完成狀態。
        /// </summary>
        public Boolean Chk { get; set; }
    }
}