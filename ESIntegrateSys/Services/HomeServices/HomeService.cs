using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ESIntegrateSys.Models;
using ESIntegrateSys.ViewModels;

namespace ESIntegrateSys.Services.HomeServices
{
    /// <summary>
    /// 首頁相關服務實作。
    /// </summary>
    public class HomeService : IHomeService
    {
        /// <summary>
        /// 資料庫存取物件。
        /// </summary>
        private readonly ESIntegrateSysEntities db;

        /// <summary>
        /// 建構 HomeService 實例，初始化資料庫物件。
        /// </summary>
        public HomeService()
        {
            db = new ESIntegrateSysEntities();
        }

        /// <summary>
        /// 取得指定會員的首頁佈局資訊。
        /// </summary>
        /// <param name="member">會員資訊 ViewModel。</param>
        /// <returns>會員功能佈局資訊清單，若 member 為 null 則回傳 null。</returns>
        public List<MemberFunctionViewModels> GetLayoutInfo(MemberViewModels member)
        {
            if (member == null) return null;
            // 取得員工工號
            string uId = member.fUserId;
            var layoutinfo = (from a in db.ES_Member
                              join b in db.ES_MemberFunction on a.fId equals b.UserNo_sno into mFunction
                              from b in mFunction.DefaultIfEmpty()
                              join c in db.ES_FunctionItem on b.FunctionNo equals c.FunctionNo into iFunction
                              from c in iFunction.DefaultIfEmpty()
                              join d in db.ES_MemberLayout on c.ActionName equals d.LayoutViewName into lViewName
                              from d in lViewName.DefaultIfEmpty()
                              where a.fUserId == uId // 員工工號
                              select new MemberFunctionViewModels
                              {
                                  LayoutViewText = c.FunctionName,
                                  LayoutIndex = d.LayoutIndex,
                                  LayoutViewName = c.ActionName
                              }).Distinct().ToList();
            // 如果沒有對應的功能畫面，顯示預設首頁
            return layoutinfo;
        }

        /// <summary>
        /// 執行會員登入，並設定 Session 與導向頁面。
        /// </summary>
        /// <param name="fUserId">使用者帳號。</param>
        /// <param name="fPwd">使用者密碼。</param>
        /// <param name="returnUrl">登入後導向的 URL。</param>
        /// <param name="controller">MVC Controller 實例。</param>
        /// <returns>登入結果的 LoginResult。</returns>
        public LoginResult Login(string fUserId, string fPwd, string returnUrl, Controller controller)
        {
            var result = new LoginResult();
            try
            {
                var member = (from a in db.ES_Member
                              join b in db.ES_MemberRole on a.fUserId equals b.USER_ID into roleJoin
                              from c in roleJoin.DefaultIfEmpty()
                              where a.fUserId == fUserId && a.fPwd == fPwd && a.fStatus == true
                              orderby a.fUserId
                              select new MemberViewModels
                              {
                                  fUserId = a.fUserId,
                                  fName = a.fName,
                                  fId = a.fId,
                                  ROLE_ID = c != null ? c.ROLE_ID : null,
                                  UDeptNo = a.Dept_No
                              }).FirstOrDefault();

                if (member == null)
                {
                    result.Success = false;
                    result.Message = "帳號密碼錯誤，請重新登入";
                    result.Member = null;
                    result.RedirectUrl = null;
                    return result;
                }
                controller.Session["WelCome"] = "員工 : " + member.fName;
                controller.Session["Member"] = member;
                result.Success = true;
                result.Message = "登入成功";
                result.Member = member;
                result.RedirectUrl = string.IsNullOrEmpty(returnUrl) ? null : returnUrl;
                return result;
            }
            catch (Exception)
            {
                result.Success = false;
                result.Message = "登入過程發生錯誤，請稍後再試。";
                result.Member = null;
                result.RedirectUrl = null;
                return result;
            }
        }

        /// <summary>
        /// 執行會員登出，清除 Session。
        /// </summary>
        /// <param name="controller">MVC Controller 實例。</param>
        public void Logout(Controller controller)
        {
            controller.Session.Abandon();
        }
    }
}
