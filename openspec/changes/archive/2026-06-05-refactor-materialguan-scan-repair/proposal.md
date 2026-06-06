## Why

現有 MaterialGunScanRepair.cshtml 存在程式碼重複與型別檢查不一致的問題，增加維護成本與潛在 Bug 風險。整理這些技術債務能提升程式碼品質與可讀性。

## What Changes

- 提取重複的焦點重設邏輯（4 處）為單一方法 `focusOnScanInput()`，統一焦點管理
- 統一 `MaintenanceResult` 資料型別為字串，消除型別檢查的冗餘判斷（目前同時檢查字串和數字 '99'、99）

## Capabilities

### New Capabilities

<!-- None - this is code refactoring only -->

### Modified Capabilities

<!-- None - this is implementation-level refactoring with no specification-level behavior changes -->

## Impact

- 影響檔案：`ESIntegrateSys/Views/MaterialGun/MaterialGunScanRepair.cshtml`
- 無 API 變更、無依賴變更
- 100% 向後相容，無使用者可見變更
