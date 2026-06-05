## Why

目前料槍維修作業採手動輸入料槍編號，容易出現「選錯料槍」的失誤。本 MVP 引入掃碼功能，使操作人員透過條碼掃描槍快速帶出料槍資訊，加快維修作業效率並降低人為錯誤。

## What Changes

- 新增「掃碼維修」頁面 (`ScanRepair.cshtml`)，使用 Vue 3 CDN 管理狀態
- 新增後端 API `GetGunByBarcode`，根據掃入的料槍編號查詢基本資訊與待維修紀錄
- 掃碼維修後的儲存邏輯**複用既有的 `MaterialGunRepair` POST Action**，無需新增獨立 API
- 更新 `MaterialGunRepairView.cshtml` 主頁工具列，加入「掃碼維修」入口按鈕
- 新增 `Web.config` 白名單設定 `RepairPrivilegedUsers`（為後續版本 v1.1 加入已完修權限控管預留）

## Capabilities

### New Capabilities
- `scan-repair-barcode`: 掃碼快速進入料槍維修編輯流程。掃碼後自動查詢料槍基本資訊 (編號、設備編號、廠商、型號) 與待維修紀錄，前端自動帶出欄位，操作人員可直接填寫維修結果，使用既有的維修儲存邏輯

### Modified Capabilities
- `material-gun-repair-list`: 維修清單主頁新增「掃碼維修」入口按鈕，導向掃碼維修頁面

## Impact

- **前端**：新增 Vue 3 CDN 頁面 (`ScanRepair.cshtml`)，與既有 jQuery 架構共存，不影響其他頁面；修改 `MaterialGunRepairView.cshtml` 加入按鈕
- **後端**：新增 1 個 API Action (`GetGunByBarcode`)，複用既有的 `MaterialGunRepair` POST 儲存邏輯
- **資料庫**：無修改（條碼 = MaterialGun_Sno，無新增欄位）
- **配置**：新增 `Web.config` 設定項 `RepairPrivilegedUsers`
- **相容性**：完全無痛整合，不破壞現有功能
