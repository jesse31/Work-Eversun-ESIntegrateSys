using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ESIntegrateSys.Models_ManpowerAllocation;
using ESIntegrateSys.Services.ManpowerAllocationServices;
using ESIntegrateSys.ViewModels;
using System.Threading.Tasks;
using ESIntegrateSys.Utilities;
using ESIntegrateSys.Filters;
using ClosedXML.Excel;
using System.IO;

namespace ESIntegrateSys.Controllers
{
    /// <summary>
    /// 人力配置控制器
    /// </summary>   
    [DepartmentAuthorization(new string[] { "IT", "IE" })] // 限制只有IT部門、IE部門可以存取    
    public class ManpowerAllocationController : Controller
    {
        /// <summary>
        /// 人力配置服務介面
        /// </summary>
        private readonly IManpowerAllocationServices _manpowerAllocationServices;


        // 使用共用方法獲取用戶資訊
        // 取得目前用戶ID
        protected string CurrentUserId => GetCurrentUserId();
        // 取得目前用戶部門
        protected string CurrentUserDeptNo => GetCurrentUserDepartment();


        /// <summary>
        /// 建構函式 - 用於依賴注入
        /// </summary>
        /// <param name="manpowerAllocationServices">人力配置服務</param>        
        public ManpowerAllocationController(IManpowerAllocationServices manpowerAllocationServices)
        {
            _manpowerAllocationServices = manpowerAllocationServices;
        }

        #region 輔助方法

        /// <summary>
        /// 獲取當前用戶ID
        /// </summary>
        /// <returns>用戶ID</returns>
        /// <exception cref="UnauthorizedAccessException">用戶未登入時拋出</exception>
        private string GetCurrentUserId()
        {
            var member = Session["Member"] as MemberViewModels;
            if (member == null || string.IsNullOrEmpty(member.fUserId))
            {
                // 如果在正式環境中，應該拋出異常或重定向到登入頁面
                // 在開發環境中，可以使用 Environment.UserName 作為備用
#if DEBUG
                return Environment.UserName;
#else
                throw new UnauthorizedAccessException("用戶未登入或登入已過期");
#endif
            }
            return member.fUserId;
        }

        /// <summary>
        /// 獲取當前用戶部門
        /// </summary>
        /// <returns>用戶部門</returns>
        /// <exception cref="UnauthorizedAccessException">用戶未登入時拋出</exception>
        private string GetCurrentUserDepartment()
        {
            var member = Session["Member"] as MemberViewModels;
            if (member == null)
            {
                // 如果在正式環境中，應該拋出異常或重定向到登入頁面
#if DEBUG
                return "";
#else
                throw new UnauthorizedAccessException("用戶未登入或登入已過期");
#endif
            }
            return member.UDeptNo ?? "";
        }

        /// <summary>
        /// 檢查記錄鎖定狀態
        /// </summary>
        /// <param name="detail">人員明細資料</param>
        /// <param name="userId">當前用戶ID</param>
        /// <returns>鎖定狀態結果，如果返回 null 表示沒有鎖定問題</returns>
        private async Task<JsonResult> CheckRecordLockStatus(人力配置明細 detail, string userId)
        {
            // 檢查編輯鎖定
            if (!string.IsNullOrEmpty(detail.str目前編輯者) && detail.str目前編輯者 != userId)
            {
                // 檢查鎖定時間
                TimeSpan? lockDuration = null;
                if (detail.dt編輯鎖定時間.HasValue)
                {
                    lockDuration = DateTime.Now - detail.dt編輯鎖定時間.Value;
                }

                // 如果鎖定時間小於30分鐘
                if (lockDuration == null || lockDuration.Value.TotalMinutes < 30)
                {
                    return Json(new { success = false, message = $"資料正在被 {detail.str目前編輯者} 編輯中，無法更新" });
                }
            }

            return null; // 沒有鎖定問題
        }

