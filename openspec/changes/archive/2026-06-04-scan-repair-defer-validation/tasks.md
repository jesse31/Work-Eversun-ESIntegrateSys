# Tasks: Scan Repair Validation Deferral

## 1. Code Review & Verification

- [x] 1.1 確認 ScanRepair.cshtml queryGun() 無維修資料驗證邏輯
- [x] 1.2 確認 ScanRepair.cshtml handleSubmit() 有完整防呆驗證（分類、原因、其他原因）
- [x] 1.3 確認 MaterialGunController.GetGunByBarcode() 只負責查詢，不驗證維修資料
- [x] 1.4 確認 MaterialGunController.MaterialGunRepair() 後端驗證包括料槍 Chk 狀態

## 2. Manual Testing - Scan Flow

- [x] 2.1 掃碼成功：輸入有效編號 → 直接帶表單，焦點進 Classification
- [x] 2.2 掃碼已完修：掃描 all_completed 料槍 → 彈警告，表單隱藏
- [x] 2.3 掃碼失敗：輸入無效編號 → 彈錯誤，掃碼框清空
- [x] 2.4 掃碼時不驗證維修資料：掃碼後即使未填維修資訊也能帶表單

## 3. Manual Testing - Save Flow

- [x] 3.1 分類未選：點存檔 → 彈「尚未選擇檢修不良分類」
- [x] 3.2 原因未選：分類選後但原因空 → 點存檔 → 彈「尚未選擇檢修不良原因」
- [x] 3.3 選「99」未填：維修原因選「99」但其他原因空 → 點存檔 → 彈「尚未輸入其他原因」
- [x] 3.4 全部填完：所有必填欄位完整 → 點存檔 → 提交成功 → 「儲存成功」→ 返回清單

## 4. Manual Testing - UI Flow

- [x] 4.1 「掃下一筆」按鈕：存檔後點按鈕 → 清空表單，焦點回掃碼框
- [x] 4.2 「返回主頁」連結：點按鈕 → 導航至 MaterialGunRepairView
- [x] 4.3 其他原因顯示邏輯：選「99」時顯示輸入框，改選其他時隱藏
- [x] 4.4 下拉選單載入：掃碼後下拉選項正常顯示，可選擇

## 5. Backend Verification

- [x] 5.1 驗證 MaterialGunRepair POST 後端是否檢查料槍 Chk 狀態
- [x] 5.2 驗證後端拒絕已完修料槍的維修紀錄
- [x] 5.3 驗證後端檢查必填欄位（如有遠端驗證）

## 6. File Naming Consistency (New Task)

- [x] 6.1 改名 Action：public ActionResult ScanRepair() → MaterialGunScanRepair()
- [x] 6.2 改名 View 回傳：return View("ScanRepair", ...) → View("MaterialGunScanRepair", ...)
- [x] 6.3 改名 ActionLink：MaterialGunRepairView.cshtml 的 ActionLink("掃碼維修", "ScanRepair", ...)
- [x] 6.4 改名 View 檔案：ScanRepair.cshtml → MaterialGunScanRepair.cshtml
- [x] 6.5 改名輔助檔：_ScanRepair_Verification.md → _MaterialGunScanRepair_Verification.md

## 7. Documentation & Sign-Off

- [x] 7.1 確認 proposal.md 記錄了變更動機與範圍
- [x] 7.2 確認 design.md 記錄了三個關鍵決策
- [x] 7.3 確認 spec.md 涵蓋所有驗收場景
- [x] 7.4 所有測試通過，準備歸檔變更
