# 新舊系統並行上線 - 檔案路徑統一方案

## Why

舊系統（ESIntegrateSys）和新系統（EIS.SubSystem.QS）需要並行上線，使業務/IE 可以在兩個系統中無縫操作。目前兩個系統的檔案儲存路徑不同（舊系統：本地路徑、新系統：網路共享路徑），導致在一個系統上傳的檔案在另一個系統無法查看，造成數據不一致和使用者體驗差。

## What Changes

### 檔案儲存統一
- 舊系統檔案上傳路徑改為網路共享：`\\192.168.4.35\EIS_Files\QS`（從 `D:\...\UploadedFiles`）
- 新舊系統共用同一個網路檔案存儲位置
- 現有檔案物理遷移到網路路徑

### 代碼設計對齐
- 舊系統改為「只存檔名」的設計，與新系統一致
- `ES_QuoteUploadFiles` 表的 `FilePath` 欄位改為存檔名（不存完整路徑）
- 下載/讀取檔案時動態拼接完整路徑（基礎路徑 + 檔名）

### 配置管理
- `Web.config` 新增 `<appSettings>` 配置存儲基礎路徑
- 代碼統一從 config 讀取基礎路徑，避免硬編碼

### 數據遷移
- SQL 批量更新 `ES_QuoteUploadFiles` 表：`FilePath` 欄位改為只存檔名
- 備份舊路徑檔案後進行遷移

## Capabilities

### New Capabilities

- `file-path-unification`: 新舊系統檔案儲存路徑統一為網路共享位置，確保無論用戶在哪個系統操作，都能看到相同的檔案資料
- `legacy-network-path-migration`: 舊系統檔案從本地路徑遷移到網路共享路徑，包括物理檔案複製和數據表更新

### Modified Capabilities

- （無現有功能的需求變更，只是實作細節改動）

## Impact

### 受影響的代碼
- `ESIntegrateSys/Controllers/QuoteScheduleController.cs`
  - `Upload()` 方法：改用網路路徑
  - `Download()` 方法：改用網路路徑讀取
  - `RecordDetails()` 方法：邏輯不變，但 FilePath 資料已改為檔名
- `ESIntegrateSys/Web.config`：新增檔案路徑配置

### 受影響的數據表
- `ES_QuoteUploadFiles`：FilePath 欄位語意改變（從完整路徑改為檔名）

### 受影響的系統
- 舊系統（ESIntegrateSys）：檔案上傳/下載路徑改動
- 新系統（EIS.SubSystem.QS）：無需改動（已使用正確的路徑）

### 用戶影響
- **業務/IE**：無感知改動，檔案操作行為保持不變，但現在新舊系統看到的檔案一致
- **檔案遷移期間**：需要短暫的維護窗口（檔案複製時間取決於檔案數量和大小）

## 風險與考量

- **檔案遷移風險**：需要確保網路路徑可用且有足夠的儲存空間
- **向後相容性**：舊系統的數據表結構改變，需要完整測試
- **網路依賴性**：舊系統從此依賴網路共享，若網路故障會影響檔案操作
