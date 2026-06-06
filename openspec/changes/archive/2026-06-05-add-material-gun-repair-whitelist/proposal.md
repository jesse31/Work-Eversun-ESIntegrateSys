# 料槍維修白名單權限控制 - Proposal

## Why

料槍維修系統目前缺乏細粒度的權限控制。當料槍維修完成（Chk = 1）後，系統應禁止一般人員編輯已完成的維修紀錄，但需要允許特定的白名單人員（例如檢查員、管理者）進行重新編輯或複查。同時，所有操作都應被記錄在案，以滿足稽審追蹤需求。

## What Changes

- **後端驗證邏輯**：在 `MaterialGunController` 中新增白名單驗證方法 `IsRepairWhitelisted(userId)`，檢查使用者帳號是否在允許清單中
- **前端隱藏控制**：修改 `MaterialGunRepairView.cshtml`，針對已完成維修（Chk = 1）的料槍行，只有白名單人員才顯示「料槍維修」按鈕
- **防呆機制強化**：在按鈕點擊時及表單提交時（GET/POST）皆驗證白名單，防止非授權人員透過 URL 直接存取
- **稽審日誌**：使用 Serilog 記錄所有維修相關操作（訪問、編輯成功、編輯失敗）及失敗原因
- **錯誤訊息**：非白名單人員若被阻擋，顯示 SweetAlert2 warning 訊息：「此料槍已完成維修，無法修改。無權進行此操作。」

## Capabilities

### New Capabilities

- `material-gun-repair-whitelist-control`: 料槍維修白名單權限控制機制，區分未維修（Chk=0）與已完成維修（Chk=1）的行為，限制只有白名單帳號才能編輯已完成的維修紀錄
- `material-gun-repair-audit-log`: 料槍維修操作稽核日誌，記錄所有訪問、編輯成功、編輯失敗的事件及人員資訊

### Modified Capabilities

- `material-gun-repair-view`: 修改「料槍維修主頁」的列表顯示邏輯，根據使用者白名單狀態動態控制「料槍維修」按鈕的可見性

## Impact

**受影響的程式碼：**
- `ESIntegrateSys/Controllers/MaterialGunController.cs` - 新增白名單驗證方法、修改 GET/POST 方法邏輯
- `ESIntegrateSys/Views/MaterialGun/MaterialGunRepairView.cshtml` - 修改條件式按鈕顯示、強化 JavaScript 防呆
- `ESIntegrateSys/Helpers/MaterialGunConstants.cs` (新建) - 定義白名單常數（暫時方案，未來整合至角色權限系統)

**受影響的系統元件：**
- 前端：jQuery/JavaScript 事件處理、SweetAlert2 提示
- 後端：Controller 層驗證邏輯、Session 會員資訊取得
- 日誌系統：Serilog 稽審日誌紀錄

**依賴性：**
- 現有 Serilog 日誌框架
- 現有 SweetAlert2 前端套件
- Session["Member"] 登入機制

**未來考量：**
本方案為暫時實作（白名單使用程式碼常數），未來將整合至計畫中的完整角色權限系統，取代硬編碼的白名單清單。
