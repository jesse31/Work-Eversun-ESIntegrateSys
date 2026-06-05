## 1. 移除 Select2 邏輯

- [x] 1.1 刪除 `ensureSelect2Ready()` 非同步輪詢方法
- [x] 1.2 刪除 `initializeSelect2Element()` 安全初始化方法
- [x] 1.3 刪除 `mounted()` 中的 `setTimeout(async, 0)` 非同步啟動邏輯
- [x] 1.4 刪除 `select2Available` 資料屬性

## 2. 簡化 mounted() 生命週期

- [x] 2.1 `mounted()` 直接焦點設定在掃碼輸入框
- [x] 2.2 移除非同步啟動邏輯，直接呼叫 `loadDropdownOptions()`
- [x] 2.3 驗證 mounted 邏輯簡潔清晰（不超過 3 行）

## 3. 簡化 loadDropdownOptions()

- [x] 3.1 移除 Classification AJAX success callback 中的守衛檢查
- [x] 3.2 移除 MResult AJAX success callback 中的守衛檢查
- [x] 3.3 純粹加載選項到 classificationOptions、maintenanceResultOptions
- [x] 3.4 刪除所有 Select2 初始化程式碼（`$('#Classification').select2(...)`）
- [x] 3.5 刪除 change 事件的 Select2 相關邏輯

## 4. 移除 Select2 CDN

- [x] 4.1 移除 `<head>` 中的 Select2 CSS CDN
- [x] 4.2 移除 `<head>` 中的 Select2 JS CDN

## 5. 驗證原生 HTML select

- [x] 5.1 確認 `<select id="Classification">` 標籤保留且正確
- [x] 5.2 確認 `<select id="MaintenanceResult">` 標籤保留且正確
- [x] 5.3 驗證 v-model="formData.Classification" 綁定正常
- [x] 5.4 驗證 v-model="formData.MaintenanceResult" 綁定正常

## 8. 修正綁定字段名稱（新發現的問題）

- [x] 8.1 修正 Classification v-for 綁定：item.code → item.Value，item.name → item.Text
- [x] 8.2 修正 MaintenanceResult v-for 綁定：item.code → item.Value，item.name → item.Text
- [x] 8.3 驗證修正後下拉選項正確顯示
- [x] 8.4 確認 v-model 綁定仍正常運作

## 6. 集成與測試

- [ ] 6.1 編譯方案，驗證無 JavaScript 錯誤 ⏳ 詳見 MANUAL_TEST_PLAN.md
- [ ] 6.2 頁面載入測試 - 驗證原生 select 顯示 ⏳ 詳見 MANUAL_TEST_PLAN.md
- [ ] 6.3 掃碼成功流程 - 驗證帶出料槍資訊，原生 select 可用 ⏳ 詳見 MANUAL_TEST_PLAN.md
- [ ] 6.4 選擇分類 - 驗證原生 select 可正常選擇，formData 同步 ⏳ 詳見 MANUAL_TEST_PLAN.md
- [ ] 6.5 選擇維修原因 - 驗證原生 select 可正常選擇，若選「99」則顯示「其他原因」輸入框 ⏳ 詳見 MANUAL_TEST_PLAN.md
- [ ] 6.6 已完修流程 - 掃碼已完修料槍，驗證彈警告且表單隱藏 ⏳ 詳見 MANUAL_TEST_PLAN.md
- [ ] 6.7 表單提交 - 驗證驗證通過後提交成功 ⏳ 詳見 MANUAL_TEST_PLAN.md

## 7. 文件與驗收

- [x] 7.1 刪除或歸檔舊的設計文檔（優雅降級相關） ✅ 已更新 proposal.md
- [x] 7.2 更新 design.md 為新決策（移除 Select2） ✅ 已確認
- [x] 7.3 建立新的驗收報告（簡化方案） ✅ VERIFICATION_REPORT.md 已建立
- [ ] 7.4 驗證所有 7 項測試均通過 ⏳ 詳見 MANUAL_TEST_PLAN.md
