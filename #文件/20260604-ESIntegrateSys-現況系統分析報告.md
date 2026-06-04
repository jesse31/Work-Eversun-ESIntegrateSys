# 昶亨整合式系統 (ESIntegrateSys) — 現況系統分析報告

**報告日期**：2026-06-04
**分析者**：系統分析師（AI 輔助）
**版本**：v1.0

---

## 1. 系統背景與定位

**昶亨整合式系統 (ES Integrate System，簡稱 ESIntegrateSys)** 是一套由昶亨公司自行開發與維護的企業內部整合管理系統，屬於 **MES（Manufacturing Execution System）延伸型** 後台管理平台。

| 項目 | 說明 |
|------|------|
| 系統名稱 | 昶亨整合式系統 (ESIntegrateSys) |
| 當前版本 | 20241114 |
| 開發模式 | 單人開發 |
| 定位 | 企業內部工廠級整合管理後台（人力、設備、報價、工時） |
| 部署環境 | IIS + Windows Server |
| 目標使用者 | 工廠管理人員、IE、業務、設備維護人員 |

---

## 2. 技術棧

### 後端技術

| 技術層面 | 技術框架 | 版本 |
|---------|---------|------|
| Web 框架 | ASP.NET MVC | 5.2.7 |
| 執行平台 | .NET Framework | 4.5（編譯目標 4.8） |
| ORM（複雜查詢） | ADO.NET (SqlDataAdapter / SqlCommand) | 原生 |
| ORM（CRUD） | Entity Framework Database First | 6.x |
| 依賴注入 | Autofac | 4.9.0 |
| 後台任務 | Hangfire | 1.7.37 |
| Excel 操作 | NPOI / EPPlus | 最新版本 |
| 身份驗證 | Forms Authentication + Session | ASP.NET Identity 2.2.3 |
| 日誌 | 應用程式 Logs 目錄（未使用 Serilog） | — |

### 前端技術

| 技術層面 | 技術框架 | 版本 |
|---------|---------|------|
| CSS 框架 | Bootstrap | 3.4.1 |
| JavaScript 庫 | jQuery | 3.4.1 |
| 模板引擎 | Razor (.cshtml) | — |
| 前端框架 | 無 Vue.js（部分頁面可能有 CDN 引用） | — |

### 資料庫

| 環境 | 伺服器 | 資料庫 | 帳號 |
|------|--------|--------|------|
| 開發測試 | 192.168.4.31 | ESIntegrateSys | eisTest |
| 正式環境 | 192.168.4.70 | ESIntegrateSys | ESI |
| AMES 1.0（唯讀參考） | 192.168.4.200 | AMES_DB | fa |

### 部署架構

- **Web 伺服器**：IIS 7.5+（SSL Port: 44301）
- **Session 存放**：InProc（記憶體內）
- **Session 逾時**：240 分鐘
- **SMTP**：192.168.4.3:25（內部郵件伺服器）

---

## 3. 架構概述

### 模組劃分

```
ESIntegrateSys.sln
└── ESIntegrateSys/                  # 主專案
    ├── App_Start/                   # 啟動設定（DI、路由、Bundle、Filter）
    ├── Controllers/                 # 8 個 MVC Controller
    ├── Models/                      # EF Database First 實體（EDMX）+ ADO.NET 工具類
    ├── Models_MGun/                 # 料槍業務邏輯模型（非 EF Entity）
    ├── Models_ManpowerAllocation/   # 人力配置業務邏輯模型
    ├── Models_QSchedule/            # 報價排程業務邏輯模型
    ├── Services/                    # 業務邏輯服務層（含子資料夾）
    ├── ViewModels/                  # ViewModel / DTO
    ├── Views/                       # Razor 視圖
    ├── Filters/                     # 自訂授權 Filter
    ├── Utilities/                   # 工具類
    └── Content/ + Scripts/          # 靜態資源
```

### 前後端關係

