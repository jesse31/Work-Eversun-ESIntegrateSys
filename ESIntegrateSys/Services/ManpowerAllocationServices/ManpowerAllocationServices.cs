using DocumentFormat.OpenXml.Spreadsheet;
using ESIntegrateSys.Models;
using ESIntegrateSys.Models_ManpowerAllocation;
using ESIntegrateSys.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace ESIntegrateSys.Services.ManpowerAllocationServices
{
    /// <summary>
    /// 人力配置服務實現類
    /// </summary>
    public class ManpowerAllocationServices : IManpowerAllocationServices
    {
        /// <summary>
        /// 資料庫上下文
        /// </summary>
        private readonly ESIntegrateSysEntities _db;


        /// <summary>
        /// 構造函數
        /// </summary>
        public ManpowerAllocationServices()
        {
            _db = new ESIntegrateSysEntities();
        }


        #region SELECT 統計 / 明細

        /// <summary>
        /// 獲取人力配置統計資料
        /// </summary>
        /// <param name="request">查詢條件</param>
        /// <returns>人力配置統計資料</returns>
        public async Task<IEnumerable<人力配置統計>> GetManpowerAllocationStatistics(人力配置明細 request)
        {
            // 取得 EmployeeAssignments，並關聯必要的資料表
            var query = _db.EmployeeAssignments
                .Include(ea => ea.HResources_報工生產單位) // 對應 ES_ReportingUnit
                .Include(ea => ea.HResources_生產單位名稱) // 對應 ES_ProductionUnit
                .Include(ea => ea.HResources_班別)         // 對應 ES_Shift
                .Include(ea => ea.HResources_出勤類型)     // 對應 ES_AttendanceType
                .AsQueryable();

            // 動態查詢條件
            if (request != null)
            {
                if (!string.IsNullOrWhiteSpace(request.str生產單位名稱))
                {
                    query = query.Where(ea => ea.HResources_生產單位名稱.GUID.ToString() == request.str生產單位名稱);
                }
                if (!string.IsNullOrWhiteSpace(request.str班別名稱))
                {
                    query = query.Where(ea => ea.HResources_班別.shift_ID == request.str班別名稱);
                }
                if (!string.IsNullOrWhiteSpace(request.str出勤類型))
                {
                    query = query.Where(ea => ea.出勤類型GUID.ToString() == request.str出勤類型);
                }
                if (!string.IsNullOrWhiteSpace(request.str作業人員工號))
                {
                    query = query.Where(ea => ea.employee_id.StartsWith(request.str作業人員工號));
                }
                if (!string.IsNullOrWhiteSpace(request.str作業人員姓名))
                {
                    query = query.Where(ea => ea.HResources_人員.employee_name.StartsWith(request.str作業人員姓名));
                }
            }

            // 分組統計
            var result = await query
                .GroupBy(ea => new { ea.HResources_生產單位名稱.生產單位名稱, ea.HResources_班別.生產單位班別 })
                .Select(g => new 人力配置統計
                {
                    str生產單位 = g.Key.生產單位名稱,
                    str班別名稱 = g.Key.生產單位班別,
                    int間接出勤人數 = g.Count(ea => ea.HResources_出勤類型.出勤名稱 == "間接出勤"),
                    int線外出勤人數 = g.Count(ea => ea.HResources_出勤類型.出勤名稱 == "線外出勤"),
                    int直接出勤人數 = g.Count(ea => ea.HResources_出勤類型.出勤名稱 == "直接出勤"),
                    int留停人數 = g.Count(ea => ea.HResources_出勤類型.出勤名稱 == "留停")
                })
                .OrderBy(x => x.str生產單位)
                .ToListAsync();

            return result;
        }


        /// <summary>
        /// 獲取人員明細資料
        /// </summary>
        /// <param name="productionUnit">生產單位名稱</param>
        /// <param name="shift">班別名稱</param>
        /// <param name="attendanceType">出勤類型</param>
        /// <returns>人員明細資料</returns>
        public async Task<IEnumerable<人力配置明細>> GetEmployeeDetails(人力配置明細 request)
        {
            // 以 EF 查詢 EmployeeAssignments 並關聯必要資料表
            var query = _db.EmployeeAssignments
                .Include(ea => ea.HResources_報工生產單位)
                .Include(ea => ea.HResources_生產單位名稱)
                .Include(ea => ea.HResources_班別)
                .Include(ea => ea.HResources_人員)
                .Include(ea => ea.HResources_出勤類型)
                .AsQueryable();

            // 動態查詢條件
            if (request != null)
            {
                if (!string.IsNullOrWhiteSpace(request.str生產單位名稱))
                {
                    query = query.Where(ea => ea.HResources_生產單位名稱.生產單位名稱 == request.str生產單位名稱);
                }
                if (!string.IsNullOrWhiteSpace(request.str班別名稱))
                {
                    query = query.Where(ea => ea.HResources_班別.生產單位班別 == request.str班別名稱);
                }
                if (!string.IsNullOrWhiteSpace(request.str出勤類型))
                {
                    query = query.Where(ea => ea.HResources_出勤類型.出勤名稱 == request.str出勤類型);

                }
                if (!string.IsNullOrWhiteSpace(request.str作業人員工號))
                {
                    query = query.Where(ea => ea.employee_id.StartsWith(request.str作業人員工號));
                }
                if (!string.IsNullOrWhiteSpace(request.str作業人員姓名))
                {
                    query = query.Where(ea => ea.HResources_人員.employee_name.StartsWith(request.str作業人員姓名));
                }
            }

            // 先查詢資料，再於記憶體中進行權限判斷
            var permissionDict = GetProductionUnitSpecificPermissions();
            var list = await query.Select(ea => new 人力配置明細
            {
                strPK = ea.assignment_id.ToString(),
                str報工生產單位名稱 = ea.HResources_報工生產單位.報工生產單位名稱,
                str生產單位名稱 = ea.HResources_生產單位名稱.生產單位名稱,
                str班別名稱 = ea.HResources_班別.生產單位班別,
                str出勤類型 = ea.HResources_出勤類型.出勤名稱,
                str作業人員工號 = ea.employee_id,
                str作業人員姓名 = ea.HResources_人員.employee_name,
                str備註 = ea.remarks
            }).ToListAsync();

            // 權限判斷於記憶體中處理，避免 LINQ 運算式樹 out 參數錯誤
            foreach (var item in list)
            {
                item.blnIsEdDel = (request.str部門 == "IT" || request.str部門 == "IE")
                    || (!string.IsNullOrEmpty(request.str修改者)
                        && permissionDict.TryGetValue(item.str報工生產單位名稱, out var allowedUsers)
                        && allowedUsers.Contains(request.str修改者));
            }

            return list;
        }

        #endregion

        #region 取得人力配置明細

        /// <summary>
        /// 根據 ID 獲取單筆人員明細資料
        /// </summary>
        /// <param name="id">人員明細 ID</param>
        /// <returns>人員明細資料</returns>
        public async Task<人力配置明細異動Dto> GetEmployeeDetailById(string id)
        {
            人力配置明細異動Dto detail = null;

            try
            {
                // 以 EF 查詢單筆 EmployeeAssignment 並關聯必要資料表
                var entity = await _db.EmployeeAssignments
                    .Include(ea => ea.HResources_報工生產單位)
                    .Include(ea => ea.HResources_生產單位名稱)
                    .Include(ea => ea.HResources_班別)
                    .Include(ea => ea.HResources_人員)
                    .Include(ea => ea.HResources_出勤類型)
                    .FirstOrDefaultAsync(ea => ea.assignment_id.ToString() == id);

                if (entity is null)
                {
                    return null;
                }

                detail = new 人力配置明細異動Dto
                {
                    strPK = entity.assignment_id.ToString(),
                    str報工生產單位名稱 = entity.HResources_報工生產單位?.報工生產單位名稱,
                    str報工生產單位GUID = entity.報工生產單位GUID.ToString(),
                    str生產單位名稱 = entity.HResources_生產單位名稱?.生產單位名稱,
                    str生產單位GUID = entity.生產單位名稱GUID.ToString(),
                    str班別名稱 = entity.HResources_班別?.生產單位班別,
                    str班別GUID = entity.班別GUID.ToString(),
                    str出勤類型 = entity.HResources_出勤類型?.出勤名稱,
                    str出勤GUID = entity.出勤類型GUID.ToString(),
                    str作業人員工號 = entity.employee_id,
                    str作業人員姓名 = entity.HResources_人員?.employee_name,
                    str備註 = entity.remarks,
                    str目前編輯者 = entity.current_editor,
                    dt編輯鎖定時間 = entity.edit_lock_time
                };

                return detail;
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                await LogHelper.LogErrorAsync(ex, "獲取單筆人員明細資料").ConfigureAwait(false);
                return null;
            }
        }

        #endregion

        #region  編輯人力配置明細

        /// <summary>
        /// 更新人員明細資料（EF 交易式實作）
        /// </summary>
        /// <param name="model">人員明細資料</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UpdateEmployeeDetail(人力配置明細異動Dto model)
        {
            // 記錄是否成功
            bool success = false;

            try
            {
                // 取得更新前資料（用於日誌）
                人力配置明細異動Dto beforeUpdateData = await GetEmployeeDetailById(model.strPK);
                if (beforeUpdateData == null)
                {
                    Console.WriteLine($"準備更新資料 (ID: {model.strPK})，但找不到更新前的資料。日誌記錄將不完整。");
                }
                if (beforeUpdateData != null)
                {
                    // 使用 EF 交易式寫入日誌
                    await LogEmployeeAssignmentActionAsync(model.strPK, "UPDATE", model.str修改者, beforeUpdateData);
                }

                // 以 EF 交易方式進行資料異動
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        // 尋找對應的 Entity
                        if (!int.TryParse(model.strPK, out int assignmentId))
                        {
                            throw new ArgumentException("strPK 不是有效的 assignment_id");
                        }
                        var entity = await _db.EmployeeAssignments.FindAsync(assignmentId);
                        if (entity is null)
                        {
                            throw new Exception($"找不到指定的人員明細資料 (ID: {model.strPK})");
                        }

                        // 依據報工生產單位GUID查詢reporting_unit_ID
                        string reportingUnitId = null;
                        if (Guid.TryParse(model.str報工生產單位名稱, out var ruGuid))
                        {
                            var reportingUnit = _db.HResources_報工生產單位.FirstOrDefault(x => x.GUID == ruGuid);
                            if (reportingUnit != null)
                            {
                                reportingUnitId = reportingUnit.reporting_unit_ID;
                            }
                            else
                            {
                                // 查無對應GUID，記錄警告
                                await LogHelper.LogWarningAsync($"查無對應報工生產單位GUID: {ruGuid}，reporting_unit_ID 將設為 null").ConfigureAwait(false);
                            }
                        }

                        // 更新欄位
                        entity.報工生產單位GUID = Guid.TryParse(model.str報工生產單位名稱, out var repGuid) ? (Guid?)repGuid : null;
                        entity.生產單位名稱GUID = Guid.TryParse(model.str生產單位名稱, out var prodGuid) ? (Guid?)prodGuid : null;
                        entity.班別GUID = Guid.TryParse(model.str班別名稱, out var shiftGuid) ? (Guid?)shiftGuid : null;
                        entity.出勤類型GUID = Guid.TryParse(model.str出勤類型, out var attGuid) ? (Guid?)attGuid : null;
                        entity.employee_id = model.str作業人員工號;
                        entity.remarks = model.str備註;
                        entity.updated_by = model.str修改者;
                        entity.updated_at = DateTime.Now;
                        entity.current_editor = model.str目前編輯者;
                        entity.edit_lock_time = model.dt編輯鎖定時間;
                        entity.reporting_unit_ID = reportingUnitId; // 中文註解：依據報工生產單位GUID寫入對應的reporting_unit_ID

                        // 儲存異動
                        await _db.SaveChangesAsync();
                        transaction.Commit();
                        success = true;
                        Console.WriteLine($"資料 (ID: {model.strPK}) 更新成功。");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"資料 (ID: {model.strPK}) 更新過程中發生錯誤: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                await LogHelper.LogErrorAsync(ex, $"更新人員明細資料時發生錯誤 (ID: {model.strPK})").ConfigureAwait(false);
                success = false;
            }

            return success;
        }

        #endregion

        #region 新增人力配置明細資料

        /// <summary>
        /// 新增人員明細，使用 Entity Framework 與交易式確保資料一致性
        /// </summary>
        /// <param name="model">前端傳入的人力配置明細異動Dto 物件</param>
        /// <returns>新增成功回傳 true，失敗回傳 false</returns>
        public async Task<bool> AddEmployeeDetail(人力配置明細異動Dto model)
        {
            // 檢查工號是否為空
            if (string.IsNullOrWhiteSpace(model.str作業人員工號))
            {
                await LogHelper.LogWarningAsync($"新增人員明細失敗：工號為空").ConfigureAwait(false);
                return false;
            }
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // 先檢查工號是否存在於 HResources_人員
                    var employee = _db.HResources_人員.FirstOrDefault(x => x.employee_id == model.str作業人員工號);
                    if (employee is null)
                    {
                        // 工號不存在，於同一交易內自動新增
                        var newEmployee = new HResources_人員
                        {
                            GUID = Guid.NewGuid(),
                            employee_id = model.str作業人員工號,
                            employee_name = model.str作業人員姓名 ?? string.Empty,
                            CreatedBy = model.str建立者 ?? "system",
                            CreatedAt = DateTime.Now
                        };
                        _db.HResources_人員.Add(newEmployee);
                        await _db.SaveChangesAsync();
                        await LogHelper.LogInfoAsync($"自動新增工號 {model.str作業人員工號} 至 HResources_人員").ConfigureAwait(false);
                    }

                    // 依據報工生產單位GUID查詢reporting_unit_ID
                    string reportingUnitId = null;
                    if (Guid.TryParse(model.str報工生產單位名稱, out var ruGuid))
                    {
                        var reportingUnit = _db.HResources_報工生產單位.FirstOrDefault(x => x.GUID == ruGuid);
                        if (reportingUnit != null)
                        {
                            reportingUnitId = reportingUnit.reporting_unit_ID;
                        }
                        else
                        {
                            // 查無對應GUID，記錄警告
                            await LogHelper.LogWarningAsync($"查無對應報工生產單位GUID: {ruGuid}，reporting_unit_ID 將設為 null").ConfigureAwait(false);
                        }
                    }

                    // 建立 EmployeeAssignments 物件，並寫入 reporting_unit_ID
                    var entity = new ESIntegrateSys.Models.EmployeeAssignments
                    {
                        報工生產單位GUID = Guid.TryParse(model.str報工生產單位名稱, out var ruGuid2) ? (Guid?)ruGuid2 : null,
                        生產單位名稱GUID = Guid.TryParse(model.str生產單位名稱, out var puGuid) ? (Guid?)puGuid : null,
                        班別GUID = Guid.TryParse(model.str班別名稱, out var shiftGuid) ? (Guid?)shiftGuid : null,
                        出勤類型GUID = Guid.TryParse(model.str出勤類型, out var attGuid) ? (Guid?)attGuid : null,
                        employee_id = model.str作業人員工號,
                        remarks = model.str備註,
                        created_by = model.str建立者,
                        created_at = DateTime.Now,
                        current_editor = model.str目前編輯者,
                        edit_lock_time = model.dt編輯鎖定時間,
                        reporting_unit_ID = reportingUnitId // 中文註解：依據報工生產單位GUID寫入對應的reporting_unit_ID
                    };
                    _db.EmployeeAssignments.Add(entity);
                    await _db.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    await LogHelper.LogErrorAsync(ex, $"新增人員明細時發生錯誤 (工號: {model.str作業人員工號})").ConfigureAwait(false);
                    return false;
                }
            }
        }

        #endregion

        #region  刪除人力配置明細資料

        /// <summary>
        /// 刪除人力配置明細資料（使用 Entity Framework 與交易，強化 id 型別檢查）
        /// </summary>
        /// <param name="id">要刪除的明細主鍵</param>
        /// <param name="deletedBy">刪除人員</param>
        /// <param name="reason">刪除原因</param>
        /// <returns>刪除是否成功</returns>
        public async Task<bool> DeleteEmployeeDetail(string id, string deletedBy, string reason)
        {
            // 取得刪除前的資料（用於日誌）
            人力配置明細異動Dto beforeDeleteData = await GetEmployeeDetailById(id);
            if (beforeDeleteData is null)
            {
                await LogHelper.LogWarningAsync($"準備刪除資料 (ID: {id})，但找不到刪除前的資料。日誌記錄將不完整。").ConfigureAwait(false);
                return false;
            }
            // 強化 id 型別檢查
            if (!int.TryParse(id, out int assignmentId))
            {
                await LogHelper.LogWarningAsync($"刪除失敗，id 不是有效的 assignment_id (ID: {id})").ConfigureAwait(false);
                return false;
            }
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // 取得要刪除的明細資料
                    var entity = _db.EmployeeAssignments.FirstOrDefault(e => e.assignment_id == assignmentId);
                    if (entity is null)
                    {
                        await LogHelper.LogWarningAsync($"刪除失敗，找不到 assignment_id: {assignmentId}").ConfigureAwait(false);
                        transaction.Rollback();
                        return false;
                    }
                    string employeeId = entity.employee_id;
                    // 刪除明細
                    _db.EmployeeAssignments.Remove(entity);
                    await _db.SaveChangesAsync();
                    // 檢查是否還有其他明細使用同一個 employee_id
                    bool hasOther = _db.EmployeeAssignments.Any(e => e.employee_id == employeeId);
                    if (!hasOther)
                    {
                        // 沒有其他明細，刪除 HResources_人員
                        var hrEntity = _db.HResources_人員.FirstOrDefault(e => e.employee_id == employeeId);
                        if (hrEntity != null)
                        {
                            _db.HResources_人員.Remove(hrEntity);
                            await LogHelper.LogInfoAsync($"自動刪除 HResources_人員 工號: {employeeId}").ConfigureAwait(false);
                        }
                    }
                    await _db.SaveChangesAsync();
                    transaction.Commit();
                    // 寫日誌
                    await LogEmployeeAssignmentActionAsync(id, "DELETE", deletedBy, beforeDeleteData);
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    await LogHelper.LogErrorAsync(ex, $"刪除人員明細時發生錯誤 (ID: {id})").ConfigureAwait(false);
                    return false;
                }
            }
        }

        #endregion

        #region List
        /// <summary>
        /// 獲取報工生產單位列表
        /// </summary>
        /// <param name="userId">登入者的用戶ID</param>
        /// <param name="userDeptNo">登入者的部門編號</param>
        /// <returns>根據用戶權限過濾後的報工生產單位列表</returns>
        public async Task<List<報工生產單位下拉選單Dto>> Get報工生產單位List(string userId, string userDeptNo)
        {
            // 取得所有啟用的報工生產單位名稱
            var allUnits = await _db.HResources_報工生產單位
                .Where(x => x.是否啟用)
                .OrderBy(x => x.報工生產單位名稱)
                .Select(x => new 報工生產單位下拉選單Dto
                {
                    Guid = x.GUID.ToString(),
                    Name = x.報工生產單位名稱
                })
                .ToListAsync();

            // 若部門為IE或IT，則回傳全部單位
            if (!string.IsNullOrWhiteSpace(userDeptNo) && (userDeptNo == "IE" || userDeptNo == "IT"))
            {
                return allUnits;
            }
            // 若有userId，則依授權表過濾
            else if (!string.IsNullOrEmpty(userId))
            {
                // 取得授權表（key=報工生產單位名稱, value=授權者工號清單）
                var permissionDict = GetProductionUnitSpecificPermissions();
                // 取得有授權的報工生產單位名稱清單
                var allowedUnitNames = permissionDict.Where(kvp => kvp.Value.Contains(userId)).Select(x => x.Key).ToList();
                // 過濾allUnits，僅回傳有授權的單位
                var filteredUnits = allUnits.Where(u => allowedUnitNames.Contains(u.Name)).ToList();
                return filteredUnits;
            }
            // 其他情況回傳空清單
            else
            {
                return new List<報工生產單位下拉選單Dto>();
            }
        }

        /// <summary>
        /// 取得生產單位下拉選單（key=GUID, value=生產單位名稱），並依授權過濾
        /// </summary>
        public async Task<List<生產單位下拉選單Dto>> Get生產單位下拉選單Async(string userId = null, string userDeptNo = null)
        {
            // 取得所有啟用的生產單位
            var allUnits = await _db.HResources_生產單位名稱
                .Where(x => x.是否啟用)
                .OrderBy(x => x.生產單位名稱)
                .Select(x => new 生產單位下拉選單Dto
                {
                    Guid = x.GUID.ToString(),
                    Name = x.生產單位名稱
                })
                .ToListAsync();

            // 若部門為IE或IT，則回傳全部單位
            if (!string.IsNullOrWhiteSpace(userDeptNo) && (userDeptNo == "IE" || userDeptNo == "IT"))
            {
                return allUnits;
            }
            // 若有userId，則依授權表過濾
            else if (!string.IsNullOrEmpty(userId))
            {
                // 取得授權表（key=生產單位名稱或報工生產單位名稱, value=授權者工號清單）
                var permissionDict = GetProductionUnitSpecificPermissions();
                // 取得有授權的生產單位名稱清單
                var allowedUnitNames = permissionDict.Where(kvp => kvp.Value.Contains(userId)).Select(x => x.Key).ToList();
                // 過濾allUnits，僅回傳有授權的生產單位
                var filteredUnits = allUnits.Where(u => allowedUnitNames.Contains(u.Name)).ToList();
                return filteredUnits;
            }
            // 其他情況回傳空清單
            else
            {
                return new List<生產單位下拉選單Dto>();
            }
        }

        /// <summary>
        /// 取得班別下拉選單（首頁用）
        /// </summary>
        public async Task<List<班別下拉選單首頁Dto>> Get查詢班別下拉選單Async()
        {
            // 取得所有啟用的班別，並依照「日班」、「小夜班」、「大夜班」自訂順序排序
            var allShifts = await _db.HResources_班別
                .Where(x => x.是否啟用)
                .OrderBy(x => x.班別名稱 == "日班" ? 1 : x.班別名稱 == "小夜班" ? 2 : x.班別名稱 == "大夜班" ? 3 : 99)
                .ThenBy(x => x.班別名稱) // 其他班別依名稱排序
                .Select(x => new 班別下拉選單首頁Dto
                {
                    shiftID = x.shift_ID.ToString(),
                    Name = x.班別名稱
                })
                .Distinct()
                .ToListAsync();

            return allShifts;

        }

        /// <summary>
        /// 取得班別下拉選單（新增/修改用）
        /// </summary>
        public async Task<List<班別下拉選單Dto>> Get班別下拉選單By新增修改Async(string userId = null, string userDeptNo = null)
        {
            // 取得所有啟用班別，依「日班」、「小夜班」、「大夜班」自訂順序排序，其他班別依名稱排序
            var allShifts = await _db.HResources_班別
                .Where(x => x.是否啟用)
                .OrderBy(x => x.生產單位班別.EndsWith("日班") ? 1 : x.生產單位班別.EndsWith("小夜班") ? 2 : x.生產單位班別.EndsWith("大夜班") ? 3 : 99)
                .ThenBy(x => x.生產單位班別)
                .Select(x => new 班別下拉選單Dto
                {
                    Guid = x.GUID.ToString(),
                    Name = x.生產單位班別
                })
                .ToListAsync();

            // 若部門為IE或IT，則回傳全部班別
            if (!string.IsNullOrWhiteSpace(userDeptNo) && (userDeptNo == "IE" || userDeptNo == "IT"))
            {
                return allShifts;
            }
            // 若有userId，則依授權表過濾
            else if (!string.IsNullOrEmpty(userId))
            {
                // 取得授權表（key=班別名稱, value=授權者工號清單）
                var permissionDict = GetProductionUnitSpecificPermissions();
                // 取得有授權的班別名稱清單
                var allowedShiftNames = permissionDict.Where(kvp => kvp.Value.Contains(userId)).Select(x => x.Key).ToList();
                // 過濾allShifts，僅回傳有授權的班別
                var filteredShifts = allShifts.Where(s => allowedShiftNames.Contains(s.Name)).ToList();
                return filteredShifts;
            }
            // 其他情況回傳空清單
            else
            {
                return new List<班別下拉選單Dto>();
            }
        }

        /// <summary>
        /// 取得出勤類型下拉選單
        /// </summary>
        public async Task<List<出勤類型下拉選單Dto>> Get出勤類型下拉選單Async()
        {
            // 依「間接出勤」、「線外出勤」、「直接出勤」、「留停」自訂順序排序，其他出勤類型依名稱排序
            return await _db.HResources_出勤類型
                .Where(x => x.是否啟用)
                .OrderBy(x => x.出勤名稱 == "間接出勤" ? 1 : x.出勤名稱 == "線外出勤" ? 2 : x.出勤名稱 == "直接出勤" ? 3 : x.出勤名稱 == "留停" ? 4 : 99)
                .ThenBy(x => x.出勤名稱)
                .Select(x => new 出勤類型下拉選單Dto
                {
                    Guid = x.GUID.ToString(),
                    Name = x.出勤名稱
                })
                .ToListAsync();
        }
        #endregion

        #region 授權
        /// <summary>
        /// 取得報工生產單位特定權限設定
        /// </summary>
        private Dictionary<string, List<string>> GetProductionUnitSpecificPermissions()
        {

            /* SMT表面貼焊 : (00761)鄭崑璋、(02491)顏伯翰
             * 板卡測試、板卡包裝、板卡後製程_前段、板卡後製程_後段 : (02423)湯慧勤、(00018)許慧萍
             * 系統組裝、系統包裝、系統測試: (02423)湯慧勤、(02688)沈聖凱、(02942)吳桂美
             */

            // 取得授權表，包含生產單位與報工生產單位名稱
            var authList = _db.人力配置授權表.Where(x => x.狀態 == true).ToList();

            // 建立一個 Dictionary，key 為報工生產單位名稱或生產單位名稱，value 為授權者工號清單
            var result = new Dictionary<string, List<string>>();

            foreach (var item in authList)
            {
                // 報工生產單位名稱授權
                if (!string.IsNullOrWhiteSpace(item.報工生產單位名稱))
                {
                    if (!result.ContainsKey(item.報工生產單位名稱))
                        result[item.報工生產單位名稱] = new List<string>();
                    if (!result[item.報工生產單位名稱].Contains(item.授權者工號))
                        result[item.報工生產單位名稱].Add(item.授權者工號);
                }
                // 生產單位名稱授權
                if (!string.IsNullOrWhiteSpace(item.生產單位))
                {
                    if (!result.ContainsKey(item.生產單位))
                        result[item.生產單位] = new List<string>();
                    if (!result[item.生產單位].Contains(item.授權者工號))
                        result[item.生產單位].Add(item.授權者工號);
                }
                //班別授權
                if (!string.IsNullOrWhiteSpace(item.班別))
                {
                    if (!result.ContainsKey(item.班別))
                        result[item.班別] = new List<string>();
                    if (!result[item.班別].Contains(item.授權者工號))
                        result[item.班別].Add(item.授權者工號);
                }
            }

            return result;
        }

        /// <summary>
        /// 取得授權者名單
        /// </summary>
        /// <returns></returns>
        public List<string> Get授權者UserIds()
        {
            return _db.人力配置授權表
                        .Where(x => x.狀態 == true)
                        .Select(x => x.授權者工號)
                        .Distinct().ToList();
        }
        #endregion

        #region 更新人員明細資料鎖定狀態

        /// <summary>
        /// 更新人員明細資料的鎖定狀態（EF 與交易式實作）
        /// </summary>
        /// <param name="id">人員明細ID</param>
        /// <param name="editorId">編輯者ID</param>
        /// <param name="lockTime">鎖定時間</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UpdateEmployeeDetailLockStatus(string id, string editorId, DateTime? lockTime)
        {
            // 記錄是否成功
            bool success = false;
            try
            {
                // 強化 id 型別檢查
                if (!int.TryParse(id, out int assignmentId))
                {
                    await LogHelper.LogWarningAsync($"更新鎖定狀態失敗，id 不是有效的 assignment_id (ID: {id})").ConfigureAwait(false);
                    return false;
                }

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        // 以 EF 查詢對應的 EmployeeAssignments 物件
                        var entity = await _db.EmployeeAssignments.FindAsync(assignmentId);
                        if (entity is null)
                        {
                            await LogHelper.LogWarningAsync($"更新鎖定狀態失敗，找不到 EmployeeAssignments (ID: {id})").ConfigureAwait(false);
                            return false;
                        }
                        // 更新欄位
                        entity.current_editor = editorId;
                        entity.edit_lock_time = lockTime;
                        // 儲存異動
                        int affected = await _db.SaveChangesAsync();
                        success = affected > 0;
                        if (success)
                        {
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        await LogHelper.LogErrorAsync(ex, $"EF 更新人員明細鎖定狀態時發生錯誤 (ID: {id})").ConfigureAwait(false);
                        success = false;
                    }
                }
            }
            catch (Exception ex)
            {
                await LogHelper.LogErrorAsync(ex, $"更新人員明細資料鎖定狀態時發生錯誤 (ID: {id}, 編輯者: {editorId})");
                success = false;
            }
            return success;
        }

        #endregion

        #region LOG
        /// <summary>
        /// 記錄人力配置明細的變動到日誌資料表（EF 交易式寫入）
        /// </summary>
        /// <param name="assignmentId">被操作的 Assignment ID</param>
        /// <param name="actionType">'UPDATE' 或 'DELETE'</param>
        /// <param name="performedBy">執行操作的使用者</param>
        /// <param name="oldData">操作前的資料</param>
        private async Task LogEmployeeAssignmentActionAsync(string assignmentId, string actionType, string performedBy, 人力配置明細異動Dto oldData)
        {
            // 若無舊資料則不記錄
            if (oldData is null)
            {
                Console.WriteLine($"無法記錄日誌：找不到 Assignment ID {assignmentId} 的舊資料。");
                return;
            }

            // 使用 EF 與交易式寫入日誌
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // 建立日誌物件
                    var log = new ESIntegrateSys.Models.EmployeeAssignments_Log
                    {
                        AssignmentId = assignmentId,
                        ActionType = actionType,
                        ActionTimestamp = DateTime.Now, // 自動填入當前時間
                        PerformedBy = string.IsNullOrEmpty(performedBy) ? Environment.UserName : performedBy,
                        Old_ReportingUnit = oldData.str報工生產單位名稱,
                        Old_ProductionUnit = oldData.str生產單位名稱,
                        Old_Shift = oldData.str班別名稱,
                        Old_AttendanceType = oldData.str出勤類型,
                        Old_EmployeeId = oldData.str作業人員工號,
                        Old_EmployeeName = oldData.str作業人員姓名,
                        Old_Remarks = oldData.str備註
                    };
                    // 新增日誌物件
                    _db.EmployeeAssignments_Log.Add(log);
                    await _db.SaveChangesAsync();
                    transaction.Commit();
                    Console.WriteLine($"已成功記錄 {actionType} 操作到日誌 (Assignment ID: {assignmentId})。");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"記錄 {actionType} 操作到日誌時發生錯誤 (Assignment ID: {assignmentId}): {ex.Message}");
                    // 根據您的錯誤處理策略，您可能希望重新拋出異常或採取其他行動
                }
            }
        }


        /// <summary>
        /// 刪除 EmployeeAssignments_Log 資料表中超過三個月的日誌記錄（使用 EF 與交易）。
        /// </summary>
        public void DeleteLogsOlderThanThreeMonths()
        {
            // 中文註解：開始執行刪除超過三個月的日誌記錄
            Console.WriteLine("開始執行刪除超過三個月的日誌記錄...");
            try
            {
                // 中文註解：計算三個月前的日期
                var threeMonthsAgo = DateTime.Now.AddMonths(-3);

                // 中文註解：查詢所有超過三個月的日誌記錄
                var oldLogs = _db.EmployeeAssignments_Log
                    .Where(log => log.ActionTimestamp < threeMonthsAgo)
                    .ToList();

                if (oldLogs.Count == 0)
                {
                    Console.WriteLine("沒有超過三個月的日誌記錄需要刪除。");
                    return;
                }

                // 中文註解：使用交易確保刪除操作的原子性
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        // 中文註解：批次刪除舊日誌
                        _db.EmployeeAssignments_Log.RemoveRange(oldLogs);
                        int rowsAffected = _db.SaveChanges();
                        transaction.Commit();
                        Console.WriteLine($"已成功刪除 {rowsAffected} 筆超過三個月的日誌記錄。");
                    }
                    catch (Exception ex)
                    {
                        // 中文註解：發生例外時回滾交易
                        transaction.Rollback();
                        Console.WriteLine($"刪除舊日誌記錄時發生錯誤（已回滾交易）: {ex.Message}");
                        // 根據您的錯誤處理策略，您可以選擇拋出異常或記錄
                        // throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查詢舊日誌記錄時發生錯誤: {ex.Message}");
                // 根據您的錯誤處理策略，您可以選擇拋出異常或記錄
                // throw;
            }
        }


        #endregion
    }
}