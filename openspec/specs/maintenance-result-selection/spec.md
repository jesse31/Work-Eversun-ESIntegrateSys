# Maintenance Result Selection Capability Spec

## Purpose

修改既存的「檢修不良原因」下拉選單交互邏輯，使其在 AJAX 初始化時也能正確觸發相關 UI 邏輯（如 Other 欄位顯示）。

**應用場景**：
- MaterialGun 維修頁面編輯時，初始化時若 MaintenanceResult = 99，Other 欄位應自動顯示
- 防止事件時序問題導致的使用者困擾

---

## Requirements

### R1. 向後相容

**Requirement Statement**: 既有的 Controller 邏輯、HTML 結構和防呆檢查應完全不變。

**Given** 現有的 Controller 邏輯和 HTML 結構保持不變  
**When** JavaScript 邏輯升級  
**Then** 現有的表單提交、驗證流程應完全相同

**驗收標準**：
- Controller 的 MaterialGunRepair(POST) 方法無修改
- HTML 欄位名稱、ID 無變更
- 防呆檢查邏輯無變更
- 無 breaking changes

---

### R2. 初始化和交互的一致性

**Requirement Statement**: 初次載入和用戶重新選擇時，Other 欄位的可見性應一致，且需要顯示上次存檔的 Other 值。

**Given** 頁面初次載入編輯既存記錄，MaintenanceResult = 99，後端傳遞 Other = "馬達故障"  
**When** 頁面完全載入  
**Then** Other 欄位顯示且包含值 "馬達故障"

**驗收標準**：
- 後端傳遞 ViewBag.Other（及 ChangeItemName、ChangeItemNo）
- 前端初始化 JavaScript 變數：`var selectedOther = '@ViewBag.Other'`
- Other input 欄位綁定 value：`value="@ViewBag.Other"`
- 初始化後 Other.display = "block"（或 show）且 input.value = "馬達故障"
- 重新選擇 99 後 Other 欄位保留用戶輸入的值
- 選擇其他值後 Other.display = "none"
- 再次選擇 99 後 Other.display = "block"

**實現方式**：
1. Controller GET 方法補充三個 ViewBag 賦值
2. Razor View 初始化時從 ViewBag 取值
3. Input 欄位綁定初始值
4. 共用的 toggleOtherField() 函式繼續在 AJAX 初始化和 change 事件中調用

---

### R3. 防呆檢查應正常通過

**Requirement Statement**: 當 MaintenanceResult = 99、Other 有值且 ChangeItemName、ChangeItemNo 正確傳遞時，前端防呆應通過，表單成功提交。

**Given** MaintenanceResult = 99、Other = "馬達故障"、ChangeItemName = "馬達"、ChangeItemNo = "M123"  
**When** 用戶提交表單  
**Then** 所有欄位值正確傳遞到後端，表單成功提交

**驗收標準**：
- 不出現「尚未輸入原因」的 alert
- 表單成功提交到 Controller
- Controller 的 MaterialGunRepair(POST) 方法正確接收所有參數：sno、MaterialGun_Sno、Classification、MaintenanceResult、Other、ChangeItemName、ChangeItemNo、b_Chk
- 資料庫成功保存新值

---

### R4. 改動最小化

**Requirement Statement**: 僅修復初始化時序問題，最小化程式碼變更，保持易讀性。

**Given** 只需修復初始化時序問題  
**When** 實施改動  
**Then** 應最小化程式碼變更，保持程式碼易讀性

**驗收標準**：
- 新增代碼 < 20 行
- 邏輯簡潔明了，易於維護
- 無重構現有 HTML 或 Controller

---

### R5. 後端傳遞完整的編輯資料

**Requirement Statement**: 當編輯既存的維修記錄（白名單允許）時，後端應傳遞所有必要的欄位值，使前端能展示完整的舊資料供用戶修改。

**Given** 用戶訪問 /MaterialGun/MaterialGunRepair?sno=1783 編輯既存記錄  
**When** Controller GET 方法執行  
**Then** 後端應設定以下 ViewBag 供前端使用

