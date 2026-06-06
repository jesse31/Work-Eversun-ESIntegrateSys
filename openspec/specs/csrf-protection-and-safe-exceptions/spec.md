## 新增需求

### 要求：對會修改資料的 POST 動作驗證 Anti-CSRF Token

系統應對所有會修改資料的 POST 動作驗證 Anti-CSRF token。所有會改變伺服器狀態的 POST 方法應標記 `[ValidateAntiForgeryToken]` 屬性，且對應的表單應包含 `@Html.AntiForgeryToken()`。

#### 情境：`CreateMaintainWork` 驗證 CSRF Token
- **WHEN** `CreateMaintainWork` (POST) 收到沒有有效 CSRF token 的請求
- **THEN** 系統拒絕該請求並回傳 HTTP 403 Forbidden

#### 情境：`MaterialGunForRepair` 驗證 CSRF Token
- **WHEN** `MaterialGunForRepair` (POST) 收到沒有有效 CSRF token 的請求
- **THEN** 系統拒絕該請求並回傳 HTTP 403 Forbidden

#### 情境：`Discard` 驗證 CSRF Token
- **WHEN** `Discard` (POST) 收到沒有有效 CSRF token 的請求
- **THEN** 系統拒絕該請求並回傳 HTTP 403 Forbidden

#### 情境：`MaterialGunRepair` 驗證 CSRF Token
- **WHEN** `MaterialGunRepair` (POST) 收到沒有有效 CSRF token 的請求
- **THEN** 系統拒絕該請求並回傳 HTTP 403 Forbidden

#### 情境：`MaterialGunCreate` 驗證 CSRF Token
- **WHEN** `MaterialGunCreate` (POST) 收到沒有有效 CSRF token 的請求
- **THEN** 系統拒絕該請求並回傳 HTTP 403 Forbidden

### 要求：安全的例外訊息

系統應隱藏使用者介面上顯示的技術例外細節。所有在 API 端點與 Action 方法中被捕捉的例外應完整記錄，但回傳給用戶的訊息僅為通用訊息。

#### 情境：`GetGunByBarcode` 隱藏資料庫錯誤
- **WHEN** `GetGunByBarcode` 捕捉到 `SqlException` 或 `EntityException`
- **THEN** 系統完整記錄該例外，但回傳 `{ success: false, message: "系統處理發生錯誤，請聯繫管理員" }` 給用戶端

#### 情境：API 端點隱藏堆疊追蹤
- **WHEN** 任一 API 端點（如 `CheckMaintainStatus`、`CheckData`、`BadDesc` 等）發生錯誤
- **THEN** 系統使用 Serilog 記錄完整堆疊，但回傳通用錯誤訊息以避免資訊外洩

#### 情境：使用者看不到資料庫結構資訊
- **WHEN** 發生與資料庫結構相關的錯誤
- **THEN** 系統不會回傳揭露資料表名稱、欄位名稱或連線字串的訊息

#### 情境：使用者看不到檔案系統路徑
- **WHEN** 發生檔案操作錯誤
- **THEN** 系統不會回傳會揭露本機檔案路徑或系統結構的錯誤訊息

### 要求：Null 安全檢查

系統應在存取 `Session`、表單資料或資料庫查詢結果前進行 null 檢查。所有成員存取應以空合併運算子或明確的 null 防護來保護。

#### 情境：使用前檢查 `Session["Member"]`
- **WHEN** 任一 Action 方法存取 `Session["Member"]`
- **THEN** 系統使用空合併 (`??`) 或明確的 null 檢查以避免 `NullReferenceException`

#### 情境：驗證查詢結果
- **WHEN** `SingleOrDefault`、`FirstOrDefault` 或 `Find` 回傳 null
- **THEN** 系統在存取屬性前檢查 null 並妥善處理

#### 情境：表單資料的 null 驗證
- **WHEN** POST Action 收到 null 或空的表單參數
- **THEN** 系統驗證參數再處理，並回傳適當的錯誤訊息

### 要求：敏感操作的稽核日誌

系統應記錄對已完成（`Chk=true`）維修記錄的所有修改，包含使用者授權理由、時間戳與詳細變更資訊。

#### 情境：記錄授權理由
- **WHEN** 已授權的使用者更新已完成的維修記錄
- **THEN** 系統記錄授權理由（例如帳號白名單、部門授權）以供稽核追蹤

#### 情境：未授權存取嘗試被記錄
- **WHEN** 未授權的使用者嘗試更新已完成的維修記錄
- **THEN** 系統記錄警告，包含 `userId`、部門、料槍序號及拒絕原因

#### 情境：記錄變更細節
- **WHEN** 維修記錄任何欄位變更發生
- **THEN** 系統以 JSON 格式記錄 `Classification` 與 `MaintenanceResult` 的舊值與新值
