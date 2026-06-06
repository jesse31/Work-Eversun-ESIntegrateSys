# Scan Repair Barcode Capability

## Purpose

Enable material gun operators to quickly initiate repair workflows by scanning or manually entering equipment barcodes. The system automatically retrieves equipment information and prefills repair forms, streamlining the repair initiation process while maintaining data integrity through read-only and editable field states.

---

## Requirements

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

系統應根據輸入的料槍編號查詢料槍基本資訊及待維修紀錄，查詢成功後自動填入表單中。Select2 初始化流程應改為非同步等待機制，當 Select2 超時時自動降級為原生 HTML select。

#### Scenario: 查詢成功
- **WHEN** 操作員輸入有效的料槍編號並按 Enter
- **THEN** 系統查詢料槍基本資訊及最新未完修維修紀錄
- **AND** 自動填入表單對應欄位
- **AND** 若 Select2 已就緒則初始化，否則使用原生 select

#### Scenario: 料槍不存在
- **WHEN** 操作員輸入不存在的料槍編號
- **THEN** 系統使用 SweetAlert2 提示「查無料槍編號：XXX」
- **AND** 清空輸入框，焦點回輸入框

#### Scenario: 料槍無待維修紀錄 (已全部完修)
- **WHEN** 操作員輸入的料槍已全部完修或無維修紀錄
- **THEN** 系統使用 SweetAlert2 提示「此料槍已全部完修，無待維修紀錄」
- **AND** 清空輸入框，焦點回輸入框
- **AND** 不帶出任何維修表單欄位（保持隱藏）

#### Scenario: 伺服器錯誤
- **WHEN** GetGunByBarcode API 拋出異常
- **THEN** 系統使用 SweetAlert2 提示「伺服器錯誤：[錯誤訊息]」
- **AND** 清空輸入框，焦點回輸入框

#### Scenario: Select2 超時但查詢成功
- **WHEN** 操作員輸入料槍編號，查詢成功，但 Select2 仍未加載
- **THEN** 系統帶出料槍基本資訊
- **AND** 維修表單使用原生 HTML select 作為下拉選項
- **AND** 用戶仍能選擇分類和維修原因，完成維修操作

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

### Requirement: 下拉選項載入與 Select2 初始化

系統應透過 AJAX 加載分類和維修原因選項，並檢查 Select2 是否已由 ensureSelect2Ready() 準備好。若就緒則初始化 Select2；若超時則自動降級為原生 HTML select（用戶無感知，但無搜尋功能）。

#### Scenario: 分類選項正常載入且 Select2 就緒
- **WHEN** Classification AJAX 成功回應，Select2 CDN 已加載
- **THEN** 系統將回應資料存入 classificationOptions
- **AND** success callback 中的守衛檢查通過（window.$.fn.select2 可用）
- **AND** 系統通過 $nextTick() 等待 Vue 渲染
- **AND** 系統調用 $('#Classification').select2({allowClear: true, ...})
- **AND** 綁定 change 事件監聽器
- **AND** Select2 初始化完成，用戶可搜尋和選擇

#### Scenario: 維修原因選項正常載入且 Select2 就緒
- **WHEN** MResult (維修原因) AJAX 成功回應，Select2 CDN 已加載
- **THEN** 系統將回應資料存入 maintenanceResultOptions
- **AND** success callback 中的守衛檢查通過
- **AND** 系統初始化 Select2
- **AND** 綁定 change 事件，當選擇「99（其他）」時觸發 handleMaintenanceResultChange()
- **AND** 表單狀態正確同步

#### Scenario: 分類選項載入但 Select2 超時
- **WHEN** Classification AJAX 成功但 Select2 在 1000ms 內未加載
- **THEN** ensureSelect2Ready() 返回 false
- **AND** loadDropdownOptions() 不執行 Select2 初始化
- **AND** classificationOptions 資料已載入，原生 <select> 標籤仍可用
- **AND** 用戶可點擊下拉、選擇分類（無搜尋功能）
- **AND** 選擇結果同步至 formData.Classification
- **AND** 維修流程正常繼續

#### Scenario: 維修原因選項載入但 Select2 超時
- **WHEN** MResult AJAX 成功但 Select2 在 1000ms 內未加載
- **THEN** ensureSelect2Ready() 返回 false
- **AND** loadDropdownOptions() 不執行 Select2 初始化
- **AND** maintenanceResultOptions 資料已載入，原生 <select> 標籤仍可用
- **AND** 用戶可選擇維修原因，change 事件觸發 handleMaintenanceResultChange()
- **AND** 若選擇「99（其他）」則顯示「其他原因」輸入框
- **AND** 維修流程正常繼續

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

### Requirement: 料槍維修清單主頁入口

維修清單主頁應新增「掃碼維修」入口按鈕，導向掃碼維修頁面。

#### Scenario: 掃碼維修按鈕可見
- **WHEN** 用戶進入 `/MaterialGun/MaterialGunRepairView` 主頁
- **THEN** 頁面應在工具列顯示「掃碼維修」按鈕

#### Scenario: 按鈕連結至掃碼維修頁面
- **WHEN** 用戶點擊「掃碼維修」按鈕
- **THEN** 系統導向 `/MaterialGun/ScanRepair` 頁面
