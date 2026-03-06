using ESIntegrateSys.Controllers;
using ESIntegrateSys.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ESIntegrateSys.Models_QSchedule
{
    /// <summary>
    /// 用於查詢報價資料並提供相關新增/修改/取消等業務邏輯的服務類別。
    /// </summary>
    public class QuoteQuery
    {
        private readonly ESIntegrateSysEntities db;
        /// <summary>
        /// 建構子：注入資料庫上下文並初始化查詢結果集合。
        /// </summary>
        /// <param name="dbContext">ESIntegrateSys 的 Entity Framework 資料庫上下文。</param>
        public QuoteQuery(ESIntegrateSysEntities dbContext)
        {
            db = dbContext;
            QuoteDataLists = new List<QuoteDataList>();
        }
        /// <summary>
        /// 查詢結果的資料集合，每個元素代表一筆報價資料。
        /// </summary>
        public List<QuoteDataList> QuoteDataLists { get; set; }
        /// <summary>
        /// 報價資料清單項目，用於封裝單筆報價的欄位顯示。
        /// </summary>
        public class QuoteDataList
        {
            /// <summary>流水號 (主鍵)</summary>
            public int Sno { get; set; }
            /// <summary>客戶代號</summary>
            public string CustNo { get; set; }
            /// <summary>客戶名稱 (含 OtherName 處理)</summary>
            public string CustName { get; set; }
            /// <summary>業務單號</summary>
            public string SalesNo { get; set; } //業務單號
            /// <summary>業務名稱</summary>
            public string SalesName { get; set; }//業務名稱
            /// <summary>業務員工編號</summary>
            public string SalesId { get; set; }//業務員工編號
            /// <summary>工程編號</summary>
            public string EngSr { get; set; }
            /// <summary>客戶機種名稱</summary>
            public string CustMaterial { get; set; }
            /// <summary>工單屬性名稱</summary>
            public string WoNoAttri { get; set; }
            /// <summary>工單屬性代碼</summary>
            public int AttriNo { get; set; }
            /// <summary>需求日期</summary>
            public DateTime RequDate { get; set; }
            /// <summary>業務備註</summary>
            public string Mark { get; set; }
            /// <summary>IE 備註</summary>
            public string IEMark { get; set; }
            /// <summary>建立日期</summary>
            public DateTime CreateDate { get; set; }
            /// <summary>是否已勾選</summary>
            public bool Chk { get; set; }
            /// <summary>是否已取消報價</summary>
            public bool CancelChk { get; set; }
            /// <summary>IE 負責人姓名</summary>
            public string IEonwer { get; set; }
            /// <summary>其他客戶名稱 (當 CustNo 為 99)</summary>
            public string OtherName { get; set; }
            /// <summary>IE 報價開始日期</summary>
            public DateTime IEQuoteDate { get; set; }
            /// <summary>IE 報價結束日期</summary>
            public DateTime IEQuoteTDate { get; set; }
            /// <summary>是否含檔案</summary>
            public bool HasFiles { get; set; }
            /// <summary>檔案記錄 Id</summary>
            public int? FRecordId { get; set; }
            /// <summary>部門代碼</summary>
            public string DeptNo { get; set; }
            /// <summary>IE 的狀態</summary>
            public string IEStatus { get; set; }
        }

        #region 頁面顯示&搜尋
        /// <summary>
        /// 執行報價資料的搜尋並回傳符合條件的清單。
        /// </summary>
        /// <param name="SalesID">業務帳號 (可為 null 或空字串，表示不篩選)</param>
        /// <param name="CustNo">客戶代號 (可為 null 或空字串，表示不篩選)</param>
        /// <param name="Indate">起始日期字串 (格式可被 DateTime.Parse 解析)</param>
        /// <param name="Indate2">結束日期字串 (格式可被 DateTime.Parse 解析)</param>
        /// <param name="Sort">排序方式 ("DESC" 表示反向排序)</param>
        /// <param name="Cancel">是否只顯示已取消報價 (true: 只顯示取消)</param>
        /// <param name="DeptNo">部門代號 (目前未使用，保留參數)</param>
        /// <param name="EngSr">工程編號 (模糊查詢)</param>
        /// <param name="CustMaterial">機種名稱 (模糊查詢)</param>
        /// <returns>回傳符合條件的 <see cref="List{QuoteDataList}"/>。</returns>
        public List<QuoteDataList> QuoteDataSearch(string SalesID, string CustNo, string Indate, string Indate2, string Sort , bool? Cancel,string DeptNo,string EngSr,string CustMaterial)
        {
            var query = from a in db.ES_QuoteForSales
                        join b in db.ES_QuoteCust on a.CustNo equals b.KeyWorld into quoteCust
                        from b in quoteCust.DefaultIfEmpty()
                        join c in db.ES_QuoteWoNoAttri on a.WoNoAttri equals c.KeyWorld into wonoAttri
                        from c in wonoAttri.DefaultIfEmpty()
                        join d in db.ES_QuoteForIE on a.sno equals d.id into forIE
                        from d in forIE.DefaultIfEmpty()
                        join e in db.ES_Member on a.SalesId equals e.fUserId into quoteUserJoin
                        from e in quoteUserJoin.DefaultIfEmpty()
                        join f in db.ES_Member on d.IEonwer equals f.fUserId into maintenanceUserJoin
                        from f in maintenanceUserJoin.DefaultIfEmpty()
                        //join g in db.ES_QuoteUploadRecords on a.sno equals g.Q_sno into recordJoin
                        //from g in recordJoin.DefaultIfEmpty()
                        join h in db.ES_QuoteUploadFiles on a.sno equals h.RecordId into filesJoin
                        from h in filesJoin.DefaultIfEmpty()
                        group new { a, b, c, d, e, f,  h } by new
                        {
                            a.sno,
                            a.CustNo,
                            CustName = a.CustNo == "99" ? a.OtherName : b != null ? b.Customer : null,
                            a.SalesNo,
                            a.EngSr,
                            a.CustMaterial,
                            c.WoNoAttri,
                            AttriNo = c.KeyWorld,
                            RequDate = a.RequDate.HasValue ? a.RequDate.Value : DateTime.MinValue,
                            SalesName = e != null ? e.fName : null,
                            a.SalesId,
                            IEonwer = f != null ? f.fName : null,
                            IEQuoteDate = d != null && d.IEQuoteDate.HasValue ? d.IEQuoteDate.Value : DateTime.MinValue,
                            IEQuoteTDate = d != null && d.IEQuoteTDate.HasValue ? d.IEQuoteTDate.Value : DateTime.MinValue,
                            CreateDate = a.CreateDate.HasValue ? a.CreateDate.Value : DateTime.MinValue,
                            CancelChk = a.CancelChk.HasValue ? a.CancelChk.Value : false,
                            a.Mark,
                            d.IEMark,
                            d.IEStatus
                        } into g
                        select new QuoteDataList
                        {
                            Sno = g.Key.sno,
                            CustNo = g.Key.CustNo,
                            CustName = g.Key.CustName,
                            SalesNo = g.Key.SalesNo,
                            EngSr = g.Key.EngSr,
                            CustMaterial = g.Key.CustMaterial,
                            WoNoAttri = g.Key.WoNoAttri,
                            AttriNo = g.Key.AttriNo,
                            RequDate = g.Key.RequDate,
                            SalesName = g.Key.SalesName,
                            SalesId = g.Key.SalesId,
                            IEonwer = g.Key.IEonwer,
                            IEQuoteDate = g.Key.IEQuoteDate,
                            IEQuoteTDate = g.Key.IEQuoteTDate,
                            CreateDate = g.Key.CreateDate,
                            CancelChk = g.Key.CancelChk,
                            HasFiles = g.Any(x => x.h.RecordId.HasValue),
                            FRecordId = g.Select(x => x.h.RecordId).FirstOrDefault(),
                            Mark = !string.IsNullOrEmpty(g.Key.IEMark) && !string.IsNullOrEmpty(g.Key.Mark)
                           ? g.Key.Mark + "," + g.Key.IEMark
                           : !string.IsNullOrEmpty(g.Key.IEMark)
                             ? g.Key.IEMark
                             : g.Key.Mark,
                            DeptNo = g.Select(x => x.h.DeptNo).FirstOrDefault(),
                            //IEMark = !string.IsNullOrEmpty(g.Key.IEMark) && !string.IsNullOrEmpty(g.Key.Mark) ? g.Key.Mark + "," + g.Key.IEMark : g.Key.Mark
                            IEStatus=g.Key.IEStatus
                        };

            #region 業務
            if (!string.IsNullOrEmpty(SalesID))
            {
                query = query.Where(a => a.SalesId == SalesID);
            }
            #endregion

            #region 客戶別
            if (!string.IsNullOrEmpty(CustNo))
            {
                query = query.Where(a => a.CustNo == CustNo);
            }
            #endregion

            #region 取消報價
            if (Cancel != null && (bool)Cancel)
            {
                query = query.Where(a => a.CancelChk == Cancel);
            }
            #endregion

            #region 日期搜尋
            if (!string.IsNullOrEmpty(Indate) && !string.IsNullOrEmpty(Indate2))
            {
                if (DateTime.TryParse(Indate, out DateTime parsedIndate) && DateTime.TryParse(Indate2, out DateTime parsedIndate2))
                {
                    query = query.Where(a => DbFunctions.TruncateTime(a.RequDate) >= parsedIndate && DbFunctions.TruncateTime(a.RequDate) <= parsedIndate2);
                }
                else
                {
                    // Handle invalid date format
                    // You could log an error, throw an exception, or return an error response
                    // For example:
                    throw new ArgumentException("Invalid date format for Indate or Indate2");
                }
            }
            else if (!string.IsNullOrEmpty(Indate))
            {
                if (DateTime.TryParse(Indate, out DateTime parsedIndate))
                {
                    query = query.Where(a => DbFunctions.TruncateTime(a.RequDate) == parsedIndate);
                }
                else
                {
                    // Handle invalid date format
                    // You could log an error, throw an exception, or return an error response
                    throw new ArgumentException("Invalid date format for Indate");
                }
            }
            else if (!string.IsNullOrEmpty(Indate2))
            {
                if (DateTime.TryParse(Indate2, out DateTime parsedIndate2))
                {
                    query = query.Where(a => DbFunctions.TruncateTime(a.RequDate) <= parsedIndate2);
                }
                else
                {
                    // Handle invalid date format
                    // You could log an error, throw an exception, or return an error response
                    throw new ArgumentException("Invalid date format for Indate2");
                }
            }
            #endregion

            #region 排序
            if (Sort == "DESC")
            {
                query = query.OrderByDescending(q => !string.IsNullOrEmpty(q.IEonwer)).ThenBy(q => q.CancelChk ? 1 : 0).ThenBy(q => q.HasFiles ? 1 : 0).ThenBy(q => q.RequDate);
            }
            else
            {
                query = query.OrderBy(q => q.CancelChk ? 1 : 0)
                                        .ThenBy(q =>
                                                        (q.HasFiles && string.IsNullOrEmpty(q.IEonwer)) ||
                                                        (!q.HasFiles && !string.IsNullOrEmpty(q.IEonwer)) ? 1 :  // 第1種類型，有檔案+無 IEonwer 或 無檔案+有 IEonwer => 優先級 1
                                                        (!q.HasFiles && string.IsNullOrEmpty(q.IEonwer)) ? 2 : // 第2種類型，無檔案+無 IEonwer => 優先級 2
                                                        3)  // 第3種類型，有檔案+有 IEonwer => 優先級 3
                                        .ThenBy(q => q.RequDate);  // 在每種類型內按日期升序排列


            }
            #endregion

            #region 工程編號
            if (!string.IsNullOrEmpty(EngSr))
            {
                query = query.Where(a => a.EngSr.Contains(EngSr));
            }
            #endregion

            #region 機種名稱(模糊查詢)
            if (!string.IsNullOrEmpty(CustMaterial))
            {
                query = query.Where(a => a.CustMaterial.Contains(CustMaterial));
            }
            #endregion


            var dataList = query.ToList();
            QuoteDataLists.AddRange(dataList.ToList());
            return QuoteDataLists;
        }
        #endregion

        /// <summary>
        /// 新增一筆業務報價 (ES_QuoteForSales) 紀錄。
        /// </summary>
        /// <param name="uId">業務使用者 Id</param>
        /// <param name="CustNo">客戶代號</param>
        /// <param name="EngSr">工程編號</param>
        /// <param name="SalesNo">業務單號</param>
        /// <param name="CustMaterial">客戶機種</param>
        /// <param name="WoNoAttri">工單屬性代碼</param>
        /// <param name="RequDate">需求日期</param>
        /// <param name="Mark">備註</param>
        /// <param name="OtherName">當 CustNo 為 99 時使用的其他客戶名稱</param>
        public void QuoteSales(string uId, string CustNo, string EngSr, string SalesNo, string CustMaterial, int WoNoAttri, DateTime RequDate, string Mark,string OtherName)
        {
            ES_QuoteForSales forSales = new ES_QuoteForSales
            {
                SalesId = uId,
                CustNo = CustNo,
                EngSr = EngSr,
                SalesNo = SalesNo,
                CustMaterial = CustMaterial,
                WoNoAttri = WoNoAttri,
                RequDate = RequDate,
                Mark = Mark,
                CreateDate = DateTime.Now,
                Chk = false,
                OtherName = OtherName,
                CancelChk = false
            }; 

            db.ES_QuoteForSales.Add(forSales);
            db.SaveChanges();
        }

        /// <summary>
        /// 編輯指定報價的欄位並在必要時發送通知郵件。
        /// </summary>
        /// <param name="sno">報價流水號</param>
        /// <param name="EngSr">工程編號</param>
        /// <param name="CustMaterial">機種名稱</param>
        /// <param name="WoNoAttri">工單屬性代碼</param>
        /// <param name="RequDate">需求日期</param>
        /// <param name="Mark">備註</param>
        /// <param name="Sales">操作人員/業務名稱，用於郵件通知</param>
        public void SalesEdit(int sno, string EngSr, string CustMaterial, int WoNoAttri, DateTime RequDate, string Mark,string Sales)
        {
            try
            {
                var edit = db.ES_QuoteForSales.Find(sno);
                var query = db.ES_QuoteWoNoAttri
                    .Where(a => a.KeyWorld == edit.WoNoAttri || a.KeyWorld == WoNoAttri)
                    .Select(a => new
                    {
                        a.KeyWorld,
                        a.WoNoAttri,
                        IsOriginal=a.KeyWorld==edit.WoNoAttri
                    }).ToList();
                // 將結果按照標示排序
                var sortedResults = query.OrderBy(r => r.IsOriginal ? 0 : 1).ToArray();
                // 根據排序結果區分Name
                var originalName = sortedResults.FirstOrDefault(r => r.IsOriginal)?.WoNoAttri;
                var modifiedName = sortedResults.FirstOrDefault(r => !r.IsOriginal)?.WoNoAttri;
                modifiedName = (modifiedName == null) ? originalName : modifiedName;

                string[] bodys = new string[] { edit.EngSr,EngSr, edit.CustMaterial, CustMaterial,
                    originalName,modifiedName, edit.RequDate.Value.ToShortDateString(),RequDate.ToShortDateString(), edit.Mark,Mark,sno.ToString(),Sales };

                edit.EngSr = EngSr;
                edit.CustMaterial = CustMaterial;
                edit.WoNoAttri = WoNoAttri;
                edit.RequDate = RequDate;
                edit.Mark = Mark;
                edit.CreateDate = DateTime.Now;
                db.SaveChanges();

                string subject = "修改報價通知";
                var otherCtrl = DependencyResolver.Current.GetService<EmailController>();   //發mail
                otherCtrl.SendTestEmail(subject, bodys);

                #region 如果修改超過
                WoNoAttriLogi attriLogi = new WoNoAttriLogi(db);
                attriLogi.AttriNo = WoNoAttri;
                attriLogi.RqDate = RequDate;
                if (!attriLogi.Logi())
                {
                    var cust = "";
                    var att = (from w in db.ES_QuoteWoNoAttri
                               where w.KeyWorld == WoNoAttri
                               select new { w.WoNoAttri }).SingleOrDefault();

                    if (edit.CustNo == "99")
                    {
                        cust = edit.OtherName;
                    }
                    else
                    {
                        cust = "(" + edit.CustNo + ")";
                        var customerResult = (from c in db.ES_QuoteCust
                                              where c.KeyWorld == edit.CustNo
                                              select new { c.Customer }).SingleOrDefault();
                        if (customerResult != null)
                        {
                            cust += customerResult.Customer;
                        }
                    }
                    bodys = new string[0];
                    bodys = new string[] { cust, EngSr, edit.SalesNo, CustMaterial, att.WoNoAttri, RequDate.ToShortDateString(), Mark, Sales };
                    subject = "報價安排數超過通知";
                    var otherCtrl2 = DependencyResolver.Current.GetService<EmailController>();   //發mail
                    otherCtrl2.SendTestEmail(subject, bodys);
                }
                #endregion

            }
            catch (Exception ex)
            {

                throw;
            }

        }

        /// <summary>
        /// 取消指定報價並發送取消通知郵件。
        /// </summary>
        /// <param name="sno">報價流水號</param>
        /// <param name="Sales">操作人員/業務名稱，用於郵件通知</param>
        public void SalesCancel(int sno,string Sales)
        {
            try
            {
                var edit = db.ES_QuoteForSales.Find(sno);
                edit.CancelChk = true;
                edit.CreateDate = DateTime.Now;
                db.SaveChanges();

                var cust = "";
                var att = (from w in db.ES_QuoteWoNoAttri
                            where w.KeyWorld == edit.WoNoAttri
                            select new {w.WoNoAttri }).SingleOrDefault();

                if (edit.CustNo == "99")
                {
                    cust = edit.OtherName;
                }
                else
                {
                    cust = "(" + edit.CustNo + ")";
                    var customerResult = (from c in db.ES_QuoteCust
                                         where c.KeyWorld == edit.CustNo
                                         select new { c.Customer }).SingleOrDefault();
                    if (customerResult != null)
                    {
                        cust += customerResult.Customer;
                    }
                }

                string[] bodys = new string[]
                {
                    sno.ToString(),
                    Sales,
                    edit.CreateDate.Value.ToShortDateString(),
                    cust,
                    edit.SalesNo,
                    edit.EngSr,
                    edit.CustMaterial,
                    (att.WoNoAttri),
                    edit.RequDate.Value.ToShortDateString(),
                    edit.Mark
                };
                string subject = "取消報價通知";
                var otherCtrl = DependencyResolver.Current.GetService<EmailController>();   //發mail
                otherCtrl.SendTestEmail(subject, bodys);
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        /// <summary>
        /// 由 IE 新增或更新報價相關資訊，包含設定負責人、報價日期或更新備註。
        /// </summary>
        /// <param name="uId">IE 使用者 Id</param>
        /// <param name="Sno">報價流水號</param>
        /// <param name="IEQuoteDate">IE 報價日期</param>
        /// <param name="IEQuoteTDate">IE 報價有效至日期</param>
        /// <param name="Mark">備註或狀態說明</param>
        /// <param name="UpdateMark">若為非空則代表僅更新備註</param>
        public void QuoteIE(string uId,int Sno,DateTime? IEQuoteDate, DateTime? IEQuoteTDate,string Mark, string UpdateMark)
        {
            var forIE = db.ES_QuoteForIE.FirstOrDefault(o => o.id == Sno);

            if (forIE != null)
            {
                //備註更新
                if (!string.IsNullOrEmpty(UpdateMark))
                {
                    forIE.IEMark = Mark;
                }
                else
                {
                    //報價更新
                    forIE.IEonwer = uId;
                    forIE.IEQuoteDate = IEQuoteDate;
                    forIE.IEQuoteTDate = IEQuoteTDate;
                    forIE.OperatorDate = DateTime.Now;
                }
            }
            else
            {
                //固定新增三要素
                forIE = new ES_QuoteForIE
                {
                    id = Sno,
                    IEonwer = uId,
                    OperatorDate = DateTime.Now,
                };

                //判斷是否僅新增備註
                if (!string.IsNullOrEmpty(UpdateMark))
                {
                    forIE.IEMark = Mark;
                }
                else
                {
                    //新增
                    forIE.IEQuoteDate = IEQuoteDate;
                    forIE.IEQuoteTDate = IEQuoteTDate;
                }
                db.ES_QuoteForIE.Add(forIE);
            }

            var forSales = db.ES_QuoteForSales.Find(Sno);
            if (forSales != null)
            {
                forSales.Chk = true;
            }

            db.SaveChanges();


            #region 依據Mark發mail
            List<string> containsmark = new List<string> { "產線尚未完成", "SOP尚未完成", "報價資料尚未提供", "需求日已到" };
            bool b = false;
            string subject=string.Empty;
            foreach (var item in containsmark)
            {
                if (Mark.Contains(item))
                {
                    b = true;
                    subject = item;
                }
            }
            if (b)
            {
                var cust = "";
                var att = (from w in db.ES_QuoteWoNoAttri
                           where w.KeyWorld == forSales.WoNoAttri
                           select new { w.WoNoAttri }).SingleOrDefault();

                if (forSales.CustNo == "99")
                {
                    cust = forSales.OtherName;
                }
                else
                {
                    cust = "(" + forSales.CustNo + ")";
                    var customerResult = (from c in db.ES_QuoteCust
                                          where c.KeyWorld == forSales.CustNo
                                          select new { c.Customer }).SingleOrDefault();
                    if (customerResult != null)
                    {
                        cust += customerResult.Customer;
                    }
                }

                string[] bodys = new string[]
                    {
                    forSales.sno.ToString(),
                    forSales.CreateDate.Value.ToShortDateString(),
                    cust,
                    forSales.SalesNo,
                    forSales.EngSr,
                    forSales.CustMaterial,
                    (att.WoNoAttri),
                    forSales.RequDate.Value.ToShortDateString(),
                    forSales.SalesId
                    };

                var otherCtrl = DependencyResolver.Current.GetService<EmailController>();   //發mail
                otherCtrl.SendTestEmail(subject, bodys);
            }
            #endregion
        }
    }
}