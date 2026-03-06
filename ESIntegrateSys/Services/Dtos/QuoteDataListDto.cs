using System;

namespace ESIntegrateSys.Services.Dtos
{
    /// <summary>
    /// 代表報價資料清單的資料傳輸物件 (DTO)。
    /// </summary>
    public class QuoteDataListDto
    {
        /// <summary>
        /// 序號。
        /// </summary>
        public int Sno { get; set; }

        /// <summary>
        /// 客戶編號。
        /// </summary>
        public string CustNo { get; set; }

        /// <summary>
        /// 客戶名稱。
        /// </summary>
        public string CustName { get; set; }

        /// <summary>
        /// 業務單號。
        /// </summary>
        public string SalesNo { get; set; }

        /// <summary>
        /// 業務名稱。
        /// </summary>
        public string SalesName { get; set; }

        /// <summary>
        /// 業務員工編號。
        /// </summary>
        public string SalesId { get; set; }

        /// <summary>
        /// 工程序號。
        /// </summary>
        public string EngSr { get; set; }

        /// <summary>
        /// 客戶料號。
        /// </summary>
        public string CustMaterial { get; set; }

        /// <summary>
        /// 工單屬性。
        /// </summary>
        public string WoNoAttri { get; set; }

        /// <summary>
        /// 屬性編號。
        /// </summary>
        public int AttriNo { get; set; }

        /// <summary>
        /// 需求日期。
        /// </summary>
        public DateTime RequDate { get; set; }

        /// <summary>
        /// 備註。
        /// </summary>
        public string Mark { get; set; }

        /// <summary>
        /// IE備註。
        /// </summary>
        public string IEMark { get; set; }

        /// <summary>
        /// 建立日期。
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 是否勾選。
        /// </summary>
        public bool Chk { get; set; }

        /// <summary>
        /// 是否取消勾選。
        /// </summary>
        public bool CancelChk { get; set; }

        /// <summary>
        /// IE負責人。
        /// </summary>
        public string IEonwer { get; set; }

        /// <summary>
        /// 其他名稱。
        /// </summary>
        public string OtherName { get; set; }

        /// <summary>
        /// IE報價日期。
        /// </summary>
        public DateTime IEQuoteDate { get; set; }

        /// <summary>
        /// IE報價截止日期。
        /// </summary>
        public DateTime IEQuoteTDate { get; set; }

        /// <summary>
        /// 是否有檔案。
        /// </summary>
        public bool HasFiles { get; set; }

        /// <summary>
        /// 檔案記錄編號。
        /// </summary>
        public int? FRecordId { get; set; }

        /// <summary>
        /// 部門編號。
        /// </summary>
        public string DeptNo { get; set; }

        /// <summary>
        /// IE狀態。
        /// </summary>
        public string IEStatus { get; set; }
    }
}