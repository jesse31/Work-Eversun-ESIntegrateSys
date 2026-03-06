using System; // 引用 System 命名空間
using System.Collections.Generic; // 引用泛型集合
using System.Linq; // 引用 LINQ 查詢擴充功能
using System.Web; // 引用 System.Web 命名空間（保留）
using ESIntegrateSys.Models; // 引用專案中的資料模型命名空間

namespace ESIntegrateSys.Models_MGun // 宣告命名空間：材料槍相關的模型與服務
{ // 命名空間起始
    /// <summary>管理材料槍資訊的服務類別，包含查詢、報廢等功能。</summary> // 類別說明（繁體中文）
    public class MaterialGunInfo // 管理材料槍資料的主要服務類別
    { // 類別起始
        private readonly ESIntegrateSysEntities db; // 資料庫上下文的唯讀欄位（注入）
        /// <summary>建構子：注入資料庫上下文並初始化資料列表。</summary> // 建構子說明（繁體中文）
        /// <param name="dbContext">ESIntegrateSys 的資料庫上下文</param> // 參數說明
        public MaterialGunInfo(ESIntegrateSysEntities dbContext) // 建構子：接受資料庫上下文參數
        { // 建構子起始
            db = dbContext; // 將注入的 dbContext 指派給內部欄位
            GunInfoDataLists = new List<GunInfoDataList>(); // 初始化暫存清單
        } // 建構子結束
        /// <summary>暫存查詢結果的材料槍清單。</summary> // 屬性說明（繁體中文）
        public List<GunInfoDataList> GunInfoDataLists { get; set; } // 暫存查詢結果的清單屬性

        /// <summary>顯示於頁面的材料槍資料項目 DTO。</summary> // DTO 類別說明（繁體中文）
        public class GunInfoDataList // 材料槍顯示用資料結構（內部類別）
        { // DTO 起始
            /// <summary>資料表流水號</summary> // 屬性說明（繁體中文）
            public int Sno { get; set; } // 資料庫內的流水號欄位
            /// <summary>材料槍設備編號（Eno）</summary>
            public string MaterialGun_Eno { get; set; } // 設備編號（Eno）
            /// <summary>材料槍序號（Sno）</summary>
            public string MaterialGun_Sno { get; set; } // 材料槍的序號
            /// <summary>材料槍型式/廠牌</summary>
            public string MaterialGun_Trade { get; set; } // 材料槍廠牌或型式
            /// <summary>材料槍規格/尺寸</summary>
            public string MaterialGun_Size { get; set; } // 材料槍規格或尺寸
            /// <summary>維護週期（天數）</summary>
            public int MaintainCycle { get; set; } // 維護週期（以天為單位）
            /// <summary>下一次維護預定日期</summary>
            public DateTime MaintainNexDate { get; set; } // 預定的下一次維護日期
            /// <summary>建立時間</summary>
            public DateTime CreateTime { get; set; } // 資料建立時間
            /// <summary>建立人姓名</summary>
            public string fName { get; set; } // 建立者的姓名
            /// <summary>建立者帳號</summary>
            public string CreateUserId { get; set; } // 建立者的帳號 ID
            /// <summary>是否已標為報廢</summary>
            public bool MaterialGunDiscard { get; set; } // 是否被標示為報廢
            /// <summary>報廢日期</summary>
            public DateTime DiscardDate { get; set; } // 實際報廢日期
            /// <summary>報廢處理人帳號</summary>
            public string DiscardOPId { get; set; } // 報廢處理人的帳號
            /// <summary>報廢確認狀態</summary>
            public bool DiscardCheck { get; set; } // 是否已確認報廢
            /// <summary>最後維護執行時間</summary>
            public DateTime MaintainTime { get; set; } // 最後一次維護的執行時間
        } // DTO 結束
        #region 頁面顯示 // 區段：頁面顯示相關方法
        /// <summary>
        /// 取得顯示用的材料槍清單（預設不包含已確認報廢項目）。
        /// </summary>
        /// <returns>回傳目前暫存的 GunInfoDataLists 清單（會將查詢結果新增至該清單）。</returns>
        public List<GunInfoDataList> GunInfoData()
        {
            var dataList = from a in db.ES_MaterialGunInfo
                           join b in db.ES_Member on a.CreateUserId equals b.fUserId into memberJoin
                           from b in memberJoin.DefaultIfEmpty()
                           where a.DiscardCheck == false
                           orderby a.MaterialGunDiscard descending, a.sno descending // 依報廢狀態與流水號排序（報廢優先、再依 sno）
                           select new GunInfoDataList
                           {
                               Sno = a.sno,
                               // MaterialGun_Eno 已暫時註解
                               MaterialGun_Sno = a.MaterialGun_Sno,
                               MaterialGun_Trade = a.MaterialGun_Trade,
                               MaterialGun_Size = a.MaterialGun_Size,
                               MaintainCycle = (int)a.MaintainCycle,
                               fName = b != null ? b.fName : null,
                               // MaintainNexDate 已暫時註解
                               CreateTime = (DateTime)a.CreateTime, // 建立時間（轉型）
                               CreateUserId = a.CreateUserId,
                               MaterialGunDiscard=(bool)a.MaterialGunDiscard, // 報廢旗標來源欄位
                               // DiscardDate 已暫時註解
                               DiscardCheck = (bool)a.DiscardCheck,
                           };

            GunInfoDataLists.AddRange(dataList.ToList());
            return GunInfoDataLists;
        }
        #endregion

