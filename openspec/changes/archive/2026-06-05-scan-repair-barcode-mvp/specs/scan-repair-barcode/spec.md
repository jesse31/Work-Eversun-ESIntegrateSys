## ADDED Requirements

### Requirement: 掃碼或手動輸入料槍編號
系統應允許操作員透過掃碼槍掃入料槍編號或手動輸入料槍編號。輸入框應支援 Enter 鍵觸發查詢動作，無須額外按鈕。

#### Scenario: 掃碼槍掃入條碼
- **WHEN** 操作員使用掃碼槍掃入料槍條碼
- **THEN** 掃碼槍自動送出 Enter 鍵，系統自動觸發查詢 API

#### Scenario: 手動輸入料槍編號
- **WHEN** 操作員在輸入框輸入料槍編號並按 Enter
- **THEN** 系統觸發查詢 API

#### Scenario: 輸入框為空時按 Enter
- **WHEN** 操作員按 Enter 但輸入框為空
- **THEN** 系統使用 SweetAlert2 提示「請輸入料槍編號」，不進行查詢

---

### Requirement: 查詢並自動帶出料槍基本資訊
系統應根據輸入的料槍編號查詢料槍基本資訊及待維修紀錄，查詢成功後自動填入表單中。

#### Scenario: 查詢成功
- **WHEN** 操作員輸入有效的料槍編號並按 Enter
- **THEN** 系統查詢料槍基本資訊 (編號、設備編號、廠商、型號) 及最新未完修維修紀錄，自動填入表單對應欄位

#### Scenario: 料槍不存在
- **WHEN** 操作員輸入不存在的料槍編號
- **THEN** 系統使用 SweetAlert2 提示「查無料槍編號：XXX」，清空輸入框，焦點回輸入框

#### Scenario: 料槍無待維修紀錄
- **WHEN** 操作員輸入的料槍已全部完修或無維修紀錄
- **THEN** 系統使用 SweetAlert2 提示「料槍 XXX 無待維修紀錄或已全部完修」，清空輸入框，焦點回輸入框

---

### Requirement: 自動帶出的欄位應為唯讀
查詢成功後帶出的料槍基本資訊欄位 (編號、設備編號、廠商、型號) 應保持 `disabled` 狀態，防止操作員手動修改。

#### Scenario: 欄位狀態檢查
- **WHEN** 查詢成功後
- **THEN** 料槍編號、設備編號、廠商、型號欄位應呈現 disabled 狀態，無法編輯

---

### Requirement: 維修資訊欄位應在查詢成功後啟用
查詢成功後，操作員應能編輯維修資訊欄位 (分類、原因、更換部品名稱、更換部品料號)。

#### Scenario: 維修欄位啟用
- **WHEN** 查詢成功後
- **THEN** 維修資訊欄位 (分類、原因、更換部品名稱、更換部品料號) 應呈現 enabled 狀態，可進行編輯

#### Scenario: 查詢失敗時欄位恢復 disabled
- **WHEN** 查詢失敗（如料槍不存在）
- **THEN** 所有維修資訊欄位應恢復 disabled 狀態

---

### Requirement: 下拉欄位使用 Select2 套件
分類、原因等下拉欄位應使用 Select2 套件，並啟用搜尋及清除功能。

#### Scenario: Select2 搜尋功能
- **WHEN** 操作員點擊下拉欄位
- **THEN** 系統顯示 Select2 搜尋框，操作員可輸入關鍵字過濾選項

#### Scenario: Select2 清除功能
- **WHEN** Select2 下拉欄位已選定值
- **THEN** 系統顯示清除按鈕 (X)，操作員可點擊清除所選

---

### Requirement: 儲存表單至既有維修流程
點擊 [儲存] 按鈕後，系統應使用既有的 `MaterialGunRepair` POST Action 儲存表單資料，無需新增獨立 API。

#### Scenario: 儲存成功
- **WHEN** 操作員填完維修資訊並點擊 [儲存]
- **THEN** 系統提交表單至 `/MaterialGun/MaterialGunRepair?sno=<repairSno>` (POST)，執行既有驗證與儲存邏輯

#### Scenario: 儲存後返回主頁
- **WHEN** 儲存成功
- **THEN** 系統使用 SweetAlert2 提示成功訊息，1.5 秒後導向 `/MaterialGun/MaterialGunRepairView` 主頁

---

### Requirement: 頁面焦點與欄位防呆
頁面載入時焦點應自動鎖定在輸入框，防止操作員跳過掃碼/輸入流程。

#### Scenario: 頁面載入時焦點設定
- **WHEN** 用戶進入 `/MaterialGun/ScanRepair` 頁面
- **THEN** 焦點自動設定在料槍編號輸入框，無需手動點擊

#### Scenario: 查詢失敗時焦點恢復輸入框
- **WHEN** 查詢失敗（如料槍不存在或無待維修紀錄）
- **THEN** 清空輸入框後焦點自動回到輸入框，等待重新輸入

---

## MODIFIED Requirements

### Requirement: 料槍維修清單主頁入口
維修清單主頁應新增「掃碼維修」入口按鈕，導向掃碼維修頁面。

#### Scenario: 掃碼維修按鈕可見
- **WHEN** 用戶進入 `/MaterialGun/MaterialGunRepairView` 主頁
- **THEN** 頁面應在工具列顯示「掃碼維修」按鈕

#### Scenario: 按鈕連結至掃碼維修頁面
- **WHEN** 用戶點擊「掃碼維修」按鈕
- **THEN** 系統導向 `/MaterialGun/ScanRepair` 頁面
