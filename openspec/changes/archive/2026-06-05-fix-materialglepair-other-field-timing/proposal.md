# Fix MaterialGunRepair Other Field Display Timing Issue

## Why

當用戶編輯既存的料槍維修紀錄（`/MaterialGun/MaterialGunRepair?sno=1783`）且 `MaintenanceResult = 99`（"其他"）時，頁面初次載入時 `<input id="Other">` 欄位不顯示，導致前端防呆檢查攔截用戶提交。用戶必須重新選擇下拉選單才能觸發顯示邏輯，造成使用體驗不佳。

根本原因：AJAX 動態載入下拉選項後，使用 `.val()` 設定選中值，但未觸發 `change` 事件，導致依賴該事件的 UI 顯示邏輯無法執行。這是典型的**事件時序問題**。

## What Changes

- 重構 MaterialGunRepair.cshtml 中的下拉選單邏輯，將 UI 顯示邏輯抽取為獨立函式 `toggleOtherField()`
- 在 AJAX 成功載入選項後主動調用該函式，確保初始化時 Other 欄位正確顯示
- 改進型別比較，使用嚴格相等 `===` 而非寬鬆相等 `==`，提升程式碼品質

## Capabilities

### New Capabilities

- `ui-field-visibility-toggle`: 支援基於下拉選單選項值動態顯示/隱藏相關表單欄位的通用機制，初始化時和用戶選擇時均能正確響應

### Modified Capabilities

- `maintenance-result-selection`: 修改既存的「檢修不良原因」下拉選單交互邏輯，使其在 AJAX 初始化時也能正確觸發 Other 欄位顯示

## Impact

- **影響檔案**：`Views\MaterialGun\MaterialGunRepair.cshtml`
- **影響功能**：料槍維修編輯作業（MaterialGunRepair 頁面）
- **使用者影響**：修復使用者體驗問題，消除重複選擇的不便
- **相依性**：無（純 UI/JavaScript 修復，不涉及 Controller 或資料庫邏輯）
- **破壞性變更**：無

---

## 驗收標準

✓ 頁面初次載入時，如果 MaintenanceResult = 99，Other 欄位應自動顯示  
✓ 用戶選擇不同的檢修不良原因時，Other 欄位應正確隱藏/顯示  
✓ 再次選擇 99 時，Other 欄位應再次顯示  
✓ 前端防呆檢查應正常通過（無誤判）
