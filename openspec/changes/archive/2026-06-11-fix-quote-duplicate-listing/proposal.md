## Why

報價清單（QuotesView）中存在邏輯錯誤：當一份上傳記錄包含多個檔案時，該份報價單會重複顯示。例如，編號 1281 上傳了 2 個檔案，清單會出現 2 筆相同的報價單記錄，造成使用者困惑且數據計算不準確。

根因：`QuoteScheduleService.GetQuoteData()` 直接 JOIN 到 ES_QuoteUploadFiles（檔案表），當一個上傳記錄有多個檔案時，會產生笛卡爾積。已存在的 `QuoteQuery.QuoteDataSearch()`（用於匯出 Excel）已用 GROUP BY 解決此問題，但新的 Service 層實作遺漏了此邏輯。

## What Changes

- 修改 `QuoteScheduleService.GetQuoteData()` 的 LINQ 查詢邏輯
- 添加 GROUP BY 語句，按報價單聚合檔案資料
- 使用 `.Any()` 和 `.FirstOrDefault()` 判斷是否有檔案及取得記錄 ID
- 結果：清單中每份報價單只顯示一次，確保分頁和排序邏輯正確

## Capabilities

### New Capabilities
- `eliminate-quote-duplication`: 透過查詢層的 GROUP BY 邏輯，確保清單中報價單不重複顯示，每份記錄僅出現一次

### Modified Capabilities
<!-- 無既有 spec 需要修改，此為純修復性變更 -->

## Impact

**受影響的模組：**
- QuoteScheduleController.QuotesView Action — 清單頁面查詢
- QuoteDataListDto — 傳遞的 HasFiles、FRecordId 欄位邏輯確認

**無影響的功能：**
- ✅ 放大鏡詳細頁（RecordDetails 使用獨立查詢）
- ✅ Excel 匯出（QuoteDataSearch 已使用 GROUP BY）
- ✅ 檔案上傳/刪除操作
- ✅ 排序邏輯（HasFiles 用於排序，GROUP BY 已正確處理）

**資料庫影響：** 無，純查詢邏輯調整
