using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ESIntegrateSys.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ESIntegrateSys.Controllers
{
    /// <summary>
    /// 工單工時相關控制器，提供工單工時查詢、統計、匯出等功能。
    /// </summary>
    public class WoTimeSheetController : Controller
    {
        /// <summary>
        /// 工單號碼。
        /// </summary>
        public string WoNo { get; set; }

        /// <summary>
        /// 查詢開始日期。
        /// </summary>
        public string Indate { get; set; }

        /// <summary>
        /// 查詢結束日期。
        /// </summary>
        public string Indate2 { get; set; }

        /// <summary>
        /// 工單工時主頁。
        /// </summary>
        /// <returns>主頁視圖。</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 工單工時查詢頁面。
        /// </summary>
        /// <returns>工單工時查詢視圖。</returns>
        public ActionResult WoTS()
        {
            return View("WoTS", "_WoTSLayout");
        }

        /// <summary>
        /// 工單工時查詢頁面 POST 方法。
        /// </summary>
        /// <param name="wono">工單號碼。</param>
        /// <param name="indate">查詢開始日期。</param>
        /// <param name="indate2">查詢結束日期。</param>
        /// <returns>工單工時查詢視圖。</returns>
        [HttpPost]
        public ActionResult WoTS(string wono, string indate, string indate2)
        {
            WoNo = wono; // 設定工單號碼
            Indate = indate; // 設定查詢開始日期
            Indate2 = indate2; // 設定查詢結束日期

            TempData["WoNo"] = wono; // 暫存工單號碼
            TempData["Indate"] = indate; // 暫存查詢開始日期
            TempData["Indate2"] = indate2; // 暫存查詢結束日期

            // 將查詢結果的資料表進行轉置處理
            DataTable pivotedData = PivotDataTable(AdoNet().Item1);

            // 回傳工單工時查詢視圖，並帶入轉置後的資料表
            return View("WoTS", "_WoTSLayout", pivotedData);
        }

        /// <summary>
        /// 站別個別加總查詢頁面。
        /// </summary>
        /// <remarks>
        /// 此方法回傳站別個別加總查詢的主視圖，供使用者進行站別工時統計查詢。
        /// </remarks>
        /// <returns>站別個別加總查詢視圖。</returns>
        public ActionResult StTS()
        {
            return View("StTS", "_WoTSLayout");
        }

        /// <summary>
        /// 站別個別加總查詢 POST 方法。<br/>
        /// 根據工單號碼與日期區間查詢站別加總資料，並將結果轉置後回傳至視圖。<br/>
        /// </summary>
        /// <param name="wono">工單號碼。</param>
        /// <param name="indate">查詢開始日期。</param>
        /// <param name="indate2">查詢結束日期。</param>
        /// <returns>站別個別加總查詢視圖，並帶入轉置後的資料表。</returns>
        [HttpPost]
        public ActionResult StTS(string wono, string indate, string indate2)
        {
            WoNo = wono;
            Indate = indate;
            Indate2 = indate2;

            // 工單號碼
            TempData["WoNo"] = wono;
            // 查詢開始日期
            TempData["Indate"] = indate;
            // 查詢結束日期
            TempData["Indate2"] = indate2;

            // 將數據表轉置
            DataTable pivotedData = PivotDataTable(AdoNet().Item2);

            return View("StTS", "_WoTSLayout", pivotedData);
        }

        /// <summary>
        /// 工單工時異動查詢主頁。
        /// </summary>
        /// <remarks>
        /// 此方法回傳工單工時異動查詢的主視圖，供使用者進行工單開工資料異動查詢。
        /// </remarks>
        /// <returns>工單工時異動查詢視圖。</returns>
        public ActionResult EdList()
        {
            return View("EdList", "_WoTSLayout");
        }

        /// <summary>
        /// 工單工時異動查詢 POST 方法。<br/>
        /// 根據工單號碼與日期區間查詢工單工時異動資料，僅顯示開工日期與修改日期相差超過 2 天的紀錄。<br/>
        /// 查詢結果包含工單號碼、開工日期、修改日期、修改人員，並依修改日期降冪排序。
        /// </summary>
        /// <param name="wono">工單號碼。</param>
        /// <param name="indate">查詢開始日期。</param>
        /// <param name="indate2">查詢結束日期。</param>
        /// <returns>工單工時異動查詢視圖，並帶入查詢結果資料表。</returns>
        [HttpPost]
        public ActionResult EdList(string wono, string indate, string indate2)
        {

            WoNo = wono;
            Indate = indate;
            Indate2 = indate2;

            TempData["WoNo"] = wono;
            TempData["Indate"] = indate;
            TempData["Indate2"] = indate2;

            string edSql = "select WO_NO,OPEN_TIME,UPDATE_DATE,b.USER_NAME from JH_WO_TIMESHEET a" +
                            " left join JH_USER b on a.UPDATE_USERID = b.USER_ID" +
                            " where datediff(day, OPEN_TIME, UPDATE_DATE) > 2";

            if (!string.IsNullOrEmpty(WoNo))
            {
                edSql += " and wo_no= @wono ";
            }
            //開始有日期;結束沒有
            if (!string.IsNullOrEmpty(Indate) && string.IsNullOrEmpty(Indate2))
            {
                edSql += " and UPDATE_DATE =@date1 ";
            }
            //開始沒有日期;結束有
            if (string.IsNullOrEmpty(Indate) && !string.IsNullOrEmpty(Indate2))
            {
                Indate2 = DateTime.Parse(Indate2).AddDays(1).ToString("yyyy-MM-dd");
                edSql += " and UPDATE_DATE =@date2 ";
            }

            if (!string.IsNullOrEmpty(Indate) && !string.IsNullOrEmpty(Indate2))
            {
                string dateLogi = @" and UPDATE_DATE between CAST(@startDate AS DATETIME) + CAST('00:10:00' AS TIME) 
                                        and DATEADD(DAY, 1, CAST(@endDate AS DATETIME)) + CAST('00:09:59' AS TIME)";
                edSql += dateLogi;
            }

            edSql += " order by UPDATE_DATE desc";

            SqlParameter[] parm = new SqlParameter[]
            {
                new SqlParameter("wono",WoNo),
                new SqlParameter("startDate",Indate),
                new SqlParameter("endDate",Indate2),
            };

            // AMES 1.0 執行 SQL 查詢並取得結果資料表
            DataTable Tdata = Db200.ExecuteDataTable(edSql, CommandType.Text, parm);
            Session["EdList"] = Tdata;
            return View("EdList", "_WoTSLayout", Tdata);
        }

        /// <summary>
        /// 將工單工時資料表依工單號碼與測試類型進行轉置，產生每個工單的各測試類型加總欄位。
        /// </summary>
        /// <param name="dt">原始工單工時資料表，需包含 WO_NO、TEST_TYPE、sb 欄位。</param>
        /// <returns>轉置後的資料表，包含 WO_NO、oo1o、oo2o、oo3o 欄位。</returns>
        private DataTable PivotDataTable(DataTable dt)
        {
            // 建立新的資料表以存放轉置後的結果
            DataTable pivotedTable = new DataTable();

            // 添加 WO_NO 列
            pivotedTable.Columns.Add("WO_NO");

            // 添加固定的 TEST_TYPE 列
            pivotedTable.Columns.Add("oo1o");
            pivotedTable.Columns.Add("oo2o");
            pivotedTable.Columns.Add("oo3o");

            // 取得所有工單號碼
            var woNos = dt.AsEnumerable()
                          .Select(r => r["WO_NO"].ToString())
                          .Distinct()
                          .ToList();

            // 依工單號碼填入資料
            foreach (var woNo in woNos)
            {
                // 建立新列
                var row = pivotedTable.NewRow();
                // 設定工單號碼
                row["WO_NO"] = woNo;

                // 依 TEST_TYPE 填入對應欄位資料
                foreach (var testType in new[] { "0010", "0020", "0030" })
                {
                    // 轉換 TEST_TYPE 為對應的列名
                    var columnName = ConvertTestTypeToColumnName(testType);
                    // 查找對應的值
                    var value = dt.AsEnumerable()
                                  .Where(r => r["WO_NO"].ToString() == woNo && r["TEST_TYPE"].ToString() == testType)
                                  .Select(r => r["sb"].ToString())
                                  .FirstOrDefault();

                    row[columnName] = value ?? "0"; // 若無資料則填 0
                }

                // 將新列加入轉置後的資料表
                pivotedTable.Rows.Add(row);
            }

            return pivotedTable;
        }



        /// <summary>
        /// 輔助方法：將 TEST_TYPE 轉換為對應的欄位名稱。
        /// </summary>
        /// <param name="testType">測試類型代碼（如 "0010", "0020", "0030"）。</param>
        /// <returns>對應的欄位名稱（如 "oo1o", "oo2o", "oo3o"）。</returns>
        /// <exception cref="System.ArgumentException">當傳入未知的 TEST_TYPE 時，拋出例外。</exception>
        private string ConvertTestTypeToColumnName(string testType)
        {
            switch (testType)
            {
                case "0010":
                    return "oo1o";
                case "0020":
                    return "oo2o";
                case "0030":
                    return "oo3o";
                default:
                    throw new ArgumentException("未知的 TEST_TYPE 值: " + testType);
            }
        }

        /// <summary>
        /// 依據工單號碼與日期區間，查詢工單工時與數量統計資料。
        /// </summary>
        /// <remarks>
        /// 此方法會根據工單號碼、開始日期、結束日期，組合 SQL 查詢語句，
        /// 並分別查詢工單工時加總、個別測試類型加總、數量統計等資料。
        /// 查詢結果會存入 ViewBag 以供前端顯示，並以 Tuple 形式回傳工單工時加總與數量統計的 DataTable。
        /// </remarks>
        /// <returns>
        /// Tuple，Item1 為工單工時加總 DataTable，Item2 為數量統計 DataTable。
        /// </returns>
        private Tuple<DataTable, DataTable> AdoNet()
        {
            string Tsql = "select a.WO_NO,b.TEST_TYPE," +
                                    "case" +
                                        " when TEST_TYPE='0010' then sum(a.TOTAL_CT*PRODUCTION_QTY)" +
                                        " when TEST_TYPE='0020' then sum(a.TOTAL_CT*PRODUCTION_QTY*a.OP_CNT)" +
                                        " when TEST_TYPE='0030' then sum(a.TOTAL_CT*PRODUCTION_QTY*a.OP_CNT)" +
                                    " end sb" +
                                    " from JH_WO_TIMESHEET a" +
                                    " left join JH_STATION b on a.STATION_ID=b.STATION_ID" +
                                    " where TEST_TYPE is not null ";// and (b.STATION_NAME <> 'CPK送驗' or STATION_NAME <> '單板送驗') ";

            #region 個別統計 0010、0020、0030
            string Tsql2 = "select b.TEST_TYPE," +
                " case when TEST_TYPE = '0010' then sum(a.TOTAL_CT* PRODUCTION_QTY)" +
                " when TEST_TYPE = '0020' then sum(a.TOTAL_CT* PRODUCTION_QTY*a.OP_CNT)" +
                " when TEST_TYPE = '0030' then sum(a.TOTAL_CT* PRODUCTION_QTY*a.OP_CNT)" +
                " end sb" +
                " from JH_WO_TIMESHEET a" +
                " left join JH_STATION b on a.STATION_ID = b.STATION_ID" +
                " where TEST_TYPE is not null ";// and (b.STATION_NAME <> 'CPK送驗' or STATION_NAME <> '單板送驗') ";
            #endregion

            #region 數量統計
            /*
            	SMT: SMT_TOP
            	測試: 送驗品保
            	包裝: 單板送驗(非CPK工單)、CPK送驗(CPK送驗)
            	組裝: PACKING
             */
            string Tsql3 = "select a.WO_NO,b.TEST_TYPE," +
                " case when TEST_TYPE = '0010' then sum(PRODUCTION_QTY)" +
                " when TEST_TYPE = '0020' then sum(PRODUCTION_QTY)" +
                " when TEST_TYPE = '0030' then sum(PRODUCTION_QTY)" +
                " end sb" +
                " from JH_WO_TIMESHEET a " +
                " left join JH_STATION b on a.STATION_ID = b.STATION_ID where TEST_TYPE is not null " +
                " and STATION_NAME in ('SMT_TOP', '送驗品保', '單板送驗', 'CPK送驗', 'PACKING') ";
            #endregion

            #region 個別統計 0010、0020、0030
            string Tsql4 = "select b.TEST_TYPE," +
                " case when TEST_TYPE = '0010' then sum(PRODUCTION_QTY)" +
                " when TEST_TYPE = '0020' then sum(PRODUCTION_QTY)" +
                " when TEST_TYPE = '0030' then sum(PRODUCTION_QTY)" +
                " end sb" +
                " from JH_WO_TIMESHEET a" +
                " left join JH_STATION b on a.STATION_ID = b.STATION_ID" +
                " where TEST_TYPE is not null " +
                " and STATION_NAME in ('SMT_TOP', '送驗品保', '單板送驗', 'CPK送驗', 'PACKING') ";
            #endregion

            //工單號碼
            if (!string.IsNullOrEmpty(WoNo))
            {
                Tsql += " and wo_no= @wono ";
                Tsql2 += " and wo_no= @wono ";
                Tsql3 += " and wo_no= @wono ";
                Tsql4 += " and wo_no= @wono ";
            }
            //開始有日期;結束沒有
            if (!string.IsNullOrEmpty(Indate) && string.IsNullOrEmpty(Indate2))
            {
                Tsql += " and a.OPEN_TIME =@date1 ";
                Tsql2 += " and a.OPEN_TIME =@date1 ";
                Tsql3 += " and a.OPEN_TIME =@date1 ";
                Tsql4 += " and a.OPEN_TIME =@date1 ";
            }
            //開始沒有日期;結束日期有
            if (string.IsNullOrEmpty(Indate) && !string.IsNullOrEmpty(Indate2))
            {
                Indate2 = DateTime.Parse(Indate2).AddDays(1).ToString("yyyy-MM-dd");
                Tsql += " and a.OPEN_TIME =@date2 ";
                Tsql2 += " and a.OPEN_TIME =@date2 ";
                Tsql3 += " and a.OPEN_TIME =@date2 ";
                Tsql4 += " and a.OPEN_TIME =@date2 ";
            }
            //開始日期;結束日期有
            if (!string.IsNullOrEmpty(Indate) && !string.IsNullOrEmpty(Indate2))
            {
                string dateLogi = @" and CLOSE_TIME between CAST(@startDate AS DATETIME) + CAST('00:10:00' AS TIME) 
                                                                and DATEADD(DAY, 1, CAST(@endDate AS DATETIME)) + CAST('00:09:59' AS TIME)";
                Tsql += dateLogi;
                Tsql2 += dateLogi;
                Tsql3 += dateLogi;
                Tsql4 += dateLogi;
            }

            Tsql += " group by a.WO_NO,b.TEST_TYPE order by wo_no , b.TEST_TYPE";
            Tsql2 += " group by b.TEST_TYPE order by b.TEST_TYPE";
            Tsql3 += " group by a.WO_NO,b.TEST_TYPE order by wo_no, b.TEST_TYPE";
            Tsql4 += " group by b.TEST_TYPE order by b.TEST_TYPE";
            SqlParameter[] parm = new SqlParameter[]
            {
                        new SqlParameter("wono",WoNo),
                        new SqlParameter("startDate",Indate),
                        new SqlParameter("endDate",Indate2),
            };

            #region AMES 1.0
            #region WoTS View
            //工單加總清單
            DataTable dt = Db200.ExecuteDataTable(Tsql, CommandType.Text, parm);
            //個別樓層加總
            DataSet ds = Db200.ExecuteDataSet(Tsql2, CommandType.Text, parm);
            #endregion

            #region StTS View
            //數量統計清單
            DataTable dt2 = Db200.ExecuteDataTable(Tsql3, CommandType.Text, parm);
            //個別樓層加總
            DataSet ds2 = Db200.ExecuteDataSet(Tsql4, CommandType.Text, parm);
            #endregion
            #endregion

            //存入ViewBag
            // 逐行處理查詢結果資料表中的每一筆資料
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                // 根據 TEST_TYPE 欄位的值，將加總結果存入對應的 ViewBag 屬性
                switch (row["TEST_TYPE"].ToString())
                {
                    case "0010":
                        // TEST_TYPE 為 0010 時，存入 ViewBag.oo1o
                        ViewBag.oo1o = row["sb"].ToString();
                        break;
                    case "0020":
                        // TEST_TYPE 為 0020 時，存入 ViewBag.oo2o
                        ViewBag.oo2o = row["sb"].ToString();
                        break;
                    case "0030":
                        // TEST_TYPE 為 0030 時，存入 ViewBag.oo3o
                        ViewBag.oo3o = row["sb"].ToString();
                        break;
                    default:
                        // 其他 TEST_TYPE 不做任何處理
                        break;
                }
            }

            // 逐行處理 ds2 資料表中的每一筆資料
            foreach (DataRow row in ds2.Tables[0].Rows)
            {
                // 根據 TEST_TYPE 欄位的值，將加總結果存入對應的 ViewBag 屬性
                switch (row["TEST_TYPE"].ToString())
                {
                    case "0010":
                        // TEST_TYPE 為 0010 時，存入 ViewBag.oo1o2
                        ViewBag.oo1o2 = row["sb"].ToString();
                        break;
                    case "0020":
                        // TEST_TYPE 為 0020 時，存入 ViewBag.oo2o2
                        ViewBag.oo2o2 = row["sb"].ToString();
                        break;
                    case "0030":
                        // TEST_TYPE 為 0030 時，存入 ViewBag.oo3o2
                        ViewBag.oo3o2 = row["sb"].ToString();
                        break;
                    default:
                        // 其他 TEST_TYPE 不做任何處理
                        break;
                }
            }

            return new Tuple<DataTable, DataTable>(dt, dt2);
        }

        /// <summary>
        /// 匯出工單工時統計 Excel 檔案。
        /// </summary>
        /// <param name="wono">工單號碼。</param>
        /// <param name="indate">查詢開始日期。</param>
        /// <param name="indate2">查詢結束日期。</param>
        /// <returns>Excel 檔案下載結果。</returns>
        public ActionResult ExportExcel(string wono, string indate, string indate2)
        {
            // 取得暫存的工單號碼
            WoNo = TempData["WoNo"].ToString();
            // 取得暫存的查詢開始日期
            Indate = TempData["Indate"].ToString();
            // 取得暫存的查詢結束日期
            Indate2 = TempData["Indate2"].ToString();
            // 取得工單工時加總資料並進行轉置
            DataTable dt = PivotDataTable(AdoNet().Item1);
            // 建立新的 Excel 工作簿
            XSSFWorkbook workbook = new XSSFWorkbook();
            // 在工作簿中建立一個工作表，名稱為「總工時List」
            ISheet sheet = workbook.CreateSheet("總工時List");
            // 建立標題列
            IRow headerRow = sheet.CreateRow(0);
            // 設定第一欄標題為「工單號碼」
            headerRow.CreateCell(0).SetCellValue("工單號碼");
            // 設定第二欄標題為「oo1o」
            headerRow.CreateCell(1).SetCellValue("oo1o");
            // 設定第三欄標題為「oo2o」
            headerRow.CreateCell(2).SetCellValue("oo2o");
            // 設定第四欄標題為「oo3o」
            headerRow.CreateCell(3).SetCellValue("oo3o");
            // 設定資料列起始索引為 1
            int rowindex = 1;

            // 逐行處理資料表中的每一筆資料
            foreach (DataRow dataRow in dt.Rows)
            {
                // 計算三個欄位的總和
                double dsum =
                    (double.TryParse(dataRow["oo1o"].ToString(), out double oo1oValues) ? oo1oValues : 0) +
                    (double.TryParse(dataRow["oo2o"].ToString(), out double oo2oValues) ? oo2oValues : 0) +
                    (double.TryParse(dataRow["oo3o"].ToString(), out double oo3oValues) ? oo3oValues : 0);
                // 如果總和大於 0，則建立新的一行
                if (dsum > 0)
                {
                    // 在 Excel 工作表建立新的一行
                    IRow row5 = sheet.CreateRow(rowindex);
                    // 設定工單號碼欄位
                    row5.CreateCell(0).SetCellValue(dataRow["WO_NO"].ToString());
                    // 設定 oo1o 欄位
                    row5.CreateCell(1).SetCellValue(double.TryParse(dataRow["oo1o"].ToString(), out double oo1oValue) ? oo1oValue : 0);
                    // 設定 oo2o 欄位
                    row5.CreateCell(2).SetCellValue(double.TryParse(dataRow["oo2o"].ToString(), out double oo2oValue) ? oo2oValue : 0);
                    // 設定 oo3o 欄位
                    row5.CreateCell(3).SetCellValue(double.TryParse(dataRow["oo3o"].ToString(), out double oo3oValue) ? oo3oValue : 0);
                    //row5.CreateCell(2).SetCellValue(dataRow["sb"].ToString()); // 備註：原本的 sb 欄位註解掉
                    // 行索引加一，準備下一行
                    rowindex++;
                }
            }

            // 設定檔案名稱
            string fileName = "總工時統計.xlsx";

            // 儲存 Excel 檔案
            using (MemoryStream stream = new MemoryStream())
            {
                // 將 Excel 工作簿寫入串流
                workbook.Write(stream);
                // 取得串流內容為 byte 陣列
                var content = stream.ToArray();
                // 回傳 Excel 檔案下載，指定檔案類型與檔名
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        /// <summary>
        /// 匯出工單工時異動 Excel 檔案。
        /// </summary>
        /// <param name="wono">工單號碼。</param>
        /// <param name="indate">查詢開始日期。</param>
        /// <param name="indate2">查詢結束日期。</param>
        /// <returns>Excel 檔案下載結果。</returns>
        public ActionResult ExportEditExcel(string wono, string indate, string indate2)
        {
            // 取得暫存的工單號碼
            WoNo = TempData["WoNo"].ToString();
            // 取得暫存的查詢開始日期
            Indate = TempData["Indate"].ToString();
            // 取得暫存的查詢結束日期
            Indate2 = TempData["Indate2"].ToString();

            // 取得工單工時異動資料表
            DataTable dt = Session["EdList"] as DataTable;
            // 建立新的 Excel 工作簿
            XSSFWorkbook workbook = new XSSFWorkbook();
            // 在工作簿中建立一個工作表，名稱為「開工資料異動List」
            ISheet sheet = workbook.CreateSheet("開工資料異動List");
            // 建立標題列
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("工單號碼");
            headerRow.CreateCell(1).SetCellValue("開工日期");
            headerRow.CreateCell(2).SetCellValue("修改日期");
            headerRow.CreateCell(3).SetCellValue("修改人員");
            int rowindex = 1;
            // 逐行處理資料表中的每一筆資料
            foreach (DataRow dataRow in dt.Rows)
            {
                // 在 Excel 工作表建立新的一行
                IRow row5 = sheet.CreateRow(rowindex);
                row5.CreateCell(0).SetCellValue(dataRow["WO_NO"].ToString());
                row5.CreateCell(1).SetCellValue(dataRow["OPEN_TIME"].ToString());
                row5.CreateCell(2).SetCellValue(dataRow["UPDATE_DATE"].ToString());
                row5.CreateCell(3).SetCellValue(dataRow["USER_NAME"].ToString());
                rowindex++;
            }
            // 設定檔案名稱
            string fileName = "開工資料異動.xlsx";

            // 儲存 Excel 檔案
            using (MemoryStream stream = new MemoryStream())
            {
                workbook.Write(stream); // 將 Excel 工作簿寫入串流
                var content = stream.ToArray(); // 取得串流內容為位元組陣列
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName); // 回傳 Excel 檔案下載，指定檔案類型與檔名
            }
        }
    }
}