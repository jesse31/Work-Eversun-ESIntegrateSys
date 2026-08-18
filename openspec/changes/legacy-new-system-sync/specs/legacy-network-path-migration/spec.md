## ADDED Requirements

### Requirement: 既有本地檔案完整遷移至網路共享路徑
系統上線前，SHALL 將舊系統本地路徑（`D:\...\UploadedFiles`）下所有既有附件檔案完整複製到網路共享路徑（`\\192.168.4.35\EIS_Files\QS`），且遷移後檔案數量與內容 SHALL 與來源一致。

#### Scenario: 遷移前核對檔案清單
- **WHEN** 執行遷移作業前
- **THEN** 產生本地路徑下所有檔案的清單（檔名、大小）作為遷移基準

#### Scenario: 遷移後驗證檔案完整性
- **WHEN** 檔案複製到網路路徑完成後
- **THEN** 網路路徑下的檔案數量與檔名 SHALL 與遷移前清單一致
- **AND** 任何缺漏或損毀的檔案 SHALL 被記錄並在部署前修正

### Requirement: 資料庫 FilePath 欄位批次更新為檔名
`ES_QuoteUploadFiles` 表中所有既有記錄的 `FilePath` 欄位，SHALL 由完整本地路徑更新為僅含檔名，以符合新的路徑組合邏輯。

#### Scenario: 執行遷移前備份
- **WHEN** 執行 SQL 更新語句之前
- **THEN** 系統管理者 SHALL 先備份 `ES_QuoteUploadFiles` 整張表

#### Scenario: 批次更新 FilePath 欄位
- **WHEN** 執行遷移 SQL，篩選條件為 `FilePath` 包含舊本地路徑前綴
- **THEN** 該欄位值 SHALL 被更新為僅含檔名（與 `FileName` 欄位內容一致）
- **AND** 更新筆數 SHALL 與符合篩選條件的記錄數一致，不多不少

### Requirement: 遷移作業於離峰時段執行並可回滾
檔案與資料庫遷移作業 SHALL 在離峰時段執行，且若驗證失敗，SHALL 能透過設定檔切換回舊的本地路徑模式以回滾。

#### Scenario: 回滾至本地路徑
- **WHEN** 遷移後驗證發現嚴重問題（例如大量檔案遺失或路徑無法存取）
- **THEN** 系統管理者可將 `Web.config` 的 `FileUploadPath` 設定值改回本地路徑
- **AND** 資料庫可由遷移前備份還原 `FilePath` 欄位內容
