using ESIntegrateSys.Services.Dtos;
using System.Collections.Generic;

namespace ESIntegrateSys.Services.EmailServices
{
    /// <summary>
    /// 定義 Email 相關操作的介面
    /// </summary>
    /// <summary>
    /// Email 服務介面，定義所有郵件相關操作
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// 發送電子郵件
        /// </summary>
        /// <param name="toEmails">收件人電子郵件清單</param>
        /// <param name="subject">郵件主旨</param>
        /// <param name="body">郵件內容 (HTML)</param>
        EmailSendResultDto SendEmail(IEnumerable<string> toEmails, string subject, string body);

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
        IEnumerable<string> GetRecipients(ESIntegrateSys.Models.ESIntegrateSysEntities db, string subjects, string[] bodys);
    }
}
