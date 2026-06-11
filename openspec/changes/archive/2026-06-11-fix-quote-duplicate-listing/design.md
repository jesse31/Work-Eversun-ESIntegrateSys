## Context

QuoteScheduleService 中的 `GetQuoteData()` 方法用於查詢報價清單，其 LINQ 查詢直接 JOIN 到 ES_QuoteUploadFiles 表。當一個上傳記錄（ES_QuoteUploadRecords）包含多個檔案時，LEFT JOIN 會產生笛卡爾積，導致同一份報價單在結果集中出現多筆。

對比現狀：
- QuoteQuery.QuoteDataSearch() 已使用 GROUP BY 解決此問題（用於 Excel 匯出）
- QuoteScheduleService 是較新的 Service 層實作，但未採用相同的去重邏輯
- 清單分頁、排序、統計都因此受影響

## Goals / Non-Goals

**Goals:**
- 修正 GetQuoteData() 的查詢邏輯，每份報價單在清單中只顯示一次
- 保持與現有 QuoteQuery 實作的一致性（都使用 GROUP BY）
- 確保 HasFiles 和 FRecordId 欄位正確賦值
- 驗證分頁、排序、篩選功能正常運作

**Non-Goals:**
- 修改 QuoteQuery.QuoteDataSearch()（已正常運作，避免不必要的改動）
- 重構整個查詢架構或遷移到 EF Core（超出範圍）
- 修改資料模型或資料庫結構

## Decisions

### 1. 修改位置：QuoteScheduleService.GetQuoteData()

**選擇：** 在 Service 層的 LINQ 查詢中添加 GROUP BY 聚合

**理由：**
- 查詢結果直接提供給 Controller，在此層修復問題最簡潔
- 無需改動 DTO 或 Controller 邏輯
- 與現有 QuoteQuery 的實作模式一致

**替代方案考量：**
- ❌ 在 Controller 中做 Distinct()：效率低，DB 無法優化
- ❌ 修改 DTO 或返回型別：破壞現有契約，改動過大
- ✅ 在 LINQ 中用 GROUP BY：最佳，DB 端優化

### 2. GROUP BY 結構

**模式（仿照 QuoteQuery）：**
```csharp
group new { a, b, c, d, e, f, h } by new 
{
    a.sno,           // 報價單編號
    a.CustNo,        // 客戶編號
    a.SalesId,       // 業務編號
    // ... 其他聚合鍵
} into g
select new QuoteDataListDto
{
    HasFiles = g.Any(x => x.h.RecordId.HasValue),
    FRecordId = g.Select(x => x.h.RecordId).FirstOrDefault(),
    // ... 其他欄位
}
```

**為什麼這個結構：**
- 按報價單主鍵 (sno) 聚合，確保每個報價單一筆
- 使用 `.Any()` 判斷是否存在檔案（避免 null 比較錯誤）
- 使用 `.FirstOrDefault()` 取得第一個檔案記錄的 ID（供放大鏡使用）
- DeptNo 也通過 `.FirstOrDefault()` 取得（與檔案相關的部門資訊）

### 3. 聚合鍵的選擇

**包含的欄位：**
- a.sno（報價單編號）
- a.CustNo、a.SalesNo、a.SalesId（客戶和業務資訊）
- a.EngSr、a.CustMaterial、a.WoNoAttri（產品資訊）
- a.RequDate、a.CreateDate（日期資訊）
- a.CancelChk（取消狀態）
- a.Mark（備註）
- e.fUserId（業務員ID）
- d.id、d.IEStatus、d.IEQuoteDate、d.IEQuoteTDate、d.IEMark、d.IEonwer（IE相關）

**理由：** 包含查詢結果中使用的所有非聚合欄位，確保完整的報價單資訊

## Risks / Trade-offs

| 風險 | 影響 | 緩解策略 |
|------|------|---------|
| GROUP BY 新增的複雜度 | 查詢性能可能略微下降（但應在可接受範圍） | 添加必要的資料庫索引；測試大數據量場景 |
| 聚合鍵遺漏欄位 | 某些欄位無法正確取值 | 仔細檢查 GetQuoteData() 返回的所有欄位是否在聚合鍵中 |
| FRecordId 多值情況 | 當一份報價單有多個上傳記錄時，只取第一個 | 此為設計現狀（放大鏡呈現該記錄的所有檔案），不改變行為 |
| 與 Excel 匯出邏輯不一致 | 兩個查詢方法略有差異 | QuoteQuery 已驗證正確；此修改與其保持一致 |

**權衡決策：**
- 犧牲小幅查詢複雜度，換取清單顯示的正確性 ✅
- 保留 FRecordId 多值時取 FirstOrDefault 的行為，不改變現有邏輯 ✅

## Migration Plan

### 部署步驟

1. **測試開發環境**
   - 修改 QuoteScheduleService.GetQuoteData() 添加 GROUP BY
   - 本地測試：清單查詢、分頁、排序、篩選
   - 驗證放大鏡詳細頁仍能正常取得檔案清單

2. **資料庫準備**
   - 檢查 ES_QuoteUploadFiles、ES_QuoteUploadRecords 表是否有必要的索引
   - 在 RecordId、Q_sno 等外鍵欄位上確保有索引

3. **程式碼部署**
   - 提交修改至版本控制
   - 部署到測試環境執行完整回歸測試
   - 確認其他依賴 GetQuoteData() 的功能無異常

4. **線上部署**
   - 標準部署流程，無需資料遷移
   - 部署後驗證：清單顯示正確筆數、分頁無誤、Excel 匯出正常

### 回滾策略

- **無狀態改動：** 如查詢邏輯有問題，直接還原程式碼即可
- **無資料迴圈：** 不涉及資料修改，可安全回滾

## Open Questions

1. **是否需要修改 QuoteQuery.QuoteDataSearch()？**
   - 當前: 已使用 GROUP BY，運作正常，建議不改動
   - 決策: 保持現狀，兩個方法分別演進

2. **DeptNo 欄位的聚合策略是否正確？**
   - 當前: 使用 FirstOrDefault() 取檔案相關的 DeptNo
   - 待確認: 是否應採用不同的聚合策略（如 Max、Min、Concat）
   - 建議: 實作時與產品確認業務邏輯

3. **大量檔案場景下的效能基準？**
   - 當前: 無效能基準測試
   - 建議: 測試包含 100+ 檔案的上傳記錄，確認查詢時間在可接受範圍
