using ESIntegrateSys.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ESIntegrateSys.Models_MGun
{
    /// <summary>
    /// 提供料槍相關 Excel 匯出功能的類別，負責產生各式報表（清單、保養/校正/維修統計與明細）。
    /// </summary>
    public class ExcelFunction
    {
        /// <summary>
        /// Db 資料庫操作物件。
        /// </summary>
        private readonly ESIntegrateSysEntities db;

        /// <summary>
        /// 建構子：初始化資料庫操作物件。
        /// </summary>
        /// <param name="dbContext">ESIntegrateSys 資料庫上下文物件，用於後續資料存取。</param>
        public ExcelFunction(ESIntegrateSysEntities dbContext)
        {
            db = dbContext;
        }

        /// <summary>
        /// 作業類型（M：保養、R：維修、C：校正、O：全部）。
        /// </summary>
        public string Works { get; set; }

        /// <summary>
        /// 料槍編號
        /// </summary>
        public string Materials { get; set; }

        /// <summary>
        /// 查詢起始日期（格式：yyyy-MM-dd）。
        /// </summary>
        public string Indate { get; set; }

        /// <summary>
        /// 查詢結束日期（格式：yyyy-MM-dd）。
        /// </summary>
        public string Indate2 { get; set; }

        /// <summary>
        /// 匯出料槍清單總表為 Excel 檔案，包含料槍明細與異動統計。
        /// </summary>
        /// <returns>回傳建立完成的 <see cref="NPOI.XSSF.UserModel.XSSFWorkbook"/> 工作簿物件。</returns>
        public XSSFWorkbook ExportGunList()
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("查詢資料");

            string Tsql1 = "SELECT MaterialGun_Trade, MaterialGun_Size, COUNT(MaterialGun_Size) AS Counts FROM ES_MaterialGunInfo " +
                           "GROUP BY MaterialGun_Trade, MaterialGun_Size " +
                           "ORDER BY MaterialGun_Trade, MaterialGun_Size";
            DataTable dt = Db.ExecuteDataTable(Tsql1, CommandType.Text, null);

            string Tsql2 = "SELECT MaterialGun_Trade, MaterialGun_Size, CAST(DiscardDate AS DATE) AS Ddate, b.DiscardDescription, " +
                           "COUNT(MaterialGun_Size) AS Counts FROM ES_MaterialGunInfo a " +
                           "LEFT JOIN ES_MaterialGunDiscardDesc b ON a.DiscardDesc = b.KeyWorld " +
                           "WHERE (DiscardDate IS NOT NULL OR DiscardDate <> '') " +
                           "GROUP BY MaterialGun_Trade, MaterialGun_Size, CAST(DiscardDate AS DATE), b.DiscardDescription " +
                           "ORDER BY MaterialGun_Trade, MaterialGun_Size, Ddate, b.DiscardDescription";
            DataTable dt2 = Db.ExecuteDataTable(Tsql2, CommandType.Text, null);

            List<MaterialGunInfo.GunInfoDataList> DataList = new MaterialGunInfo(db).GunInfoData();

            // 建立表頭
            CreateHeader(sheet);

            // 塞資料
            FillGunListData(sheet, dt, dt2, DataList);

            return workbook;
        }

        /// <summary>
        /// 匯出料槍查詢結果為 Excel 檔案，依照指定作業類型輸出統計與明細資料。
        /// </summary>
        /// <returns>回傳建立完成的 <see cref="NPOI.XSSF.UserModel.XSSFWorkbook"/> 工作簿物件。</returns>
        public XSSFWorkbook ExportGunSearch()
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            List<MaterialAbout.AboutDataList> abDataList = new List<MaterialAbout.AboutDataList>();
            List<SqlParameter> parameters = new List<SqlParameter>();
            List<string> conditions = new List<string>();
            List<string> conditions2 = new List<string>();
            List<string> conditions3 = new List<string>();
            DataTable dtMRCO = new DataTable();
            DataTable dtOM = new DataTable();
            DataTable dtOR = new DataTable();
            DataTable dtOC = new DataTable();
            int rowindex = 0;
            int EFcount = 0;
            MaterialAbout about = new MaterialAbout(db)
            {
                Works = Works,
                Materials = Materials,
                Indate = Indate,
                Indate2 = Indate2,
            };

            ISheet sheet = workbook.CreateSheet("查詢資料");
            IRow headerRow = sheet.CreateRow(0);
            switch (Works)
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
            switch (Works)
            {
                case "M":
                    string tsqlM = "select b.MaterialGun_Trade,b.MaterialGun_Size,count(b.MaterialGun_Size)as Counts from ES_MaintainWork a" +
                                   " left join ES_MaterialGunInfo b on a.MaterialGun_Sno = b.MaterialGun_Sno";

                    if (!string.IsNullOrEmpty(Materials))
                    {
                        conditions.Add(" a.MaterialGun_Sno = @m_Sno");
                        parameters.Add(new SqlParameter("@m_Sno", Materials));
                    }
                    if (!string.IsNullOrEmpty(Indate) && !string.IsNullOrEmpty(Indate2))
                    {
                        conditions.Add(" cast(a.MaintainTime as date) between @startDate and @endDate");
                        parameters.Add(new SqlParameter("@startDate", Indate));
                        parameters.Add(new SqlParameter("@endDate", Indate2));
                    }
                    // 如果有任何條件，則添加 WHERE 子句
                    if (conditions.Count > 0)
                    {
                        tsqlM += " where " + string.Join(" and ", conditions) + " ";
                    }
                    tsqlM += " group by b.MaterialGun_Trade, b.MaterialGun_Size";

                    dtMRCO = Db.ExecuteDataTablePmsList(tsqlM, CommandType.Text, parameters);

                    IRow rowM0 = sheet.CreateRow(0);
                    rowM0.CreateCell(0).SetCellValue("列印日期區間:");
                    rowM0.CreateCell(1).SetCellValue(Indate);
                    rowM0.CreateCell(2).SetCellValue("TO");
                    rowM0.CreateCell(3).SetCellValue(Indate2);

                    IRow rowM2 = sheet.CreateRow(2);
                    rowM2.CreateCell(0).SetCellValue("保養統計:");


                    IRow rowM4 = sheet.CreateRow(3);
                    rowM4.CreateCell(0).SetCellValue("");
                    rowM4.CreateCell(1).SetCellValue("料槍廠商");
                    rowM4.CreateCell(2).SetCellValue("料槍尺寸(規格)");
                    rowM4.CreateCell(3).SetCellValue("保養數量");
                    //這裡開始塞資料
                    rowindex = 4;//4就是excel第5行

                    EFcount = dtMRCO.Rows.Count + rowindex;
                    headerRow = sheet.CreateRow(EFcount + 2);
                    headerRow.CreateCell(0).SetCellValue("原廠編號");
                    headerRow.CreateCell(1).SetCellValue("料槍廠商");
                    headerRow.CreateCell(2).SetCellValue("料槍尺寸");
                    headerRow.CreateCell(3).SetCellValue("保養週期");
                    headerRow.CreateCell(4).SetCellValue("保養日期");
                    headerRow.CreateCell(5).SetCellValue("保養人員");
                    break;
                case "R":
                    string tsqlR = "select b.MaterialGun_Trade,b.MaterialGun_Size,count(b.MaterialGun_Size)as Counts from ES_MaterialGunRepair a" +
                       " left join ES_MaterialGunInfo b on a.MaterialGun_Sno = b.MaterialGun_Sno";

                    if (!string.IsNullOrEmpty(Materials))
                    {
                        conditions.Add(" a.MaterialGun_Sno = @m_Sno");
                        parameters.Add(new SqlParameter("@m_Sno", Materials));
                    }
                    if (!string.IsNullOrEmpty(Indate) && !string.IsNullOrEmpty(Indate2))
                    {
                        conditions.Add(" cast(a.RepairDate as date) between @startDate and @endDate");
                        parameters.Add(new SqlParameter("@startDate", Indate));
                        parameters.Add(new SqlParameter("@endDate", Indate2));
                    }
                    // 如果有任何條件，則添加 WHERE 子句
                    if (conditions.Count > 0)
                    {
                        tsqlR += " where " + string.Join(" and ", conditions) + " ";
                    }
                    tsqlR += " group by b.MaterialGun_Trade, b.MaterialGun_Size";

                    dtMRCO = Db.ExecuteDataTablePmsList(tsqlR, CommandType.Text, parameters);

                    IRow rowR0 = sheet.CreateRow(0);
                    rowR0.CreateCell(0).SetCellValue("列印日期區間:");
                    rowR0.CreateCell(1).SetCellValue(Indate);
                    rowR0.CreateCell(2).SetCellValue("TO");
                    rowR0.CreateCell(3).SetCellValue(Indate2);

                    IRow rowR2 = sheet.CreateRow(2);
                    rowR2.CreateCell(0).SetCellValue("維修統計:");

                    IRow rowR4 = sheet.CreateRow(3);
                    rowR4.CreateCell(0).SetCellValue("");
                    rowR4.CreateCell(1).SetCellValue("料槍廠商");
                    rowR4.CreateCell(2).SetCellValue("料槍尺寸(規格)");
                    rowR4.CreateCell(3).SetCellValue("保養數量");
                    //這裡開始塞資料
                    rowindex = 4;//4就是excel第5行
                    //塞完資料再寫表頭
                    EFcount = dtMRCO.Rows.Count + rowindex;
                    headerRow = sheet.CreateRow(EFcount + 2);
                    headerRow.CreateCell(0).SetCellValue("原廠編號");
                    headerRow.CreateCell(1).SetCellValue("料槍廠商");
                    headerRow.CreateCell(2).SetCellValue("料槍尺寸");
                    headerRow.CreateCell(3).SetCellValue("不良情形");
                    headerRow.CreateCell(4).SetCellValue("送修人員");
                    headerRow.CreateCell(5).SetCellValue("不良分類");
                    headerRow.CreateCell(6).SetCellValue("維修說明");
                    headerRow.CreateCell(7).SetCellValue("更換部品料號");
                    headerRow.CreateCell(8).SetCellValue("更換部品名稱");
                    headerRow.CreateCell(9).SetCellValue("維修日期");
                    headerRow.CreateCell(10).SetCellValue("維修人員");
                    break;
                case "C":
                    string tsqlC = "select b.MaterialGun_Trade,b.MaterialGun_Size,count(b.MaterialGun_Size)as Counts from ES_MaterialCorrection a" +
                       " left join ES_MaterialGunInfo b on a.MaterialGun_Sno = b.MaterialGun_Sno";

                    if (!string.IsNullOrEmpty(Materials))
                    {
                        conditions.Add(" a.MaterialGun_Sno = @m_Sno");
                        parameters.Add(new SqlParameter("@m_Sno", Materials));
                    }
                    if (!string.IsNullOrEmpty(Indate) && !string.IsNullOrEmpty(Indate2))
                    {
                        conditions.Add(" cast(a.CorrectionDate as date) between @startDate and @endDate");
                        parameters.Add(new SqlParameter("@startDate", Indate));
                        parameters.Add(new SqlParameter("@endDate", Indate2));
                    }
                    // 如果有任何條件，則添加 WHERE 子句
                    if (conditions.Count > 0)
                    {
                        tsqlC += " where " + string.Join(" and ", conditions) + " ";
                    }
                    tsqlC += " group by b.MaterialGun_Trade, b.MaterialGun_Size";

                    dtMRCO = Db.ExecuteDataTablePmsList(tsqlC, CommandType.Text, parameters);

                    IRow rowC0 = sheet.CreateRow(0);
                    rowC0.CreateCell(0).SetCellValue("列印日期區間:");
                    rowC0.CreateCell(1).SetCellValue(Indate);
                    rowC0.CreateCell(2).SetCellValue("TO");
                    rowC0.CreateCell(3).SetCellValue(Indate2);

                    IRow rowC2 = sheet.CreateRow(2);
                    rowC2.CreateCell(0).SetCellValue("校正統計:");

                    IRow rowC4 = sheet.CreateRow(3);
                    rowC4.CreateCell(0).SetCellValue("");
                    rowC4.CreateCell(1).SetCellValue("料槍廠商");
                    rowC4.CreateCell(2).SetCellValue("料槍尺寸(規格)");
                    rowC4.CreateCell(3).SetCellValue("校正數量");
                    //這裡開始塞資料
                    rowindex = 4;//4就是excel第5行
                                 //塞完資料再寫表頭
                    EFcount = dtMRCO.Rows.Count + rowindex;
                    headerRow = sheet.CreateRow(EFcount + 2);
                    headerRow.CreateCell(0).SetCellValue("原廠編號");
                    headerRow.CreateCell(1).SetCellValue("料槍廠商");
                    headerRow.CreateCell(2).SetCellValue("料槍尺寸");
                    headerRow.CreateCell(3).SetCellValue("校正週期");
                    headerRow.CreateCell(4).SetCellValue("校正結果");
                    headerRow.CreateCell(5).SetCellValue("校正時間");
                    headerRow.CreateCell(6).SetCellValue("操作人員");
                    headerRow.CreateCell(7).SetCellValue("狀態");
                    break;
                case "O":
                    string tsqlOM = "select b.MaterialGun_Trade,b.MaterialGun_Size,count(b.MaterialGun_Size)as Counts from ES_MaintainWork a" +
                                    " left join ES_MaterialGunInfo b on a.MaterialGun_Sno = b.MaterialGun_Sno";
                    string tsqlOC = "select b.MaterialGun_Trade,b.MaterialGun_Size,count(b.MaterialGun_Size)as Counts from ES_MaterialCorrection a" +
                                    " left join ES_MaterialGunInfo b on a.MaterialGun_Sno = b.MaterialGun_Sno" +
                                    " left join ES_MaterialGunSize c on b.MaterialGun_Size = c.MaintenanceSize";
                    string tsqlOR = "select b.MaterialGun_Trade,b.MaterialGun_Size,count(b.MaterialGun_Size)as Counts from ES_MaterialGunRepair a" +
                                    " left join ES_MaterialGunInfo b on a.MaterialGun_Sno = b.MaterialGun_Sno";

                    if (!string.IsNullOrEmpty(Materials))
                    {
                        conditions.Add(" a.MaterialGun_Sno = @m_Sno");
                        parameters.Add(new SqlParameter("@m_Sno", Materials));
                    }
                    if (!string.IsNullOrEmpty(Indate) && !string.IsNullOrEmpty(Indate2))
                    {
                        conditions.Add(" cast(a.MaintainTime as date) between @startDate and @endDate");
                        conditions2.Add(" cast(a.CorrectionDate as date) between @startDate and @endDate");
                        conditions3.Add(" cast(a.RepairDate as date) between @startDate and @endDate");
                        parameters.Add(new SqlParameter("@startDate", Indate));
                        parameters.Add(new SqlParameter("@endDate", Indate2));
                    }
                    // 如果有任何條件，則添加 WHERE 子句
                    if (conditions.Count > 0)
                    {
                        tsqlOM += " where " + string.Join(" and ", conditions) + " ";
                        tsqlOC += " where " + string.Join(" and ", conditions2) + " ";
                        tsqlOR += " where " + string.Join(" and ", conditions3) + " ";
                    }
                    tsqlOM += " group by b.MaterialGun_Trade, b.MaterialGun_Size";
                    tsqlOC += " group by b.MaterialGun_Trade,b.MaterialGun_Size,c.KeyWorld";
                    tsqlOR += " group by b.MaterialGun_Trade,b.MaterialGun_Size";

                    dtOM = Db.ExecuteDataTablePmsList(tsqlOM, CommandType.Text, parameters);
                    dtOR = Db.ExecuteDataTablePmsList(tsqlOR, CommandType.Text, parameters);
                    dtOC = Db.ExecuteDataTablePmsList(tsqlOC, CommandType.Text, parameters);

                    // 建立表頭
                    CreateMRCOHeader(sheet);
                    break;
                default:
                    break;
            }

            if (Works != "O")
            {
                // 塞資料
                FillGunListData(sheet, dtMRCO, abDataList);
            }
            else
            {
                // 塞資料
                FillGunOListData(sheet, dtOM, dtOC, dtOR, abDataList);
            }

            return workbook;
        }

        /// <summary>
        /// 建立料槍清單總表的表頭。
        /// </summary>
        /// <param name="sheet">Excel 工作表</param>
        private void CreateHeader(ISheet sheet)
        {
            IRow row0 = sheet.CreateRow(0);
            row0.CreateCell(0).SetCellValue("列印日期:");
            row0.CreateCell(1).SetCellValue(DateTime.Now.ToString("yyyyMMdd"));

            IRow row2 = sheet.CreateRow(2);
            row2.CreateCell(0).SetCellValue("料槍明細:");

            IRow row4 = sheet.CreateRow(3);
            row4.CreateCell(1).SetCellValue("料槍廠商");
            row4.CreateCell(2).SetCellValue("料槍尺寸(規格)");
            row4.CreateCell(3).SetCellValue("數量");
        }

        /// <summary>
        /// 建立保養/校正/維修統計的表頭。
        /// </summary>
        /// <param name="sheet">Excel 工作表</param>
        private void CreateMRCOHeader(ISheet sheet)
        {
            IRow row0 = sheet.CreateRow(0);
            row0.CreateCell(0).SetCellValue("列印日期:");
            row0.CreateCell(1).SetCellValue(DateTime.Now.ToString("yyyyMMdd"));

            IRow row2 = sheet.CreateRow(2);
            row2.CreateCell(0).SetCellValue("保養統計:");

            IRow row4 = sheet.CreateRow(3);
            row4.CreateCell(1).SetCellValue("料槍廠商");
            row4.CreateCell(2).SetCellValue("料槍尺寸(規格)");
            row4.CreateCell(3).SetCellValue("數量");
        }

        /// <summary>
        /// 填入料槍清單總表資料。
        /// </summary>
        /// <param name="sheet">Excel 工作表</param>
        /// <param name="dt">料槍統計資料表</param>
        /// <param name="dt2">料槍異動資料表</param>
        /// <param name="DataList">料槍明細資料</param>
        private void FillGunListData(ISheet sheet, DataTable dt, DataTable dt2, List<MaterialGunInfo.GunInfoDataList> DataList)
        {
            int rowIndex = 4;
            foreach (DataRow row in dt.Rows)
            {
                IRow dataRow = sheet.CreateRow(rowIndex++);
                dataRow.CreateCell(1).SetCellValue(row["MaterialGun_Trade"].ToString());
                dataRow.CreateCell(2).SetCellValue(row["MaterialGun_Size"].ToString());
                dataRow.CreateCell(3).SetCellValue((int)row["Counts"]);
            }

            rowIndex++;
            IRow row8 = sheet.CreateRow(rowIndex++);
            row8.CreateCell(0).SetCellValue("料槍異動:");

            IRow row10 = sheet.CreateRow(rowIndex++);
            row10.CreateCell(1).SetCellValue("料槍廠商");
            row10.CreateCell(2).SetCellValue("料槍尺寸(規格)");
            row10.CreateCell(3).SetCellValue("異動日期");
            row10.CreateCell(4).SetCellValue("異動原因");
            row10.CreateCell(5).SetCellValue("數量");

            foreach (DataRow row in dt2.Rows)
            {
                IRow dataRow = sheet.CreateRow(rowIndex++);
                dataRow.CreateCell(1).SetCellValue(row["MaterialGun_Trade"].ToString());
                dataRow.CreateCell(2).SetCellValue(row["MaterialGun_Size"].ToString());
                dataRow.CreateCell(3).SetCellValue(row["Ddate"].ToString());
                dataRow.CreateCell(4).SetCellValue(row["DiscardDescription"].ToString());
                dataRow.CreateCell(5).SetCellValue((int)row["Counts"]);
            }

            rowIndex++;
            IRow row13 = sheet.CreateRow(rowIndex++);
            row13.CreateCell(0).SetCellValue("料槍清單:");

            IRow row14 = sheet.CreateRow(rowIndex++);
            row14.CreateCell(0).SetCellValue("料槍編號");
            row14.CreateCell(1).SetCellValue("料槍廠商");
            row14.CreateCell(2).SetCellValue("料槍尺寸");
            row14.CreateCell(3).SetCellValue("保養週期");
            row14.CreateCell(4).SetCellValue("新增日期");

            foreach (var item in DataList)
            {
                IRow dataRow = sheet.CreateRow(rowIndex++);
                dataRow.CreateCell(0).SetCellValue(item.MaterialGun_Sno);
                dataRow.CreateCell(1).SetCellValue(item.MaterialGun_Trade);
                dataRow.CreateCell(2).SetCellValue(item.MaterialGun_Size);
                dataRow.CreateCell(3).SetCellValue(item.MaintainCycle);
                dataRow.CreateCell(4).SetCellValue(item.CreateTime.ToString("yyyy-MM-dd"));
            }
        }

        /// <summary>
        /// 填入料槍查詢資料 (保養/維修/校正/全部)。
        /// </summary>
        /// <param name="sheet">Excel 工作表</param>
        /// <param name="dt">統計資料表</param>
        /// <param name="DataList">查詢明細資料</param>
        private void FillGunListData(ISheet sheet, DataTable dt, List<MaterialAbout.AboutDataList> DataList)
        {
            int rowIndex = 4;
            int rowsCount = 0;
            foreach (DataRow row in dt.Rows)
            {
                IRow dataRow = sheet.CreateRow(rowIndex++);
                dataRow.CreateCell(1).SetCellValue(row["MaterialGun_Trade"].ToString());
                dataRow.CreateCell(2).SetCellValue(row["MaterialGun_Size"].ToString());
                dataRow.CreateCell(3).SetCellValue((int)row["Counts"]);
                rowsCount += (int)row["Counts"];
            }
            IRow dataCount = sheet.CreateRow(rowIndex++);
            dataCount.CreateCell(2).SetCellValue("總計數量");
            dataCount.CreateCell(3).SetCellValue(rowsCount);

            // 填入 Excel 資料
            rowIndex += 3;
            foreach (var abouts in DataList)
            {
                IRow dataRow = sheet.CreateRow(rowIndex);
                switch (Works)
                {
                    case "M":
                        dataRow.CreateCell(0).SetCellValue(abouts.MaterialGun_Sno);
                        dataRow.CreateCell(1).SetCellValue(abouts.MaterialGun_Trade);
                        dataRow.CreateCell(2).SetCellValue(abouts.MaterialGun_Size);
                        dataRow.CreateCell(3).SetCellValue(abouts.MaintainCycle);
                        dataRow.CreateCell(4).SetCellValue(abouts.MaintainTime != DateTime.MinValue ? abouts.MaintainTime.ToString("yyyy-MM-dd") : "");
                        dataRow.CreateCell(5).SetCellValue(abouts.fName);
                        break;
                    case "R":
                        dataRow.CreateCell(0).SetCellValue(abouts.MaterialGun_Sno);
                        dataRow.CreateCell(1).SetCellValue(abouts.MaterialGun_Trade);
                        dataRow.CreateCell(2).SetCellValue(abouts.MaterialGun_Size);
                        dataRow.CreateCell(3).SetCellValue(abouts.BadDescription);
                        dataRow.CreateCell(4).SetCellValue(abouts.rName);
                        dataRow.CreateCell(5).SetCellValue(abouts.Classification);
                        dataRow.CreateCell(6).SetCellValue(abouts.MaintenanceResult);
                        dataRow.CreateCell(7).SetCellValue(abouts.ChangeItemNo);
                        dataRow.CreateCell(8).SetCellValue(abouts.ChangeItemName);
                        dataRow.CreateCell(9).SetCellValue(abouts.MaintenanceTime != DateTime.MinValue ? abouts.MaintenanceTime.ToString("yyyy-MM-dd") : "");
                        dataRow.CreateCell(10).SetCellValue(abouts.mName);
                        break;
                    case "C":
                        dataRow.CreateCell(0).SetCellValue(abouts.MaterialGun_Sno);
                        dataRow.CreateCell(1).SetCellValue(abouts.MaterialGun_Trade);
                        dataRow.CreateCell(2).SetCellValue(abouts.MaterialGun_Size);
                        dataRow.CreateCell(3).SetCellValue(abouts.MaintainCycle);
                        dataRow.CreateCell(4).SetCellValue(abouts.TestResult ? "OK" : "NG");
                        dataRow.CreateCell(5).SetCellValue(abouts.CorrectionDate != DateTime.MinValue ? abouts.CorrectionDate.ToString("yyyy-MM-dd") : "");
                        dataRow.CreateCell(6).SetCellValue(abouts.fName);
                        dataRow.CreateCell(7).SetCellValue(abouts.Status);
                        break;
                    case "O":
                        dataRow.CreateCell(0).SetCellValue(abouts.MaterialGun_Sno);
                        dataRow.CreateCell(1).SetCellValue(abouts.MaterialGun_Trade);
                        dataRow.CreateCell(2).SetCellValue(abouts.MaterialGun_Size);
                        dataRow.CreateCell(3).SetCellValue(abouts.MaintainCycle);
                        dataRow.CreateCell(4).SetCellValue(abouts.MaintainTime != DateTime.MinValue ? abouts.MaintainTime.ToString("yyyy-MM-dd") : "");
                        dataRow.CreateCell(5).SetCellValue(abouts.mName);
                        dataRow.CreateCell(6).SetCellValue(abouts.TestResult ? "OK" : "NG");
                        dataRow.CreateCell(7).SetCellValue(abouts.CorrectionDate != DateTime.MinValue ? abouts.CorrectionDate.ToString("yyyy-MM-dd") : "");
                        dataRow.CreateCell(8).SetCellValue(abouts.fName);
                        break;
                }
                rowIndex++;
            }
        }

        /// <summary>
        /// 填入全部查詢 (保養/校正/維修) 統計及明細資料。
        /// </summary>
        /// <param name="sheet">Excel 工作表</param>
        /// <param name="dtM">保養統計資料表</param>
        /// <param name="dtC">校正統計資料表</param>
        /// <param name="dtR">維修統計資料表</param>
        /// <param name="DataList">查詢明細資料</param>
        private void FillGunOListData(ISheet sheet, DataTable dtM, DataTable dtC, DataTable dtR, List<MaterialAbout.AboutDataList> DataList)
        {
            int rowIndex = 4;
            int rowsCount = 0;
            IRow dataCount;
            //小統計保養塞資料
            foreach (DataRow row in dtM.Rows)
            {
                IRow dataRow = sheet.CreateRow(rowIndex++);
                dataRow.CreateCell(1).SetCellValue(row["MaterialGun_Trade"].ToString());
                dataRow.CreateCell(2).SetCellValue(row["MaterialGun_Size"].ToString());
                dataRow.CreateCell(3).SetCellValue((int)row["Counts"]);
                rowsCount += (int)row["Counts"];
            }
            dataCount = sheet.CreateRow(rowIndex++);
            dataCount.CreateCell(2).SetCellValue("總計數量");
            dataCount.CreateCell(3).SetCellValue(rowsCount);
            rowsCount = 0;

            //小統計校正開始
            IRow rowC2 = sheet.CreateRow(rowIndex++);
            rowC2.CreateCell(0).SetCellValue("校正統計:");
            IRow rowC4 = sheet.CreateRow(rowIndex++);
            rowC4.CreateCell(1).SetCellValue("料槍廠商");
            rowC4.CreateCell(2).SetCellValue("料槍尺寸(規格)");
            rowC4.CreateCell(3).SetCellValue("數量");
            //小統計校正塞資料
            foreach (DataRow row in dtC.Rows)
            {
                IRow dataRow = sheet.CreateRow(rowIndex++);
                dataRow.CreateCell(1).SetCellValue(row["MaterialGun_Trade"].ToString());
                dataRow.CreateCell(2).SetCellValue(row["MaterialGun_Size"].ToString());
                dataRow.CreateCell(3).SetCellValue((int)row["Counts"]);
                rowsCount += (int)row["Counts"];
            }
            dataCount = sheet.CreateRow(rowIndex++);
            dataCount.CreateCell(2).SetCellValue("總計數量");
            dataCount.CreateCell(3).SetCellValue(rowsCount);
            rowsCount = 0;

            //小統計維修開使
            IRow rowM5 = sheet.CreateRow(rowIndex++);
            rowM5.CreateCell(0).SetCellValue("維修統計:");
            IRow rowM6 = sheet.CreateRow(rowIndex++);
            rowM6.CreateCell(1).SetCellValue("料槍廠商");
            rowM6.CreateCell(2).SetCellValue("料槍尺寸(規格)");
            rowM6.CreateCell(3).SetCellValue("數量");
            //小統計維修塞資料
            foreach (DataRow row in dtR.Rows)
            {
                IRow dataRow = sheet.CreateRow(rowIndex++);
                dataRow.CreateCell(1).SetCellValue(row["MaterialGun_Trade"].ToString());
                dataRow.CreateCell(2).SetCellValue(row["MaterialGun_Size"].ToString());
                dataRow.CreateCell(3).SetCellValue((int)row["Counts"]);
                rowsCount += (int)row["Counts"];
            }
            dataCount = sheet.CreateRow(rowIndex++);
            dataCount.CreateCell(2).SetCellValue("總計數量");
            dataCount.CreateCell(3).SetCellValue(rowsCount);

            IRow headerRow0 = sheet.CreateRow(rowIndex++);
            headerRow0.CreateCell(0).SetCellValue("清單總表:");
            IRow headerRow = sheet.CreateRow(rowIndex++);
            headerRow.CreateCell(0).SetCellValue("原廠編號");
            headerRow.CreateCell(1).SetCellValue("料槍廠商");
            headerRow.CreateCell(2).SetCellValue("料槍尺寸");
            headerRow.CreateCell(3).SetCellValue("作業日期");
            headerRow.CreateCell(4).SetCellValue("校正結果");
            headerRow.CreateCell(5).SetCellValue("狀態");
            headerRow.CreateCell(6).SetCellValue("不良情形");
            headerRow.CreateCell(7).SetCellValue("維修說明");
            headerRow.CreateCell(8).SetCellValue("更換部品名稱");
            headerRow.CreateCell(9).SetCellValue("更換部品料號");
            headerRow.CreateCell(10).SetCellValue("操作人員");

            // 填入 Excel 資料
            foreach (var abouts in DataList)
            {
                IRow dataRow = sheet.CreateRow(rowIndex);
                dataRow.CreateCell(0).SetCellValue(abouts.MaterialGun_Sno);
                dataRow.CreateCell(1).SetCellValue(abouts.MaterialGun_Trade);
                dataRow.CreateCell(2).SetCellValue(abouts.MaterialGun_Size);
                dataRow.CreateCell(3).SetCellValue(abouts.MaintainTime != DateTime.MinValue ? abouts.MaintainTime.ToString("yyyy-MM-dd") : "");
                dataRow.CreateCell(4).SetCellValue(abouts.TestResult ? "OK" : "NG");
                dataRow.CreateCell(5).SetCellValue(abouts.Status);
                dataRow.CreateCell(6).SetCellValue(abouts.BadDescription);
                dataRow.CreateCell(7).SetCellValue(abouts.MaintenanceResult);
                dataRow.CreateCell(8).SetCellValue(abouts.ChangeItemName);
                dataRow.CreateCell(9).SetCellValue(abouts.ChangeItemNo);
                dataRow.CreateCell(10).SetCellValue(abouts.fName);
                rowIndex++;
            }
        }
    }
}