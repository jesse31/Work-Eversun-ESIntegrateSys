using System.Collections.Generic;
using ESIntegrateSys.ViewModels;
using System.Web.Mvc;

namespace ESIntegrateSys.Services.HomeServices
{
    /// <summary>
    /// 首頁相關服務介面。
    /// </summary>
    public interface IHomeService
    {
        /// <summary>
        /// 取得指定人員的首頁佈局資訊。
        /// </summary>
        /// <param name="member">人員資訊模型。</param>
        /// <returns>首頁功能佈局資訊列表。</returns>
        List<MemberFunctionViewModels> GetLayoutInfo(MemberViewModels member);

        /// <summary>
        /// 執行登入動作，驗證使用者帳號與密碼。
        /// </summary>
        /// <param name="fUserId">使用者帳號。</param>
        /// <param name="fPwd">使用者密碼。</param>
        /// <param name="returnUrl">登入後導向的 URL。</param>
        /// <param name="controller">MVC 控制器實例。</param>
        /// <returns>登入結果的 LoginResult。</returns>
        LoginResult Login(string fUserId, string fPwd, string returnUrl, Controller controller);

        /// <summary>
        /// 執行登出動作，清除使用者登入狀態。
        /// </summary>
        /// <param name="controller">MVC 控制器實例。</param>
        void Logout(Controller controller);
    }
}
