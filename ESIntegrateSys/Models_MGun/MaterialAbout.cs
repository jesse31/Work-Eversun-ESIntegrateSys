using System; // 使用 System 命名空間，提供基本類型與系統功能
using System.Collections.Generic; // 使用泛型集合類別 (List<T> 等)
using System.Data.Entity; // 使用 Entity Framework 的 DbFunctions 與其他 EF 功能
using System.Linq; // 使用 LINQ 查詢語法與擴充方法
using ESIntegrateSys.Models; // 引用專案中的 Models 命名空間

namespace ESIntegrateSys.Models_MGun // 定義命名空間：ESIntegrateSys.Models_MGun
{ // 命名空間區塊開始
    /// <summary> // XML 註解：類別用途說明
    /// 供維修、保養與校正相關查詢使用的服務類別，負責組合不同來源的耗材槍資料並回傳統一格式的結果。 // 類別用途說明
    /// </summary> // XML 註解結束
    public class MaterialAbout // 定義 MaterialAbout 類別，作為耗材槍查詢的服務類別
    { // 類別區塊開始
        private readonly ESIntegrateSysEntities db; // 私有唯讀欄位：Entity Framework 的資料庫上下文
        /// <summary> // XML 註解：Works 屬性說明
        /// 指定查詢類型，使用代碼："M"=保養, "R"=維修, "C"=校正, "O"=全部 // 描述可接受的代碼
        /// </summary>
        public string Works { get; set; }

        /// <summary>
        /// 指定耗材槍序號 (MaterialGun_Sno) 作為查詢條件
        /// </summary>
        public string Materials { get; set; }

        /// <summary>
        /// 查詢起始日期 (以字串傳入，會嘗試解析為 DateTime)
        /// </summary>
        public string Indate { get; set; }

        /// <summary>
        /// 查詢結束日期 (以字串傳入，會嘗試解析為 DateTime)
        /// </summary>
        public string Indate2 { get; set; }
        /// <summary>
        /// 建構子：注入資料庫上下文並初始化查詢結果集合。
        /// </summary>
        /// <param name="dbContext">Entity Framework 的資料庫上下文</param>
        public MaterialAbout(ESIntegrateSysEntities dbContext) // 建構子，透過 DI 注入 dbContext
        { // 建構子區塊開始
            db = dbContext; // 將注入的 dbContext 指派給本地欄位
            AboutDataLists = new List<AboutDataList>(); // 初始化結果清單為空 List
        } // 建構子區塊結束
        /// <summary>
        /// 儲存查詢結果的集合，每次呼叫查詢方法會將結果加入此集合中
        /// </summary>
        public List<AboutDataList> AboutDataLists { get; set; }
        /// <summary>
        /// 單筆查詢結果資料之資料傳輸物件 (DTO)，包含保養/維修/校正等欄位
        /// </summary>
        public class AboutDataList
        {
            /// <summary>資料庫流水號</summary>
            public int Sno { get; set; }
            /// <summary>耗材槍編號 (外觀編號)</summary>
            public string MaterialGun_Eno { get; set; }
            /// <summary>耗材槍序號 (唯一識別)</summary>
            public string MaterialGun_Sno { get; set; }
            /// <summary>耗材槍牌別/型號</summary>
            public string MaterialGun_Trade { get; set; }
            /// <summary>耗材槍規格/尺寸</summary>
            public string MaterialGun_Size { get; set; }
            /// <summary>故障/不良描述</summary>
            public string BadDescription { get; set; }
            /// <summary>回報維修者帳號</summary>
            public string RepairUserId { get; set; }
            /// <summary>實際維修者帳號</summary>
            public string MaintenanceUserId { get; set; }
            /// <summary>維修結果或維護結果描述</summary>
            public string MaintenanceResult { get; set; }
            /// <summary>維修分類</summary>
            public string Classification { get; set; }
            /// <summary>備註或標記欄位</summary>
            public string Mark { get; set; }
            /// <summary>維護結果為其他時之補充說明</summary>
            public string Other { get; set; }
            /// <summary>更換零件名稱</summary>
            public string ChangeItemName { get; set; }
            /// <summary>更換零件料號</summary>
            public string ChangeItemNo { get; set; }
            /// <summary>校正或測試的結果 (布林)</summary>
            public bool TestResult { get; set; }
            /// <summary>保養週期 (天數)</summary>
            public int MaintainCycle { get; set; }
            /// <summary>下次保養時間</summary>
            public DateTime MaintainNexDate { get; set; }
            /// <summary>建立時間</summary>
            public DateTime CreateTime { get; set; }
            /// <summary>保養時間</summary>
            public DateTime MaintainTime { get; set; }
            /// <summary>維護時間/維修完成時間</summary>
            public DateTime MaintenanceTime { get; set; }
            /// <summary>維修報修日期</summary>
            public DateTime RepairDate { get; set; }
            /// <summary>校正日期</summary>
            public DateTime CorrectionDate { get; set; }
            /// <summary>建立者帳號</summary>
            public string CreateUserId { get; set; }
            /// <summary>是否已報廢</summary>
            public bool MaterialGunDiscard { get; set; }
            /// <summary>報廢日期</summary>
            public DateTime DiscardDate { get; set; }
            /// <summary>報廢處理人帳號</summary>
            public string DiscardOPId { get; set; }
            /// <summary>是否核准報廢</summary>
            public bool DiscardCheck { get; set; }
            /// <summary>保養負責人帳號</summary>
            public string MUserNo { get; set; }
            /// <summary>建立者姓名或顯示名稱 (來源於會員)</summary>
            public string fName { get; set; }
            /// <summary>報修者姓名</summary>
            public string rName { get; set; }
            /// <summary>維修者姓名</summary>
            public string mName { get; set; }
            /// <summary>狀態說明 (例如：定保/維修/精度確認)</summary>
            public string Status { get; set; }
        }