        /// <summary>
        /// 處理並記錄錯誤
        /// </summary>
        /// <param name="ex">異常</param>
        /// <param name="operation">操作名稱</param>
        /// <param name="additionalInfo">額外信息</param>
        /// <returns>錯誤消息</returns>
        private async Task<string> HandleAndLogError(Exception ex, string operation, string additionalInfo = "")
        {
            string errorMessage = $"{operation}時發生錯誤";
            await LogHelper.LogErrorAsync(ex, $"{errorMessage} - {additionalInfo}").ConfigureAwait(false);
            return errorMessage;
        }

        /// <summary>
        /// 處理並記錄錯誤，返回 JSON 結果
        /// </summary>
        /// <param name="ex">異常</param>
        /// <param name="operation">操作名稱</param>
        /// <param name="additionalInfo">額外信息</param>
        /// <returns>JSON 結果</returns>
        private async Task<JsonResult> HandleAndLogErrorJson(Exception ex, string operation, string additionalInfo = "")
        {
            string errorMessage = await HandleAndLogError(ex, operation, additionalInfo).ConfigureAwait(false);
            return Json(new { success = false, message = errorMessage }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 獲取記錄鎖定狀態
        /// </summary>
        /// <param name="id">記錄ID</param>
        /// <returns>鎖定狀態信息</returns>
        [HttpGet]
        public async Task<JsonResult> GetRecordLockStatus(string id)
        {
            try
            {
                // 獲取記錄資料
                var record = await _manpowerAllocationServices.GetEmployeeDetailById(id)
                    .ConfigureAwait(false);

                if (record == null)
                {
                    return Json(new { success = false, message = "找不到記錄" }, JsonRequestBehavior.AllowGet);
                }

                // 檢查是否被鎖定
                bool isLocked = false;
                string editorName = "";
                DateTime? lockTime = null;

                if (!string.IsNullOrEmpty(record.str目前編輯者) && record.dt編輯鎖定時間.HasValue)
                {
                    // 檢查鎖定是否過期（30分鐘過期）
                    if (DateTime.Now.Subtract(record.dt編輯鎖定時間.Value).TotalMinutes < 30)
                    {
                        isLocked = true;
                        editorName = record.str目前編輯者;
                        lockTime = record.dt編輯鎖定時間;
                    }
                }

                return Json(new
                {
                    success = true,
                    isLocked = isLocked,
                    editorName = editorName,
                    lockTime = lockTime,
                    lastModified = record.dt最後修改時間,
                    timestamp = record.str時間戳
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return await HandleAndLogErrorJson(ex, "獲取鎖定狀態", $"ID: {id}").ConfigureAwait(false);
            }
        }
        #endregion

        /// <summary>
        /// 人力配置首頁
        /// </summary>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁顯示筆數</param>
        /// <returns>人力配置視圖</returns>
        public async Task<ActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                // 驗證分頁參數
                if (page < 1)
                {
                    page = 1; // 確保頁碼至少為 1
                }

                if (pageSize < 1)
                {
                    pageSize = 10; // 確保每頁顯示筆數至少為 1
                }
                else if (pageSize > 100)
                {
                    pageSize = 100; // 限制每頁顯示筆數上限，避免性能問題
                }

                // 獲取生產單位列表
                List<生產單位下拉選單Dto> productionUnits = await _manpowerAllocationServices.Get生產單位下拉選單Async(CurrentUserId, CurrentUserDeptNo);
                ViewBag.ProductionUnits = productionUnits;

                // 獲取班別列表
                List<班別下拉選單首頁Dto> shifts = await _manpowerAllocationServices.Get查詢班別下拉選單Async();
                ViewBag.Shifts = shifts;

                // 獲取出勤類型列表
                List<出勤類型下拉選單Dto> attendanceTypes = await _manpowerAllocationServices.Get出勤類型下拉選單Async();
                ViewBag.AttendanceTypes = attendanceTypes;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"訪問人力配置頁面時發生錯誤:{ex.Message}");

                return RedirectToAction("Index", "Home");
            }
        }

        #region 人力配置統計資料
        /// <summary>
        /// 分頁查詢人力配置統計資料
        /// </summary>
        /// <param name="request">查詢條件</param>            
        /// <returns>分頁查詢結果（Json）</returns>
        [HttpPost]
        public async Task<JsonResult> GetManpowerAllocationList(查詢人力配置Request request)
        {

            try
            {
                // 驗證分頁參數
                if (request.page < 1)
                {
                    request.page = 1; // 確保頁碼至少為 1
                }

                if (request.pageSize < 1)
                {
                    request.pageSize = 10; // 確保每頁顯示筆數至少為 1
                }
                else if (request.pageSize > 100)
                {
                    request.pageSize = 100; // 限制每頁顯示筆數上限，避免性能問題
                }

                // 查詢資料
                var allData = await _manpowerAllocationServices.GetManpowerAllocationStatistics(request.查詢條件);

                if (allData == null || !allData.Any())
                {
                    // 回傳一個空的結果或是做其他處理
                    return Json(new
                    {
                        success = true,
                        TotalCount = 0,
                        Page = request.page,
                        PageSize = request.pageSize
                    }, JsonRequestBehavior.AllowGet);
                }

                // 分頁
                var pagedData = allData
                    .OrderBy(x => x.str生產單位)
                    .ThenBy(x => x.str班別名稱)
                    .Skip((request.page - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .ToList() ?? new List<人力配置統計>();

                // 總筆數
                int totalCount = allData.Count();

                return Json(new
                {
                    success = true,
                    total = totalCount,
                    data = pagedData
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "查詢資料失敗：" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 獲取人員明細資料
        /// <summary>
        /// 獲取人員明細資料
        /// </summary>
        /// <param name="productionUnit">生產單位</param>
        /// <param name="shift">班別</param>
        /// <param name="attendanceType">出勤類型</param>
        /// <returns>人員明細資料的部分視圖</returns>
        [HttpGet]
        public async Task<ActionResult> EmployeeDetails(string productionUnit, string shift, string attendanceType, string str作業人員工號, string str作業人員姓名)
        {

            try
            {
                var request = new 人力配置明細
                {
                    str生產單位名稱 = productionUnit,
                    str班別名稱 = shift,
                    str出勤類型 = attendanceType,
                    str作業人員工號 = str作業人員工號,
                    str作業人員姓名 = str作業人員姓名,
                    str修改者 = CurrentUserId,
                    str部門 = CurrentUserDeptNo
                };

                // 獲取人員明細資料
                var employeeDetails = await _manpowerAllocationServices.GetEmployeeDetails(request).ConfigureAwait(false);

                // 將查詢條件傳送給視圖
                ViewBag.ProductionUnit = productionUnit;
                ViewBag.Shift = shift;
                ViewBag.AttendanceType = attendanceType;
                //ViewBag.EmployeeId = str作業人員工號;
                //ViewBag.EmployeeName = str作業人員姓名;


                return View("EmployeeDetails", employeeDetails);

            }
            catch (Exception ex)
            {
                // 使用統一的錯誤處理方法
                string errorMessage = await HandleAndLogError(ex, "獲取人員明細資料").ConfigureAwait(false);
                Response.StatusCode = 500; // 設定 HTTP 狀態碼
                return Content($"<div class='alert alert-danger'>{errorMessage}</div>"); // 返回帶有錯誤訊息的 HTML
            }
        }
        #endregion

        #region 編輯人力配置明細

        /// <summary>
        /// 顯示編輯人員明細頁面
        /// </summary>
        /// <param name="id">人員明細ID</param>
        /// <returns>編輯人員明細視圖</returns>
        [HttpGet]
        public async Task<ActionResult> UpdateEmployeeDetail(string id)
        {
            try
            {
                // 獲取當前用戶資訊
                string userId = GetCurrentUserId();

                // 獲取人員明細資料
                var detail = await _manpowerAllocationServices.GetEmployeeDetailById(id).ConfigureAwait(false);

                if (detail == null)
                {
                    // 找不到資料，返回錯誤訊息
                    TempData["ErrorMessage"] = "找不到指定的人員明細資料";
                    return RedirectToAction("Index");
                }

                // 檢查資料是否正在被編輯
                if (!string.IsNullOrEmpty(detail.str目前編輯者) && detail.str目前編輯者 != CurrentUserId)
                {
                    // 計算鎖定時間
                    TimeSpan? lockDuration = null;
                    if (detail.dt編輯鎖定時間.HasValue)
                    {
                        lockDuration = DateTime.Now - detail.dt編輯鎖定時間.Value;
                    }

                    // 如果鎖定時間超過30分鐘，則釋放鎖定
                    if (lockDuration == null || lockDuration.Value.TotalMinutes < 30)
                    {
                        //TempData["ErrorMessage"] = $"資料正在被 {detail.str目前編輯者} 編輯中，請稍後再試";
                        //return RedirectToAction("Index");
                        ViewBag.LockedUser = detail.str目前編輯者;
                        return View("Locked");
                    }
                }

                // 設置編輯鎖定
                detail.str目前編輯者 = CurrentUserId;
                detail.dt編輯鎖定時間 = DateTime.Now;

                // 更新資料庫中的鎖定狀態
                await _manpowerAllocationServices.UpdateEmployeeDetailLockStatus(id, CurrentUserId, DateTime.Now);

                // 將資料傳送給視圖
                return View("UpdateEmployeeDetail", "_ManpowerAllocationLayout", detail);
            }
            catch (Exception ex)
            {
                // 使用統一的錯誤處理方法
                string errorMessage = await HandleAndLogError(ex, "獲取人員明細資料").ConfigureAwait(false);
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region 刪除人力配置明細

        /// <summary>
        /// 刪除人員明細資料
        /// </summary>
        /// <param name="id">人員明細資料ID</param>
        /// <param name="timestamp">時間戳，用於並發控制</param>
        /// <returns>刪除結果（Json）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteEmployeeDetail(string id, string timestamp = null)
        {
            try
            {
                // 檢查ID是否有效
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "無效的ID" });
                }

                // 檢查資料是否正在被編輯
                var currentDetail = await _manpowerAllocationServices.GetEmployeeDetailById(id).ConfigureAwait(false);

                if (currentDetail == null)
                {
                    return Json(new { success = false, message = "找不到指定的人員明細資料" });
                }

                // 檢查是否正在被編輯
                if (!string.IsNullOrEmpty(currentDetail.str目前編輯者))
                {
                    // 檢查鎖定時間
                    TimeSpan? lockDuration = null;
                    if (currentDetail.dt編輯鎖定時間.HasValue)
                    {
                        lockDuration = DateTime.Now - currentDetail.dt編輯鎖定時間.Value;
                    }

                    // 如果鎖定時間小於30分鐘，且不是自己在編輯
                    if ((lockDuration == null || lockDuration.Value.TotalMinutes < 30) && currentDetail.str目前編輯者 != CurrentUserId)
                    {
                        return Json(new { success = false, message = $"資料正在被 {currentDetail.str目前編輯者} 編輯中，無法刪除" });
                    }
                }

                // 檢查時間戳是否匹配
                if (!string.IsNullOrEmpty(timestamp) && currentDetail.dt最後修改時間.HasValue)
                {
                    DateTime clientTimestamp;
                    if (DateTime.TryParse(timestamp, out clientTimestamp))
                    {
                        // 如果資料庫中的時間戳比客戶端的更新，表示資料已被修改
                        if (currentDetail.dt最後修改時間.Value > clientTimestamp)
                        {
                            return Json(new { success = false, message = "資料已被修改，請重新整理後再刪除", needRefresh = true });
                        }
                    }
                }

                // 執行刪除操作
                bool result = await _manpowerAllocationServices.DeleteEmployeeDetail(id, CurrentUserId, CurrentUserDeptNo);

                if (result)
                {
                    // 刪除成功後釋放鎖定
                    await _manpowerAllocationServices.UpdateEmployeeDetailLockStatus(id, null, null);
                    return Json(new { success = true, message = "人員明細資料已成功刪除" });
                }
                else
                {
                    return Json(new { success = false, message = "刪除人員明細資料失敗" });
                }
            }
            catch (Exception ex)
            {
                return await HandleAndLogErrorJson(ex, "刪除人員明細", $"ID: {id}").ConfigureAwait(false);
            }
        }
        #endregion

        #region 更新人員明細
        /// <summary>
        /// 更新人員明細資料
        /// </summary>
        /// <param name="model">人員明細資料</param>
        /// <param name="TimestampString">時間戳字符串</param>
        /// <returns>更新結果（Json）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateEmployeeDetail(人力配置明細異動Dto model, string TimestampString = null)
        {
            try
            {
                if (!ModelState.IsValid) // 檢查模型驗證
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = "資料驗證失敗：" + string.Join("; ", errors) });
                }

                // 檢查模型是否有效
                if (string.IsNullOrEmpty(model.strPK))
                {
                    return Json(new { success = false, message = "缺少人員明細ID" });
                }

                // 檢查資料是否已被他人修改
                var currentDetail = await _manpowerAllocationServices.GetEmployeeDetailById(model.strPK).ConfigureAwait(false);

                if (currentDetail == null)
                {
                    return Json(new { success = false, message = "找不到指定的人員明細資料" });
                }

                // 檢查編輯鎖定
                if (!string.IsNullOrEmpty(currentDetail.str目前編輯者) && currentDetail.str目前編輯者 != CurrentUserId)
                {
                    // 檢查鎖定時間
                    TimeSpan? lockDuration = null;
                    if (currentDetail.dt編輯鎖定時間.HasValue)
                    {
                        lockDuration = DateTime.Now - currentDetail.dt編輯鎖定時間.Value;
                    }

                    // 如果鎖定時間小於30分鐘
                    if (lockDuration == null || lockDuration.Value.TotalMinutes < 30)
                    {
                        return Json(new { success = false, message = $"資料正在被 {currentDetail.str目前編輯者} 編輯中，無法更新" });
                    }
                }

                // 檢查時間戳是否匹配
                if (!string.IsNullOrEmpty(TimestampString) && currentDetail.dt最後修改時間.HasValue)
                {
                    // 計算資料庫中記錄的時間戳
                    string dbTimestamp = currentDetail.TimestampString;

                    // 如果前端提供的時間戳與資料庫中的不同，表示資料已被修改
                    if (dbTimestamp != TimestampString)
                    {
                        return Json(new { success = false, message = "資料已被他人修改，請重新整理後再存檔", needRefresh = true });
                    }
                }

                // 設定修改者和時間戳
                model.str修改者 = CurrentUserId;
                model.str部門 = CurrentUserDeptNo;
                model.dt最後修改時間 = DateTime.Now;
                model.str目前編輯者 = null; // 釋放編輯鎖定
                model.dt編輯鎖定時間 = null;

                bool result = await _manpowerAllocationServices.UpdateEmployeeDetail(model).ConfigureAwait(false);

                if (result)
                {
                    return Json(new { success = true, message = "人員明細資料已成功更新" });
                }
                else
                {
                    return Json(new { success = false, message = "更新人員明細資料失敗" });
                }
            }
            catch (Exception ex)
            {
                return await HandleAndLogErrorJson(ex, "更新人員明細", $"ID: {model.strPK}").ConfigureAwait(false);
            }
        }
        #endregion

        #region 新增人員明細
        /// <summary>
        /// 新增人員明細頁面
        /// </summary>
        /// <returns></returns>
        public ActionResult AddEmployeeDetail()
        {

            return View();
        }

        /// <summary>
        /// 新增人員明細資料
        /// </summary>
        /// <param name="model">人員明細資料</param>
        /// <returns>新增結果（Json）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddEmployeeDetail(人力配置明細異動Dto model)
        {

            if (!ModelState.IsValid) // 檢查模型驗證
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "資料驗證失敗：" + string.Join("; ", errors) });
            }

            try
            {
                // 設定建立者           
                model.str建立者 = CurrentUserId;
                model.str部門 = CurrentUserDeptNo;

                // 新增人員明細資料
                bool result = await _manpowerAllocationServices.AddEmployeeDetail(model);

                if (result)
                {
                    return Json(new { success = true, message = "人員明細資料已成功新增" });
                }
                else
                {
                    return Json(new { success = false, message = "新增人員明細資料失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "新增失敗：" + ex.Message });
            }
        }

        #endregion

        #region 刷新鎖定狀態
        /// <summary>
        /// 刷新人員明細的鎖定狀態
        /// </summary>
        /// <param name="id">人員明細ID</param>
        /// <returns>操作結果（Json）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RefreshLockStatus(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "缺少人員明細ID" });
                }

                // 檢查記錄是否存在
                var detail = await _manpowerAllocationServices.GetEmployeeDetailById(id).ConfigureAwait(false);
                if (detail == null)
                {
                    return Json(new { success = false, message = "找不到指定的人員明細資料" });
                }

                // 檢查是否為當前編輯者
                if (string.IsNullOrEmpty(detail.str目前編輯者) || detail.str目前編輯者 != CurrentUserId)
                {
                    return Json(new { success = false, message = "您不是此記錄的當前編輯者，無法刷新鎖定狀態" });
                }

                // 刷新鎖定狀態
                bool result = await _manpowerAllocationServices.UpdateEmployeeDetailLockStatus(id, CurrentUserId, DateTime.Now).ConfigureAwait(false);

                if (result)
                {
                    return Json(new { success = true, message = "鎖定狀態已刷新" });
                }
                else
                {
                    return Json(new { success = false, message = "刷新鎖定狀態失敗" });
                }
            }
            catch (Exception ex)
            {
                return await HandleAndLogErrorJson(ex, "刷新鎖定狀態", $"ID: {id}").ConfigureAwait(false);
            }
        }
        #endregion