        #region 報廢功能 // 區段：報廢相關功能
        /// <summary>執行報廢：當 isTrue 為 true 時，將指定序號的材料槍標記為報廢並儲存報廢資訊。</summary> // 方法說明（繁體中文）
        /// <param name="Sno">材料槍的序號（字串）</param>
        /// <param name="isTrue">是否執行報廢</param>
        /// <param name="DiscardDescription">報廢描述代碼</param>
        /// <param name="Mark">報廢備註</param>
        /// <param name="Id">執行報廢的使用者 Id</param>
        /// <returns>成功則回傳 true，失敗或未執行則回傳 false</returns>
        public Boolean DiscardWork(string Sno,bool isTrue,int DiscardDescription,string Mark,string Id) // 執行報廢作業的方法
        { // 方法起始
            bool b = false; // 預設回傳值為 false（表示未成功）
            if (isTrue) // 當參數指示要執行報廢時才進行變更
            { // 條件起始
                try // 嘗試更新資料，並捕捉可能的例外
                { // try 起始
                    var result = db.ES_MaterialGunInfo.SingleOrDefault(m => m.sno.ToString() == Sno); // 以 Sno 尋找單一資料列
                    result.MaterialGunDiscard = true; // 標記為報廢
                    result.DiscardDesc = DiscardDescription; // 設定報廢描述代碼
                    result.DiscardOPId = Id; // 設定報廢操作人帳號
                    result.DiscardDate = DateTime.Now; // 設定報廢時間為目前時間
                    result.Mark = Mark; // 設定報廢備註
                    b = true; // 標記為操作成功
                    db.SaveChanges(); // 儲存資料庫變更
                } // try 結束
                catch (Exception err) // 捕捉任何例外
                { // catch 起始
                    // 發生例外時回傳失敗（目前僅忽略或可加入紀錄）
                }                
            } // 條件結束
            return b; // 回傳操作結果
        } // 方法結束
        /// <summary>標記指定序號的材料槍為已確認報廢，並更新報廢時間。</summary> // 方法說明（繁體中文）
        /// <param name="Sno">材料槍的序號（字串）</param>
        public void DiscardCheck(string Sno) // 標記報廢確認的方法
        { // 方法起始
            try // 嘗試更新資料庫
            { // try 起始
                var result = db.ES_MaterialGunInfo.SingleOrDefault(m => m.sno.ToString() == Sno); // 以 Sno 查詢該筆資料
                result.DiscardCheck = true; // 設定為已確認報廢
                result.DiscardDate = DateTime.Now; // 更新報廢時間為目前時間
                db.SaveChanges(); // 儲存變更
            } // try 結束
            catch (Exception err) // 捕捉例外並處理
            { // catch 起始
                // 處理例外（目前僅記錄或忽略）
            }            
        } // 方法結束
        #endregion

