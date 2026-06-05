## ADDED Requirements

### Requirement: Select2 非同步可靠初始化
系統應使用 Promise-based 輪詢機制確保 Select2 CDN 完全加載後再初始化下拉選單，避免非同步競態條件。最多輪詢 1000ms；如超時則自動降級為原生 HTML select。

#### Scenario: Select2 在 mounted() 時已加載
- **WHEN** ScanRepair 頁面載入且 Select2 CDN 已就緒
- **THEN** 系統透過 ensureSelect2Ready() 快速檢測到 Select2 可用
- **AND** 立即調用 loadDropdownOptions() 載入下拉選項
- **AND** 分類和維修原因下拉選單正常初始化為 Select2 組件

#### Scenario: Select2 在 AJAX 回應時到達
- **WHEN** ScanRepair 頁面載入後，Classification AJAX 回應在 200ms 後到達
- **THEN** ensureSelect2Ready() 在後臺輪詢，於 AJAX 回應時發現 Select2 已就緒
- **AND** success callback 中的守衛檢查通過
- **AND** 系統安全初始化 Select2（無錯誤）

#### Scenario: Select2 超時未加載
- **WHEN** 經過 1000ms 後 Select2 仍未加載
- **THEN** ensureSelect2Ready() 返回 false（超時）
- **AND** 系統記錄警告：「[ScanRepair] Select2 超時」
- **AND** 降級為原生 HTML select（移除 Select2 初始化）
- **AND** 用戶仍能選擇分類和維修原因（功能完整但無搜尋增強）

---

### Requirement: 非同步啟動不阻塞 UI
系統應在 mounted() 後臺非同步啟動 Select2 準備，不阻塞頁面互動。用戶可立即掃碼，Select2 在背景準備。

#### Scenario: 頁面載入後立即掃碼
- **WHEN** ScanRepair 頁面載入
- **THEN** mounted() 立即返回（不等待 Select2 準備）
- **AND** 掃碼輸入框自動獲得焦點
- **AND** 用戶可立即輸入料槍編號並按 Enter
- **AND** 若 Select2 已準備則帶出下拉；若未準備則降級為原生 select

---

### Requirement: AJAX 回調中的守衛檢查
系統應在每個 AJAX success callback 中重新檢查 Select2 是否可用，確保即使延遲也能安全初始化。

#### Scenario: Classification AJAX 成功但 Select2 未就緒
- **WHEN** Classification AJAX 回應到達，但 Select2 仍未加載
- **THEN** success callback 中的守衛檢查發現 Select2 不可用
- **AND** 系統記錄警告並跳過 Select2 初始化
- **AND** 下拉元素保留為原生 HTML select，用戶仍能操作

#### Scenario: Classification AJAX 成功且 Select2 已就緒
- **WHEN** Classification AJAX 回應到達且 Select2 已加載
- **THEN** success callback 中的守衛檢查通過
- **AND** 系統通過 $nextTick() 等待 Vue 渲染完成
- **AND** 系統調用 $('#Classification').select2({...}) 初始化 Select2
- **AND** change 事件監聽器正確綁定

---

### Requirement: 原生 HTML select 降級
當 Select2 超時時，系統應保留原生 HTML select 標籤的功能，允許用戶選擇維修分類和原因。

#### Scenario: 降級為原生 select 後的操作
- **WHEN** Select2 超時，系統降級為原生 select
- **THEN** 原生 <select id="Classification"> 標籤保持可用
- **AND** 用戶可點擊下拉、掃描選項、選擇分類
- **AND** 選擇結果同步至 formData.Classification
- **AND** 維修表單可正常提交，資料儲存成功
- **AND** UI 無視覺錯誤（無紅色邊框、無錯誤提示）

---

## MODIFIED Requirements

### Requirement: 查詢並自動帶出料槍基本資訊

**修改內容**: Select2 初始化流程從「單次檢查」改為「非同步等待」，降級策略從「禁用」改為「自動降級為原生 select」

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
