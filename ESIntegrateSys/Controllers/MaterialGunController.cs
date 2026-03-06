using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ESIntegrateSys.Models;
using ESIntegrateSys.Models_MGun;
using ESIntegrateSys.ViewModels;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using PagedList;

namespace ESIntegrateSys.Controllers
{
    /// <summary>
    /// 料槍管理相關功能的控制器。
    /// </summary>
    public class MaterialGunController : Controller
    {
        /// <summary>
        /// 頁面預設分頁大小。
        /// </summary>
        int pagesize = 10;

        /// <summary>
        /// 全域鎖定物件 (執行緒安全)。
        /// </summary>
        private static readonly object _maintainLock = new object();

        /// <summary>
        /// 最近提交記錄 (料槍號_使用者ID → 提交時間)。
        /// </summary>
        private static readonly Dictionary<string, DateTime> _recentSubmissions =
            new Dictionary<string, DateTime>();

        /// <summary>
        /// 資料庫操作物件。
        /// </summary>
        ESIntegrateSysEntities db = new ESIntegrateSysEntities();

        /// <summary>
        /// 料槍主畫面。
        /// </summary>
        /// <returns>主畫面 View</returns>
        public ActionResult Index()
        {
            // 檢查使用者是否已登入
            if (!Login_Authentication())
            {
                // 若未登入則導向登入頁面
                return RedirectToAction("Login", "Home");
            }
            else
            {
                // 若已登入則回傳主畫面 View
                return View("Index", "_MaterialLayout", "MaterialGun");
            }
        }

        /// <summary>
        /// 料槍保養資料查詢畫面。
        /// </summary>
        /// <param name="page">分頁頁碼</param>
        /// <param name="itemgun">指定顯示的料槍編號 (新增後導向用)</param>
        /// <param name="type">操作類型 (如 "new" 表示剛新增)</param>
        /// <returns>料槍保養 View</returns>
        public ActionResult MaterialGunMaintain(int page = 1, string itemgun = null, string type = null)
        {
            // 檢查使用者是否已登入
            if (!Login_Authentication())
            {
                // 若未登入則導向登入頁面
                return RedirectToAction("Login", "Home");
            }
            // 設定目前頁碼，若小於 1 則設為 1
            int currentPage = page < 1 ? 1 : page;
            // 建立 MaintainGun 物件，並傳入資料庫操作物件
            MaintainGun mGun = new MaintainGun(db);
            // 取得料槍保養資料清單
            List<MaintainGun.MGunDataList> gunDataLists = mGun.MaintainGunData();

            // 若有指定 itemgun，則進行篩選
            if (!string.IsNullOrEmpty(itemgun))
            {
                gunDataLists = gunDataLists.Where(x => x.MaterialGun_Sno == itemgun).ToList();
                ViewBag.FilteredItemGun = itemgun;

                // 若操作類型為 new，代表剛新增完畢，標記顯示成功提示
                if (type == "new")
                {
                    ViewBag.ShowSuccessToast = true;
                }
            }

            // 回傳料槍保養資料分頁後的 View
            return View("MaterialGunMaintain", "_MaterialLayout", gunDataLists.ToPagedList(currentPage, pagesize));
        }

        /// <summary>
        /// 料槍保養開單畫面。
        /// </summary>
        /// <returns>保養開單 View</returns>
        public ActionResult CreateMaintainWork()
        {
            return View("CreateMaintainWork", "_MaterialLayout");
        }

