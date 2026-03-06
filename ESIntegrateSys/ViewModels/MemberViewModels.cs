using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ESIntegrateSys.Models;
namespace ESIntegrateSys.ViewModels
{
    /// <summary>
    /// 人員資料的檢視模型。
    /// </summary>
    public class MemberViewModels
    {
        /// <summary>
        /// 人員唯一識別碼。
        /// </summary>
        public int fId { get; set; }

        /// <summary>
        /// 使用者帳號。
        /// </summary>
        public string fUserId { get; set; }

        /// <summary>
        /// 使用者姓名。
        /// </summary>
        public string fName { get; set; }

        /// <summary>
        /// 角色描述。
        /// </summary>
        public string ROLE_DESC { get; set; }

        /// <summary>
        /// 角色代碼。
        /// </summary>
        public string ROLE_ID { get; set; }

        /// <summary>
        /// 使用者密碼。
        /// </summary>
        public string fPwd { get; set; }

        /// <summary>
        /// 使用者狀態。
        /// </summary>
        public string fStatus { get; set; }

        /// <summary>
        /// 部門代碼。
        /// </summary>
        public string UDeptNo { get; set; }

        /// <summary>
        /// 功能描述。
        /// </summary>
        public string Func { get; set; }
    }

    /// <summary>
    /// 人員名單建立使用的檢視模型。
    /// </summary>
    public class MemberCreateViewModels
    {
        /// <summary>
        /// 是否有錯誤訊息。
        /// </summary>
        public bool ErrMsg { get; set; }

        /// <summary>
        /// 使用者帳號。
        /// </summary>
        public string fUserId { get; set; }

        /// <summary>
        /// 使用者姓名。
        /// </summary>
        public string fName { get; set; }

        /// <summary>
        /// 角色描述。
        /// </summary>
        public string ROLE_DESC { get; set; }

        /// <summary>
        /// 角色代碼。
        /// </summary>
        public string ROLE_ID { get; set; }

        /// <summary>
        /// 使用者密碼。
        /// </summary>
        public string fPwd { get; set; }

        /// <summary>
        /// 部門代碼。
        /// </summary>
        public string Dept_No { get; set; }
    }


    /// <summary>
    /// 人員畫面相關的檢視模型。
    /// </summary>
    public class MemberFunctionViewModels
    {
        /// <summary>
        /// 畫面顯示文字。
        /// </summary>
        public string LayoutViewText { get; set; }

        /// <summary>
        /// 畫面索引。
        /// </summary>
        public string LayoutIndex { get; set; }

        /// <summary>
        /// 畫面名稱。
        /// </summary>
        public string LayoutViewName { get; set; }

        /// <summary>
        /// 動作名稱。
        /// </summary>
        public string ActionName { get; set; }
    }
}