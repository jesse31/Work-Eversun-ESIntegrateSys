# 料槍維修存取控制強化 - 測試計畫

## 前置設定

1. **建立測試料槍資料**
   - 未完成維修料槍（Chk=0, Discard=false）- Sno: XXX_1
   - 已完成維修料槍（Chk=1, Discard=false）- Sno: XXX_2
   - 報廢料槍（Chk=1, Discard=true）- Sno: XXX_3
   - 已完成且報廢（Chk=1, Discard=true）- Sno: XXX_4

2. **測試帳號**
   - 白名單帳號：02898 或 IT 部門員工
   - 非白名單帳號：99999（需自行設置）

---

## Test Case 4.1: 已完成維修 + 非白名單使用者

**目標** 驗證非白名單使用者無法進入已完成維修頁面

**步驟**
1. 以非白名單帳號登入
2. 手動輸入 URL: `MaterialGun/MaterialGunRepair?sno=XXX_2`
3. 預期行為：
   - ❌ 不進入編輯頁面
   - ✅ 重新導向至 `/MaterialGun/MaterialGunRepairView?page=1`
   - ✅ 顯示提示訊息：「此料槍已完成維修，無權修改」
   - ✅ 檢查日誌中是否有 Warning 記錄

**結果**：通過 / 失敗 ____

---

## Test Case 4.2: 報廢料槍

**目標** 驗證報廢料槍無法編輯

**步驟**
1. 以任何帳號登入
2. 手動輸入 URL: `MaterialGun/MaterialGunRepair?sno=XXX_3`
3. 預期行為：
   - ❌ 不進入編輯頁面
   - ✅ 重新導向至 `/MaterialGun/MaterialGunRepairView?page=1`
   - ✅ 顯示提示訊息：「此料槍已報廢，無法編輯」
   - ✅ 檢查日誌中是否有 Warning 記錄

**結果**：通過 / 失敗 ____

---

## Test Case 4.3: 已完成維修 + 白名單使用者

**目標** 驗證白名單使用者可以進入已完成維修頁面

**步驟**
1. 以白名單帳號登入（02898 或 IT 部門）
2. 手動輸入 URL: `MaterialGun/MaterialGunRepair?sno=XXX_2`
3. 預期行為：
   - ✅ 進入編輯頁面
   - ✅ 顯示維修編輯表單（料槍編號、分類、結果等）
   - ✅ 檢查日誌中是否有 Information 記錄

**結果**：通過 / 失敗 ____

---

## Test Case 4.4: 未完成維修 + 任何使用者

**目標** 驗證未完成維修料槍允許任何使用者進入

**步驟**
1. 以非白名單帳號登入
2. 手動輸入 URL: `MaterialGun/MaterialGunRepair?sno=XXX_1`
3. 預期行為：
   - ✅ 進入編輯頁面（無需白名單檢查）
   - ✅ 顯示維修編輯表單
   - ✅ 檢查日誌中是否有 Information 記錄

**結果**：通過 / 失敗 ____

---

## Test Case 4.5: 不存在的 sno

**目標** 驗證不存在的料槍編號友善提示

**步驟**
1. 以任何帳號登入
2. 手動輸入 URL: `MaterialGun/MaterialGunRepair?sno=99999`
3. 預期行為：
   - ✅ 不進入編輯頁面
   - ✅ 重新導向至 `/MaterialGun/MaterialGunRepairView?page=1`
   - ✅ 顯示提示訊息：「維修記錄不存在」

**結果**：通過 / 失敗 ____

---

## Test Case 4.6: TempData 訊息顯示

**目標** 驗證返回頁面正確顯示 TempData 訊息

**步驟**
1. 執行 Test Case 4.1 或 4.2 或 4.5（任意導向情況）
2. 觀察返回的 MaterialGunRepairView 頁面
3. 預期行為：
   - ✅ 頁面頂部顯示提示訊息（toast、alert 或 banner）
   - ✅ 訊息內容與設置的 TempData["message"] 一致
   - ✅ 訊息在頁面重新整理後消失（TempData 特性）

**結果**：通過 / 失敗 ____

---

## Test Case 4.7: 日誌記錄

**目標** 驗證所有情況下都正確記錄日誌

**步驟**
1. 執行 Test Cases 4.1 ~ 4.5
2. 檢查應用程式日誌檔案或 Serilog 輸出
3. 預期日誌記錄：

| 情況 | 預期日誌等級 | 預期訊息內容 |
|------|-----------|-----------|
| 允許進入（通過所有檢查） | Information | `User {UserId} (Dept: {UDeptNo}) accessed repair record for MaterialGun Sno {Sno}` |
| 無權限被擋（已完成+非白名單） | Warning | `User {UserId} denied access to repair record {Sno}` 包含原因 |
| 報廢被擋 | Warning | `Access denied to repair record {Sno}: record is discarded` |
| 記錄不存在 | 無需日誌 | 直接重導向 |

**驗證清單**
- [ ] 確認 Information 日誌被正確記錄
- [ ] 確認 Warning 日誌被正確記錄
- [ ] 確認日誌包含所有 ForContext 資訊（UserId, UDeptNo, MaterialGunSno, Sno）
- [ ] 確認日誌訊息明確表達拒絕原因

**結果**：通過 / 失敗 ____

---

## 總結

完成所有測試後，填寫此部分：

| Test Case | 狀態 | 備註 |
|-----------|------|------|
| 4.1 已完成+非白名單 | ☐ | |
| 4.2 報廢料槍 | ☐ | |
| 4.3 已完成+白名單 | ☐ | |
| 4.4 未完成 | ☐ | |
| 4.5 不存在 | ☐ | |
| 4.6 TempData 訊息 | ☐ | |
| 4.7 日誌記錄 | ☐ | |

**所有測試通過**：☐ 是 / ☐ 否

如有失敗，請詳述失敗原因並進行修復。
