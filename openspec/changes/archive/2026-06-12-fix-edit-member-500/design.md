## Context

`MemberMaintain.Edit(int fId)` 是人員編輯頁的 GET 查詢，現行以四個 INNER JOIN 串接 `ES_MemberRole`、`ES_RoleClassification`、`ES_MemberFunction`、`ES_FunctionItem`。任一關聯表缺少記錄時整個查詢回傳空 List，導致 View 以空字串渲染 `fUserId`/`fName`，後續 POST 觸發 `NullReferenceException`（HTTP 500）。

`MemberMaintain.Edit(string fUserId, ...)` 是 POST 處理方法，對 `member`、`member_Role` 無 null 防護，且 `fItem` 在無勾選時為 `null`，三者均有崩潰風險。

框架：ASP.NET MVC + Entity Framework 6，沿用既有 Repository-in-Model 模式，不引入新依賴。

## Goals / Non-Goals

**Goals:**
- 人員無 Role 或 Function 記錄時，GET 仍能正確帶出 `fUserId`、`fName`
- POST 對 `member` null 提供明確錯誤；`member_Role` 不存在時自動新增（upsert）
- `fItem` 為空時不崩潰

**Non-Goals:**
- 不重構 Controller、View 或資料庫結構
- 不改變已有完整資料人員的現行行為
- 不加入前端驗證或 UX 提示

## Decisions

### 決策 1：GET 查詢改 LEFT JOIN（而非拆成多次查詢）

選擇將四個 INNER JOIN 改為 LEFT JOIN（`into … DefaultIfEmpty()`）。

**替代方案**：先查 `ES_Member` 取得基本資料，再個別查 Role/Function。

**選擇理由**：LEFT JOIN 在同一次資料庫往返完成，程式碼改動範圍最小（只改 join 語法與最後一個 `Func` 欄位的 null 判斷），不改變回傳型別與呼叫端。拆成多次查詢需要重組資料結構，修改範圍過大。

### 決策 2：`member_Role` 採 upsert，不拋例外

`member_Role` 為 null 時建立新記錄並 `Add` 到 context，而非拋出例外。

**替代方案**：拋出 `Exception("找不到角色記錄")`。

**選擇理由**：編輯頁的目的就是讓管理者設定角色；若角色記錄不存在，本次操作本身就是要建立它。拋例外反而阻止了合理的業務操作。

### 決策 3：`member_Role.USER_ID = fUserId` 移至 upsert 初始化

原有的 `member_Role.USER_ID = fUserId` 賦值（更新時冗餘）移入 upsert 的 `new ES_MemberRole { USER_ID = fUserId }` 初始化，更新路徑不再重複設值。

### 決策 4：`fItem` 以 `?? new List<string>()` 防護

HTTP form 沒有勾選任何 checkbox 時，MVC model binding 傳入 `null`。加入 null coalescing 替換為空集合，既不拋例外也不影響「清除所有功能」的語義。

## Risks / Trade-offs

- **LEFT JOIN 串鏈**：`b`（ES_MemberRole）為 null 時，`b.ROLE_ID` 在 EF6 SQL 轉譯中映射為 `NULL`，導致下一層 JOIN 同樣取不到結果（c 為 null）。此行為符合預期，EF6 可正確轉譯，但需確認生成 SQL 無誤。→ 驗證方式：以無 Role 記錄的人員執行 GET，確認頁面顯示正確基本資料。
- **LEFT JOIN 回傳 1 筆 null Func 列**：無 Function 記錄時 `selectedItems = [null]`，JavaScript `includes(item.Value)` 不會匹配任何真實值，無 checkbox 被預選。語義正確。
- **upsert 未設 EXPIRED_DATE 初始值**：新建 `member_Role` 時 `EXPIRED_DATE` 由後續 `= DateTime.Now` 賦值，不需額外處理。