        #region 保養 // 區域標記：保養相關方法開始
        /// <summary>
        /// 取得保養紀錄 (Works = "M") 的查詢結果，並套用通用篩選條件。 // 取得保養資料並套用共用篩選
        /// </summary>
        /// <returns>回傳 AboutDataList 的清單結果</returns>
        public List<AboutDataList> WorkM() // 方法：查詢保養紀錄
        { // 方法區塊開始
            var query = from a in db.ES_MaintainWork // 從 ES_MaintainWork 資料表查詢保養工作
                        join b in db.ES_MaterialGunInfo // 連接 ES_MaterialGunInfo 取得耗材槍資訊
                         on new { a.MaterialGun_Sno } equals new { b.MaterialGun_Sno } into materialGunJoin // 依序號連接
                        from b in materialGunJoin.DefaultIfEmpty() // 左外部連接，保留保養紀錄
                        join c in db.ES_Member on a.MUserNo equals c.fUserId into memberJoin // 連接會員以取得姓名
                        from c in memberJoin.DefaultIfEmpty() // 左外部連接會員
                        where b.DiscardCheck == false // 過濾已報廢的耗材槍
                        orderby a.MaintainTime descending // 依保養時間遞減排序
                        select new AboutDataList // 投影結果為 AboutDataList
                        {
                            Sno = a.sno, // 指定 Sno
                            MaterialGun_Sno = a.MaterialGun_Sno, // 指定序號
                            // 暫不使用 MaterialGun_Eno 欄位（來源: a.MaterialGun_Eno），保留以供日後參考
                            MaterialGun_Trade = b.MaterialGun_Trade, // 填入牌別/型號
                            MaterialGun_Size = b.MaterialGun_Size, // 填入規格/尺寸
                            MaintainCycle = (int)a.MaintainCycle, // 保養週期 (轉 int)
                            MaintainTime = (DateTime)a.MaintainTime, // 保養時間
                            fName = b != null ? c.fName : null, // 若有會員資料則填入姓名
                        }; // 查詢投影結束

            query = ApplyFilters(query); // 套用共用的序號/日期篩選
            var dataList = query.ToList(); // 執行查詢並取得清單
            AboutDataLists.AddRange(dataList.ToList()); // 將結果加入類別層級的集合
            return AboutDataLists; // 回傳累積的查詢結果集合
        } // 方法結束