        /// <summary>
        /// 料槍保養開單資料送出。
        /// </summary>
        /// <param name="isTrue">是否確認</param>
        /// <param name="itemgun">料槍編號</param>
        /// <returns>導向保養查詢頁</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMaintainWork(bool isTrue, string itemgun)
        {
            // 強制轉大寫並去除空白，確保資料一致性
            itemgun = NormalizeMaterialGunSno(itemgun);

            // 取得目前使用者的ID
            string uId = (Session["Member"] as MemberViewModels).fUserId;
            string lockKey = $"{itemgun}_{uId}";

            // ⚡ 效能關鍵:使用 lock 確保執行緒安全,同時進行時間窗口檢查
            lock (_maintainLock)
            {
                // 檢查 5 秒內是否有相同提交 (Dictionary 查詢,O(1) 時間複雜度)
                if (_recentSubmissions.TryGetValue(lockKey, out DateTime lastSubmitTime))
                {
                    var timeDiff = (DateTime.Now - lastSubmitTime).TotalSeconds;
                    if (timeDiff < 5)
                    {
                        TempData["message"] = $"請勿在 5 秒內重複提交! (距離上次提交僅 {timeDiff:F1} 秒)";
                        return RedirectToAction("CreateMaintainWork");
                    }
                }

                // 記錄本次提交時間
                _recentSubmissions[lockKey] = DateTime.Now;

                // ⚡ 效能優化:每 100 次提交才清理一次過期記錄
                if (_recentSubmissions.Count % 100 == 0)
                {
                    CleanupExpiredRecords();
                }
            }

            // 檢查該料槍本月是否已保養 (防止同月重複 Key 入)
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            bool isDuplicate = db.ES_MaintainWork.Any(x => x.MaterialGun_Sno == itemgun &&
                                                          x.MaintainTime != null &&
                                                          x.MaintainTime.Value.Year == currentYear &&
                                                          x.MaintainTime.Value.Month == currentMonth);
            if (isDuplicate)
            {
                TempData["message"] = $"料槍 {itemgun} 本月 ({currentYear}/{currentMonth}) 已完成保養，請勿重複輸入！";
                return RedirectToAction("CreateMaintainWork");
            }

            // 建立料槍保養物件，傳入資料庫操作物件 (在 lock 外執行,減少鎖定時間)
            MaintainGun maintainGun = new MaintainGun(db);
            // 執行料槍保養開單作業
            bool isSaved = maintainGun.MaintainWork(itemgun, uId);

            if (!isSaved)
            {
                TempData["message"] = $"新增保養失敗！請確認料槍編號 [{itemgun}] 是否正確，或該料槍是否已報廢。";
                return RedirectToAction("CreateMaintainWork");
            }

            // 保養完成後導向料槍保養查詢頁面，並帶入參數以觸發篩選與提示
            return RedirectToAction("MaterialGunMaintain", new { itemgun = itemgun, type = "new" });
        }

        /// <summary>
        /// 檢查料槍本月是否已保養 (AJAX)。
        /// </summary>
        /// <param name="itemgun">料槍編號</param>
        /// <returns>JSON 結果 (isDuplicate: 是否重複, message: 訊息)</returns>
        [HttpPost]
        public JsonResult CheckMaintainStatus(string itemgun)
        {
            if (string.IsNullOrWhiteSpace(itemgun))
            {
                return Json(new { isDuplicate = false });
            }

            // 強制轉大寫並去除空白，確保資料一致性
            itemgun = NormalizeMaterialGunSno(itemgun);


            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            // 檢查本月是否已有保養紀錄
            bool isDuplicate = db.ES_MaintainWork.Any(x => x.MaterialGun_Sno == itemgun &&
                                                          x.MaintainTime != null &&
                                                          x.MaintainTime.Value.Year == currentYear &&
                                                          x.MaintainTime.Value.Month == currentMonth);

            if (isDuplicate)
            {
                return Json(new {
                    isDuplicate = true,
                    message = $"料槍 {itemgun} 本月 ({currentYear}/{currentMonth}) 已完成保養，請勿重複輸入！"
                });
            }

            return Json(new { isDuplicate = false });
        }

