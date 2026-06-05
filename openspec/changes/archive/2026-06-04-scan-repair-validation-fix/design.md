## Context

掃碼維修功能在實測中暴露出兩個問題：(1) **後端邏輯反向**：GetGunByBarcode 查詢條件錯誤，使用 `Chk == true` 尋找已完修紀錄而非未完修紀錄，導致掃碼流程無法進行；(2) **前端 UI 冗餘**：自動帶出的「料槍資訊」面板（Eno、Sno、Trade、Size）在 MVP 階段無法增加價值，反而增加頁面複雜度和維護成本。

當前代碼基線：
- 後端：`MaterialGunController.GetGunByBarcode()` 中第二步查詢為 `.Where(x => x.Chk == true)` 
- 前端：`ScanRepair.cshtml` 中存在 `<div v-if="isScanned" class="panel panel-info">` 顯示料槍資訊

## Goals / Non-Goals

**Goals：**
- 修正 GetGunByBarcode 的查詢條件，確保正確識別未完修紀錄（Chk=false）
- 簡化 ScanRepair.cshtml，移除不必要的「料槍資訊」UI 面板
- 保留掃碼驗證邏輯，後端仍執行查詢取得 repairSno，前端仍正確提交參數
- 確保已完修和無紀錄的場景被正確攔截，使用者體驗一致

**Non-Goals：**
- 修改 MaterialGunRepair POST Action 的簽名或邏輯
- 修改下拉選單選項（Classification、MaintenanceResult）的綁定
- 新增其他驗證規則（如權限、業務邏輯）
- 修改資料庫 schema 或欄位定義

## Decisions

### D1：Chk 欄位語義修正
**決策**：將 GetGunByBarcode 的第二步查詢從 `Chk == true` 改為 `Chk == false`；第三步檢查從無條件的 FirstOrDefault 改為明確的 `Chk == true` 篩選。

**理由**：根據業務定義，Chk=false 表示「未完修」，Chk=true 表示「已完修」。當前代碼語義反向，導致功能失效。修正查詢條件是最小化、最直接的解決方案。

**替代方案考慮**：
- A1: 改變資料庫中 Chk 欄位的含義（反向存儲）— 破壞性強，影響現有清單頁面，不採納
- A2: 在前端反向邏輯判斷 — 分散邏輯，難以維護，不如後端統一修正

### D2：前端移除「料槍資訊」面板
**決策**：刪除 ScanRepair.cshtml 中的料槍資訊顯示區塊（整個 `<div v-if="isScanned" class="panel panel-info">` 面板），保留掃碼區和維修資訊區。

**理由**：
- MVP 階段無需確認顯示，使用者掃碼後直接填寫維修資訊即可
- 該面板的資訊（Eno/Sno/Trade/Size）對維修流程非必需（repairSno 已確保料槍有效）
- 簡化頁面結構，降低維護成本

**替代方案考慮**：
- A1: 保留面板但使用簡化佈局（如行內顯示）— 額外工作，不符合 MVP 優先級
- A2: 將料槍資訊以 readonly inputs 形式保留 — 冗餘顯示，浪費空間

### D3：保留隱藏欄位綁定
**決策**：保留 `<input type="hidden" v-model="formData.repairSno" name="sno" />` 的實現，確保 sno 參數正確流轉。

**理由**：repairSno 是提交時的必需參數，不移除隱藏欄位可確保無其他變數影響提交邏輯。此欄位本身不涉及 UI，只影響表單提交。

## Risks / Trade-offs

**Risk R1：已完修的判斷時機**
- **風險**：若料槍在掃碼後、提交前被標記為已完修，提交可能異常
- **現象**：低概率發生，實際業務流程中料槍通常不會在用戶填表期間被另一使用者更改
- **緩解**：提交時後端可再驗證 sno 的狀態；若有必要可後續新增樂觀鎖

**Risk R2：API 返回值的完整性**
- **風險**：若 GetGunByBarcode API 後續被其他頁面（如 MaterialGunRepair.cshtml）共用，移除前端顯示可能遺漏需求
- **緩解**：API 設計應獨立於 UI；若其他頁面需要 Eno/Sno/Trade/Size，應另外呼叫相應 API；ScanRepair 本身不應綁定 API 設計

**Trade-off T1：使用者體驗 vs. 開發效率**
- 移除料槍資訊面板後，使用者無法立即確認掃碼的料槍是否正確（但錯誤的料槍會被後端攔截）
- 權衡：MVP 階段優先完成功能，後續可根據使用者反饋新增簡化確認（如提示「正在維修料槍 XX」）

## Migration Plan

1. **後端修正**（無舊資料遷移需求）
   - 修改 GetGunByBarcode 的查詢條件，提交至版本控制
   - 編譯驗證無錯誤
   - 部署至測試環境

2. **前端修正**
   - 刪除「料槍資訊」區塊（lines ~108-151）
   - 驗證 v-model 綁定 formData.repairSno 仍正確
   - 本地測試：掃碼 → 驗證表單直接進入維修資訊區 → 提交成功

3. **驗收**
   - 掃碼存在未完修紀錄 → 成功進入維修表單
   - 掃碼已全部完修 → 彈警告，表單隱藏
   - 掃碼無紀錄 → 彈錯誤，表單隱藏
   - 維修表單提交 → 後端正確接收 sno 參數

4. **回滾計畫**（若需要）
   - 後端：撤銷查詢條件改動，恢復原始 Chk 判斷
   - 前端：恢復「料槍資訊」面板 HTML

## Open Questions

- Q1: 後續若需要顯示料槍資訊以提升使用者體驗，是否應增加一個簡化的「掃碼確認」提示（如 Toast），而非完整面板？
- Q2: 掃碼後至提交前的時間窗口中，是否需要防止料槍狀態異變（如另一使用者標記為完修）？