        #endregion // 保養區域結束

        #region 維修 // 區域標記：維修相關方法開始
        /// <summary>
        /// 取得維修紀錄 (Works = "R") 的查詢結果，並套用通用篩選條件。 // 取得維修資料並套用共用篩選
        /// </summary>
        /// <returns>回傳 AboutDataList 的清單結果</returns>
        public List<AboutDataList> WorkR() // 方法：查詢維修紀錄
        { // 方法區塊開始
            var query = from a in db.ES_MaterialGunRepair // 從 ES_MaterialGunRepair 查詢維修紀錄
                        join b in db.ES_MaterialGunBadDesc on a.BadDescription equals b.KeyWorld into badDescJoin // 連接故障描述表
                        from b in badDescJoin.DefaultIfEmpty() // 左外部連接
                        join c in db.ES_MaterialGunRepairClass on a.Classification equals c.KeyWorld into classJoin // 連接維修分類
                        from c in classJoin.DefaultIfEmpty() // 左外部連接
                        join d in db.ES_MaterialGunMResult on a.MaintenanceResult equals d.KeyWorld into resultJoin // 連接維修結果對照表
                        from d in resultJoin.DefaultIfEmpty() // 左外部連接
                        join e in db.ES_Member on a.RepairUserId equals e.fUserId into repairUserJoin // 連接回報維修者會員
                        from e in repairUserJoin.DefaultIfEmpty() // 左外部連接
                        join f in db.ES_Member on a.MaintenanceUserId equals f.fUserId into maintenanceUserJoin // 連接實際維修者會員
                        from f in maintenanceUserJoin.DefaultIfEmpty() // 左外部連接
                        join g in db.ES_MaterialGunInfo on a.MaterialGun_Sno equals g.MaterialGun_Sno into materialGunJoin // 連接耗材槍基本資料
                        from g in materialGunJoin.DefaultIfEmpty() // 左外部連接
                        where a.Chk == true // 只取已經核准的維修紀錄
                        orderby a.MaintenanceTime descending // 依維修完成時間遞減排序
                        select new AboutDataList // 投影為 DTO
                        {
                            Sno = a.sno, // 流水號
                            MaterialGun_Sno = a.MaterialGun_Sno, // 序號
                            // 暫不使用 MaterialGun_Eno 欄位（來源: g.MaterialGun_Eno），保留以供日後參考
                            BadDescription = b != null ? b.BadDescription : null, // 故障描述
                            Classification = c != null ? c.Classification : null, // 維修分類
                            MaintenanceResult = a.MaintenanceResult == 99 ? a.Other : d != null ? d.MaintenanceResult : null, // 維修結果，特殊代碼處理
                            RepairUserId = a.RepairUserId, // 回報者帳號
                            MaintenanceUserId = a.MaintenanceUserId, // 維修者帳號
                            rName = e != null ? e.fName : null, // 回報者姓名
                            mName = f != null ? f.fName : null, // 維修者姓名
                            RepairDate = a.RepairDate.HasValue ? a.RepairDate.Value : DateTime.MinValue, // 若無值則填 MinValue
                            MaintenanceTime = a.MaintenanceTime.HasValue ? a.MaintenanceTime.Value : DateTime.MinValue, // 同上
                            MaterialGun_Trade = g.MaterialGun_Trade, // 耗材槍牌別
                            ChangeItemName = a.ChangeItemName, // 更換零件名稱
                            ChangeItemNo = a.ChangeItemNo, // 更換零件料號
                            MaterialGun_Size = g.MaterialGun_Size // 耗材槍規格
                        }; // 投影結束

            query = ApplyFilters(query); // 套用共用篩選
            var dataList = query.ToList(); // 執行查詢
            AboutDataLists.AddRange(dataList.ToList()); // 累加結果
            return AboutDataLists; // 回傳結果
        } // 方法結束
        #endregion // 維修區域結束

