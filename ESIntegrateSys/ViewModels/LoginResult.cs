using ESIntegrateSys.Models;

namespace ESIntegrateSys.ViewModels
{
    /// <summary>
    /// 登入結果 ViewModel。
    /// </summary>
    public class LoginResult
    {
        /// <summary>
        /// 指示登入是否成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 登入結果訊息。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 登入成功時的會員資訊。
        /// </summary>
        public MemberViewModels Member { get; set; }

        /// <summary>
        /// 登入後導向的網址。
        /// </summary>
        public string RedirectUrl { get; set; }
    }
}
