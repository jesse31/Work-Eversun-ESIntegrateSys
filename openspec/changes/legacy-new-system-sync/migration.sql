-- ============================================================
-- legacy-new-system-sync：ES_QuoteUploadFiles.FilePath 遷移腳本
-- 用途：舊系統 FilePath 欄位由「完整本地路徑」改為「僅存檔名」
-- 注意：本腳本僅為草稿，尚未執行。請先在測試環境驗證，
--       並於執行 UPDATE 前完成 ES_QuoteUploadFiles 表備份（tasks.md 1.2）
-- ============================================================

-- Step 1：核對影響範圍與筆數（對應 tasks.md 3.1）
-- 執行後請人工確認筆數與資料是否符合預期，再進行下一步
SELECT
    sno,
    RecordId,
    FilePath,
    FileName,
    UploadTime,
    DeptNo
FROM ES_QuoteUploadFiles
WHERE FilePath LIKE 'D:\%UploadedFiles%';

-- 統計筆數
SELECT COUNT(*) AS AffectedRowCount
FROM ES_QuoteUploadFiles
WHERE FilePath LIKE 'D:\%UploadedFiles%';

-- ============================================================
-- Step 2：批次更新（對應 tasks.md 3.2 測試環境 / 3.4 正式環境）
-- ⚠️ 執行前請確認：
--   1. 已完成 ES_QuoteUploadFiles 表備份（tasks.md 1.2）
--   2. 已完成實體檔案遷移至網路共享路徑（tasks.md 2.1-2.3）
--   3. 已在測試環境驗證過本腳本無誤
-- ============================================================
UPDATE ES_QuoteUploadFiles
SET FilePath = FileName
WHERE FilePath LIKE 'D:\%UploadedFiles%';

-- Step 3：更新後驗證（對應 tasks.md 3.3）
-- 預期：以下查詢應回傳 0 筆（不應再有殘留的完整路徑）
SELECT COUNT(*) AS RemainingOldPathCount
FROM ES_QuoteUploadFiles
WHERE FilePath LIKE 'D:\%UploadedFiles%';

-- 預期：FilePath 應與 FileName 完全一致
SELECT COUNT(*) AS MismatchCount
FROM ES_QuoteUploadFiles
WHERE FilePath <> FileName;
