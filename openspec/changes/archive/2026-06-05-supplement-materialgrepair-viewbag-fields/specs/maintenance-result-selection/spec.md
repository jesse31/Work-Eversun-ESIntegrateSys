# Delta Spec: Maintenance Result Selection - 補齊後端資料傳遞

## MODIFIED Requirements

### Requirement: 初始化和交互的一致性

**Requirement Statement**: 初次載入和用戶重新選擇時，Other 欄位的可見性應一致，且需要顯示上次存檔的 Other 值。

**Given** 頁面初次載入編輯既存記錄，MaintenanceResult = 99，後端傳遞 Other = "馬達故障"  
**When** 頁面完全載入  
**Then** Other 欄位顯示且包含值 "馬達故障"

**驗收標準**：
- 後端傳遞 ViewBag.Other（及 ChangeItemName、ChangeItemNo）
- 前端初始化 JavaScript 變數：`var selectedOther = '@ViewBag.Other'`
- Other input 欄位綁定 value：`value="@ViewBag.Other"`
- 初始化後 Other.display = "block" 且 input.value = "馬達故障"
- 重新選擇 99 後 Other 欄位保留用戶輸入的值
- 選擇其他值後 Other.display = "none"
- 再次選擇 99 後 Other.display = "block"

**實現方式**：
1. Controller GET 方法補充三個 ViewBag 賦值
2. Razor View 初始化時從 ViewBag 取值
3. Input 欄位綁定初始值
4. 共用的 toggleOtherField() 函式繼續在 AJAX 初始化和 change 事件中調用

---

### Requirement: 防呆檢查應正常通過

**Requirement Statement**: 當 MaintenanceResult = 99、Other 有值且 ChangeItemName、ChangeItemNo 正確傳遞時，前端防呆應正常通過，表單成功提交。

**Given** MaintenanceResult = 99、Other = "馬達故障"、ChangeItemName = "馬達"、ChangeItemNo = "M123"  
**When** 用戶提交表單  
**Then** 所有欄位值正確傳遞到後端，表單成功提交

**驗收標準**：
- 不出現「尚未輸入原因」的 alert
- 表單成功提交到 Controller
- Controller 的 MaterialGunRepair(POST) 方法正確接收所有六個參數：sno、MaterialGun_Sno、Classification、MaintenanceResult、Other、ChangeItemName、ChangeItemNo、b_Chk
- 資料庫成功保存新值

---

## ADDED Requirements

### Requirement: 後端傳遞完整的編輯資料

**Requirement Statement**: 當編輯既存的維修記錄（白名單允許）時，後端應傳遞所有必要的欄位值，使前端能展示完整的舊資料供用戶修改。

**Given** 用戶訪問 /MaterialGun/MaterialGunRepair?sno=1783 編輯既存記錄  
**When** Controller GET 方法執行  
**Then** 後端應設定以下 ViewBag 供前端使用

#### Scenario: 傳遞 Other 欄位值
- **WHEN** 維修記錄中 MaintenanceResult = 99 且 Other = "馬達故障"
- **THEN** ViewBag.Other = "馬達故障"，前端可在 Other input 中顯示此值

#### Scenario: 傳遞 ChangeItemName 欄位值
- **WHEN** 維修記錄中 ChangeItemName = "馬達"
- **THEN** ViewBag.ChangeItemName = "馬達"，前端可在 ChangeItemName input 中顯示此值

#### Scenario: 傳遞 ChangeItemNo 欄位值
- **WHEN** 維修記錄中 ChangeItemNo = "M123"
- **THEN** ViewBag.ChangeItemNo = "M123"，前端可在 ChangeItemNo input 中顯示此值

#### Scenario: 新建記錄時欄位為空
- **WHEN** 建立新維修記錄（result = null）
- **THEN** ViewBag.Other、ViewBag.ChangeItemName、ViewBag.ChangeItemNo 均為 null 或空字串，前端 input 顯示空值

---

## Impact Summary

**後端變更**：
- MaterialGunController.cs 的 MaterialGunRepair(GET) 方法新增三個 ViewBag 賦值
- 改動範圍：第 433-437 行附近，共 3 行

**前端變更**：
- MaterialGunRepair.cshtml 的 Razor 初始化部分新增三個 JavaScript 變數初始化
- MaterialGunRepair.cshtml 的 HTML 部分新增三個 input 欄位的 value 綁定
- 改動範圍：初始化區塊 + 欄位定義區塊，共 6 行

**相容性**：完全向後相容，只是補齊遺漏的資料流
