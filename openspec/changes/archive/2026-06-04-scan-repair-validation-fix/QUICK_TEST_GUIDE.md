# 🧪 快速測試驗證指南

**目標**：驗證掃碼維修功能在去除料槍資訊面板後仍正常運作  
**預計耗時**：15-20 分鐘  
**環境**：本地調試環境 + 瀏覽器 F12

---

## 🚀 快速開始

### Step 1: 本地編譯運行（2 分鐘）

```bash
1. 開啟 Visual Studio → ESIntegrateSys.sln
2. Build → Build Solution (Ctrl+Shift+B)
3. 預期：無編譯錯誤
4. Debug → Start Debugging (F5)
5. 瀏覽至：http://localhost:port/MaterialGun/ScanRepair
```

### Step 2: 開啟開發者工具（1 分鐘）

```
F12 → Console 標籤
預期：無紅色 JavaScript 錯誤
```

---

## ✅ 快速驗證清單

### 🎯 測試場景 1：掃碼成功（有未完修紀錄）

**步驟**：
```
1. 輸入包含未完修紀錄的料槍編號（如 GUN001）
2. 按 Enter 或等待自動查詢
3. 查看頁面變化
```

**預期結果**：✅
- ✓ 無「料槍資訊」面板（重要！）
- ✓ 「維修資訊」區直接顯示
- ✓ 焦點自動進入「檢修不良分類」下拉
- ✓ Console 無錯誤
- ✓ formData.repairSno 已設置（檢查開發工具 Vue 標籤）

**快速檢查**：
```javascript
// 在 Console 執行
app.formData.repairSno  // 應該有值
app.isScanned           // 應該是 true
```

---

### 🎯 測試場景 2：掃碼已完修

**步驟**：
```
1. 清空輸入框（清空按鈕或手動）
2. 輸入已全部完修的料槍編號（如 GUN999）
3. 按 Enter
```

**預期結果**：✅
- ✓ 彈警告對話框：「此料槍已全部完修，無待維修紀錄」
- ✓ 點「確定」後，表單隱藏
- ✓ 掃碼框獲焦
- ✓ 無「料槍資訊」面板

---

### 🎯 測試場景 3：掃碼失敗（無紀錄或不存在）

**步驟**：
```
1. 清空輸入框
2. 輸入不存在的料槍編號（如 INVALID）
3. 按 Enter
```

**預期結果**：✅
- ✓ 彈錯誤對話框：「查無料槍編號」或「無維修紀錄」
- ✓ 點「確定」後，表單隱藏
- ✓ 掃碼框獲焦，可重新輸入

---

### 🎯 測試場景 4：下拉選單載入

**步驟**：
```
1. 掃碼成功後，點擊「檢修不良分類」下拉
2. 點擊「檢修不良原因」下拉
```

**預期結果**：✅
- ✓ 選項正常載入，無 JS 錯誤
- ✓ 可正常選擇
- ✓ 選擇後 v-model 同步（檢查開發工具）

---

### 🎯 測試場景 5：其他原因邏輯

**步驟**：
```
1. 掃碼成功後
2. 分類選擇任意項
3. 維修原因選「99」（其他）
```

**預期結果**：✅
- ✓ 「其他原因」輸入框自動顯示
- ✓ 輸入文字
- ✓ 其他原因選項變更時，輸入框自動隱藏

---

### 🎯 測試場景 6：表單提交

**步驟**：
```
1. 掃碼成功
2. 填寫完整表單：
   - 分類：選擇任意項
   - 維修原因：選擇任意項
   - 若選「99」填寫其他原因
   - 更換零件名稱、編號（可選）
3. 點「提交維修」
```

**預期結果**：✅
- ✓ 表單驗證通過（若有必填），提交請求發送
- ✓ Network 標籤中可看到 POST 請求至 `/MaterialGun/MaterialGunRepair`
- ✓ 請求 Body 中 `sno` 參數存在且有值
- ✓ 成功提示：「儲存成功」
- ✓ 1.5 秒後自動返回清單頁

---

### 🎯 測試場景 7：清空和返回

**步驟**：
```
1. 填寫表單後點「清空」
2. 按「返回清單」
```

**預期結果**：✅
- ✓ 「清空」重置所有欄位，掃碼框獲焦
- ✓ 「返回清單」正常導航至清單頁

---

## 🔍 Console 檢查

在各個測試場景中，按 F12 → Console 檢查：

```javascript
// 檢查 Vue 應用狀態
console.log(app.$data)

// 特別檢查
console.log('isScanned:', app.isScanned)
console.log('repairSno:', app.formData.repairSno)
console.log('Classification:', app.formData.Classification)
console.log('MaintenanceResult:', app.formData.MaintenanceResult)
```

**預期**：無紅色錯誤，只有正常的 AJAX 日誌

---

## ✅ 驗收檢查清單

完成所有 7 個場景後，檢查：

- [ ] 場景 1：成功無面板 ✓
- [ ] 場景 2：已完修攔截 ✓
- [ ] 場景 3：失敗攔截 ✓
- [ ] 場景 4：下拉選單 ✓
- [ ] 場景 5：其他原因邏輯 ✓
- [ ] 場景 6：表單提交 ✓
- [ ] 場景 7：清空返回 ✓
- [ ] Console 無錯誤 ✓

**所有項目 ✓ 通過 → 變更準備就緒！**

---

## 🚀 後續步驟

```bash
# 1. 提交 git commit
git add .
git commit -m "fix: 修正掃碼維修驗證邏輯與 UI 簡化

- 修正 GetGunByBarcode 查詢條件：Chk==true→Chk==false
- 移除 ScanRepair 前端料槍資訊面板
- 簡化 MVP 設計，提升使用體驗"

# 2. 存檔變更
openspec archive --change scan-repair-validation-fix
```

---

**提示**：若任何場景失敗，檢查：
1. 後端查詢條件是否正確修改
2. 前端 isScanned 判斷
3. API 響應格式
4. Console 錯誤訊息
