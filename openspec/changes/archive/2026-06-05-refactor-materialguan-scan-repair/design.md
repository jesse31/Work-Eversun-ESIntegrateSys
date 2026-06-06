## Context

MaterialGunScanRepair.cshtml 中存在以下技術債務：
1. 焦點重設邏輯重複 4 次（第 319-321、340-351、347-350、374-376 行）
2. `MaintenanceResult` 型別檢查鬆散，同時檢查字串 '99' 和數字 99

## Goals / Non-Goals

**Goals:**
- 消除焦點邏輯重複，提高可維護性
- 統一 `MaintenanceResult` 的型別處理，簡化型別檢查邏輯
- 保持 100% 行為相容，無使用者可見變更

**Non-Goals:**
- 修改表單互動流程或驗證邏輯
- 改變 UI 外觀或使用者體驗
- 新增功能或重新設計架構

## Decisions

1. **焦點邏輯提取為方法**
   - 建立 `focusOnScanInput()` 方法，集中管理焦點邏輯
   - 取代所有 4 處重複的 `this.$nextTick(() => { this.$refs.scanInputRef.focus(); })`
   - 優勢：DRY 原則，單一責任，易於維護與測試

2. **型別統一為字串**
   - 確保 API 回傳的 `MaintenanceResult` 一律為字串
   - 檢查改為 `=== '99'`（移除 `=== 99` 的判斷）
   - 優勢：消除型別歧義，簡化條件判斷，減少潛在 Bug

## Risks / Trade-offs

- **風險**：若 API 側已回傳數字型別 99，需確認後端是否已改為字串
- **緩解**：透過驗證後端 API 回傳值，在必要時補充型別轉換邏輯
