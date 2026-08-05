## Context

報價查詢頁面中，使用者勾選「報價中」checkbox 時會觸發後端更新，但隨後執行 `window.location.assign(window.location.href)` 重新整理整個頁面。由於昨日改動引入了「初次進入頁面不自動查詢」的邏輯，導致重新整理後 Model 為空，頁面顯示「請設定查詢條件後點擊查詢」，使用者體驗不佳。

涉及的檔案：
- `Controllers/QuoteScheduleController.cs` - `HandleCheckboxChange()` 方法（第 545-590 行）
- `Views/QuoteSchedule/_QuotePartialView.cshtml` - HTML 標記（第 183 行）與 JavaScript 邏輯（第 325-351 行）

## Goals / Non-Goals

**Goals:**
- 改用 AJAX 部分 DOM 更新，避免整個頁面刷新
- 新增權限檢驗：只有原勾選者才能取消勾選
- AJAX 失敗時自動還原 checkbox 狀態
- 用 SweetAlert2 提供友善提示視窗（自動關閉）
- 改動範圍最小化，保持現有架構不變

**Non-Goals:**
- 不改變業務邏輯（IE 負責人的角色、報價流程等）
- 不涉及資料表結構修改
- 不涉及其他模組（只限 QuoteSchedule）

## Decisions

### Decision 1：AJAX 部分 DOM 更新 vs 整個頁面刷新

**選擇**：AJAX 部分 DOM 更新

**替代方案對比**：
| 方案 | 優點 | 缺點 |
|------|------|------|
| **AJAX 部分更新** ✓ | 流暢無延遲、保持頁面狀態、無 UX 中斷 | 需要 DOM 操作、需要後端回傳額外欄位 |
| 整個頁面刷新 | 簡單（已有邏輯）、伺服器負擔輕 | UI 清空、查詢參數遺失、使用者體驗差 |
| 帶查詢參數重新整理 | 保持查詢結果 | 需要從 Session 或其他地方恢復條件 |

**理由**：方案A（部分更新）提供最佳使用者體驗。根據 explore 階段的分析，這個改動原本的設計也傾向於此。

---

### Decision 2：權限檢驗邏輯位置

**選擇**：後端驗證（Server-side validation），使用 Session 中的 uId，不採用前端傳來的 uId

**實現**：
- 後端 `HandleCheckboxChange()` 第 548 行已經從 `Session["Member"]` 取得當前登入者 uId
- 直接使用這個 Session-based uId 做權限比對：
  - 如果 `isChecked = false` 且 `ES_QuoteForIE.IEonwer != uId`（Session 值）→ 回傳錯誤
  - 否則允許操作
- 前端**不需要**也**不應該**傳遞 uId 作為權限判斷依據（避免被竄改冒充身份）

**理由**：
- 安全性：Session 中的 uId 已通過登入驗證，前端傳來的任何 uId 都可能被竄改，用來做權限判斷會造成認證繞過風險（惡意使用者可冒充他人身份取消勾選）
- 簡化：不需要新增 uId 參數，減少攻擊面
- 可靠性：後端掌握最新的 IEonwer 值（並發安全）
- 前端獲得清晰的錯誤訊息以提示使用者

---

### Decision 3：前端回傳使用者名稱的來源

**選擇**：前端傳遞 `@ViewBag.Name`（僅供顯示用途，非權限判斷依據）

**實現**：
```javascript
var ieonwerName = "@ViewBag.Name";  // 當前登入者姓名，來自 Session，僅用於前端顯示
$.ajax({
    data: {
        sno: recordId,
        isChecked: isChecked,
        ieonwerName: ieonwerName  // 僅用於前端 DOM 顯示，後端權限驗證一律使用 Session 的 uId
    }
});
```

**理由**：
- ViewBag.Name 來自已驗證的 Session（安全）
- 避免多一次 DB 查詢
- 只在勾選時需要（取消時欄位清空，無需名稱）
- 名稱僅用於顯示，不參與任何權限判斷，故竄改風險不影響安全性

---

### Decision 4：失敗還原機制

**選擇**：前端記住原狀態，失敗時 DOM 還原

**實現**：
```javascript
var originalState = !isChecked;  // 翻轉前的狀態
// AJAX 失敗時：
$("#handleCheckbox_" + recordId).prop("checked", originalState);
```

**理由**：
- 簡單可靠（無需後端協助）
- 快速反應（使用者立即看到還原）
- 防止 UI 與資料庫不同步

---

### Decision 5：UX 提示機制

**選擇**：SweetAlert2（替代 `alert()`）

**實現**：
- 成功時：icon=success，自動關閉（1.5 秒）
- 取消時：icon=info，自動關閉（1.5 秒）
- 失敗時：icon=error，等待使用者確認

**理由**：
- 項目已使用 SweetAlert2（匯出功能），保持一致性
- 自動關閉改善使用者體驗（無需手動確認）
- 圖標視覺化提示結果

---

## Risks / Trade-offs

| 風險 | 影響 | 緩解策略 |
|------|------|--------|
| **AJAX 請求失敗** | 使用者感受到操作未執行，但 UI 可能已改變 | 失敗時自動還原 checkbox 狀態；清晰的錯誤提示 |
| **並發勾選同一筆** | 兩個使用者同時勾選，後來者覆蓋先前者 | 後端 `FirstOrDefault()` 已處理；若需嚴格防止，考慮樂觀鎖或版本控制 |
| **使用者名稱與 ID 不同步** | 若 Session 延遲更新，顯示的名稱可能過時 | 名稱來自 Session（每次登入重新設置），通常不會發生 |
| **前端傳遞使用者名稱可被篡改** | 攻擊者修改 JavaScript 傳送假名稱 | 後端權限驗證一律使用 Session 的 uId（不信任任何前端傳入值），前端傳來的名稱僅用於顯示 |

---

## Migration Plan

**步驟**：
1. 修改 `_QuotePartialView.cshtml`：
   - 第 183 行：加 id 屬性 → `<td id="ieowner_@item.Sno">`
   - 第 325-351 行：重寫 checkbox change 事件邏輯
2. 修改 `QuoteScheduleController.cs`：
   - 第 545 行：方法簽名加參數 `uId`, `ieonwerName`
   - 第 550 行：新增權限檢驗邏輯
   - 第 589 行：回傳值擴充 `ieonwer`, `iestatus`
3. 重新編譯並部署
4. 清瀏覽器快取（避免舊 JavaScript 被載入）

**回滾策略**：
- 若出現問題，恢復提交前的版本
- 無資料遷移，無需清理工作

**後向相容性**：
- 回應 JSON 增加欄位（`ieonwer`, `iestatus`），但現有欄位保留
- 前端只讀新欄位，不影響其他功能

---

## Open Questions

1. **並發場景**：若兩個使用者同時勾選同一筆，目前的 `FirstOrDefault()` 會怎樣處理？需要樂觀鎖嗎？
   - *目前假設*：不需要（業務上不常見），但可在將來優化

2. **其他模組相容性**：是否有其他頁面或模組呼叫 `HandleCheckboxChange()`？
   - *目前假設*：只有 _QuotePartialView.cshtml 使用

3. **SweetAlert2 CSS**：前端已載入 `sweetalert2.all.min.js` 嗎？
   - *目前假設*：是（_QuotePartialView.cshtml 第 45 行已引入）
