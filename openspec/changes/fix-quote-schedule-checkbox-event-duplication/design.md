## Context

`QuotesView` 採「父頁 + AJAX 局部更新」架構：`QuotesView.cshtml` 負責篩選表單，查詢時透過 AJAX 呼叫 `QuotesView` action 取回 `_QuotePartialView.cshtml` 的 HTML，用 `$('#partialViewContainer').html(data)` 整段替換清單內容（`QuotesView.cshtml` 第 96-126 行）。

`_QuotePartialView.cshtml` 自身在檔案尾端內嵌了一大段 `<script>`（第 258-461 行），其中包含 4 處委派事件綁定：

| 綁定對象 | 事件 | 行號 |
|---|---|---|
| `document` (`.seditlink, .cancel-link, .edit-link`) | click | 265 |
| `document` (`.handleCheckbox`) | change | 325 |
| `window`（關閉 detailsModal） | click | 388 |
| `document` (`.upload-link`) | click | 432 |

`.html(data)` 只會替換 `#partialViewContainer` 內部的 DOM 節點，但 `<script>` 標籤仍會隨 HTML 插入而重新執行（jQuery `.html()` 的既有行為）。由於綁定目標是 `document`/`window`（跨越 partial 重建仍存在），每次查詢（含下拉選單自動觸發查詢）都會在其上疊加一份新的事件處理器，舊的處理器不會被移除。

實測（正式環境，2026-08-12，瀏覽器 Network 面板截圖）證實：對同一筆資料勾選/取消一次，觸發 7 次 `HandleCheckboxChange` XHR 請求。

後端 `HandleCheckboxChange`（`QuoteScheduleController.cs` 第 547-598 行）目前邏輯完全未使用前端傳入的 `isChecked` 參數，而是依 `ES_QuoteForIE.IEStatus` 目前是否為 `"U"` 做切換：是則刪除整筆記錄、否則設為 `"U"`。單次呼叫時語意上仍大致正確，但疊加 N 次呼叫後，最終狀態變成「依疊加次數奇偶決定」，且完全無法反映使用者實際的最後一次點擊意圖。

## Goals / Non-Goals

**Goals:**
- 消除「查詢次數越多、同一事件觸發次數越多」的委派事件疊加問題（4 處全部處理，不僅止於 checkbox）
- 讓「報價中」checkbox 的勾選/取消狀態可靠地反映使用者最後一次操作意圖，且能正確持久化於 `ES_QuoteForIE`
- 補上併發保護：不允許使用者的勾選動作覆蓋他人既有的鎖定
- 移除「勾選後 disable、需重整才能取消」的 UX 落差，讓原勾選者可直接取消

**Non-Goals:**
- 不重構 `QuotesView`/`_QuotePartialView` 的整體父子頁架構（維持「父頁查詢 + partial 局部更新」既有模式）
- 不處理 `.expand-btn`、`.close`、`#uploadForm submit` 等本身就會隨 partial 重建而正確銷毀重綁的事件（無此問題）
- 不新增樂觀鎖版本號或分散式鎖機制，併發保護僅靠既有的 `IEonwer` 欄位比對即可滿足目前規模的需求
- 不處理正式環境的實際部署動作（部署為另一個交付步驟，非本次程式碼變更範圍）

## Decisions

### 1. 委派事件綁定搬到父層 `QuotesView.cshtml`，而非在 partial 內加 `.off()`

**選擇**：將 4 處委派綁定自 `_QuotePartialView.cshtml` 搬移至 `QuotesView.cshtml`（放在既有第 50 行 `$(document).ready(function () {...})` 內，與既有的查詢按鈕、下拉選單綁定並列），只在整頁載入時執行一次。

**替代方案**：在 partial 的 `<script>` 綁定前加 `.off("change", ".handleCheckbox").on("change", ...)` 先解綁再綁定。

**理由**：`.off()` 方案改動範圍最小（只動一個檔案），但屬於治標——只要之後有人在 partial 內用同樣「委派綁定寫在會被重複插入的 HTML 裡」的手法新增功能，就會重新踩坑。搬到父層才是讓「一次載入只執行一次」這個不變量在架構上成立，徹底根除此類問題，經與需求方確認後採用此案。

### 2. `HandleCheckboxChange` 改為依 `isChecked` 參數明確設定狀態