#### Scenario: 傳遞 Other 欄位值
- **WHEN** 維修記錄中 MaintenanceResult = 99 且 Other = "馬達故障"
- **THEN** ViewBag.Other = "馬達故障"，前端可在 Other input 中顯示此值

#### Scenario: 傳遞 ChangeItemName 欄位值
- **WHEN** 維修記錄中 ChangeItemName = "馬達"
- **THEN** ViewBag.ChangeItemName = "馬達"，前端可在 ChangeItemName input 中顯示此值

#### Scenario: 傳遞 ChangeItemNo 欄位值
- **WHEN** 維修記錄中 ChangeItemNo = "M123"
- **THEN** ViewBag.ChangeItemNo = "M123"，前端可在 ChangeItemNo input 中顯示此值

#### Scenario: 新建記錄時欄位為空
- **WHEN** 建立新維修記錄（result = null）
- **THEN** ViewBag.Other、ViewBag.ChangeItemName、ViewBag.ChangeItemNo 均為 null 或空字串，前端 input 顯示空值

---

## Verification Scenarios

| # | 場景 | 預期結果 | 驗證方式 |
|---|------|--------|--------|
| S1 | 打開 /MaterialGun/MaterialGunRepair?sno=1783 | Other 欄位自動顯示（如記錄的 MaintenanceResult=99） | 載入後不操作，檢查 Other 可見性 |
| S2 | 選擇 MaintenanceResult = 0（請選擇） | Other 欄位隱藏 | change 事件觸發後檢查 |
| S3 | 選擇 MaintenanceResult = 99 | Other 欄位顯示 | change 事件觸發後檢查 |
| S4 | 選擇其他值，然後選 99 | Other 先隱藏後顯示 | 觀察 UI 行為 |
| S5 | MaintenanceResult=99、Other="測試原因"，提交 | 表單成功提交，no alert | 提交後檢查頁面導向或提示 |
| S6 | MaintenanceResult=99、Other=""，提交 | alert 出現「尚未輸入原因」 | 防呆檢查正常 |

---

## Non-Functional Requirements

- **Performance**: 無性能下降
- **Compatibility**: 不增加任何相容性問題
- **Browser Support**: jQuery 3.4.1、IE 11+

---

## Related Capabilities

- **ui-field-visibility-toggle**: 通用的欄位可見性切換機制，本 capability 的實現基礎

---

## Implementation Notes

### Current State (Before)
- AJAX 從 MResult() API 動態載入選項
- 使用 `.val(selectedMaintenanceResult)` 設定初始值
- 監聽 `change` 事件以觸發 Other 欄位顯示/隱藏邏輯
- **問題**：`.val()` 設值不觸發 `change` 事件，導致初始化時 Other 欄位未顯示

### Changes Required

**C1. AJAX Success 回調中添加顯示邏輯執行**
- 在設定值後立即調用 toggleOtherField()
- 確保初始化時Other 欄位正確顯示

**C2. change 事件監聽中使用獨立函式**
- 改進 change 監聽以調用 toggleOtherField()
- 確保用戶交互時邏輯正確執行

**C3. 型別安全的值比較**
- 在 toggleOtherField() 中使用 `parseInt(selectedValue) === 99`
- 防止隱含的型別強制轉換

---

## Risks & Mitigations

| 風險 | 等級 | 緩解措施 |
|------|------|---------|
| AJAX 成功回調執行時序不確定 | 低 | 在 success 內同步執行 toggleOtherField()，確保時序 |
| 其他 AJAX (Classification) 的邏輯衝突 | 低 | 兩個 AJAX 獨立處理，不存在衝突 |
| 現有防呆檢查邏輯受影響 | 低 | 防呆邏輯不變，只是 Other 欄位的顯示時序修復 |
| 瀏覽器相容性問題 | 低 | 只用基礎 jQuery API (val, show, hide, change)，無相容性問題 |

---

## Migration & Deployment

- **Database Changes**: 無
- **API Changes**: 無
- **Breaking Changes**: 無
- **Deployment Steps**: 直接部署 JavaScript 修改，無需特殊步驟
