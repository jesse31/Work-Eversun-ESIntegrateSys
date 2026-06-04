# Select2 移除與原生 select 簡化

## Why

掃碼維修 MVP 中 Select2 下拉選單在生產環境經過 2 次迭代修正仍持續出現初始化失敗（`TypeError: $(...).select2 is not a function`）。經過詳細分析，Select2 在當前環境中存在根本性問題，無法穩定工作。**策略調整**：放棄 Select2，改用瀏覽器原生的 HTML <select> 標籤，確保功能穩定可靠。

## What Changes

- **完全移除 Select2 邏輯** - 刪除所有非同步輪詢、初始化檢查、防禦邏輯、CDN 引入
- **使用原生 HTML select** - 改用瀏覽器原生 <select> 標籤，無外部依賴風險
- **簡化 AJAX 加載** - 純淨的 AJAX → Vue v-model 綁定流程
- **降低維護成本** - 消除 ~120 行複雜代碼，提高可讀性

## Capabilities

### Removed Capabilities
- `select2-async-resilient-initialization`: 放棄 Promise-based 輪詢方案（已棄用）

### Modified Capabilities
- `scan-repair-barcode-mvp`: 簡化 ScanRepair.cshtml 中的下拉選單實作，改用原生 <select> + v-model

## Impact

**修改檔案**:
- `ESIntegrateSys/Views/MaterialGun/ScanRepair.cshtml` - 移除 Select2 邏輯，簡化 mounted()、loadDropdownOptions()

**影響範圍**:
- 前端使用者體驗 - 原生 select 更加穩定可靠，消除初始化失敗的風險
- 代碼複雜性 - 大幅簡化，維護成本降低
- 功能完整性 - 維修分類與原因選擇功能保持完整，無損失

**無破壞性改變** - 原有邏輯架構保留，僅替換下拉選單實作方式；降級為功能簡化但功能完整