        /// <summary>
        /// 清理 1 分鐘前的過期記錄 (避免記憶體洩漏)。
        /// </summary>
        private static void CleanupExpiredRecords()
        {
            var oneMinuteAgo = DateTime.Now.AddMinutes(-1);
            var expiredKeys = _recentSubmissions
                .Where(x => x.Value < oneMinuteAgo)
                .Select(x => x.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _recentSubmissions.Remove(key);
            }
        }

        /// <summary>
        /// 維修主畫面。
        /// </summary>
        /// <param name="page">分頁頁碼</param>
        /// <returns>維修主畫面 View</returns>
        public ActionResult MaterialGunRepairView(int page = 1)
        {
            // 檢查使用者是否已登入
            if (!Login_Authentication())
            {
                // 若未登入則導向登入頁面
                return RedirectToAction("Login", "Home");
            }
            // 設定目前頁碼，若小於 1 則設為 1
            int currentPage = page < 1 ? 1 : page;
            // 建立 RepairGun 物件，並傳入資料庫操作物件
            RepairGun rGun = new RepairGun(db);
            // 取得料槍維修資料清單
            List<RGunDataListDto> gunDataLists = rGun.RepairGunData();
            // 回傳料槍維修資料分頁後的 View
            return View("MaterialGunRepairView", "_MaterialLayout", gunDataLists.ToPagedList(currentPage, pagesize));
        }

        /// <summary>
        /// 送修開單畫面。
        /// </summary>
        /// <returns>送修開單 View</returns>
        public ActionResult MaterialGunForRepair()
        {
            // 檢查使用者是否已登入
            if (!Login_Authentication())
            {
                // 若未登入則導向登入頁面
                return RedirectToAction("Login", "Home");
            }
            // 回傳送修開單畫面
            return View("MaterialGunForRepair", "_MaterialLayout");
        }

        /// <summary>
        /// 送修開單資料送出。
        /// </summary>
        /// <param name="uId">使用者 ID</param>
        /// <param name="MaterialGun_Sno">料槍編號</param>
        /// <param name="BadDescription">不良描述代碼</param>
        /// <param name="Mark">備註</param>
        /// <returns>導向維修主畫面或顯示錯誤訊息</returns>
        [HttpPost]
        public ActionResult MaterialGunForRepair(string uId, string MaterialGun_Sno, int BadDescription, string Mark)
        {
            // 強制轉大寫並去除空白，確保資料一致性
            MaterialGun_Sno = NormalizeMaterialGunSno(MaterialGun_Sno);

            // 取得目前使用者的ID
            string Id = (Session["Member"] as MemberViewModels).fUserId;
            // 檢查是否有尚未維修的料槍資料
            bool foundData = db.ES_MaterialGunRepair.Any(o => o.MaterialGun_Sno == MaterialGun_Sno && o.MaintenanceResult == null);
            if (foundData)
            {
                // 若有尚未維修的資料，顯示錯誤訊息並回到送修開單畫面
                TempData["message"] = "料槍尚未維修，請勿重複開單！";
                return View("MaterialGunForRepair", "_MaterialLayout");
            }
            else
            {
                // 若無重複開單，建立 RepairGun 物件並執行送修作業
                RepairGun repair = new RepairGun(db);
                // 執行送修作業
                repair.ForRepairWork(Id, MaterialGun_Sno, BadDescription, Mark);

                // 設定成功標記 (傳遞給 View 顯示 SweetAlert2)
                TempData["ShowSuccess"] = true;

                // 送修完成後導向維修主畫面
                return RedirectToAction("MaterialGunRepairView");
            }
        }

        /// <summary>
        /// 送修的維修畫面。
        /// </summary>
        /// <param name="sno">維修單號</param>
        /// <returns>維修作業 View</returns>
        public ActionResult MaterialGunRepair(int sno)
        {
            // 檢查使用者是否已登入
            if (!Login_Authentication())
            {
                // 若未登入則導向登入頁面
                return RedirectToAction("Login", "Home");
            }
            // 依據維修單號取得料槍維修資料
            var result = db.ES_MaterialGunRepair.Find(sno);
            // 將料槍編號存入 ViewBag
            ViewBag.Eno = result.MaterialGun_Sno;
            // 將分類代碼存入 ViewBag
            ViewBag.Classic = result.Classification;
            // 將維修結果代碼存入 ViewBag
            ViewBag.Results = result.MaintenanceResult;
            // 回傳維修作業畫面
            return View("MaterialGunRepair", "_MaterialLayout");
        }

