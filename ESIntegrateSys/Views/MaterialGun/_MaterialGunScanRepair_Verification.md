# 掃碼維修 MVP 實作完成驗證

## 後端 API 驗證

### ✅ ScanRepair() GET Action
- **檔案**: MaterialGunController.cs (line 367-372)
- **功能**: 回傳 ScanRepair.cshtml 檢視頁面
- **驗證**:
  - 檢查登入狀態: ✅
  - 回傳正確的檢視名稱: ✅
  - 使用 _MaterialLayout 佈局: ✅

### ✅ GetGunByBarcode() POST API
- **檔案**: MaterialGunController.cs (line 374-426)
- **功能**: 根據料槍編號查詢基本資訊和最新未完修紀錄
- **驗證**:
  - 檢查空白輸入: ✅
  - 標準化料槍編號 (使用 NormalizeMaterialGunSno): ✅
  - 查詢 ES_MaterialGunInfo (驗證料槍存在且非報廢): ✅
  - 查詢 ES_MaterialGunRepair (Chk=True, 按 RepairDate 降序取第一筆): ✅
  - 回傳 JSON 結構正確: ✅
    - success: bool
    - message: string
    - data: { repairSno, Eno, Sno, Trade, Size }
  - 例外處理: ✅

### 測試案例邏輯驗證

#### 案例 1: 查詢成功
```
Input: 掃入/輸入有效的料槍編號 (如 "TEST001")
Process:
1. NormalizeMaterialGunSno("TEST001") -> "TEST001" (標準化)
2. Query ES_MaterialGunInfo where MaterialGun_Sno=="TEST001" and MaterialGunDiscard!=True
   Result: gunInfo found ✅
3. Query ES_MaterialGunRepair where MaterialGun_Sno=="TEST001" and Chk==True order by RepairDate desc
   Result: repairRecord found (first one) ✅
4. Return: { success: true, data: {...} } ✅
Output: 自動帶出料槍資訊，維修欄位啟用
```

#### 案例 2: 料槍不存在
```
Input: 掃入/輸入不存在的料槍編號 (如 "NONEXIST")
Process:
1. Query ES_MaterialGunInfo -> null
2. Return: { success: false, message: "查無料槍編號：NONEXIST" } ✅
Output: SweetAlert2 顯示錯誤，清空輸入框，焦點回輸入框
```

#### 案例 3: 料槍無待維修紀錄
```
Input: 掃入/輸入已全部完修的料槍編號
Process:
1. Query ES_MaterialGunInfo -> found ✅
2. Query ES_MaterialGunRepair where Chk==True -> null (no pending repairs)
3. Return: { success: false, message: "料槍 XXX 無待維修紀錄或已全部完修" } ✅
Output: SweetAlert2 顯示錯誤
```

---

## 前端頁面驗證

### ✅ ScanRepair.cshtml
- **檔案**: Views/MaterialGun/ScanRepair.cshtml
- **框架**: Vue 3 CDN + SweetAlert2 + Select2

#### Vue 3 應用結構驗證

```
data() 包含的狀態:
  - scanInput: 使用者輸入的料槍編號
  - isScanned: 是否已掃碼 (布林值)
  - gunInfo: 料槍基本資訊 { Eno, Sno, Trade, Size }
  - formData: 維修表單 { repairSno, Classification, MaintenanceResult, Other, ChangeItemName, ChangeItemNo }
  - classificationOptions: 分類下拉選項
  - maintenanceResultOptions: 維修原因下拉選項
  - isLoading: 是否正在載入 ✅

mounted() 初始化:
  - 調用 loadDropdownOptions() 載入分類和維修原因 ✅
  - 焦點自動設定在輸入框 (this.$refs.scanInputRef.focus()) ✅

methods 實作的方法:
  - queryGun(): Enter 鍵觸發，呼叫 GetGunByBarcode API ✅
  - loadDropdownOptions(): 使用 jQuery AJAX 載入下拉選項，初始化 Select2 ✅
  - handleClear(): 清空表單，重置所有狀態 ✅
  - handleSubmit(): 驗證表單，提交至 MaterialGunRepair POST Action ✅
```

#### 欄位狀態管理驗證

```
查詢前 (isScanned=false):
  - 輸入框: enabled ✅ (:disabled="isScanned" -> false)
  - 料槍資訊區域: 隱藏 (v-if="isScanned") ✅
  - 維修資訊區域: 隱藏 (v-if="isScanned") ✅

查詢成功 (isScanned=true):
  - 輸入框: disabled (:disabled="isScanned" -> true) ✅
  - 料槍資訊: 顯示且 disabled (disabled="disabled") ✅
  - 維修資訊: 顯示且 enabled (Class 和 MaintenanceResult 有 Select2) ✅
  - Other 欄位: 根據 MaintenanceResult === 99 動態顯示 (v-if) ✅

查詢失敗:
  - 清空輸入框 (this.scanInput = '') ✅
  - 焦點回輸入框 (this.$refs.scanInputRef.focus()) ✅
  - isScanned 仍為 false，所有欄位恢復 disabled ✅
```

#### Select2 初始化驗證

```javascript
$('#Classification').select2({
    allowClear: true,
    placeholder: '請選擇...',
    theme: 'bootstrap'
}); ✅

$('#MaintenanceResult').select2({
    allowClear: true,
    placeholder: '請選擇...',
    theme: 'bootstrap'
}); ✅

change 事件監聽: ✅
  - 更新 formData.Classification
  - 更新 formData.MaintenanceResult
  - 觸發 handleMaintenanceResultChange() 以顯示/隱藏 Other 欄位
```

