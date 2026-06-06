# Maintenance Result Selection Capability Spec (Delta)

## Overview

修改既存的「檢修不良原因」下拉選單交互邏輯，使其在 AJAX 初始化時也能正確觸發相關 UI 邏輯（如 Other 欄位顯示）。

## Current State

現有實現：
- AJAX 從 MResult() API 動態載入選項
- 使用 `.val(selectedMaintenanceResult)` 設定初始值
- 監聽 `change` 事件以觸發 Other 欄位顯示/隱藏邏輯

**問題**：`.val()` 設值不觸發 `change` 事件，導致初始化時 Other 欄位未顯示

## Changes Required

### C1. AJAX Success 回調中添加顯示邏輯執行

在現有的 MResult AJAX 的 success 回調中（MaterialGunRepair.cshtml 第 42-67 行）：

```javascript
// 設定之前選中的值
if (selectedMaintenanceResult) {
    $('#MaintenanceResult').val(selectedMaintenanceResult);
    // [新增] 立即執行欄位可見性邏輯
    toggleOtherField();
}
```

### C2. change 事件監聽中使用獨立函式

改進現有的 change 監聽（MaterialGunRepair.cshtml 第 152-164 行）：

```javascript
$('#MaintenanceResult').change(function () {
    toggleOtherField();
});
```

### C3. 型別安全的值比較

在 toggleOtherField() 中使用：

```javascript
function toggleOtherField() {
    var selectedValue = $('#MaintenanceResult').val();
    if (parseInt(selectedValue) === 99) {
        $('#Other').show();
    } else {
        $('#Other').hide();
    }
}
```

## Requirements

### R1. 向後相容

**Given** 現有的 Controller 邏輯和 HTML 結構保持不變  
**When** JavaScript 邏輯升級  
**Then** 現有的表單提交、驗證流程應完全相同

**驗收標準**：
- Controller 的 MaterialGunRepair(POST) 方法無修改
- HTML 欄位名稱、ID 無變更
- 防呆檢查邏輯無變更

### R2. 初始化和交互的一致性

**Given** 頁面初次載入設定 MaintenanceResult = 99  
**When** 用戶之後再次選擇 99  
**Then** Other 欄位的可見性應相同

**驗收標準**：
- 初始化後 Other.display = "block"（或 show）
- 重新選擇 99 後 Other.display = "block"
- 選擇其他值後 Other.display = "none"
- 再次選擇 99 後 Other.display = "block"

### R3. 防呆檢查應正常通過

**Given** MaintenanceResult = 99 且 Other 欄位有值  
**When** 用戶提交表單  
**Then** 前端防呆檢查應通過（不攔截）

**驗收標準**：
- 不出現「尚未輸入原因」的 alert
- 表單成功提交到 Controller

### R4. 改動最小化

**Given** 只需修復初始化時序問題  
**When** 實施改動  
**Then** 應最小化程式碼變更，保持程式碼易讀性

**驗收標準**：
- 新增代碼 < 20 行
- 邏輯簡潔明了，易於維護
- 無重構現有 HTML 或 Controller

## Verification Scenarios

| # | 場景 | 預期結果 | 驗證方式 |
|---|------|--------|--------|
| S1 | 打開 /MaterialGun/MaterialGunRepair?sno=1783 | Other 欄位自動顯示（如記錄的 MaintenanceResult=99） | 載入後不操作，檢查 Other 可見性 |
| S2 | 選擇 MaintenanceResult = 0（請選擇） | Other 欄位隱藏 | change 事件觸發後檢查 |
| S3 | 選擇 MaintenanceResult = 99 | Other 欄位顯示 | change 事件觸發後檢查 |
| S4 | 選擇其他值，然後選 99 | Other 先隱藏後顯示 | 觀察 UI 行為 |
| S5 | MaintenanceResult=99、Other="測試原因"，提交 | 表單成功提交，no alert | 提交後檢查頁面導向或提示 |
| S6 | MaintenanceResult=99、Other=""，提交 | alert 出現「尚未輸入原因」| 防呆檢查正常 |

## Impact

- **Affected Pages**: MaterialGunRepair.cshtml
- **Affected Controller Actions**: MaterialGunRepair (GET 和 POST)
- **Affected APIs**: MResult()
- **Breaking Changes**: 無
- **Database Changes**: 無

## Migration Notes

無數據遷移需求。此為純前端 UI 改動。

## Future Considerations

此改動的模式（獨立 toggleOtherField() 函式 + AJAX/change 都調用）可應用於其他表單中的類似場景：
- MaterialGunForRepair 的 BadDescription（如有類似問題）
- 其他頁面的條件式欄位顯示邏輯

建議保存此模式作為今後的參考實踐。