```
使用者瀏覽器
    │  HTTP Request
    ▼
IIS (ASP.NET MVC 5)
    │
    ├── Controller（路由分發）
    │       │
    │       ├── Service（業務邏輯）
    │       │       │
    │       │       ├── EF（CRUD 操作）→ SQL Server
    │       │       └── ADO.NET / Db.cs（複雜查詢）→ SQL Server
    │       │
    │       └── Models_*/（業務計算、Excel 匯出）
    │
    └── Razor View（HTML 輸出）
            │
            └── Bootstrap + jQuery（前端互動）
```

---

## 4. 現有功能清單

### 4.1 模組與主要頁面

| 模組 | Controller | 主要頁面 / 功能 |
|------|-----------|----------------|
| **首頁 / 登入** | HomeController | Login（登入）、Index（儀表板）、Logout（登出） |
| **人力配置管理** | ManpowerAllocationController | 人力資源配置、工作分配、配置歷史記錄、授權管理 |
| **人力授權管理** | ManpowerAuthorizationController | 員工權限管理、部門授權、角色控制 |
| **物料槍管理** | MaterialGunController | Index（料槍清單）、MaterialGunCreate（新增）、MaterialGunInfoView（詳情）、MaterialGunRepair（維修申報）、MaterialGunMaintain（保養）、MaterialGunRepairView（維修紀錄查詢）、MaterialGunForRepair（送修管理）、MaterialAbout（料槍相關資料）、CreateMaintainWork（建立保養工單）、Discard（報廢） |
| **報價單排程** | QuoteScheduleController | 報價計劃管理、排程調度、進度追蹤、Excel 上傳 |
| **工時單管理** | WoTimeSheetController | 工時記錄、工作單管理、時間追蹤 |
| **郵件系統** | EmailController | 系統通知郵件、批量發送 |
| **系統維護** | MaintainController | 系統設定、會員管理、角色管理、日誌查詢 |

### 4.2 料槍模組詳細功能（重點模組）

| 頁面 | 功能說明 |
|------|---------|
| Index | 料槍清單查詢（依條件篩選） |
| MaterialGunCreate | 新增料槍基本資料 |
| MaterialGunInfoView | 料槍詳細資訊查看 |
| MaterialGunRepair | 新增維修申報單 |
| MaterialGunRepairView | 維修記錄查詢 |
| MaterialGunForRepair | 送修/領回管理 |
| MaterialGunMaintain | 保養記錄作業 |
| CreateMaintainWork | 建立保養工單 |
| Discard | 料槍報廢作業 |
| MaterialAbout | 料槍相關參考資料 |

---

## 5. 資料庫概覽

### 5.1 主要資料表與關聯

#### 會員與權限相關

| 資料表 | 中文名稱 | 主鍵 | 關鍵欄位 |
|--------|---------|------|---------|
| ES_Member | 系統使用者 | fId (int) | fUserId、fPwd、fName、fStatus、Dept_No、email |
| ES_MemberRole | 使用者角色對應 | sno (int) | USER_ID (FK→ES_Member)、ROLE_ID (FK→ES_RoleClassification)、EXPIRED_DATE |
| ES_MemberFunction | 使用者功能授權 | sno (int) | UserNo_sno (FK→ES_Member)、FunctionNo (FK→ES_FunctionItem) |
| ES_RoleClassification | 角色定義 | sno (int) | ROLE_ID、ROLE_NAME、ROLE_DESC、CREATE_USERID |
| ES_FunctionItem | 功能項目清單 | sno (int) | FunctionNo、FunctionName、ActionName |

#### 料槍管理相關

