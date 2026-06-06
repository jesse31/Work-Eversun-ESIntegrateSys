## 強化料槍維修存取控制 - Proposal

### Why

目前 `MaterialGunController.MaterialGunRepair(int sno)` 的 GET 方法缺乏防呆機制。使用者可直接手動輸入 URL (如 `?sno=1783`) 進入已完成維修或已報廢的料槍編輯頁面，違反白名單授權邏輯。防呆檢查只在 POST 提交時才進行，造成使用者體驗不佳（進入頁面後才被告知無權限）。

需要在 GET 階段即早擋下非授權存取，提供清晰的防呆提示，改善安全性與使用者體驗。

### What Changes

- 修改 `MaterialGunController.MaterialGunRepair(int sno)` GET 方法
- 加入三層檢查（順序優先）：
  1. **權限檢查**：若維修記錄已完成 (Chk=1)，檢查使用者是否在白名單內
     - 非白名單 → 擋下，提示「此料槍已完成維修，無權修改」，返回維修首頁
  2. **報廢檢查**：若料槍已報廢 (MaterialGunDiscard=true)
     - 已報廢 → 擋下，提示「此料槍已報廢，無法編輯」，返回維修首頁
  3. **正常流程**：所有檢查通過 → 回傳編輯頁面

### Capabilities

#### Modified Capabilities
- `materialgun-repair-access-control`: 強化 GET 方法的存取控制邏輯，在頁面載入時即進行權限與報廢檢查

### Impact

**修改檔案：**
- `ESIntegrateSys/Controllers/MaterialGunController.cs` - GET 方法邏輯加強

**受影響的使用者場景：**
- 直接輸入 URL 進入已完成維修頁面 → 現在會被擋下
- 直接輸入 URL 進入已報廢料槍編輯頁面 → 現在會被擋下
- 白名單使用者存取已完成維修記錄 → 行為不變，仍可進入

**相依性：**
- 無新增相依
- 現有白名單機制（MaterialGunConstants.RepairEditWhitelistUsers / RepairEditWhitelistDepts）

**API/功能變更：**
- GET 方法返回值不變（View 或 Redirect）
- 新增 TempData 訊息傳遞（["message"] 或 ["error"]）
