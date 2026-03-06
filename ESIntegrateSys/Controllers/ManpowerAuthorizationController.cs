using System;
using System.Web.Mvc;
using ESIntegrateSys.Services;
using ESIntegrateSys.Models_ManpowerAllocation;
using System.Collections.Generic;
using System.Linq;
using ESIntegrateSys.Filters;

namespace ESIntegrateSys.Controllers
{
    /// <summary>
    /// 人力配置授權管理控制器
    /// </summary>
    [DepartmentAuthorization(new string[] { "IT", "IE" })] // 限制只有IT部門、IE部門可以存取  
    public class ManpowerAuthorizationController : Controller
    {
        private readonly IManpowerAuthorizationService _授權服務;

        /// <summary>
        /// 建構函式，注入授權服務
        /// </summary>
        public ManpowerAuthorizationController(ManpowerAuthorizationService 授權服務)
        {
            _授權服務 = 授權服務;
        }

        #region 報工生產單位 CRUD
        /// <summary>
        /// 取得所有報工生產單位
        /// </summary>
        public ActionResult GetAllProductionUnits()
        {
            var 單位清單 = _授權服務.取得所有報工生產單位();
            return Json(單位清單, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 取得生產單位下拉選單資料
        /// </summary>
        public ActionResult GetProductionUnitsForSelect()
        {
            try
            {
                var 單位清單 = _授權服務.取得所有報工生產單位()
                    .Where(u => u.啟用)  // 只取啟用的單位
                    .Select(u => new
                    {
                        value = u.單位ID.ToString(),
                        text = u.單位名稱
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = 單位清單
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 分頁查詢報工生產單位資料
        /// </summary>
        public ActionResult GetProductionUnitsWithPaging(string unitName, bool? isActive, int page = 1, int pageSize = 10)
        {
            try
            {
                // 獲取所有報工生產單位資料
                var allUnits = _授權服務.取得所有報工生產單位();

                // 根據條件過濾資料
                var filteredData = allUnits.AsQueryable();

                // 如果有單位名稱條件，進行過濾
                if (!string.IsNullOrEmpty(unitName))
                {
                    filteredData = filteredData.Where(p => p.單位名稱 != null &&
                                                       p.單位名稱.Contains(unitName));
                }

                // 如果有啟用狀態條件，進行過濾
                if (isActive.HasValue)
                {
                    filteredData = filteredData.Where(p => p.啟用 == isActive.Value);
                }

                // 將過濾後的資料轉換為列表
                var filteredList = filteredData.ToList();

                // 計算總記錄數
                var totalRecords = filteredList.Count;

                // 計算總頁數
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                // 分頁資料
                var pagedData = filteredList
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = pagedData,
                    totalPages = totalPages,
                    totalRecords = totalRecords,
                    currentPage = page
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 新增報工生產單位
        /// </summary>
        [HttpPost]
        public ActionResult CreateProductionUnit(ES_ReportingUnitDto dto)
        {
            _授權服務.新增報工生產單位(dto);
            return Json(new { success = true });
        }

        /// <summary>
        /// 更新報工生產單位
        /// </summary>
        [HttpPost]
        public ActionResult UpdateProductionUnit(ES_ReportingUnitDto dto)
        {
            _授權服務.更新報工生產單位(dto);
            return Json(new { success = true });
        }

        /// <summary>
        /// 刪除報工生產單位
        /// </summary>
        [HttpPost]
        public ActionResult DeleteProductionUnit(int 單位ID)
        {
            _授權服務.刪除報工生產單位(單位ID);
            return Json(new { success = true });
        }

        /// <summary>
        /// 根據 ID 獲取報工生產單位
        /// </summary>
        public ActionResult GetProductionUnitById(int id)
        {
            try
            {
                var 單位 = _授權服務.取得所有報工生產單位().FirstOrDefault(u => u.單位ID == id);
                
                if (單位 == null)
                {
                    return Json(new { success = false, message = "找不到指定的報工生產單位" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = 單位.單位ID,
                        unitName = 單位.單位名稱,
                        status = 單位.啟用 ? 1 : 0
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 使用者 CRUD
        /// <summary>
        /// 取得所有使用者
        /// </summary>
        public ActionResult GetAllUsers()
        {
            var 使用者清單 = _授權服務.取得所有使用者();
            return Json(使用者清單, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新增使用者
        /// </summary>
        [HttpPost]
        public ActionResult CreateUser(MP_UserDto dto)
        {
            _授權服務.新增使用者(dto);
            return Json(new { success = true });
        }

        /// <summary>
        /// 更新使用者
        /// </summary>
        [HttpPost]
        public ActionResult UpdateUser(MP_UserDto dto)
        {
            _授權服務.更新使用者(dto);
            return Json(new { success = true });
        }

        /// <summary>
        /// 刪除使用者
        /// </summary>
        [HttpPost]
        public ActionResult DeleteUser(string 使用者ID)
        {
            _授權服務.刪除使用者(使用者ID);
            return Json(new { success = true });
        }
        #endregion

        #region 權限 CRUD
        /// <summary>
        /// 取得所有權限
        /// </summary>
        public ActionResult GetAllPermissions()
        {
            var 權限清單 = _授權服務.取得所有權限();
            return Json(權限清單, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新增權限
        /// </summary>
        [HttpPost]
        public ActionResult CreatePermission(MP_ProductionUnitPermissionDto dto)
        {
            _授權服務.新增權限(dto);
            return Json(new { success = true });
        }

        /// <summary>
        /// 更新權限
        /// </summary>
        [HttpPost]
        public ActionResult UpdatePermission(MP_ProductionUnitPermissionDto dto)
        {
            _授權服務.更新權限(dto);
            return Json(new { success = true });
        }

        /// <summary>
        /// 刪除權限
        /// </summary>
        [HttpPost]
        public ActionResult DeletePermission(int 權限ID)
        {
            _授權服務.刪除權限(權限ID);
            return Json(new { success = true });
        }
        #endregion

        #region 權限查詢
        /// <summary>
        /// 取得使用者可編輯的報工生產單位清單
        /// </summary>
        public ActionResult GetUserEditableUnits(string 使用者ID)
        {
            var 單位清單 = _授權服務.取得使用者可編輯單位(使用者ID);
            return Json(單位清單, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 完整頁面視圖
        /// <summary>
        /// 人力配置授權頁面
        /// </summary>
        public ActionResult 人力配置授權()
        {
            return View("ManpowerAuthorization");
        }
        /// <summary>
        /// 生產單位頁面
        /// </summary>
        public ActionResult 生產單位設定()
        {
            return View("_ProductionUnit");
        }

        /// <summary>
        /// 報工生產單位頁面
        /// </summary>
        public ActionResult 報工生產單位設定()
        {
            return View("_UnitNameQueryPartial");
        }

        /// <summary>
        /// 權限設定頁面
        /// </summary>
        public ActionResult 權限設定()
        {
            return View("_PermissionQueryPartial");
        }
        #endregion

        #region 部分視圖
        public ActionResult Get生產單位Partial()
        {
            return PartialView("_ProductionUnit");
        }

        /// <summary>
        /// 取得報工生產單位部分視圖
        /// </summary>
        public ActionResult Get報工生產單位Partial()
        {
            return PartialView("_UnitNameQueryPartial");
        }

        /// <summary>
        /// 取得使用者部分視圖
        /// </summary>
        public ActionResult GetUserPartial()
        {
            return PartialView("_UserQueryPartial");
        }

        /// <summary>
        /// 取得權限部分視圖
        /// </summary>
        public ActionResult Get權限設定Partial()
        {
            return PartialView("_PermissionQueryPartial");
        }
        #endregion

        /// <summary>
        /// 分頁查詢權限資料
        /// </summary>
        public ActionResult GetPermissionsWithPaging(string productionUnit, string employeeId, int page = 1, int pageSize = 10)
        {
            try
            {
                // 獲取所有權限資料（已包含關聯資料）
                var allPermissions = _授權服務.取得所有權限();

                // 根據條件過濾資料
                var filteredData = allPermissions.AsQueryable();

                // 如果有生產單位條件，進行過濾
                if (!string.IsNullOrEmpty(productionUnit))
                {
                    filteredData = filteredData.Where(p => p.UnitName != null &&
                                                       p.UnitName.Contains(productionUnit));
                }

                // 如果有人員工號條件，進行過濾
                if (!string.IsNullOrEmpty(employeeId))
                {
                    filteredData = filteredData.Where(p => p.EmpId != null &&
                                                       p.EmpId.Contains(employeeId));
                }

                // 將過濾後的資料轉換為列表
                var filteredList = filteredData.ToList();

                // 計算總記錄數
                var totalRecords = filteredList.Count;

                // 計算總頁數
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                // 分頁資料
                var pagedData = filteredList
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = pagedData,
                    totalPages = totalPages,
                    totalRecords = totalRecords,
                    currentPage = page
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
