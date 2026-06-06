# Tasks: 補齊後端 ViewBag 欄位傳遞

## 1. 後端修改 - 添加 ViewBag 賦值

- [x] 1.1 開啟 MaterialGunController.cs，定位到 MaterialGunRepair(GET) 方法第 433-437 行
- [x] 1.2 在現有的三行 ViewBag 賦值後添加新的三行賦值（Other、ChangeItemName、ChangeItemNo）
- [x] 1.3 驗證代碼語法無誤（Ctrl+K+D 自動格式化）

## 2. 前端修改 - 綁定 Input 欄位值

- [x] 2.1 開啟 MaterialGunRepair.cshtml，定位到 Other input 欄位（約第 141 行）
- [x] 2.2 在 Other input 添加 value 綁定：`value="@ViewBag.Other"`
- [x] 2.3 定位到 ChangeItemName input 欄位（約第 145 行）
- [x] 2.4 在 ChangeItemName input 添加 value 綁定：`value="@ViewBag.ChangeItemName"`
- [x] 2.5 定位到 ChangeItemNo input 欄位（約第 149 行）
- [x] 2.6 在 ChangeItemNo input 添加 value 綁定：`value="@ViewBag.ChangeItemNo"`
- [x] 2.7 驗證代碼格式化無誤

## 3. 驗收測試

- [x] 3.1 啟動應用程式（IIS Express 或本機部署）
- [x] 3.2 測試新建維修記錄：打開 /MaterialGun/MaterialGunRepair（無 sno 參數）
  - 驗證：Other、ChangeItemName、ChangeItemNo 欄位為空
- [x] 3.3 測試編輯既存記錄：打開 /MaterialGun/MaterialGunRepair?sno=1783
  - 驗證：如果此記錄有 Other、ChangeItemName、ChangeItemNo 的舊值，應在頁面加載時顯示
- [x] 3.4 測試 MaintenanceResult = 99 時的 Other 欄位顯示
  - 驗證：if MaintenanceResult = 99，Other 欄位應可見且有值
- [x] 3.5 測試表單提交：修改某些欄位值並提交表單
  - 驗證：新值應正確傳遞到 POST 方法並保存到資料庫
- [x] 3.6 驗證無前端錯誤：打開瀏覽器開發者工具（F12），檢查 Console 是否有 JavaScript 錯誤

## 4. 代碼審查與文檔

- [x] 4.1 確認後端改動符合既有代碼風格（ViewBag 命名、縮排）
- [x] 4.2 確認前端改動符合既有 HTML 結構（class、id、屬性順序）
- [x] 4.3 檢查無多餘代碼或註解（遵循簡潔原則）
- [x] 4.4 驗收所有改動已 staged（準備提交但尚未 commit）
