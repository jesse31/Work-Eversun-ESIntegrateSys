## 1. 後端 API 實作

- [x] 1.1 在 `MaterialGunController` 中新增 `ScanRepair()` GET Action
- [x] 1.2 在 `MaterialGunController` 中新增 `GetGunByBarcode()` POST API
  - 查詢 `ES_MaterialGunInfo` 驗證料槍存在
  - 查詢 `ES_MaterialGunRepair` 取最新未完修紀錄 (Chk=True)
  - 回傳 JSON: { success, message, data: { repairSno, Eno, Sno, Trade, Size } }
- [x] 1.3 測試 `GetGunByBarcode` API 所有情境
  - 查詢成功（有待維修紀錄）
  - 料槍不存在
  - 料槍無待維修紀錄

## 2. 前端頁面實作

- [x] 2.1 建立 `ScanRepair.cshtml` 檢視頁面
  - 使用 Razor 標準佈局 (`_MaterialLayout`)
  - 引入 Vue 3 CDN、SweetAlert2、Select2 CDN
  - HTML 結構：輸入框、料槍基本資訊欄位、維修資訊欄位
- [x] 2.2 實作 Vue 3 應用 (`createApp`) 管理頁面狀態
  - 定義 `data()`: scanInput、gunInfo、repairSno、isScanned、formData 等
  - 定義 `methods`: queryGun()、clearForm()、submitForm() 等
- [x] 2.3 實作輸入框 Enter 鍵監聽 (`@keydown.enter`)
  - 調用 `GetGunByBarcode` API
  - 根據回傳結果更新表單狀態
- [x] 2.4 實作欄位狀態管理
  - 查詢前：輸入框 enabled，其餘 disabled
  - 查詢成功：自動帶出欄位 disabled，維修資訊 enabled
  - 查詢失敗：清空輸入框，所有欄位 disabled，焦點回輸入框
- [x] 2.5 實作 Select2 下拉欄位初始化
  - 分類、原因下拉欄位使用 Select2
  - 啟用 allowClear、搜尋功能
- [x] 2.6 實作頁面焦點自動設定
  - 頁面載入時焦點鎖定在輸入框
  - 查詢失敗時焦點恢復輸入框

## 3. 表單提交與驗證

- [x] 3.1 實作 [儲存] 按鈕
  - 收集表單資料 (Classification、MaintenanceResult、Other、ChangeItemName、ChangeItemNo)
  - 進行客戶端驗證（分類、原因不能為空）
  - 提交至 `/MaterialGun/MaterialGunRepair?sno=<repairSno>` (POST)
- [x] 3.2 複用既有的 MaterialGunRepair POST 邏輯
  - 儲存成功後由後端決定導向

## 4. 錯誤處理與提示

- [x] 4.1 實作 SweetAlert2 錯誤提示
  - 輸入框為空時提示「請輸入料槍編號」
  - 料槍不存在提示「查無料槍編號：XXX」
  - 料槍無待維修紀錄提示「料槍 XXX 無待維修紀錄或已全部完修」
  - API 伺服器錯誤提示「伺服器錯誤：...」
- [x] 4.2 實作自動清空與焦點恢復邏輯
  - 錯誤提示後清空輸入框
  - 焦點自動回到輸入框

## 5. 主頁面更新

- [x] 5.1 更新 `MaterialGunRepairView.cshtml` 工具列
  - 在「料槍送修」按鈕旁新增「掃碼維修」按鈕
  - 連結至 `/MaterialGun/ScanRepair`

## 6. 整合測試

- [x] 6.1 掃碼流程測試
  - 掃碼槍掃入有效料槍編號 → Enter → 查詢成功 → 自動帶出 → 填寫 → 儲存 → 返回主頁
- [x] 6.2 手動輸入流程測試
  - 手動輸入料槍編號 → Enter → 同上流程
- [x] 6.3 異常情況測試
  - 掃入/輸入不存在的料槍 → SweetAlert2 錯誤提示 → 焦點回輸入框
  - 掃入/輸入無待維修紀錄的料槍 → SweetAlert2 錯誤提示 → 焦點回輸入框
  - 輸入為空直接按 Enter → SweetAlert2 提示 → 無動作
- [x] 6.4 維修填寫與儲存測試
  - 填寫分類、原因、更換部品等欄位
  - 點擊儲存，驗證資料正確儲存至資料庫
  - 驗證導向至主頁
- [x] 6.5 Select2 下拉功能測試
  - 搜尋功能正常
  - 清除功能正常（按 X）
- [x] 6.6 多次操作測試
  - 掃碼/輸入 A 料槍 → 儲存 → 返回 → 重新進入 ScanRepair → 掃碼/輸入 B 料槍
  - 驗證系統狀態正確重置
- [x] 6.7 驗收清單確認
  - 無 YSOD 或例外
  - 欄位狀態符合防呆邏輯
  - SweetAlert2 提示清晰
  - 主頁按鈕連結正確
  - 文字尺寸適合年長操作員閱讀

## 7. 文件與知識庫

- [x] 7.1 更新系統架構文件
  - 新增掃碼維修功能說明
  - 記錄 `GetGunByBarcode` API 用途與回傳格式
- [x] 7.2 準備使用者操作手冊
  - 掃碼維修流程說明
  - 常見錯誤與排查指南