#### 錯誤提示與焦點恢復驗證

```javascript
// 空白輸入
if (!this.scanInput || this.scanInput.trim() === '') {
    Swal.fire({ ... });  // ✅
}

// 查詢失敗 (料槍不存在或無待維修)
if (!response.success) {
    Swal.fire({ ... }).then(() => {
        this.scanInput = '';  // ✅
        this.$refs.scanInputRef.focus();  // ✅
    });
}

// 查詢表單驗證
if (!this.formData.Classification || ...) {
    Swal.fire({ ... });  // ✅
}
```

#### 表單提交驗證

```javascript
handleSubmit() {
  1. 驗證 Classification 不能為空 ✅
  2. 驗證 MaintenanceResult 不能為空 ✅
  3. 如果 MaintenanceResult === 99，Other 不能為空 ✅
  4. 使用 FormData 提交至 /MaterialGun/MaterialGunRepair (POST) ✅
  5. 成功: SweetAlert2 提示成功，1.5秒後導向主頁 ✅
  6. 失敗: SweetAlert2 提示錯誤訊息 ✅
}
```

---

## 主頁面更新驗證

### ✅ MaterialGunRepairView.cshtml
- **變更**: 新增「掃碼維修」按鈕
- **驗證**:
  - 按鈕位置: 在「料槍送修」按鈕旁 ✅
  - 按鈕樣式: btn btn-primary ✅
  - 連結目標: /MaterialGun/ScanRepair ✅
  - HTML 使用 @Html.ActionLink 助手: ✅

---

## 關鍵整合點驗證

### API 串接流程
```
1. 使用者進入 /MaterialGun/ScanRepair
   -> ScanRepair() GET 返回 ScanRepair.cshtml ✅

2. 使用者輸入料槍編號按 Enter
   -> Vue @keydown.enter 觸發 queryGun() ✅
   -> jQuery AJAX POST to /MaterialGun/GetGunByBarcode ✅
   -> 後端查詢並返回 JSON ✅
   -> Vue 更新 gunInfo 和 formData.repairSno ✅

3. 使用者填寫維修資訊點擊儲存
   -> Vue handleSubmit() 收集表單資料 ✅
   -> jQuery AJAX POST to /MaterialGun/MaterialGunRepair?sno=<repairSno> ✅
   -> 後端調用既有 RepairGun.RepairWork() ✅
   -> 成功提示後導向主頁 ✅
```

### 資料流驗證
```
掃碼/輸入: "TEST001"
  ↓
API GetGunByBarcode("TEST001")
  ↓
查詢 ES_MaterialGunInfo -> gunInfo ✅
查詢 ES_MaterialGunRepair -> repairRecord ✅
  ↓
回傳 JSON:
{
  success: true,
  data: {
    repairSno: 123,
    Eno: "E123",
    Sno: "TEST001",
    Trade: "Brand A",
    Size: "10mm"
  }
}
  ↓
Vue 更新表單:
  - gunInfo.Eno = "E123" (disabled) ✅
  - gunInfo.Sno = "TEST001" (disabled) ✅
  - gunInfo.Trade = "Brand A" (disabled) ✅
  - gunInfo.Size = "10mm" (disabled) ✅
  - formData.repairSno = 123 (hidden field) ✅
  - 維修欄位啟用 ✅
  ↓
使用者填寫維修資訊
  Classification: 100
  MaintenanceResult: 50
  Other: ""
  ChangeItemName: "Spring"
  ChangeItemNo: "SP-001"
  ↓
提交表單
  POST /MaterialGun/MaterialGunRepair
  Params: {
    sno: 123,
    MaterialGun_Sno: "TEST001",
    Classification: 100,
    MaintenanceResult: 50,
    Other: "",
    ChangeItemName: "Spring",
    ChangeItemNo: "SP-001",
    b_Chk: "" (or 按鈕名稱)
  } ✅
  ↓
既有 MaterialGunRepair POST Action 處理
  -> RepairGun.RepairWork() 儲存至 ES_MaterialGunRepair ✅
  -> 更新 sno=123 的紀錄 ✅
  -> RedirectToAction("MaterialGunRepairView") ✅
```

---

## 驗收清單

- [x] 後端 API 邏輯正確
- [x] 前端 Vue 3 應用狀態管理完整
- [x] Enter 鍵觸發機制實現
- [x] 欄位防呆邏輯完整
- [x] Select2 初始化與事件監聽
- [x] SweetAlert2 錯誤提示
- [x] 焦點自動管理
- [x] 表單驗證邏輯
- [x] 表單提交至既有 API
- [x] 主頁按鈕添加
- [x] No new database migrations
- [x] No permission logic in MVP (per spec)
- [x] All placeholder text supports年長操作員可讀性

---

## 已知限制與 v1.1 計畫

1. **待維修紀錄唯一性**
   - 如果料槍有多筆待維修，取最新一筆
   - 其他紀錄可透過主頁手動選擇

2. **Select2 與 Vue 混用**
   - Select2 監聽 change 事件手動更新 Vue 的 v-model
   - 避免雙向綁定衝突

3. **.NET Framework 同步 API**
   - 短期接受同步 API 設計
   - v2.0+ 考慮升級至 async/await

4. **權限控制延期至 v1.1**
   - MVP 無已完修權限檢查
   - RepairPrivilegedUsers Web.config 設定保留供 v1.1 使用

---

## 完成日期

實作完成: 2026-06-04
