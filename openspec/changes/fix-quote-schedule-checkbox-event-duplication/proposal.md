## Why

正式環境回報：在 `/QuoteSchedule/QuotesView` 勾選「報價中」checkbox 後，狀態沒有保持，重新整理頁面、甚至登出再登入後都會被清除。經比對正式與測試環境的差異行為，並以瀏覽器 Network 面板實際截圖佐證（同一次點擊觸發 7 次 `HandleCheckboxChange` 請求），根因確認為：`_QuotePartialView.cshtml` 把 4 個委派在 `document`/`window` 上的事件綁定寫在會被 AJAX 反覆整段替換的 partial view 裡，每查詢一次就疊加綁定一次；疊加後，後端「依 DB 現況切換」的邏輯讓最終狀態變成不可預期。此外過程中一併發現後端邏輯有資料一致性風險（可被他人的勾選覆蓋），且既有的「勾選後 disable checkbox」設計與「原勾選者可自行取消」的需求互相矛盾（已知問題，先前未修正）。

此問題對應的 capability `quote-schedule-ie-status-management` 先前只在已封存變更（`2026-08-05-fix-quote-schedule-checkbox-ui`）的變更資料夾內留有草稿 spec，從未實際 promote 進 `openspec/specs/`，本次一併正式建檔並修正其中與實作矛盾之處。

## What Changes

- 將 `_QuotePartialView.cshtml` 中 4 處委派在 `document`/`window` 的事件綁定（Edit/Cancel/IE Check 連結點擊、報價中 checkbox 變更、上傳連結點擊、詳細資料 Modal 外部點擊關閉）搬移至父層 `QuotesView.cshtml`，改為整頁載入時只綁定一次，徹底消除隨查詢次數疊加綁定的問題
- `HandleCheckboxChange`（`QuoteScheduleController.cs`）後端邏輯由「依資料庫現況切換」改為「依前端傳入的 `isChecked` 參數明確設定狀態」
- **BREAKING（行為變更）**：`isChecked=true` 時，若該筆資料已被其他使用者鎖定（`IEonwer` 不同），回傳鎖定中錯誤並拒絕覆蓋，不再允許後點擊者直接清掉前一位使用者的鎖定
- 移除勾選成功後將 checkbox 設為 `disabled` 的前端行為，原勾選者可直接再次點擊取消勾選，不需重新整理頁面（後端既有的「僅原勾選者可取消」權限驗證維持不變，作為唯一把關機制）
- 修正前端「已被他人鎖定」提示邏輯與後端回應欄位不一致的問題，使該提示能真正被觸發（先前為死碼，因為後端從未回傳前端所檢查的欄位）

## Capabilities

### New Capabilities
- `quote-schedule-ie-status-management`：QuotesView 頁面「報價中」checkbox 的鎖定/取消鎖定狀態管理，包含權限驗證、併發鎖定保護、失敗還原與使用者提示

### Modified Capabilities

（無：`quote-schedule-ie-status-management` 目前不存在於 `openspec/specs/`，先前草稿未 promote，故以新建處理，不列為 Modified）

## Impact

- **程式碼**：
  - `ESIntegrateSys/Controllers/QuoteScheduleController.cs` — `HandleCheckboxChange` 方法邏輯重寫
  - `ESIntegrateSys/Views/QuoteSchedule/_QuotePartialView.cshtml` — 移除 4 處委派事件綁定與 disable-on-check 行為
  - `ESIntegrateSys/Views/QuoteSchedule/QuotesView.cshtml` — 新增上述 4 處事件綁定，新增 `fName`/`uId` 變數宣告（改用既有的 `ViewBag.Name`/`ViewBag.UserId`，已確認於整頁載入時即會設定）
- **資料庫**：`ES_QuoteForIE` 資料表結構不變，僅寫入/查詢邏輯調整
- **部署**：正式環境目前極可能尚未部署 2026-08-05 的既有修復（commit `6a7b888`），本次修正需與該次修復一併部署，並在正式環境重新驗證，不能只在測試環境驗證
- **不受影響**：`.expand-btn`、`.close`、`#uploadForm submit` 等直接綁定在會隨 partial 重建而銷毀的元素上的事件，不受本次影響，維持原樣