        /// <summary>
        /// 送修的維修資料送出。
        /// </summary>
        /// <param name="sno">維修單號</param>
        /// <param name="MaterialGun_Sno">料槍編號</param>
        /// <param name="Classification">分類代碼</param>
        /// <param name="MaintenanceResult">維修結果代碼</param>
        /// <param name="Other">其他說明</param>
        /// <param name="ChangeItemName">更換品項名稱</param>
        /// <param name="ChangeItemNo">更換品項編號</param>
        /// <param name="b_Chk">是否勾選</param>
        /// <returns>導向維修主畫面</returns>
        [HttpPost]
        public ActionResult MaterialGunRepair(int sno, string MaterialGun_Sno, int Classification, int MaintenanceResult, string Other, string ChangeItemName, string ChangeItemNo, string b_Chk)
        {
            string Id = (Session["Member"] as MemberViewModels).fUserId;
            RepairGun repairGun = new RepairGun(db);
            repairGun.RepairWork(Id, sno, Classification, MaintenanceResult, Other, ChangeItemName, ChangeItemNo, b_Chk);
            return RedirectToAction("MaterialGunRepairView");
        }

        /// <summary>
        /// 料槍基本資料查詢畫面。
        /// </summary>
        /// <param name="page">分頁頁碼</param>
        /// <param name="SChk">是否篩選保養</param>
        /// <param name="MaterialG">料槍編號</param>
        /// <param name="Trade">廠商</param>
        /// <param name="Discrad">是否篩選報廢</param>
        /// <param name="Btn">查詢按鈕</param>
        /// <param name="N_Maintain">是否篩選未保養</param>
        /// <returns>料槍基本資料 View</returns>
        public ActionResult MaterialGunInfoView(int? page, bool? SChk, string MaterialG, string Trade, bool? Discrad, string Btn, bool? N_Maintain)
        {
            if (!Login_Authentication())
            {
                return RedirectToAction("Login", "Home");
            }
            int pagesize = 50;
            int currentPage = page ?? 1;
            bool _SChk = SChk ?? false;
            bool _Discrad = Discrad ?? false;
            bool _N_Maintain = N_Maintain ?? false;
            List<MaterialGunInfo.GunInfoDataList> gunDataLists;

            ViewBag.CategoryItems = GetMaterialGunCategories();

            MaterialGunInfo GunInfo = new MaterialGunInfo(db);
            if (string.IsNullOrEmpty(Btn))
            {
                gunDataLists = GunInfo.GunInfoData();
            }
            else
            {
                gunDataLists = GunInfo.GunInfoSearch(_SChk, _Discrad, MaterialG, Trade, _N_Maintain);
            }

            return View("MaterialGunInfoView", "_MaterialLayout", gunDataLists.ToPagedList(currentPage, pagesize));
        }

        /// <summary>
        /// 維修開單資料確認 (AJAX)。
        /// </summary>
        /// <param name="input">料槍編號</param>
        /// <returns>是否已維修的 JSON 結果</returns>
        [HttpPost]
        public JsonResult CheckData(string input)
        {
            // 強制轉大寫並去除空白，確保資料一致性
            input = NormalizeMaterialGunSno(input);

            // 檢查該料槍是否有「尚未維修」的送修資料
            bool hasUnrepaired = db.ES_MaterialGunRepair.Any(o => o.MaterialGun_Sno == input && !o.MaintenanceResult.HasValue);

            // 若有尚未維修，回傳 "未維修"，否則回傳空字串
            return Json(new { result = hasUnrepaired ? "未維修" : "" });
        }

