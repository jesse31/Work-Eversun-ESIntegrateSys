# 料槍維修部門授權 - Implementation Tasks

## 1. 常數定義與授權方法

- [x] 1.1 在 `MaterialGunConstants.cs` 中新增 `RepairEditWhitelistDepts` HashSet，包含授權部門代碼 "IT"
- [x] 1.2 為 RepairEditWhitelistDepts 新增 XML 文件註解，說明此常數的用途
- [x] 1.3 驗證 MaterialGunConstants.cs 編譯無誤

## 2. 授權驗證方法改進

- [x] 2.1 在 `MaterialGunController.cs` 中修改 `IsRepairWhitelisted()` 方法簽名，改為接受 (userId, userDept) 兩個參數
- [x] 2.2 實現新的授權邏輯，使用 OR 組合：`(userId in RepairEditWhitelistUsers) OR (userDept in RepairEditWhitelistDepts)`
- [x] 2.3 新增帶原因返回的版本 `IsRepairWhitelistedWithReason(userId, userDept)`，返回 (bool isWhitelisted, string reason) 元組
- [x] 2.4 為新增的方法新增 XML 文件註解，清楚說明兩個參數與返回值的含義
- [x] 2.5 驗證方法邏輯：null 檢查、字串比較是否區分大小寫一致性

## 3. MaterialGunRepairView() 方法修改

- [x] 3.1 在 `MaterialGunRepairView()` 方法中，取得當前使用者的部門代碼（從 Session["Member"].UDeptNo）
- [x] 3.2 修改授權檢查呼叫，從 `IsRepairWhitelisted(userId)` 改為 `IsRepairWhitelisted(userId, userDept)`
- [x] 3.3 驗證 ViewBag.IsWhitelisted 仍被正確設定為布林值
- [x] 3.4 驗證部門代碼為 null 時的處理（無應報例外，IsRepairWhitelisted 應安全處理）

## 4. MaterialGunRepair() GET 方法修改

- [x] 4.1 在 `MaterialGunRepair(int sno)` GET 方法中，取得使用者的部門代碼
- [x] 4.2 修改 Serilog 日誌記錄，新增 UDeptNo 欄位到 ForContext 中
- [x] 4.3 驗證日誌訊息包含足夠資訊以追蹤操作

## 5. MaterialGunRepair() POST 方法修改

- [x] 5.1 在 `MaterialGunRepair(...)` POST 方法中，取得使用者的部門代碼
- [x] 5.2 修改白名單驗證邏輯，呼叫 `IsRepairWhitelistedWithReason(userId, userDept)` 獲取授權決策與原因
- [x] 5.3 修改授權失敗的回應，確保仍返回統一的錯誤訊息：`{ success: false, msg: "此料槍已完成維修，無法修改。無權進行此操作。" }`
- [x] 5.4 修改成功的 Serilog 日誌記錄，新增 UDeptNo 與授權原因欄位

## 6. 稽審日誌完整性驗證

- [x] 6.1 確認成功情況下的日誌格式：`User {UserId} (Dept: {UDeptNo}) authorized to edit repair {Sno} - Reason: {AuthorizationReason}`
- [x] 6.2 確認失敗情況下的日誌格式：`User {UserId} (Dept: {UDeptNo}) denied access to repair record {Sno}`
- [x] 6.3 確認使用 Log.ForContext() 傳入的所有欄位（UserId, UDeptNo, Sno, AuthorizationReason）
- [x] 6.4 驗證日誌級別：成功用 Information，失敗用 Warning

## 7. 前端驗證（無需改動，驗證相容性）

- [x] 7.1 確認 MaterialGunRepairView.cshtml 中 ViewBag.IsWhitelisted 的布林條件式仍正確運作
- [x] 7.2 確認 JavaScript 中從 ViewBag.IsWhitelisted 讀取布林值無需改動
- [x] 7.3 驗證按鈕顯示邏輯（Chk 與 IsWhitelisted 的組合）仍正確

## 8. 單元測試與場景驗證

- [x] 8.1 場景 A：白名單帳號 "02898" 編輯已完成料槍 → 應授權（帳號白名單原因）
- [x] 8.2 場景 B：IT 部門使用者編輯已完成料槍 → 應授權（部門授權原因）
- [x] 8.3 場景 C：非授權部門使用者編輯已完成料槍 → 應拒絕
- [x] 8.4 場景 D：未完成料槍的編輯 → 無須授權檢查（邏輯不變）
- [x] 8.5 驗證 Serilog 日誌正確記錄授權原因（可通過檢查日誌檔案或 Application Insights）

## 9. 程式碼品質檢查

- [x] 9.1 確認所有新增/修改的方法都有完整的 XML 文件註解（`<summary>` 等）
- [x] 9.2 確認所有註解都使用繁體中文，且以句點結尾
- [x] 9.3 檢查是否有多餘的 Console.WriteLine 或調試代碼需要移除
- [x] 9.4 驗證命名規範一致（變數名、方法名遵循現有風格）
- [x] 9.5 確認異常處理：null 檢查、字串比較等邊界情況

## 10. 整合與系統驗證

- [x] 10.1 編譯專案確保無編譯錯誤
- [x] 10.2 運行（若有現成測試）或手動驗證：白名單帳號、IT 部門使用者、非授權使用者的各種場景
- [x] 10.3 確認 ViewBag.IsWhitelisted 在所有情況下都正確傳遞到前端
- [x] 10.4 驗證錯誤訊息與前端 SweetAlert2 相容性

## 11. 程式碼審查與最終檢查

- [x] 11.1 檢查 MaterialGunConstants.cs 中兩個白名單（帳號、部門）的定義是否清晰
- [x] 11.2 檢查 IsRepairWhitelisted() 與 IsRepairWhitelistedWithReason() 兩個方法的邏輯是否一致
- [x] 11.3 檢查 POST 方法中授權驗證與日誌記錄是否完整
- [x] 11.4 檢查是否有遺漏的部門代碼 null 檢查
- [x] 11.5 最終確認無破壞性改動，現有帳號白名單功能保持不變

## 12. 上線前檢查清單

- [x] 12.1 確認部門代碼 "IT" 是否與系統實際部門代碼相符
- [x] 12.2 確認 Serilog 配置正確，日誌輸出目標已確認（檔案、ApplicationInsights）
- [x] 12.3 驗證錯誤訊息文案最終定稿
- [x] 12.4 準備部署計畫與回滾步驟文檔
- [x] 12.5 確認是否需要資料遷移（應無需）
