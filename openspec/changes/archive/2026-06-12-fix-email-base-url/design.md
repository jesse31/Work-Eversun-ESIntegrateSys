## Context

`EmailController.Woatt()` 為 ASP.NET MVC 5 專案中負責組裝 email HTML body 的方法。
目前 QuoteSchedule 系統的連結 Base URL（`http://192.168.4.70`）hardcode 於此方法的 5 個 case 中，伺服器遷移後需更新為 `http://192.168.4.133:8004`。

現有設定管理採用 `Web.config` + `ConfigurationManager.AppSettings`，專案中已有此模式（如 `ESIVersion`）。

## Goals / Non-Goals

**Goals:**
- 將 QuoteSchedule Base URL 移至 `Web.config` 集中管理
- 修正連結 IP，使 email 內連結可正常開啟

**Non-Goals:**
- 不重構 `Woatt()` 方法的結構或 switch/case 邏輯
- 不引入強型別 Options 類別（過度設計）
- 不修改其他 email 相關邏輯

## Decisions

### 使用 `ConfigurationManager.AppSettings`

沿用專案既有模式，不引入新的設定讀取方式。

- **採用**：`ConfigurationManager.AppSettings["QuoteScheduleBaseUrl"]`
- **放棄**：強型別 `IOptions<T>` — 需要 ASP.NET Core DI，不適用此 MVC 5 專案
- **放棄**：常數（`const string`）— 雖簡單，但換 IP 仍需重新編譯部署

### 在 `Woatt()` 方法頂端讀取一次

```csharp
var baseUrl = ConfigurationManager.AppSettings["QuoteScheduleBaseUrl"]
              ?? "http://192.168.4.133:8004";
```

以 fallback 值確保 key 遺漏時系統仍可運作，不拋出例外。

### Key 命名：`QuoteScheduleBaseUrl`

語意明確，僅限 QuoteSchedule 模組使用，避免與其他模組混淆。

## Risks / Trade-offs

| 風險 | 緩解 |
|------|------|
| 部署時忘記更新 `Web.config` | fallback 值確保系統不崩潰；email 連結仍可用（指向新 IP） |
| 多環境 `Web.config` 各異 | 透過 `Web.Release.config` Transform 管理，與現有 connectionString 處理方式一致 |

## Migration Plan

1. 更新 `Web.config`：`<appSettings>` 新增 `QuoteScheduleBaseUrl`
2. 修改 `EmailController.cs`：`Woatt()` 頂端讀取設定，替換 5 處 hardcode 字串
3. 編譯確認無誤
4. 部署至 `192.168.4.133:8004`，手動觸發一封測試 email 確認連結正確
