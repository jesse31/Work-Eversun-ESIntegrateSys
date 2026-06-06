# department-authorization-for-repair Specification

## Purpose
TBD - created by archiving change add-department-authorization-to-repair. Update Purpose after archive.
## Requirements
### Requirement: IT 部門成員可編輯已完成維修記錄

IT 部門的所有員工（部門代碼 "IT"）SHALL 能夠編輯已完成的維修記錄（Chk=1），用於複查或更新維修信息。這是對現有帳號白名單的補充，使部門級授權成為可能。

#### Scenario: IT 部門成員成功編輯已完成料槍
- **WHEN** 使用者的部門代碼為 "IT" 且訪問 `/MaterialGun/MaterialGunRepair?sno=123` （其中料槍的 Chk=1）
- **THEN** 頁面加載成功，使用者可以編輯維修記錄

#### Scenario: IT 部門成員提交已完成料槍的編輯表單
- **WHEN** 部門代碼為 "IT" 的使用者提交已完成維修料槍的編輯表單
- **THEN** 後端驗證通過，記錄異動並返回成功訊息

#### Scenario: 非授權部門成員無法編輯
- **WHEN** 使用者的部門代碼非 "IT"（如 "HR"、"Finance"）且維修記錄已完成（Chk=1）
- **THEN** 前端隱藏「料槍維修」按鈕，即使直接訪問 URL 後端亦拒絕提交

### Requirement: 稽審日誌記錄部門授權原因

系統 SHALL 在 Serilog 日誌中清楚記錄使用者的部門代碼與授權原因（帳號白名單 vs 部門授權）。

#### Scenario: 記錄帳號白名單授權
- **WHEN** 帳號 "02898" 編輯已完成維修料槍
- **THEN** Serilog 記錄：`User 02898 (Dept: IT) authorized to edit repair {Sno} - Reason: Account whitelist`

#### Scenario: 記錄部門授權
- **WHEN** 帳號 "01234"（部門代碼 "IT"，不在帳號白名單中）編輯已完成維修料槍
- **THEN** Serilog 記錄：`User 01234 (Dept: IT) authorized to edit repair {Sno} - Reason: IT department`

#### Scenario: 記錄授權失敗
- **WHEN** 非授權部門的使用者嘗試編輯已完成維修料槍
- **THEN** Serilog 在 Warning 級別記錄：`User {UserId} (Dept: {UDeptNo}) denied access to repair record {Sno}`

### Requirement: 部門授權與帳號白名單共存

系統SHALL 同時支援帳號級和部門級授權，使用 OR 邏輯組合：帳號在白名單或部門在授權清單中，即允許編輯已完成維修。

#### Scenario: 帳號白名單人員保持編輯權
- **WHEN** 帳號 "02898"（在帳號白名單中）嘗試編輯已完成維修料槍，無論其部門是什麼
- **THEN** 編輯被允許，Serilog 記錄授權原因為 "Account whitelist"

#### Scenario: 部門授權人員獲得編輯權
- **WHEN** 部門代碼為 "IT"（在部門授權清單中）的任何使用者嘗試編輯已完成維修料槍
- **THEN** 編輯被允許，Serilog 記錄授權原因為 "IT department"

#### Scenario: 非授權使用者被拒絕
- **WHEN** 使用者既不在帳號白名單中，部門也不在授權清單中，嘗試編輯已完成維修料槍
- **THEN** 編輯被拒絕，返回錯誤訊息「此料槍已完成維修，無法修改。無權進行此操作。」

### Requirement: 後端驗證強化

每次編輯已完成維修記錄時，後端 SHALL 同時驗證帳號和部門代碼，確保授權決策完整。

#### Scenario: 後端驗證帳號白名單
- **WHEN** POST 到 `/MaterialGun/MaterialGunRepair` 進行提交
- **THEN** 後端首先檢查使用者帳號是否在 RepairEditWhitelistUsers 中

#### Scenario: 後端驗證部門授權
- **WHEN** POST 到 `/MaterialGun/MaterialGunRepair` 進行提交
- **THEN** 後端檢查使用者部門代碼是否在 RepairEditWhitelistDepts 中

#### Scenario: 授權後才允許資料變更
- **WHEN** 使用者通過帳號或部門授權驗證，POST 請求包含維修資料修改
- **THEN** 後端執行維修資料更新，不阻攔任何欄位修改

### Requirement: 前端無需改動部門傳遞

前端 View 和 JavaScript 無需改動以傳遞部門信息，後端 SHALL 獨自負責授權決策。

#### Scenario: 前端仍使用簡單布林值
- **WHEN** MaterialGunRepairView 頁面渲染
- **THEN** ViewBag.IsWhitelisted 仍為簡單 true/false 布林值，無需包含部門信息

#### Scenario: JavaScript 防呆邏輯無需改動
- **WHEN** 使用者點擊「料槍維修」按鈕
- **THEN** JavaScript 檢查 ViewBag.IsWhitelisted，不需要檢查或傳遞部門代碼

### Requirement: 向後相容

新增部門授權 SHALL 不影響現有帳號白名單的功能。

#### Scenario: 帳號 02898 保持原有編輯權
- **WHEN** 帳號 "02898" 編輯已完成維修料槍
- **THEN** 功能行為與部門授權新增前相同，編輯被允許

#### Scenario: 未完成維修記錄無須授權檢查
- **WHEN** 編輯未完成維修記錄（Chk=0）
- **THEN** 無須進行白名單或部門授權檢查，邏輯保持不變

