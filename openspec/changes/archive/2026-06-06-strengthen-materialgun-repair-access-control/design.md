## 強化料槍維修存取控制 - Design

### Context

當前 `MaterialGunController.MaterialGunRepair(int sno)` 的 GET 方法直接查詢維修記錄並回傳編輯頁面，不進行任何權限或狀態檢查。防呆邏輯僅在 POST 提交時才執行。

這導致：
1. 非授權使用者可進入已完成維修的頁面
2. 已報廢料槍仍可顯示編輯表單
3. 使用者體驗不佳（進入後才被擋下）

白名單機制已存在於 `IsRepairWhitelistedWithReason()` 方法，可複用。

### Goals / Non-Goals

**Goals:**
- 在 GET 階段即早擋下非授權存取
- 區分提示訊息：無權限 vs 已報廢
- 單一進入點保護（GET 後無需重複檢查）
- 改善安全性與使用者體驗

**Non-Goals:**
- 修改 POST 方法邏輯（已有防護）
- 修改白名單定義（使用現有 MaterialGunConstants）
- 改變 View 或資料模型
- 新增審計日誌（複用既有 Log.ForContext）

### Decisions

#### Decision 1: 檢查順序 - 權限優先

**選擇：** 先檢查權限 (Chk + Whitelist)，再檢查報廢

**理由：**
- 反映業務優先級：無權限是操作層面的拒絕，比報廢（設備層面）更常見
- 使用者體驗：告訴使用者「無權」比告訴「報廢」更明確當下的問題
- 白名單使用者可看到已報廢資訊（偵錯目的）

**實現：**
```
if (Chk == true && !IsWhitelisted) → 提示無權限，返回首頁
else if (MaterialGunDiscard == true) → 提示已報廢，返回首頁
else → 回傳編輯頁面
```

#### Decision 2: 使用 TempData 訊息 + Redirect

**選擇：** TempData["message"] 傳遞提示，RedirectToAction("MaterialGunRepairView", new { page = 1 })

**理由：**
- TempData 與 View 上的 @ViewBag.message 一致（現有模式）
- page=1 確保返回首頁，不保留分頁狀態
- Redirect 比 Json 更適合 GET 方法

**替代方案考慮：**
- ❌ Json：GET 方法中不宜用 Json（前端需要 AJAX 整合）
- ❌ View("Error")：無法複用，維護成本高
- ✅ TempData + Redirect：與現有 CreateMaintainWork 做法一致

#### Decision 3: 記錄存取日誌 (所有路徑)

**選擇：** 無論通過或拒絕，都用 Log.ForContext 記錄

**理由：**
- 安全審計（追蹤非授權存取嘗試）
- 現有做法複用（POST 方法已有模板）

**日誌內容：**
- 通過 → Information 等級：「User {UserId} accessed repair record {Sno}」
- 被擋（無權限） → Warning 等級：「User denied: no whitelist privilege」
- 被擋（報廢） → Warning 等級：「Record is discarded」

### Risks / Trade-offs

| 風險 | 潛在問題 | 緩解方法 |
|------|--------|--------|
| 記錄不存在 | 呼叫 `IsRepairWhitelistedWithReason()` 時 repairRecord 為 null | 先檢查 repairRecord != null，不存在時回傳 404 或友善提示 |
| 未登入使用者 | Session["Member"] 為 null | 複用既有 `Login_Authentication()` 檢查 |
| Discard 欄位 null | `MaterialGunDiscard.Value` 拋例外 | 使用 `repairRecord.MaterialGunDiscard != true` 語法，null 視同 false |
| 多個 Redirect | 使用者被轉向多次 | 檢查清晰且順序正確，只會 Redirect 一次 |

### Implementation Approach

**修改檔案：** `ESIntegrateSys/Controllers/MaterialGunController.cs` 中的 GET MaterialGunRepair 方法

**步驟：**
1. 查詢維修記錄 (Find)
2. 若不存在 → 友善提示
3. 若存在且 Chk == true → 檢查白名單
   - 非白名單 → TempData["message"] = 無權限提示 → Redirect
4. 若 MaterialGunDiscard == true → TempData["message"] = 已報廢提示 → Redirect
5. 否則 → 記錄 Information 日誌 → 回傳編輯頁面

**代碼風格：**
- 複用現有 `IsRepairWhitelistedWithReason()` 方法
- 參考 CreateMaintainWork 的 TempData/Redirect 模式
- 參考 POST 方法的日誌格式

### Open Questions

1. ✅ 返回目標：MaterialGunRepairView (page=1) - **已確認**
2. ✅ 檢查順序：權限優先 - **已確認**
3. ✅ 提示訊息分開：無權限 vs 已報廢 - **已確認**
