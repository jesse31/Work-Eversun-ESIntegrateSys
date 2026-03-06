using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESIntegrateSys.Services.Dtos
{

    /// <summary>
    /// 發送郵件結果
    /// </summary>
    public class EmailSendResultDto
    {
        /// <summary>是否成功</summary>
        public bool IsSuccess { get; set; }
        /// <summary>錯誤訊息</summary>
        public string ErrorMessage { get; set; }
    }
}