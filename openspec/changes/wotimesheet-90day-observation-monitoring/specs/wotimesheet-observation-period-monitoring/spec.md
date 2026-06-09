## Purpose

監控和驗證 90 天觀察期（2026-06-11 至 2026-09-09）內 WoTimeSheet 操作日誌的持續生成，確保無數據遺漏。

---

## ADDED Requirements

### Requirement: 每週驗證日誌檔案產生狀態
系統監控人員應該每週檢查一次 Logs 目錄，確認日誌檔案持續按日期產生，無中斷或異常。

#### Scenario: 日誌檔案正常產生
- **WHEN** 監控人員在每週一檢查 Logs 目錄（例如 2026-06-16）
- **THEN** 應該看到過去 7 天的日誌檔案（例如 WoTimeSheet-2026-06-09.txt 至 WoTimeSheet-2026-06-16.txt）已正確產生，每個檔案包含該天的操作記錄

#### Scenario: 偵測日誌檔案中斷
- **WHEN** 監控人員發現某個預期日期的日誌檔案缺失或為空
- **THEN** 應立即記錄異常日期，調查原因（伺服器故障、磁碟滿等），並在觀察期結束時於分析報告中說明

#### Scenario: 確認 Logs 目錄權限正常
- **WHEN** 監控期間定期檢查
- **THEN** Logs 目錄應保持可讀狀態，IIS 應用程式池對目錄有寫入權限，無權限異常

---

### Requirement: 90 天持續監控週期
觀察期從 2026-06-11 開始，監控應持續至 2026-09-09，確保整個期間日誌無中斷。

#### Scenario: 觀察期開始驗證
- **WHEN** 2026-06-11 系統開始正式記錄
- **THEN** 應確認 WoTimeSheet-2026-06-11.txt 檔案已建立，首筆日誌記錄已寫入

#### Scenario: 觀察期中期檢查
- **WHEN** 在 2026-06-11 至 2026-09-09 期間內
- **THEN** 每週至少檢查一次，確認日誌持續產生，累積檔案數量符合預期

#### Scenario: 觀察期結束確認
- **WHEN** 2026-09-09 23:59:59 後
- **THEN** 應確認所有 90 天的日誌檔案已完整保存（WoTimeSheet-2026-06-11.txt 至 WoTimeSheet-2026-09-09.txt），共 91 個檔案

---

### Requirement: 日誌檔案內容驗證
監控人員應定期抽查日誌檔案內容，確認格式正確，資料完整。

#### Scenario: 驗證日誌格式
- **WHEN** 打開任意一個日誌檔案（例如 WoTimeSheet-2026-06-11.txt）
- **THEN** 應確認每一行的格式為 `Timestamp|UserId|Department|/WoTimeSheet/FunctionName|Status|Duration|ClientIP`，無亂碼或破損記錄

#### Scenario: 驗證業務使用者操作被記錄
- **WHEN** 檢查日誌檔案內容
- **THEN** 應確認業務使用者（非 IT 部門）的操作都被記錄，包括成功（SUCCESS）和失敗（ERROR）狀態

#### Scenario: 確認 IT 部門操作被排除
- **WHEN** 檢查日誌檔案
- **THEN** 應確認 Department='IT' 的操作記錄不出現在檔案中（Serilog Filter 正常運作）
