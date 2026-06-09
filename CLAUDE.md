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

# Core Skill

優先遵循：

skills/karpathy-guidelines/SKILL.md

---

# 技術棧規範

# 既有系統相容與技術沿用規範 (Legacy System Alignment)

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

資料庫

Redis

HTTP

檔案操作

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