        /// <summary>
        /// 取得料槍廠商下拉選單資料。
        /// </summary>
        /// <returns>廠商選單資料</returns>
        private List<SelectListItem> GetMaterialGunCategories()
        {
            var categories = db.ES_MaterialGunInfo
                                .Select(a => new { Value = a.MaterialGun_Trade, Text = a.MaterialGun_Trade })
                                .Distinct()
                                .ToList();

            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem()
            {
                Text = "All",
                Value = "All"
            });
            foreach (var category in categories)
            {
                items.Add(new SelectListItem()
                {
                    Text = category.Text,
                    Value = category.Text
                });
            }

            return items;
        }

        /// <summary>
        /// 料槍報廢畫面。
        /// </summary>
        /// <param name="Sno">料槍編號</param>
        /// <returns>報廢 View</returns>
        public ActionResult Discard(string Sno)
        {
            var result = db.ES_MaterialGunInfo.SingleOrDefault(m => m.sno.ToString() == Sno);
            return View("Discard", "_MaterialLayout", result);
        }

        /// <summary>
        /// 料槍報廢資料送出。
        /// </summary>
        /// <param name="Sno">料槍編號</param>
        /// <param name="isTrue">是否確認</param>
        /// <param name="DiscardDescription">報廢描述代碼</param>
        /// <param name="Mark">備註</param>
        /// <returns>導向料槍基本資料頁</returns>
        [HttpPost]
        public ActionResult Discard(string Sno, bool isTrue, int DiscardDescription, string Mark)
        {
            string Id = (Session["Member"] as MemberViewModels).fUserId;
            MaterialGunInfo D_material = new MaterialGunInfo(db);
            D_material.DiscardWork(Sno, isTrue, DiscardDescription, Mark, Id);
            return RedirectToAction("MaterialGunInfoView");
        }

        /// <summary>
        /// 料槍報廢主管確認。
        /// </summary>
        /// <param name="Sno">料槍編號</param>
        /// <returns>導向料槍基本資料頁</returns>
        public ActionResult ManagerCheck(string Sno)
        {
            MaterialGunInfo D_material = new MaterialGunInfo(db);
            D_material.DiscardCheck(Sno);
            return RedirectToAction("MaterialGunInfoView");
        }

        /// <summary>
        /// 料槍資訊查詢 (AJAX 部分頁)。
        /// </summary>
        /// <param name="SChk">是否篩選保養</param>
        /// <param name="Discrad">是否篩選報廢</param>
        /// <param name="MaterialG">料槍編號</param>
        /// <param name="Trade">廠商</param>
        /// <param name="N_Maintain">是否篩選未保養</param>
        /// <param name="page">分頁頁碼</param>
        /// <returns>料槍資訊 PartialView</returns>
        public ActionResult MaterialGunInfoSearch(bool? SChk, bool? Discrad, string MaterialG, string Trade, bool? N_Maintain, int? page)
        {
            if (!Login_Authentication())
            {
                return RedirectToAction("Login", "Home");
            }
            int pagesize = 50;
            int currentPage = page ?? 1;
            bool _SChk = SChk ?? false;
            bool _Discrad = Discrad ?? false;
            bool _N_Maintain = N_Maintain ?? false;

            ViewBag.CategoryItems = GetMaterialGunCategories();

            MaterialGunInfo G_info = new MaterialGunInfo(db);
            List<MaterialGunInfo.GunInfoDataList> gunDataLists = G_info.GunInfoSearch(_SChk, _Discrad, MaterialG, Trade, _N_Maintain);
            return PartialView("_MaterialGunInfoView", gunDataLists.ToPagedList(currentPage, pagesize));
        }

        /// <summary>
        /// 料槍新增畫面。
        /// </summary>
        /// <returns>新增 View</returns>
        public ActionResult MaterialGunCreate()
        {
            return View("MaterialGunCreate", "_MaterialLayout");
        }

