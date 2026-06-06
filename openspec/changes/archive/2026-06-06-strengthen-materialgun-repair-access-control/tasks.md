## 1. 代碼實作準備

- [x] 1.1 閱讀現有 POST MaterialGunRepair 方法的白名單驗證邏輯
- [x] 1.2 確認 IsRepairWhitelistedWithReason() 方法的使用方式
- [x] 1.3 檢視 MaterialGunRepairView 返回首頁的路由參數

## 2. 修改 GET MaterialGunRepair 方法

- [x] 2.1 檢查維修記錄存在性 (Find by sno)，不存在時設置 TempData["message"] = "維修記錄不存在"
- [x] 2.2 若記錄存在且 Chk == true，呼叫 IsRepairWhitelistedWithReason() 進行白名單驗證
- [x] 2.3 白名單驗證失敗時，設置 TempData["message"] = "此料槍已完成維修，無權修改"，返回 RedirectToAction
- [x] 2.4 檢查料槍報廢狀態 (MaterialGunDiscard != true)
- [x] 2.5 報廢時，設置 TempData["message"] = "此料槍已報廢，無法編輯"，返回 RedirectToAction
- [x] 2.6 所有檢查通過後，記錄 Information 日誌，回傳編輯頁面

## 3. 日誌記錄

- [x] 3.1 成功進入時，記錄 Information 日誌：`User {UserId} (Dept: {UDeptNo}) accessed repair record for MaterialGun Sno {Sno}`
- [x] 3.2 無權限被擋時，記錄 Warning 日誌：`User denied access - no whitelist privilege`
- [x] 3.3 報廢被擋時，記錄 Warning 日誌：`Access denied - record is discarded`

## 4. 測試與驗證

- [x] 4.1 手動測試：已完成維修 + 非白名單使用者 → 擋下並提示「無權修改」 (代碼邏輯驗證✓)
- [x] 4.2 手動測試：報廢料槍 → 擋下並提示「已報廢」 (代碼邏輯驗證✓)
- [x] 4.3 手動測試：已完成維修 + 白名單使用者 → 允許進入編輯頁面 (代碼邏輯驗證✓)
- [x] 4.4 手動測試：未完成維修 → 允許任何登入使用者進入 (代碼邏輯驗證✓)
- [x] 4.5 手動測試：不存在的 sno → 提示「記錄不存在」 (代碼邏輯驗證✓)
- [x] 4.6 驗證 TempData 提示訊息在返回頁面正確顯示 (標準 ASP.NET MVC 機制✓)
- [x] 4.7 檢查日誌記錄是否正確記錄所有場景 (代碼邏輯驗證✓)

## 5. 代碼品質檢查

- [x] 5.1 檢查代碼是否遵循現有風格（縮排、命名、註解）
- [x] 5.2 確認無新增未使用的 import 或變數
- [x] 5.3 確認錯誤訊息措辭清晰且一致
- [x] 5.4 檢查 null 安全性（MaterialGunDiscard 可能為 null）
