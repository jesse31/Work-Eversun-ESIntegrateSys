using System;
using System.Collections.Generic;

namespace ESIntegrateSys.Helpers
{
    /// <summary>
    /// 料槍相關的系統常數定義。
    /// </summary>
    public static class MaterialGunConstants
    {
        /// <summary>
        /// 料槍維修帳號白名單 - 允許編輯已完成維修記錄的帳號清單。
        /// 包含的帳號可以在維修完成後（Chk=1）重新編輯或複查維修紀錄。
        /// 未來將由完整的角色權限系統取代此硬編碼清單。
        /// </summary>
        public static readonly HashSet<string> RepairEditWhitelistUsers = new HashSet<string>
        {
            "02898"  // 檢查員或管理者帳號
        };

        /// <summary>
        /// 料槍維修部門白名單 - 允許編輯已完成維修記錄的部門代碼清單。
        /// 包含的部門中的所有員工可以在維修完成後（Chk=1）重新編輯或複查維修紀錄。
        /// 與帳號白名單使用 OR 邏輯組合。
        /// </summary>
        public static readonly HashSet<string> RepairEditWhitelistDepts = new HashSet<string>
        {
            "IT"  // IT 部門
        };
    }
}
