# 昶亨整合式系統 (ES Integrate System)

## 項目簡介

**昶亨整合式系統** 是一個基於 ASP.NET MVC 框架建構的企業級整合管理系統，提供人力資源配置、授權管理、物料管理、報價排程等綜合功能。系統採用 Entity Framework 進行數據持久化，支援後台任務調度和郵件通知。

---

## 主要功能模塊

### 1. **人力配置管理** (Manpower Allocation)
   - 員工人力資源配置
   - 工作分配與追蹤
   - 配置歷史記錄

### 2. **人力授權管理** (Manpower Authorization)
   - 員工權限管理
   - 部門級別授權
   - 角色控制

### 3. **物料槍管理** (Material Gun)
   - 物料設備管理
   - 庫存追蹤
   - 使用記錄

### 4. **報價單排程** (Quote Schedule)
   - 報價單計劃管理
   - 排程調度
   - 進度追蹤

### 5. **工時單管理** (WoTimeSheet)
   - 工時記錄
   - 工作單管理
   - 時間追蹤

### 6. **郵件系統** (Email)
   - 系統郵件通知
   - 批量郵件發送
   - 郵件配置管理

### 7. **系統維護** (Maintain)
   - 系統配置管理
   - 日誌管理
   - 數據維護

---

## 技術棧

| 技術層面 | 技術框架 | 版本 |
|---------|--------|------|
| **Web 框架** | ASP.NET MVC | 5.2.7 |
| **運行平台** | .NET Framework | 4.5 |
| **ORM 框架** | Entity Framework | 6.x |
| **依賴注入** | Autofac | 4.9.0 |
| **後台任務** | Hangfire | 1.7.37 |
| **前端框架** | Bootstrap | 3.4.1 |
| **JavaScript 庫** | jQuery | 3.4.1 |
| **Excel 操作** | NPOI / EPPlus | 最新版本 |
| **數據庫** | SQL Server | 配置見 Web.config |
| **身份驗證** | ASP.NET Identity | 2.2.3 |

---

## 系統要求

### 硬體要求
- **CPU**: Intel Core i5 或以上
- **記憶體**: 最少 4GB RAM（建議 8GB）
- **硬碟**: 最少 2GB 可用空間

### 軟體要求
- **操作系統**: Windows Server 2012 R2 以上 / Windows 7 以上
- **IIS**: 7.5 以上版本
- **.NET Framework**: 4.5 或以上
- **SQL Server**: 2012 以上版本
- **Visual Studio**: 2017 以上（開發環境）

---

## 安裝與配置

### 1. 開發環境設置

```bash
# 克隆項目
git clone <repository-url>

# 還原 NuGet 套件
nuget restore ESIntegrateSys.sln
```

### 2. 數據庫配置

編輯 `Web.config` 文件，配置資料庫連線字串：

```xml
<connectionStrings>
  <add name="ESIntegrateSys" connectionString="Server=YOUR_SERVER;Database=ESIntegrateSys;User Id=YOUR_USER;Password=YOUR_PASSWORD;" providerName="System.Data.SqlClient" />
</connectionStrings>
```

### 3. SMTP 郵件配置

在 `Web.config` 的 `appSettings` 中配置 SMTP 伺服器：

```xml
<add key="SmtpServer" value="192.168.4.3" />
<add key="SmtpPort" value="25" />
<add key="SmtpUser" value="ESIntegrate@eversun.com.tw" />
<add key="SmtpPass" value="[YOUR_PASSWORD]" />
```

### 4. Hangfire 配置

系統使用 Hangfire 進行後台任務調度，自動刪除三個月前的日誌記錄。

### 5. 構建與部署

```bash
# Visual Studio 中開啟解決方案
# 構建解決方案 (Ctrl + Shift + B)
# 選擇發佈功能進行部署
```

---

## 項目結構

```
ESIntegrateSys/
├── App_Start/              # 應用程式啟動配置
│   ├── BundleConfig.cs     # 捆綁包配置
│   ├── DependencyConfig.cs # 依賴性注入配置
│   ├── FilterConfig.cs     # 全域篩選器
│   └── RouteConfig.cs      # 路由配置
├── Controllers/            # MVC 控制器
│   ├── HomeController.cs
│   ├── ManpowerAllocationController.cs
│   ├── ManpowerAuthorizationController.cs
│   ├── MaterialGunController.cs
│   ├── QuoteScheduleController.cs
│   ├── WoTimeSheetController.cs
│   ├── EmailController.cs
│   └── MaintainController.cs
├── Models/                 # 數據模型
│   ├── Db.cs             # EF Core 模型
│   └── 人力配置授權表.cs  # 特定業務模型
├── Models_*/               # 按模塊分類的模型
├── Services/               # 業務邏輯層
├── ViewModels/             # 視圖模型
├── Views/                  # MVC 視圖
├── Content/                # CSS 和靜態資源
├── Scripts/                # JavaScript 文件
├── Logs/                   # 應用程式日誌
├── UploadedFiles/          # 上載文件目錄
├── Web.config              # 應用程式配置
└── Global.asax             # 全域應用程式類別
```

---

## 主要特性

✅ **企業級架構** - 分層設計，符合 SOLID 原則  
✅ **依賴性注入** - 使用 Autofac 進行容器管理  
✅ **後台任務** - Hangfire 支援定時任務和歷史日誌清理  
✅ **數據持久化** - Entity Framework ORM 簡化數據庫操作  
✅ **身份驗證** - 表單驗證，Session 超時 240 分鐘  
✅ **響應式設計** - Bootstrap 前端框架  
✅ **文件導入導出** - NPOI 和 EPPlus Excel 支援  
✅ **多語言支援** - 繁體中文及其他語言資源  

---

## 版本信息

- **當前版本**: 20241114
- **開發語言**: C#
- **上次更新**: 2024年11月14日

---

## 配置參數

### 會話配置
- **Session 超時**: 240 分鐘（登入後無操作自動登出）
- **認證模式**: Forms 驗證
- **登入 URL**: ~/Home/Login

### IIS Express 配置
- **SSL 連接埠**: 44301
- **http://localhost:44301**

### 文件上傳限制
- **最大請求長度**: 10 MB
- **執行超時**: 600 秒

---

## 開發者指南

### 新增功能模塊

1. 在 `Controllers` 目錄下建立新的控制器類別
2. 在 `Models` 中定義相關數據模型
3. 在 `Services` 中實現業務邏輯
4. 在 `Views` 中建立視圖文件
5. 在 Route 中註冊新的路由

### 日誌査詢

應用程式日誌存儲在 `Logs` 目錄下，可用於調試和生產環境監控。

---

## 聯繫與支援

如有問題或建議，請聯繫開發團隊。

---

*此項目由昶亨公司維護和開發。*
