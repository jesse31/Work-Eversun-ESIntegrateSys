using System;

namespace ESIntegrateSys.Models_ManpowerAllocation
{
    /// <summary>
    /// 使用者資料傳輸物件
    /// </summary>
    public class MP_UserDto
    {
        /// <summary>
        /// 使用者工號（唯一識別）
        /// </summary>
        public string 使用者ID { get; set; }

        /// <summary>
        /// 使用者姓名
        /// </summary>
        public string 姓名 { get; set; }

        /// <summary>
        /// 電子郵件
        /// </summary>
        public string 電子郵件 { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool 啟用 { get; set; }
    }
}