        #region 查詢功能 // 區段：查詢功能
        // 材料槍搜尋
        /// <summary>
        /// 根據條件搜尋材料槍資料並回傳結果清單。
        /// </summary>
        /// <param name="SChk">是否以序號 (Sno) 搜尋（現僅以 Sno 搜尋）</param>
        /// <param name="Discrad">是否回傳已報廢項目</param>
        /// <param name="MaterialG">搜尋的材料槍序號或設備編號</param>
        /// <param name="Trade">廠牌篩選（"All" 表示不篩選）</param>
        /// <param name="N_Maintain">若為 true，僅回傳尚未維護的項目</param>
        /// <returns>回傳符合條件的 GunInfoDataLists 清單（會將查詢結果新增至 GunInfoDataLists）。</returns>
        public List<GunInfoDataList> GunInfoSearch(bool SChk, bool Discrad, string MaterialG, string Trade,bool N_Maintain)
        {
            var query = from a in db.ES_MaterialGunInfo // 從材料槍資料表查詢
                           join b in db.ES_Member on a.CreateUserId equals b.fUserId into memberJoin // 連接會員表
                           from b in memberJoin.DefaultIfEmpty() // 若無會員則為預設
                           join c in db.ES_MaintainWork on new { a.MaterialGun_Sno, a.MaterialGun_Eno } equals new { c.MaterialGun_Sno, c.MaterialGun_Eno } into maintainWorkJoin // 連接維護工作表
                           from c in maintainWorkJoin.DefaultIfEmpty() // 若無維護紀錄則為預設
                           orderby a.sno descending // 以流水號做排序（由新到舊）
                           //where a.DiscardCheck == false
                           select new GunInfoDataList // 投影為顯示用 DTO
                           {
                               Sno = a.sno, // 指定流水號
                               // MaterialGun_Eno 已暫時註解
                               MaterialGun_Sno = a.MaterialGun_Sno, // 指定序號來源欄位
                               MaterialGun_Trade = a.MaterialGun_Trade, // 指定廠牌來源欄位
                               MaterialGun_Size = a.MaterialGun_Size, // 指定規格欄位
                               MaintainCycle = (int)a.MaintainCycle, // 維護週期（轉為 int）
                               fName = b != null ? b.fName : null, // 建立者姓名（若有會員資料則顯示）
                               MaintainNexDate = a.MaintainNexDate.HasValue?a.MaintainNexDate.Value:DateTime.MinValue, // 下一次維護預定或預設
                               CreateTime = a.CreateTime.HasValue?a.CreateTime.Value:DateTime.MinValue, // 建立時間或預設
                               CreateUserId = a.CreateUserId, // 建立者帳號來源
                               MaterialGunDiscard = (bool)a.MaterialGunDiscard, // 報廢旗標來源
                               // DiscardDate 已暫時註解
                               DiscardDate = a.DiscardDate.HasValue ? a.DiscardDate.Value : DateTime.MinValue, // 報廢日期或預設
                               DiscardCheck = (bool)a.DiscardCheck, // 報廢確認狀態
                               MaintainTime=c.MaintainTime.HasValue ? c.MaintainTime.Value : DateTime.MinValue, // 最後維護時間或預設
                           }; 
            if (!string.IsNullOrEmpty(MaterialG)) // 若有輸入搜尋字串
            {
                // 原先支援以 Eno 或 Sno 搜尋，現以 Sno 為主
                query = query.Where(a => a.MaterialGun_Sno == MaterialG); // 以序號過濾
            }
            if (N_Maintain) // 若僅需回傳尚未維護的項目
            {
                query = query.Where(a => a.MaintainTime==null); // 過濾出尚未維護的
            }
            if (Discrad) // 若要包含已報廢項目
            {
                query = query.Where(a => a.DiscardCheck); // 過濾出已報廢確認的
            }
            else
            {
                query = query.Where(a => !a.DiscardCheck); // 過濾出未報廢確認的
            }

            if (Trade != "All" && !string.IsNullOrEmpty(Trade)) // 若指定廠牌並非 "All"
            {
                query = query.Where(a => a.MaterialGun_Trade == Trade); // 以廠牌過濾
            }
            
            var dataList = query.ToList(); // 執行查詢並取得清單
            GunInfoDataLists.AddRange(dataList.ToList()); // 將查詢結果加入暫存清單
            return GunInfoDataLists; // 回傳結果清單
        }
        #endregion
    }
}