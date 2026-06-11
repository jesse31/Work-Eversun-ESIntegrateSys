# CLAUDE.md

## 一、 User Profile & Persona (個人化與角色設定)

### 1.1 AI 互動偏好

- 每次回應使用者的第一個句子開頭，必須使用：**"Hi 豬豬 🐽🐽🐷"**

---

# 語言規範（最高優先級）

所有對話、分析、文件、註解、例外訊息、日誌訊息及 UI 顯示文字，強制使用繁體中文（zh-TW）。

變數、函式、類別、資料表、欄位及 API Route 使用英文命名。

除非使用者明確要求，禁止使用英文或簡體中文回覆。

---

# 開發協作最高準則 (Core Rule)

> 在進行任何系統開發與除錯行動前必須閱讀

**此準則為系統修改的最高原則。** 針對現有專案邏輯，未經嚴格分析、對齊並將行動寫入計畫書或獲得明確許可前，禁止擅作主張進行任何實作更改。

## 嚴守工作邊界

如果現階段任務為「分析與規劃 (Analysis & Planning)」，則不論發現的問題多麼明顯，也絕不直接修改程式碼，只能產出對應的分析報告交予檢閱。

## 禁止未經授權的優化

不得擅自進行「架構重構」、「更換底層實作（如 Dapper 換 EF Core）」或「移除原有程式碼」。所有修改必須與前端分析或使用者明確授權一致。

## 尊重現有代碼的背景考量

現有實作（尤其是帶有 `Optimized` 或手寫 SQL 的部分）通常存在特定背景或限制。即使看似有錯，在未與專案擁有者共同確認前，不得單方面推翻。

## 即時同步，先問再動

遇到不符預期的邏輯時，唯一的正確動作是在對話中即時告知情形，並提出「可行的解決方案供選擇」，等候使用者確認後方可執行，而非自行實作自認為「較好」的解法。

---

# Core Skill

優先遵循（自動引入）：

@.claude/skills/karpathy-guidelines/SKILL.md

---

# 技術棧規範

## 既有系統相容與技術沿用規範 (Legacy System Alignment)

當進行既有系統（如現行 MES 系統）的維護、擴充或 MVP 實作時，若原系統架構與上述「技術棧規範」衝突，一律以「系統穩定度、不破壞既有架構、最低修改範圍」為最高原則，允許以下例外：

1. **資料庫沿用**：若既有功能依賴 Oracle，或屬於 MSSQL 與 Oracle 的跨庫整合專案，維持既有的資料庫連線與查詢模式（如跨伺服器 Linked Server 查詢），唯獨「禁止 SQL 字串拼接與必須參數化」之資安底線不可磨滅。
2. **前端技術沿用**：若既有模組採用傳統 Razor Views 搭配 Ajax/JavaScript，在不適合全面導入 Vue.js 3 的舊頁面維護中，允許沿用原頁面之撰寫風格，以修改範圍最小化為原則。
3. **架構分層放寬**：若既有模組未採用 Clean Architecture（例如為傳統三層式或直連 DB 的舊 MVC 模式），在局部修補（Hotfix）或微幅擴充時，允許依循既有程式碼的 Pattern 進行設計，避免過度設計導致不必要的 Refactoring。
4. **決策主導權**：凡涉及技術沿用或新舊架構衝突，AI 必須在 Propose（提案）階段主動說明「沿用舊技術」與「重構成新架構」的影響與取捨（Pros & Cons），由使用者確認後方可執行。

## Backend

預設使用：

* .NET 8
* ASP.NET Core MVC
* ASP.NET Core Web API
* C#
* Dependency Injection

除非特別要求：

* 不使用 Minimal API
* 不使用 Node.js
* 不使用 Java Spring

---

## Frontend

預設使用：

* Razor Views
* Vue.js 3
* Composition API
* JavaScript

UI 元件：

* Bootstrap 5
* SweetAlert2
* Select2

除非特別要求：

* 不使用 React
* 不使用 Angular

---

## Database

預設使用：

* Microsoft SQL Server

資料存取：

* Dapper（主要）
* Entity Framework Core（輔助 CRUD）

禁止：

* SQL 字串拼接
* 動態 SQL 未參數化

