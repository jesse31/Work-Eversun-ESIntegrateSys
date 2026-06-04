## ADDED Requirements

### Requirement: Select2 插件非同步初始化保障
系統應確保在 jQuery 和 Select2 CDN 完全加載後，再嘗試初始化下拉欄位的 Select2 插件，防止 `$(...).select2 is not a function` 錯誤。

#### Scenario: 頁面載入時 Select2 已可用
- **WHEN** ScanRepair 頁面在 mounted() 階段
- **THEN** 系統檢查 window.$ 和 window.$.fn.select2 是否存在
- **AND** 若存在則立即初始化下拉選項；若不存在則輪詢最多 3 次（每次延遲 100ms）

#### Scenario: Select2 初始化成功
- **WHEN** 輪詢確認 Select2 可用
- **THEN** 系統通過 AJAX 載入分類和維修原因選項
- **AND** 初始化 #Classification 和 #MaintenanceResult 為 Select2 元件（allowClear: true）

#### Scenario: Select2 初始化逾時
- **WHEN** 輪詢 3 次（300ms）後 Select2 仍不可用
- **THEN** 系統記錄警告日誌，下拉欄位回退為原生 HTML select（無 Select2 功能但仍可用）
- **AND** 系統仍允許用戶進行維修操作（不完全中斷功能）

---

### Requirement: 下拉選項載入後自動關聯事件
Select2 初始化後，系統應自動設定 change 事件監聽，使下拉選項變化能同步至 Vue 資料模型。

#### Scenario: 分類選項變化同步
- **WHEN** 操作員在 Select2 下拉中選定分類
- **THEN** formData.Classification 值自動更新為選定的分類值
- **AND** Select2 change 事件完成後無延遲或錯誤

#### Scenario: 維修原因選項變化同步
- **WHEN** 操作員在 Select2 下拉中選定維修原因
- **THEN** formData.MaintenanceResult 值自動更新為選定的原因值
- **AND** 若原因為 99（其他），系統觸發 handleMaintenanceResultChange()，顯示「其他原因」輸入框
