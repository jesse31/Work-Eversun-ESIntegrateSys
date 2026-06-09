## Purpose

在 90 天觀察期結束後（2026-09-10），執行數據分析腳本，計算 WoTimeSheet 的使用者覆蓋率，以評估功能是否應保留。

---

## ADDED Requirements

### Requirement: 執行數據分析腳本
觀察期結束後，應執行自動化的 Python/PowerShell 分析腳本，從 90 天的日誌檔案中提取和分析數據。

#### Scenario: 腳本環境準備
- **WHEN** 2026-09-10 或之後開始數據分析
- **THEN** 應確認分析環境已準備（Python 3.x 或 PowerShell 7+ 可用），所有必要的套件或模組已安裝

#### Scenario: 腳本執行和日誌解析
- **WHEN** 執行分析腳本，指定 Logs 目錄路徑
- **THEN** 腳本應讀取所有 WoTimeSheet-2026-06-11.txt 至 WoTimeSheet-2026-09-09.txt 的檔案，逐行解析日誌格式，提取 UserId、Department、Status、Timestamp 等欄位

#### Scenario: 異常日誌處理
- **WHEN** 分析過程中遇到格式異常或損壞的記錄
- **THEN** 腳本應記錄異常檔案和行號，繼續處理其他記錄，最後在分析報告中列出受影響的日期範圍

---

### Requirement: 計算使用者覆蓋率指標
腳本應計算以下核心指標：WoTimeSheet 活躍使用者數與全系統活躍業務使用者數的比例。

#### Scenario: 提取 WoTimeSheet 不重複使用者
- **WHEN** 腳本分析 90 天日誌檔案
- **THEN** 應識別所有不重複的 UserId（排除 Department='IT'），計數為 `WoTimeSheet_Users`

#### Scenario: 提取全系統活躍業務使用者
- **WHEN** 分析全系統日誌或查詢資料庫
- **THEN** 應識別所有不重複的 UserId（非 IT 部門），計數為 `Active_Business_Users`
- **注意**：數據來源應清晰定義（例如同期系統日誌、資料庫使用者表等）

#### Scenario: 計算覆蓋率百分比
- **WHEN** 取得 WoTimeSheet_Users 和 Active_Business_Users 的計數
- **THEN** 計算覆蓋率 = `(WoTimeSheet_Users / Active_Business_Users) × 100%`，結果精確到小數點一位

#### Scenario: 與閾值進行比較
- **WHEN** 覆蓋率計算完成
- **THEN** 腳本應自動判斷：
  - 若覆蓋率 > 50%：輸出「功能保留」建議
  - 若覆蓋率 ≤ 50%：輸出「功能廢棄或優化」建議

---

### Requirement: 生成分析報告
腳本應生成可讀的分析報告，包含計算過程、結果、異常說明和決策建議。

#### Scenario: 報告內容完整性
- **WHEN** 分析完成後生成報告
- **THEN** 報告應包含：
  - 觀察期時間範圍（2026-06-11 至 2026-09-09）
  - WoTimeSheet 使用者總數和名單
  - 全系統活躍業務使用者總數
  - 覆蓋率百分比
  - 任何異常日期和原因
  - 最終決策建議（保留/廢棄）

#### Scenario: 報告格式和可追溯性
- **WHEN** 生成報告
- **THEN** 報告應為人類可讀格式（PDF、Markdown 或 Excel），清晰顯示計算步驟，便於複查和驗證

#### Scenario: 報告交付
- **WHEN** 分析報告完成
- **THEN** 應保存至專案文件位置（例如 `#文件/wotimesheet-coverage-analysis-2026-09-10.md`），並通知相關決策者

---

### Requirement: 支援手動驗證和重新運行
分析腳本應設計為可重複執行，支援在不同環境中驗證結果。

#### Scenario: 腳本參數化和配置
- **WHEN** 執行分析腳本
- **THEN** 腳本應支援命令行參數指定 Logs 目錄路徑和輸出檔案位置，無需修改程式碼

#### Scenario: 結果重現
- **WHEN** 以相同的 Logs 目錄和參數重新執行腳本
- **THEN** 應產生相同的計算結果，確保分析的可追溯性和準確性

#### Scenario: 環境差異文件化
- **WHEN** 在不同環境（開發機、正式伺服器）執行分析
- **THEN** 報告應說明執行環境（Python 版本、執行日期、執行者）和任何環境差異對結果的影響