        /// <summary>
        /// 料槍新增資料送出。
        /// </summary>
        /// <param name="MaterialGun_Eno">料槍編號</param>
        /// <param name="MaterialGun_Sno">料槍序號</param>
        /// <param name="MaterialGun_Trade">廠商</param>
        /// <param name="MaterialGun_Size">尺寸</param>
        /// <param name="MaintainCycle">保養週期</param>
        /// <returns>導向料槍基本資料頁</returns>
        [HttpPost]
        public ActionResult MaterialGunCreate(string MaterialGun_Eno, string MaterialGun_Sno, string MaterialGun_Trade, string MaterialGun_Size, int MaintainCycle)
        {
            string uId = (Session["Member"] as MemberViewModels).fUserId;
            ES_MaterialGunInfo gunAdd = new ES_MaterialGunInfo();
            //gunAdd.MaterialGun_Eno = MaterialGun_Eno;
            gunAdd.MaterialGun_Sno = MaterialGun_Sno;
            gunAdd.MaterialGun_Trade = MaterialGun_Trade;
            gunAdd.MaterialGun_Size = MaterialGun_Size;
            gunAdd.MaintainCycle = MaintainCycle;
            gunAdd.MaterialGunDiscard = false;
            gunAdd.DiscardCheck = false;
            gunAdd.CreateTime = DateTime.Now;
            gunAdd.CreateUserId = uId;
            db.ES_MaterialGunInfo.Add(gunAdd);
            db.SaveChanges();
            return RedirectToAction("MaterialGunInfoView");
        }

        /// <summary>
        /// 料槍履歷查詢主畫面。
        /// </summary>
        /// <param name="work">查詢類型 (M:保養, R:維修, C:校正, O:全部)</param>
        /// <returns>履歷查詢 View</returns>
        public ActionResult MaterialAbout(string work)
        {
            if (!Login_Authentication())
            {
                return RedirectToAction("Login", "Home");
            }
            ViewBag.Works = work;
            return View("MaterialAbout", "_MaterialLayout");
        }

        /// <summary>
        /// 料槍履歷查詢 (AJAX 部分頁)。
        /// </summary>
        /// <param name="work">查詢類型</param>
        /// <param name="MaterialG">料槍編號</param>
        /// <param name="indate">起始日期</param>
        /// <param name="indate2">結束日期</param>
        /// <param name="page">分頁頁碼</param>
        /// <returns>履歷查詢 PartialView</returns>
        public ActionResult PartialViewMRCO(string work, string MaterialG, string indate, string indate2, int? page)
        {
            int currentPage = page ?? 1;
            List<MaterialAbout.AboutDataList> abDataList = new List<MaterialAbout.AboutDataList>();
            MaterialAbout about = new MaterialAbout(db)
            {
                Works = work,
                Materials = MaterialG,
                Indate = indate,
                Indate2 = indate2
            };

            switch (work)
            {
                case "M":
                    abDataList = about.WorkM();
                    break;
                case "R":
                    abDataList = about.WorkR();
                    break;
                case "C":
                    abDataList = about.WorkC();
                    break;
                case "O":
                    abDataList = about.WorkO();
                    break;
            }
            ViewBag.Works = work;
            return PartialView("_PartialViewMRCO", abDataList.ToPagedList(currentPage, pagesize));
        }

        #region 各頁下拉選單

        /// <summary>
        /// 取得[送修原因]/[不良描述]下拉選單資料。
        /// </summary>
        /// <returns>不良描述 JSON 資料</returns>
        /// <summary>
        /// 多語系資源 DTO (用於 SQL 查詢)
        /// </summary>
        public class AppMultiLanguageResource
        {
            /// <summary>
            /// 語系 ID
            /// </summary>
            public string ResourceId { get; set; }
            /// <summary>
            /// 語系值
            /// </summary>
            public string ResourceValue { get; set; }
        }

