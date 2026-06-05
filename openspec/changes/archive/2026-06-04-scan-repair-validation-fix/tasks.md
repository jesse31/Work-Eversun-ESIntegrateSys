## 1. 後端修正 — GetGunByBarcode 查詢邏輯

- [x] 1.1 修改第二步查詢條件：`Chk == true` 改為 `Chk == false`
- [x] 1.2 修改第三步檢查：明確使用 `Chk == true` 篩選已完修紀錄
- [x] 1.3 驗證查詢邏輯的正確性（無 SQL 語法錯誤）
- [x] 1.4 編譯方案，確保無編譯錯誤

## 2. 前端修正 — ScanRepair.cshtml UI 簡化

- [x] 2.1 移除「料槍資訊」區塊（整個 `<div v-if="isScanned" class="panel panel-info">` section）
- [x] 2.2 驗證移除後的 HTML 結構（無孤立 tags 或樣式衝突）
- [x] 2.3 確認 v-model="formData.repairSno" 的隱藏欄位仍存在且正確

## 3. 邏輯驗證 — 前端流程整合

- [x] 3.1 驗證 mounted() 中 loadDropdownOptions 仍正確呼叫
- [x] 3.2 驗證掃碼成功時 isScanned=true，焦點進入維修資訊區（非料槍資訊區）
- [x] 3.3 驗證已完修流程：all_completed 時彈警告，表單隱藏
- [x] 3.4 驗證掃碼失敗流程：error 時彈錯誤，表單隱藏

## 4. 集成測試

- [x] 4.1 本地編譯並運行應用，無 JavaScript 錯誤 ✅
- [x] 4.2 掃碼存在未完修紀錄 — 驗證直接進入維修資訊區，無料槍資訊面板 ✅
- [x] 4.3 掃碼已全部完修 — 驗證彈警告「此料槍已全部完修」，表單隱藏 ✅
- [x] 4.4 掃碼無紀錄 — 驗證彈錯誤「料槍無維修紀錄」或「查無料槍編號」，表單隱藏 ✅
- [x] 4.5 填寫維修表單並提交 — 驗證 sno 參數正確傳遞至後端 ✅

## 5. 回歸測試

- [x] 5.1 驗證下拉選單（Classification、MaintenanceResult）仍正常載入選項 ✅
- [x] 5.2 驗證「其他原因」條件判斷仍正確（MaintenanceResult === 99 時顯示） ✅
- [x] 5.3 驗證「清空」按鈕功能正常 ✅
- [x] 5.4 驗證「返回清單」連結正常 ✅
- [x] 5.5 驗證現有 MaterialGunRepair 修改單筆紀錄流程無影響 ✅

## 6. 文件與驗收

- [x] 6.1 確認修改後的代碼註解清晰 ✅
- [x] 6.2 驗證無多餘 console.log 或除錯代碼 ✅
- [x] 6.3 建立驗收報告，記錄所有測試結果 ✅
- [x] 6.4 符合所有 spec 需求（scan-repair-validation、scan-repair-ui-simplification） ✅
