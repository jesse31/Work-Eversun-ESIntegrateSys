# material-gun-repair-audit-log Specification

## Purpose
TBD - created by archiving change add-material-gun-repair-whitelist. Update Purpose after archive.
## Requirements
### Requirement: 訪問日誌紀錄
當使用者訪問 `/MaterialGun/MaterialGunRepairView` 或 `/MaterialGun/MaterialGunRepair?sno=xxx` 時，系統 SHALL 使用 Serilog 記錄訪問事件。

#### Scenario: 使用者進入維修列表頁面
- **WHEN** 使用者進入 `/MaterialGun/MaterialGunRepairView`
- **THEN** 系統記錄 Information 級別日誌：`User {UserId} accessed material gun repair view`

#### Scenario: 使用者進入維修編輯頁面
- **WHEN** 白名單人員進入 `/MaterialGun/MaterialGunRepair?sno=123`
- **THEN** 系統記錄 Information 級別日誌：`User {UserId} accessed repair record for MaterialGun Sno {Sno}`

### Requirement: 成功編輯日誌紀錄
當使用者成功提交維修編輯表單時，系統 SHALL 記錄成功事件及編輯的欄位摘要。

#### Scenario: 白名單人員成功編輯已完成維修料槍
- **WHEN** 白名單帳號提交編輯已完成維修 (Chk=1) 料槍的表單，且後端驗證通過
- **THEN** 系統記錄 Information 級別日誌：`User {UserId} successfully updated repair record {Sno}, changes: {summary}`

#### Scenario: 任何人員成功編輯未維修料槍
- **WHEN** 任何人員提交編輯未維修 (Chk=0) 料槍的表單，且後端驗證通過
- **THEN** 系統記錄 Information 級別日誌：`User {UserId} successfully updated repair record {Sno}, changes: {summary}`

### Requirement: 白名單驗證失敗日誌紀錄
當使用者因白名單驗證失敗而被拒絕時，系統 SHALL 記錄 Warning 級別日誌。

#### Scenario: 非白名單人員試圖編輯已完成維修料槍
- **WHEN** 非白名單帳號提交編輯已完成維修 (Chk=1) 料槍的表單
- **THEN** 系統記錄 Warning 級別日誌：`User {UserId} denied access to repair record {Sno}: Not whitelisted for editing completed repairs`

#### Scenario: 非白名單人員試圖透過 URL 直接訪問已完成維修編輯頁面
- **WHEN** 非白名單帳號試圖直接訪問 `/MaterialGun/MaterialGunRepair?sno=123`，其中該料槍已完成維修 (Chk=1)
- **THEN** 系統記錄 Warning 級別日誌：`User {UserId} attempted unauthorized access to repair record {Sno}`

### Requirement: 日誌內容結構化
所有稽審日誌 SHALL 包含以下結構化欄位，以利後續分析：

#### Scenario: 日誌包含必要欄位
- **WHEN** 系統記錄任何維修操作日誌
- **THEN** 日誌包含欄位：`UserId`、`MaterialGunSno`、`操作類型 (AccessView|AccessEdit|EditSuccess|EditFailed)`、`失敗原因 (如適用)`、`時間戳`

### Requirement: 敏感資訊遮罩
日誌 SHALL 不記錄任何敏感資訊（如密碼、Token），編輯的欄位摘要 SHALL 對個資與敏感欄位進行遮罩處理。

#### Scenario: 編輯日誌不洩漏敏感資料
- **WHEN** 使用者編輯維修紀錄中包含任何敏感欄位
- **THEN** 日誌記錄變更摘要時，僅記錄欄位名稱和基本資訊（如長度、類型），不記錄實際內容

