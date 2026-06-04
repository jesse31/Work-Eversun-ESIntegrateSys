## ADDED Requirements

### Requirement: 掃碼驗證邏輯修正 — 識別未完修紀錄

系統 SHALL 在掃碼時正確識別料槍的未完修紀錄。後端查詢 GetGunByBarcode 應按以下順序進行驗證：(1) 料槍存在且未報廢；(2) 查詢最新的未完修紀錄（Chk=false）；(3) 若無未完修紀錄，檢查是否全部已完修（Chk=true）；(4) 根據結果回傳成功、已完修或錯誤狀態。

#### Scenario: 掃碼成功 — 存在未完修紀錄
- **WHEN** 使用者掃碼輸入包含至少一筆未完修紀錄（Chk=false）的料槍編號
- **THEN** 系統回傳 `status="success"` 及該紀錄的 repairSno，前端應允許填寫維修表單

#### Scenario: 掃碼失敗 — 料槍已全部完修
- **WHEN** 使用者掃碼輸入的料槍存在但所有維修紀錄均已完修（Chk=true）
- **THEN** 系統回傳 `status="all_completed"` 及警告訊息，前端應彈警告對話框並禁止表單填寫

#### Scenario: 掃碼失敗 — 料槍不存在
- **WHEN** 使用者掃碼輸入的料槍編號在系統中不存在或已報廢
- **THEN** 系統回傳 `status="error"` 及錯誤訊息「查無料槍編號」，前端應提示使用者

#### Scenario: 掃碼失敗 — 無維修紀錄
- **WHEN** 使用者掃碼輸入的料槍存在但完全沒有任何維修紀錄
- **THEN** 系統回傳 `status="error"` 及錯誤訊息「料槍無維修紀錄」，前端應提示使用者

### Requirement: 查詢條件修正

系統 SHALL 確保 GetGunByBarcode 的查詢邏輯如下：第二步查詢應明確使用 `WHERE MaterialGun_Sno=? AND Chk=false` 取得最新未完修紀錄；第三步檢查已完修時應明確使用 `WHERE MaterialGun_Sno=? AND Chk=true` 檢查是否存在完修紀錄。

#### Scenario: 查詢未完修紀錄
- **WHEN** 後端執行第二步查詢
- **THEN** 只返回 Chk=false 的紀錄，按 RepairDate 降序排列取最新

#### Scenario: 檢查已完修
- **WHEN** 後端未查到未完修紀錄，執行第三步檢查
- **THEN** 只檢查 Chk=true 的紀錄存在否，不混合其他狀態
