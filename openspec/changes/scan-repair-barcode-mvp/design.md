## Context

### 現況系統架構
- **ASP.NET MVC 5.2.7** + **.NET Framework 4.5** (老版本，無 async/await 原生支援，但已廣泛採用)
- **前端**：Bootstrap 3.4.1 + jQuery 3.4.1 + Razor Views 為主；新功能逐步引入 Vue 3 CDN
- **資料庫**：SQL Server，使用 EF 6 Database First 進行 CRUD，複雜查詢用 ADO.NET
- **料槍維修流程**：手動輸入編號 → 開單 → 編輯維修結果 → 儲存
- **現有頁面**：
  - `MaterialGunRepairView.cshtml`：維修清單主頁，列出所有維修紀錄 (已完修/未完修)
  - `MaterialGunRepair.cshtml`：維修編輯頁面，POST Action 儲存維修結果至 `ES_MaterialGunRepair`

### 資料模型
- **ES_MaterialGunInfo**：料槍基本資訊 (Eno, Sno, Trade, Size, MaintainCycle, MaintainNexDate 等)
- **ES_MaterialGunRepair**：維修紀錄 (sno, MaterialGun_Sno, RepairDate, Classification, MaintenanceResult, Chk 等)
  - `Chk=True`：未完修
  - `Chk=False`：已完修

### 制約
- MVP 範圍內**不修改資料庫**（條碼 = MaterialGun_Sno）
- **不能新增全系統 RBAC**，白名單暫存 `Web.config`
- **不能新增 Serilog**，使用既有日誌機制
- 前端必須與 jQuery 共存，不能全面重構

## Goals / Non-Goals

**Goals:**
- 提供快速掃碼進入維修編輯的流程，降低人為選錯料槍的失誤
- 同時支援手動輸入料槍編號，增加靈活性
- 掃碼/輸入後自動帶出料槍基本資訊，減少手動輸入
- 複用既有的維修儲存邏輯，無需新增獨立的保存 API
- 前端使用 Vue 3 CDN，與既有 jQuery 架構無痛共存

**Non-Goals:**
- 新增條碼管理或條碼生成功能
- 實作已完修權限控管（僅在 Web.config 預留白名單設定，v1.1+ 實作）
- 批次掃碼或多筆同時處理
- 全系統 RBAC 重構
- 新增日誌系統或異常監控

## Decisions

### D1：條碼來源 = MaterialGun_Sno
**決策**：掃描槍掃出的條碼值直接等於料槍編號 (MaterialGun_Sno)，無需條碼欄位對應。

**理由**：
- 當前業務中料槍編號本身已具有唯一標識作用
- 無需新增資料庫欄位，MVP 可快速交付
- 後續若需獨立條碼，可升級為 v1.1 加入新欄位

---

### D2：前端框架 = Vue 3 CDN
**決策**：ScanRepair 頁面使用 Vue 3 CDN (`createApp`)，而非改寫為 ASP.NET Controller 傳統 View。

**理由**：
- 規格書明確要求「新頁面使用 Vue 3 CDN」
- CDN 方式可與既有 jQuery 頁面共存，無相依性衝突
- 狀態管理清晰，掃碼/輸入 → 欄位鎖定/解鎖的邏輯易實現

---

### D3：輸入方式 = 掃碼 OR 手動輸入
**決策**：輸入框支援兩種方式：
- **掃碼**：掃碼槍掃入條碼，自動觸發 Enter 鍵
- **手動**：直接輸入料槍編號，按 Enter 提交

**理由**：
- 掃碼槍不是必須硬體，手動輸入提供備選方案
- 某些無掃碼槍環境下仍可使用
- 兩種輸入最終都以 Enter 鍵觸發查詢，邏輯統一

---

### D4：查詢驗證邏輯 = 料槍基本資訊 + 最新未完修紀錄
**決策**：`GetGunByBarcode` API 查詢三層邏輯：
1. 料槍是否存在 (ES_MaterialGunInfo)
2. 該料槍是否有最新的未完修紀錄 (ES_MaterialGunRepair where Chk=True，按 RepairDate desc 取第一筆)
3. 回傳料槍基本資訊 + 維修紀錄序號