        #region 匯出人力配置清單Excel
        /// <summary>
        /// 匯出人力配置清單Excel
        /// </summary>
        /// <param name="exportData">匯出條件</param>
        /// <returns>Excel檔案</returns>
        [HttpPost]
        public async Task<ActionResult> ExportExcelMAList(人力配置明細 request)
        {
            try
            {

                // 查詢資料
                var data = await _manpowerAllocationServices.GetEmployeeDetails(request);

                if (data == null || !data.Any())
                {
                    return Json(new { success = false, message = "沒有可匯出的資料。" }); // 返回提示訊息
                }

                // 使用 ClosedXML 創建 Excel
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("人力配置清單");

                    // 設定標題樣式
                    var titleStyle = worksheet.Style;
                    titleStyle.Font.Bold = true;
                    titleStyle.Fill.BackgroundColor = XLColor.LightGray;
                    titleStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // 設定標題
                    string[] headers = new string[] {
                        "報工生產單位名稱", "生產單位名稱", "班別",
                        "出勤類型", "作業人員工號", "作業人員姓名", "備註"
                    };

                    // 設定欄寬
                    int[] columnWidths = new int[] { 25, 15, 10, 10, 15, 15, 30 };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                        worksheet.Column(i + 1).Width = columnWidths[i];
                    }

                    // 設定標題樣式
                    var headerRange = worksheet.Range(1, 1, 1, 7);
                    headerRange.Style = titleStyle;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#26C6DA");
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    // 填入資料
                    var row = 2;
                    if (data != null && data.Count() > 0)
                    {
                        foreach (var item in data)
                        {
                            worksheet.Cell(row, 1).Value = item.str報工生產單位名稱;
                            worksheet.Cell(row, 2).Value = item.str生產單位名稱;
                            worksheet.Cell(row, 3).Value = item.str班別名稱;
                            worksheet.Cell(row, 4).Value = item.str出勤類型;
                            // 將工號設定為文字格式，避免開頭的 0 被省略
                            // 使用單引號前綴強制 Excel 將數值視為文字
                            var cell = worksheet.Cell(row, 5);
                            // 若工號不為空，則加上單引號前綴
                            if (!string.IsNullOrEmpty(item.str作業人員工號))
                            {
                                cell.Value = "'" + item.str作業人員工號;
                            }
                            else
                            {
                                cell.Value = item.str作業人員工號;
                            }
                            cell.DataType = XLDataType.Text;
                            // 設定單元格格式為文字
                            cell.Style.NumberFormat.Format = "@";

                            worksheet.Cell(row, 6).Value = item.str作業人員姓名;
                            worksheet.Cell(row, 7).Value = item.str備註;
                            worksheet.Cell(row, 7).Style.Alignment.WrapText = true;
                            worksheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            // 設定資料列樣式
                            var dataRange = worksheet.Range(row, 1, row, 7);
                            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                            row++;
                        }
                    }

                    // 自動調整欄寬
                    worksheet.Columns().AdjustToContents();

                    // 將工作表轉為二進位資料
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        // 設定回傳的檔案名稱和類型
                        string fileName = $"人力配置清單_{DateTime.Now:yyyyMMdd}.xlsx";

                        var cd = new System.Net.Mime.ContentDisposition
                        {
                            // 使用 UTF-8 編碼檔名，避免中文亂碼
                            FileName = Uri.EscapeDataString(fileName),
                            Inline = false // 強制下載
                        };

                        Response.Headers.Add("Content-Disposition", cd.ToString());

                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                    }
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.StatusDescription = "匯出 Excel 失敗：" + ex.Message;
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }
        #endregion

