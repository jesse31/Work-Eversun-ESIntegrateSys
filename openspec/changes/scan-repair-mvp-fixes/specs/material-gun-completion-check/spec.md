## ADDED Requirements

### Requirement: 已完修料槍狀態檢查與提示
系統應在掃碼查詢時判斷料槍是否已全部完修。若已全部完修，應提示用戶警告訊息並禁止進入維修表單。

#### Scenario: 掃到已全部完修的料槍
- **WHEN** 操作員掃碼或輸入一個已全部完修的料槍編號
- **THEN** API GetGunByBarcode 回傳 status: "all_completed"
- **AND** 前端收到回應後彈出 SweetAlert2 警告窗口「此料槍已全部完修，無待維修紀錄」
- **AND** 警告窗口關閉後，系統清空掃碼輸入框
- **AND** 維修表單保持隱藏（不帶出任何維修資訊）

#### Scenario: 掃到有待修紀錄的料槍
- **WHEN** 操作員掃碼或輸入一個有待修紀錄的料槍編號
- **THEN** API GetGunByBarcode 回傳 status: "success" 及料槍資訊
- **AND** 前端正常帶出料槍資訊和維修表單
- **AND** 系統不彈任何警告訊息

#### Scenario: 料槍不存在的情況（保持既有行為）
- **WHEN** 操作員掃碼或輸入一個不存在的料槍編號
- **THEN** API GetGunByBarcode 回傳 status: "error"
- **AND** 前端彈出 SweetAlert2 錯誤提示「查無料槍編號」
- **AND** 清空輸入框，焦點回到輸入框

---

### Requirement: 已完修判斷邏輯
系統應根據 ES_MaterialGunRepair 表中的 Chk 欄位判斷料槍是否已全部完修。

#### Scenario: 判斷為已全部完修
- **WHEN** 查詢該料槍的所有維修紀錄，發現無任何 Chk=True 的紀錄
- **THEN** 系統判定此料槍已全部完修
- **AND** API 返回 status: "all_completed"

#### Scenario: 判斷為有待修
- **WHEN** 查詢該料槍的維修紀錄，發現至少存在一筆 Chk=True 的紀錄
- **THEN** 系統判定此料槍有待修工作
- **AND** API 返回 status: "success"，並回傳最新的 Chk=True 紀錄

#### Scenario: 料槍不存在時無判斷
- **WHEN** 料槍編號在 ES_MaterialGunInfo 中不存在
- **THEN** 系統先返回「料槍不存在」的錯誤
- **AND** 不進行已完修判斷（完修判斷只在料槍存在的前提下進行）

---

### Requirement: API 回應格式擴展（向後相容）
GetGunByBarcode() API 應擴展回應結構，新增 status 欄位，以精準區分三種業務狀態。

#### Scenario: 成功查詢回應格式
- **WHEN** 查詢成功且料槍有待修紀錄
- **THEN** API 返回：
  ```json
  {
    "success": true,
    "status": "success",
    "message": "查詢成功",
    "data": {
      "repairSno": "...",
      "Eno": "...",
      "Sno": "...",
      "Trade": "...",
      "Size": "..."
    }
  }
  ```

#### Scenario: 已完修回應格式
- **WHEN** 料槍已全部完修
- **THEN** API 返回：
  ```json
  {
    "success": false,
    "status": "all_completed",
    "message": "此料槍已全部完修，無待維修紀錄",
    "data": null
  }
  ```

#### Scenario: 錯誤查詢回應格式
- **WHEN** 查詢失敗（料槍不存在或伺服器錯誤）
- **THEN** API 返回：
  ```json
  {
    "success": false,
    "status": "error",
    "message": "查無料槍編號：XXX",
    "data": null
  }
  ```
