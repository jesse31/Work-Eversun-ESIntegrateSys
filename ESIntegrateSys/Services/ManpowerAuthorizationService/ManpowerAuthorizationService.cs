using System;
using System.Collections.Generic;
using System.Linq;
using ESIntegrateSys.Models_ManpowerAllocation;
using ESIntegrateSys.Models;

namespace ESIntegrateSys.Services
{
    /// <summary>
    /// 人力配置授權服務（使用 Entity Framework 的 ESIntegrateSysEntities 進行資料存取）
    /// </summary>
    public class ManpowerAuthorizationService : IManpowerAuthorizationService
    {
        #region 報工生產單位 CRUD
        /// <summary>
        /// 取得所有報工生產單位。
        /// </summary>
        /// <returns>回傳包含所有報工生產單位的清單（<see cref="ES_ReportingUnitDto"/>）。</returns>
        public List<ES_ReportingUnitDto> 取得所有報工生產單位()
        {
            using (var db = new ESIntegrateSysEntities())
            {
                return db.ES_ReportingUnit.Select(x => new ES_ReportingUnitDto
                {
                    單位ID = x.ReportingUnitID,
                    單位名稱 = x.ReportingUnitName,                    
                    啟用 = x.IsActive
                }).ToList();
            }
        }

        /// <summary>
        /// 取得單一報工生產單位。
        /// </summary>
        /// <param name="單位ID">欲查詢的單位識別碼。</param>
        /// <returns>回傳對應的 <see cref="ES_ReportingUnitDto"/>，若找不到則回傳 null。</returns>
        public ES_ReportingUnitDto 取得報工生產單位(int 單位ID)
        {
            using (var db = new ESIntegrateSysEntities())
            {
                var x = db.ES_ReportingUnit.FirstOrDefault(y => y.ReportingUnitID == 單位ID);
                if (x == null) return null;
                return new ES_ReportingUnitDto
                {
                    單位ID = x.ReportingUnitID,
                    單位名稱 = x.ReportingUnitName,
                    啟用 = x.IsActive
                };
            }
        }