        /// <summary>
        /// 取得[送修原因]/[不良描述]下拉選單資料 (支援越南文多語系)。
        /// </summary>
        /// <returns>不良描述 JSON 資料</returns>
        [HttpGet]
        public ActionResult BadDesc()
        {
            // 1. 取得原始中文資料
            var rawList = db.ES_MaterialGunBadDesc.AsNoTracking().ToList();

            // 2. 嘗試取得越南文翻譯資源 (使用原始 SQL 避免 EDMX 未更新問題)
            List<AppMultiLanguageResource> viResources = new List<AppMultiLanguageResource>();
            try
            {
                // 檢查資料表是否存在並讀取
                string sql = @"
                    SELECT ResourceId, ResourceValue 
                    FROM AppMultiLanguageResources 
                    WHERE ResourceType = 'ES_MaterialGunBadDesc' 
                    AND CultureCode = 'vi'";

                viResources = db.Database.SqlQuery<AppMultiLanguageResource>(sql).ToList();
            }
            catch (Exception ex)
            {
                // 若資料表不存在或查詢失敗，僅記錄但不阻擋流程，維持顯示中文
                // System.Diagnostics.Debug.WriteLine($"多語系資源讀取失敗: {ex.Message}");
            }

            // 3. 合併顯示資料
            var descriptions = rawList.Select(a => {
                // 嘗試比對翻譯 (ResourceId 對應 KeyWorld)
                var viText = viResources.FirstOrDefault(r => r.ResourceId == a.KeyWorld.ToString())?.ResourceValue;

                // 組合顯示文字：若有翻譯則顯示 "中文 (越南文)"，否則僅顯示 "中文"
                var displayText = string.IsNullOrEmpty(viText)
                    ? a.BadDescription
                    : $"{a.BadDescription} ({viText})";

                return new { Value = a.KeyWorld, Text = displayText };
            }).ToList();

            descriptions.Insert(0, new { Value = 0, Text = "請選擇... (Vui lòng chọn...)" });
            return Json(descriptions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得分類下拉選單資料。
        /// </summary>
        /// <returns>分類 JSON 資料</returns>
        [HttpGet]
        public ActionResult Classification()
        {
            var descriptions = db.ES_MaterialGunRepairClass.Select(a => new { Value = a.KeyWorld, Text = a.Classification }).ToList();
            descriptions.Insert(0, new { Value = 0, Text = "請選擇..." });
            return Json(descriptions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得維修結果下拉選單資料。
        /// </summary>
        /// <returns>維修結果 JSON 資料</returns>
        [HttpGet]
        public ActionResult MResult()
        {
            var descriptions = db.ES_MaterialGunMResult.Select(a => new { Value = a.KeyWorld, Text = a.MaintenanceResult }).ToList();
            descriptions.Insert(0, new { Value = 0, Text = "請選擇..." });
            descriptions.Add(new { Value = 99, Text = "其他" });
            return Json(descriptions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得料槍廠商下拉選單資料。
        /// </summary>
        /// <returns>廠商 JSON 資料</returns>
        [HttpGet]
        public ActionResult MGunInfoTrade()
        {
            var descriptions = db.ES_MaterialGunInfo.Distinct().Select(a => new { Value = a.MaterialGun_Trade, Text = a.MaterialGun_Trade }).Distinct().ToList();
            descriptions.Insert(0, new { Value = "0", Text = "請選擇..." });
            return Json(descriptions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得料槍尺寸下拉選單資料。
        /// </summary>
        /// <returns>尺寸 JSON 資料</returns>
        [HttpGet]
        public ActionResult MGunInfoSize()
        {
            var descriptions = db.ES_MaterialGunSize.OrderBy(a => a.sno).Select(a => new { Value = a.MaintenanceSize, Text = a.MaintenanceSize }).ToList();
            descriptions.Insert(0, new { Value = "0", Text = "請選擇..." });
            return Json(descriptions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得料槍保養週期下拉選單資料。
        /// </summary>
        /// <returns>保養週期 JSON 資料</returns>
        [HttpGet]
        public ActionResult MGunInfoCycle()
        {
            var descriptions = db.ES_MaterialGunInfo.Select(a => new { Value = 1, Text = a.MaintainCycle.ToString() }).Distinct().ToList();
            descriptions.Insert(0, new { Value = 0, Text = "請選擇..." });
            return Json(descriptions, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得報廢描述下拉選單資料。
        /// </summary>
        /// <returns>報廢描述 JSON 資料</returns>
        [HttpGet]
        public ActionResult DiscardDesc()
        {
            var descriptions = db.ES_MaterialGunDiscardDesc.Select(a => new { Value = a.KeyWorld, Text = a.DiscardDescription }).ToList();
            descriptions.Insert(0, new { Value = 0, Text = "請選擇..." });
            return Json(descriptions, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Login 驗證相關Class

        /// <summary>
        /// 驗證使用者是否已登入。
        /// </summary>
        /// <returns>登入狀態</returns>
        public bool Login_Authentication()
        {
            if (Session["Member"] != null)
            {
                string UserId = (Session["Member"] as MemberViewModels).fUserId;
                string RoleId = (Session["Member"] as MemberViewModels).ROLE_ID;
                ViewBag.RoleId = RoleId;
                ViewBag.UserId = UserId;
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Excel ActionFunction

        /// <summary>
        /// 匯出料槍查詢結果 Excel 檔案。
        /// </summary>
        /// <param name="works">查詢類型</param>
        /// <param name="MaterialG">料槍編號</param>
        /// <param name="indate">起始日期</param>
        /// <param name="indate2">結束日期</param>
        /// <returns>Excel 檔案下載</returns>
        public ActionResult ExportToExcel(string works, string MaterialG, string indate, string indate2)
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            ExcelFunction excel = new ExcelFunction(db)
            {
                Works = works,
                Materials = MaterialG,
                Indate = indate,
                Indate2 = indate2
            };
            ISheet sheet = workbook.CreateSheet("查詢資料");
            IRow headerRow = sheet.CreateRow(0);

            if (works == "Lists")
            {
                workbook = excel.ExportGunList();
            }
            else
            {
                workbook = excel.ExportGunSearch();
            }

            string fileName = "查詢明細.xlsx";

            using (MemoryStream stream = new MemoryStream())
            {
                workbook.Write(stream);
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
        #endregion

        /// <summary>
        /// 將料槍編號標準化：移除全形空白、轉半形、轉大寫，
        /// 並保留字母與數字間的空格。
        /// </summary>
        /// <param name="input">原始料槍編號</param>
        /// <returns>標準化後的料槍編號</returns>
        private static string NormalizeMaterialGunSno(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // 1. 轉半形（全形英數→半形英數，全形空白→半形空白）
            // 2. 去頭尾空白
            // 3. 轉大寫
            var normalized = input
                .Replace('　', ' ') // 全形空白→半形空白
                .Trim()
                .ToUpperInvariant();

            // 4. 只移除「字母和字母」或「數字和數字」間的空白
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < normalized.Length; i++)
            {
                char c = normalized[i];
                if (char.IsWhiteSpace(c) && i > 0 && i < normalized.Length - 1)
                {
                    char prev = normalized[i - 1];
                    char next = normalized[i + 1];
                    bool prevIsLetter = char.IsLetter(prev);
                    bool nextIsLetter = char.IsLetter(next);
                    bool prevIsDigit = char.IsDigit(prev);
                    bool nextIsDigit = char.IsDigit(next);

                    // 移除字母和字母或數字和數字間的空白
                    if ((prevIsLetter && nextIsLetter) || (prevIsDigit && nextIsDigit))
                    {
                        continue;
                    }
                }
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}