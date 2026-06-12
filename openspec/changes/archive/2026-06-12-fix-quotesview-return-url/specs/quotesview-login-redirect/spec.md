## ADDED Requirements

### Requirement: QuotesView 登入後 ReturnUrl 保留完整查詢參數

未登入使用者存取 `QuotesView` 時，系統 SHALL 將所有原始查詢參數（包含 `EngSr` 與 `CustMaterial`）完整保存於 `returnUrl`，使登入後可還原至原始篩選條件頁面。

#### Scenario: 從含 EngSr 與 CustMaterial 的 email 連結進入

- **WHEN** 未登入使用者點擊含 `EngSr`、`CustMaterial`、`Indate`、`SalesId` 參數的 email 連結
- **THEN** 系統 MUST 將這四個參數全數帶入 `returnUrl` 並導向登入頁

#### Scenario: 登入後回到含完整參數的頁面

- **WHEN** 使用者完成登入，且 `returnUrl` 包含 `EngSr` 與 `CustMaterial`
- **THEN** 系統 MUST 導向含完整查詢參數的 `QuotesView`，顯示對應筆數的報價資料

#### Scenario: 無 EngSr 與 CustMaterial 的一般進入

- **WHEN** 未登入使用者從一般導覽進入 `QuotesView`（不含 `EngSr`、`CustMaterial`）
- **THEN** 系統 MUST 正常導向登入頁，登入後回到不帶這兩個參數的 `QuotesView`（行為與修改前相同）
