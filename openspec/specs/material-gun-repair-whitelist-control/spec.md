# material-gun-repair-whitelist-control Specification

## Purpose
TBD - created by archiving change add-material-gun-repair-whitelist. Update Purpose after archive.
## Requirements
### Requirement: 後端白名單驗證方法
系統 SHALL 提供 `IsRepairWhitelisted(userId)` 方法，用來檢查指定的使用者帳號是否在維修白名單中。

#### Scenario: 白名單人員驗證通過
- **WHEN** 呼叫 `IsRepairWhitelisted("02898")`
- **THEN** 方法返回 `true`

#### Scenario: 非白名單人員驗證失敗
- **WHEN** 呼叫 `IsRepairWhitelisted("99999")`
- **THEN** 方法返回 `false`

### Requirement: GET 維修檢視頁面時的白名單檢查
當使用者訪問 `/MaterialGun/MaterialGunRepairView` 時，系統 SHALL 檢查登入使用者是否在白名單中，並將結果存放在 `ViewBag.IsWhitelisted`。

#### Scenario: 白名單人員進入維修列表
- **WHEN** 白名單帳號 (02898) 進入 `/MaterialGun/MaterialGunRepairView`
- **THEN** 頁面加載，`ViewBag.IsWhitelisted = true`

#### Scenario: 非白名單人員進入維修列表
- **WHEN** 非白名單帳號進入 `/MaterialGun/MaterialGunRepairView`
- **THEN** 頁面加載，`ViewBag.IsWhitelisted = false`

### Requirement: POST 送出維修編輯時的白名單驗證（防呆）
當使用者提交維修編輯表單（POST to `MaterialGunRepair`）時，若該料槍的 `Chk = 1`（已完成維修），系統 SHALL 驗證使用者是否在白名單中。

#### Scenario: 白名單人員提交已完成維修料槍的編輯
- **WHEN** 白名單帳號提交編輯已完成維修 (Chk=1) 的料槍
- **THEN** 系統接受提交，更新維修紀錄

#### Scenario: 非白名單人員提交已完成維修料槍的編輯
- **WHEN** 非白名單帳號提交編輯已完成維修 (Chk=1) 的料槍
- **THEN** 系統拒絕提交，返回 `JsonResult { success = false, msg = "此料槍已完成維修，無法修改。無權進行此操作。" }`

#### Scenario: 任何人員提交未維修料槍的編輯（不檢查白名單）
- **WHEN** 任何人員提交編輯未維修 (Chk=0) 的料槍
- **THEN** 系統接受提交，不檢查白名單，更新維修紀錄

### Requirement: 前端按鈕顯示邏輯
在 `MaterialGunRepairView.cshtml` 中，「料槍維修」按鈕的顯示邏輯 SHALL 根據 `Chk` 值與白名單狀態動態控制。

#### Scenario: 未維修料槍按鈕顯示（非 V、FR 角色）
- **WHEN** 使用者角色非 V、FR，且料槍為未維修 (Chk=0)
- **THEN** 顯示「料槍維修」按鈕

#### Scenario: 已完成維修料槍 - 白名單人員按鈕顯示
- **WHEN** 料槍為已完成維修 (Chk=1)，且使用者在白名單中
- **THEN** 顯示「料槍維修」按鈕

#### Scenario: 已完成維修料槍 - 非白名單人員按鈕隱藏
- **WHEN** 料槍為已完成維修 (Chk=1)，且使用者不在白名單中
- **THEN** 隱藏「料槍維修」按鈕

### Requirement: 前端防呆機制
在 JavaScript 中，當使用者點擊「料槍維修」連結時，系統 SHALL 檢查 `data-chk` 屬性，防止非白名單人員編輯已完成維修的料槍。

#### Scenario: 點擊已完成維修料槍的編輯連結（非白名單）
- **WHEN** 非白名單人員點擊已完成維修 (data-chk="true") 料槍的「料槍維修」連結
- **THEN** JavaScript 攔截點擊，顯示 SweetAlert2 提示：「此料槍已完成維修，無法修改。無權進行此操作。」，不進行頁面導向

#### Scenario: 點擊未維修料槍的編輯連結
- **WHEN** 使用者點擊未維修 (data-chk="false") 料槍的「料槍維修」連結
- **THEN** JavaScript 允許導向，進入 `/MaterialGun/MaterialGunRepair?sno=xxx` 編輯頁面

