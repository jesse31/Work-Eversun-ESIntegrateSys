# Spec: 人員編輯健壯性

## Purpose

確保人員編輯功能（Edit GET / POST）在關聯資料缺漏、角色記錄不存在、或表單未勾選任何功能的邊界情況下，仍能穩健運作而不產生 500 錯誤或未預期崩潰。

---

## Requirements

### Requirement: 人員編輯查詢在關聯資料缺漏時仍返回基本資料

GET 人員編輯查詢（`Edit(int fId)`）SHALL 使用 LEFT JOIN 連接 `ES_MemberRole`、`ES_RoleClassification`、`ES_MemberFunction`、`ES_FunctionItem`，確保即使上述任一表缺少對應記錄，仍能返回包含 `fUserId`、`fName` 的非空 List。

#### Scenario: 人員無 Role 記錄時仍可載入編輯頁
- **WHEN** 以 `fId` 查詢的人員在 `ES_MemberRole` 無對應記錄
- **THEN** 系統 SHALL 返回包含 1 筆資料的 List，其中 `fUserId`、`fName` 有值，`ROLE_ID`、`ROLE_DESC`、`Func` 為 null

#### Scenario: 人員無 Function 記錄時仍可載入編輯頁
- **WHEN** 以 `fId` 查詢的人員在 `ES_MemberFunction` 無對應記錄
- **THEN** 系統 SHALL 返回包含 1 筆資料的 List，其中 `fUserId`、`fName` 有值，`Func` 為 null

#### Scenario: 人員有完整關聯資料時行為不變
- **WHEN** 人員在所有關聯表均有記錄
- **THEN** 系統 SHALL 返回與修改前相同的多筆資料（每筆 Function 對應一列），行為不受影響

---

### Requirement: POST 儲存前驗證人員存在

`Edit(string fUserId, ...)` SHALL 在更新名稱前驗證 `fUserId` 能對應到 `ES_Member` 中的記錄，若找不到則 MUST 拋出包含 `fUserId` 的明確例外訊息，不得繼續執行後續邏輯。

#### Scenario: fUserId 為空字串或不存在時拋出明確例外
- **WHEN** POST 收到的 `fUserId` 無法在 `ES_Member` 找到對應記錄
- **THEN** 系統 SHALL 拋出 `Exception`，訊息包含傳入的 `fUserId` 值，且不執行任何資料庫寫入

---

### Requirement: POST 角色記錄採 upsert 模式

`Edit(string fUserId, ...)` SHALL 在更新 `ES_MemberRole` 時採用 upsert 邏輯：若記錄存在則更新 `ROLE_ID` 與 `EXPIRED_DATE`；若不存在則新增一筆包含 `USER_ID`、`ROLE_ID`、`EXPIRED_DATE` 的記錄。

#### Scenario: 角色記錄已存在時更新
- **WHEN** `ES_MemberRole` 中已有該 `USER_ID` 的記錄
- **THEN** 系統 SHALL 更新 `ROLE_ID` 與 `EXPIRED_DATE`，不新增記錄

#### Scenario: 角色記錄不存在時新增
- **WHEN** `ES_MemberRole` 中無該 `USER_ID` 的記錄
- **THEN** 系統 SHALL 新增一筆記錄，包含 `USER_ID`、`ROLE_ID`、`EXPIRED_DATE = DateTime.Now`

---

### Requirement: POST 在無勾選功能時不崩潰

`Edit(string fUserId, ...)` 的 `fItem` 參數為 `null` 時，系統 SHALL 將其視為空集合處理，清除該人員的所有功能記錄後不新增任何 `ES_MemberFunction` 記錄，不得拋出例外。

#### Scenario: 未勾選任何 checkbox 提交
- **WHEN** POST 的 `fItem` 為 `null`（表單無任何勾選的 checkbox）
- **THEN** 系統 SHALL 刪除該人員現有所有 `ES_MemberFunction` 記錄，且不新增任何記錄，儲存成功後正常導向
