## MODIFIED Requirements

### Requirement: 下拉選項載入與 Select2 初始化

**修改內容**: Select2 初始化流程改為非同步等待機制，當 Select2 超時時自動降級為原生 HTML select，而不是完全失敗

**原需求**: 
- 透過 AJAX 加載分類和維修原因選項
- 載入後立即初始化 Select2，若失敗則使用原生 select

**修改後需求**:
- 透過 AJAX 加載分類和維修原因選項（不變）
- 載入後檢查 Select2 是否已由 ensureSelect2Ready() 準備好
- 若就緒則初始化 Select2；若超時則自動降級為原生 HTML select（用戶無感知，但無搜尋功能）
- 無錯誤、無警告、無降級提示

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
