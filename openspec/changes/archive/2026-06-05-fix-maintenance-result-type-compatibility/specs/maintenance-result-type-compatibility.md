## 維修結果型別相容性

此變更在前端加入型別相容層，用以處理後端 API 回傳的 `MaintenanceResult` 可能為數字（99）或字串（"99"）的情況。

## 新增需求

### 要求：不分型別的「其他原因」檢測

系統應提供一個方法，能可靠偵測 `MaintenanceResult` 是否等於 99（代表「其他」選項），不論該值為數字或字串型別。

#### 情境：API 回傳數字值
- **WHEN** 後端回傳 `MaintenanceResult` 為整數 99
- **THEN** 檢測方法回傳 true，並顯示「其他原因」輸入欄位

#### 情境：API 回傳字串值
- **WHEN** 後端回傳 `MaintenanceResult` 為字串 "99"
- **THEN** 檢測方法回傳 true，並顯示「其他原因」輸入欄位

#### 情境：其他值
- **WHEN** `MaintenanceResult` 為除 99 或 "99" 之外的任何值
- **THEN** 檢測方法回傳 false，並隱藏「其他原因」輸入欄位
