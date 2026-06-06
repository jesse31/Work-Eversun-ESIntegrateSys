# 料槍維修白名單權限控制 - Design

## Context

**當前狀況：**
- `MaterialGunRepairView` 頁面列出所有料槍維修記錄，根據 `Chk` 欄位（0=未維修，1=已完成）以不同背景顏色標示
- 當前的權限控制只基於 Role ID（V、FR 角色被隱藏「料槍維修」按鈕），未考慮已完成維修記錄的編輯限制
- 系統計畫未來要整合完整的角色權限系統，但目前需要快速實現白名單功能

**使用者流程：**
1. 使用者進入 `/MaterialGun/MaterialGunRepairView`，看到維修清單
2. 未維修 (Chk=0) 的料槍：允許所有非 V、FR 角色的使用者點擊「料槍維修」進行編輯
3. 已完成維修 (Chk=1) 的料槍：只有白名單人員（如 02898）才能編輯，一般人員被阻擋

**制約條件：**
- 白名單暫時使用程式碼常數（不建 DB 表）
- 必須支援稽審追蹤（記錄誰、何時、哪個料槍、成功/失敗）
- 前端隱藏 + 後端驗證（雙層防護）
- 錯誤訊息使用現有的 SweetAlert2 提示

## Goals / Non-Goals

**Goals:**
- 實現針對已完成維修記錄的細粒度權限控制
- 白名單人員能夠重新編輯已完成的維修紀錄
- 非白名單人員無法透過任何方式（UI 按鈕、直接 URL）存取已完成維修的編輯頁面
- 所有維修操作被完整記錄在稽核日誌中
- 實作簡潔易維護，為未來的完整權限系統預留接口

**Non-Goals：**
- 不建置永久性的白名單資料表（暫用程式碼常數）
- 不修改角色系統 (ROLE_ID)
- 不改變未維修 (Chk=0) 的流程邏輯
- 不新增複雜的簽核流程（簽核欄位、簽核状態）

## Decisions

### Decision 1：白名單存儲位置 - 使用程式碼常數

**選擇：** 在 `Helpers/MaterialGunConstants.cs` 中定義 `RepairEditWhitelist` HashSet<string>

**理由：**
- 快速實現，無需建表或修改 DB schema
- 清單人數有限且相對穩定（如 1-5 人），無需動態管理
- 為未來整合完整權限系統預留了清晰的介面（只需改變查詢白名單的方式，邏輯不變）

**替代方案考慮：**
- ❌ Web.config appSettings：難以類型安全、易誤設、需重啟應用
- ❌ 資料庫表：成本高，未來重構時需遷移資料
- ✅ Constants.cs：簡潔、強型態、易於版本管理

### Decision 2：驗證邏輯位置 - 後端 + 前端

**選擇：** 
- 後端：在 Controller 層實施白名單檢查（POST MaterialGunRepair 時）
- 前端：在 Razor View 層隱藏按鈕、JavaScript 層防呆攔截

**理由：**
- 後端驗證是資安防線，防止繞過 URL 直接存取（如 `?sno=123`）
- 前端隱藏是 UX 最佳化，提前阻止非授權操作
- 雙層防護符合「深度防禦」原則

**實作細節：**
```
GET /MaterialGun/MaterialGunRepair?sno=123
├─ Controller 檢查 Chk == 1 且白名單失敗 → 返回編輯 View（但不允許提交）
└─ 或直接返回 View，前端 JS 設定為唯讀模式

POST /MaterialGun/MaterialGunRepair (提交編輯)
├─ 再次驗證白名單 → 失敗時返回 JSON { success: false, msg: "..." }
└─ 前端 AJAX 攔截，顯示 SweetAlert2
```

### Decision 3：稽審日誌方案 - 使用現有 Serilog

**選擇：** 透過 Serilog 記錄操作日誌（ILogger 依賴注入或 static Log）

**理由：**
- 系統已整合 Serilog，無須新增依賴
- 支援結構化日誌（UserId、Sno、操作類型），便於日後查詢和分析
- 日誌分級清晰：Information (正常操作) / Warning (被拒絕) / Error (異常)

**記錄內容：**
- `Information`: 白名單人員成功訪問或編輯
- `Warning`: 非白名單人員被拒絕訪問
- 所有日誌包含：UserId, MaterialGun_Sno, 時間戳, 操作結果

### Decision 4：錯誤提示方案 - 強化既有 SweetAlert2

**選擇：** 複用現有的 SweetAlert2 提示框，新增「無權進行此操作」的文案

**理由：**
- 已完成維修時的提示邏輯已存在（189-195 行檢查 Chk），只需擴充文案
- 維持 UI 一致性，不額外引入新的提示元件

**訊息文案：**
```
現有：「料槍已維修，不可重複維修！」
新增：「此料槍已完成維修，無法修改。無權進行此操作。」
       (僅在白名單驗證失敗時)
```

## Risks / Trade-offs

| 風險 | 影響 | 緩解方案 |
|------|------|--------|
| **常數白名單難以動態更新** | 新增帳號需改程式碼 + 重新部署 | 在完整權限系統完成後立即遷移；過渡期可設定排期更新 |
| **SQL Injection 風險（前端直接傳 UserId）** | 使用者可偽造帳號 | 使用 Session["Member"].fUserId（伺服器端取得，不信任前端） |
| **Serilog 日誌量過大** | 磁碟/記憶體負擔 | 設定適當的日誌保留期 (如 30 天自動清理) |
| **白名單人員權限過大** | 已完成維修可被任意修改，失去完整性 | 記錄 OldValue/NewValue，未來可新增簽核流程 |
| **前端 JS 被繞過（開發者工具）** | 使用者修改 HTML/CSS 顯示隱藏按鈕 | 後端驗證是絕對防線，前端隱藏只是 UX 優化 |

## Migration Plan

**Phase 1：準備階段（開發環境驗證）**
1. 新增 `MaterialGunConstants.cs` 定義白名單
2. 在 `MaterialGunController` 新增 `IsRepairWhitelisted(userId)` 方法
3. 修改 `MaterialGunRepairView()` 傳遞 `ViewBag.IsWhitelisted`
4. 在 `MaterialGunRepairView.cshtml` 修改按鈕顯示邏輯
5. 單元測試 + 手動測試

**Phase 2：實施階段（測試環境）**
1. 修改 `MaterialGunRepair(sno)` GET 方法，加入訪問日誌
2. 修改 `MaterialGunRepair(...)` POST 方法，加入白名單驗證 + 失敗日誌
3. 強化前端 JavaScript 防呆機制
4. 整合測試（白名單 vs. 非白名單使用者）

**Phase 3：上線階段（生產環境）**
1. 部署新程式碼（無資料庫遷移）
2. 驗證白名單帳號清單是否正確
3. 監控 Serilog 日誌，確保紀錄正常
4. 後續巡檢

**回滾策略：**
- 若發現白名單邏輯有誤，臨時註解掉 `IsRepairWhitelisted()` 檢查，恢復原有行為
- 無資料庫依賴，安全回滾

## Open Questions

1. **稽審日誌的保留期多長？** 需要設定 Serilog 的日誌清理政策嗎？
2. **未來整合完整權限系統時，白名單如何遷移？** 是否需預先預留遷移指南？
3. **白名單人員編輯後的資料，是否需要簽核或審批流程？** （目前未納入，可於未來擴展）
4. **白名單清單是否會定期異動？** （目前假設相對穩定，變動不頻繁）
