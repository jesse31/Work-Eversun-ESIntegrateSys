## Why

掃碼維修功能中存在兩個問題：(1) 後端驗證邏輯反向 — Chk 欄位語義錯誤，導致掃碼時查詢已完修的紀錄而非未完修的，造成流程失敗；(2) 前端 UI 過於繁瑣 — 自動帶出的「料槍資訊」面板在 MVP 階段不必要，增加頁面複雜度。需要修正驗證邏輯並簡化 UI，確保掃碼維修流程順暢進行。

## What Changes

- **修正後端掃碼驗證邏輯**：GetGunByBarcode 方法查詢條件從 `Chk == true` 改為 `Chk == false`（因為 Chk=false 表示未完修）
- **明確化「已全部完修」判斷**：將 anyRecord 查詢改為明確檢查 `Chk == true` 的紀錄，避免邏輯模糊
- **簡化前端 ScanRepair.cshtml**：移除「料槍資訊」區塊（Eno、Sno、Trade、Size），保留掃碼區塊和維修資訊區塊
- **保留掃碼驗證邏輯**：後端仍執行掃碼查詢驗證，確保料槍存在且有待維修紀錄；formData.repairSno 仍從 API 響應取得，正常提交

## Capabilities

### New Capabilities

- `scan-repair-validation`: 修正的掃碼驗證邏輯，確保正確識別未完修紀錄（Chk=false）並攔截已完修情況

### Modified Capabilities

- `scan-repair-ui-simplification`: 簡化 ScanRepair.cshtml 前端，移除不必要的料槍資訊面板，提升 MVP 使用體驗

## Impact

**後端代碼**：
- `MaterialGunController.cs` - GetGunByBarcode 方法修正（第二步和第三步的查詢條件）

**前端代碼**：
- `ScanRepair.cshtml` - 移除料槍資訊區塊（<div v-if="isScanned" class="panel panel-info">）及相關樣式

**使用者流程**：
- 掃碼後直接進入維修資訊填寫，無中間確認步驟
- 已完修或無紀錄的料槍仍被正確攔截，提示相應警告

**API 契約**：
- GetGunByBarcode 仍返回 repairSno，前端隱藏 Eno/Sno/Trade/Size 顯示但邏輯不變
