## 1. 提取焦點邏輯方法

- [x] 1.1 在 Vue methods 中新增 `focusOnScanInput()` 方法
- [x] 1.2 驗證方法正確執行 `this.$nextTick()` 和焦點設定
- [x] 1.3 確保在多個環境下都能正確執行（包括 disabled 元素的焦點行為）

## 2. 替換焦點邏輯重複程式碼

- [x] 2.1 在 `queryGun()` success 回調中，替換焦點邏輯為 `this.focusOnScanInput()`
- [x] 2.2 在 `queryGun()` error 回調中，替換焦點邏輯為 `this.focusOnScanInput()`
- [x] 2.3 在 `handleClear()` 方法結尾，替換焦點邏輯為 `this.focusOnScanInput()`
- [x] 2.4 在 `queryGun()` all_completed 情況的 then 回調中，替換焦點邏輯為 `this.focusOnScanInput()`
- [x] 2.5 驗證所有 4 處都已替換成新方法，保持原有行為

## 3. 統一 MaintenanceResult 型別檢查

- [x] 3.1 檢查後端 API 回傳的 `MaintenanceResult` 值型別（字串或數字）
- [x] 3.2 確保所有下拉選項在 `v-model` 中的值皆為字串型別
- [x] 3.3 在 `handleSubmit()` 驗證中，將 `=== '99' || === 99` 簡化為 `=== '99'`
- [x] 3.4 如需要，在資料綁定時添加型別轉換邏輯（例如 String()）
- [x] 3.5 驗證「其他原因」的條件判斷正確無誤

## 4. 驗證與測試

- [x] 4.1 手動測試掃碼流程 — 驗證焦點正確設定，無遺漏
- [x] 4.2 手動測試選擇「其他原因」(99) 的流程，驗證輸入框顯示與提交正確
- [x] 4.3 測試清空表單與重新開始流程，驗證焦點與狀態重置正確
- [x] 4.4 驗證無 console 錯誤或警告訊息
- [x] 4.5 檢查 Vue DevTools 中的資料綁定正確性
