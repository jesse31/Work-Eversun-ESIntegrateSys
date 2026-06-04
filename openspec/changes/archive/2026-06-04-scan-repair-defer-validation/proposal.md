# Proposal: Scan Repair Validation Deferral

## Why

掃碼維修流程目前在掃碼時就進行防呆驗證（檢查維修資料是否填寫），導致使用者每次掃碼都被迫填寫所有必填欄位才能掃下一筆。這違反了「掃碼就是掃碼」的簡單原則。

改進方向：掃碼時只查詢料槍資訊，所有防呆驗證延遲到存檔時進行。這樣掃碼流程更輕快，使用者體驗更順暢。

## What Changes

- **掃碼流程（queryGun）**：
  - ✅ 系統 SHALL 保持現有邏輯：成功帶表單、all_completed 彈警告、失敗彈錯誤
  - ✅ 系統 SHALL NOT 在掃碼時進行維修資料檢查

- **存檔流程（handleSubmit）**：
  - ✅ 系統 SHALL 保持現有邏輯：驗證分類、驗證原因、驗證其他原因必填
  - ✅ 系統 SHALL 繼續使用 Swal.fire() 彈提示

- **設計決策**：
  - 確認掃碼只負責「查詢」，不負責「驗證」
  - 確認所有業務驗證集中在存檔時進行
  - 確認邏輯與既有的 MaterialGunRepair 單筆維修流程保持一致

## Capabilities

### New Capabilities

- `scan-repair-validation-deferral`：系統 SHALL 在掃碼時只進行查詢，所有維修資料驗證 SHALL 延遲至存檔時進行，以簡化掃碼體驗

### Modified Capabilities

（無 - 此變更只是確認現有設計，不改變 API 或資料結構）

## Impact

- **受影響代碼**：
  - `ESIntegrateSys/Views/MaterialGun/MaterialGunScanRepair.cshtml`：Vue 前端邏輯確認與文件
  - `ESIntegrateSys/Controllers/MaterialGunController.cs`：GetGunByBarcode / MaterialGunRepair 後端邏輯確認

- **使用者體驗**：
  - 掃碼流程 SHALL 更快速、更專注
  - 驗證提示 SHALL 集中在存檔時，減少中斷

- **測試**：
  - 需驗證掃碼不檢查維修資料
  - 需驗證存檔時才進行防呆驗證
