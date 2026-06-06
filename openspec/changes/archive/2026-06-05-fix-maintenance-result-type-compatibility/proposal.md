## Why

實測發現「檢修不良原因」選項值為 99 時，前端「其他原因」輸入框無法正常顯示，也不執行驗證。根本原因是後端回傳 `Value = 99`（數字型別），但前端使用 `=== '99'`（字串）進行嚴格比較，導致條件判斷失敗。需要在前端實作相容性層，同時接受數字和字串型別。

## What Changes

- 新增 `isOtherReason()` 方法，集中管理「其他原因」判斷邏輯，相容 `99` 和 `'99'` 兩種型別
- 在 v-if 條件、驗證邏輯、@change 回調中統一使用新方法
- 提升程式碼可讀性，避免散落的型別檢查邏輯

## Capabilities

### New Capabilities

- `maintenance-result-type-compatibility`: 前端相容層，統一處理後端回傳的數字或字串 MaintenanceResult 值

### Modified Capabilities

<!-- None - this is implementation-level type compatibility fix -->

## Impact

- 影響檔案：`ESIntegrateSys/Views/MaterialGun/MaterialGunScanRepair.cshtml`
- 無 API 變更、無資料庫變更
- 向後相容，修復既有功能缺陷
