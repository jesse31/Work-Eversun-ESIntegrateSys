# 綁定字段修正記錄

**發現日期**：2026-06-04 15:10  
**問題等級**：🟡 中（功能受阻）  
**狀態**：⏳ 待修正

---

## 問題描述

### 根源
後端 API (`Classification` 和 `MResult`) 返回資料格式：
```json
{
  "Value": 1,
  "Text": "分類A"
}
```

但前端 ScanRepair.cshtml 的 v-for 期望錯誤的字段名：
```html
<option v-for="item in classificationOptions" :key="item.code" :value="item.code">
    {{ item.name }}
</option>
```

### 現象
- 下拉選項渲染為空（無可見選項）
- Console 無錯誤（Vue 靜默失敗）
- item.code 和 item.name 均為 undefined

---

## API 對應表

| 組件 | 後端返回 | 前端期望（錯誤） | 正確應該用 |
|------|--------|-----------------|-----------|
| 分類 | `Value` | `item.code` | `item.Value` ✅ |
| 分類 | `Text` | `item.name` | `item.Text` ✅ |
| 原因 | `Value` | `item.code` | `item.Value` ✅ |
| 原因 | `Text` | `item.name` | `item.Text` ✅ |

---

## 修正清單

### 修正位置 1：Classification select（第 186-188 行）

```html
<!-- ❌ 當前（錯誤） -->
<option v-for="item in classificationOptions" :key="item.code" :value="item.code">
    {{ item.name }}
</option>

<!-- ✅ 修正後 -->
<option v-for="item in classificationOptions" :key="item.Value" :value="item.Value">
    {{ item.Text }}
</option>
```

### 修正位置 2：MaintenanceResult select（第 206-208 行）

```html
<!-- ❌ 當前（錯誤） -->
<option v-for="item in maintenanceResultOptions" :key="item.code" :value="item.code">
    {{ item.name }}
</option>

<!-- ✅ 修正後 -->
<option v-for="item in maintenanceResultOptions" :key="item.Value" :value="item.Value">
    {{ item.Text }}
</option>
```

---

## 驗證步驟

修正後需驗證：

1. ✅ 頁面載入時下拉選項不再為空
2. ✅ Classification 顯示所有分類選項
3. ✅ MaintenanceResult 顯示所有維修原因選項
4. ✅ 選擇選項後 v-model 正確同步
5. ✅ 選擇「99」時「其他原因」輸入框出現

---

## 參考資料

**既有實作**（已驗證正確）：
- MaterialGunRepair.cshtml（第 24-28、50-54 行）
  - 使用 `$.each(data, function(index, item) { ... })`
  - 正確使用 `item.Value` 和 `item.Text`
  - 這是我們應該遵循的模式

**後端 API**：
- `@Url.Action("Classification", "MaterialGun")` → 返回 `[{ Value, Text }, ...]`
- `@Url.Action("MResult", "MaterialGun")` → 返回 `[{ Value, Text }, ...]`

---

## 後續

修正完成後：
- [ ] 執行手動功能測試（tasks 8.3-8.4）
- [ ] 確認所有測試通過（tasks 6.1-6.7）
- [ ] 標記任務完成
- [ ] 存檔變更
