## 1. 前端 HTML 標記改動

- [x] 1.1 修改 `_QuotePartialView.cshtml` 第 183 行，在 IE 負責人欄位加 id 屬性 → `<td id="ieowner_@item.Sno">@item.IEonwer</td>`
- [x] 1.2 驗證 HTML 語法無誤、id 格式符合命名規範

## 2. 後端 Controller 改動

- [x] 2.1 修改 `QuoteScheduleController.cs` 第 545 行，`HandleCheckboxChange()` 方法簽名加參數 → `public JsonResult HandleCheckboxChange(int sno, bool isChecked, string ieonwerName)`（不新增 uId 參數，權限判斷一律使用 Session 既有的 uId，避免前端竄改冒充身份）
- [x] 2.2 在方法內加入權限檢驗邏輯：如果 `isChecked = false`，檢查 `forIE.IEonwer == uId`（第 548 行 Session 取得的 uId），不符合則回傳 `{ status = "error", message = "只有原勾選者才能取消" }`
- [x] 2.3 修改第 589 行回傳值，改為 `return Json(new { status = "success", ieonwer = isChecked ? ieonwerName : "", iestatus = isChecked ? "U" : "" });`
- [x] 2.4 編譯測試無編譯錯誤（MSBuild 編譯成功，僅既有無關警告）

## 3. 前端 JavaScript 改動 - AJAX 邏輯

- [x] 3.1 修改 `_QuotePartialView.cshtml` 第 325-351 行的 checkbox change 事件
- [x] 3.2 新增變數蒐集：記住原狀態 `var originalState = !isChecked;`；使用者名稱直接重用既有的 `fName`（頁面頂部已定義為 `@ViewBag.Name`），不另建變數
- [x] 3.3 修改 AJAX data 參數，新增 `ieonwerName: fName`（僅用於前端顯示，不用於權限判斷）
- [x] 3.4 刪除 `window.location.assign(window.location.href);` 這一行（不刷新頁面）
- [x] 3.5 在 success 回調中加入 DOM 更新邏輯：
  - [x] 3.5.1 如果 `isChecked = true`：更新 `$ieowner.text(response.ieonwer)`、禁用 checkbox、SweetAlert2 成功提示
  - [x] 3.5.2 如果 `isChecked = false`：清空 `$ieowner.text("")`、啟用 checkbox、SweetAlert2 取消提示
- [x] 3.6 在 error 回調（含後端回傳 status != success 時）加入容錯邏輯：還原 checkbox `$checkbox.prop("checked", originalState)`、SweetAlert2 錯誤提示

## 4. 前端 JavaScript 改動 - SweetAlert2 提示

- [x] 4.1 修改成功時的提示，改為 SweetAlert2（icon: success、自動關閉 1.5 秒）
- [x] 4.2 修改取消時的提示，改為 SweetAlert2（icon: info、自動關閉 1.5 秒）
- [x] 4.3 修改失敗時的提示，改為 SweetAlert2（icon: error、需手動確認）
- [x] 4.4 驗證 SweetAlert2 已在 QuotesView.cshtml 中載入（第 45 行，父頁面載入一次，partial 重複執行 script 時仍可用全域 Swal 物件）

## 5. 整合測試

- [x] 5.1 在開發環境編譯，確保無編譯錯誤（同 2.4，已用 MSBuild 驗證通過）
- [x] 5.2 測試勾選流程：以測試環境（192.168.4.31 / ESIntegrateSysTest）針對 sno=840 實測，勾選後直接查詢 `ES_QuoteForIE` 資料表確認 `IEStatus=U`、`IEonwer` 正確寫入；另一帳號（涂子悻）以工程編號 `UAM0014` 精準查詢時，IE負責人欄位與 checkbox 勾選狀態均正確顯示。確認**跨帳號可見性正常運作**，先前懷疑的「其他人查詢看不到勾選」實為分頁/排序造成的誤判（sno=840 未落在該帳號預設查詢的第一頁），並非功能缺陷。
- [ ] 5.3 測試取消流程：**已知問題尚未修正，暫緩測試**（見下方「8. 實測發現與修正記錄」）
- [ ] 5.4 測試權限檢驗：用另一個使用者取消別人的勾選 → 後端回傳權限錯誤 → 前端還原 checkbox → SweetAlert2 錯誤提示
- [ ] 5.5 測試失敗還原：模擬網路錯誤或伺服器錯誤 → checkbox 自動還原 → SweetAlert2 錯誤提示

