## Why

在先前的掃碼維修功能規劃中，ScanRepair.cshtml 改名為 MaterialGunScanRepair.cshtml 以統一視圖命名規範，但此改名只在代碼層面完成（檔案、控制器、連結），文件和規格書中的舊名稱參考仍未更新。需要完整清理所有殘留的舊命名參考，確保文檔與實現保持同步。

## What Changes

- 更新規格書文件中的所有 `ScanRepair.cshtml` 參考改為 `MaterialGunScanRepair.cshtml`
- 更新規格書文件中的路由參考 `/MaterialGun/ScanRepair` 改為 `/MaterialGun/MaterialGunScanRepair`
- 驗證代碼層面（控制器、View、連結）中的改名已完成
- 清理文檔中遺留的舊命名痕跡

## Capabilities

### New Capabilities
- `view-naming-consistency`: 統一 MaterialGun 相關視圖的命名規範，確保檔案名、控制器呼叫、路由和文檔參考全部一致

### Modified Capabilities
- 無（這是文檔/命名清理工作，不影響功能需求）

## Impact

**受影響的文件：**
- `#文件/20260604-料槍維修條碼化-MVP規格書.html` — 更新過時的視圖名稱參考
- `#文件/20260604-料槍維修條碼化-MVP階段3-規格書.md` — 更新過時的視圖名稱參考
- `#文件/20260604-料槍維修條碼化-MVP階段2-方案規劃與技術債評估.md` — 更新過時的視圖名稱參考

**代碼層面（已完成）：**
- `ESIntegrateSys/Controllers/MaterialGunController.cs` — 已改名
- `ESIntegrateSys/Views/MaterialGun/MaterialGunRepairView.cshtml` — 已改名
- `ESIntegrateSys/Views/MaterialGun/MaterialGunScanRepair.cshtml` — 檔案已改名

**用戶體驗：** 無影響（純文檔和命名清理）
