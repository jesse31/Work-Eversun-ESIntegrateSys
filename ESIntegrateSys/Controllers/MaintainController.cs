using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ESIntegrateSys.Models;
using ESIntegrateSys.ViewModels;
using PagedList;

namespace ESIntegrateSys.Controllers
{
    /// <summary>
    /// 負責維護人員相關功能的控制器
    /// </summary>
    public class MaintainController : Controller
    {
        /// <summary>
        /// 資料庫操作物件
        /// </summary>
        ESIntegrateSysEntities db = new ESIntegrateSysEntities();

        /// <summary>
        /// 分頁每頁顯示筆數
        /// </summary>
        int pageSize = 50;

        /// <summary>
        /// 維護首頁
        /// </summary>
        /// <returns>首頁視圖</returns>
        public ActionResult Index()
        {
            // 回傳首頁視圖
            return View();
        }

        /// <summary>
        /// 會員維護頁面，僅限 SA/DA 角色
        /// </summary>
        /// <param name="page">分頁頁碼</param>
        /// <returns>會員維護視圖</returns>
        public ActionResult MemberMaintain(int page = 1)
        {
            // 檢查是否已登入
            if (!Login_Authentication())
            {
                // 未登入則導向登入頁面
                return RedirectToAction("Login", "Home");
            }

            // 設定目前頁碼，若小於1則設為1
            int currentPage = page < 1 ? 1 : page;
            // 建立會員維護物件
            MemberMaintain MM = new MemberMaintain(db);

            // 加入防呆：安全取得部門編號
            var member = Session["Member"] as MemberViewModels;
            MM.dEpt = member?.UDeptNo ?? string.Empty;

            // 根據部門名稱獲取對應的 LayoutEnum
            LayoutEnum layout = LayoutMapping.GetLayoutForDept(MM.dEpt);

            // 檢查角色是否為 SA 或 DA
            if (ViewBag.RoleId != "SA" && ViewBag.RoleId != "DA")
            {
                // 非 SA/DA 則導向首頁
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // 回傳會員維護視圖，並帶入分頁資料
                return View("MemberMaintain", layout.ToString(), MM.ESI_Member().ToPagedList(currentPage, pageSize));
            }
        }

        /// <summary>
        /// 管理者權限-進入編輯會員頁面
        /// </summary>
        /// <param name="fId">會員編號</param>
        /// <returns>編輯會員視圖</returns>
        public ActionResult Edit_Member(int fId)
        {
            // 檢查是否已登入
            if (!Login_Authentication())
            {
                // 未登入則導向登入頁面
                return RedirectToAction("Login", "Home");
            }
            // 加入防呆：安全取得部門編號
            var member = Session["Member"] as MemberViewModels;
            string dept = member?.UDeptNo ?? string.Empty;

            // 根據部門名稱獲取對應的 LayoutEnum
            LayoutEnum layout = LayoutMapping.GetLayoutForDept(dept);
            // 建立會員維護物件
            MemberMaintain editMember = new MemberMaintain(db);
            // 取得會員資料
            var memberData = editMember.Edit(fId);

            // 回傳編輯會員視圖
            return View("Edit_Member", layout.ToString(), memberData);
        }

        /// <summary>
        /// 編輯會員資料（POST）
        /// </summary>
        /// <param name="fUserId">會員帳號</param>
        /// <param name="fName">會員姓名</param>
        /// <param name="ROLE_ID">角色代號</param>
        /// <param name="fItem">功能清單</param>
        /// <returns>導向會員維護頁面</returns>
        [HttpPost]
        public ActionResult Edit_Member(string fUserId, string fName, string ROLE_ID, List<string> fItem)
        {
            // 檢查是否已登入
            if (!Login_Authentication())
            {
                // 未登入則導向登入頁面
                return RedirectToAction("Login", "Home");
            }
            // 建立會員維護物件
            MemberMaintain editMember = new MemberMaintain(db);

            // 編輯會員資料
            editMember.Edit(fUserId, fName, ROLE_ID, fItem);

            // 編輯完成後導向會員維護頁面
            return RedirectToAction("MemberMaintain");
        }

        /// <summary>
        /// 停用會員
        /// </summary>
        /// <param name="fId">會員編號</param>
        /// <returns>導向會員維護頁面</returns>
        public ActionResult Del_Member(int fId)
        {
            // 建立會員維護物件
            MemberMaintain delMember = new MemberMaintain(db);

            // 停用會員
            delMember.Del(fId);

            // 停用後導向會員維護頁面
            return RedirectToAction("MemberMaintain");
        }

        /// <summary>
        /// 進入變更密碼頁面
        /// </summary>
        /// <returns>變更密碼視圖</returns>
        public ActionResult Edit_Password()
        {
            // 檢查是否已登入
            if (!Login_Authentication())
            {
                // 未登入則導向登入頁面
                return RedirectToAction("Login", "Home");
            }
            // 建立會員維護物件
            MemberMaintain editMember = new MemberMaintain(db);

            // 取得會員編號
            int fId = (Session["Member"] as MemberViewModels).fId;

            // 回傳變更密碼視圖
            return View(editMember.Password(fId));
        }

