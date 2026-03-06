using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ESIntegrateSys.Models;
using ESIntegrateSys.Services;
using ESIntegrateSys.Services.HomeServices;
using ESIntegrateSys.ViewModels;

namespace ESIntegrateSys.Controllers
{
    /// <summary>
    /// 首頁控制器，負責處理登入、登出及首頁顯示邏輯。
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IHomeService homeService;
        public HomeController(IHomeService homeService)
        {
            this.homeService = homeService;
        }

        /// <summary>
        /// 首頁顯示，根據 Session 會員資訊載入對應的功能畫面。
        /// </summary>
        /// <returns>首頁視圖。</returns>
        public ActionResult Index()
        {
            // 從 Session 取得會員資訊
            var member = Session["Member"] as MemberViewModels;
            if (member == null)
            {
                return View("Index", "_Layout");
            }
            // 如果沒有對應的功能畫面，顯示預設首頁
            var layoutinfo = homeService.GetLayoutInfo(member);
            // 取得會員角色
            Session["Admin"] = member.ROLE_ID;
            if (layoutinfo != null && layoutinfo.Any())
            {
                // 設置第一個頁面資料
                Session["Layout"] = layoutinfo.First();
            }
            return View("Index", "_LayoutIndex", layoutinfo);
        }

        /// <summary>
        /// 顯示登入頁面。
        /// </summary>
        /// <param name="returnUrl">登入後欲導向的網址。</param>
        /// <returns>登入視圖。</returns>
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// 處理登入請求，驗證帳號密碼並設定 Session。
        /// </summary>
        /// <param name="fUserId">使用者帳號。</param>
        /// <param name="fPwd">使用者密碼。</param>
        /// <param name="returnUrl">登入後欲導向的網址。</param>
        /// <returns>登入成功則導向首頁或 returnUrl，失敗則回登入頁。</returns>
        [HttpPost]
        public ActionResult Login(string fUserId, string fPwd, string returnUrl)
        {
            // 呼叫 HomeService.Login，傳入 Controller 以便存取 Session 或 ModelState
            var result = homeService.Login(fUserId, fPwd, returnUrl, this);
            // 假設 result 型別為 LoginResult，包含 Success、Message、Member、RedirectUrl 屬性
            if (result == null || !result.Success)
            {
                ViewBag.Message = result?.Message ?? "登入失敗，請再試一次。";
                return View();
            }
            Session["WelCome"] = $"員工 : {result.Member.fName}";
            Session["Member"] = result.Member;
            if (!string.IsNullOrEmpty(result.RedirectUrl))
            {
                return Redirect(result.RedirectUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// 登出，清除 Session 並導向首頁。
        /// </summary>
        /// <returns>首頁視圖。</returns>
        public ActionResult Logout()
        {
            homeService.Logout(this);
            return RedirectToAction("Index");
        }
    }
}