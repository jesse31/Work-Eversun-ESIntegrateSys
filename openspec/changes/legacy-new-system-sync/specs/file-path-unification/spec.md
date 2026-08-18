## ADDED Requirements

### Requirement: 舊系統檔案上傳使用網路共享路徑
舊系統（ESIntegrateSys）的 QuoteSchedule 附件上傳功能 SHALL 將檔案儲存至網路共享路徑 `\\192.168.4.35\EIS_Files\QS`，且該路徑 SHALL 從 `Web.config` 的 `<appSettings>` 讀取，不得寫死於程式碼中。

#### Scenario: 業務或 IE 上傳新附件
- **WHEN** 使用者在舊系統的報價單附件上傳表單提交檔案
- **THEN** 檔案被寫入 `\\192.168.4.35\EIS_Files\QS` 路徑下
- **AND** 資料庫 `ES_QuoteUploadFiles.FilePath` 欄位僅記錄檔名，不含目錄路徑

#### Scenario: 基礎路徑透過設定檔配置
- **WHEN** 系統需要組合檔案完整路徑以進行讀寫
- **THEN** 系統從 `Web.config` 的 `FileUploadPath` 設定值取得基礎路徑
- **AND** 使用 `Path.Combine(基礎路徑, 檔名)` 組合完整路徑

### Requirement: 舊系統檔案下載自網路共享路徑讀取
舊系統的附件下載功能 SHALL 依據資料庫中記錄的檔名，動態組合網路共享路徑後讀取檔案內容。

#### Scenario: 使用者下載既有附件
- **WHEN** 使用者點擊下載連結，系統依 `sno` 查得對應的 `ES_QuoteUploadFiles` 記錄
- **THEN** 系統以設定檔中的基礎路徑與該記錄的檔名組合出完整路徑
- **AND** 從該路徑讀取檔案並回傳供下載

#### Scenario: 網路路徑不可存取時的錯誤處理
- **WHEN** 網路共享路徑因故無法存取（斷線、權限不足）
- **THEN** 系統捕捉例外並記錄錯誤日誌（含時間戳記、錯誤訊息）
- **AND** 回傳明確的錯誤訊息給使用者，不得讓例外未處理導致頁面崩潰

### Requirement: 新舊系統檔案可互相查看
業務與 IE 在舊系統上傳的附件，SHALL 可在新系統（EIS.SubSystem.QS）中被查詢與下載；反之新系統上傳的附件也 SHALL 可在舊系統中被查詢與下載，因為兩系統共用同一實體儲存路徑。

#### Scenario: 舊系統上傳後新系統可見
- **WHEN** 業務在舊系統上傳報價單 A 的附件
- **THEN** IE 在新系統查詢報價單 A 時，可看到該附件並成功下載

#### Scenario: 新系統上傳後舊系統可見
- **WHEN** IE 在新系統上傳報價單 B 的附件
- **THEN** 業務在舊系統查詢報價單 B 時，可看到該附件並成功下載

### Requirement: 測試環境與正式環境檔案路徑須隔離
`FileUploadPath` 設定 SHALL 依環境區分為測試路徑（`\\192.168.4.35\EIS_Files\QS_Test`）與正式路徑（`\\192.168.4.35\EIS_Files\QS`），比照既有 `connectionStrings` 的維運模式於 `Web.config` 內註解切換，測試環境操作不得寫入或刪除正式路徑下的檔案。

#### Scenario: 測試環境使用測試路徑
- **WHEN** 系統以測試環境設定運作（`FileUploadPath` 指向 `QS_Test`）
- **THEN** 所有上傳、下載操作僅作用於 `\\192.168.4.35\EIS_Files\QS_Test`
- **AND** 不影響 `\\192.168.4.35\EIS_Files\QS` 下的正式檔案

#### Scenario: 部署正式環境前確認路徑已切換
- **WHEN** 系統管理者準備部署至正式環境
- **THEN** SHALL 於部署 checklist 中確認 `Web.config` 的 `FileUploadPath` 已切換為正式路徑（`QS`），而非測試路徑（`QS_Test`）
