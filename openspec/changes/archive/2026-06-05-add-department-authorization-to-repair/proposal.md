# 料槍維修部門授權 - Proposal

## Why

現有白名單機制只支援帳號級別的授權（如 02898），無法滿足組織內跨部門的授權需求。IT 部門作為一個整體，其所有維修人員應該能夠編輯已完成維修記錄進行複查或更新，因此需要加入**部門級別的授權層**，使授權機制更符合現代組織結構。

## What Changes

- **白名單常數擴展** - 在 `MaterialGunConstants.cs` 中新增 `RepairEditWhitelistDepts` HashSet，包含授權部門代碼（如 "IT"）
- **授權邏輯改進** - 修改 `IsRepairWhitelisted()` 方法支援 OR 組合邏輯：帳號在白名單 **或** 部門在授權清單中，即允許編輯已完成維修記錄
- **稽審日誌增強** - Serilog 日誌新增記錄授權原因（"Account whitelist" 或 "IT department"）與部門代碼（UDeptNo），以便追蹤授權來源
- **後端驗證強化** - Controller 層的驗證方法改為同時檢查使用者帳號與部門代碼，確保授權決策準確

## Capabilities

### New Capabilities
- `department-authorization-for-repair`: IT 部門（或其他授權部門）的員工可編輯已完成維修記錄，同時保留帳號白名單機制

### Modified Capabilities
- `material-gun-repair-whitelist`: 現有白名單機制擴展為支援部門授權，使用 OR 邏輯判定是否允許編輯已完成維修

## Impact

**修改檔案：**
- `ESIntegrateSys/Helpers/MaterialGunConstants.cs` - 新增部門白名單常數
- `ESIntegrateSys/Controllers/MaterialGunController.cs` - 擴展授權驗證方法、修改 MaterialGunRepairView() 與 MaterialGunRepair() 方法
- `openspec/changes/add-material-gun-repair-whitelist/tasks.md` - 無須修改（前次 change 已完成）

**前端無需改動：**
- 前端 View 與 JavaScript 邏輯保持不變，ViewBag.IsWhitelisted 仍為簡單布林值

**系統架構：**
- 後端驗證邏輯演進：從單層帳號檢查 → 多層（帳號 + 部門）OR 組合
- 稽審追蹤能力提升：新增「授權原因」與「部門代碼」欄位
- 為未來擴展預留空間：未來可輕鬆增加其他部門或加入 AND 邏輯的細粒度授權

**安全性與合規：**
- 保持「深度防禦」原則：後端驗證仍為主要資安防線
- 稽審日誌完整性提升：便於事後追蹤與合規檢查
- 無破壞性更新（Non-breaking）：現有白名單帳號授權機制保持不變