        #region 校正 // 區域標記：校正相關方法開始
        /// <summary>
        /// 取得校正紀錄 (Works = "C") 的查詢結果，並套用通用篩選條件。 // 取得校正資料並套用共用篩選
        /// </summary>
        /// <returns>回傳 AboutDataList 的清單結果</returns>
        public List<AboutDataList> WorkC() // 方法：查詢校正紀錄
        { // 方法區塊開始
            var query = from a in db.ES_MaterialCorrection // 從 ES_MaterialCorrection 查詢校正紀錄
                        join b in db.ES_MaterialGunInfo // 連接耗材槍基本資料
                         on new { a.MaterialGun_Sno } equals new { b.MaterialGun_Sno } into materialGunJoin // 依序號連接
                        from b in materialGunJoin.DefaultIfEmpty() // 左外部連接
                        join c in db.ES_Member on a.operators equals c.fUserId into memberJoin // 連接操作者會員資料
                        from c in memberJoin.DefaultIfEmpty() // 左外部連接
                        where b.DiscardCheck == false // 過濾已報廢耗材槍
                        orderby a.CorrectionDate descending // 依校正日期遞減排序
                        select new AboutDataList // 投影結果
                        {
                            Sno = a.sno, // 流水號
                            MaterialGun_Sno = a.MaterialGun_Sno, // 序號
                            // 暫不使用 MaterialGun_Eno 欄位（來源: a.MaterialGun_Eno），保留以供日後參考
                            MaterialGun_Trade = b.MaterialGun_Trade, // 牌別/型號
                            MaterialGun_Size = b.MaterialGun_Size, // 規格/尺寸
                            MaintainCycle = (int)b.MaintainCycle, // 保養週期 (來源於機具資料)
                            TestResult = (bool)a.TestResult, // 校正結果轉 bool
                            CorrectionDate = (DateTime)a.CorrectionDate, // 校正日期
                            fName = b != null ? c.fName : null, // 操作者姓名 (若有會員資料)
                            Status = (a.memo == "0") ? "定保" :
                                              (a.memo == "1") ? "維修" :
                                              (a.memo == "2") ? "精度確認" : "未知狀態", // 根據 memo 決定狀態文字
                        }; // 投影結束

            query = ApplyFilters(query); // 套用共用篩選
            var dataList = query.ToList(); // 執行查詢
            AboutDataLists.AddRange(dataList.ToList()); // 累加結果
            return AboutDataLists; // 回傳結果集合
        } // 方法結束
        #endregion // 校正區域結束

