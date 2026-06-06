## Context

MaterialGunScanRepair.cshtml 前端與後端存在型別不匹配：
- 後端 MResult() 回傳 `Value = 99`（數字，`int` 型別）
- 前端檢查 `formData.MaintenanceResult === '99'`（字串）
- 嚴格相等比較失敗，導致「其他原因」輸入框不顯示、驗證邏輯不執行

## Goals / Non-Goals

**Goals:**
- 提取型別檢查邏輯為獨立方法，相容數字和字串
- 提升程式碼可讀性與可維護性
- 修復「其他原因」的顯示與驗證缺陷

**Non-Goals:**
- 修改後端 API 回傳型別（保持相容）
- 改變表單互動流程或驗證規則
- 新增功能

## Decisions

1. **提取 `isOtherReason()` 方法**
   - 邏輯：`val === 99 || val === '99'`
   - 位置：新增在 Vue methods 中，與其他驗證方法相鄰
   - 複用性：v-if 條件、handleSubmit 驗證、@change 回調都使用此方法

2. **關鍵修復點**
   - 第 148 行 v-if：改為 `isOtherReason()`
   - 第 404 行驗證：改為 `isOtherReason()`
   - 第 377-380 行 @change：補充邏輯或使用新方法

## Risks / Trade-offs

- **風險**：後端若改為回傳字串型別，此方法仍有效（向前相容）
- **緩解**：邏輯集中於一個方法，未來若移除數字檢查只需改一處
