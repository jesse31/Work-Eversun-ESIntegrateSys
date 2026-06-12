## 1. 修正 ReturnUrl 參數

- [x] 1.1 在 `ESIntegrateSys/Controllers/QuoteScheduleController.cs` 第 103 行的 `Url.Action` 匿名物件中補上 `EngSr` 與 `CustMaterial` 參數

## 2. 修正 Session["Admin"] 未設定問題

- [x] 2.1 在 `ESIntegrateSys/Controllers/HomeController.cs` Login POST 成功段（第 75-76 行附近），補上 `Session["Admin"] = result.Member.ROLE_ID;`

## 3. 驗證

- [x] 3.1 在未登入狀態下點擊含 `EngSr`、`CustMaterial` 參數的 QuotesView 連結，確認登入後跳轉的 URL 包含這兩個參數且頁面正常載入
- [x] 3.2 在未登入狀態下直接進入不含 `EngSr`、`CustMaterial` 的 QuotesView，確認登入後行為與修改前相同（不多餘帶入空參數）
