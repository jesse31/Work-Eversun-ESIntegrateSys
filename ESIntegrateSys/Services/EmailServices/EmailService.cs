using ESIntegrateSys.Models;
using ESIntegrateSys.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace ESIntegrateSys.Services.EmailServices
{

    /// <summary>
    /// EmailService 實作 IEmailService，負責寄送郵件
    /// </summary>
    public class EmailService : IEmailService
    {
        /// <summary> SMTP 伺服器位址 </summary>
        private readonly string smtpServer;
        /// <summary> SMTP 連接埠 </summary>
        private readonly int smtpPort;
        /// <summary> SMTP 使用者帳號 </summary>
        private readonly string smtpUser;
        /// <summary> SMTP 密碼 </summary>
        private readonly string smtpPass;

        /// <summary>
        /// 建構函式，讀取 SMTP 設定
        /// </summary>
        public EmailService()
        {
            // 取得 SMTP 伺服器位址
            smtpServer = System.Configuration.ConfigurationManager.AppSettings["SmtpServer"];
            // 取得 SMTP 連接埠，若未設定則預設為 25
            smtpPort = System.Configuration.ConfigurationManager.AppSettings["SmtpPort"] != null ? int.Parse(System.Configuration.ConfigurationManager.AppSettings["SmtpPort"]) : 25;
            // 取得 SMTP 使用者帳號
            smtpUser = System.Configuration.ConfigurationManager.AppSettings["SmtpUser"];
            // 取得 SMTP 密碼
            smtpPass = System.Configuration.ConfigurationManager.AppSettings["SmtpPass"];
        }

        #region

        /// <summary>
        /// 根據郵件主旨與內容，從資料庫取得適合的收件人電子郵件清單。
        /// </summary>
        /// <param name="db">資料庫物件，提供會員資料查詢。</param>
        /// <param name="subjects">郵件主旨，用於判斷收件人範圍。</param>
        /// <param name="bodys">郵件內容陣列，部分元素用於收件人條件判斷。</param>
        /// <returns>收件人電子郵件清單（List&lt;string&gt;）。</returns>
        /// <remarks>
        /// 根據不同主旨，收件人條件如下：
        /// - 報價安排數超過通知：業務姓名或 IT 部門
        /// - 修改報價通知/取消報價通知：IE 或 IT 部門，排除 Brian（fUserId = "00081"）
        /// - IE報價完成通知：業務、IE 或 IT 部門，排除 Brian
        /// - 其他主旨：使用者編號或 IT 部門
        /// </remarks>
        public IEnumerable<string> GetRecipients(ESIntegrateSysEntities db, string subjects, string[] bodys)
        {
            // 只查詢一次資料庫，根據主旨合併條件，減少記憶體消耗
            List<string> toEmails;
            switch (subjects)
            {
                case "報價安排數超過通知":
                    // 業務姓名或 IT 部門
                    // 排除離職員工（fStatus == true）By Jesse 20260211
                    string fNameBody = bodys.Length >= 7 ? bodys[7] : string.Empty;
                    toEmails = db.ES_Member
                        .Where(m => (m.fName == fNameBody || m.Dept_No == "IT") && m.fStatus == true)
                        .Select(m => m.email)
                        .ToList();
                    break;
                case "修改報價通知":
                case "取消報價通知":
                    // IE 或 IT 部門，排除 Brian
                    // 排除離職員工（fStatus == true）
                    toEmails = db.ES_Member
                        .Where(m => (m.Dept_No == "IE" || m.Dept_No == "IT") && m.fUserId != "00081" && m.fStatus == true)
                        .Select(m => m.email)
                        .ToList();
                    break;
                case "IE報價完成通知":
                    // 業務、IE 或 IT 部門，排除 Brian
                    // 排除離職員工（fStatus == true）By Jesse 20260211
                    string fUserId1 = bodys.Length > 3 ? bodys[3] : string.Empty;
                    toEmails = db.ES_Member
                        .Where(m => (m.fUserId == fUserId1 || m.Dept_No == "IE" || m.Dept_No == "IT")
                            && m.fUserId != "00081" && m.fStatus == true)
                        .Select(m => m.email)
                        .ToList();
                    break;
                default:
                    // 使用者編號或 IT 部門
                    // 排除離職員工（fStatus == true）By Jesse 20260211
                    string fUserIdBody = bodys.Length >= 8 ? bodys[8] : string.Empty;
                    toEmails = db.ES_Member
                        .Where(m => (m.fUserId == fUserIdBody || m.Dept_No == "IT") && m.fStatus == true)
                        .Select(m => m.email)
                        .ToList();
                    break;
            }
            // 回傳收件人清單
            return toEmails;
        }

        #endregion

        /// <summary>
        /// 發送電子郵件
        /// </summary>
        /// <param name="toEmails">收件人電子郵件清單</param>
        /// <param name="subject">郵件主旨</param>
        /// <param name="body">郵件內容 (HTML)</param>
        public EmailSendResultDto SendEmail(IEnumerable<string> toEmails, string subject, string body)
        {
            var result = new EmailSendResultDto();
            #region 防呆檢查
            if (string.IsNullOrWhiteSpace(smtpServer))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "SMTP 伺服器位址未設定";
                return result;
            }
            if (string.IsNullOrWhiteSpace(smtpUser))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "SMTP 使用者帳號未設定";
                return result;
            }
            if (string.IsNullOrWhiteSpace(smtpPass))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "SMTP 密碼未設定";
                return result;
            }
            if (toEmails == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "收件人不可為空";
                return result;
            }
            var validRecipients = toEmails.Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
            if (!validRecipients.Any())
            {
                result.IsSuccess = false;
                result.ErrorMessage = "所有收件人皆為空，無法發送郵件";
                return result;
            }
            if (string.IsNullOrWhiteSpace(subject))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "郵件主旨不可為空";
                return result;
            }
            if (string.IsNullOrWhiteSpace(body))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "郵件內容不可為空";
                return result;
            }
            #endregion

            try
            {
                // 建立 SmtpClient 物件，設定 SMTP 參數
                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    // 設定不啟用 SSL
                    client.EnableSsl = false;
                    // 設定 SMTP 認證
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    // 設定郵件傳送方式為網路
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    // 建立 MailMessage 物件
                    using (var mail = new MailMessage())
                    {
                        // 設定寄件者
                        mail.From = new MailAddress(smtpUser, "ESIntegrateSys");
                        // 加入所有有效收件人
                        foreach (var to in validRecipients)
                        {
                            mail.To.Add(to);
                        }
                        mail.Subject = subject;
                        mail.Body = body;
                        mail.IsBodyHtml = true;

                        // 發送郵件
                        client.Send(mail);
                    }
                }
                result.IsSuccess = true;
                result.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"郵件發送失敗: {ex.Message}";
            }
            return result;
        }
    }
}
