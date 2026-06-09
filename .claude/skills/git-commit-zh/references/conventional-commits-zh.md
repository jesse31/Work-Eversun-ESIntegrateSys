# Conventional Commits 正體中文參考

## Type 類型說明

| Type       | 說明                             | 使用時機                                           |
| ---------- | -------------------------------- | -------------------------------------------------- |
| `feat`     | 新功能                           | 新增功能、新增 API 端點、新增頁面                  |
| `fix`      | 修復錯誤                         | 修復 bug、修正邏輯錯誤、修正回歸問題               |
| `docs`     | 文件變更                         | 修改 README、API 文件、註解、JSDoc                 |
| `style`    | 程式碼風格                       | 格式調整、空白、分號、不影響邏輯的變更             |
| `refactor` | 重構                             | 不改變行為的程式碼重組、抽取函式、重新命名         |
| `perf`     | 效能改善                         | 最佳化演算法、減少記憶體使用、快取策略             |
| `test`     | 測試相關                         | 新增測試、修正測試、調整測試設定                   |
| `build`    | 建置系統                         | 修改 webpack、npm scripts、Dockerfile              |
| `ci`       | 持續整合                         | 修改 GitHub Actions、CI 設定檔                     |
| `chore`    | 雜務                             | 更新依賴套件、調整設定檔、不影響 src/test 的變更   |
| `revert`   | 還原                             | 還原先前的 commit                                  |

## Scope 命名慣例

Scope 表示變更影響的範圍，根據變更檔案的路徑或模組來推斷：

- 以模組或目錄名稱為主：`feat(auth):`、`fix(api):`、`docs(readme):`
- 跨多個模組時可省略 scope：`refactor: 統一錯誤處理邏輯`
- 常見 scope 範例：`api`、`ui`、`db`、`auth`、`config`、`deps`、`ci`、`docker`

## Commit Message 格式

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Header（必要）

- 格式：`<type>(<scope>): <subject>`
- scope 為選填，用小括號包裹
- subject 使用正體中文描述，簡明扼要
- 整行不超過 72 個字元
- 不以句號結尾
- 使用祈使語氣（如「新增」而非「新增了」）

### Body（選填）

- 與 header 之間空一行
- 說明「為什麼」做這個變更，而非「做了什麼」
- 每行不超過 72 個字元
- 可使用 `-` 項目符號列舉

### Footer（選填）

- 與 body 之間空一行
- Breaking changes 以 `BREAKING CHANGE:` 開頭
- 關聯 issue 使用 `Refs: #123` 或 `Closes: #456`

## Breaking Changes

兩種標記方式：

1. 在 type 後加 `!`：`feat(api)!: 變更使用者認證 API 回傳格式`
2. 在 footer 中說明：`BREAKING CHANGE: 認證 API 回傳結構從陣列改為物件`

## 範例

### 單行（簡單變更）

```
feat(auth): 新增 OAuth2 社群登入功能
```

```
fix(ui): 修復表單送出後未清除輸入欄位的問題
```

```
docs(readme): 更新安裝步驟說明
```

```
refactor(api): 抽取共用的請求驗證中介層
```

```
chore(deps): 升級 React 至 v19
```

### 多行（複雜變更）

```
feat(api): 新增批次匯入使用者功能

- 支援 CSV 與 JSON 格式匯入
- 新增匯入進度回報 WebSocket 端點
- 單次匯入上限為 10,000 筆

Refs: #234
```

```
fix(db): 修復併發寫入導致資料遺失的問題

交易隔離等級從 READ COMMITTED 調整為 SERIALIZABLE，
避免兩個寫入操作同時修改同一筆記錄時產生競態條件。

Closes: #567
```

### Breaking Change

```
feat(api)!: 變更分頁 API 回傳格式

回傳結構從陣列改為包含 metadata 的物件：
- `data`: 資料陣列
- `pagination`: 分頁資訊（total, page, perPage）

BREAKING CHANGE: GET /api/users 回傳格式變更，
客戶端需更新解析邏輯。
```

## 常用正體中文術語對照

| 英文               | 正體中文       |
| ------------------ | -------------- |
| function           | 函式           |
| variable           | 變數           |
| parameter          | 參數           |
| argument           | 引數           |
| dependency         | 依賴套件       |
| middleware         | 中介層         |
| component          | 元件           |
| module             | 模組           |
| interface          | 介面           |
| implement          | 實作           |
| deploy             | 部署           |
| configuration      | 設定           |
| authentication     | 認證 / 驗證    |
| authorization      | 授權           |
| endpoint           | 端點           |
| callback           | 回呼函式       |
| refactor           | 重構           |
| optimize           | 最佳化         |
| deprecate          | 棄用           |
| initialize         | 初始化         |
| serialize          | 序列化         |
| deserialize        | 反序列化       |
| cache              | 快取           |
| render             | 渲染           |
| routing            | 路由           |
| migration          | 遷移           |
| schema             | 綱要           |
| template           | 範本           |
| instance           | 實例           |
| property           | 屬性           |
| method             | 方法           |
| class              | 類別           |
| inheritance        | 繼承           |
| encapsulation      | 封裝           |
| abstraction        | 抽象           |
| polymorphism       | 多型           |
| exception          | 例外           |
| error handling     | 錯誤處理       |
| type               | 型別           |
| generic            | 泛型           |
| assertion          | 斷言           |
| mock               | 模擬物件       |
| stub               | 替身           |
| fixture            | 測試固件       |
| regression         | 回歸           |
| breaking change    | 破壞性變更     |
| pull request       | 合併請求       |
| code review        | 程式碼審查     |
| branch             | 分支           |
| merge              | 合併           |
| conflict           | 衝突           |
| repository         | 儲存庫         |
| commit             | 提交           |
| staged             | 暫存的         |
