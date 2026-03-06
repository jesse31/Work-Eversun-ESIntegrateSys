using ESIntegrateSys.Services.Dtos;
using System.Collections.Generic;

namespace ESIntegrateSys.Services.QuoteScheduleServices
{
    /// <summary>
    /// 報價排程服務介面，負責查詢報價資料。
    /// </summary>
    public interface IQuoteScheduleService
    {
        /// <summary>
        /// 取得報價資料清單。
        /// </summary>
        /// <param name="salesId">業務員工編號。</param>
        /// <param name="custNo">客戶編號。</param>
        /// <param name="inDate">查詢起始日期 (字串格式)。</param>
        /// <param name="inDate2">查詢結束日期 (字串格式)。</param>
        /// <param name="sort">排序方式。</param>
        /// <param name="cancel">是否取消勾選。</param>
        /// <param name="dept">部門編號。</param>
        /// <param name="engSr">工程序號。</param>
        /// <param name="custMaterial">客戶料號。</param>
        /// <returns>報價資料清單的 DTO 集合。</returns>
        IEnumerable<QuoteDataListDto> GetQuoteData(
            string salesId,
            string custNo,
            string inDate,
            string inDate2,
            string sort,
            bool? cancel,
            string dept,
            string engSr,
            string custMaterial);
    }
}
