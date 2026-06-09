## ADDED Requirements

### Requirement: 記錄 WoTimeSheet 功能的操作頻率
系統應該將每次 WoTimeSheet 查詢和匯出操作記錄到結構化文本檔案中，捕捉使用者身份、部門、功能名稱、執行狀態、執行耗時和客戶端 IP 地址。這些日誌應該按日期輪轉，存放在 `Logs/` 目錄，並採用標準化格式，以支援觀察期結束後的使用者覆蓋率和功能使用率分析。

#### Scenario: 成功的查詢操作記錄
- **WHEN** 使用者執行 WoTimeSheet 查詢操作（WoTS POST、StTS POST 或 EdList POST）
- **THEN** 系統應該將一筆日誌條目寫入 `Log/WoTimeSheet-{Date}.txt`，格式為：`Timestamp|UserId|Department|/WoTimeSheet/FunctionName|SUCCESS|DurationMs|ClientIP`

#### Scenario: 失敗的查詢操作記錄
- **WHEN** WoTimeSheet 查詢操作遇到錯誤或異常
- **THEN** 系統應該將一筆日誌條目寫入 `Log/WoTimeSheet-{Date}.txt`，格式為：`Timestamp|UserId|Department|/WoTimeSheet/FunctionName|ERROR|DurationMs|ClientIP`

#### Scenario: 匯出操作記錄
- **WHEN** 使用者執行 WoTimeSheet 匯出操作（ExportExcel 或 ExportEditExcel）
- **THEN** 系統應該以與查詢操作相同的格式寫入日誌條目，捕捉匯出功能名稱和執行結果

#### Scenario: 排除非操作記錄
- **WHEN** 應用程式啟動時（Hangfire 初始化、DI 容器建立等）觸發 Serilog 寫入
- **THEN** 這些非使用者操作的記錄不應該寫入日誌檔案；只有透過 `LogOperationFrequency` 設定了 `Function` 屬性的 log 事件才應被寫入

#### Scenario: 日誌檔案按日期輪轉
- **WHEN** 系統從一個日曆日期過渡到下一個日期
- **THEN** 應該為新日期建立新的日誌檔案（例如：從 `WoTimeSheet-2026-06-09.txt` 轉為 `WoTimeSheet-2026-06-10.txt`）

#### Scenario: 從 Session 識別使用者
- **WHEN** 記錄 WoTimeSheet 操作時
- **THEN** 系統應該將 `Session["Member"]` 強制轉型為 `MemberViewModels`，從 `fUserId` 欄位取得使用者帳號、從 `UDeptNo` 欄位取得部門代碼；如果 Session["Member"] 為 null，系統應該將兩個欄位都記為 "UNKNOWN"

#### Scenario: 執行耗時測量
- **WHEN** 記錄 WoTimeSheet 操作時
- **THEN** 系統應該使用 Stopwatch 測量操作耗時，從操作邏輯之前開始，到完成後結束（包括任何資料庫或外部呼叫），並以毫秒為單位記錄耗時

#### Scenario: 客戶端 IP 捕捉
- **WHEN** 記錄 WoTimeSheet 操作時
- **THEN** 系統應該從 `Request.UserHostAddress` 捕捉客戶端 IP 地址，並將其包含在日誌條目中

---

### Requirement: 支援 90 天觀察期的持續穩定記錄
系統應該在連續的 90 天觀察期（2026-06-11 至 2026-09-09）內可靠地記錄所有 WoTimeSheet 操作，不遺漏數據，確保每日日誌檔案累積以支援後續的使用者覆蓋率分析。

#### Scenario: 90 天期間的持續記錄
- **WHEN** 系統從 2026-06-11 00:00:00 至 2026-09-09 23:59:59 運作期間
- **THEN** 觀察期內的每一天應該存在一筆日誌檔案，每個檔案應該包含該日期執行的所有 WoTimeSheet 操作

#### Scenario: 觀察期間無數據遺漏
- **WHEN** WoTimeSheet 模組在整個觀察期內被使用
- **THEN** 所有使用者初始化的查詢和匯出操作應該被記錄；不應該因為檔案鎖定、並發寫入或應用程式錯誤而遺漏任何日誌條目

#### Scenario: 日誌檔案持久化
- **WHEN** 觀察期在 2026-09-09 結束
- **THEN** 2026-06-11 至 2026-09-09 的所有日誌檔案應該保持在 `Logs/` 目錄中，保持不變以供後續數據提取和分析

---

### Requirement: 在寫入層排除開發人員記錄
日誌系統應該在寫入階段就排除開發團隊（部門 = 'IT'）的操作記錄，確保日誌檔案中只包含真正的業務使用者活動，無需在後續分析階段額外過濾。

#### Scenario: 日誌條目包含部門資訊
- **WHEN** 記錄 WoTimeSheet 操作時
- **THEN** 日誌條目應該包含從 `MemberViewModels.UDeptNo` 提取的使用者部門代碼，或如果無法取得則為 "UNKNOWN"

#### Scenario: 寫入層排除 IT 部門操作
- **WHEN** `MemberViewModels.UDeptNo` 為 `"IT"` 的使用者執行 WoTimeSheet 操作時
- **THEN** 該操作記錄不應該被寫入日誌檔案；IT 部門的過濾在 Serilog Filter 層執行，而非在後續分析腳本中處理

---

### Requirement: 支援使用者覆蓋率指標的計算
日誌數據應該能夠支援計算使用者覆蓋率（在觀察期內使用過 WoTimeSheet 的活躍業務使用者百分比），以便在觀察期結束後評估該功能是否符合 50% 覆蓋率閾值。

#### Scenario: 識別觀察期內的不重複使用者
- **WHEN** 分析 90 天觀察期的日誌檔案時
- **THEN** 數據分析腳本應該能夠提取所有不重複的使用者 ID（排除 IT 部門）並計數，以確定使用過 WoTimeSheet 的業務使用者總數

#### Scenario: 計算系統活躍業務使用者總數
- **WHEN** 分析 90 天觀察期的日誌檔案時
- **THEN** 數據分析腳本應該能夠提取所有系統操作中的不重複使用者 ID（排除 IT 部門），以確定活躍業務使用者的總人口

#### Scenario: 計算使用者覆蓋率百分比
- **WHEN** 觀察期結束後
- **THEN** 數據分析腳本應該計算使用者覆蓋率為：`(WoTimeSheet 不重複使用者數 / 活躍業務使用者總數) × 100%`，並與 50% 閾值進行比較，以決定是否保留該功能
