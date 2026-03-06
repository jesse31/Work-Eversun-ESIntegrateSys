using ESIntegrateSys.Models;
using ESIntegrateSys.Models_QSchedule;
using ESIntegrateSys.Services.Dtos;
using ESIntegrateSys.Services.QuoteScheduleServices;
using ESIntegrateSys.ViewModels;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ESIntegrateSys.Controllers
{
    /// <summary>
    /// 電子報價排程控制器，負責處理報價相關的操作與頁面顯示。
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class QuoteScheduleController : Controller
    {
        /// <summary>
        /// 報價排程服務 (由 DI 注入)
        /// </summary>
        private readonly IQuoteScheduleService _quoteScheduleService;

        /// <summary>
        /// Email 服務 (由 DI 注入)
        /// </summary>
        private readonly ESIntegrateSys.Services.EmailServices.IEmailService _emailService;

        /// <summary>
        /// Entity Framework 資料庫存取物件，負責連接 ESIntegrateSysEntities 資料庫。
        /// </summary>
        private readonly ESIntegrateSysEntities db;

        /// <summary>
        /// 建立 <see cref="QuoteScheduleController"/> 類別的新執行個體。
        /// </summary>
        /// <param name="quoteScheduleService">報價排程服務介面，負責查詢報價資料。</param>
        /// <param name="emailService">Email 服務 (由 DI 注入)。</param>
        /// <param name="dbContext">Entity Framework 資料庫存取物件。</param>
        /// <exception cref="ArgumentNullException">
        /// 當 <paramref name="quoteScheduleService"/>、<paramref name="emailService"/> 或 <paramref name="dbContext"/> 為 null 時，拋出例外。
        /// </exception>
        public QuoteScheduleController(
            IQuoteScheduleService quoteScheduleService,
            ESIntegrateSys.Services.EmailServices.IEmailService emailService,
            ESIntegrateSysEntities dbContext)
        {
            if (quoteScheduleService is null)
                throw new ArgumentNullException(nameof(quoteScheduleService), "QuoteScheduleService 不可為 null");
            if (emailService is null)
                throw new ArgumentNullException(nameof(emailService), "EmailService 不可為 null，請確認 DI 設定");
            if (dbContext is null)
                throw new ArgumentNullException(nameof(dbContext), "ESIntegrateSysEntities 不可為 null，請確認 DI 設定");
            _quoteScheduleService = quoteScheduleService;
            _emailService = emailService;
            db = dbContext;
        }

        /// <summary>
        /// 報價排程主頁面
        /// </summary>
        /// <returns>ActionResult，若未登入則導向登入頁，否則導向 QuotesView</returns>
        public ActionResult Index()
        {
            // 檢查是否已登入
            if (!Login_Authentication())
            {
                // 未登入則導向登入頁
                return RedirectToAction("Login", "Home");
            }
            else
            {
                // 已登入則導向報價主頁
                return RedirectToAction("QuotesView");
            }
        }
        /// <summary>
        /// 報價資料查詢與顯示
        /// </summary>
        /// <param name="page">目前頁碼</param>
        /// <param name="SalesID">業務ID</param>
        /// <param name="CustNo">客戶編號</param>
        /// <param name="Indate">起始日期</param>
        /// <param name="Indate2">結束日期</param>
        /// <param name="Sort">排序方式</param>
        /// <param name="Btn">按鈕名稱</param>
        /// <param name="Cancel">是否取消</param>
        /// <param name="EngSr">工程編號</param>
        /// <param name="CustMaterial">機種名稱</param>
        /// <returns>ActionResult，回傳報價資料頁面或部分頁面</returns>
        public ActionResult QuotesView(int? page, string SalesID, string CustNo, string Indate, string Indate2, string Sort, string Btn, bool? Cancel, string EngSr, string CustMaterial)
        {
            // 檢查是否已登入
            if (!Login_Authentication())
            {
                // 取得目前網址（包含查詢參數）
                var currentUrl = Url.Action("QuotesView", "QuoteSchedule", new { page, SalesID, CustNo, Indate, Indate2, Sort, Cancel });
                // 導向登入頁，並帶入 returnUrl 參數
                return RedirectToAction("Login", "Home", new { returnUrl = currentUrl });
            }

            // 設定每頁顯示筆數
            const int pageSize = 15;
            // 取得目前頁碼，預設為1
            int currentPage = page ?? 1;
            // 取得取消狀態，預設為false
            bool _Cancel = Cancel ?? false;
            // 取得部門編號
            string dept = (Session["Member"] as MemberViewModels)?.UDeptNo ?? string.Empty;
            // 取得使用者ID與姓名
            string uId = (Session["Member"] as MemberViewModels)?.fUserId ?? string.Empty;
            string fname = (Session["Member"] as MemberViewModels)?.fName ?? string.Empty;

            try
            {
                // 直接取得查詢結果（可能為 DTO 或內部模型），防止 service 回傳 null
                var rawResult = _quoteScheduleService.GetQuoteData(SalesID, CustNo, Indate, Indate2, Sort, Cancel, dept, EngSr, CustMaterial);

                // 將部門、使用者ID、姓名存入ViewBag
                ViewBag.DeptNo = dept;
                ViewBag.UserId = uId;
                ViewBag.Name = fname;

                // 需要確保傳入 View 的型別為 IPagedList<QuoteDataListDto>
                IEnumerable<QuoteDataListDto> dtoEnumerable = null;

                if (rawResult is IEnumerable<QuoteDataListDto> dtoList)
                {
                    dtoEnumerable = dtoList;
                }
                else if (rawResult is IEnumerable<QuoteQuery.QuoteDataList> modelList)
                {
                    // 將內部模型轉換為 DTO
                    dtoEnumerable = modelList.Select(m => new QuoteDataListDto
                    {
                        SalesName = m.SalesName,
                        CreateDate = m.CreateDate,
                        CustNo = m.CustNo,
                        CustName = m.CustName,
                        SalesNo = m.SalesNo,
                        EngSr = m.EngSr,
                        CustMaterial = m.CustMaterial,
                        WoNoAttri = m.WoNoAttri,
                        RequDate = m.RequDate,
                        Mark = m.Mark,
                        IEonwer = m.IEonwer,
                        IEQuoteDate = m.IEQuoteDate,
                        IEQuoteTDate = m.IEQuoteTDate
                    }).ToList();
                }
                else if (rawResult is System.Collections.IEnumerable any)
                {
                    // 未知集合型別，嘗試以反射或序列化弱型別轉換（保守處理：轉為空清單）
                    dtoEnumerable = new List<QuoteDataListDto>();
                }
                else
                {
                    dtoEnumerable = new List<QuoteDataListDto>();
                }

                // 取得分頁結果（若為空集合則會回傳空的分頁），避免 NullReferenceException
                var pagedResult = dtoEnumerable.ToPagedList(currentPage, pageSize);

                // 檢查是否為 Ajax 請求
                if (Request.IsAjaxRequest())
                {
                    // 回傳部分頁面（_QuotePartialView），只更新部分內容
                    return PartialView("_QuotePartialView", pagedResult);
                }
                else
                {
                    // 回傳完整頁面（QuotesView），使用 _QuoteLayout 版型
                    return View("QuotesView", "_QuoteLayout", pagedResult);
                }
            }
            catch (Exception ex)
            {
                // 記錄錯誤到伺服器日誌，並避免拋出未處理例外導致整個頁面崩潰
                try
                {
                    var logPath = Server.MapPath("~/App_Data/QuoteSchedule_error.log");
                    System.IO.File.AppendAllText(logPath, DateTime.Now.ToString("s") + " - QuotesView error: " + ex.ToString() + Environment.NewLine);
                }
                catch
                {
                    // 忽略日誌寫入錯誤
                }

                // 對於 AJAX 請求回傳可辨識的錯誤 JSON
                if (Request.IsAjaxRequest())
                {
                    return Json(new { error = true, message = "載入資料發生錯誤，請稍後重試。" }, JsonRequestBehavior.AllowGet);
                }

                // 對於一般頁面導向一個友善的錯誤頁面或顯示空列表
                ViewBag.ErrorMessage = "載入報價資料發生錯誤，已記錄。請聯絡系統管理員。";
                var emptyPaged = (new List<QuoteQuery.QuoteDataList>()).ToPagedList(1, pageSize);
                return View("QuotesView", "_QuoteLayout", emptyPaged);
            }
        }

        #region 業務開單
        /// <summary>
        /// 業務開單頁面
        /// </summary>
        /// <remarks>
        /// 檢查使用者是否已登入，若未登入則導向登入頁面，否則顯示業務開單頁面
        /// </remarks>
        /// <returns>
        /// ActionResult，回傳業務開單頁面或登入頁面
        /// </returns>
        public ActionResult QuoteSales()
        {
            // 檢查是否已登入
            if (!Login_Authentication())
            {
                // 未登入則導向登入頁
                return RedirectToAction("Login", "Home");
            }
            // 已登入則顯示業務開單頁面
            return View("QuoteSales", "_QuoteLayout");
        }

        /// <summary>
        /// 業務開單功能，處理業務填寫的報價需求並進行工單性質數量檢查，若超過限制則發送通知郵件。
        /// </summary>
        /// <param name="CustNo">客戶編號</param>
        /// <param name="EngSr">工程編號</param>
        /// <param name="SalesNo">業務單號</param>
        /// <param name="CustMaterial">機種名稱</param>
        /// <param name="WoNoAttri">工單性質編號</param>
        /// <param name="RequDate">需求日期</param>
        /// <param name="Mark">備註</param>
        /// <param name="OtherName">其他客戶名稱（當客戶編號為99時使用）</param>
        /// <returns>ActionResult，執行完畢後導向報價查詢頁面</returns>
        [HttpPost]
        public ActionResult QuoteSales(string CustNo, string EngSr, string SalesNo, string CustMaterial, int WoNoAttri, DateTime RequDate, string Mark, string OtherName)
        {
            // 取得目前登入者的使用者ID
            string uId = (Session["Member"] as MemberViewModels).fUserId;
            // 取得目前登入者的姓名
            string Sales = (Session["Member"] as MemberViewModels).fName;
            // 建立報價查詢物件
            QuoteQuery forSales = new QuoteQuery(db);
            // 執行業務開單資料新增
            forSales.QuoteSales(uId, CustNo, EngSr, SalesNo, CustMaterial, WoNoAttri, RequDate, Mark, OtherName);

            // 建立工單性質邏輯判斷物件
            WoNoAttriLogi attriLogi = new WoNoAttriLogi(db);
            // 設定工單性質編號
            attriLogi.AttriNo = WoNoAttri;
            // 設定需求日期
            attriLogi.RqDate = RequDate;
            // 檢查工單性質同日安排數量是否超過限制
            if (!attriLogi.Logi())
            {
                // 宣告客戶名稱字串
                var cust = "";
                // 取得工單性質名稱
                var att = (from w in db.ES_QuoteWoNoAttri
                           where w.KeyWorld == WoNoAttri
                           select new { w.WoNoAttri }).SingleOrDefault();

                // 判斷是否為其他客戶
                if (CustNo == "99")
                {
                    // 其他客戶直接使用OtherName
                    cust = OtherName;
                }
                else
                {
                    // 一般客戶組合編號與名稱
                    cust = "(" + CustNo + ")";
                    var customerResult = (from c in db.ES_QuoteCust
                                          where c.KeyWorld == CustNo
                                          select new { c.Customer }).SingleOrDefault();
                    if (customerResult != null)
                    {
                        cust += customerResult.Customer;
                    }
                }
                // 組合郵件內容
                string[] bodys = new string[] { cust, EngSr, SalesNo, CustMaterial, att.WoNoAttri, RequDate.ToShortDateString(), Mark, Sales };
                // 郵件主旨
                string subject = "報價安排數超過通知";
                // 根據主旨與內容取得收件人清單
                var recipients = _emailService.GetRecipients(db, subject, bodys);
                // 組合郵件內容 (可複用 EmailController 的 Woatt 方法，或直接於此組合)
                string bodyHtml = new ESIntegrateSys.Controllers.EmailController(_emailService, db).Woatt(subject, bodys);
                // 發送郵件並取得結果
                var result = _emailService.SendEmail(recipients, subject, bodyHtml);
                // 若發送失敗，將錯誤訊息存入 ViewBag 以供顯示
                if (!result.IsSuccess)
                    ViewBag.EmailError = $"寄信失敗：{result.ErrorMessage}";

            }

            // 新增完成後導向報價查詢頁面
            return RedirectToAction("QuotesView");
        }

        /// <summary>
        /// 編輯業務開單資料頁面
        /// </summary>
        /// <param name="sno">業務開單資料的唯一識別碼</param>
        /// <returns>ActionResult，回傳編輯頁面或登入頁面</returns>
        [HttpGet]
        public ActionResult SalesEdit(int? sno)
        {
            // 檢查 sno 是否有值，且已登入
            if (!sno.HasValue || !Login_Authentication())
            {
                // sno 缺漏或未登入則導向登入頁
                return RedirectToAction("Login", "Home");
            }
            // 依據 sno 取得業務開單資料
            var forsales = db.ES_QuoteForSales.Find(sno.Value);
            // 設定唯一識別碼
            ViewBag.Sno = forsales.sno;
            // 判斷客戶編號是否為 "99"，若是則顯示 OtherName，否則顯示客戶編號
            ViewBag.Cust = forsales.CustNo != "99" ? forsales.CustNo : forsales.OtherName;
            // 設定業務單號
            ViewBag.SalesNo = forsales.SalesNo;
            // 設定工程編號
            ViewBag.Engsr = forsales.EngSr;
            // 設定機種名稱
            ViewBag.CustMaterial = forsales.CustMaterial;
            // 設定工單性質
            ViewBag.WonoAttri = forsales.WoNoAttri;
            // 設定需求日期（格式化為 yyyy-MM-dd）
            ViewBag.RequDate = forsales.RequDate.Value.ToString("yyyy-MM-dd");
            // 設定備註
            ViewBag.Mark = forsales.Mark;
            // 回傳編輯頁面
            return View();
        }

        /// <summary>
        /// 編輯業務開單資料的 POST 方法，根據傳入參數更新指定的業務開單資料。
        /// </summary>
        /// <param name="sno">業務開單資料的唯一識別碼</param>
        /// <param name="engSr">工程編號</param>
        /// <param name="CustMaterial">機種名稱</param>
        /// <param name="WoNoAttri">工單性質編號</param>
        /// <param name="RequDate">需求日期</param>
        /// <param name="Mark">備註</param>
        /// <returns>ActionResult，執行完畢後導向報價查詢頁面</returns>
        [HttpPost]
        public ActionResult SalesEdit(int sno, string engSr, string CustMaterial, int WoNoAttri, DateTime RequDate, string Mark)
        {
            // 取得目前登入者的姓名
            string Sales = (Session["Member"] as MemberViewModels).fName;
            // 建立報價查詢物件
            QuoteQuery forsales = new QuoteQuery(db);
            // 執行業務開單資料編輯
            forsales.SalesEdit(sno, engSr, CustMaterial, WoNoAttri, RequDate, Mark, Sales);
            // 編輯完成後導向報價查詢頁面
            return RedirectToAction("QuotesView");
        }

        /// <summary>
        /// 取消指定的業務開單資料。
        /// </summary>
        /// <param name="sno">要取消的業務開單資料唯一識別碼。</param>
        /// <returns>ActionResult，執行完畢後導向報價查詢頁面。</returns>
        public ActionResult SalesCancel(int sno)
        {
            // 取得目前登入者的姓名
            string Sales = (Session["Member"] as MemberViewModels).fName;
            // 建立報價查詢物件
            QuoteQuery cancel = new QuoteQuery(db);
            // 執行取消業務開單資料
            cancel.SalesCancel(sno, Sales);
            // 取消完成後導向報價查詢頁面
            return RedirectToAction("QuotesView");
        }
        #endregion

        #region IE 報價
        /// <summary>
        /// IE 報價頁面，根據指定的業務開單資料編號 sno，載入相關資料並進行 IE 報價狀態處理。
        /// </summary>
        /// <param name="sno">業務開單資料的唯一識別碼</param>
        /// <returns>ActionResult，回傳 IE 報價頁面或登入頁面</returns>
        public ActionResult QuoteIE(int sno)
        {
            // 檢查是否已登入
            if (!Login_Authentication())
            {
                // 未登入則導向登入頁
                return RedirectToAction("Login", "Home");
            }

            // 取得目前登入者的使用者ID
            string uId = (Session["Member"] as MemberViewModels).fUserId;
            // 依據 sno 取得業務開單資料
            var forsales = db.ES_QuoteForSales.Find(sno);

            // 判斷客戶編號是否為 "99"，若是則顯示 OtherName，否則顯示客戶編號
            if (forsales.CustNo == "99")
            {
                ViewBag.Other = forsales.OtherName;
                ViewBag.Cust = forsales.CustNo;
            }
            else
            {
                ViewBag.Cust = forsales.CustNo;
            }
            // 設定業務開單資料相關欄位至 ViewBag
            ViewBag.Sno = forsales.sno;
            ViewBag.SalesNo = forsales.SalesNo;
            ViewBag.Engsr = forsales.EngSr;
            ViewBag.CustMaterial = forsales.CustMaterial;
            ViewBag.WonoAttri = forsales.WoNoAttri;
            ViewBag.RequDate = forsales.RequDate.Value.ToString("yyyy-MM-dd");
            ViewBag.Mark = forsales.Mark;

            // 取得 IE 報價資料
            var forIE = db.ES_QuoteForIE.FirstOrDefault(o => o.id == sno);
            if (forIE != null)
            {
                // 若有 IE 報價資料，則載入報價日與測試報價日
                ViewBag.IEQuoteDate = (forIE.IEQuoteDate.HasValue && forIE.IEQuoteDate.Value != DateTime.MinValue)
                ? forIE.IEQuoteDate.Value.ToString("yyyy-MM-dd") : string.Empty;

                ViewBag.IEQuoteTDate = (forIE.IEQuoteTDate.HasValue && forIE.IEQuoteTDate.Value != DateTime.MinValue)
                ? forIE.IEQuoteTDate.Value.ToString("yyyy-MM-dd") : string.Empty;

                // 將 IE 備註合併至原備註
                ViewBag.Mark += " " + forIE.IEMark;

                // 設定 IE 狀態為 "U"（代表正在編輯或更新）
                forIE.IEStatus = "U";
            }
            else
            {
                // 若無 IE 報價資料，則新增一筆
                forIE = new ES_QuoteForIE
                {
                    id = sno,
                    IEonwer = uId,
                    IEStatus = "U",
                };
                db.ES_QuoteForIE.Add(forIE);
            }
            // 儲存資料庫變更
            db.SaveChanges();
            // 回傳 IE 報價頁面，使用 _QuoteLayout 版型
            return View("QuoteIE", "_QuoteLayout");
        }


        /// <summary>
        /// IE 報價資料儲存功能，處理 IE 報價日、測試報價日、備註等欄位的更新。
        /// </summary>
        /// <param name="sno">業務開單資料的唯一識別碼</param>
        /// <param name="IEQuoteDate">IE 報價日</param>
        /// <param name="IEQuoteTDate">IE 測試報價日</param>
        /// <param name="Mark">原始備註</param>
        /// <param name="UpdateMark">IE 備註</param>
        /// <returns>ActionResult，執行完畢後導向報價查詢頁面</returns>
        [HttpPost]
        public ActionResult QuoteIE(int sno, DateTime? IEQuoteDate, DateTime? IEQuoteTDate, string Mark, string UpdateMark)
        {
            // 取得目前登入者的使用者ID
            string uId = (Session["Member"] as MemberViewModels).fUserId;
            // 建立報價查詢物件
            QuoteQuery quoteIE = new QuoteQuery(db);
            // 執行 IE 報價資料更新
            quoteIE.QuoteIE(uId, sno, IEQuoteDate, IEQuoteTDate, Mark, UpdateMark);
            // 更新完成後導向報價查詢頁面
            return RedirectToAction("QuotesView");
        }

        #endregion

        #region 報價使用檢查

        /// <summary>
        /// 清除指定 sno 的 IE 報價資料鎖定狀態。
        /// </summary>
        /// <param name="sno">業務開單資料的唯一識別碼。</param>
        /// <returns>HttpStatusCodeResult，回傳 200 表示成功。</returns>
        [HttpPost]
        public ActionResult ClearIEStatus(int sno)
        {
            // 根據 sno 查詢 IE 報價資料
            var record = db.ES_QuoteForIE.FirstOrDefault(o => o.id == sno);
            if (record != null)
            {
                record.IEStatus = ""; // 清空 IE 報價資料的狀態
                db.SaveChanges();     // 儲存資料庫變更
            }

            // 回傳 200 狀態碼表示成功
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        /// <summary>
        /// 檢查指定 sno 的 IE 報價資料是否被鎖定（IEStatus 為 "U"），並回傳鎖定狀態及鎖定者。
        /// </summary>
        /// <param name="sno">業務開單資料的唯一識別碼</param>
        /// <returns>
        /// JsonResult，若有鎖定則回傳 isChecked=true 並附上 lockedBy（鎖定者），否則回傳 isChecked=false。
        /// </returns>
        public JsonResult CheckHandleStatus(int sno)
        {
            // 依據 sno 查詢 IE 報價資料，且狀態為 "U"（表示已鎖定）
            var forIE = db.ES_QuoteForIE.FirstOrDefault(o => o.id == sno && o.IEStatus == "U");
            if (forIE != null)
            {
                // 若有資料，回傳已鎖定狀態及鎖定者
                return Json(new { isChecked = true, lockedBy = forIE.IEonwer }, JsonRequestBehavior.AllowGet);
            }

            // 若查無資料，回傳未鎖定狀態
            return Json(new { isChecked = false }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 處理 IE 報價資料的勾選狀態變更，根據 isChecked 參數新增或移除鎖定狀態。
        /// </summary>
        /// <param name="sno">業務開單資料的唯一識別碼</param>
        /// <param name="isChecked">是否勾選（true 表示鎖定，false 表示解除鎖定）</param>
        /// <returns>JsonResult，回傳操作結果狀態</returns>
        [HttpPost]
        public JsonResult HandleCheckboxChange(int sno, bool isChecked)
        {
            // 取得目前登入者的使用者ID
            string uId = (Session["Member"] as MemberViewModels).fUserId;
            // 依據 sno 查詢 IE 報價資料
            var forIE = db.ES_QuoteForIE.FirstOrDefault(o => o.id == sno);

            //if (isChecked)
            //{
            // 其他使用者已經勾選，回傳錯誤提示
            // return Json(new { isAlreadyChecked = true });
            //}

            // 若查有 IE 報價資料
            if (forIE != null)
            {
                // 若目前狀態為 "U"（已鎖定），則移除該筆資料
                if (forIE.IEStatus == "U")
                {
                    db.ES_QuoteForIE.Remove(forIE);
                    //forIE.IEStatus = "";
                }
                else
                {
                    // 否則設定目前使用者為鎖定者，並將狀態設為 "U"
                    forIE.IEonwer = uId;
                    forIE.IEStatus = "U";
                }
            }
            else
            {
                // 若查無資料，則新增一筆鎖定紀錄
                forIE = new ES_QuoteForIE
                {
                    id = sno,
                    IEonwer = uId,
                    IEStatus = "U",
                };
                db.ES_QuoteForIE.Add(forIE);
            }
            // 儲存資料庫變更
            db.SaveChanges();

            // 回傳成功狀態
            return Json(new { status = "success" });
        }

        #endregion

        #region 上傳
        /// <summary>
        /// 上傳檔案並記錄相關資訊
        /// </summary>
        /// <param name="name">檔案名稱</param>
        /// <param name="sno">業務開單資料的唯一識別碼</param>
        /// <param name="file">上傳的檔案物件</param>
        /// <returns>ActionResult，執行完畢後導向報價查詢頁面</returns>
        [HttpPost]
        public ActionResult Upload(string name, int sno, HttpPostedFileBase file)
        {
            // 檢查檔案是否存在且大小大於 0
            if (file != null && file.ContentLength > 0)
            {
                // 開啟資料庫交易
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // 取得目前登入者的部門編號
                        string deptNo = (Session["Member"] as MemberViewModels).UDeptNo;
                        // 取得目前登入者的使用者ID
                        string ieonwer = (Session["Member"] as MemberViewModels).fUserId;
                        // 建立新檔案上傳紀錄
                        var record = new Models.ES_QuoteUploadRecords { Q_sno = sno, Name = name };
                        db.ES_QuoteUploadRecords.Add(record);
                        db.SaveChanges();

                        // TODO: 檔案存檔路徑不要存絕對路徑
                        // 取得檔案名稱
                        var fileName = Path.GetFileName(file.FileName);
                        // 組合檔案儲存路徑
                        var filePath = Path.Combine(Server.MapPath("~/UploadedFiles"), fileName);
                        // 儲存檔案至指定路徑
                        file.SaveAs(filePath);

                        // 建立檔案詳細紀錄
                        var fileRecord = new Models.ES_QuoteUploadFiles
                        {
                            RecordId = record.Q_sno, // 使用 record 的 Q_sno 作為 RecordId
                            FilePath = filePath, // 檔案儲存路徑
                            UploadTime = DateTime.Now, // 上傳時間
                            FileName = fileName, // 檔案名稱
                            DeptNo = deptNo // 部門編號
                        };

                        // 新增檔案詳細紀錄至資料庫
                        db.ES_QuoteUploadFiles.Add(fileRecord);
                        db.SaveChanges();

                        // 提交交易
                        transaction.Commit();

                        // 若部門為 IE，則發送 IE 報價完成通知郵件
                        if (deptNo == "IE")
                        {
                            string[] bodys = new string[] { };
                            // 取得相關資料
                            var data = from a in db.ES_QuoteForSales
                                       join b in db.ES_QuoteForIE on a.sno equals b.id into g
                                       from b in g.DefaultIfEmpty()
                                       where a.sno == sno
                                       select new
                                       {
                                           a.EngSr,
                                           a.CustMaterial,
                                           a.RequDate,
                                           a.SalesId,
                                           ieonwer
                                       };
                            // 組合郵件內容
                            foreach (var item in data)
                            {
                                bodys = new string[] { item.EngSr, item.CustMaterial, item.RequDate.Value.ToShortDateString(), item.SalesId, item.ieonwer };
                            }

                            // 郵件主旨
                            string subject = "IE報價完成通知";
                            // 取得 EmailController 並發送郵件
                            var otherCtrl = DependencyResolver.Current.GetService<EmailController>();
                            otherCtrl.SendTestEmail(subject, bodys);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 發生例外時回滾交易
                        transaction.Rollback();
                        // 寫入錯誤 log
                        System.IO.File.AppendAllText(Server.MapPath("~/App_Data/upload_error.log"),
                            DateTime.Now + " - " + ex.ToString() + Environment.NewLine);
                        // 回傳錯誤訊息
                        return new HttpStatusCodeResult(500, ex.Message);
                    }
                }
            }

            // 上傳完成後導向報價查詢頁面
            return RedirectToAction("QuotesView", "QuoteSchedule");
        }

        /// <summary>
        /// 下載指定 sno 的上傳檔案。
        /// </summary>
        /// <param name="sno">檔案的唯一識別碼。</param>
        /// <returns>若找到檔案則回傳檔案下載，否則回傳 404。</returns>
        public ActionResult Download(int sno)
        {
            // 根據 sno 查詢檔案紀錄
            var fileRecord = db.ES_QuoteUploadFiles.Find(sno);
            // 若查無檔案則回傳 404
            if (fileRecord == null)
            {
                return HttpNotFound();
            }

            // 讀取檔案內容為 byte 陣列
            byte[] fileBytes = System.IO.File.ReadAllBytes(fileRecord.FilePath);
            // 取得檔案名稱
            string fileName = fileRecord.FileName;
            // 回傳檔案下載，MIME 型態為 octet-stream
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        /// <summary>
        /// 取得指定記錄的檔案詳細資料，依據部門權限篩選，並回傳部分檢視頁面。
        /// </summary>
        /// <param name="recordId">記錄的唯一識別碼</param>
        /// <returns>若有檔案則回傳部分檢視頁面，否則回傳 404</returns>
        public ActionResult RecordDetails(int recordId)
        {
            // 取得目前登入者的部門編號
            string deptNo = (Session["Member"] as MemberViewModels).UDeptNo;
            // 依據 recordId 查詢所有檔案記錄
            var recordQuery = db.ES_QuoteUploadFiles.Where(r => r.RecordId == recordId);

            // 若部門為 IE，則只顯示 IE 部門的檔案
            if (deptNo == "IE")
            {
                recordQuery = recordQuery.Where(d => d.DeptNo == "IE");
            }

            // 將查詢結果轉換成 List
            var record = recordQuery.ToList();
            // 將記錄編號存入 ViewBag
            ViewBag.no = recordId;
            // 將部門編號存入 ViewBag
            ViewBag.dept = deptNo;
            // 若查無資料則回傳 404
            if (!record.Any())
            {
                return HttpNotFound();
            }
            // 回傳部分檢視頁面，顯示檔案資料
            return PartialView("_FilesPartialView", record);
        }

        /// <summary>
        /// 刪除指定 sno 的上傳檔案記錄。
        /// </summary>
        /// <param name="sno">檔案的唯一識別碼。</param>
        /// <param name="rcode">記錄的唯一識別碼（未使用）。</param>
        /// <returns>執行完畢後導向報價查詢頁面。</returns>
        public ActionResult DelDownload(int sno, int rcode)
        {
            // 根據 sno 查詢檔案紀錄
            var fileRecord = db.ES_QuoteUploadFiles.Find(sno);
            // 若查有檔案則刪除
            if (fileRecord != null)
            {
                db.ES_QuoteUploadFiles.Remove(fileRecord);
                db.SaveChanges();
            }

            // RedirectToAction 的第二個參數需要一個命名參數物件，直接將 rcode 傳入會出現錯誤
            // 刪除完成後導向報價查詢頁面
            return RedirectToAction("QuotesView");
        }

        #endregion

        #region 下拉選單
        /// <summary>
        /// 取得業務人員下拉選單資料。
        /// </summary>
        /// <remarks>
        /// 從 ES_Member 資料表中篩選部門編號為 "SD" 的業務人員，
        /// 並將其 fUserId 與 fName 組成下拉選單項目，
        /// 首項為 "請選擇..."，回傳 JSON 格式資料。
        /// </remarks>
        /// <returns>
        /// ActionResult，回傳 JSON 格式的業務人員下拉選單資料。
        /// </returns>
        [HttpGet]
        public ActionResult QuoteSalesdrop()
        {
            // 從 ES_Member 資料表中篩選部門編號為 "SD" 的業務人員
            var descriptions = db.ES_Member.Where(m => m.Dept_No == "SD")
                .Select(a => new { Value = a.fUserId, Text = a.fName }).ToList();
            // 在清單最前面插入 "請選擇..." 項目
            descriptions.Insert(0, new { Value = "", Text = "請選擇..." });
            // 回傳 JSON 格式資料
            return Json(descriptions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得客戶下拉選單資料。
        /// </summary>
        /// <remarks>
        /// 從 ES_QuoteCust 資料表取得所有客戶，
        /// 並將 KeyWorld 與 Customer 組成顯示文字。
        /// 首項插入「請選擇...」，末尾插入「其他」選項。
        /// 回傳 JSON 格式資料供前端下拉選單使用。
        /// </remarks>
        /// <returns>
        /// ActionResult，回傳 JSON 格式的客戶下拉選單資料。
        /// </returns>
        [HttpGet]
        public ActionResult QuoteCust()
        {
            // 從 ES_QuoteCust 資料表取得所有客戶，組成下拉選單資料
            var descriptions = db.ES_QuoteCust.Select(a => new { Value = a.KeyWorld, Text = "(" + a.KeyWorld + ")" + a.Customer }).ToList();
            // 在清單最前面插入「請選擇...」選項
            descriptions.Insert(0, new { Value = "", Text = "請選擇..." });
            // 在清單最後插入「其他」選項
            descriptions.Add(new { Value = "99", Text = "其他" });
            // 回傳 JSON 格式資料
            return Json(descriptions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得工單性質下拉選單資料。
        /// </summary>
        /// <remarks>
        /// 從 ES_QuoteWoNoAttri 資料表取得所有工單性質，
        /// 並將 KeyWorld 與 WoNoAttri 組成下拉選單項目。
        /// 首項插入「請選擇...」選項。
        /// 回傳 JSON 格式資料供前端下拉選單使用。
        /// </remarks>
        /// <returns>
        /// ActionResult，回傳 JSON 格式的工單性質下拉選單資料。
        /// </returns>
        public ActionResult QuoteWoNoAttri()
        {
            // 從 ES_QuoteWoNoAttri 資料表取得所有工單性質，組成下拉選單資料
            var descriptions = db.ES_QuoteWoNoAttri.Select(a => new { Value = a.KeyWorld, Text = a.WoNoAttri }).ToList();
            // 在清單最前面插入「請選擇...」選項
            descriptions.Insert(0, new { Value = 0, Text = "請選擇..." });
            // 回傳 JSON 格式資料
            return Json(descriptions, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Login 驗證相關Class
        /// <summary>
        /// 驗證使用者是否已登入，並將角色與使用者ID存入ViewBag
        /// </summary>
        /// <returns>
        /// 若已登入則回傳 true，否則回傳 false
        /// </returns>
        public bool Login_Authentication()
        {
            // 檢查 Session["Member"] 是否有值，判斷是否已登入
            if (Session["Member"] != null)
            {
                // 取得使用者ID
                string UserId = (Session["Member"] as MemberViewModels).fUserId;
                // 取得角色ID
                string RoleId = (Session["Member"] as MemberViewModels).ROLE_ID;
                // 將角色ID存入 ViewBag
                ViewBag.RoleId = RoleId;
                // 將使用者ID存入 ViewBag
                ViewBag.UserId = UserId;
                // 已登入，回傳 true
                return true;
            }
            else
            {
                // 未登入，回傳 false
                return false;
            }
        }
        #endregion

        #region Excel ActionFunction
        /// <summary>
        /// 匯出報價資料為 Excel 檔案。
        /// </summary>
        /// <param name="SalesID">業務ID</param>
        /// <param name="CustNo">客戶編號</param>
        /// <param name="Indate">起始日期</param>
        /// <param name="Indate2">結束日期</param>
        /// <param name="Sort">排序方式</param>
        /// <param name="Cancel">是否取消</param>
        /// <param name="EngSr">工程編號</param>
        /// <param name="CustMaterial">機種名稱</param>
        /// <returns>Excel 檔案下載</returns>
        public ActionResult ExportToExcel(string SalesID, string CustNo, string Indate, string Indate2, string Sort, bool? Cancel, string EngSr, string CustMaterial)
        {
            // 建立 Excel 工作簿
            XSSFWorkbook workbook = new XSSFWorkbook();

            // 建立報價查詢物件
            QuoteQuery qq = new QuoteQuery(db);
            // 宣告報價資料清單
            List<QuoteQuery.QuoteDataList> quoteDataLists;
            // 取得部門編號
            string dept = (Session["Member"] as MemberViewModels).UDeptNo;
            // 查詢報價資料
            quoteDataLists = qq.QuoteDataSearch(SalesID, CustNo, Indate, Indate2, Sort, Cancel, dept, EngSr, CustMaterial);

            // 建立工作表
            ISheet sheet = workbook.CreateSheet("報價資料");
            // 建立標題列
            sheet.CreateRow(0);
            // 標題文字陣列
            string[] HeaderTexts = { "業務", "填寫時間", "客戶別", "業務單號", "工程編號", "機種名稱", "工單性質", "需求日期", "備註", "IE負責人", "IE報價日", "IE測試報價日" };
            // 填入標題
            for (int i = 0; i < HeaderTexts.Length; i++)
            {
                sheet.GetRow(0).CreateCell(i).SetCellValue(HeaderTexts[i]);
            }

            // 塞入資料
            int currentRow = 1;
            foreach (var dataRow in quoteDataLists)
            {
                // 建立新資料列
                IRow row = sheet.CreateRow(currentRow++);
                row.CreateCell(0).SetCellValue(dataRow.SalesName);  // 業務
                row.CreateCell(1).SetCellValue(dataRow.CreateDate.ToString("yyyy/MM/dd"));  // 填寫時間
                row.CreateCell(2).SetCellValue($"({dataRow.CustNo}){dataRow.CustName}");  // 客戶別
                row.CreateCell(3).SetCellValue(dataRow.SalesNo);  // 業務單號
                row.CreateCell(4).SetCellValue(dataRow.EngSr);  // 工程編號
                row.CreateCell(5).SetCellValue(dataRow.CustMaterial);  // 機種名稱
                row.CreateCell(6).SetCellValue(dataRow.WoNoAttri);  // 工單性質
                row.CreateCell(7).SetCellValue(dataRow.RequDate.ToString("yyyy/MM/dd"));  // 需求日期
                row.CreateCell(8).SetCellValue(dataRow.Mark);  // 備註
                row.CreateCell(9).SetCellValue(dataRow.IEonwer);  // IE負責人
                row.CreateCell(10).SetCellValue(dataRow.IEQuoteDate != DateTime.MinValue ? dataRow.IEQuoteDate.ToString("yyyy/MM/dd") : "");  // IE報價日
                row.CreateCell(11).SetCellValue(dataRow.IEQuoteTDate != DateTime.MinValue ? dataRow.IEQuoteTDate.ToString("yyyy/MM/dd") : "");   // IE測試報價日
            }
            // 設定欄位自動調整寬度
            for (int i = 0; i < HeaderTexts.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }
            // 設定檔案名稱
            string fileName = "報價資料匯出" + DateTime.Now + ".xlsx";

            // 儲存 Excel 檔案
            using (MemoryStream stream = new MemoryStream())
            {
                workbook.Write(stream);
                var content = stream.ToArray();
                // 回傳 Excel 檔案
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
        #endregion
    }
}