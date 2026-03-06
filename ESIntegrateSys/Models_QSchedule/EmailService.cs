using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;

namespace ESIntegrateSys.Models_QSchedule
{
    /// <summary>
    /// 提供 SMTP 郵件發送服務的類別。
    /// </summary>
    public class EmailService
    {
        /// <summary>
        /// SMTP 伺服器位址。
        /// </summary>
        private readonly string _smtpServer;

        /// <summary>
        /// SMTP 伺服器連接埠。
        /// </summary>
        private readonly int _smtpPort;

        /// <summary>
        /// SMTP 使用者帳號。
        /// </summary>
        private readonly string _smtpUser;

        /// <summary>
        /// SMTP 使用者密碼。
        /// </summary>
        private readonly string _smtpPass;

        /// <summary>
        /// 建立 <see cref="EmailService"/> 類別的新執行個體。
        /// </summary>
        /// <param name="smtpServer">SMTP 伺服器位址。</param>
        /// <param name="smtpPort">SMTP 伺服器連接埠，預設為 25。</param>
        /// <param name="smtpUser">SMTP 使用者帳號。</param>
        /// <param name="smtpPass">SMTP 使用者密碼。</param>
        public EmailService(string smtpServer, int smtpPort = 25, string smtpUser = null, string smtpPass = null)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpUser = smtpUser;
            _smtpPass = smtpPass;
        }

        /// <summary>
        /// 發送電子郵件給指定收件人。
        /// </summary>
        /// <param name="toEmails">收件人電子郵件地址集合。</param>
        /// <param name="subject">郵件主旨。</param>
        /// <param name="body">郵件內容（HTML 格式）。</param>
        /// <exception cref="InvalidOperationException">發送郵件失敗時拋出。</exception>
        public void SendEmail(IEnumerable<string> toEmails, string subject, string body)
        {
            try // 嘗試執行以下程式碼
            {
                var fromAddress = new MailAddress(_smtpUser, "ESIntegrateSys"); // 建立寄件者郵件地址物件
                using (var smtp = new SmtpClient // 建立 SMTP 用戶端物件
                {
                    Host = _smtpServer, // 設定 SMTP 伺服器位址
                    Port = _smtpPort, // 設定 SMTP 伺服器連接埠
                    EnableSsl = false, // 不啟用 SSL 加密
                    DeliveryMethod = SmtpDeliveryMethod.Network, // 設定郵件傳送方式為網路
                    UseDefaultCredentials = false, // 不使用預設認證
                    Credentials = string.IsNullOrEmpty(_smtpUser) ? null : new NetworkCredential(_smtpUser, _smtpPass) // 設定 SMTP 認證資訊
                })
                {
                    using (var message = new MailMessage() // 建立郵件訊息物件
                    {
                        From = fromAddress, // 設定寄件者
                        Subject = subject, // 設定郵件主旨
                        Body = body, // 設定郵件內容
                        IsBodyHtml = true // 設定郵件內容為 HTML 格式
                    })
                    {
                        foreach (var toEmail in toEmails) // 逐一加入收件者
                        {
                            message.To.Add(new MailAddress(toEmail)); // 新增收件者郵件地址
                        }
                        smtp.Send(message); // 發送郵件
                    }
                }
            }
            catch (Exception ex) // 捕捉例外狀況
            {
                throw new InvalidOperationException("Failed to send email.", ex); // 拋出郵件發送失敗的例外
            }
        }
    }
}