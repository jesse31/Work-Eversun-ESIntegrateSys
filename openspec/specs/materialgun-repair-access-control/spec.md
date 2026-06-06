# materialgun-repair-access-control Specification

## ADDED Requirements

### Requirement: GET 方法載入時的權限檢查
當使用者訪問料槍維修編輯頁面時，系統 SHALL 檢查該維修記錄是否已完成 (Chk=1)，若已完成則進一步檢查使用者是否有修改權限。

#### Scenario: 已完成維修 - 白名單使用者可進入
- **WHEN** 白名單使用者 (帳號或部門在白名單中) 訪問 `/MaterialGun/MaterialGunRepair?sno=1783`，且該維修記錄已完成 (Chk=1)
- **THEN** 系統允許進入編輯頁面，回傳維修編輯表單

#### Scenario: 已完成維修 - 非白名單使用者被擋下
- **WHEN** 非白名單使用者訪問 `/MaterialGun/MaterialGunRepair?sno=1783`，且該維修記錄已完成 (Chk=1)
- **THEN** 系統擋下存取，設置 TempData["message"] = "此料槍已完成維修，無權修改"，重新導向至 `/MaterialGun/MaterialGunRepairView?page=1`

#### Scenario: 未完成維修 - 任何使用者可進入
- **WHEN** 任何登入使用者訪問 `/MaterialGun/MaterialGunRepair?sno=999`，且該維修記錄未完成 (Chk=0)
- **THEN** 系統允許進入編輯頁面，不檢查白名單

### Requirement: GET 方法載入時的報廢檢查
在權限檢查通過後，系統 SHALL 檢查料槍是否已報廢。若已報廢，系統 SHALL 擋下存取。

#### Scenario: 報廢料槍被擋下
- **WHEN** 使用者訪問 `/MaterialGun/MaterialGunRepair?sno=888`，該料槍已報廢 (MaterialGunDiscard=true)
- **THEN** 系統擋下存取，設置 TempData["message"] = "此料槍已報廢，無法編輯"，重新導向至 `/MaterialGun/MaterialGunRepairView?page=1`

#### Scenario: 未報廢料槍正常進入
- **WHEN** 使用者訪問 `/MaterialGun/MaterialGunRepair?sno=777`，該料槍未報廢 (MaterialGunDiscard=false)
- **THEN** 系統允許進入編輯頁面

### Requirement: 檢查順序與優先級
系統 SHALL 按以下順序進行檢查：1) 權限檢查（若已完成維修），2) 報廢檢查。若其中任一檢查失敗，系統 SHALL 擋下並返回首頁，不繼續後續檢查。

#### Scenario: 已完成且報廢 - 提示無權限優先
- **WHEN** 使用者訪問已完成維修 (Chk=1) 且已報廢 (Discard=true) 的料槍，且使用者無白名單權限
- **THEN** 系統返回提示「此料槍已完成維修，無權修改」，不顯示報廢訊息

#### Scenario: 已完成且報廢 - 白名單使用者看到報廢提示
- **WHEN** 白名單使用者訪問已完成維修 (Chk=1) 且已報廢 (Discard=true) 的料槍
- **THEN** 系統返回提示「此料槍已報廢，無法編輯」

### Requirement: 存取日誌記錄
系統 SHALL 為所有存取嘗試（成功或失敗）記錄日誌，包含使用者帳號、部門、料槍編號、存取結果。

#### Scenario: 允許進入時記錄
- **WHEN** 使用者成功進入維修編輯頁面
- **THEN** 系統記錄 Information 等級日誌：`User {UserId} (Dept: {UDeptNo}) accessed repair record {Sno}`

#### Scenario: 無權限擋下時記錄
- **WHEN** 非白名單使用者嘗試訪問已完成維修的頁面
- **THEN** 系統記錄 Warning 等級日誌：`User {UserId} denied access to repair record {Sno}: no whitelist privilege`

#### Scenario: 報廢記錄擋下時記錄
- **WHEN** 使用者嘗試訪問已報廢料槍的編輯頁面
- **THEN** 系統記錄 Warning 等級日誌：`Access denied to repair record {Sno}: record is discarded`

### Requirement: 返回目標的正確性
系統 SHALL 將所有被擋的存取導向到料槍維修列表的第一頁 (`MaterialGunRepairView` with `page=1`)，確保使用者可見完整的可修改項目列表。

#### Scenario: 返回到維修列表首頁
- **WHEN** 使用者被擋下（無權限或報廢）
- **THEN** 系統執行 `RedirectToAction("MaterialGunRepairView", new { page = 1 })`

#### Scenario: TempData 訊息在返回頁面顯示
- **WHEN** 使用者被導向維修列表首頁
- **THEN** 頁面讀取 `TempData["message"]`，並根據現有 View 邏輯顯示提示訊息（如 toast、alert 或 banner）

### Requirement: 記錄查詢不存在的處理
當維修單號不存在時，系統 SHALL 回傳友善提示並返回首頁，而非拋出例外。

#### Scenario: 維修單號不存在
- **WHEN** 使用者訪問 `/MaterialGun/MaterialGunRepair?sno=99999`，該單號在資料庫不存在
- **THEN** 系統設置 TempData["message"] = "維修記錄不存在"，重新導向至 `/MaterialGun/MaterialGunRepairView?page=1`
