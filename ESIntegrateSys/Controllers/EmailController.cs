using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ESIntegrateSys.Models;
using ESIntegrateSys.Models_QSchedule;

namespace ESIntegrateSys.Controllers
{

    /// <summary>
    /// EmailController 負責處理與電子郵件相關的操作，例如發送測試郵件。
    /// </summary>
    public class EmailController : Controller
    {
        /// <summary>
        ///  Entity Framework 資料庫存取物件，負責連接 ESIntegrateSysEntities 資料庫。
        /// </summary>
        private readonly ESIntegrateSysEntities db;
        /// <summary>
        /// The email service (由 DI 注入)
        /// </summary>
        private readonly ESIntegrateSys.Services.EmailServices.IEmailService _emailService;

        /// <summary>
        /// 建構函式，使用 DI 注入 EmailService 與資料庫物件。
        /// </summary>
        public EmailController(ESIntegrateSys.Services.EmailServices.IEmailService emailService, ESIntegrateSysEntities dbContext)
        {
            if (emailService is null)
                throw new ArgumentNullException(nameof(emailService), "EmailService 不可為 null");
            if (dbContext is null)
                throw new ArgumentNullException(nameof(dbContext), "ESIntegrateSysEntities 不可為 null，請確認 DI 設定");
            _emailService = emailService;
            db = dbContext;
        }

