using System.Configuration;
using System.Net;
using System.Web.Mvc;

namespace ESIntegrateSys.Filters
{
    /// <summary>
    /// 電子報價唯讀守衛：新系統上線後，舊系統的電子報價只能查詢，不能異動資料。
    /// Web.config appSettings 的 QsReadOnly 設為 true 時才生效；未設定視同 false，行為與原本完全相同。
    /// 依入口的回應方式分別擋下，讓原本的前端處理能正確顯示訊息：
    /// 回傳 JsonResult 的 action → JSON { status = "error", message }；其他 Ajax 請求 → 403；一般頁面 → 帶訊息導回清單。
    /// </summary>
    public class QsReadOnlyAttribute : ActionFilterAttribute
    {
        /// <summary>擋下時顯示的訊息</summary>
        public const string Message = "電子報價已移至新系統，舊系統僅供查詢";

        /// <summary>TempData 鍵值，供清單頁顯示被擋下的訊息</summary>
        public const string TempDataKey = "QsReadOnlyMessage";

        /// <summary>是否已切換為唯讀（Web.config appSettings 的 QsReadOnly）</summary>
        public static bool IsEnabled
        {
            get
            {
                bool enabled;
                return bool.TryParse(ConfigurationManager.AppSettings["QsReadOnly"], out enabled) && enabled;
            }
        }

        /// <inheritdoc/>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!IsEnabled) return;

            var returnsJson = (filterContext.ActionDescriptor as ReflectedActionDescriptor)?.MethodInfo.ReturnType == typeof(JsonResult);
            if (returnsJson)
            {
                filterContext.Result = new JsonResult { Data = new { status = "error", message = Message } };
            }
            else if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden, Message);
            }
            else
            {
                filterContext.Controller.TempData[TempDataKey] = Message;
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary { { "controller", "QuoteSchedule" }, { "action", "QuotesView" } });
            }
        }
    }
}
