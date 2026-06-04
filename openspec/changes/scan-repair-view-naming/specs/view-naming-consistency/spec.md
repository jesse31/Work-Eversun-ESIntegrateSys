## ADDED Requirements

### Requirement: MaterialGun View 命名一致性

系統文檔、規格書、代碼實現中的視圖名稱參考應保持一致性。所有 MaterialGun 相關視圖應遵循 `MaterialGun<功能>.cshtml` 的命名慣例，且所有參考位置（代碼層的控制器、View() 呼叫、連結以及文檔層的規格書、API 說明）都應使用相同名稱。

#### Scenario: 掃碼維修視圖改名完整性驗證
- **WHEN** 檢查 MaterialGunScanRepair 功能的所有參考位置
- **THEN** 代碼層中的控制器、View() 呼叫、ActionLink 連結都使用 `MaterialGunScanRepair`
- **THEN** 文檔層（規格書、API 文件）中也使用 `MaterialGunScanRepair`，無 `ScanRepair` 舊名稱殘留

#### Scenario: 文檔中的舊名稱參考清理
- **WHEN** 掃描所有規格書文件（.md 和 .html）
- **THEN** 不存在 `ScanRepair.cshtml` 的舊名稱參考（應為 `MaterialGunScanRepair.cshtml`）
- **THEN** 不存在 `/MaterialGun/ScanRepair` 的舊路由參考（應為 `/MaterialGun/MaterialGunScanRepair`）

#### Scenario: 新增功能開發時遵循命名規範
- **WHEN** 開發新的 MaterialGun 相關功能
- **THEN** 視圖檔案命名為 `MaterialGun<功能>.cshtml` 格式
- **THEN** 控制器 Action、View() 呼叫、連結、文檔中的名稱參考保持一致

### Requirement: 視圖改名檢查清單

實施任何視圖改名或命名統一化時，應遵循完整的檢查清單，確保所有層面的改名都已完成。

#### Scenario: 改名實施前清單驗證
- **WHEN** 計畫改名或發現命名不一致
- **THEN** 應檢查以下位置是否都已更新：
  - 視圖檔案名稱
  - 控制器中的 Action 名稱
  - 控制器中的 View() 呼叫
  - 其他 View 中的 ActionLink 連結
  - 規格書和 API 文檔中的名稱參考
  - 備註和評論中的舊名稱參考

#### Scenario: 完成後驗證
- **WHEN** 所有改名工作完成
- **THEN** 可使用 grep/搜尋工具掃描舊名稱，確認無殘留參考
- **THEN** 應進行代碼審查和文檔審查，確認命名一致