        #region 全部查詢       // 區域標記：全部合併查詢
        /// <summary>
        /// 取得全部類型的合併查詢結果 (Works = "O")，包含保養、維修、校正等資料，並套用專用篩選條件。 // 合併查詢說明
        /// </summary>
        /// <returns>回傳 AboutDataList 的清單結果</returns>
        public List<AboutDataList> WorkO() // 方法：合併查詢全部類型資料
        { // 方法區開始
            var query = from a in db.ES_MaterialGunInfo // 從耗材槍基本資料開始
                        join b in db.ES_MaintainWork on new { a.MaterialGun_Sno } equals new { b.MaterialGun_Sno } into maintainWorkJoin // 左外部連接保養資料
                        from b in maintainWorkJoin.DefaultIfEmpty() // 左外部連接結果
                        join c in db.ES_MaterialGunRepair on new { a.MaterialGun_Sno } equals new { c.MaterialGun_Sno } into materialGunRepairJoin // 左外部連接維修資料
                        from c in materialGunRepairJoin.DefaultIfEmpty() // 左外部連接結果
                        join d in db.ES_MaterialGunBadDesc on c.BadDescription equals d.KeyWorld into badDescJoin // 連接故障描述
                        from d in badDescJoin.DefaultIfEmpty() // 左外部連接
                        join e in db.ES_MaterialGunRepairClass on c.Classification equals e.KeyWorld into classJoin // 連接維修分類
                        from e in classJoin.DefaultIfEmpty() // 左外部連接
                        join f in db.ES_MaterialGunMResult on c.MaintenanceResult equals f.KeyWorld into resultJoin // 連接維修結果表
                        from f in resultJoin.DefaultIfEmpty() // 左外部連接
                        join g in db.ES_MaterialCorrection on new { a.MaterialGun_Sno, a.MaterialGun_Eno } equals new { g.MaterialGun_Sno, g.MaterialGun_Eno } into correctionJoin // 連接校正資料（序號 + 外觀編號）
                        from g in correctionJoin.DefaultIfEmpty() // 左外部連接
                        join h1 in db.ES_Member on b.MUserNo equals h1.fUserId into memberMUserJoin // 連接保養負責人會員
                        from h1 in memberMUserJoin.DefaultIfEmpty() // 左外部連接
                        join h2 in db.ES_Member on c.RepairUserId equals h2.fUserId into memberRepairUserJoin // 連接回報者會員
                        from h2 in memberRepairUserJoin.DefaultIfEmpty() // 左外部連接
                        join h3 in db.ES_Member on c.MaintenanceUserId equals h3.fUserId into memberMaintenanceUserJoin // 連接維修者會員
                        from h3 in memberMaintenanceUserJoin.DefaultIfEmpty() // 左外部連接
                        join h4 in db.ES_Member on g.operators equals h4.fUserId into memberOperatorsJoin // 連接校正操作者會員
                        from h4 in memberOperatorsJoin.DefaultIfEmpty() // 左外部連接
                        orderby b.MaintainTime descending // 依保養時間排序
                        // 範例：以下為序號與日期範圍的條件範本，已改為使用 ApplyFilters2 處理，故保留註解以供參考
                        // where a.MaterialGun_Sno == Materials &&
                        // (
                        //   (b.MaintainTime >= Indate && b.MaintainTime <= Indate2) ||
                        //   (g.CorrectionDate >= Indate && g.CorrectionDate <= Indate2) ||
                        //   (c.RepairDate >= Indate && c.RepairDate <= Indate2)
                        // )
                        select new AboutDataList // 投影為 AboutDataList
                        {
                            Sno = a.sno, // 流水號
                            MaterialGun_Sno = a.MaterialGun_Sno, // 序號
                            MaterialGun_Trade = a.MaterialGun_Trade, // 牌別
                            MaterialGun_Size = a.MaterialGun_Size, // 規格
                            MaintainTime = b.MaintainTime.HasValue ? b.MaintainTime.Value : DateTime.MinValue, // 保養時間或預設值
                            CorrectionDate = g.CorrectionDate.HasValue ? g.CorrectionDate.Value : DateTime.MinValue, // 校正日期或預設
                            RepairDate = c.RepairDate.HasValue ? c.RepairDate.Value : DateTime.MinValue, // 維修日期或預設
                            TestResult = g.TestResult ?? false, // 校正結果或預設 false
                            Status = (g.memo == "0") ? "定保" :
                                            (g.memo == "1") ? "維修" :
                                            (g.memo == "2") ? "精度確認" : "未知狀態", // 根據 g.memo 判斷狀態文字
                            BadDescription = d != null ? d.BadDescription : null, // 故障描述
                            MaintenanceResult = c.MaintenanceResult == 99 ? c.Other : d != null ? f.MaintenanceResult : null, // 維修結果處理
                            ChangeItemName = c.ChangeItemName, // 更換零件名稱
                            ChangeItemNo = c.ChangeItemNo, // 更換零件料號
                            fName = h1 != null ? h1.fName : h2 != null ? h2.fName : h3 != null ? h3.fName : h4 != null ? h4.fName : null, // 優先取得相關人員姓名
                        }; // 投影結束

            query = ApplyFilters2(query); // 套用全部查詢的特殊篩選
            var dataList = query.ToList(); // 執行查詢取得清單
            AboutDataLists.AddRange(dataList.ToList()); // 累加結果
            return AboutDataLists; // 回傳結果集合
        } // 方法結束
        #endregion // 全部查詢區域結束

