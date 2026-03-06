using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ESIntegrateSys.Models;

namespace ESIntegrateSys.Models_MGun
{
    /// <summary>
    /// 物料槍維修記錄管理類別。
    /// </summary>
    public class RepairGun
    {
        /// <summary>
        /// DataBase 物件，用於資料庫操作。
        /// </summary>
        private readonly ESIntegrateSysEntities db;

        /// <summary>
        /// 建構函式，初始化 RepairGun 類別並設定資料庫內容。
        /// </summary>
        /// <param name="dbContext">資料庫內容物件。</param>
        public RepairGun(ESIntegrateSysEntities dbContext)
        {
            db = dbContext;
            RGunDataLists = new List<RGunDataListDto>();
        }

        /// <summary>
        /// 物料槍維修記錄資料清單。
        /// </summary>
        public List<RGunDataListDto> RGunDataLists { get; set; }

        #region 取得物料槍維修記錄資料清單
        /// <summary>
        /// 取得物料槍維修記錄資料清單。
        /// </summary>
        /// <returns>
        /// 回傳 <see cref="RGunDataListDto"/> 物件的 List，包含所有維修記錄資料。
        /// </returns>
        public List<RGunDataListDto> RepairGunData()
        {
            // 從資料庫取得物料槍維修記錄資料
            var dataList = from a in db.ES_MaterialGunRepair
                               // 連接故障描述資料表
                           join b in db.ES_MaterialGunBadDesc on a.BadDescription equals b.KeyWorld into badDescJoin
                           // 若無對應資料則使用預設值
                           from b in badDescJoin.DefaultIfEmpty()
                               // 連接維修分類資料表
                           join c in db.ES_MaterialGunRepairClass on a.Classification equals c.KeyWorld into classJoin
                           // 若無對應資料則使用預設值
                           from c in classJoin.DefaultIfEmpty()
                               // 連接維修結果資料表
                           join e in db.ES_MaterialGunMResult on a.MaintenanceResult equals e.KeyWorld into resultJoin
                           // 若無對應資料則使用預設值
                           from e in resultJoin.DefaultIfEmpty()
                               // 連接送修人員資料表
                           join f in db.ES_Member on a.RepairUserId equals f.fUserId into repairUserJoin
                           // 若無對應資料則使用預設值
                           from f in repairUserJoin.DefaultIfEmpty()
                               // 連接維修人員資料表
                           join g in db.ES_Member on a.MaintenanceUserId equals g.fUserId into maintenanceUserJoin
                           // 若無對應資料則使用預設值
                           from g in maintenanceUserJoin.DefaultIfEmpty()
                               // 依維修完成狀態及送修日期排序
                           orderby a.Chk, a.RepairDate descending
                           // 建立維修記錄資料模型
                           select new RGunDataListDto
                           {
                               Sno = a.sno, // 維修記錄序號
                               //MaterialGun_Eno = a.MaterialGun_Eno, // 物料槍設備編號（註解掉）
                               MaterialGun_Sno = a.MaterialGun_Sno, // 物料槍序號
                               BadDescription = b != null ? b.BadDescription : null, // 故障描述
                               Classification = c != null ? c.Classification : null, // 維修分類
                               MaintenanceResult = a.MaintenanceResult == 99 ? a.Other : e != null ? e.MaintenanceResult : null, // 維修結果
                               RepairUserId = a.RepairUserId, // 送修人員編號
                               MaintenanceUserId = a.MaintenanceUserId, // 維修人員編號
                               rName = f != null ? f.fName : null, // 送修人員姓名
                               mName = g != null ? g.fName : null, // 維修人員姓名
                               RepairDate = a.RepairDate.HasValue ? a.RepairDate.Value : DateTime.MinValue, // 送修日期
                               MaintenanceTime = a.MaintenanceTime.HasValue ? a.MaintenanceTime.Value : DateTime.MinValue, // 維修完成時間
                               Mark = a.Mark, // 備註
                               Other = a.Other, // 其他維修結果說明
                               ChangeItemName = a.ChangeItemName, // 更換項目名稱
                               ChangeItemNo = a.ChangeItemNo, // 更換項目編號
                               Chk = (bool)a.Chk // 維修完成狀態
                           };

            // 將查詢結果加入維修記錄資料清單
            RGunDataLists.AddRange(dataList.ToList());
            // 回傳維修記錄資料清單
            return RGunDataLists;
        }
        #endregion

        /// <summary>
        /// 送修作業，新增一筆送修記錄至資料庫。
        /// </summary>
        /// <param name="uId">送修人員的使用者編號。</param>
        /// <param name="Sno">物料槍的序號。</param>
        /// <param name="BadDesc">故障描述代碼。</param>
        /// <param name="mark">備註。</param>
        public void ForRepairWork(string uId, string Sno, int BadDesc, string mark)
        {
            // 建立一個新的 ES_MaterialGunRepair 物件
            ES_MaterialGunRepair ForRepair = new ES_MaterialGunRepair();
            // 設定物料槍序號
            ForRepair.MaterialGun_Sno = Sno;
            // 設定送修日期為現在時間
            ForRepair.RepairDate = DateTime.Now;
            // 設定送修人員編號
            ForRepair.RepairUserId = uId;
            // 設定故障描述代碼
            ForRepair.BadDescription = BadDesc;
            // 設定備註
            ForRepair.Mark = mark;
            // 設定送修狀態為未完成
            ForRepair.Chk = false;

            // 將送修資料新增到資料庫
            db.ES_MaterialGunRepair.Add(ForRepair);
            // 儲存變更到資料庫
            db.SaveChanges();
        }

        /// <summary>
        /// 執行維修作業，更新指定物料槍的維修記錄。
        /// </summary>
        /// <param name="uId">維修人員的使用者編號。</param>
        /// <param name="sno">物料槍維修記錄的序號。</param>
        /// <param name="classes">維修分類代碼。</param>
        /// <param name="MResult">維修結果代碼。</param>
        /// <param name="Other">其他維修結果說明。</param>
        /// <param name="ChangeName">更換項目名稱。</param>
        /// <param name="ChangeNo">更換項目編號。</param>
        /// <param name="b_Chk">維修完成狀態（空字串表示已完成）。</param>
        public void RepairWork(string uId, int sno, int classes, int MResult, string Other, string ChangeName, string ChangeNo, string b_Chk)
        {
            // 取得指定序號的維修記錄
            var result = db.ES_MaterialGunRepair.SingleOrDefault(m => m.sno == sno);

            // 設定維修時間為現在
            result.MaintenanceTime = DateTime.Now;
            // 設定維修人員編號
            result.MaintenanceUserId = uId;
            // 設定維修分類代碼
            result.Classification = classes;
            // 設定維修結果代碼
            result.MaintenanceResult = MResult;
            // 設定其他維修結果說明
            result.Other = Other;
            // 設定更換項目名稱（轉大寫）
            result.ChangeItemName = ChangeName.ToUpper();
            // 設定更換項目編號（轉大寫）
            result.ChangeItemNo = ChangeNo.ToUpper();
            // 判斷維修完成狀態，空字串表示已完成
            if (string.IsNullOrEmpty(b_Chk))
            {
                result.Chk = true; // 已完成
            }
            else
            {
                result.Chk = false; // 未完成
            }
            // 儲存變更到資料庫
            db.SaveChanges();
        }
    }
}