## Why

WoTimeSheet 的查詢功能（WoTS、StTS、EdList）存在 SQL 參數名稱不匹配問題，導致在特定查詢條件下（只填起日或只填結束日）回傳 500 錯誤。同時，系統缺乏操作頻率監控，無法判斷這些功能是否真正被使用，造成維護成本不必要地增加。

## What Changes

- **修復 WoTimeSheet 500 錯誤**：統一 SQL 參數命名（@date1/@date2 → @indate/@indate2），確保所有查詢條件組合都能正常執行
- **加入操作頻率記錄**：在 WoTimeSheet 的所有查詢和匯出功能中加入 Serilog 日誌記錄，記錄每次操作的時間戳、使用者、部門、功能名稱、執行狀態、耗時等信息
- **按日期分割日誌**：日誌檔案自動按日期分割存放在 `Logs/` 資料夾（如 `Log/WoTimeSheet-2026-06-11.txt`）
- **3 個月觀察期**（2026-06-11 ~ 2026-09-09）：蒐集真實使用者的操作數據，過濾開發人員（IT 部門）後，計算使用者覆蓋率
- **決策標準**：若 3 個月內使用者覆蓋率 < 50%，考慮移除或優化該功能，減少維護負擔

## Capabilities

### New Capabilities
- `wotimesheet-operation-frequency-monitoring`: 記錄 WoTimeSheet 模組所有查詢和匯出操作的頻率指標，支援後續的使用者覆蓋率分析和功能決策

### Modified Capabilities
<!-- 無既有功能的需求變更 -->

## Impact

- **修改檔案**：
  - `Controllers/WoTimeSheetController.cs`：修復參數名稱、加入日誌記錄
  - `appsettings.json`：配置 Serilog File Sink
  
- **新增資源**：
  - `Logs/` 目錄：存放日誌檔案
  
- **影響範圍**：
  - WoTimeSheet 的所有查詢和匯出功能（WoTS、StTS、EdList、ExportExcel、ExportEditExcel）
  - 所有業務使用者（非 IT 部門）的操作都會被記錄和分析
  
- **依賴**：Serilog（已配置在 CLAUDE.md 規範中）
