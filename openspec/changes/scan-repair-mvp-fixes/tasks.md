## 1. 後端 API 修復 (GetGunByBarcode)

- [x] 1.1 擴展 GetGunByBarcode() 回應結構，新增 status 欄位
- [x] 1.2 實作已完修判斷邏輯 - 查詢該料槍是否存在 Chk=True 的紀錄
- [x] 1.3 新增「all_completed」狀態回應，當料槍無待修紀錄時返回
- [x] 1.4 確保回應格式向後相容 - success: false 情況下正確返回不同的 status

## 2. 前端 Select2 初始化修復

- [x] 2.1 新增 initializeSelect2WithFallback() 方法，檢查 jQuery 和 Select2 是否加載
- [x] 2.2 實作輪詢機制 - 最多 3 次，每次延遲 100ms
- [x] 2.3 改造 mounted() 鉤子，優先調用 initializeSelect2WithFallback() 而非直接 loadDropdownOptions()
- [x] 2.4 測試 Select2 初始化在不同網路速度下的表現

## 3. 前端已完修狀態提示

- [x] 3.1 修改 queryGun() 方法，檢查 response.status 欄位
- [x] 3.2 新增 status="all_completed" 的處理分支，彈 SweetAlert2 警告
- [x] 3.3 已完修情況下清空 scanInput，但不改變 isScanned 狀態（保持表單隱藏）
- [x] 3.4 確保焦點自動回到 scanInputRef
- [x] 3.5 驗證三種狀態轉換：success → all_completed → error

## 4. 前端狀態管理驗證

- [x] 4.1 驗證已完修情況下，維修表單（panel-warning）保持隱藏（v-if="isScanned" 為 false）
- [x] 4.2 驗證已完修後用戶可立即掃碼下一個料槍（輸入框可用）
- [x] 4.3 驗證 queryGun 失敗時，焦點恢復邏輯正確

## 5. 整合與測試

- [x] 5.1 編譯方案，驗證無編譯錯誤
- [x] 5.2 掃碼正常料槍 - 驗證能正常帶出維修表單
- [x] 5.3 掃碼已完修料槍 - 驗證彈警告，表單隱藏
- [x] 5.4 掃碼不存在料槍 - 驗證既有錯誤提示邏輯不變
- [x] 5.5 選擇下拉選項 - 驗證 Select2 工作正常，formData 更新
- [x] 5.6 提交維修表單 - 驗證能正常儲存到既有 MaterialGunRepair POST

## 6. 文件與交付

- [x] 6.1 更新 GetGunByBarcode() 的 XML 文件註解，說明新的 status 欄位
- [x] 6.2 更新系統架構文檔，補充已完修判斷邏輯說明
- [x] 6.3 記錄修復總結 - 問題描述、解決方案、驗證結果
