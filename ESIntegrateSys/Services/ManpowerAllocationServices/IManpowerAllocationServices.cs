using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using ESIntegrateSys.Models;
using ESIntegrateSys.Models_ManpowerAllocation;

namespace ESIntegrateSys.Services.ManpowerAllocationServices
{
    /// <summary>
    /// 人力配置服務介面
    /// </summary>
    public interface IManpowerAllocationServices
    {
        #region 下拉
        /// <summary>
        /// 獲取報工生產單位列表
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="userDeptNo">部門編號</param>
        /// <returns></returns>
        Task<List<報工生產單位下拉選單Dto>> Get報工生產單位List(string userId, string userDeptNo);
        /// <summary>
        /// 獲取生產單位列表
        /// </summary>
        /// <returns>生產單位列表</returns>
        Task<List<生產單位下拉選單Dto>> Get生產單位下拉選單Async(string userId, string userDeptNo);

        /// <summary>
        /// 獲取班別列表By新增修改
        /// </summary>
        /// <returns></returns>
        Task<List<班別下拉選單Dto>> Get班別下拉選單By新增修改Async(string userId = null, string userDeptNo = null);

        /// <summary>
        /// 獲取班別列表
        /// </summary>
        /// <returns>班別列表</returns>
        Task<List<班別下拉選單首頁Dto>> Get查詢班別下拉選單Async();

        /// <summary>
        /// 獲取出勤類型列表
        /// </summary>
        /// <returns>出勤類型列表</returns>
        Task<List<出勤類型下拉選單Dto>> Get出勤類型下拉選單Async();
        #endregion

        #region 查詢-統計 / 明細
        /// <summary>
        /// 獲取人力配置統計資料
        /// </summary>
        /// <param name="查詢條件">查詢條件</param>
        /// <returns>人力配置統計資料</returns>
        Task<IEnumerable<人力配置統計>> GetManpowerAllocationStatistics(人力配置明細 查詢條件);

        /// <summary>
        /// 獲取人員明細資料
        /// </summary>
        /// <param name="查詢條件">查詢條件</param>            
        /// <returns>人員明細資料</returns>
        Task<IEnumerable<人力配置明細>> GetEmployeeDetails(人力配置明細 查詢條件);
        #endregion

        /// <summary>
        /// 根據ID獲取單筆人員明細資料
        /// </summary>
        /// <param name="id">人員明細ID</param>
        /// <returns>人員明細資料</returns>
        Task<人力配置明細異動Dto> GetEmployeeDetailById(string id);

        /// <summary>
        /// 更新人員明細資料
        /// </summary>
        /// <param name="model">人員明細資料</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateEmployeeDetail(人力配置明細異動Dto model);

        /// <summary>
        /// 刪除人員明細資料
        /// </summary>
        /// <param name="id">人員明細ID</param>
        /// <param name="userId">用戶ID</param>
        /// <param name="UDeptNo">部門編號</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteEmployeeDetail(string id, string userId, string UDeptNo);

        /// <summary>
        /// 新增人員明細資料
        /// </summary>
        /// <param name="model">人員明細資料</param>
        /// <returns>是否成功</returns>
        Task<bool> AddEmployeeDetail(人力配置明細異動Dto model);

        /// <summary>
        /// 更新人員明細資料的鎖定狀態
        /// </summary>
        /// <param name="id">人員明細ID</param>
        /// <param name="editorId">編輯者ID</param>
        /// <param name="lockTime">鎖定時間</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateEmployeeDetailLockStatus(string id, string editorId, DateTime? lockTime);

        /// <summary>
        /// 獲取授權者名單
        /// </summary>
        /// <returns></returns>
        List<string> Get授權者UserIds();
    }
}
