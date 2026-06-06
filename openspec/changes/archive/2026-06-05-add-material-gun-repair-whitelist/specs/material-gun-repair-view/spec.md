# 料槍維修列表檢視 - Specs

## ADDED Requirements

### Requirement: 料槍維修列表顯示邏輯
系統在 `/MaterialGun/MaterialGunRepairView` 頁面顯示料槍維修記錄清單時，每一行 SHALL 根據維修狀態 (`Chk` 欄位) 和使用者權限決定是否顯示「料槍維修」按鈕。

**原有行為：** 當 `Chk = 1`（已完成維修）時，所有使用者都隱藏「料槍維修」按鈕。僅當 `Chk = 0` 且角色非 V、FR 時，才顯示按鈕。

**新增行為：** 當 `Chk = 1`（已完成維修）時，檢查使用者是否在白名單中：
- 白名單人員 → 顯示「料槍維修」按鈕
- 非白名單人員 → 隱藏「料槍維修」按鈕

當 `Chk = 0`（未維修）時，行為不變（僅檢查角色）。

#### Scenario: 未維修料槍 - 非 V/FR 角色使用者
- **WHEN** 料槍為未維修 (Chk=0)，使用者角色非 V 且非 FR
- **THEN** 顯示該行的「料槍維修」按鈕，背景顏色為紅色 (forrepair)

#### Scenario: 未維修料槍 - V/FR 角色使用者
- **WHEN** 料槍為未維修 (Chk=0)，使用者角色為 V 或 FR
- **THEN** 隱藏該行的「料槍維修」按鈕，背景顏色為紅色 (forrepair)

#### Scenario: 已完成維修料槍 - 白名單人員
- **WHEN** 料槍為已完成維修 (Chk=1)，使用者帳號在白名單中 (如 02898)
- **THEN** 顯示該行的「料槍維修」按鈕，背景顏色為綠色 (repaired)

#### Scenario: 已完成維修料槍 - 非白名單人員
- **WHEN** 料槍為已完成維修 (Chk=1)，使用者帳號不在白名單中
- **THEN** 隱藏該行的「料槍維修」按鈕，背景顏色為綠色 (repaired)

### Requirement: 列表行 data-chk 屬性
每一行記錄 SHALL 包含 `data-chk` 屬性，用於前端 JavaScript 防呆檢查，其值應與資料庫的 `Chk` 欄位保持同步。

#### Scenario: 未維修料槍行的 data-chk 屬性
- **WHEN** 系統渲染未維修 (Chk=0) 的料槍行
- **THEN** 該行的 `<tr>` 元素包含 `data-chk="false"` 或 `data-chk="0"`

#### Scenario: 已完成維修料槍行的 data-chk 屬性
- **WHEN** 系統渲染已完成維修 (Chk=1) 的料槍行
- **THEN** 該行的 `<tr>` 元素包含 `data-chk="true"` 或 `data-chk="1"`

### Requirement: 前端點擊事件防呆
用戶在列表頁點擊「料槍維修」按鈕時，JavaScript 事件處理程式 SHALL 檢查料槍的維修狀態和使用者的白名單身分，拒絕不授權的操作。

#### Scenario: 使用者點擊未維修料槍的「料槍維修」按鈕
- **WHEN** 使用者點擊未維修 (data-chk="false") 料槍的「料槍維修」連結
- **THEN** JavaScript 允許預設行為，進行頁面導向至 `/MaterialGun/MaterialGunRepair?sno=xxx`

#### Scenario: 使用者點擊已完成維修料槍的「料槍維修」按鈕（已被隱藏）
- **WHEN** 由於前端隱藏邏輯，該按鈕在 DOM 中不存在
- **THEN** 使用者無法點擊，防呆失效無

#### Scenario: 使用者透過開發者工具強行修改 HTML 顯示被隱藏的按鈕並點擊
- **WHEN** 非白名單人員修改 CSS/HTML，使隱藏的「料槍維修」按鈕重新出現，並點擊該按鈕
- **THEN** JavaScript 檢查 data-chk="true"，顯示 SweetAlert2 警告：「此料槍已完成維修，無法修改。無權進行此操作。」，阻止頁面導向