**理由**：
- 防止輸入已完修/不存在的料槍
- 自動找到最新的待維修紀錄，減少使用者手動選擇

---

### D5：儲存邏輯 = 複用 MaterialGunRepair POST Action
**決策**：掃碼/輸入維修填完表單後，不新增獨立 API，直接使用既有的 `MaterialGunRepair` POST Action 儲存。

**理由**：
- 維修邏輯已在 MaterialGunRepair POST 實現
- 複用邏輯減少新增代碼，降低維護成本
- 確保掃碼/輸入維修與傳統維修流程行為一致

---

### D6：錯誤提示 = SweetAlert2
**決策**：查詢失敗、驗證失敗等錯誤均使用 SweetAlert2 (Swal.fire()) 彈窗提示。

**理由**：
- 既有頁面已大規模使用，UI/UX 一致
- SweetAlert2 已優化字體尺寸適合年長操作員

---

### D7：輸入觸發 = Enter 鍵
**決策**：輸入框監聽 `@keydown.enter` 事件以送出查詢。

**理由**：
- 掃碼槍掃完自動帶 Enter
- 手動輸入也按 Enter 提交，行為統一

---

### D8：欄位狀態管理 = Vue v-bind:disabled + v-model
**決策**：
- 查詢前：輸入框 enabled，其餘欄位 (料槍編號、廠商、型號、維修資訊) disabled
- 查詢成功：自動帶出的欄位 (編號、廠商、型號) 保持 disabled，維修資訊欄位 enabled
- 查詢失敗：清空輸入框，所有欄位恢復 disabled，焦點回輸入框

**理由**：
- 防呆：強制掃碼/輸入流程，無法跳過或手動修改基本資訊
- 狀態清晰，使用者體驗直白

---

### D9：白名單設定格式 = 逗號分隔
**決策**：`Web.config` 中 `RepairPrivilegedUsers` 設定使用逗號分隔多個使用者 ID。

**範例**：
```xml
<add key="RepairPrivilegedUsers" value="02898,02899,02900" />
```

**理由**：
- 逗號分隔是常見標準，易於閱讀和維護
- 分號可能與 ADO.NET 連線字串衝突

---

### D10：下拉欄位 = Select2 套件 + 搜尋 + 清除
**決策**：維修資訊中的下拉欄位 (分類、原因) 使用 Select2 套件，並啟用搜尋及清除功能。

**設定**：
```javascript
$('#Classification').select2({
    allowClear: true,
    placeholder: '請選擇...'
});
$('#MaintenanceResult').select2({
    allowClear: true,
    placeholder: '請選擇...'
});
```

**理由**：
- Select2 提供更好的下拉體驗，特別是選項眾多時
- 搜尋功能加快操作效率
- 清除功能讓使用者可輕鬆重選
- 與既有 MaterialGunRepair 頁面一致（後續可統一升級）

---

## Risks / Trade-offs

| 風險 | 緩解策略 |
|------|---------|
| **條碼 = Sno 的限制** | 規劃 v1.1+ 新增 Barcode 欄位 |
| **待維修紀錄唯一性** | 取最新未完修紀錄，其他紀錄可回主頁手動選擇 |
| **jQuery 與 Vue 混用** | 隔離 Vue 至獨立 `<div id="app">`，避免全域命名空間污染 |
| **Select2 與 Vue 相互作用** | Select2 主要用 jQuery，需小心不與 Vue 狀態綁定衝突 |
| **.NET Framework 無 async** | 短期接受同步 API，v2.0+ 評估升級 .NET 框架 |

## Migration Plan

**部署步驟**：
1. 部署新的 Controller Action (`GetGunByBarcode`)
2. 部署新的 View (`ScanRepair.cshtml`)
3. 更新 `MaterialGunRepairView.cshtml` 加入掃碼/輸入入口按鈕
4. 更新 `Web.config` 新增 `RepairPrivilegedUsers` 設定（初值空或註解）
5. 引入 Select2 CDN（或 npm）
6. 測試掃碼/輸入流程

**回滾策略**：
- 移除 `/MaterialGun/ScanRepair` 路由或註解按鈕
- 刪除 `ScanRepair.cshtml`
- 刪除 `GetGunByBarcode` Action
- 恢復 `MaterialGunRepairView.cshtml` 至先前版本
