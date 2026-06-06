## 新增需求

### 要求：統一例外處理並使用 Serilog 結構化記錄

系統應在關鍵業務操作中捕捉所有例外，並使用 Serilog 以結構化欄位（例如 userId、部門、料槍序號、操作類型）記錄詳細資訊。對使用者回傳的訊息不得暴露技術細節。

#### 情境：SaveChanges 拋出例外並被記錄
- **WHEN** `db.SaveChanges()` 拋出例外（例如違反約束、連線失敗）
- **THEN** 系統捕捉該例外，使用 Serilog 記錄包含上下文欄位的詳細資訊，並向使用者回傳通用錯誤訊息

#### 情境：API 呼叫或 LINQ 查詢拋出例外並被記錄
- **WHEN** 外部 API 呼叫或 LINQ 查詢拋出例外
- **THEN** 系統捕捉該例外，使用結構化上下文記錄，並向使用者回傳通用錯誤訊息

#### 情境：防止 NullReferenceException
- **WHEN** 存取 `Session["Member"]` 或其他可能為 null 的值
- **THEN** 系統先進行 null 檢查並在嘗試存取前記錄相關情況

#### 情境：例外記錄包含操作元資料
- **WHEN** 任一關鍵操作失敗（例如 `CreateMaintainWork`、`MaterialGunForRepair`、`MaterialGunRepair`）
- **THEN** 記錄的例外包含 `UserId`、`UDeptNo`、`MaterialGunSno`、操作名稱與時間戳，使用 Serilog ForContext 加入欄位

### 要求：GetGunByBarcode 的例外安全性

系統應在 `GetGunByBarcode` 中捕捉例外，且回傳不暴露內部系統細節的通用錯誤格式。

#### 情境：資料庫查詢錯誤被遮蔽
- **WHEN** `GetGunByBarcode` 發生資料庫連線或查詢錯誤
- **THEN** 系統捕捉例外，使用 Serilog 記錄，並回傳 `{ success: false, message: "系統處理發生錯誤，請聯繫管理員" }`

#### 情境：無效條碼處理
- **WHEN** 使用者提供會導致解析或驗證錯誤的無效條碼
- **THEN** 系統捕捉例外，記錄錯誤，並向使用者回傳不含技術細節的友善訊息

#### 情境：空資料處理
- **WHEN** 查詢結果為 null 或回傳意外資料結構
- **THEN** 系統安全處理 null 值並記錄異常，而不會拋出未處理例外

### 要求：Session 存取安全

系統應安全地存取 `Session` 物件以避免 `NullReferenceException`。

#### 情境：在 `CreateMaintainWork` 安全存取 `Session["Member"]`
- **WHEN** 執行 `CreateMaintainWork` 方法
- **THEN** 系統檢查 `Session["Member"]` 是否存在，若為 null 則導向登入頁面

#### 情境：在 `MaterialGunRepair` 安全存取 Session
- **WHEN** `MaterialGunRepair` (POST) 方法存取 Session 的會員資料
- **THEN** 系統驗證 `Session["Member"]` 非 null 再擷取 `userId` 與 `department`

#### 情境：Session 資料回退策略
- **WHEN** `Session["Member"]` 為 null 或損壞
- **THEN** 系統記錄警告並導向登入頁，而非發生崩潰

### 要求：關鍵 POST Action 的全面例外處理

系統應在所有關鍵 POST Action（`CreateMaintainWork`、`MaterialGunForRepair`、`MaterialGunRepair`、`Discard`、`MaterialGunCreate`）中使用 try-catch 包覆。

#### 情境：`CreateMaintainWork` 的例外被捕捉
- **WHEN** 在 `CreateMaintainWork` 處理期間發生任何錯誤
- **THEN** 系統捕捉例外，使用上下文記錄，並回傳適當的回應（導向或錯誤訊息）

#### 情境：`MaterialGunRepair` 的例外被捕捉
- **WHEN** 在提交維修資料時發生任何錯誤
- **THEN** 系統捕捉例外、記錄，並以通用訊息回傳 JSON 錯誤回應

#### 情境：`Discard` 操作的例外被捕捉
- **WHEN** 在 `Discard` 或 `ManagerCheck` 發生資料庫錯誤
- **THEN** 系統捕捉例外、記錄，並導向資訊頁面顯示通用錯誤訊息
