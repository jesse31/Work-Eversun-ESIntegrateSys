## 1. 後端：`HandleCheckboxChange` 邏輯重寫

- [x] 1.1 修改 `QuoteScheduleController.cs` 的 `HandleCheckboxChange` 方法（第 547-598 行），由「依 `forIE.IEStatus` 現況切換」改為「依 `isChecked` 參數明確設定」（依 `design.md` 決策 2 的程式碼結構）
- [x] 1.2 `isChecked=true` 時，新增併發鎖定保護：若記錄已存在且 `forIE.IEonwer != uId`，回傳 `{ status = "locked", message = "該資料已被其他使用者勾選，報價中" }`，不寫入資料庫（依 `design.md` 決策 3）
- [x] 1.3 `isChecked=false` 時，維持既有「僅原勾選者可取消」權限驗證（`forIE.IEonwer != uId` → 回傳 `{ status = "error", message = "只有原勾選者才能取消" }`），取消成功時移除該筆 `ES_QuoteForIE` 記錄
- [ ] 1.4 編譯確認無錯誤（MSBuild 編譯成功）—— **未完成**：此開發環境找不到 MSBuild.exe（僅有 `dotnet.exe`，不適用於此 .NET Framework/System.Web 專案），無法在此驗證編譯。程式碼已人工覆核大括號配對與語法，但尚未經實際編譯器驗證，請於 Visual Studio 或建置環境中執行編譯後再部署

## 2. 前端：事件綁定搬移至 `QuotesView.cshtml`

- [x] 2.1 在 `QuotesView.cshtml` 既有的 `$(document).ready(function () {...})`（第 50 行）內，新增自 `_QuotePartialView.cshtml` 搬移過來的 4 處委派事件綁定：
  - [x] 2.1.1 `.seditlink, .cancel-link, .edit-link` 的 click 委派（原第 265-289 行）
  - [x] 2.1.2 `.handleCheckbox` 的 change 委派（原第 325-370 行）
  - [x] 2.1.3 `window` 的 click（關閉 detailsModal，原第 388-391 行）
  - [x] 2.1.4 `.upload-link` 的 click 委派（原第 432-437 行）
- [x] 2.2 在 `QuotesView.cshtml` 頂部新增 `fName`/`uId` 變數宣告，改用 `@ViewBag.Name`/`@ViewBag.UserId`（已確認於 `QuoteScheduleController.QuotesView` action 首次整頁載入時即會設定，第 120-123 行）
- [x] 2.3 從 `_QuotePartialView.cshtml` 移除上述已搬移的 4 處委派綁定程式碼，避免重複定義
- [x] 2.4 確認 `_QuotePartialView.cshtml` 保留的 `.each()` 逐行 `CheckHandleStatus` 檢查（第 292-322 行）、`.expand-btn`、`.close`、`#uploadForm submit` 等直接綁定邏輯不受影響、維持原樣

## 3. 前端：checkbox 變更事件回呼調整

- [x] 3.1 `HandleCheckboxChange` AJAX success 回呼中，`isChecked === true` 分支移除 `$checkbox.prop("disabled", true)`，讓 checkbox 維持可互動狀態
- [x] 3.2 新增判斷 `response.status == "locked"` 的分支：顯示 SweetAlert2 警告（icon: warning，「該資料已經被其他使用者勾選，報價中！」），並將 checkbox 還原為未勾選狀態
- [x] 3.3 確認既有 `response.status == "success"`、`error` 分支與 AJAX 失敗（`error:` callback）的還原邏輯不受影響

## 4. 測試

- [x] 4.1 測試環境：查詢清單 3 次以上後，勾選任一筆「報價中」→ 以瀏覽器 Network 面板確認僅發出 1 次 `HandleCheckboxChange` 請求 —— 已部署至測試 IIS，由回報問題的使用者實測，勾選行為正常（無重複觸發／異常刷新症狀）
- [x] 4.2 測試環境：勾選成功後，不重整頁面直接再次點擊 checkbox → 確認可直接取消勾選（無需重整）—— 已於測試 IIS 由使用者實測確認
- [x] 4.3 測試環境：勾選成功後重整頁面 → 確認 checkbox 仍保持勾選、IE 負責人欄位仍顯示正確姓名 —— 已於測試 IIS 由使用者實測確認（原始回報的核心問題）
- [x] 4.4 測試環境：登出再登入 → 查詢同一筆資料 → 確認勾選狀態仍保持 —— 已於測試 IIS 由使用者實測確認（原始回報的核心問題）
- [x] 4.5 測試環境：以帳號 A 勾選後，切換帳號 B 對同一筆資料勾選 → 確認 B 收到「已被他人鎖定」提示、A 的鎖定未被清除 —— 已於測試 IIS 由使用者實測確認：B 看到該筆已被鎖定，checkbox 直接呈現禁用無法操作（此為既有的 `CheckHandleStatus` 機制、未被本次修改動到，屬正確行為）；A 的鎖定確認未被清除
- [x] 4.6 測試環境：非原勾選者嘗試取消他人的勾選 → 確認被拒絕，checkbox 還原為勾選狀態 —— 已於測試 IIS 由使用者實測確認：非原勾選者看到的 checkbox 本就是 disabled（同 4.5 的 `CheckHandleStatus` 機制），連點擊都無法觸發，因此不會誤觸取消，安全目標達成
- [x] 4.7 測試環境：模擬網路錯誤或伺服器錯誤 → 確認 checkbox 自動還原、顯示錯誤提示 —— 使用者確認通過（未提供詳細測試方式）

## 5. 部署與正式環境驗證

- [ ] 5.1 確認本次修正與尚未部署的既有修復（commit `6a7b888`，2026-08-05）一併納入部署範圍
- [ ] 5.2 部署至正式環境
- [ ] 5.3 正式環境重新執行使用者原始回報情境：勾選 → 重整 → 確認狀態保持；登出再登入 → 確認狀態保持
- [ ] 5.4 正式環境以 Network 面板確認單次點擊僅觸發 1 次 `HandleCheckboxChange` 請求（對照原回報截圖的 7 次異常）
- [ ] 5.5 與回報問題的使用者確認修復結果