        /// <summary>
        /// 根據主旨與內容，選擇收件人並發送測試郵件。
        /// </summary>
        /// <param name="subjects">郵件主旨</param>
        /// <param name="bodys">郵件內容陣列</param>
        /// <returns>發送結果訊息</returns>
        public ActionResult SendTestEmail(string subjects, string[] bodys)
        {

            // 檢查 db 是否為 null，避免 NullReferenceException
            if (db is null)
                return Content("錯誤：資料庫物件 (db) 為 null，請確認 DI 設定或建構函式注入");

            // 根據主旨與內容取得收件人清單
            var toEmails = _emailService.GetRecipients(db, subjects, bodys);

            // 取得郵件主旨
            var subject = subjects;
            // 取得郵件內容 (HTML 格式)
            var body = Woatt(subjects, bodys); // 根據主旨與內容陣列產生郵件內容

            // 發送郵件並取得結果
            var result = _emailService.SendEmail(toEmails, subject, body);
            if (!result.IsSuccess)
            {
                ViewBag.EmailError = $"寄信失敗：{result.ErrorMessage}";
                return Content(ViewBag.EmailError);
            }
            return Content("寄信成功");
        }
        /// <summary>
        /// 根據主旨與內容陣列產生郵件內容 (HTML 格式)。
        /// </summary>
        /// <param name="subj">郵件主旨</param>
        /// <param name="bodys">郵件內容陣列</param>
        /// <returns>郵件內容 (HTML)</returns>
        public string Woatt(string subj, string[] bodys)
        {
            string body = string.Empty;
            try
            {
                switch (subj)
                {
                    /*
                    string[] bs = new string[] { 0cust, 1EngSr, 2SalesNo, 3CustMaterial, 4att.WoNoAttri, 5RequDate.ToShortDateString(), 6Mark , Sales };
                    */
                    case "報價安排數超過通知":
                        body = "<table style=width: 100 %;  border=1><tbody><tr style=background-color:#E8FFF5><td>業務</td><td>客戶別</td><td>業務單號</td><td>工程編號</td><td>機種名稱</td><td>工單性質</td><td>需求日期</td><td>備註</td></tr>";
                        body += "<tr>" +
                            "<td>" + bodys[7] + "</td><td>" + bodys[0] + "</td><td>" + bodys[2] + "</td><td>" + bodys[1] + "</td>" +
                            "<td>" + bodys[3] + "</td><td>" + bodys[4] + "</td><td>" + bodys[5] + "</td><td>" + bodys[6] + "</td>" +
                            "</tr></tbody></table>";
                        body += "<p>http://192.168.4.70/QuoteSchedule/QuotesView" + "?CustMaterial=" + bodys[3] + "&indate=" + bodys[5] + "</p>";
                        break;
                    case "修改報價通知":
                        body = "<h1>序號 : " + bodys[10] + " 修改報價</h1>";
                        body += "<table style=width: 100 %;  border=1><tbody><tr style=background-color:#E8FFF5><td></td><td>業務</td><td>工程編號</td><td>機種名稱</td><td>工單性質</td><td>需求日期</td><td>備註</td></tr>";
                        body +=
                            "<tr><td>異動前</td><td>" + bodys[11] + "</td><td>" + bodys[0] + "</td><td>" + bodys[2] + "</td><td>" + bodys[4] + "</td><td>" + bodys[6] + "</td><td>" + bodys[8] + "</td></tr>" +
                            "<tr style=background-color:#E2C2DE><td>異動後</td><td>" + bodys[11] + "</td><td>" + bodys[1] + "</td><td>" + bodys[3] + "</td><td>" + bodys[5] + "</td><td>" + bodys[7] + "</td><td>" + bodys[9] + "</td></tr></tbody></table>";
                        body += "<p>http://192.168.4.70/QuoteSchedule/QuotesView" + "?Indate=" + HttpUtility.UrlEncode(bodys[7]) + "&CustMaterial=" + HttpUtility.UrlEncode(bodys[3]) + "</p>";
                        break;
                    case "取消報價通知":
                        body = "<h1>序號 : " + bodys[0] + " 取消報價</h1>";
                        body += "<table style=width: 100 %;  border=1><tbody><tr style=background-color:#E8FFF5><td>業務</td><td>填寫時間</td><td>客戶別</td><td>業務單號</td><td>工程編號</td><td>機種名稱</td><td>工單性質</td><td>需求日期</td><td>備註</td></tr>";
                        body += "<tr>" +
                            "<td>" + bodys[1] + "</td><td>" + bodys[2] + "</td><td>" + bodys[3] + "</td><td>" + bodys[4] + "</td>" +
                            "<td>" + bodys[5] + "</td><td>" + bodys[6] + "</td><td>" + bodys[7] + "</td><td>" + bodys[8] + "</td>" +
                            "<td>" + bodys[9] + "</td></tr></tbody></table>";
                        body += "<p>http://192.168.4.70/QuoteSchedule/QuotesView" + "?EngSr=" + HttpUtility.UrlEncode(bodys[5]) + "&CustMaterial=" + HttpUtility.UrlEncode(bodys[6]) + "&Indate=" + HttpUtility.UrlEncode(bodys[8]) + "</p>";
                        break;
                    case "IE報價完成通知":
                        //item.EngSr, item.CustMaterial, item.RequDate.Value.ToShortDateString(), item.SalesId
                        body = "<h1>IE報價完成通知</h1>";
                        body += "<p>http://192.168.4.70/QuoteSchedule/QuotesView" + "?EngSr=" + HttpUtility.UrlEncode(bodys[0]) + "&CustMaterial=" + HttpUtility.UrlEncode(bodys[1]) + "&Indate=" + HttpUtility.UrlEncode(bodys[2]) + "&SalesId=" + HttpUtility.UrlEncode(bodys[3]) + "</p>";
                        break;
                    default:
                        body = "<h1>序號 : " + bodys[0] + subj + "</h1>";
                        body += "<table style=width: 100 %;  border=1><tbody><tr style=background-color:#E8FFF5><td>填寫時間</td><td>客戶別</td><td>業務單號</td><td>工程編號</td><td>機種名稱</td><td>工單性質</td><td>需求日期</td><td>備註</td></tr>";
                        body += "<tr>" +
                            "<td>" + bodys[1] + "</td><td>" + bodys[2] + "</td><td>" + bodys[3] + "</td><td>" + bodys[4] + "</td>" +
                            "<td>" + bodys[5] + "</td><td>" + bodys[6] + "</td><td>" + bodys[7] + "</td><td>" + bodys[8] + "</td>" +
                            "</td></tr></tbody></table>";
                        body += "<p>http://192.168.4.70/QuoteSchedule/QuotesView" + "?EngSr=" + HttpUtility.UrlEncode(bodys[5]) + "&CustMaterial=" + HttpUtility.UrlEncode(bodys[6]) + "&Indate=" + HttpUtility.UrlEncode(bodys[8]) + "</p>";
                        break;
                }
            }
            catch (Exception e)
            {
                // 僅記錄例外，不拋出，避免 Controller 失效
                // TODO: 可導入 logging 機制
            }
            return body;
        }

    }
}