| 資料表 | 中文名稱 | 主鍵 | 關鍵欄位 |
|--------|---------|------|---------|
| ES_MaterialGunInfo | 料槍基本資訊 | sno (int) | MaterialGun_Eno（設備編號）、MaterialGun_Sno（序號）、MaterialGun_Trade（廠牌）、MaterialGun_Size（規格）、MaintainCycle、MaintainNexDate、MaterialGunDiscard、DiscardDate |
| ES_MaterialGunRepair | 料槍維修記錄 | sno (int) | MaterialGun_Eno、MaterialGun_Sno、RepairDate、RepairUserId、BadDescription、MaintenanceTime、MaintenanceUserId、MaintenanceResult、Classification、ChangeItemName、ChangeItemNo、Chk |
| ES_MaterialGunBadDesc | 故障描述代碼 | — | KeyWorld、BadDescription |
| ES_MaterialGunDiscardDesc | 報廢原因代碼 | — | KeyWorld、DiscardDescription |
| ES_MaterialGunMResult | 維護結果代碼 | sno (int) | KeyWorld、MaintenanceResult、Date、CreateUserId |
| ES_MaterialGunRepairClass | 維修分類代碼 | — | KeyWorld、RepairClass |
| ES_MaterialGunSize | 料槍規格代碼 | — | KeyWorld、GunSize |
| ES_MaintainWork | 保養工單 | — | 料槍保養工作單記錄 |
| ES_MaterialCorrection | 料槍校正記錄 | — | 校正相關欄位 |

#### 人力配置相關

| 資料表 | 中文名稱 | 主鍵 | 關鍵欄位 |
|--------|---------|------|---------|
| EmployeeAssignments | 員工人力配置 | assignment_id (int) | employee_id、reporting_unit_ID、班別GUID、出勤類型GUID、生產單位名稱GUID、current_editor（悲觀鎖定）、edit_lock_time |
| EmployeeAssignments_Log | 人力配置異動日誌 | — | 操作記錄 |
| HResources_人員 | 人員基礎資料 | — | 員工基本資料（來源 AMES/HR） |
| HResources_出勤類型 | 出勤類型代碼 | — | — |
| HResources_班別 | 班別代碼 | — | — |
| HResources_生產單位名稱 | 生產單位名稱 | — | — |
| HResources_報工生產單位 | 報工生產單位 | — | — |

#### 報價排程相關

| 資料表 | 中文名稱 | 主鍵 | 關鍵欄位 |
|--------|---------|------|---------|
| ES_QuoteForSales | 業務報價單 | sno (int) | CustNo、SalesNo、EngSr、CustMaterial、WoNoAttri、RequDate、SalesId、Chk、CancelChk |
| ES_QuoteForIE | IE 報價單 | sno (int) | IE 相關欄位 |
| ES_QuoteCust | 客戶資料 | — | 客戶清單 |
| ES_QuoteWoNoAttri | 工單屬性 | — | 工單屬性代碼 |
| ES_QuoteUploadRecords | 報價文件上傳記錄 | — | 文件上傳紀錄 |
| ES_QuoteUploadFiles | 報價文件檔案 | — | 實體檔案存放路徑 |
| ES_DeptInfo | 部門資訊 | — | 部門資料 |
| ES_ProductionUnit | 生產單位 | — | 生產單位資料 |
| ES_ReportingUnit | 報工單位 | — | — |

### 5.2 資料存取模式

系統同時使用兩種資料存取方式：

1. **Entity Framework 6 Database First（EDMX）**：用於 CRUD 操作，自動產生 Entity 類別
2. **ADO.NET（Db.cs 工具類）**：用於複雜查詢，手動撰寫參數化 SQL，回傳 DataTable / DataSet

---

## 6. 權限與角色設計

### 6.1 架構說明

本系統採用**雙層權限控管**機制：

| 層次 | 機制 | 說明 |
|------|------|------|
| 第一層 | Session 登入驗證 | 所有頁面均需登入（Forms Authentication） |
| 第二層 | DepartmentAuthorizationAttribute | 部分頁面限定「特定部門代碼」或「特定使用者ID」才可存取 |

### 6.2 角色設計

| 角色要素 | 設計方式 |
|---------|---------|
| 角色定義 | ES_RoleClassification（ROLE_ID、ROLE_NAME） |
| 使用者角色指派 | ES_MemberRole（USER_ID + ROLE_ID，含 EXPIRED_DATE） |
| 功能授權 | ES_MemberFunction（UserNo_sno + FunctionNo） |
| 功能項目 | ES_FunctionItem（FunctionNo、FunctionName、ActionName） |
| 部門授權 | DepartmentAuthorizationAttribute（Controller/Action 層級 Attribute） |

