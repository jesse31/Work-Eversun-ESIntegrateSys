# 設計文件：新舊系統並行上線 - 檔案路徑統一方案

## Context

舊系統（ESIntegrateSys，ASP.NET MVC）已上線多年，QuoteSchedule 模組的檔案上傳功能將檔案儲存在應用程式本地磁碟（`Server.MapPath("~/UploadedFiles")`），並在資料庫 `ES_QuoteUploadFiles.FilePath` 欄位存放完整絕對路徑。

新系統（EIS.SubSystem.QS，位於 `D:\專案\企業級整合系統\EIS\EIS\01_Presentation\EIS.SubSystem.QS`）目前已將 QuoteSchedule 對應模組移植過去，檔案設計為存放在網路共享路徑 `\\192.168.4.35\EIS_Files\QS`，資料庫僅存檔名，讀取時由後端動態組合基礎路徑 + 檔名。

由於兩套系統將並行上線（依功能模組分配使用者入口，QuoteSchedule 相關功能新舊系統皆保留），業務與 IE 上傳的附件必須在兩邊系統都能看到一致的結果。目前路徑與資料庫欄位語意不同，是本次變更要解決的核心問題。

**限制與約束：**
- 舊系統為 .NET Framework MVC5（非 .NET 8），沿用現有 Entity Framework 存取方式，不做架構重構（依 CLAUDE.md「既有系統相容與技術沿用規範」）
- 修改範圍僅限 QuoteSchedule 檔案上傳/下載相關程式碼，不涉及其他模組
- 不可中斷既有使用者的檔案存取能力

## Goals / Non-Goals

**Goals:**
- 舊系統檔案上傳/下載改為使用網路共享路徑 `\\192.168.4.35\EIS_Files\QS`
- 舊系統資料庫欄位語意與新系統對齊（僅存檔名，動態組合路徑）
- 既有檔案完整遷移，遷移後新舊系統皆可讀取
- 基礎路徑改為可配置（Web.config），不寫死於程式碼

**Non-Goals:**
- 不重構舊系統的整體架構或上傳/下載流程之外的邏輯
- 不處理新系統程式碼變更（新系統設計已符合目標狀態）
- 不建立即時雙向檔案同步機制（採用「共用同一路徑」而非「同步兩份檔案」）
- 不改動 `ES_QuoteUploadFiles` 表結構（不新增/刪除欄位，僅改變 FilePath 欄位存放內容）

## Decisions

### 決策 1：共用同一網路路徑，而非建立同步機制
**選擇**：舊系統直接改為讀寫 `\\192.168.4.35\EIS_Files\QS`，與新系統共用同一存儲位置。
**替代方案**：建立同步服務，定時將舊系統本地檔案複製到網路路徑。
**理由**：同步機制引入額外的延遲與失敗點（複製失敗、時間差造成的暫時不一致），共用路徑架構更簡單、無延遲、資料單一來源。

### 決策 2：FilePath 欄位改存檔名，而非新增欄位
**選擇**：直接改變 `ES_QuoteUploadFiles.FilePath` 的語意，使其只存檔名；不新增 `FileName2` 或其他欄位。
**替代方案**：新增獨立欄位存檔名，保留 FilePath 相容舊資料。
**理由**：`FileName` 欄位已存在且目前皆與 FilePath 對應之檔案同名，直接複寫最單純，避免程式碼中出現「該讀哪個欄位」的分支判斷，降低維護成本。

### 決策 3：基礎路徑放在 Web.config `<appSettings>`
**選擇**：新增 `FileUploadPath` 設定於 `Web.config`，程式碼透過 `ConfigurationManager.AppSettings["FileUploadPath"]` 讀取。
**替代方案**：寫死於程式碼常數。
**理由**：符合 CLAUDE.md「禁止 Hardcode」精神的延伸（雖非密碼/金鑰，但路徑异动本就该走配置），未來若網路路徑異動，只需改設定檔不需重新編譯。

### 決策 4：先遷移實體檔案，再批次更新資料庫
**選擇**：Step 1 複製實體檔案到新路徑 → Step 2 執行 SQL UPDATE 改寫 FilePath → Step 3 部署程式碼。
**替代方案**：先改程式碼與資料庫，檔案遷移用背景工作慢慢處理。
**理由**：若先切程式碼但檔案尚未搬移完成，會造成下載 404。檔案優先落地，可用腳本先行核對數量與雜湊值再動資料庫，降低風險。