        #region 下拉

        /// <summary>
        /// 生產單位下拉選單
        /// </summary>
        /// <returns>生產單位</returns>
        [HttpGet]
        public async Task<JsonResult> GetProductionUnits()
        {

            List<生產單位下拉選單Dto> productionUnits = await _manpowerAllocationServices.Get生產單位下拉選單Async(CurrentUserId, CurrentUserDeptNo);

            return Json(productionUnits, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 報工生產單位下拉選單
        /// </summary>
        /// <returns>生產單位</returns>
        [HttpGet]
        public async Task<JsonResult> Get報工生產單位List()
        {

            // 根據用戶ID和部門編號過濾報工生產單位
            List<報工生產單位下拉選單Dto> 報工生產單位List = await _manpowerAllocationServices.Get報工生產單位List(CurrentUserId, CurrentUserDeptNo)
                .ConfigureAwait(false);

            return Json(報工生產單位List, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 班別下拉選單
        /// </summary>
        /// <returns>班別</returns>
        [HttpGet]
        public async Task<JsonResult> GetShifts()
        {
            List<班別下拉選單Dto> shifts = await _manpowerAllocationServices.Get班別下拉選單By新增修改Async(CurrentUserId, CurrentUserDeptNo);

            return Json(shifts, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 出勤類型下拉選單
        /// </summary>
        /// <returns>出勤類型</returns>
        [HttpGet]
        public async Task<JsonResult> GetAttendanceTypes()
        {

            List<出勤類型下拉選單Dto> attendanceTypes = await _manpowerAllocationServices.Get出勤類型下拉選單Async();

            return Json(attendanceTypes, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 人力配置授權設定
        /// <summary>
        /// 人力配置授權設定頁面
        /// </summary>
        public ActionResult ManpowerAuthorization()
        {
            return View("ManpowerAuthorization");
        }
        #endregion

        #region 生產單位管理
        /// <summary>
        /// 生產單位管理頁面
        /// </summary>
        /// <returns>生產單位管理視圖</returns>
        public ActionResult ProductionUnit()
        {
            try
            {
                return View("ManpowerAuthorization/_ProductionUnit");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"訪問生產單位管理頁面時發生錯誤:{ex.Message}");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// 獲取所有生產單位（用於下拉選單）
        /// </summary>
        /// <returns>生產單位列表</returns>
        [HttpGet]
        public async Task<JsonResult> GetAllProductionUnits()
        {
            try
            {
                var productionUnits = await _manpowerAllocationServices.Get生產單位下拉選單Async(CurrentUserId, CurrentUserDeptNo);
                var result = productionUnits.Select(unit => new { UnitName = unit }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"獲取生產單位列表時發生錯誤: {ex.Message}");
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 分頁獲取生產單位列表
        /// </summary>
        /// <param name="unitName">生產單位名稱（可選）</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>分頁後的生產單位列表</returns>
        [HttpGet]
        public async Task<JsonResult> GetProductionUnitsWithPaging(string unitName = "", int page = 1, int pageSize = 10)
        {
            try
            {
                // 獲取所有生產單位
                var allUnits = await _manpowerAllocationServices.Get生產單位下拉選單Async(CurrentUserId, CurrentUserDeptNo);

                // 根據查詢條件過濾（修正：針對 DTO 屬性做 Contains 比對）
                if (!string.IsNullOrEmpty(unitName))
                {
                    // 請將 UnitName 替換為實際 DTO 屬性名稱
                    allUnits = allUnits.Where(u => u.Name != null && u.Name.Contains(unitName)).ToList();
                }

                // 計算總記錄數和總頁數
                int totalRecords = allUnits.Count;
                int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                // 分頁
                var pagedUnits = allUnits
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(unit => new { UnitId = unit.Guid, UnitName = unit.Name })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = pagedUnits,
                    totalRecords = totalRecords,
                    totalPages = totalPages
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // 例外處理，記錄錯誤資訊
                Console.WriteLine($"獲取生產單位列表時發生錯誤: {ex.Message}");
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 獲取生產單位詳情
        /// </summary>
        /// <param name="unitId">生產單位ID</param>
        /// <returns>生產單位詳情</returns>
        [HttpGet]
        public JsonResult GetProductionUnit(string unitId)
        {
            try
            {
                if (string.IsNullOrEmpty(unitId))
                {
                    return Json(new { success = false, message = "生產單位ID不能為空" }, JsonRequestBehavior.AllowGet);
                }

                // 在實際應用中，這裡應該從資料庫獲取生產單位詳情
                // 這裡為了示範，直接返回一個包含ID的對象
                var unit = new { UnitId = unitId, UnitName = unitId, Description = "", IsActive = true };

                return Json(new { success = true, data = unit }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"獲取生產單位詳情時發生錯誤: {ex.Message}");
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 保存（新增或更新）生產單位
        /// </summary>
        /// <returns>保存結果</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveProductionUnit(string UnitId, string UnitName, string Description, bool IsActive)
        {
            try
            {
                if (string.IsNullOrEmpty(UnitName))
                {
                    return Json(new { success = false, message = "生產單位名稱不能為空" });
                }

                // 獲取當前用戶資訊
                string userId = (Session["Member"] as MemberViewModels)?.fUserId ?? Environment.UserName;

                // 判斷是新增還是更新
                bool isNew = string.IsNullOrEmpty(UnitId);
                string message = isNew ? "新增成功" : "更新成功";

                // 在實際應用中，這裡應該調用服務層方法保存到資料庫
                // 這裡為了示範，直接返回成功

                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return await HandleAndLogErrorJson(ex, "保存生產單位", $"UnitId: {UnitId}, UnitName: {UnitName}");
            }
        }

        /// <summary>
        /// 刪除生產單位
        /// </summary>
        /// <param name="unitId">生產單位ID</param>
        /// <returns>刪除結果</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteProductionUnit(string unitId)
        {
            try
            {
                if (string.IsNullOrEmpty(unitId))
                {
                    return Json(new { success = false, message = "生產單位ID不能為空" });
                }

                // 獲取當前用戶資訊
                string userId = (Session["Member"] as MemberViewModels)?.fUserId ?? Environment.UserName;

                // 在實際應用中，這裡應該調用服務層方法從資料庫刪除
                // 這裡為了示範，直接返回成功

                return Json(new { success = true, message = "刪除成功" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刪除生產單位時發生錯誤: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region 釋放人員明細鎖定
        /// <summary>
        /// 釋放人員明細編輯鎖定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ReleaseEmployeeDetailLock(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "缺少人員明細ID" });

                string userId = GetCurrentUserId();
                var detail = await _manpowerAllocationServices.GetEmployeeDetailById(id).ConfigureAwait(false);
                if (detail == null)
                    return Json(new { success = false, message = "找不到指定的人員明細資料" });

                // 只有自己才能釋放自己的鎖定
                if (detail.str目前編輯者 == userId)
                {
                    detail.str目前編輯者 = null;
                    detail.dt編輯鎖定時間 = null;
                    await _manpowerAllocationServices.UpdateEmployeeDetailLockStatus(id, null, null);
                    return Json(new { success = true, message = "已釋放鎖定" });
                }
                else
                {
                    return Json(new { success = false, message = "您不是該筆資料的鎖定者" });
                }
            }
            catch (Exception ex)
            {
                return await HandleAndLogErrorJson(ex, "釋放人員明細鎖定", $"ID: {id}").ConfigureAwait(false);
            }
        }
        #endregion

    }
}
