# scan-repair-validation-deferral Specification

## Purpose
TBD - created by archiving change scan-repair-defer-validation. Update Purpose after archive.
## Requirements
### Requirement: Barcode Scan Query Only (No Repair Data Validation)

系統 SHALL 在掃碼時只進行料槍資訊查詢，不驗證維修資料是否填寫。維修資料驗證 SHALL 延遲到存檔時進行。

#### Scenario: User scans gun barcode successfully

- **WHEN** 使用者輸入有效的料槍編號並按 Enter 或自動查詢
- **THEN** 系統呼叫 GetGunByBarcode API 查詢料槍資訊
- **AND** 若查詢成功，系統直接帶出維修表單（Classification、MaintenanceResult、Other 欄位保持空白無驗證）
- **AND** 焦點自動進入 Classification 下拉框
- **AND** 無任何維修資料驗證提示

#### Scenario: User scans gun barcode that is already repaired (all_completed)

- **WHEN** 使用者掃描已全部完修的料槍編號
- **THEN** 系統呼叫 GetGunByBarcode API 並收到 status=all_completed
- **AND** 系統彈警告提示：「此料槍已全部完修，無待維修紀錄」
- **AND** 使用者點「確定」後，表單隱藏，掃碼框獲焦
- **AND** 無維修資料驗證提示

#### Scenario: User scans invalid or non-existent barcode

- **WHEN** 使用者輸入不存在的料槍編號
- **THEN** 系統呼叫 GetGunByBarcode API 並收到 error 狀態
- **AND** 系統彈錯誤提示：「查詢失敗」或相應的錯誤訊息
- **AND** 使用者點「確定」後，掃碼框清空並獲焦
- **AND** 無維修資料驗證提示

---

### Requirement: Repair Data Validation on Save (Deferred Validation)

系統 SHALL 在使用者點「儲存」時進行所有維修資料防呆驗證。包括分類、維修原因、其他原因等必填欄位。

#### Scenario: User clicks save without selecting classification

- **WHEN** 使用者填寫了掃碼編號但未選擇「檢修不良分類」
- **AND** 使用者點「儲存」按鈕
- **THEN** 系統檢查 formData.Classification 是否為空
- **AND** 系統彈警告提示：「尚未選擇檢修不良分類」
- **AND** 使用者點「確定」後，表單保留但停止提交

#### Scenario: User clicks save without selecting maintenance reason

- **WHEN** 使用者選擇了「檢修不良分類」但未選擇「檢修不良原因」
- **AND** 使用者點「儲存」按鈕
- **THEN** 系統檢查 formData.MaintenanceResult 是否為空或 "0"
- **AND** 系統彈警告提示：「尚未選擇檢修不良原因」
- **AND** 使用者點「確定」後，表單保留但停止提交

#### Scenario: User selects "Other" but doesn't fill in reason

- **WHEN** 使用者選擇「檢修不良原因」為「99」(其他)
- **AND** 未填寫「其他原因」輸入框
- **AND** 使用者點「儲存」按鈕
- **THEN** 系統檢查 formData.Other 是否為空
- **AND** 系統彈警告提示：「尚未輸入其他原因」
- **AND** 使用者點「確定」後，表單保留但停止提交

#### Scenario: User fills all required fields and clicks save

- **WHEN** 使用者完整填寫所有必填欄位（分類、原因、若選「99」則填其他原因）
- **AND** 使用者點「儲存」按鈕
- **THEN** 系統驗證通過，提交 AJAX 至 MaterialGunRepair POST API
- **AND** 後端執行業務驗證（料槍 Chk 狀態、資料完整性等）
- **AND** 若後端驗證成功，儲存完成並顯示「儲存成功」提示
- **AND** 1.5 秒後自動返回清單頁

---

### Requirement: Validation Feedback using SweetAlert

系統 SHALL 在防呆驗證失敗時，使用 Swal.fire() 彈提示框，格式統一。

#### Scenario: Display validation warning

- **WHEN** 前端驗證發現必填欄位未填
- **THEN** 系統使用 Swal.fire() 彈提示框
- **AND** 提示框格式為：
  ```javascript
  {
    icon: 'warning',
    title: '提示',
    text: '<相應的驗證失敗訊息>',
    confirmButtonText: '確定'
  }
  ```
- **AND** 使用者點「確定」後，提示框關閉，表單保留當前狀態

#### Scenario: Display error message

- **WHEN** 掃碼時查詢失敗或伺服器錯誤
- **THEN** 系統使用 Swal.fire() 彈提示框
- **AND** 提示框格式為：
  ```javascript
  {
    icon: 'error',
    title: '<錯誤標題>',
    text: '<錯誤訊息>',
    confirmButtonText: '確定'
  }
  ```

---

### Requirement: Consistent Flow with MaterialGunRepair

掃碼維修流程 SHALL 與既有的單筆維修（MaterialGunRepair）功能保持邏輯一致。

#### Scenario: Both ScanRepair and MaterialGunRepair use same validation rules

- **WHEN** 對比 ScanRepair.cshtml 與 MaterialGunRepair.cshtml 的驗證邏輯
- **THEN** 兩者都驗證分類、原因、其他原因必填
- **AND** 兩者都在存檔時驗證（不在查詢/載入時驗證）
- **AND** 後端都呼叫 MaterialGunRepair POST API 執行最終業務驗證

#### Scenario: Both use appropriate UI feedback mechanisms

- **WHEN** ScanRepair 使用 Swal.fire() 提示
- **THEN** MaterialGunRepair 可保留 alert()（舊代碼）
- **AND** 新代碼優先使用 Swal 標準化提示方式

