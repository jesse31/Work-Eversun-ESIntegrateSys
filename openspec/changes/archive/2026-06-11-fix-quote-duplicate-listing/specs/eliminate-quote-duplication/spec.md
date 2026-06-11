## ADDED Requirements

### Requirement: 報價清單每份記錄只顯示一次

報價清單（QuotesView）應該每份報價單記錄只顯示一次，無論該記錄關聯了多少個檔案。當一份上傳記錄包含多個檔案時，系統不應在結果集中產生重複行。

#### Scenario: 單檔案上傳記錄
- **WHEN** 報價編號 1280 有一個上傳記錄，包含一個檔案（file1.pdf）
- **THEN** 該報價在清單中顯示一次，HasFiles = true

#### Scenario: 單個上傳記錄包含多個檔案
- **WHEN** 報價編號 1281 有一個上傳記錄，包含多個檔案（file1.pdf, file2.pdf）
- **THEN** 該報價在清單中精確顯示一次，而非出現兩次或更多

#### Scenario: 多個上傳記錄各自包含多個檔案
- **WHEN** 報價編號 1282 有多個上傳記錄，每個記錄各包含多個檔案
- **THEN** 該報價在清單中只顯示一次

### Requirement: 分頁精確性

分頁系統應正確處理去重後的記錄，顯示每頁指定筆數的記錄，不因笛卡爾積擴展而造成計數誤差。

#### Scenario: 每頁筆數限制
- **WHEN** 系統設定每頁顯示 15 份報價
- **THEN** 結果集包含精確的 15 份不重複報價（而非因檔案相關的重複行造成超量）

#### Scenario: 總筆數準確反映唯一報價數
- **WHEN** 使用者篩選和搜尋報價清單
- **THEN** 總筆數和分頁計算反映的是唯一報價記錄，而非經過檔案重複擴展的行數

### Requirement: HasFiles 標記準確指示檔案存在狀態

HasFiles 布林值應準確指示報價是否有任何關聯的上傳檔案，使用聚合邏輯而非依賴 JOIN 的基數。

#### Scenario: 報價有檔案
- **WHEN** 報價編號 1281 有任何關聯的 ES_QuoteUploadFiles 記錄
- **THEN** HasFiles = true

#### Scenario: 報價無檔案
- **WHEN** 報價編號 1279 沒有任何關聯的 ES_QuoteUploadFiles 記錄
- **THEN** HasFiles = false

### Requirement: FRecordId 指向最新上傳記錄

FRecordId 欄位應包含該報價最新上傳記錄的 ES_QuoteUploadRecords.sno（業務開單的主 KEY）。當一份報價有多個上傳記錄時，應取最新上傳的記錄編號。

#### Scenario: 單個上傳記錄
- **WHEN** 報價編號 1281 恰好有一個上傳記錄（sno: 1001），包含 2 個檔案
- **THEN** FRecordId = 1001

#### Scenario: 多個上傳記錄取最新
- **WHEN** 報價編號 1282 有多個上傳記錄（sno: 1001, 1002, 1003，其中 1003 時間最新）
- **THEN** FRecordId = 1003（最新上傳的記錄編號）

### Requirement: 排序和篩選在去重結果集上正常運作

所有排序和篩選操作（按業務、客戶、日期、機種名稱等）應在去重後的資料集上運作，不被重複行的檔案數差異扭曲。

#### Scenario: 按客戶篩選
- **WHEN** 使用者按客戶篩選（CustNo: C001）
- **THEN** 結果包含該客戶的唯一報價，無因多個檔案造成的重複行

#### Scenario: 按需求日期排序
- **WHEN** 使用者按需求日期排序（升序或降序）
- **THEN** 排序順序反映唯一報價的時間順序，不受檔案數差異影響

#### Scenario: 依據 HasFiles 狀態篩選
- **WHEN** 使用者隱含地按 HasFiles 狀態篩選（透過優先考量有檔案報價的排序邏輯）
- **THEN** 排序正確反映檔案存在狀態，不重複計算記錄

### Requirement: 放大鏡展開功能顯示全部檔案

放大鏡按鈕用於展開和查看報價的所有檔案。系統應根據最新上傳記錄編號（FRecordId）查找對應的報價，然後顯示該報價下所有上傳記錄的檔案清單。

#### Scenario: 放大鏡按鈕出現條件
- **WHEN** 報價的 HasFiles = true（存在檔案）
- **THEN** 該行顯示放大鏡圖示

#### Scenario: 單個上傳記錄的放大鏡
- **WHEN** 使用者點擊報價 1281（FRecordId = 1001）的放大鏡圖示
- **THEN** 系統根據 FRecordId 找出報價編號，顯示該報價的所有檔案

#### Scenario: 多個上傳記錄的放大鏡
- **WHEN** 使用者點擊報價 1282（FRecordId = 1003，最新記錄）的放大鏡圖示
- **THEN** 系統根據 FRecordId = 1003 找出報價編號 1282，顯示該報價下所有上傳記錄的全部檔案

### Requirement: Excel 匯出功能不受影響

Excel 匯出功能（ExportToExcel action）應繼續正常運作，使用 QuoteQuery.QuoteDataSearch()，該方法已實作 GROUP BY 去重。

#### Scenario: 匯出筆數正確
- **WHEN** 使用者匯出與清單篩選條件相同的報價（清單顯示 50 份唯一報價）
- **THEN** Excel 檔案包含 50 列（加上標題），而非因檔案相關重複而增加
