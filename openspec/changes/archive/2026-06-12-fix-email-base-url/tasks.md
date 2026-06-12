## 1. 設定檔更新

- [x] 1.1 在 `ESIntegrateSys/Web.config` 的 `<appSettings>` 新增 `<add key="QuoteScheduleBaseUrl" value="http://192.168.4.133:8004" />`

## 2. Controller 修改

- [x] 2.1 在 `EmailController.Woatt()` 方法頂端新增讀取設定值的程式碼：`var baseUrl = ConfigurationManager.AppSettings["QuoteScheduleBaseUrl"] ?? "http://192.168.4.133:8004";`
- [x] 2.2 將第 90 行（`報價安排數超過通知`）的 `http://192.168.4.70` 替換為 `baseUrl`
- [x] 2.3 將第 98 行（`修改報價通知`）的 `http://192.168.4.70` 替換為 `baseUrl`
- [x] 2.4 將第 107 行（`取消報價通知`）的 `http://192.168.4.70` 替換為 `baseUrl`
- [x] 2.5 將第 112 行（`IE報價完成通知`）的 `http://192.168.4.70` 替換為 `baseUrl`
- [x] 2.6 將第 121 行（`default`）的 `http://192.168.4.70` 替換為 `baseUrl`

## 3. 驗證

- [x] 3.1 編譯專案，確認無編譯錯誤
- [x] 3.2 手動觸發測試 email（IE報價完成通知），確認信件連結以 `http://192.168.4.133:8004` 開頭