### 決策 5：測試／正式路徑比照既有 connectionStrings 模式，於 Web.config 內註解切換
**選擇**：`FileUploadPath` 在 Web.config 中比照現有 `connectionStrings` 區塊的既有作法——測試路徑（`\\192.168.4.35\EIS_Files\QS_Test`）與正式路徑（`\\192.168.4.35\EIS_Files\QS`）並列，正式環境那行預設用註解隱藏，部署正式環境前由人工切換註解。
**替代方案**：改用 `Web.Release.config` 的 XDT Transform，於 Release 建置時自動替換設定值。
**理由**：專案目前並未真正使用 XDT Transform 自動切換（`Web.Release.config`/`Web.Debug.config` 僅為範本註解，實際部署靠人工複製 Web.config），為避免引入新的部署機制、維持與既有 connectionStrings 一致的維運習慣，採用相同的手動註解切換模式。
**已知風險**：人工切換仍可能忘記，尤其是本次變更的起因正是「測試操作誤動到正式檔案」；此風險已記錄於下方 Risks / Trade-offs。

## Risks / Trade-offs

- **[風險] 網路共享路徑無法存取（斷線、權限問題）** → 部署前需以服務帳號（IIS App Pool Identity）實際測試讀寫權限；建議程式碼中對 I/O 例外做明確錯誤訊息與記錄（沿用既有 try/catch + log 模式），不新增複雜重試機制（避免過度設計）
- **[風險] SQL UPDATE 誤改非 UploadedFiles 相關的資料** → UPDATE 前先用 SELECT 確認影響筆數與範圍（`WHERE FilePath LIKE 'D:\%UploadedFiles%'`），並於執行前備份整張表
- **[風險] 遷移期間有使用者正在上傳/下載** → 選在離峰時段執行，並提前對業務/IE發布正式停機公告，執行窗口內舊系統上傳功能暫時停用或提示維護中
- **[Trade-off] 舊系統從此依賴網路共享，可用性受 NAS/網路狀況影響** → 這是「共用路徑而非同步」方案的必然代價，換取架構簡單與資料一致性；若日後 NAS 穩定性成為問題，可再評估同步方案
- **[風險] Web.config 內測試/正式路徑手動切換註解，部署時可能忘記切換，導致測試操作誤寫/誤刪正式檔案** → 部署正式環境前，於 checklist 中明確列出「確認 FileUploadPath 已切換為正式路徑」步驟；比照現有 connectionStrings 的既有維運流程，非本次變更新增的風險，但仍需在部署 SOP 中特別標註

## Migration Plan

1. **準備**：備份 `ES_QuoteUploadFiles` 表、備份 `D:\...\UploadedFiles` 目錄；確認 IIS 服務帳號對 `\\192.168.4.35\EIS_Files\QS` 有讀寫權限
2. **實體檔案遷移**：將本地 `UploadedFiles` 目錄下所有檔案複製到網路路徑，核對檔案數量與檔名一致
3. **資料庫遷移**：執行 SQL，將 `FilePath` 欄位內容改為檔名（`SET FilePath = FileName`），先在測試環境驗證
4. **程式碼部署**：
   - Web.config 新增 `FileUploadPath` 設定
   - `QuoteScheduleController.Upload()` / `Download()` / 相關方法改用設定值 + `Path.Combine`
5. **驗證**：舊系統上傳新檔案 → 確認落於網路路徑；新系統查詢同一報價單 → 確認可見；舊系統下載既有（已遷移）檔案 → 確認正常
6. **回滾策略**：若驗證失敗，程式碼可透過 Web.config 設定值切回本地路徑（`D:\...\UploadedFiles`）；資料庫遷移前的備份可還原 FilePath 欄位

## Open Questions（已確認）

- 遷移執行窗口需正式對業務/IE發布停機公告，提前通知使用者舊系統將於指定時間短暫維護/停用，避免遷移期間有人操作造成資料不一致。
- 舊路徑 `D:\...\UploadedFiles` 下的原始檔案不設自動清除排程；待舊系統於新路徑正常運作滿 30 天以上，由系統管理者手動確認後刪除。
