using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ESIntegrateSys.Services.ManpowerAllocationServices;
using ESIntegrateSys.ViewModels;

namespace ESIntegrateSys.Filters
{
    /// <summary>
    /// 部門授權過濾器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class DepartmentAuthorizationAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// 允許的部門代號
        /// </summary>
        private readonly string[] _allowedDepartments;

        /// <summary>
        /// 允許的使用者ID
        /// </summary>
        private readonly string[] _allowedUserIds;

        /// <summary>
        /// 建構函式 - 指定允許的部門代號
        /// </summary>
        /// <param name="departments">允許的部門代號列表</param>
        public DepartmentAuthorizationAttribute(params string[] departments)
        {
            _allowedDepartments = departments;
            _allowedUserIds = new string[0];
        }

        /// <summary>
        /// 建構函式 - 指定允許的使用者ID和部門代號
        /// [DepartmentAuthorization(new string[] { "user001", "user002" }, new string[0])] - 指定允許的使用者ID
        /// </summary>
        /// <param name="userIds">允許的使用者ID列表</param>
        /// <param name="departments">允許的部門代號列表</param>
        public DepartmentAuthorizationAttribute(string[] userIds, string[] departments)
        {
            _allowedUserIds = userIds ?? new string[0];
            _allowedDepartments = departments ?? new string[0];
        }

        /// <summary>
        /// 授權核心方法
        /// </summary>
        /// <param name="httpContext">HTTP上下文</param>
        /// <returns>是否授權</returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // 輸出診斷信息
            System.Diagnostics.Debug.WriteLine($"AuthorizeCore 被調用，檢查授權");

            // 直接檢查 Session 中是否有 Member 對象，不依賴於基類的授權檢查
            if (httpContext.Session == null || httpContext.Session["Member"] == null)
            {
                System.Diagnostics.Debug.WriteLine($"Session 為空或 Member 不存在，授權失敗");
                return false;
            }

            // 如果沒有指定部門或使用者ID，則允許所有已登入的使用者
            if ((_allowedDepartments == null || _allowedDepartments.Length == 0) &&
                (_allowedUserIds == null || _allowedUserIds.Length == 0))
            {
                return true;
            }

            // 從 Session 獲取使用者資訊
            System.Diagnostics.Debug.WriteLine($"Session 存在: {httpContext.Session != null}, Session[\"Member\"] 存在: {httpContext.Session?["Member"] != null}");

            var member = httpContext.Session?["Member"] as MemberViewModels;
            if (member == null)
            {
                System.Diagnostics.Debug.WriteLine("Member 對象為空或不是 MemberViewModels 類型");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"Member 對象: UserId={member.fUserId}, UDeptNo={member.UDeptNo ?? "(null)"}");

            // 檢查使用者ID是否在允許列表中
            if (_allowedUserIds != null && _allowedUserIds.Length > 0)
            {
                if (_allowedUserIds.Contains(member.fUserId))
                {
                    return true;
                }
            }

            var service = DependencyResolver.Current.GetService<IManpowerAllocationServices>();
            if (service != null)
            {
                // 取得授權工號
                var authorizedUserIds = service.Get授權者UserIds();
                if (authorizedUserIds.Contains(member.fUserId))
                {
                    return true;
                }
            }

            // 檢查部門是否在允許列表中
            if (_allowedDepartments != null && _allowedDepartments.Length > 0)
            {
                // 輸出診斷信息
                System.Diagnostics.Debug.WriteLine($"用戶部門: {member.UDeptNo}，允許的部門: {string.Join(", ", _allowedDepartments)}");

                // 不區分大小寫比較部門代碼
                if (_allowedDepartments.Any(d => string.Equals(d, member.UDeptNo, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            // 如果都不符合，則拒絕存取
            return false;
        }

        /// <summary>
        /// 處理未授權的請求
        /// </summary>
        /// <param name="filterContext">授權上下文</param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                // 對於AJAX請求，返回JSON結果
                filterContext.Result = new JsonResult
                {
                    Data = new { success = false, message = "您沒有權限執行此操作。", requireLogin = false, unauthorized = true },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                filterContext.HttpContext.Response.StatusCode = 403; // Forbidden
            }
            else
            {
                // 對於一般請求，重定向到未授權頁面
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Unauthorized.cshtml"
                };
            }
        }
    }
}
