# Design: Scan Repair Validation Deferral

## Context

**現狀**：
- ScanRepair.cshtml 前端使用 Vue 3 管理掃碼與維修表單
- queryGun() 方法負責掃碼查詢 → GetGunByBarcode 後端 API
- handleSubmit() 方法負責表單提交 → MaterialGunRepair 後端 API

**關鍵發現**：
- queryGun() 現在只負責「查詢」，不檢查維修資料
- handleSubmit() 已有完整的防呆驗證（分類、原因、其他原因）
- 邏輯已符合預期設計：掃碼查詢 → 存檔驗證

**對標**：
- MaterialGunRepair.cshtml（單筆維修）的存檔驗證模式是正確示範

---

## Goals / Non-Goals

**Goals：**
- ✅ 確認掃碼流程不檢查維修資料
- ✅ 確認所有防呆驗證集中在存檔時進行
- ✅ 確認邏輯與 MaterialGunRepair 保持一致
- ✅ 建檔設計決策與規格，形成清晰的架構文件

**Non-Goals：**
- ❌ 不修改現有代碼（代碼已正確）
- ❌ 不改變 API 契約或資料結構
- ❌ 不新增業務驗證邏輯（只確認現有邏輯）

---

## Decisions

### D1：掃碼流程職責（Query Only）

**決策**：掃碼時只負責「查詢料槍資訊」，不做維修資料驗證

**方案對比**：
| 方案 | 掃碼時驗證 | 存檔時驗證 | 使用者體驗 | 推薦度 |
|------|----------|---------|---------|-------|
| A | ✓ | ✓ | 冗餘驗證，掃碼慢 | ❌ |
| B | ✓ | ✗ | 只在掃碼驗證，存檔信任 | ⚠️ |
| **C** | **✗** | **✓** | **掃碼快速，存檔嚴格** | **✅** |

**選擇 C 的理由**：
- 掃碼即掃碼，讓使用者快速進行
- 驗證集中在存檔，確保資料完整性
- 符合關注點分離（SoC）原則

**實作**：
```
queryGun() 流程
├─ 1. 驗證掃碼框非空 ✓（掃碼前提）
├─ 2. 呼叫 GetGunByBarcode
├─ 3. 判斷查詢結果
│  ├─ success → isScanned=true（帶表單）
│  ├─ all_completed → 彈警告（料槍狀態，非維修資料）
│  └─ error → 彈錯誤（查詢失敗原因）
└─ 無維修資料檢查 ✅
```

---

### D2：存檔驗證集中（Multi-Field Validation）

**決策**：所有防呆驗證在 handleSubmit() 集中執行

**驗證清單**（當前已有）：
```javascript
handleSubmit()
├─ 驗證分類必選（!Classification）
├─ 驗證原因必選（!MaintenanceResult）
├─ 驗證其他原因（MaintenanceResult===99 && !Other）
└─ 都使用 Swal.fire() 彈提示 + return false
```

**後端層面**（MaterialGunRepair POST）：
- 後端應再驗一次：料槍 Chk 狀態、資料完整性
- 後端驗證失敗時拒絕存檔

---

### D3：提示方式一致性

**決策**：保持使用 Swal.fire() 彈提示（不改 alert()）

**理由**：
- ScanRepair 已用 Swal，保持一致性
- 提示效果更佳，使用者體驗更好
- MaterialGunRepair 用 alert() 是舊代碼，新代碼應升級

**格式**：
```javascript
// 現有格式（保留）
Swal.fire({
    icon: 'warning',
    title: '提示',
    text: '尚未選擇檢修不良分類',
    confirmButtonText: '確定'
});
```

---

## Risks / Trade-offs

| 風險 | 影響 | 預防/處理 |
|------|------|---------|
| 後端未驗證料槍 Chk 狀態 | 已完修的料槍也能存檔 | 後端 MaterialGunRepair 必須檢查 Chk 欄位 |
| 使用者掃錯編號但沒發現 | 維修記錄綁到錯誤料槍 | 掃碼成功後直接帶出料槍編號唯讀欄位供檢視 |
| 存檔前需填多個欄位 | 使用者感覺繁瑣 | 表單設計簡潔，焦點自動進入下一欄位 |

---

## Migration Plan

**部署步驟**：
1. 代碼無需修改（設計已正確）
2. 建檔本 design.md 與規格文件
3. 測試驗證現有邏輯符合預期
4. 文件歸檔，形成架構參考

**回滾策略**：
- 無需回滾（設計確認，無代碼變更）
- 若後續發現問題，修改 handleSubmit() 驗證邏輯

---

## Open Questions

1. **後端驗證**：MaterialGunRepair POST 是否已驗證料槍 Chk 狀態？
   - 需確認 GetGunByBarcode 後端邏輯
   
2. **其他原因欄位寬度**：當 MaintenanceResult === 99 時，Other 輸入框的字數限制？
   - 建議 50-100 字

3. **下拉重新查詢**：切換料槍後，下拉選項要重新查詢嗎？
   - 現狀：掃碼成功後已載入，handleClear() 時重置