### 6.3 授權流程

```
使用者登入
    │
    ▼
Session["Member"] = MemberViewModels
    │（含 fUserId、fName、ROLE_ID、UDeptNo）
    │
    ▼
Controller/Action 帶有 [DepartmentAuthorization] 屬性
    │
    ├── 驗證 Session["Member"] 存在
    ├── 比對 _allowedUserIds（指定工號白名單）
    ├── 比對「授權者工號清單」（ManpowerAllocationServices）
    └── 比對 _allowedDepartments（部門代碼）
```

### 6.4 現有問題

- 部分頁面授權採用**硬編碼部門代碼**方式（Attribute 參數），缺乏動態設定能力
- 無細粒度的**功能型 RBAC**（`ES_MemberFunction` 雖有設計，但 Controller 層尚未統一套用）
- 無前端層級的功能按鈕顯示/隱藏控制

---

## 7. 現有使用者操作流程

### 7.1 登入流程

```
1. 使用者開啟 ~/Home/Login
2. 輸入帳號（fUserId）/ 密碼（fPwd）
3. [POST] HomeController.Login
4. ES_MemberLogin.MemberLogin()
   └── EF Join ES_Member + ES_MemberRole
   └── 比對 fStatus = true
5. 登入成功 → Session["Member"]、Session["WelCome"] 設定
6. 重導向 ~/Home/Index
```

### 7.2 首頁儀表板

```
1. HomeController.Index
2. homeService.GetLayoutInfo(member) → 取得使用者可見功能選單
3. Session["Admin"] = ROLE_ID；Session["Layout"] = 第一個畫面資訊
4. 渲染 _LayoutIndex 側邊欄 + 功能清單
```

### 7.3 料槍維修典型操作流程

```
1. 操作員開啟 MaterialGun/Index（料槍清單）
2. 搜尋條件篩選 → 找到目標料槍
3. 點擊「維修申報」→ MaterialGun/MaterialGunRepair（手動填寫故障描述、維修類別）
4. 維修技術員 → MaterialGun/MaterialGunForRepair（領取 / 送修確認）
5. 維修完成 → 更新維修結果（MaintenanceResult、MaintenanceTime）
6. 確認 Chk = true（完工確認）
```

---

## 8. 現有痛點與問題

### 8.1 使用體驗問題

| 問題 | 描述 | 優先級 |
|------|------|-------|
| 料槍維修輸入繁瑣 | 維修申報需手動輸入料槍編號，容易出錯，未支援條碼掃描 | 高 |
| 無行動裝置最佳化 | Bootstrap 3.x 響應式設計有限，工廠現場平板操作體驗差 | 中 |
| 查詢效率低 | 複雜查詢仍使用 DataTable 傳遞，無分頁/排序最佳化 | 中 |
| 缺乏即時通知 | 維修狀態變更無主動推播，需人工確認 | 低 |

### 8.2 安全性問題

| 問題 | 描述 | 優先級 |
|------|------|-------|
| 密碼明文儲存 | ES_Member.fPwd 直接比對，未使用雜湊 | 高 |
| Session InProc | 伺服器重啟即失效，單機限制，無法水平擴展 | 中 |
| 授權方式不一致 | 部分 Action 使用 DepartmentAuthorization，部分直接 Session 判斷 | 中 |

### 8.3 維護性問題

| 問題 | 描述 | 優先級 |
|------|------|-------|
| EDMX Database First | EF EDMX 模型更新需手動同步，維護成本高 | 高 |
| 非同步未落實 | 大多數 Controller/Service 為同步呼叫，I/O 阻塞風險 | 中 |
| 無統一日誌框架 | 未使用 Serilog，錯誤追蹤能力弱 | 中 |
| 模型命名混亂 | Models、Models_MGun、Models_ManpowerAllocation 各自分散，命名規範不統一 | 低 |
| Hangfire 任務過少 | 僅用於日誌清理，背景任務潛力未充分利用 | 低 |

