## ADDED Requirements

### Requirement: Toggle IE reporting status

使用者可以透過勾選「報價中」checkbox 切換 IE 報價狀態。系統應立即反映 UI 變化，無需整個頁面重新整理。

#### Scenario: User checks reporting status

- **WHEN** 使用者勾選「報價中」checkbox
- **THEN** 系統立即在該行 IE 負責人欄位顯示當前使用者名稱
- **AND** checkbox 被禁用（防止二次操作）
- **AND** 顯示「紀錄已設置為報價中」的友善提示
- **AND** 頁面不刷新，其他資訊保持不變

#### Scenario: User unchecks reporting status

- **WHEN** 使用者取消勾選「報價中」checkbox
- **THEN** 系統立即清空該行 IE 負責人欄位
- **AND** checkbox 被啟用（恢復可點擊狀態）
- **AND** 顯示「已取消」的友善提示
- **AND** 頁面不刷新

### Requirement: Permission validation on toggle cancellation

只有原始勾選者才能取消「報價中」狀態，防止其他使用者誤操作。

#### Scenario: Authorized user cancels status

- **WHEN** 原始勾選者取消勾選
- **THEN** 系統驗證當前使用者 ID 與原勾選者相符
- **AND** 後端刪除該筆 ES_QuoteForIE 記錄
- **AND** 前端顯示「已取消」提示

#### Scenario: Unauthorized user attempts to cancel status

- **WHEN** 非原始勾選者嘗試取消勾選
- **THEN** 系統驗證失敗，回傳錯誤訊息「只有原勾選者才能取消」
- **AND** 前端自動還原 checkbox 為勾選狀態
- **AND** 顯示權限不符的友善提示

### Requirement: Resilient state handling on operation failure

操作失敗時自動還原 checkbox 狀態，確保 UI 與後端資料一致。

#### Scenario: AJAX request fails

- **WHEN** AJAX 請求遇到網路錯誤或伺服器錯誤
- **THEN** 系統自動還原 checkbox 到操作前的狀態
- **AND** 顯示「無法更新紀錄」的錯誤提示
- **AND** 使用者可以重試操作

#### Scenario: Backend validation fails

- **WHEN** 後端驗證失敗（如權限不符）
- **THEN** 系統回傳錯誤 JSON（包含 status 和 message）
- **AND** 前端自動還原 checkbox 狀態
- **AND** 顯示對應的錯誤訊息

### Requirement: User-friendly feedback mechanism

系統應使用 SweetAlert2 提供友善的操作反饋，替代傳統 `alert()` 對話框。

#### Scenario: Success notification

- **WHEN** 勾選或取消操作成功
- **THEN** 系統顯示 SweetAlert2 提示窗（icon: success/info、可自動關閉）
- **AND** 提示窗在 1.5 秒後自動消失

#### Scenario: Error notification

- **WHEN** 操作失敗或驗證不通過
- **THEN** 系統顯示 SweetAlert2 錯誤提示窗（icon: error）
- **AND** 提示窗保持顯示直到使用者確認

### Requirement: Data consistency on response

後端應回傳足夠的資訊以支援前端 DOM 更新，確保顯示的資料與資料庫一致。

#### Scenario: Backend response includes updated values

- **WHEN** AJAX 操作成功
- **THEN** 後端回傳 JSON 包含 `{ status: "success", ieonwer: "...", iestatus: "U"|"" }`
- **AND** 前端使用 ieonwer 值更新 IE 負責人欄位
- **AND** 前端使用 iestatus 值判斷 checkbox 的禁用狀態
