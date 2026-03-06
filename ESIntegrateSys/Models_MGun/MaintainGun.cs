using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ESIntegrateSys.Models;

namespace ESIntegrateSys.Models_MGun
{
    /// <summary>
    /// 管理 Material Gun 保養相關的作業與資料，包含查詢保養紀錄與新增保養工作記錄。
    /// </summary>
    public class MaintainGun
    {
        private readonly ESIntegrateSysEntities db;
        /// <summary>
        /// 建構子，使用指定的資料庫上下文初始化 MaintainGun 實例。
        /// </summary>
        /// <param name="dbContext">資料庫上下文 <see cref="ESIntegrateSysEntities"/></param>
        public MaintainGun(ESIntegrateSysEntities dbContext)
        {
            db = dbContext;
            MGunDataLists = new List<MGunDataList>();
        }
        /// <summary>
        /// 保存查詢結果的保養資料清單，可供畫面顯示或後續處理使用。
        /// </summary>
        public List<MGunDataList> MGunDataLists { get; set; }
        /// <summary>
        /// 表示單筆保養資料，用於頁面顯示或資料傳遞。
        /// </summary>
        public class MGunDataList
        {
            /// <summary>資料庫內的流水號</summary>
            public int Sno { get; set; }
            /// <summary>Material Gun 的序號</summary>
            public string MaterialGun_Sno { get; set; }
            /// <summary>Material Gun 的員工編號或操作人編號</summary>
            public string MaterialGun_Eno { get; set; }
            /// <summary>保養週期（以月為單位）</summary>
            public int MaintainCycle { get; set; }
            /// <summary>實際保養時間</summary>
            public DateTime MaintainTime { get; set; }
            /// <summary>保養執行者的使用者代號</summary>
            public string MUserNo { get; set; }
            /// <summary>保養執行者姓名</summary>
            public string fName { get; set; }
            /// <summary>下一次保養預定日期</summary>
            public DateTime MaintainNexDate { get; set; }
        }
        #region 頁面顯示
        /// <summary>
        /// 取得所有保養作業記錄並回傳為 <see cref="MGunDataList"/> 的清單（含執行者姓名）。
        /// 註：已優化僅顯示每把料槍最新的一筆紀錄。
        /// </summary>
        /// <returns>保養資料清單</returns>
        public List<MGunDataList> MaintainGunData()
        {
            // 1. 基礎查詢
            var baseQuery = from a in db.ES_MaintainWork
                            join b in db.ES_Member on a.MUserNo equals b.fUserId into memberJoin
                            from b in memberJoin.DefaultIfEmpty()
                            select new
                            {
                                sno = a.sno,
                                MaterialGun_Sno = a.MaterialGun_Sno,
                                MaintainCycle = a.MaintainCycle,
                                MaintainTime = a.MaintainTime,
                                fName = b != null ? b.fName : null,
                                MaintainNexDate = a.MaintainNexDate,
                            };

            // 2. 記憶體中分組：針對每把料槍取最新一筆
            var latestData = baseQuery.ToList()
                .GroupBy(x => x.MaterialGun_Sno)
                .Select(g => g.OrderByDescending(x => x.MaintainTime).FirstOrDefault())
                .ToList();

            MGunDataLists.Clear();
            foreach (var item in latestData)
            {
                if (item == null) continue;

                MGunDataLists.Add(new MGunDataList
                {
                    Sno = item.sno,
                    MaterialGun_Sno = item.MaterialGun_Sno,
                    MaintainCycle = (int)(item.MaintainCycle ?? 0),
                    MaintainTime = (DateTime)(item.MaintainTime ?? DateTime.Now),
                    fName = item.fName,
                    MaintainNexDate = (DateTime)(item.MaintainNexDate ?? DateTime.Now),
                });
            }

            return MGunDataLists.OrderByDescending(x => x.MaintainTime).ToList();
        }
        #endregion

        /// <summary>
        /// 建立指定 Material Gun 的保養工作記錄並儲存至資料庫（會根據保養週期計算下一次保養日期）。
        /// </summary>
        /// <param name="Gun_Sno">要保養的 Material Gun 序號</param>
        /// <param name="uId">執行保養的使用者帳號</param>
        /// <returns>若成功儲存回傳 true，否則回傳 false (例如找不到料槍或已報廢)</returns>
        public bool MaintainWork(string Gun_Sno, string uId)
        {
            var infos = from a in db.ES_MaterialGunInfo
                        join b in db.ES_Member on a.MaterialGun_Eno equals b.fUserId into memberJoin
                        from b in memberJoin.DefaultIfEmpty()
                        where a.MaterialGun_Sno == Gun_Sno && a.DiscardCheck == false
                        select new MGunDataList
                        {
                            MaterialGun_Sno = a.MaterialGun_Sno,
                            MaintainCycle = (int)a.MaintainCycle,
                            MaintainTime = DateTime.Now,
                            fName = b != null ? b.fName : null,
                        };

            var info = infos.FirstOrDefault();
            if (info != null)
            {
                ES_MaintainWork work = new ES_MaintainWork();
                work.MaterialGun_Sno = info.MaterialGun_Sno;
                work.MUserNo = uId;
                work.MaintainCycle = info.MaintainCycle;
                work.MaintainTime = DateTime.Now;
                work.MaintainNexDate = DateTime.Now.AddMonths(info.MaintainCycle);
                db.ES_MaintainWork.Add(work);
                db.SaveChanges();
                return true;
            }

            return false;
        }
    }
}