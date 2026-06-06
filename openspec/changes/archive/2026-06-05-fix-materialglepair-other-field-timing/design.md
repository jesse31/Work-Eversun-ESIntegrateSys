# Design: Fix MaterialGunRepair Other Field Display Timing Issue

## Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│     MaterialGunRepair.cshtml 事件流程                 │
├─────────────────────────────────────────────────────┤
│                                                     │
│  頁面載入                                            │
│     │                                               │
│     ├─ $(document).ready()                          │
│     │                                               │
│     ├─ AJAX: Classification                         │
│     │                                               │
│     └─ AJAX: MResult                                │
│        ├─ Success callback:                         │
│        │  ├─ $('.MaintenanceResult').empty()        │
│        │  ├─ $.each() 添加選項                      │
│        │  ├─ .val(selectedMaintenanceResult)        │
│        │  └─ [新增] toggleOtherField() ← 立即執行   │
│        │                                            │
│        └─ change 監聽綁定                            │
│           └─ toggleOtherField()                     │
│                                                     │
│  結果：                                              │
│  初始化時 ✓ + 用戶選擇時 ✓                          │
│                                                     │
└─────────────────────────────────────────────────────┘
```

## Technical Decisions

### 決策 1：抽取 toggleOtherField() 函式

**選項**：
- A. 在 AJAX success 後手動設定 display（重複代碼）
- B. 使用 `.trigger('change')` 觸發事件（有副作用風險）
- **C. 抽取獨立函式，AJAX 和 change 都調用（選定）**

**理由**：
- DRY 原則：邏輯只維護一份
- 無副作用：不依賴事件系統，直接邏輯調用
- 易測試：可獨立驗證函式邏輯
- 高內聚：相關邏輯集中在一起

### 決策 2：型別比較使用嚴格相等

**現有代碼**：
```javascript
if (selectedValue == 99)  // ⚠️ 寬鬆比較，"99" == 99 → true
```

**改為**：
```javascript
if (parseInt(selectedValue) === 99)  // ✓ 明確轉型後嚴格比較
```

**理由**：
- 防止隱含的型別強制轉換
- 增加程式碼可讀性和穩定性
- 遵循 JavaScript 最佳實踐

### 決策 3：維持現有的 HTML 結構

**不改變**：
- `<select id="MaintenanceResult">` 及其事件綁定
- `<input id="Other" style="display:none">` 的初始狀態
- 防呆檢查邏輯（第 69-89 行）

**理由**：
- 最小化改動，降低迴歸風險
- 避免涉及 Controller 邏輯
- 保持向後相容性

## Implementation Strategy

### Phase 1：邏輯重構（純 JavaScript）

在 MaterialGunRepair.cshtml 中：

1. **建立 toggleOtherField() 函式**（新增）
   - 取得 MaintenanceResult 當前值
   - 型別轉換後與 99 比較
   - 設定 Other 的 display 屬性

2. **改進 MResult AJAX success 回調**（修改）
   - 現有邏輯保持不變
   - 在 `.val(selectedMaintenanceResult)` 後添加 `toggleOtherField()` 呼叫
   - 條件：`if (selectedMaintenanceResult)` 成立時執行

3. **改進 change 監聽綁定**（修改）
   - 抽取現有邏輯到 toggleOtherField()
   - change 事件回調中調用該函式

### Phase 2：驗證（手動測試）

- [ ] 新建維修紀錄，MaintenanceResult 選 99，檢查 Other 是否顯示
- [ ] 編輯既存紀錄（sno=1783），Other 應自動顯示
- [ ] 切換下拉選項，Other 應正確隱藏/顯示
- [ ] 提交表單時防呆檢查應通過

## Risks & Mitigations

| 風險 | 等級 | 緩解措施 |
|------|------|---------|
| AJAX 成功回調執行時序不確定 | 低 | 在 success 內同步執行 toggleOtherField()，確保時序 |
| 其他 AJAX (Classification) 的邏輯衝突 | 低 | 兩個 AJAX 獨立處理，不存在衝突 |
| 現有防呆檢查邏輯受影響 | 低 | 防呆邏輯不變，只是 Other 欄位的顯示時序修復 |
| 瀏覽器相容性問題 | 低 | 只用基礎 jQuery API (val, show, hide, change)，無相容性問題 |

## Open Questions

1. **Classification 下拉選單**是否也有類似的時序問題？需要檢查 ViewBag.Classic 的使用方式。
2. **表單提交時** Other 欄位值是否正確傳遞給 Controller？需要驗證 `name="Other"` 的綁定。

## Dependencies

- 無外部相依性
- 現有的 jQuery 3.4.1、Bootstrap 等不變
- Controller 邏輯不需修改
