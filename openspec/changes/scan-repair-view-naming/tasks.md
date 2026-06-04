## 1. 代碼層驗證 — 確認改名已完成

- [x] 1.1 grep 搜索代碼中所有 `ScanRepair` 參考（不含 MaterialGunScanRepair）
- [x] 1.2 確認控制器 MaterialGunController.cs 中的 Action 名為 MaterialGunScanRepair()
- [x] 1.3 確認 View() 呼叫使用 View("MaterialGunScanRepair", ...)
- [x] 1.4 確認 MaterialGunRepairView.cshtml 中的 ActionLink 連結到 MaterialGunScanRepair
- [x] 1.5 確認視圖檔案名稱為 MaterialGunScanRepair.cshtml（無 ScanRepair.cshtml 殘留）

## 2. 文檔掃描 — 梳理所有舊名稱參考

- [x] 2.1 grep 搜索規格書文件中的 `ScanRepair`（查找所有 .md 和 .html 檔案）
- [x] 2.2 列出所有需要更新的文件位置（記錄具體行號和上下文）
- [x] 2.3 特別檢查以下文件：
  - [x] 2.3a `#文件/20260604-料槍維修條碼化-MVP規格書.html`
  - [x] 2.3b `#文件/20260604-料槍維修條碼化-MVP階段3-規格書.md`
  - [x] 2.3c `#文件/20260604-料槍維修條碼化-MVP階段2-方案規劃與技術債評估.md`

## 3. 文檔更新 — 統一為新名稱

- [x] 3.1 更新規格書中 `ScanRepair.cshtml` → `MaterialGunScanRepair.cshtml`
- [x] 3.2 更新規格書中 `/MaterialGun/ScanRepair` → `/MaterialGun/MaterialGunScanRepair`
- [x] 3.3 更新 HTML 規格書中的舊名稱參考
- [x] 3.4 更新 Markdown 規格書中的舊名稱參考
- [x] 3.5 檢查是否還有其他隱藏的舊名稱參考（如程式碼片段中的註解）

## 4. 最終驗證 — 確認完整一致性

- [x] 4.1 再次 grep 搜索代碼和文檔中是否存在 `ScanRepair` 殘留（應無結果）
- [x] 4.2 手工審查關鍵文件，確認所有參考均已改為 `MaterialGunScanRepair`
- [x] 4.3 驗證代碼層和文檔層的命名完全一致
- [x] 4.4 確認無其他視圖（如 MaterialGunForRepair、MaterialGunInfoView）需要統一命名
