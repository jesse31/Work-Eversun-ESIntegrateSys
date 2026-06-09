## 1. 修復 WoTimeSheet 500 錯誤

- [x] 1.1 修改 WoTimeSheetController.cs 的 AdoNet() 方法：將第 341-344 行的 @date1 改為 @indate
- [x] 1.2 修改 WoTimeSheetController.cs 的 AdoNet() 方法：將第 350-353 行的 @date2 改為 @indate2
- [x] 1.3 修改 WoTimeSheetController.cs 的 AdoNet() 方法：將第 358-359 行的 @startDate/@endDate 改為 @indate/@indate2
- [x] 1.4 修改 WoTimeSheetController.cs 的 AdoNet() 方法：將第 370-375 行的參數定義改為 "indate" 和 "indate2"
- [x] 1.5 修改 WoTimeSheetController.cs 的 EdList() 方法：將第 160 行的 @date1 改為 @indate
- [x] 1.6 修改 WoTimeSheetController.cs 的 EdList() 方法：將第 166 行的 @date2 改為 @indate2
- [x] 1.7 修改 WoTimeSheetController.cs 的 EdList() 方法：將第 171-172 行的 @startDate/@endDate 改為 @indate/@indate2
- [x] 1.8 修改 WoTimeSheetController.cs 的 EdList() 方法：將第 180-182 行的參數定義改為 "indate" 和 "indate2"
- [x] 1.9 編譯 WoTimeSheetController.cs，確認無編譯錯誤

## 2. 配置 Serilog 日誌寫入

- [x] 2.1 檢查 appsettings.json 中是否已配置 Serilog（查看 CLAUDE.md 規範）
- [x] 2.2 在 appsettings.json 中添加 File Sink 配置，指定路徑為 "Logs/WoTimeSheet-{Date}.txt"
- [x] 2.3 配置輸出模板為：`{Timestamp:yyyy-MM-dd HH:mm:ss.fff}|{UserId}|{Department}|{Function}|{Status}|{Duration}|{ClientIP}{NewLine}`（修正：原模板使用 `{Properties[X]}` 語法無效，Serilog outputTemplate 應直接使用 `{PropertyName}`）
- [x] 2.4 驗證 Logs/ 目錄存在，若不存在則創建
- [x] 2.5 驗證 Serilog.Core 和 Serilog.Sinks.File NuGet 套件已安裝
- [x] 2.6 修正 Web.config assembly binding redirect：新增 Serilog 版本重定向（`0.0.0.0-4.3.0.0` → `4.3.0.0`），解決 Serilog.Sinks.File 7.0.0 參考 Serilog 4.2.0.0 而實際 DLL 為 4.3.0.0 的版本不符問題

## 3. 在 WoTimeSheet 功能加入操作頻率記錄

- [x] 3.1 在 WoTimeSheetController.cs 的 WoTS() POST 方法末尾添加日誌記錄代碼（使用 Stopwatch 測量耗時，記錄執行狀態）
- [x] 3.2 在 WoTimeSheetController.cs 的 StTS() POST 方法末尾添加日誌記錄代碼
- [x] 3.3 在 WoTimeSheetController.cs 的 EdList() POST 方法末尾添加日誌記錄代碼
- [x] 3.4 在 WoTimeSheetController.cs 的 ExportExcel() 方法末尾添加日誌記錄代碼
- [x] 3.5 在 WoTimeSheetController.cs 的 ExportEditExcel() 方法末尾添加日誌記錄代碼
- [x] 3.6 驗證日誌記錄代碼中能正確從 Session["Member"] 讀取使用者 ID 和部門（修正：原實作使用 `dynamic` 型別及錯誤屬性名，改為 `Session["Member"] as MemberViewModels`，使用 `fUserId` 取得使用者帳號、`UDeptNo` 取得部門代碼；補上 `using ESIntegrateSys.ViewModels`）
- [x] 3.7 驗證日誌記錄代碼中能正確捕捉客戶端 IP 地址（Request.UserHostAddress）
- [x] 3.8 在 Global.asax.cs ConfigureSerilog() 加入 Serilog Filter，使用 `.Filter.ByIncludingOnly()` 同時排除兩類非目標記錄：(1) 無 `Function` 屬性的啟動期 log（來自 Hangfire 等）；(2) `Department` 屬性值為 `"IT"` 的開發人員操作
- [x] 3.9 編譯 WoTimeSheetController.cs 與 Global.asax.cs，確認無編譯錯誤

## 4. 測試驗證（2026-06-09 ~ 2026-06-10）

- [x] 4.1 開啟本機開發環境，編譯整個專案（見 TEST_GUIDE.md）
- [x] 4.2 使用測試帳號登入系統（確保 Session["Member"] 正確設定）
- [x] 4.3 測試場景 1：執行 WoTS 查詢，填寫起日和結束日，驗證結果頁面返回 200 OK（無 500 錯誤）
- [x] 4.4 測試場景 2：執行 WoTS 查詢，只填起日，驗證結果頁面返回 200 OK
- [x] 4.5 測試場景 3：執行 WoTS 查詢，只填結束日，驗證結果頁面返回 200 OK
- [x] 4.6 測試場景 4：順序執行 WoTS、StTS、ExportExcel 操作，驗證功能正常
- [x] 4.7 檢查 Logs/ 目錄，確認日誌檔案 Logs/WoTimeSheet-{今天日期}.txt 已建立
- [x] 4.8 打開日誌檔案，驗證每一行的格式正確：Timestamp|UserId|Department|Function|Status|Duration|IP
- [x] 4.9 驗證日誌中包含所有執行的操作（WoTS、StTS、ExportExcel 各一筆）
- [x] 4.10 測試異常情況：手動註解掉部分 SQL 參數，驗證日誌是否記錄 ERROR 狀態
- [x] 4.11 驗證多執行緒並發寫入時，日誌檔案無亂碼或斷行

## 5. 準備正式觀察期

- [ ] 5.1 刪除 2026-06-09 ~ 2026-06-10 的測試日誌檔案，保留乾淨的 Logs/ 目錄
- [ ] 5.2 確認所有代碼修改已提交到版本控制（git commit）
- [ ] 5.3 部署修改後的代碼到生產環境或測試環境
- [ ] 5.4 驗證部署後 WoTimeSheet 功能正常運作
- [ ] 5.5 確認 2026-06-10 晚上的部署已完成，系統準備好在 2026-06-11 開始正式記錄

## 6. 觀察期及後續（2026-06-11 ~ 2026-09-10）

- [ ] 6.1 2026-06-11 開始，系統自動記錄所有 WoTimeSheet 操作，無需人工介入（運作中）
- [ ] 6.2 定期檢查 Logs/ 目錄，確認日誌檔案持續產生（每週檢查一次）
- [ ] 6.3 2026-09-09 晚上停止記錄（系統自動進行）
- [ ] 6.4 2026-09-10 及之後，準備 Python/PowerShell 數據分析腳本
- [ ] 6.5 運行數據分析腳本：清洗日誌、過濾 IT 部門、計算使用者覆蓋率
- [ ] 6.6 生成分析報告，根據使用者覆蓋率結果（< 50% 或 >= 50%）決定是否保留 WoTimeSheet 功能