## 6. 驗收與上線

- [ ] 6.1 在測試環境進行完整的 E2E 測試（至少涵蓋 5.2-5.5 的所有場景）
- [ ] 6.2 檢查瀏覽器開發者工具，驗證 Network 面板中 AJAX 請求正確、Response JSON 格式無誤
- [ ] 6.3 檢查瀏覽器 Console，驗證無 JavaScript 錯誤或警告
- [ ] 6.4 清瀏覽器快取（Ctrl+Shift+Delete），重新整理頁面，確保舊 JavaScript 不被載入
- [ ] 6.5 驗收：確認 UI 與後端資料一致、提示友善、無 UX 中斷
- [ ] 6.6 提交 PR、代碼複審
- [ ] 6.7 部署至正式環境

## 7. 知識轉移與文檔

- [ ] 7.1 在代碼中添加註解解釋新的權限檢驗邏輯（若需要）
- [ ] 7.2 更新相關的系統文檔或 README（若有）
- [ ] 7.3 記錄此改動在 Git 提交訊息中

## 8. 實測發現與修正記錄（2026-07-31）

### 8.1 跨帳號可見性問題 — 已排查，確認非缺陷

- **現象**：使用者回報「有人勾選報價中後，其他人查詢看不到該筆被勾選」
- **排查方式**：直接查詢測試資料庫（192.168.4.31 / ESIntegrateSysTest）`ES_QuoteForIE` 表，對 sno=840 做即時對照測試（勾選前 / 勾選後 / 他人查詢後三個時間點分別查庫）
- **結論**：資料庫寫入與 `GetQuoteData()` 查詢邏輯皆正確；「看不到」是因為該帳號預設查詢清單分頁/排序關係，sno=840 未出現在第一頁。改用工程編號精準查詢後，IE負責人與 checkbox 狀態皆正確顯示。
- **狀態**：✅ 非缺陷，結案

### 8.2 勾選後無法再取消勾選 — 待修正（尚未套用）

- **現象**：使用者實測，勾選 checkbox 成功後，無法再次點擊取消勾選
- **根因**：`_QuotePartialView.cshtml` 第 350 行，勾選成功後執行 `$checkbox.prop("disabled", true);` 將 checkbox 禁用，導致其永遠不會再觸發 `change` 事件——這與 spec.md 中「原勾選者可自行取消」的需求直接矛盾（此矛盾源自 explore 階段將「防止二次操作」與「可取消」兩個需求同時寫入 spec，實作時才發現無法並存）
- **提議修正**：勾選成功後不禁用 checkbox，改由後端權限驗證把關（已實作於 Task 2.2），前端僅在「AJAX 失敗」或「後端拒絕」時才需要還原狀態，不需要靠 disabled 屬性防呆
- **狀態**：⏳ 待使用者確認後套用（Task 3.5.1 需修正，spec.md 對應描述需同步調整）

- [ ] 8.2.1 修正 `_QuotePartialView.cshtml` 第 350 行：移除勾選成功後的 `$checkbox.prop("disabled", true);`
- [ ] 8.2.2 同步修正 `specs/quote-schedule-ie-status-management/spec.md` 中「checkbox 被禁用（防止二次操作）」的矛盾敘述
- [ ] 8.2.3 修正後重新測試 5.3（取消流程）與 5.4（權限驗證）
