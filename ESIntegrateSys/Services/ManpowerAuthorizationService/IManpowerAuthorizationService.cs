using System.Collections.Generic;
using ESIntegrateSys.Models_ManpowerAllocation;

namespace ESIntegrateSys.Services
{
    /// <summary>
    /// 人力配置授權服務介面
    /// </summary>
    public interface IManpowerAuthorizationService
    {
        #region 報工生產單位 CRUD
        /// <summary>
        /// 取得所有報工生產單位。
        /// </summary>
        /// <returns>回傳包含所有報工生產單位的清單。</returns>
        List<ES_ReportingUnitDto> 取得所有報工生產單位();

        /// <summary>
        /// 取得指定的報工生產單位。
        /// </summary>
        /// <param name="單位ID">欲查詢的單位識別碼。</param>
        /// <returns>回傳對應的 <see cref="ES_ReportingUnitDto"/>，若不存在則回傳 null。</returns>
        ES_ReportingUnitDto 取得報工生產單位(int 單位ID);

        /// <summary>
        /// 新增一個報工生產單位。
        /// </summary>
        /// <param name="單位Dto">要新增的單位資料傳輸物件。</param>
        void 新增報工生產單位(ES_ReportingUnitDto 單位Dto);

        /// <summary>
        /// 更新指定的報工生產單位。
        /// </summary>
        /// <param name="單位Dto">包含更新資料的單位資料傳輸物件。</param>
        void 更新報工生產單位(ES_ReportingUnitDto 單位Dto);

        /// <summary>
        /// 刪除指定的報工生產單位。
        /// </summary>
        /// <param name="單位ID">欲刪除的單位識別碼。</param>
        void 刪除報工生產單位(int 單位ID);
        #endregion

        #region 使用者 CRUD
        /// <summary>
        /// 取得所有使用者清單。
        /// </summary>
        /// <returns>回傳包含所有使用者的清單。</returns>
        List<MP_UserDto> 取得所有使用者();

        /// <summary>
        /// 取得指定使用者的資料。
        /// </summary>
        /// <param name="使用者ID">欲查詢的使用者識別碼。</param>
        /// <returns>回傳對應的 <see cref="MP_UserDto"/>，若不存在則回傳 null。</returns>
        MP_UserDto 取得使用者(string 使用者ID);

        /// <summary>
        /// 新增一位使用者。
        /// </summary>
        /// <param name="使用者Dto">要新增的使用者資料傳輸物件。</param>
        void 新增使用者(MP_UserDto 使用者Dto);

        /// <summary>
        /// 更新指定的使用者資料。
        /// </summary>
        /// <param name="使用者Dto">包含更新內容的使用者資料傳輸物件。</param>
        void 更新使用者(MP_UserDto 使用者Dto);

        /// <summary>
        /// 刪除指定的使用者。
        /// </summary>
        /// <param name="使用者ID">欲刪除的使用者識別碼。</param>
        void 刪除使用者(string 使用者ID);
        #endregion

        #region 權限 CRUD
        /// <summary>
        /// 取得所有權限資料。
        /// </summary>
        /// <returns>回傳包含所有權限的清單。</returns>
        List<MP_ProductionUnitPermissionDto> 取得所有權限();

        /// <summary>
        /// 取得指定權限的資料。
        /// </summary>
        /// <param name="權限ID">欲查詢的權限識別碼。</param>
        /// <returns>回傳對應的 <see cref="MP_ProductionUnitPermissionDto"/>，若不存在則回傳 null。</returns>
        MP_ProductionUnitPermissionDto 取得權限(int 權限ID);

        /// <summary>
        /// 新增一個權限項目。
        /// </summary>
        /// <param name="權限Dto">要新增的權限資料傳輸物件。</param>
        void 新增權限(MP_ProductionUnitPermissionDto 權限Dto);

        /// <summary>
        /// 更新指定的權限資料。
        /// </summary>
        /// <param name="權限Dto">包含更新內容的權限資料傳輸物件。</param>
        void 更新權限(MP_ProductionUnitPermissionDto 權限Dto);

        /// <summary>
        /// 刪除指定的權限項目。
        /// </summary>
        /// <param name="權限ID">欲刪除的權限識別碼。</param>
        void 刪除權限(int 權限ID);
        #endregion

        #region 權限查詢
        /// <summary>
        /// 根據使用者ID取得其可編輯的報工生產單位清單
        /// </summary>
        List<ES_ReportingUnitDto> 取得使用者可編輯單位(string 使用者ID);
        #endregion
    }
}
