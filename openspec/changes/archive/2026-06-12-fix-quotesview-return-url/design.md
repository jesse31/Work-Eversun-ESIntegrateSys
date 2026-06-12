## Context

`QuoteScheduleController.QuotesView()` 在未登入時，以 `Url.Action` 組建 `returnUrl` 並導向登入頁。登入成功後，系統以此 `returnUrl` 還原使用者原始請求。

**現有問題**（第 103 行）：
```csharp
var currentUrl = Url.Action("QuotesView", "QuoteSchedule",
    new { page, SalesID, CustNo, Indate, Indate2, Sort, Cancel });
// EngSr、CustMaterial 未包含在內
```

`QuotesView` 的 Action 簽章有 `string EngSr` 與 `string CustMaterial` 兩個篩選參數，常見於 email 通知連結中，但建立 `returnUrl` 時被遺漏。

## Goals / Non-Goals

**Goals:**
- 登入後 ReturnUrl 完整保留所有原始查詢參數，包含 `EngSr` 與 `CustMaterial`

**Non-Goals:**
- 不修改登入流程或 Session 機制
- 不調整 `QuotesView` 的查詢邏輯或頁面呈現
- 不影響其他 Action 的登入跳轉行為

## Decisions

### 直接補上缺漏的參數

在現有的匿名物件中加入 `EngSr` 與 `CustMaterial`：

```csharp
var currentUrl = Url.Action("QuotesView", "QuoteSchedule",
    new { page, SalesID, CustNo, Indate, Indate2, Sort, Cancel, EngSr, CustMaterial });
```

- **採用**：直接補參數 — 與現有程式碼風格一致，修改範圍最小
- **放棄**：改用 `Request.RawUrl` 取得完整 URL — 會包含 Host、Port，需要額外處理相對路徑，且跨伺服器部署有 open redirect 風險

### 不使用 `Request.RawUrl`

`Url.Action` 回傳的是相對路徑，天然避免 open redirect；`Request.RawUrl` 包含完整路徑但需額外驗證，不值得為單行修改引入此複雜度。

## Risks / Trade-offs

| 風險 | 緩解 |
|------|------|
| 日後 `QuotesView` 新增參數但再次遺漏 | `EngSr`、`CustMaterial` 是 email 連結的核心參數，此修改解決最常見情境；其餘參數（`page`、`Sort` 等）遺失屬可接受的非關鍵狀態 |
| `null` 參數出現在 URL | `Url.Action` 會自動省略 `null` 值，無副作用 |
