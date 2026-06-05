# Select2 移除、改用原生 select - 技術設計

## Context

掃碼維修 MVP 中 Select2 下拉選單在生產環境多次初始化失敗（2 次迭代修正）。經過詳細分析和驗證，Select2 在當前環境中存在根本性問題，無法穩定工作。

**問題回顧**：
- 第一次實作：Select2 CDN 加載後，AJAX 回調時仍發生 `TypeError: $(...).select2 is not a function`
- 第一次修正：嘗試新增輪詢邏輯，但仍然不穩定
- 第二次修正：嘗試加強防禦性檢查，但成本與收益不符

**技術現狀**：
- 前端：Vue 3 CDN（development build）+ jQuery 3.4.1 + Select2 4.0.13（CDN）
- 後端：.NET Framework 4.5 MVC
- Select2 CDN 在當前環境無法可靠加載

## Goals / Non-Goals

**Goals:**
1. **簡化代碼**：移除所有 Select2 邏輯與防禦檢查，保持代碼乾淨
2. **確保功能**：使用原生 HTML <select>，維修分類與原因仍可正常選擇
3. **快速交付**：MVP 優先，功能可用比完美更重要
4. **降低維護**：原生 select 無複雜初始化邏輯，無需維護

**Non-Goals:**
- 不保留 Select2 代碼分支（完全移除）
- 不在原生 select 上模擬 Select2 功能（如搜尋）
- 不修改資料庫 Schema 或後端 API
- 不進行優雅降級設計（明確的失敗比隱藏的複雜性更好）

## Decision

### D：簡化策略 - 移除 Select2，使用原生 HTML select

**決策**：完全放棄 Select2，改用瀏覽器原生的 <select> 標籤

**理由**：
- Select2 經過 2 次修正仍無法穩定工作 —— 無法再投入成本
- 原生 <select> 在所有瀏覽器上都可靠運行
- 維修分類和原因選項數量有限（<50 項），用戶可快速掃描，不需要搜尋功能
- 代碼複雜性從 ~80 行降至 ~25 行
- 無 Select2 初始化、無輪詢、無守衛檢查、無防禦邏輯

**實作**：
```javascript
mounted() {
    // 焦點設定，完全沒有 Select2 邏輯
    this.$refs.scanInputRef.focus();
},

methods: {
    loadDropdownOptions() {
        // 載入分類
        $.ajax({
            url: '@Url.Action("Classification", "MaterialGun")',
            success: (data) => {
                this.classificationOptions = data || [];
                // 完全沒有 Select2 初始化
            }
        });

        // 載入維修原因
        $.ajax({
            url: '@Url.Action("MResult", "MaterialGun")',
            success: (data) => {
                this.maintenanceResultOptions = data || [];
                // 完全沒有 Select2 初始化
            }
        });
    }
}
```

**特點**：
- 邏輯清晰：AJAX 加載選項 → Vue v-model 綁定 → 原生 select 呈現
- 無錯誤：原生 <select> 沒有初始化風險
- 易維護：沒有複雜的防禦檢查

---

## 修改清單

| 項目 | 操作 |
|------|------|
| `ensureSelect2Ready()` | ❌ 刪除 |
| `initializeSelect2Element()` | ❌ 刪除 |
| `mounted()` 中的 setTimeout(async, 0) 邏輯 | ❌ 刪除 |
| `loadDropdownOptions()` 中的守衛檢查 | ❌ 刪除 |
| `select2Available` 資料屬性 | ❌ 刪除 |
| Select2 CSS/JS CDN 引入 | ❌ 刪除 |
| HTML 原生 <select id="Classification"> | ✅ 保留 |
| HTML 原生 <select id="MaintenanceResult"> | ✅ 保留 |
| Vue v-model 綁定 | ✅ 保留 |

---

## 已知問題與修正

### ⚠️ 發現 1：v-for 綁定字段錯誤

**問題**：ScanRepair.cshtml 中 v-for 綁定使用錯誤的字段名

```html
<!-- ❌ 錯誤 -->
<option v-for="item in classificationOptions" :key="item.code" :value="item.code">
    {{ item.name }}
</option>

<!-- ✅ 正確 -->
<option v-for="item in classificationOptions" :key="item.Value" :value="item.Value">
    {{ item.Text }}
</option>
```

**根源**：後端 API 返回 `{ Value, Text }`，與既有的 MaterialGunRepair.cshtml 一致  
**影響**：下拉選項渲染為空（item.code 和 item.name 均為 undefined）  
**修正**：更新 v-for 綁定為正確的字段名（item.Value, item.Text）  
**狀態**：⏳ 待修正

---

## Risk Assessment

| 項目 | 評估 |
|------|------|
| 原生 select 無搜尋功能 | 低風險 —— 項目數有限，用戶快速掃描可接受 |
| 放棄 Select2 搜尋增強 | 低風險 —— MVP 優先，功能完整 > 體驗增強 |
| 代碼簡化導致功能遺漏 | 無 —— 原生 select 支援全部功能 |
| 資料綁定字段不匹配 | 中風險 —— 已識別，待修正 |

**總體風險等級**：🟡 中（待修正綁定問題）

---

## 後續改進

未來若需要搜尋功能，可考慮：
1. 升級至 Vue 3 Composition API + 原生組件
2. 使用輕量級套件（如 Selectize.js）
3. 改用 datalist 提供搜尋體驗