        #region 一般查詢 // 區域標記：共用篩選函式
        /// <summary>
        /// 套用一般查詢的篩選條件（序號與日期）。
        /// 依照 Works 屬性決定使用哪個日期欄位進行篩選："M" = 保養 (MaintainTime), "R" = 維修 (MaintenanceTime), "C" = 校正 (CorrectionDate)。
        /// </summary>
        /// <param name="query">原始的 IQueryable 查詢序列，將回傳套用篩選條件後的結果。</param>
        /// <returns>套用篩選條件後的 IQueryable<AboutDataList></returns>
        private IQueryable<AboutDataList> ApplyFilters(IQueryable<AboutDataList> query) // 私有方法：套用序號與日期篩選
        { // 方法區開始
            if (!string.IsNullOrEmpty(Materials)) // 若 Materials 有設定
            {
                query = query.Where(a => a.MaterialGun_Sno == Materials); // 以序號篩選
            }
            #region 日期 // 日期條件區段開始
            if (!string.IsNullOrEmpty(Indate) && !string.IsNullOrEmpty(Indate2)) // 若同時提供起訖日期
            {
                if (DateTime.TryParse(Indate, out DateTime parsedIndate) && DateTime.TryParse(Indate2, out DateTime parsedIndate2)) // 嘗試解析日期字串
                {
                    if (Works == "M") // 若查詢類型為保養
                    {
                        query = query.Where(a => DbFunctions.TruncateTime(a.MaintainTime) >= parsedIndate && DbFunctions.TruncateTime(a.MaintainTime) <= parsedIndate2); // 保養時間區間
                    }
                    else if (Works == "R") // 若查詢類型為維修
                    {
                        query = query.Where(a => DbFunctions.TruncateTime(a.MaintenanceTime) >= parsedIndate && DbFunctions.TruncateTime(a.MaintenanceTime) <= parsedIndate2); // 維修時間區間
                    }
                    else if (Works == "C") // 若查詢類型為校正
                    {
                        query = query.Where(a => DbFunctions.TruncateTime(a.CorrectionDate) >= parsedIndate && DbFunctions.TruncateTime(a.CorrectionDate) <= parsedIndate2); // 校正時間區間
                    }
                }
                else
                {
                    throw new ArgumentException("Indate 或 Indate2 的日期格式無效"); // 若解析失敗，拋出例外
                }
            }
            else if (!string.IsNullOrEmpty(Indate)) // 僅提供起始日期
            {
                if (DateTime.TryParse(Indate, out DateTime parsedIndate)) // 解析起始日期
                {
                    if (Works == "M")
                    {
                        query = query.Where(a => DbFunctions.TruncateTime(a.MaintainTime) == parsedIndate); // 保養等於該日期
                    }
                    else if (Works == "R")
                    {
                        query = query.Where(a => DbFunctions.TruncateTime(a.MaintenanceTime) == parsedIndate); // 維修等於該日期
                    }
                    else if (Works == "C")
                    {
                        query = query.Where(a => DbFunctions.TruncateTime(a.CorrectionDate) == parsedIndate); // 校正等於該日期
                    }
                }
                else
                {
                    throw new ArgumentException("Indate 的日期格式無效"); // 起始日期解析失敗
                }
            }
            else if (!string.IsNullOrEmpty(Indate2)) // 僅提供結束日期
            {
                if (DateTime.TryParse(Indate2, out DateTime parsedIndate2)) // 解析結束日期
                {
                    if (Works == "M")
                    {
                        query = query.Where(a => DbFunctions.TruncateTime(a.MaintainTime) <= parsedIndate2); // 保養小於等於結束日期
                    }
                    else if (Works == "R")
                    {
                        query = query.Where(a => DbFunctions.TruncateTime(a.MaintenanceTime) <= parsedIndate2); // 維修小於等於結束日期
                    }
                    else if (Works == "C")
                    {
                        query = query.Where(a => DbFunctions.TruncateTime(a.CorrectionDate) <= parsedIndate2); // 校正小於等於結束日期
                    }

                }
                else
                {
                    throw new ArgumentException("Indate2 的日期格式無效"); // 結束日期解析失敗
                }
            }
            #endregion // 日期區段結束

            return query; // 回傳套用條件後的查詢
        } // 方法結束
        #endregion // 一般查詢結束

