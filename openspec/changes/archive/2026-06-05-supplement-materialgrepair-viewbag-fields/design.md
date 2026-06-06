# Design: 補齊後端 ViewBag 欄位傳遞

## Context

編輯已完修的料槍維修記錄時，用戶無法看到三個欄位的上次存檔值：
- Other（其他原因，當 MaintenanceResult = 99 時使用）
- ChangeItemName（更換部品名稱）
- ChangeItemNo（更換部品料號）

根本原因：後端 `MaterialGunController.MaterialGunRepair(GET)` 方法僅傳遞了 5 個 ViewBag（Eno、Classic、Results），遺漏了這三個欄位。因白名單允許編輯已完修記錄，所以需要補齊資料流以支援完整的編輯體驗。

## Goals / Non-Goals

**Goals:**
- 編輯頁面打開時，這三個欄位顯示上次存檔的值
- 用戶可以在這基礎上修改並重新提交
- 保持最小化改動，不重構現有邏輯

**Non-Goals:**
- 不修改資料庫 schema
- 不改變 Controller POST 方法簽名或驗證邏輯
- 不涉及前端防呆檢查邏輯（已由前次變更修復）
- 不新增任何新 API 端點

## Decisions

### D1. 後端傳遞方式：使用 ViewBag

**決策**：在 `MaterialGunController.MaterialGunRepair(GET)` 第 433-437 行添加三個 ViewBag 賦值。

**理由**：
- 既有架構已使用 ViewBag（Eno、Classic、Results 都用 ViewBag）
- 維持一致性，無需引入新的傳遞機制
- 最小化改動

**實現**：
```csharp
// 現有代碼（第 433-437 行）
ViewBag.Eno = result.MaterialGun_Sno;
ViewBag.Classic = result.Classification;
ViewBag.Results = result.MaintenanceResult;

// 新增代碼
ViewBag.Other = result.Other;
ViewBag.ChangeItemName = result.ChangeItemName;
ViewBag.ChangeItemNo = result.ChangeItemNo;
```

**代替方案考量**：
- **JSON API**：需新建 API 端點，額外複雜度不必要
- **Model 傳遞**：改為傳遞整個 model，但會改變既有架構
- **Hidden Input**：前端改用 hidden input，但增加 HTML 複雜度

### D2. 前端初始化方式：Razor 綁定 + HTML value 屬性

**決策**：在 View 中直接綁定 `@ViewBag.XXX` 到 HTML input 的 value 屬性。

**理由**：
- Razor 編譯時就完成資料綁定，邏輯清晰
- 無需額外的 JavaScript 初始化邏輯
- 與既有 HTML 結構保持一致（MaterialGun_Sno 也是用此方式）

**實現**：
```html
<!-- Other 欄位 -->
<input class="form-control" id="Other" name="Other" value="@ViewBag.Other" />

<!-- ChangeItemName 欄位 -->
<input class="form-control" id="ChangeItemName" name="ChangeItemName" value="@ViewBag.ChangeItemName" />

<!-- ChangeItemNo 欄位 -->
<input class="form-control" id="ChangeItemNo" name="ChangeItemNo" value="@ViewBag.ChangeItemNo" />
```

**代替方案考量**：
- **JavaScript 初始化**：需在 script 中動態設值，額外邏輯
- **默認空白**：不顯示舊值，違反編輯表單預期

### D3. JavaScript 邏輯：無改動

**決策**：前次變更已修復的 toggleOtherField() 邏輯保持不變，不需新增初始化代碼。

**理由**：
- 既有 AJAX success 回調已調用 toggleOtherField()，可正確處理初始值
- change 事件監聽已就位，用戶交互時邏輯正確執行
- 只需補齊資料流，不需修改邏輯

**驗證**：
當 Other input 有初始值 `value="@ViewBag.Other"` 且 MaintenanceResult = 99 時：
1. AJAX 設定選項完成後，toggleOtherField() 執行
2. toggleOtherField() 判斷 MaintenanceResult === 99，執行 show()
3. Other 欄位顯示，value 已由 Razor 綁定，用戶可見舊值

## Risks / Trade-offs

| 風險 | 等級 | 緩解措施 |
|------|------|---------|
| result 為 null（新建記錄）時，ViewBag 值為 null | 低 | HTML input 會接收 null，呈現為空字串，符合預期 |
| 編輯時若 result 在資料庫不存在 | 低 | Controller 已有 null 檢查（第 472-475 行），會返回錯誤 |
| 欄位長度超過 HTML 屬性限制 | 低 | 標準 HTML input 可容納 4096 字元，一般文本不會超過 |
| XSS 攻擊風險（若 ViewBag 未轉義） | 中 | Razor 默認 HTML 編碼，@ViewBag.Other 自動轉義，安全 |

## Migration Plan

### 部署步驟

1. **後端部署**
   - 在 `MaterialGunController.cs` 的 MaterialGunRepair(GET) 方法中添加三行代碼
   - 測試：編輯既存記錄，檢查 ViewBag 是否有值

2. **前端部署**
   - 在 `MaterialGunRepair.cshtml` 中更新三個 input 欄位的 value 綁定
   - 清除瀏覽器快取（Ctrl+Shift+Delete）
   - 測試：打開編輯頁面，檢查三個欄位是否顯示舊值

3. **測試場景**
   - S1：新建記錄 → 三個欄位為空
   - S2：編輯既存記錄（sno=1783，Other="馬達故障"）→ Other 欄位顯示 "馬達故障"
   - S3：修改值並提交 → 新值正確保存到資料庫

### 回滾策略

- 後端：移除三行 ViewBag 賦值（恢復原狀）
- 前端：移除 value 綁定（input 恢復為空）
- 無資料庫改動，無需復原步驟

## Open Questions

- 無明顯未決問題
- 假設：Controller.MaterialGunRepair(GET) 已正確載入 result 物件且所有欄位有值（若為 null 應已在業務邏輯中處理）