---

## Cache

預設使用：

* Redis

快取格式：

System:Module:Entity:Id

範例：

EIMS:User:1001

---

## Logging

預設使用：

* Serilog

Log 必須包含：

* Timestamp
* Level
* CorrelationId
* Message
* Exception

---

## 即時通訊

預設使用：

* SignalR

---

# 架構規範

採用：

* Clean Architecture
* Repository Pattern
* Service Layer Pattern

分層：

Presentation
↓
Application
↓
Domain
↓
Infrastructure

禁止反向依賴。

---

# 程式碼規範

## Controller

僅負責：

* Routing
* Validation
* Response

禁止：

* SQL
* 商業邏輯

---

## Service

負責：

* Business Logic
* Transaction

---

## Repository

負責：

* Data Access

---

## 非同步規範

* 資料庫
* Redis
* HTTP
* 檔案操作

一律使用：

async / await

---

## XML 註解

所有公開成員必須撰寫：

```csharp
/// <summary>
/// 取得使用者資料
/// </summary>
```

註解使用繁體中文。

---

# 安全規範

## Authentication

使用：

* JWT
* Refresh Token

---

## Authorization

使用：

* RBAC

---

## 密碼儲存

使用：

* BCrypt

禁止：

* MD5
* SHA1
* 明文密碼

---

## Secret

禁止：

* Hardcode ConnectionString
* Hardcode Password
* Hardcode API Key

---

# Code Review 重點

檢查：

* SQL Injection
* N+1 Query
* Missing Index
* Hardcode
* Magic Number
* TODO
* IDisposable 未釋放
* async void
* 未使用 using
* 未參數化查詢

---

# 開發原則

1. 需求不明確先提問
2. 不自行假設資料表或欄位存在
3. 優先使用最簡單可行方案
4. 避免過度設計
5. 避免提前抽象化
6. 修改範圍最小化
7. 所有建議需說明原因與取捨
8. 所有程式碼必須可編譯
9. 所有實作必須可驗證
10. 不得虛構 API、框架或套件

---

## 檔案讀取規則
- `#` 開頭的檔名一律視為暫存、備忘或說明用途。
- Claude Code 不得主動掃描或讀取這類檔案。
- 只有在使用者明確要求時，才可讀取 `#` 開頭的檔案。

---

# Permission Key 命名規範

## 格式

所有 Permission Key 必須使用三段式格式：

```
{subsystem}:{module}:{operation}
```

- 全小寫英文，段內可含連字號（`-`），以半型冒號（`:`）分隔
- Regex：`^[a-z][a-z0-9-]*:[a-z][a-z0-9-]*:[a-z][a-z0-9-]*$`

範例：`sys:user:view`、`wrsgs:material:import`

## 標準 Operation 詞彙表

| 詞彙 | 語意 | 備注 |
|------|------|------|
| `view` | 唯讀查詢、詳細頁 | 所有模組必有 |
| `write` | 新增 + 修改（單筆） | Create + Update 合一 |
| `delete` | 刪除（不可逆） | 永遠獨立，不可併入 write |
| `import` | 批次匯入（覆蓋性） | 需要時才加 |
| `export` | 匯出 / 下載 | 需要時才加 |
| `print` | 列印 | 需要時才加 |
| `approve` | 審核通過（狀態轉換） | 業務操作 |
| `reject` | 審核退回 | 業務操作 |
| `freeze` | 凍結 / 停用 | 業務操作 |
| `confirm` | 業務節點確認 | 業務操作 |
| `assign` | 指派關聯（角色、部門等） | 關聯操作 |
| `member` | 管理成員（加入 / 移除） | 關聯操作 |

## 禁止使用的同義詞

`edit`、`create`、`update`、`add`、`remove`、`bulk`

## 何時需要獨立 Key

1. 操作是不可逆或破壞性的 → 獨立（`delete`、`import`）
2. 操作影響範圍明顯比單筆寫入大 → 獨立（`import` 批次 vs `write` 單筆）
3. 有明確業務角色才能執行 → 獨立（`approve`、`confirm`）
4. 操作本身是唯讀但敏感 → 獨立（`export`、`report`）