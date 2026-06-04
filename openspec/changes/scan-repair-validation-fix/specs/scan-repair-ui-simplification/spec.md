## ADDED Requirements

### Requirement: 移除「料槍資訊」顯示面板

前端 ScanRepair.cshtml 應移除「料槍資訊」區塊的 UI 顯示。掃碼成功後，系統仍應執行後端驗證（取得 repairSno），但不顯示自動帶出的料槍基本資訊（Eno、Sno、Trade、Size），直接進入維修資訊填寫區域，簡化使用者介面。

#### Scenario: 掃碼成功後的頁面狀態
- **WHEN** 使用者掃碼成功（系統回傳 status="success"）
- **THEN** 頁面應隱藏「料槍資訊」面板，直接進入「維修資訊」區塊，焦點移至「檢修不良分類」下拉選單

#### Scenario: 已完修警告後的頁面狀態
- **WHEN** 使用者掃碼觸發 status="all_completed"
- **THEN** 系統彈警告對話框，表單隱藏，「料槍資訊」面板不顯示

#### Scenario: 掃碼失敗後的頁面狀態
- **WHEN** 使用者掃碼觸發 status="error"
- **THEN** 系統彈錯誤對話框，「料槍資訊」和「維修資訊」區塊均隱藏，掃碼輸入框保持獲焦

### Requirement: 保留後端驗證邏輯

系統 SHALL 保留掃碼時的後端驗證邏輯不變。GetGunByBarcode 仍應返回 repairSno（以及 Eno、Sno、Trade、Size），前端仍應將 repairSno 儲存至 formData.repairSno 並提交；只是不再顯示 Eno/Sno/Trade/Size 的 UI 面板。

#### Scenario: formData 綁定完整性
- **WHEN** 掃碼成功且 API 回傳 repairSno
- **THEN** 前端應設置 formData.repairSno = response.data.repairSno，隱藏欄位應正確綁定 name="sno"，提交時 sno 參數應正確傳遞至後端

#### Scenario: 隱藏欄位提交
- **WHEN** 使用者填寫維修資訊並點擊「提交」
- **THEN** 表單應包含隱藏欄位 `<input type="hidden" name="sno" value="..." />`，POST 時 sno 參數應與其他維修資訊一併提交至 MaterialGunRepair Action
