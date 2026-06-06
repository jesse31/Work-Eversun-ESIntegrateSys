# 料槍維修白名單權限控制 - Tasks

## 1. 準備階段 - 白名單常數與驗證方法

- [x] 1.1 建立 `ESIntegrateSys/Helpers/MaterialGunConstants.cs` 檔案，定義 `RepairEditWhitelist` HashSet<string>，包含白名單帳號 "02898"
- [x] 1.2 在 `MaterialGunController.cs` 中新增 `IsRepairWhitelisted(string userId)` 方法，檢查帳號是否在白名單中
- [x] 1.3 驗證 `IsRepairWhitelisted()` 方法的邏輯（白名單帳號返回 true，其他返回 false）

## 2. 後端驗證邏輯 - Controller 修改

- [x] 2.1 修改 `MaterialGunRepairView()` 方法，在 `Login_Authentication()` 後調用 `IsRepairWhitelisted()` 並傳遞結果到 `ViewBag.IsWhitelisted`
- [x] 2.2 修改 `MaterialGunRepair(int sno)` GET 方法，新增 Serilog 記錄：使用者訪問維修編輯頁面的日誌
- [x] 2.3 修改 `MaterialGunRepair(...) ` POST 方法，新增邏輯：
  - 若 `Chk == 1`，驗證白名單
  - 若白名單驗證失敗，記錄 Warning 級別 Serilog 日誌
  - 返回 `JsonResult { success = false, msg = "此料槍已完成維修，無法修改。無權進行此操作。" }`
- [x] 2.4 修改 `MaterialGunRepair(...) ` POST 方法，新增成功時的 Serilog 記錄：包含 UserId、Sno、修改摘要
- [x] 2.5 單元測試或手動測試 POST 方法的白名單驗證邏輯

## 3. 前端隱藏邏輯 - View 修改

- [x] 3.1 修改 `MaterialGunRepairView.cshtml` 的按鈕條件式（150-154 行），將原有邏輯改為：
  - 若 `Chk == 0` 且角色非 V、FR → 顯示按鈕
  - 若 `Chk == 1` 且 `ViewBag.IsWhitelisted == true` → 顯示按鈕
  - 其他情況 → 隱藏按鈕
- [x] 3.2 確認 HTML 中 `<tr>` 元素的 `data-chk="@item.Chk"` 屬性正確設定

## 4. 前端防呆機制 - JavaScript 修改

- [x] 4.1 修改 `MaterialGunRepairView.cshtml` 的 JavaScript 防呆邏輯（184-200 行）：
  - 檢查 `data-chk="true"` 的行是否點擊「料槍維修」連結
  - 若白名單驗證失敗，顯示 SweetAlert2 warning：「此料槍已完成維修，無法修改。無權進行此操作。」
  - 若驗證通過，允許頁面導向
- [x] 4.2 強化防呆文案，與後端錯誤訊息保持一致

## 5. 稽審日誌集成

- [x] 5.1 在 `MaterialGunController.cs` 中引入 Serilog（ILogger 依賴注入或 Log.ForContext）
- [x] 5.2 在 `MaterialGunRepairView()` 方法中新增訪問日誌：`Information: User {UserId} accessed material gun repair view`
- [x] 5.3 在 `MaterialGunRepair(int sno)` GET 方法中新增訪問日誌：`Information: User {UserId} accessed repair record for MaterialGun Sno {Sno}`
- [x] 5.4 在 `MaterialGunRepair(...) ` POST 方法中，成功提交時新增日誌：`Information: User {UserId} successfully updated repair record {Sno}`
- [x] 5.5 在 `MaterialGunRepair(...) ` POST 方法中，白名單驗證失敗時新增日誌：`Warning: User {UserId} denied access to repair record {Sno}: Not whitelisted`

## 6. 整合測試與驗證

- [x] 6.1 測試場景：白名單人員訪問 MaterialGunRepairView，驗證 ViewBag.IsWhitelisted = true
- [x] 6.2 測試場景：非白名單人員訪問 MaterialGunRepairView，驗證 ViewBag.IsWhitelisted = false
- [x] 6.3 測試場景：白名單人員點擊已完成維修料槍的「料槍維修」按鈕，應進入編輯頁面
- [x] 6.4 測試場景：非白名單人員點擊已完成維修料槍的「料槍維修」按鈕（若被顯示），應看到 SweetAlert2 warning
- [x] 6.5 測試場景：白名單人員提交已完成維修料槍的編輯表單，應成功更新並記錄日誌
- [x] 6.6 測試場景：非白名單人員提交已完成維修料槍的編輯表單（透過直接 URL），應被後端拒絕並記錄 Warning 日誌
- [x] 6.7 測試場景：任何人員編輯未維修料槍，不檢查白名單，應正常更新
- [x] 6.8 驗證 Serilog 日誌正確記錄所有操作（檢查日誌檔案或日誌系統）

## 8. 上線前檢查清單

- [x] 8.1 確認白名單帳號清單（"02898"）是否正確，是否需要新增其他帳號
- [x] 8.2 檢查 Serilog 設定是否正確，日誌輸出目標（檔案、ApplicationInsights 等）
- [x] 8.3 驗證錯誤訊息文案「此料槍已完成維修，無法修改。無權進行此操作。」是否最終定稿
- [x] 8.4 建立部署計畫及回滾方案文檔
- [x] 8.5 準備 Git Commit，訊息應清楚說明所有變更範圍
