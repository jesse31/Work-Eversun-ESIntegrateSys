## Context

視圖命名規範是代碼可維護性的基礎。在 MaterialGun 相關功能中：
- 大多數視圖遵循 `MaterialGun<功能>.cshtml` 的命名慣例（如 MaterialGunRepair.cshtml、MaterialGunMaintain.cshtml）
- ScanRepair.cshtml 在先前改名為 MaterialGunScanRepair.cshtml，代碼層面（控制器、連結）已更新
- 但文檔和規格書中仍保留舊的 `ScanRepair` 參考，造成文檔與實現的不一致

**當前狀態：**
- 代碼實現：已改名（檔案、控制器 Action、View() 呼叫、ActionLink 連結）
- 文檔參考：未更新（規格書中仍有 5+ 處舊名稱參考）

## Goals / Non-Goals

**Goals：**
- 統一文檔中所有的視圖命名參考，確保與代碼實現一致
- 梳理所有參考舊名稱的文件位置
- 建立清晰的改名檢查清單，確保沒有遺漏

**Non-Goals：**
- 修改代碼實現（已完成）
- 新增或移除功能
- 修改控制器邏輯或視圖樣式

## Decisions

### D1：文檔更新範圍
**決策**：逐一掃描所有規格書文件（.md 和 .html），統一更新舊名稱參考。

**理由**：
- 規格書是用戶和開發者的參考，必須準確反映實現
- 舊名稱會造成新人困惑，降低代碼可維護性

**替代方案**：
- A1：建立名稱對應表在 README 中 — 麻煩且易遺忘
- A2：只更新 HTML 規格書，保留 Markdown — 造成不同格式版本不一致（不採納）

### D2：驗證策略
**決策**：使用三層驗證確保完整性：
1. 代碼層 — grep 搜索確認改名完成
2. 文檔層 — 手工審查所有規格書檔案
3. 完整性檢查 — 確認無遺漏參考

**理由**：防止遺漏任何殘留參考，提升命名一致性

## Risks / Trade-offs

**Risk R1：遺漏某些參考位置**
- 現象：規格書中可能有多版本或隱藏的舊名稱參考
- 緩解：使用 grep 搜索所有文本檔案，逐一清理

**Trade-off T1：文檔版本管理**
- HTML 規格書和 Markdown 版本需要同步更新
- 權衡：優先更新主規格書，確保所有版本最終一致

## Migration Plan

1. **代碼驗證**（5 分鐘）
   - grep 搜索代碼中的 `ScanRepair` 參考
   - 確認所有代碼層改名已完成

2. **文檔梳理**（10 分鐘）
   - 列出所有提及舊名稱的文件
   - 標記需要更新的位置

3. **文檔更新**（15 分鐘）
   - 逐一更新規格書文件
   - 確認參考改為 `MaterialGunScanRepair`

4. **最終驗證**（5 分鐘）
   - grep 確認不再存在舊名稱參考
   - 手工審查關鍵文件確認改名完整

## Open Questions

- Q1：是否有其他 View（如 MaterialGunForRepair、MaterialGunInfoView）也需要統一命名？
- Q2：API 端點名稱（如 ScanRepairSave）是否也需要改名以保持一致性？
