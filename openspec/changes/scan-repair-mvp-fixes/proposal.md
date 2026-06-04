# 掃碼維修 MVP 修復與補強

## Why

掃碼維修 MVP 上線後，發現兩個核心問題：

1. **Select2 套件加載失敗** - 下拉選單在頁面加載時無法初始化（TypeError: `$(...).select2 is not a function`），影響用戶無法選擇分類與原因
2. **已完修邏輯缺失** - 當料槍所有維修紀錄都已完修時，系統仍允許用戶掃碼進入維修表單，違反業務規則。應該提示用戶「此料槍已全部完修」並禁止維修操作

這兩個問題堵住了 MVP 的實際使用路徑。需要立即修復以確保可用性。

## What Changes

- **修復 Select2 非同步加載** - 重構 ScanRepair.cshtml 的 mounted() 和 loadDropdownOptions() 邏輯，確保在 Select2 CDN 完全加載後再初始化插件
- **新增已完修狀態檢查** - 在 GetGunByBarcode() API 中添加邏輯，區分「有待維修紀錄」與「已全部完修」兩種狀態，並向前端返回不同的訊號
- **前端已完修提示** - ScanRepair.cshtml 接收「已完修」訊號時，彈窗警告用戶，清空表單，禁止進入維修編輯流程

## Capabilities

### New Capabilities
- `select2-async-initialization-fix`: 修復 Select2 在 Vue 3 環境中的非同步加載問題，確保下拉選單正確初始化
- `material-gun-completion-check`: 新增已完修料槍的檢查與防呆流程，提示用戶並禁止進入維修表單

### Modified Capabilities
- `scan-repair-barcode-mvp`: 修改現有掃碼維修 MVP 功能，增加已完修狀態判斷邏輯（GetGunByBarcode API 回應擴展）

## Impact

**修改檔案**:
- `ESIntegrateSys/Views/MaterialGun/ScanRepair.cshtml` - 重構 Vue mounted() 和 Select2 初始化邏輯
- `ESIntegrateSys/Controllers/MaterialGunController.cs` - GetGunByBarcode() 擴展回應結構，新增已完修判斷

**影響範圍**:
- 前端使用者體驗 - 下拉選單恢復可用，已完修流程增加防呆提示
- 後端 API - GetGunByBarcode() 回應新增狀態欄位（向後相容）
- 測試範圍 - 異常場景測試補強（已完修情況）

**無破壞性改變** - 既有邏輯無修改，僅補強缺失部分
