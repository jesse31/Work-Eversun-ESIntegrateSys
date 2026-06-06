# 料槍維修白名單擴展 - Specification (Delta)

## ADDED Requirements

### Requirement: 已完成維修記錄的編輯授權

已完成維修記錄（Chk=1）的編輯 SHALL 受到授權檢查。現在擴展為支援部門授權，使用 OR 邏輯：帳號在白名單或部門在授權清單中，即允許編輯。

**Previous Behavior:**
- 只有帳號在白名單 (RepairEditWhitelist) 中的使用者才能編輯已完成維修記錄
- 白名單包含特定帳號如 "02898"

**Updated Behavior:**
- 帳號在白名單 **或** 部門在授權清單中的使用者可編輯已完成維修記錄
- 白名單分為兩部分：RepairEditWhitelistUsers（帳號）、RepairEditWhitelistDepts（部門）
- 部門授權清單包含 "IT"
- 使用 OR 邏輯組合：`(userId in WhitelistUsers) OR (userDept in WhitelistDepts)`

#### Scenario: 帳號白名單授權保持不變
- **WHEN** 帳號 "02898" 嘗試編輯已完成維修料槍（Chk=1）
- **THEN** 編輯被允許，行為與擴展前相同

#### Scenario: 部門授權用於新增授權
- **WHEN** 帳號 "01234"（部門 "IT"，不在帳號白名單）嘗試編輯已完成維修料槍（Chk=1）
- **THEN** 編輯被允許（通過部門授權）

#### Scenario: 非授權使用者被拒絕
- **WHEN** 帳號 "99999"（不在帳號白名單，部門 "HR"，不在部門授權清單）嘗試編輯已完成維修料槍
- **THEN** 編輯被拒絕

### Requirement: 前端隱藏按鈕的條件

前端 SHALL 根據擴展的授權邏輯隱藏「料槍維修」按鈕。

**Previous Behavior:**
- Chk=0（未完成）：顯示按鈕
- Chk=1（已完成）：僅當帳號在白名單時顯示按鈕

**Updated Behavior:**
- Chk=0（未完成）：顯示按鈕（邏輯不變）
- Chk=1（已完成）：當帳號在白名單 **或** 部門在授權清單時顯示按鈕

#### Scenario: 未完成維修無條件顯示按鈕
- **WHEN** 維修記錄未完成（Chk=0），無論使用者身份
- **THEN** 「料槍維修」按鈕顯示

#### Scenario: 已完成維修按授權展示按鈕
- **WHEN** 維修記錄已完成（Chk=1）且使用者帳號在白名單或部門在授權清單
- **THEN** 「料槍維修」按鈕顯示

#### Scenario: 已完成維修對非授權使用者隱藏按鈕
- **WHEN** 維修記錄已完成（Chk=1）且使用者既不在帳號白名單也不在部門授權清單
- **THEN** 「料槍維修」按鈕隱藏

### Requirement: 後端驗證邏輯擴展

後端 SHALL 同時檢查帳號和部門，使用 OR 邏輯進行授權決策。

**Previous Behavior:**
- MaterialGunRepair() POST 方法驗證帳號是否在白名單
- IsRepairWhitelisted(userId) 只檢查帳號

**Updated Behavior:**
- IsRepairWhitelisted(userId, userDept) 同時檢查帳號與部門
- 使用 OR 邏輯：`(userId in RepairEditWhitelistUsers) OR (userDept in RepairEditWhitelistDepts)`
- 返回授權決策與授權原因

#### Scenario: 帳號白名單驗證保持一致
- **WHEN** POST 到 `/MaterialGun/MaterialGunRepair` 提交已完成維修更新，帳號 "02898"
- **THEN** 驗證通過，返回成功訊息（行為與擴展前相同）

#### Scenario: 部門授權驗證用於新增場景
- **WHEN** POST 到 `/MaterialGun/MaterialGunRepair` 提交已完成維修更新，帳號 "01234"，部門 "IT"
- **THEN** 驗證通過（通過部門授權），返回成功訊息

#### Scenario: 非授權驗證拒絕
- **WHEN** POST 到 `/MaterialGun/MaterialGunRepair` 提交已完成維修更新，帳號 "99999"，部門 "HR"
- **THEN** 驗證失敗，返回 JSON 錯誤訊息「此料槍已完成維修，無法修改。無權進行此操作。」

### Requirement: 稽審日誌記錄授權來源

稽審日誌 SHALL 明確記錄是通過帳號白名單還是部門授權允許的編輯操作。

**Previous Behavior:**
- 記錄成功時：Information 級別，記錄 UserId 與 Sno
- 記錄失敗時：Warning 級別，記錄 UserId 與 Sno
- 未記錄部門信息或授權原因

**Updated Behavior:**
- 記錄成功時：Information 級別，記錄 UserId、UDeptNo、Sno、及授權原因（"Account whitelist" 或 "IT department"）
- 記錄失敗時：Warning 級別，記錄 UserId、UDeptNo、Sno、及失敗原因
- 授權原因用於事後追蹤與合規檢查

#### Scenario: 帳號白名單授權的日誌
- **WHEN** 帳號 "02898"（部門 "IT"）成功編輯已完成維修料槍 Sno=123
- **THEN** Serilog Information：`User 02898 (Dept: IT) authorized to edit repair 123 - Reason: Account whitelist`

#### Scenario: 部門授權的日誌
- **WHEN** 帳號 "01234"（部門 "IT"）成功編輯已完成維修料槍 Sno=456
- **THEN** Serilog Information：`User 01234 (Dept: IT) authorized to edit repair 456 - Reason: IT department`

#### Scenario: 授權失敗的日誌
- **WHEN** 帳號 "99999"（部門 "HR"）嘗試編輯已完成維修料槍 Sno=789
- **THEN** Serilog Warning：`User 99999 (Dept: HR) denied access to repair record 789`

### Requirement: ViewBag 傳遞簡化布林值

前端接收的 ViewBag.IsWhitelisted SHALL 保持簡單布林值，不需傳遞部門信息。

**Previous Behavior:**
- ViewBag.IsWhitelisted 為布林值，表示使用者是否在帳號白名單中

**Updated Behavior:**
- ViewBag.IsWhitelisted 仍為布林值，表示使用者是否通過帳號或部門授權（OR 邏輯結果）
- 不需傳遞部門代碼到前端

#### Scenario: 前端收到授權結果
- **WHEN** MaterialGunRepairView() 方法執行
- **THEN** ViewBag.IsWhitelisted 被設定為 true/false，基於 IsRepairWhitelisted(userId, userDept) 結果

#### Scenario: JavaScript 使用布林值
- **WHEN** MaterialGunRepairView.cshtml JavaScript 初始化
- **THEN** 從 ViewBag.IsWhitelisted 讀取布林值，無需其他信息
