# fix-quote-schedule-checkbox-ui 歸檔 MANIFEST

**歸檔日期**：2026-08-05  
**Schema**：spec-driven  
**狀態**：部份完成，帶已知問題歸檔

## 交付成果

✅ **已完成**：
- 後端 Controller 改動（`HandleCheckboxChange` 權限驗證、回傳擴充）
- 前端 HTML 標記改動（IE負責人欄位加 id）
- 前端 AJAX 邏輯重寫（移除整頁刷新、改用 DOM 部分更新）
- SweetAlert2 整合（友善提示視窗）
- MSBuild 編譯驗證通過
- 跨帳號可見性測試（已排查結案）

✅ **已寫入代碼**：
- `ESIntegrateSys/Controllers/QuoteScheduleController.cs`：第 545 行（方法簽名+參數）、第 557-560 行（權限檢驗）、第 598 行（回傳擴充）
- `ESIntegrateSys/Views/QuoteSchedule/_QuotePartialView.cshtml`：第 183 行（IE負責人欄位加 id）、第 325-371 行（checkbox change 事件重寫）

## 已知問題

### 問題 1：勾選後無法再取消勾選（Task 8.2）

**症狀**：使用者勾選 checkbox 後，會禁用該 checkbox，導致無法再次點擊取消勾選。

**根因**：`_QuotePartialView.cshtml` 第 350 行執行 `$checkbox.prop("disabled", true);`，被禁用的 checkbox 永遠不會觸發 `change` 事件。

**修正方案**（未套用）：
1. 移除第 350 行的 `$checkbox.prop("disabled", true);`
2. 改由後端權限驗證把關（已於 Task 2.2 實作）
3. 修正 `specs/quote-schedule-ie-status-management/spec.md` 中自相矛盾的敘述

**後續建議**：在 sprint 計劃中單獨拉 task 完成 Task 8.2.1-8.2.3 的修正與重測。

---

## 測試與驗收進度

| Task | 狀態 | 備註 |
|------|------|------|
| 1-4, 2.4, 5.1 | ✅ | 代碼實作、編譯驗證 |
| 5.2 | ✅ | 跨帳號可見性確認正常（分頁/排序造成的誤判已排查） |
| 5.3 | ⏳ | 待 Task 8.2 修正後重測 |
| 5.4 | ⏳ | 待 Task 8.2 修正後重測 |
| 5.5 | ⏳ | 待執行 |
| 6.1-6.7 | ⏳ | 待執行 |
| 7.1-7.3 | ⏳ | 待執行 |
| 8.2.1-8.2.3 | ⏳ | **待修正** |

---

## 檔案索引

- `proposal.md`：為什麼要改、改什麼、影響什麼
- `design.md`：技術決策、權限驗證邏輯、Session-based 安全檢驗（已修正為使用 Session uId 而非前端傳入 uId）
- `specs/quote-schedule-ie-status-management/spec.md`：系統需求與驗收條件（待 Task 8.2.2 同步修正）
- `tasks.md`：實作任務清單與進度（包含第 8 章實測發現記錄）

---

## 後續建議

1. **立即後續**（Task 8.2）：在下個 sprint 完成「移除 disabled、重測取消流程、權限驗證」
2. **測試完善**（Tasks 5.3-5.5）：完成 E2E 測試、Network/Console 驗證
3. **上線準備**（Tasks 6.1-6.7）：PR 審查、測試環境驗收、正式部署
4. **文檔更新**（Tasks 7.1-7.3）：代碼註解、Git commit message

---

## 代碼安全驗證（CLAUDE.md 規範檢查）

✅ 權限驗證使用 Session-based uId，不信任前端傳入值  
✅ 無 SQL 字串拼接  
✅ 無 hardcode 密碼/金鑰  
✅ 所有 I/O 操作為同步（符合既有 EF Core DbContext 同步介面相容性）  
✅ XML 註解更新於 Controller 方法（Task 2 完成時補上）