**選擇**：
```csharp
if (isChecked)
{
    if (forIE != null && forIE.IEStatus == "U" && forIE.IEonwer != uId)
    {
        return Json(new { status = "locked", message = "該資料已被其他使用者勾選，報價中" });
    }
    if (forIE != null) { forIE.IEonwer = uId; forIE.IEStatus = "U"; }
    else { db.ES_QuoteForIE.Add(new ES_QuoteForIE { id = sno, IEonwer = uId, IEStatus = "U" }); }
}
else
{
    if (forIE != null && forIE.IEonwer != uId)
    {
        return Json(new { status = "error", message = "只有原勾選者才能取消" });
    }
    if (forIE != null) { db.ES_QuoteForIE.Remove(forIE); }
}
db.SaveChanges();
return Json(new { status = "success", ieonwer = isChecked ? ieonwerName : "", iestatus = isChecked ? "U" : "" });
```

**替代方案**：維持切換邏輯，只靠前端修好事件重複綁定來間接避免問題重現。

**理由**：切換邏輯讓後端行為與傳入參數脫節，即使前端不再重複觸發，仍留有「後端邏輯意圖不明確」的技術債，且完全無法防禦併發鎖定被覆蓋的問題（見決策 3）。改為明確依參數設定，一次修正到位。

### 3. 併發鎖定保護：`isChecked=true` 時檢查既有 `IEonwer`

**選擇**：如決策 2 程式碼所示，`isChecked=true` 且記錄已存在且 `IEonwer` 非目前使用者時，回傳 `status: "locked"`，不寫入。

**理由**：舊邏輯下，只要目前狀態是 `"U"`，任何使用者點擊 checkbox 都會直接刪除該筆鎖定記錄，等同任何人都能清除他人的鎖定，屬於資料一致性風險。此保護與既有「取消時僅原勾選者可操作」的權限驗證邏輯對稱，一併補上。

**前後端回應欄位對齊**：前端 `_QuotePartialView.cshtml`（未來搬至 `QuotesView.cshtml`）第 342-345 行已寫有 `if (response.isChecked) {...}` 的「已被他人鎖定」提示分支，但後端從未回傳 `isChecked` 欄位，此分支為死碼。本次選擇修改**前端**改為判斷 `response.status == "locked"`（對齊決策 2 後端回應的既有欄位風格 `status`/`message`），而非在後端額外回傳 `isChecked` 欄位，理由是專案既有的錯誤回應已統一採用 `{ status, message }` 形式（如「只有原勾選者才能取消」），維持單一回應格式風格一致性。

### 4. 移除「勾選後 disable checkbox」

**選擇**：`HandleCheckboxChange` AJAX 的 success 回呼中，`isChecked === true` 分支不再呼叫 `$checkbox.prop("disabled", true)`。

**理由**：後端已有「僅原勾選者可取消」的權限驗證把關（決策 2 的 else 分支），前端不需要再靠 `disabled` 屬性防呆；維持 disable 反而讓原勾選者必須重新整理頁面才能取消，與「原勾選者可自行取消」的預期矛盾（此為既有已知問題，先前的規格草稿明文要求 disable，與此矛盾，本次修正時一併更正該矛盾的規格描述）。

## Risks / Trade-offs

- **[風險] 搬移事件綁定到父層後，若日後又有人在 partial 內新增委派在 document/window 上的事件** → 緩解：無法用程式碼強制避免，僅能靠 code review 把關；建議之後若發現同類模式，優先比照本次做法搬到父層
- **[風險] 併發鎖定保護是新增的行為限制（BREAKING）**，先前「後點擊者可直接覆蓋」的行為即使是缺陷，仍可能被少數使用者依賴（例如手動清除卡住的鎖定）→ 緩解：`ClearIEStatus` action（Controller 第 503 行）仍保留、未變動，可作為管理員/特殊情況下清除鎖定的既有管道
- **[風險] 正式環境目前可能運行舊版程式碼（未部署 2026-08-05 的既有修復）**，本次修正若只部署新程式碼、未確認舊版已被完整取代，可能無法徹底解決問題 → 緩解：部署後需在正式環境重新以 Network 面板實測，確認單次點擊僅觸發 1 次 `HandleCheckboxChange` 請求

## Migration Plan

1. 部署本次修正（連同尚未上線的 2026-08-05 既有修復一併部署，兩者為同一批未部署變更）
2. 部署後於正式環境重新執行使用者原始回報情境：勾選 → 重整 → 確認狀態保持；登出再登入 → 確認狀態保持
3. 以瀏覽器開發者工具 Network 面板確認：單次點擊 checkbox 僅觸發 1 次 `HandleCheckboxChange` 請求（不再是多次）
4. 無資料庫結構異動，無需資料遷移；`ES_QuoteForIE` 既有資料（含目前狀態不一致的殘留鎖定記錄）維持原樣，不做回溯清理
5. 若部署後發現異常，可直接回退程式碼版本；`ES_QuoteForIE` 資料表結構未變動，回退不需資料庫還原