        #region 全部查詢(特殊) // 區域標記：全部查詢(特殊) 的篩選函式
        /// <summary>
        /// 套用「全部查詢(特殊)」的篩選條件。
        /// - 若設定 Materials，則以序號 (MaterialGun_Sno) 篩選（備註：外觀編號判斷暫時停用）。
        /// - 若設定 Indate / Indate2，會解析為 DateTime，並對 MaintainTime、CorrectionDate、RepairDate 三個欄位進行區間或等於的篩選。
        /// 若日期字串無法解析，會拋出 ArgumentException。
        /// </summary>
        /// <param name="query">要套用篩選的 IQueryable<AboutDataList></param>
        /// <returns>套用篩選條件後的 IQueryable<AboutDataList></returns>
        private IQueryable<AboutDataList> ApplyFilters2(IQueryable<AboutDataList> query) // 私有方法：全部查詢的特殊篩選
        { // 方法區開始
            if (!string.IsNullOrEmpty(Materials)) // 若設定序號
            {
                // 根據輸入長度判斷序號或外觀編號（範例，暫時停用）：
                // if (Materials.Length > 10)
                // {
                //     query = query.Where(a => a.MaterialGun_Sno == Materials);
                // }
                // else
                // {
                //     query = query.Where(a => a.MaterialGun_Eno == Materials);
                // }
                query = query.Where(a => a.MaterialGun_Sno == Materials); // 以序號篩選（目前以序號為準）
            }
            #region 日期
            if (!string.IsNullOrEmpty(Indate) && !string.IsNullOrEmpty(Indate2)) // 若同時提供起訖日期
            {
                if (DateTime.TryParse(Indate, out DateTime parsedIndate) && DateTime.TryParse(Indate2, out DateTime parsedIndate2)) // 解析起訖日期
                {
                    query = query.Where(a => (DbFunctions.TruncateTime(a.MaintainTime) >= parsedIndate && DbFunctions.TruncateTime(a.MaintainTime) <= parsedIndate2) ||
                    (DbFunctions.TruncateTime(a.CorrectionDate) >= parsedIndate && DbFunctions.TruncateTime(a.CorrectionDate) <= parsedIndate2) ||
                    (DbFunctions.TruncateTime(a.RepairDate) >= parsedIndate && DbFunctions.TruncateTime(a.RepairDate) <= parsedIndate2)); // 任一日期欄位符合區間即可
                }
                else
                {
                    throw new ArgumentException("Indate 或 Indate2 的日期格式無效"); // 解析失敗，拋例外
                }
            }
            else if (!string.IsNullOrEmpty(Indate)) // 僅提供單一日期
            {
                if (DateTime.TryParse(Indate, out DateTime parsedIndate)) // 解析
                {
                    query = query.Where(a => (DbFunctions.TruncateTime(a.MaintainTime) == parsedIndate) ||
                    (DbFunctions.TruncateTime(a.CorrectionDate) == parsedIndate) ||
                    (DbFunctions.TruncateTime(a.RepairDate) == parsedIndate)); // 任一欄位等於該日期
                }
                else
                {
                    throw new ArgumentException("Indate 的日期格式無效"); // 解析失敗
                }
            }
            else if (!string.IsNullOrEmpty(Indate2)) // 僅提供結束日期
            {
                if (DateTime.TryParse(Indate2, out DateTime parsedIndate2))
                {
                    query = query.Where(a => (DbFunctions.TruncateTime(a.MaintainTime) <= parsedIndate2) ||
                    (DbFunctions.TruncateTime(a.CorrectionDate) <= parsedIndate2) ||
                    (DbFunctions.TruncateTime(a.RepairDate) <= parsedIndate2)); // 任一欄位小於等於結束日期
                }
                else
                {
                    throw new ArgumentException("Indate2 的日期格式無效"); // 解析失敗
                }
            }
            #endregion // 日期區段結束

            return query; // 回傳已套用篩選的查詢
        } // 方法結束
        #endregion // 全部查詢(特殊) 區域結束
    } // 類別結束
} // 命名空間結束