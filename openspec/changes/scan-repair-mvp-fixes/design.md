# 掃碼維修 MVP 修復與補強 - 技術設計

## Context

掃碼維修 MVP 已上線並在實際使用中。當前遇到兩個阻斷性問題：

**問題 1：Select2 非同步加載失敗**
- 現狀：ScanRepair.cshtml 中 Vue mounted() 觸發 AJAX 加載下拉選項，success callback 嘗試初始化 Select2
- 症狀：瀏覽器控制台報錯 `TypeError: $(...).select2 is not a function`，下拉選單不可用
- 根因：非同步 AJAX 與 Select2 CDN 加載的競態條件；可能 Select2.js 尚未完成初始化

**問題 2：已完修邏輯缺失**
- 現狀：GetGunByBarcode() 只檢查「是否存在 Chk=True 的待修紀錄」，無法判斷「是否 ALL 已完修」
- 症狀：掃到已全部完修的料槍時，API 返回「無待維修紀錄」通用訊息，前端無法區分；業務規則要求「已完修應提示警告並禁止維修」
- 語義：Chk=True（未完修） vs Chk=False（已完修）

**架構現狀**：
- 前端：Vue 3 CDN + jQuery 3.4.1 + Select2 4.0.13（CDN）
- 後端：.NET Framework 4.5 MVC，Entity Framework 6
- 資料庫：MSSQL，表 ES_MaterialGunInfo, ES_MaterialGunRepair

## Goals / Non-Goals

**Goals:**
1. 修復 Select2 初始化失敗，確保下拉選單在頁面加載後正常工作
2. 新增已完修狀態檢查，提示用戶並禁止進入維修表單
3. 確保修復不引入新的技術債或破壞既有功能
4. 兩項修復都在 MVP 範圍內，無需大規模重構

**Non-Goals:**
- 升級至 Vue 3 reactive forms（超出 MVP 範圍）
- 重構 jQuery 與 Vue 的混用（架構性改革，留待 v1.1）
- 新增獨立條碼欄位（已規劃為 v1.1）
- 修改資料庫 Schema（無需，只補業務邏輯）

## Decisions

### D1: Select2 初始化保障策略

**決策**：在 mounted() 中使用 Promise.all() 等待 jQuery 和 Select2 完全加載，再執行 loadDropdownOptions()

**理由**：
- 直接方案：改用 document.readyState 或 MutationObserver 觀察 Select2 全域物件是否就緒
- 替代方案 A：將 Select2 初始化延遲至 `$nextTick(() => { $nextTick(() => { ... })})` (嵌套 tick，不優雅)
- 替代方案 B：改用原生 setTimeout 延遲 (不可靠，取決於瀏覽器性能)
- **選擇方案**：在 mounted() 一開始就檢查 window.$ 和 window.$.fn.select2 是否存在，若不存在則用 setTimeout 輪詢（最實用）

**設計細節**：
```javascript
mounted() {
  this.initializeSelect2WithFallback();
  this.focusInput();
},
initializeSelect2WithFallback() {
  if (window.$ && window.$.fn.select2) {
    this.loadDropdownOptions();
  } else {
    // 3 次輪詢，每次延遲 100ms
    setTimeout(() => this.initializeSelect2WithFallback(), 100);
  }
}
```

### D2: 已完修狀態回應設計

**決策**：GetGunByBarcode() 回應擴展，新增 `status` 欄位標明三種狀態：`success`(有待修)、`all_completed`(已完修)、`error`(查詢失敗)

**理由**：
- 替代方案 A：使用 HTTP 狀態碼（409 Conflict，但語義不佳；成功情況應該用 200）
- 替代方案 B：統一返回 success: false + 不同 message（前端無法精準判斷狀態）
- **選擇方案**：一個 `status` 欄位，前端可精準區分業務狀態，向後相容（舊客戶端忽略新欄位）

**API 回應格式**：
```json
// 成功情況 - 有待維修紀錄
{
  "success": true,
  "status": "success",
  "message": "查詢成功",
  "data": { "repairSno": "...", "Eno": "...", ... }
}

// 已完修情況 - 無待維修紀錄
{
  "success": false,
  "status": "all_completed",
  "message": "此料槍已全部完修，無待維修紀錄",
  "data": null
}

// 錯誤情況
{
  "success": false,
  "status": "error",
  "message": "查無料槍編號",
  "data": null
}
```

### D3: 前端已完修流程

**決策**：在 queryGun() 成功回調中檢查 status，若為 `all_completed`，彈 SweetAlert2 警告後清空表單

**理由**：
- 使用既有的 SweetAlert2 組件保持一致性
- 不改動 isScanned 狀態，避免複雜的狀態機轉換
- 清空 scanInput，讓用戶可立即掃碼下一個料槍

### D4: 相容性考量

**決策**：GetGunByBarcode() 回應向後相容（success: false 時仍可能有不同的 status 值）

**理由**：
- 既有的 ScanRepair 功能已上線，可能有其他消費者
- 新增 status 欄位不破壞既有解析邏輯

## Risks / Trade-offs

| 風險 | 緩解策略 |
|------|--------|
| Select2 CDN 加載緩慢或失敗 | 輪詢最多 3 次（300ms），若失敗則頁面降級為禁用下拉選單並提示「載入失敗」 |
| 已完修判斷邏輯錯誤（誤判為全部完修） | 通過單元測試驗證：Chk 字段的三種值（True/False/Null）的結果 |
| 前端舊版本不認識新 status 欄位 | 回應設計成可選擇性：舊版本檢查 success 欄位即可 |
| 業務規則變化（已完修的判斷標準） | 日後改需求時，只需改 GetGunByBarcode() 一處；前端邏輯無變 |

## Migration Plan

**部署流程**：
1. 部署新的 MaterialGunController（GetGunByBarcode 邏輯更新）
2. 部署新的 ScanRepair.cshtml（Select2 修復 + 已完修流程）
3. 前端自動刷新（無快取）

**回滾策略**：
- 若發現已完修判斷有誤：恢復舊的 GetGunByBarcode 邏輯
- 若 Select2 初始化仍有問題：改為使用原生 HTML select 標籤（降級模式）

## Open Questions

1. 輪詢 Select2 CDN 的次數和延遲是否合適？（目前提議 3 次 × 100ms）
2. 已完修後用戶是否允許切換到歷史紀錄查看模式？（目前設計：禁止，提示返回主頁）
3. 業務上是否可能同時有「部分完修 + 部分待修」的情況？（目前假設：每筆維修紀錄獨立 Chk 狀態，選最新未完修紀錄）
