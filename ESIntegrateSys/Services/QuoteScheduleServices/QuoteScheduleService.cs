using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using ESIntegrateSys.Models;
using ESIntegrateSys.Models_QSchedule;
using ESIntegrateSys.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace ESIntegrateSys.Services.QuoteScheduleServices
{
    /// <summary>
    /// 報價排程服務實作，負責查詢報價資料。
    /// </summary>
    public class QuoteScheduleService : IQuoteScheduleService
    {
        /// <summary>
        /// 資料庫操作物件。
        /// </summary>
        private readonly ESIntegrateSysEntities db;

        // 移除冗餘屬性，直接回傳查詢結果以減少記憶體佔用

        /// <summary>
        /// 建立 <see cref="QuoteScheduleService"/> 類別的新執行個體。
        /// </summary>
        /// <param name="db">資料庫操作物件。</param>
        public QuoteScheduleService(ESIntegrateSysEntities db)
        {
            this.db = db;
        }

        /// <summary>
        /// 依條件查詢報價資料。
        /// </summary>
        /// <param name="salesId">業務員編號。</param>
        /// <param name="custNo">客戶編號。</param>
        /// <param name="inDate">起始日期。</param>
        /// <param name="inDate2">結束日期。</param>
        /// <param name="sort">排序方式。</param>
        /// <param name="cancel">是否取消。</param>
        /// <param name="dept">部門編號。</param>
        /// <param name="engSr">工程師編號。</param>
        /// <param name="custMaterial">客戶料號。</param>
        /// <returns>符合條件的報價資料清單。</returns>
        public IEnumerable<QuoteDataListDto> GetQuoteData(
            string salesId,
            string custNo,
            string inDate,
            string inDate2,
            string sort,
            bool? cancel,
            string dept,
            string engSr,
            string custMaterial)
        {
            var query =
                from a in db.ES_QuoteForSales
                join b in db.ES_QuoteCust on a.CustNo equals b.KeyWorld into quoteCust
                from b in quoteCust.DefaultIfEmpty()
                join c in db.ES_QuoteWoNoAttri on a.WoNoAttri equals c.KeyWorld into wonoAttri
                from c in wonoAttri.DefaultIfEmpty()
                join d in db.ES_QuoteForIE on a.sno equals d.id into forIE
                from d in forIE.DefaultIfEmpty()
                join e in db.ES_Member on a.SalesId equals e.fUserId into quoteUserJoin
                from e in quoteUserJoin.DefaultIfEmpty()
                join f in db.ES_Member on (d != null ? d.IEonwer : null) equals f.fUserId into maintenanceUserJoin
                from f in maintenanceUserJoin.DefaultIfEmpty()
                join h in db.ES_QuoteUploadFiles on a.sno equals h.RecordId into filesJoin
                from h in filesJoin.DefaultIfEmpty()
                select new QuoteDataListDto
                {
                    Sno = a.sno,
                    CustNo = a.CustNo,
                    CustName = a.CustNo == "99" ? a.OtherName : (b != null ? b.Customer : null),
                    SalesNo = a.SalesNo,
                    EngSr = a.EngSr,
                    CustMaterial = a.CustMaterial,
                    WoNoAttri = c != null ? c.WoNoAttri : null,
                    AttriNo = c != null ? c.KeyWorld : 0,
                    RequDate = a.RequDate ?? DateTime.MinValue,
                    SalesName = e != null ? e.fName : null,
                    SalesId = a.SalesId,
                    IEonwer = f != null ? f.fName : null,
                    IEQuoteDate = d != null && d.IEQuoteDate.HasValue ? d.IEQuoteDate.Value : DateTime.MinValue,
                    IEQuoteTDate = d != null && d.IEQuoteTDate.HasValue ? d.IEQuoteTDate.Value : DateTime.MinValue,
                    CreateDate = a.CreateDate ?? DateTime.MinValue,
                    CancelChk = a.CancelChk ?? false,
                    HasFiles = h != null && h.RecordId.HasValue,
                    FRecordId = h != null ? h.RecordId : (int?)null,
                    Mark = (d != null && !string.IsNullOrEmpty(d.IEMark) && !string.IsNullOrEmpty(a.Mark))
                        ? a.Mark + "," + d.IEMark
                        : (d != null && !string.IsNullOrEmpty(d.IEMark))
                            ? d.IEMark
                            : a.Mark,
                    DeptNo = h != null ? h.DeptNo : null,
                    IEStatus = d != null ? d.IEStatus : null
                };

            // 條件查詢
            if (!string.IsNullOrEmpty(salesId))
                query = query.Where(a => a.SalesId == salesId);
            if (!string.IsNullOrEmpty(custNo))
                query = query.Where(a => a.CustNo == custNo);
            if (cancel.HasValue && cancel.Value)
                query = query.Where(a => a.CancelChk == cancel.Value);
            // 日期搜尋
            if (!string.IsNullOrEmpty(inDate) && !string.IsNullOrEmpty(inDate2))
            {
                if (DateTime.TryParse(inDate, out DateTime parsedIndate) && DateTime.TryParse(inDate2, out DateTime parsedIndate2))
                {
                    query = query.Where(a => DbFunctions.TruncateTime(a.RequDate) >= parsedIndate && DbFunctions.TruncateTime(a.RequDate) <= parsedIndate2);
                }
                else
                {
                    throw new ArgumentException("Invalid date format for Indate or Indate2");
                }
            }
            else if (!string.IsNullOrEmpty(inDate))
            {
                if (DateTime.TryParse(inDate, out DateTime parsedIndate))
                {
                    query = query.Where(a => DbFunctions.TruncateTime(a.RequDate) == parsedIndate);
                }
                else
                {
                    throw new ArgumentException("Invalid date format for Indate");
                }
            }
            else if (!string.IsNullOrEmpty(inDate2))
            {
                if (DateTime.TryParse(inDate2, out DateTime parsedIndate2))
                {
                    query = query.Where(a => DbFunctions.TruncateTime(a.RequDate) <= parsedIndate2);
                }
                else
                {
                    throw new ArgumentException("Invalid date format for Indate2");
                }
            }
            if (!string.IsNullOrEmpty(engSr))
                query = query.Where(a => a.EngSr != null && a.EngSr.Contains(engSr));
            if (!string.IsNullOrEmpty(custMaterial))
                query = query.Where(a => a.CustMaterial != null && a.CustMaterial.Contains(custMaterial));

            // 排序
            if (sort == "DESC")
            {
                query = query.OrderByDescending(q => q.IEonwer != null && q.IEonwer != "")
                             .ThenBy(q => q.CancelChk ? 1 : 0)
                             .ThenBy(q => q.HasFiles ? 1 : 0)
                             .ThenBy(q => q.RequDate);
            }
            else
            {
                query = query.OrderBy(q => q.CancelChk ? 1 : 0)
                             .ThenBy(q =>
                                 (q.HasFiles && (q.IEonwer == null || q.IEonwer == "")) ||
                                 (!q.HasFiles && q.IEonwer != null && q.IEonwer != "") ? 1 :
                                 (!q.HasFiles && (q.IEonwer == null || q.IEonwer == "")) ? 2 :
                                 3)
                             .ThenBy(q => q.RequDate);
            }

            // 只在查詢結尾執行 ToList()，直接回傳結果
            return query.ToList();
        }
    }
}