### 8.4 技術債

| 技術債項目 | 說明 |
|-----------|------|
| .NET Framework 4.5 | 已是舊版，無法享受 .NET 6+ 效能與新語法優勢 |
| Bootstrap 3.4 | 已停止維護，無現代 CSS 特性支援 |
| Entity Framework 6 | Database First + EDMX，現代開發應改用 Code First / EF Core |
| 無前端框架 | jQuery 為主，缺乏元件化與響應式資料綁定能力 |

---

## 9. 既有系統的限制與假設

| 限制 / 假設 | 說明 |
|------------|------|
| 單人開發 | 維護與新功能開發均由單一開發者負責 |
| 時間資源有限 | 以 MVP 策略快速交付為主，每次迭代聚焦核心功能 |
| 資料庫共用 | 部分資料（人員、班別等）來源於 AMES 1.0（Oracle/MSSQL） |
| 無 CI/CD | 目前為手動發布（Visual Studio Publish） |
| 無自動化測試 | 無單元測試 / 整合測試 |
| 功能逐步轉型 | 現有頁面以 Bootstrap + jQuery 為主，新頁面可逐步引入 Vue.js 3 CDN |

---

## 10. 分析後建議優先處理的 3–5 個關鍵問題

### 問題 1（最高優先）：料槍維修條碼掃描輸入

**現況**：維修申報時需手動輸入料槍設備編號，容易輸入錯誤，現場作業效率低。
**建議**：新增條碼掃描輸入支援（`<input>` 配合 USB/藍牙條碼槍自動輸入），同步驗證料槍存在性，降低人工錯誤。
**影響範圍**：`MaterialGunController.MaterialGunRepair`、對應 View

---

### 問題 2（高優先）：密碼安全性升級

**現況**：`ES_Member.fPwd` 以明文比對，存在重大安全風險。
**建議**：採用 BCrypt 或 SHA-256+Salt 雜湊存儲，並補充密碼長度規則。
**影響範圍**：`ES_MemberLogin`、`MaintainController`（帳號管理）

---

### 問題 3（高優先）：統一授權機制

**現況**：授權方式混用（Session 判斷 + DepartmentAuthorizationAttribute），難以集中管控。
**建議**：統一以 `DepartmentAuthorizationAttribute` 或改為 `ES_MemberFunction` + `ES_MemberRole` 驅動的細粒度 RBAC。建立「功能白名單」查詢，讓前端側邊欄動態顯示/隱藏功能按鈕。
**影響範圍**：所有 Controller、`HomeService.GetLayoutInfo`

---

### 問題 4（中優先）：非同步化與效能優化

**現況**：Controller / Service 均為同步呼叫，大量 DataTable 操作，高併發時有阻塞風險。
**建議**：逐步改用 `async/await`（從 Controller 到 Repository），並引入 Dapper 取代 ADO.NET DataTable 操作，提升查詢效率。
**影響範圍**：所有 Controller、Db.cs、Service 層

---

### 問題 5（中優先）：引入 Serilog 統一日誌

**現況**：錯誤處理採用 `try-catch` 後回傳 null，無結構化日誌，上線問題難以追蹤。
**建議**：引入 Serilog（File Sink），統一記錄操作記錄與例外 Stack Trace，並整合至 `MaintainController` 提供日誌查詢介面。
**影響範圍**：全系統 Service 層、Global.asax.cs

---

## 附錄：待確認資訊

以下資訊在本次分析中尚未完整取得，建議後續補充：

1. **完整的 SQL Server Schema**：各表的完整欄位定義（含 FK 約束、Index）
2. **人力配置授權表**（Models/人力配置授權表.cs）的業務邏輯說明
3. **Hangfire 現有 Job 清單**：目前排程任務清單及執行頻率
4. **部署架構圖**：IIS 伺服器數量、負載平衡、備援機制
5. **使用者人數與角色分布**：現有帳號數量、各角色人數比例
6. **AMES 1.0 整合範圍**：哪些資料從 AMES 同步，同步頻率為何