        /// <summary>
        /// 變更密碼（POST）
        /// </summary>
        /// <param name="fId">會員編號</param>
        /// <param name="fPwd">舊密碼</param>
        /// <param name="fNPwd">新密碼</param>
        /// <returns>變更密碼結果視圖</returns>
        [HttpPost]
        public ActionResult Edit_Password(int fId, string fPwd, string fNPwd)
        {
            // 檢查是否已登入
            if (!Login_Authentication())
            {
                // 未登入則導向登入頁面
                return RedirectToAction("Login", "Home");
            }
            // 建立會員維護物件
            MemberMaintain editMember = new MemberMaintain(db);

            // 嘗試變更密碼
            if (editMember.Password(fId, fPwd, fNPwd))
            {
                // 密碼變更成功
                ViewBag.msg = "密碼變更完成!!";
                return View("Edit_Password", editMember.Password(fId));
            }
            else
            {
                // 密碼變更失敗
                ViewBag.msg = "新舊密碼輸入錯誤，請再次確認!!";
                return View("Edit_Password", editMember.Password(fId));
            }
        }

        /// <summary>
        /// 進入新增會員頁面
        /// </summary>
        /// <returns>新增會員視圖</returns>
        public ActionResult CreateMember()
        {
            // 檢查是否已登入
            if (!Login_Authentication())
            {
                // 未登入則導向登入頁面
                return RedirectToAction("Login", "Home");
            }
            // 加入防呆：安全取得部門編號
            var member = Session["Member"] as MemberViewModels;
            string dept = member?.UDeptNo ?? string.Empty;

            // 根據部門名稱獲取對應的 LayoutEnum
            LayoutEnum layout = LayoutMapping.GetLayoutForDept(dept);
            // 回傳新增會員視圖
            return View("CreateMember", layout.ToString());
        }

        /// <summary>
        /// 新增會員（POST）
        /// </summary>
        /// <param name="fUserId">會員帳號</param>
        /// <param name="fName">會員姓名</param>
        /// <param name="Level">角色等級</param>
        /// <param name="email">電子郵件</param>
        /// <param name="fItem">功能清單</param>
        /// <returns>導向會員維護頁面或顯示錯誤訊息</returns>
        [HttpPost]
        public ActionResult CreateMember(string fUserId, string fName, string Level, string email, List<string> fItem)
        {
            // 檢查是否已登入
            if (!Login_Authentication())
            {
                // 未登入則導向登入頁面
                return RedirectToAction("Login", "Home");
            }
            // 建立會員維護物件
            MemberMaintain create = new MemberMaintain(db);
            // 加入防呆：安全取得部門編號
            var member = Session["Member"] as MemberViewModels;
            string dept = member?.UDeptNo ?? string.Empty;

            // 根據部門名稱獲取對應的 LayoutEnum
            LayoutEnum layout = LayoutMapping.GetLayoutForDept(dept);
            // 檢查使用者代號是否已存在
            if (!create.Create(fUserId, fName, Level, email, fItem).ErrMsg)
            {
                // 使用者代號已存在，顯示錯誤訊息
                ViewBag.msg = "該使用者代號已存在，請輸入不同的代號";
                return View("CreateMember", layout.ToString());
            }
            // 新增成功導向會員維護頁面
            return RedirectToAction("MemberMaintain");
        }

        /// <summary>
        /// 取得會員角色下拉選單資料
        /// </summary>
        /// <returns>角色清單 JSON</returns>
        [HttpGet]
        public ActionResult MemberLeveldrop()
        {
            // 加入防呆：安全取得部門編號
            var member = Session["Member"] as MemberViewModels;
            string dept = member?.UDeptNo ?? string.Empty;

            // 取得角色清單
            var descriptions = db.ES_RoleClassification
                .Where(m => m.ROLE_ID != "SA" &&
                (dept == "IT" ||    // 如果是 IT 部門顯示所有資料
                 (dept == "QE") ||  // 如果是 MG 部門顯示包含 FR
                 (dept == "IE" && m.ROLE_ID != "FR"))) // 其他部門不顯示 FR
                .Select(a => new { Value = a.ROLE_ID.Trim(), Text = a.ROLE_NAME }).ToList();
            // 插入預設選項
            descriptions.Insert(0, new { Value = "", Text = "請選擇..." });
            // 回傳 JSON 資料
            return Json(descriptions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得會員功能下拉選單資料
        /// </summary>
        /// <returns>功能清單 JSON</returns>
        [HttpGet]
        public ActionResult MemberFuncdrop()
        {
            // 加入防呆：安全取得部門編號
            var member = Session["Member"] as MemberViewModels;
            string dept = member?.UDeptNo ?? string.Empty;

            // 取得功能清單
            var descriptions = db.ES_FunctionItem
                .Where(m => m.FunctionNo != "" &&
                (dept == "IT" ||                           // 如果是 IT 部門顯示所有資料
                 (dept == "QE" && m.FunctionNo == "MG") || // 如果是 MG 部門顯示包含 FR
                 (dept == "IE" && m.FunctionNo != "MG"))) // 其他部門不顯示 FR
                .Select(a => new { Value = a.FunctionNo, Text = a.FunctionName }).ToList();

            // 回傳 JSON 資料
            return Json(descriptions, JsonRequestBehavior.AllowGet);
        }

        #region Login 驗證相關Class
        /// <summary>
        /// 驗證使用者是否已登入
        /// </summary>
        /// <returns>登入狀態</returns>
        public bool Login_Authentication()
        {
            // 檢查 Session 是否有會員資料
            if (Session["Member"] != null)
            {
                // 取得使用者帳號
                string UserId = (Session["Member"] as MemberViewModels).fUserId;
                // 取得角色代號
                string RoleId = (Session["Member"] as MemberViewModels).ROLE_ID;

                // 設定 ViewBag 角色及帳號
                ViewBag.RoleId = RoleId;
                ViewBag.UserId = UserId;
                // 已登入回傳 true
                return true;
            }
            else
            {
                // 未登入回傳 false
                return false;
            }
        }
        #endregion
    }
}