        /// <summary>
        /// 新增報工生產單位。
        /// </summary>
        /// <param name="dto">包含要新增之單位資料的 <see cref="ES_ReportingUnitDto"/>。</param>
        /// <remarks>會將資料寫入資料庫並呼叫 SaveChanges。</remarks>
        public void 新增報工生產單位(ES_ReportingUnitDto dto)
        {
            using (var db = new ESIntegrateSysEntities())
            {
                var entity = new ES_ReportingUnit
                {
                    ReportingUnitName = dto.單位名稱,                    
                    IsActive = dto.啟用
                };
                db.ES_ReportingUnit.Add(entity);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 更新報工生產單位。
        /// </summary>
        /// <param name="dto">包含要更新之單位資料的 <see cref="ES_ReportingUnitDto"/>（須含有效的 單位ID）。</param>
        /// <exception cref="System.Exception">當找不到指定單位時會拋出例外。</exception>
        public void 更新報工生產單位(ES_ReportingUnitDto dto)
        {
            using (var db = new ESIntegrateSysEntities())
            {
                var entity = db.ES_ReportingUnit.FirstOrDefault(x => x.ReportingUnitID == dto.單位ID);
                if (entity == null) throw new Exception("找不到指定單位");
                entity.ReportingUnitName = dto.單位名稱;                
                entity.IsActive = dto.啟用;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 刪除報工生產單位。
        /// </summary>
        /// <param name="單位ID">欲刪除的單位識別碼。</param>
        /// <exception cref="System.Exception">當找不到指定單位時會拋出例外。</exception>
        public void 刪除報工生產單位(int 單位ID)
        {
            using (var db = new ESIntegrateSysEntities())
            {
                var entity = db.ES_ReportingUnit.FirstOrDefault(x => x.ReportingUnitID == 單位ID);
                if (entity == null) throw new Exception("找不到指定單位");
                db.ES_ReportingUnit.Remove(entity);
                db.SaveChanges();
            }
        }
        #endregion

        #region 使用者 CRUD
        /// <summary>
        /// 取得所有使用者。
        /// </summary>
        /// <returns>回傳包含所有使用者的清單（<see cref="MP_UserDto"/>）。</returns>
        public List<MP_UserDto> 取得所有使用者()
        {
            using (var db = new ESIntegrateSysEntities())
            {
                //return db.MP_Users.Select(x => new MP_UserDto
                //{
                //    使用者ID = x.UserID,
                //    姓名 = x.UserName,
                //    啟用 = x.IsActive
                //}).ToList();
                return null;
            }
        }

        /// <summary>
        /// 取得單一使用者。
        /// </summary>
        /// <param name="使用者ID">欲查詢的使用者識別碼。</param>
        /// <returns>回傳對應的 <see cref="MP_UserDto"/>，若找不到則回傳 null。</returns>
        public MP_UserDto 取得使用者(string 使用者ID)
        {
            using (var db = new ESIntegrateSysEntities())
            {
                //var x = db.MP_Users.FirstOrDefault(y => y.UserID == 使用者ID);
                //if (x == null) return null;
                //return new MP_UserDto
                //{
                //    使用者ID = x.UserID,
                //    姓名 = x.UserName,
                //    啟用 = x.IsActive
                //};
                return null;
            }
        }

        /// <summary>
        /// 新增使用者。
        /// </summary>
        /// <param name="dto">包含要新增之使用者資料的 <see cref="MP_UserDto"/>。</param>
        /// <remarks>會將資料新增至資料庫並呼叫 SaveChanges。</remarks>
        public void 新增使用者(MP_UserDto dto)
        {
            using (var db = new ESIntegrateSysEntities())
            {
                //var entity = new MP_Users
                //{
                //    UserID = dto.使用者ID,
                //    UserName = dto.姓名,
                //    IsActive = dto.啟用
                //};
                //db.MP_Users.Add(entity);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 更新使用者。
        /// </summary>
        /// <param name="dto">包含更新內容的 <see cref="MP_UserDto"/>（須含有效的 使用者ID）。</param>
        /// <exception cref="System.Exception">當找不到指定使用者時會拋出例外。</exception>
        public void 更新使用者(MP_UserDto dto)
        {
            using (var db = new ESIntegrateSysEntities())
            {
                //var entity = db.MP_Users.FirstOrDefault(x => x.UserID == dto.使用者ID);
                //if (entity == null) throw new Exception("找不到指定使用者");
                //entity.UserName = dto.姓名;
                //entity.IsActive = dto.啟用;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 刪除使用者。
        /// </summary>
        /// <param name="使用者ID">欲刪除的使用者識別碼。</param>
        /// <exception cref="System.Exception">當找不到指定使用者時會拋出例外。</exception>
        public void 刪除使用者(string 使用者ID)
        {
            using (var db = new ESIntegrateSysEntities())
            {
                //var entity = db.MP_Users.FirstOrDefault(x => x.UserID == 使用者ID);
                //if (entity == null) throw new Exception("找不到指定使用者");
                //db.MP_Users.Remove(entity);
                //db.SaveChanges();
            }
        }
        #endregion

        #region 權限 CRUD
        /// <summary>
        /// 取得所有權限（包含關聯資料）。
        /// </summary>
        /// <returns>回傳包含所有權限項目的清單（<see cref="MP_ProductionUnitPermissionDto"/>）。</returns>
        public List<MP_ProductionUnitPermissionDto> 取得所有權限()
        {
            using (var db = new ESIntegrateSysEntities())
            {
                // 使用JOIN操作獲取關聯資料
                //var query = from permission in db.MP_ProductionUnitPermissions
                //           join unit in db.MP_WorkReportProductionUnits on permission.UnitID equals unit.UnitID
                //           join user in db.MP_Users on permission.UserID equals user.UserID into userJoin
                //           from user in userJoin.DefaultIfEmpty()
                //           select new MP_ProductionUnitPermissionDto
                //           {
                //               權限ID = permission.PermissionID,
                //               單位ID = permission.UnitID,
                //               使用者ID = permission.UserID,
                //               授權日期 = (DateTime)permission.GrantedDate,
                //               到期日 = permission.ExpiresDate,
                //               備註 = permission.Notes,
                //               // 添加關聯資料
                //               UnitName = unit.UnitName,
                //               EmpId = permission.UserID,
                //               EmpName = user != null ? user.UserName : string.Empty
                //           };

                //return query.ToList();
                return null;
            }
        }

        /// <summary>
        /// 取得單一權限。
        /// </summary>
        /// <param name="權限ID">欲查詢的權限識別碼。</param>
        /// <returns>回傳對應的 <see cref="MP_ProductionUnitPermissionDto"/>，若找不到則回傳 null。</returns>
        public MP_ProductionUnitPermissionDto 取得權限(int 權限ID)
        {
            using (var db = new ESIntegrateSysEntities())
            {
                //var x = db.MP_ProductionUnitPermissions.FirstOrDefault(y => y.PermissionID == 權限ID);
                //if (x == null) return null;
                //return new MP_ProductionUnitPermissionDto
                //{
                //    權限ID = x.PermissionID,
                //    單位ID = x.UnitID,
                //    使用者ID = x.UserID,
                //    授權日期 = (DateTime)x.GrantedDate,
                //    到期日 = x.ExpiresDate,
                //    備註 = x.Notes
                //};
                return null;
            }
        }

        /// <summary>
        /// 新增權限項目。
        /// </summary>
        /// <param name="dto">包含要新增之權限資料的 <see cref="MP_ProductionUnitPermissionDto"/>。</param>
        public void 新增權限(MP_ProductionUnitPermissionDto dto)
        {
            using (var db = new ESIntegrateSysEntities())
            {
                //var entity = new MP_ProductionUnitPermissions
                //{
                //    UnitID = dto.單位ID,
                //    UserID = dto.使用者ID,
                //    GrantedDate = dto?.授權日期 ?? DateTime.Now,
                //    ExpiresDate = dto.到期日,
                //    Notes = dto.備註
                //};
                //db.MP_ProductionUnitPermissions.Add(entity);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 更新權限資料。
        /// </summary>
        /// <param name="dto">包含更新內容的 <see cref="MP_ProductionUnitPermissionDto"/>（須含有效的 權限ID）。</param>
        /// <exception cref="System.Exception">當找不到指定權限時會拋出例外。</exception>
        public void 更新權限(MP_ProductionUnitPermissionDto dto)
        {
            using (var db = new ESIntegrateSysEntities())
            {
                //var entity = db.MP_ProductionUnitPermissions.FirstOrDefault(x => x.PermissionID == dto.權限ID);
                //if (entity == null) throw new Exception("找不到指定權限");
                //entity.UnitID = dto.單位ID;
                //entity.UserID = dto.使用者ID;
                //entity.GrantedDate = dto?.授權日期 ?? entity.GrantedDate;
                //entity.ExpiresDate = dto.到期日;
                //entity.Notes = dto.備註;
                //db.SaveChanges();
            }
        }

        /// <summary>
        /// 刪除權限項目。
        /// </summary>
        /// <param name="權限ID">欲刪除之權限識別碼。</param>
        /// <exception cref="System.Exception">當找不到指定權限時會拋出例外。</exception>
        public void 刪除權限(int 權限ID)
        {
            using (var db = new ESIntegrateSysEntities())
            {
                //var entity = db.MP_ProductionUnitPermissions.FirstOrDefault(x => x.PermissionID == 權限ID);
                //if (entity == null) throw new Exception("找不到指定權限");
                //db.MP_ProductionUnitPermissions.Remove(entity);
                //db.SaveChanges();
            }
        }

        /// <summary>
        /// 取得使用者可編輯的報工生產單位清單。
        /// </summary>
        /// <param name="使用者ID">欲查詢的使用者識別碼。</param>
        /// <returns>回傳該使用者可編輯之報工生產單位清單（<see cref="ES_ReportingUnitDto"/>）。</returns>
        public List<ES_ReportingUnitDto> 取得使用者可編輯單位(string 使用者ID)
        {
            using (var db = new ESIntegrateSysEntities())
            {
                //var 單位IDs = db.MP_ProductionUnitPermissions.Where(x => x.UserID == 使用者ID)
                //    .Select(x => x.UnitID).ToList();
                //return db.MP_WorkReportProductionUnits.Where(x => 單位IDs.Contains(x.UnitID)).Select(x => new ES_ReportingUnitDto
                //{
                //    單位ID = x.UnitID,
                //    單位名稱 = x.UnitName,
                //    描述 = x.Description,
                //    啟用 = x.IsActive
                //}).ToList();
                return null;
            }
        }
        #endregion
    }
}
