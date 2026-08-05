## Why

報價查詢頁面中，使用者勾選「報價中」checkbox 後，頁面執行整個重新整理導致 UI 清空。根本原因是昨日改動「初次進入頁面不自動查詢」，使得頁面重新載入時 Model 為空。本改動改採 AJAX 部分 DOM 更新，避免整個頁面刷新，提升使用者體驗並同時加入權限檢驗。

## What Changes

- **前端改動**：checkbox change 事件改成 AJAX 成功後直接更新 DOM（IE 負責人欄位、checkbox 狀態），而非 `window.location.reload()`
- **後端改動**：`HandleCheckboxChange()` 回傳 `ieonwer` 和 `iestatus` 以支援前端 DOM 更新；新增權限檢驗邏輯
- **新增權限檢驗**：取消勾選時檢查「只有原勾選者才能取消」，防止其他使用者誤操作
- **容錯機制**：AJAX 失敗時自動還原 checkbox 狀態，避免 UI 與後端不同步
- **UX 改進**：改用 SweetAlert2 替代 `alert()`，提供更友善的提示視窗（可自動關閉）

## Capabilities

### New Capabilities
- `quote-schedule-ie-status-management`: IE 報價狀態（「報價中」）的完整管理需求，包括勾選/取消、權限控制、容錯機制、即時 UI 反映

### Modified Capabilities
<!-- 功能邏輯本身不變，只是實現方式和安全性改進，故無既有 spec 需修改 -->

## Impact

- **修改檔案**：
  - `Controllers/QuoteScheduleController.cs` - `HandleCheckboxChange()` 方法
  - `Views/QuoteSchedule/_QuotePartialView.cshtml` - HTML 標記與 JavaScript 邏輯
- **影響使用者**：IE 部門在報價查詢頁面勾選/取消「報價中」的工作流
- **無 Breaking Changes**：API 回應格式擴充（增加欄位），但後向相容
