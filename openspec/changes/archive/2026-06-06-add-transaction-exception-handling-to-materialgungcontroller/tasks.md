## 1. Session Null Safety 加固

- [x] 1.1 CreateMaintainWork 方法：添加 Session["Member"] null 檢查，改用安全的變數提取模式
- [x] 1.2 MaterialGunForRepair (POST) 方法：添加 Session["Member"] null 檢查與日誌
- [x] 1.3 MaterialGunRepair (POST) 方法：添加 Session["Member"] null 檢查
- [x] 1.4 MaterialGunCreate (POST) 方法：添加 Session["Member"] null 檢查
- [x] 1.5 Discard (POST) 方法：添加 Session["Member"] null 檢查

## 2. Anti-CSRF Token 驗證補足

- [x] 2.1 MaterialGunForRepair (POST) 方法：添加 [ValidateAntiForgeryToken] 特性
- [x] 2.2 MaterialGunRepair (POST) 方法：添加 [ValidateAntiForgeryToken] 特性
- [x] 2.3 Discard (POST) 方法：添加 [ValidateAntiForgeryToken] 特性
- [x] 2.4 MaterialGunCreate (POST) 方法：添加 [ValidateAntiForgeryToken] 特性
- [x] 2.5 確認對應的 View 包含 @Html.AntiForgeryToken() 呼叫（手動檢查或備註）

## 3. GetGunByBarcode 例外處理與敏感訊息隱藏

- [x] 3.1 包裝 GetGunByBarcode 整個邏輯到 try-catch 區塊
- [x] 3.2 替換 line 515 的 `ex.Message` 為通用訊息 "系統處理發生錯誤，請聯繫管理員"
- [x] 3.3 添加 Serilog 日誌記錄異常，包含 barcode 參數上下文
- [x] 3.4 確保 catch 區塊返回安全的 JSON 回應

## 4. 其他 API 端點例外處理

- [x] 4.1 CheckMaintainStatus：包裝 try-catch，日誌記錄，返回安全訊息
- [x] 4.2 CheckData：包裝 try-catch，日誌記錄，返回安全訊息
- [x] 4.3 BadDesc：包裝 try-catch，日誌記錄，返回安全訊息（避免暴露多語系查詢錯誤）
- [x] 4.4 Classification、MResult、MGunInfoTrade、MGunInfoSize、MGunInfoCycle、DiscardDesc：各添加例外處理

## 5. CreateMaintainWork 交易保護與例外處理

- [x] 5.1 CreateMaintainWork (POST) 方法：將重複檢查、MaintainWork 呼叫包裝到 using(transaction) 區塊
- [x] 5.2 添加 try-catch，捕捉資料庫異常、重複提交、業務邏輯錯誤
- [x] 5.3 catch 區塊添加 Serilog 日誌，包含 userId、userDept、itemgun、operation name
- [x] 5.4 確保 catch 區塊設定適當的 TempData["message"]

## 6. MaterialGunForRepair 交易保護與例外處理

- [x] 6.1 MaterialGunForRepair (POST) 方法：將 ForRepairWork 呼叫包裝到 using(transaction) 區塊
- [x] 6.2 添加 try-catch 環繞整個業務邏輯
- [x] 6.3 catch 區塊添加 Serilog 日誌，包含 userId、materialGunSno、badDescription 等上下文
- [x] 6.4 錯誤回應改為通用訊息，不暴露異常詳節

## 7. MaterialGunRepair 交易保護與例外處理

- [x] 7.1 MaterialGunRepair (POST) 方法：將 RepairWork 呼叫包裝到 using(transaction) 區塊
- [x] 7.2 添加 try-catch 環繞整個業務邏輯
- [x] 7.3 授權檢查後的 catch：添加 Serilog 日誌，記錄授權拒絕理由與操作者資訊
- [x] 7.4 RepairWork 執行後的 catch：添加操作成功後的日誌（authorization reason）
- [x] 7.5 整個方法添加外層 catch 捕捉非預期的例外

## 8. MaterialGunCreate 交易保護與例外處理

- [x] 8.1 MaterialGunCreate (POST) 方法：將 db.ES_MaterialGunInfo.Add 與 db.SaveChanges 包裝到 using(transaction) 區塊
- [x] 8.2 添加 try-catch
- [x] 8.3 catch 區塊添加 Serilog 日誌，記錄新增料槍失敗原因（如序號重複）
- [x] 8.4 錯誤回應改為通用訊息

## 9. Discard 與 ManagerCheck 交易保護與例外處理

- [x] 9.1 Discard (POST) 方法：將 DiscardWork 呼叫包裝到 using(transaction) 區塊
- [x] 9.2 Discard 添加 try-catch，日誌記錄 userId、Sno、discardDescription
- [x] 9.3 ManagerCheck 方法：將 DiscardCheck 呼叫包裝到 using(transaction) 區塊
- [x] 9.4 ManagerCheck 添加 try-catch，日誌記錄

## 10. MaintainGun.MaintainWork 交易保護（如需要）

- [ ] 10.1 確認 MaintainWork 服務方法內部是否已有交易保護
- [ ] 10.2 若無，需聯繫服務層開發者協調（超出控制器作用域，但應協調確保端到端交易一致性）
- [ ] 10.3 備註：控制器層外層 transaction 將確保原子性

> **評估備註 (2025-02-11)**：控制器層已使用 Database.BeginTransaction() 包裝 MaintainWork 呼叫，確保原子性。服務層內部交易管理是分離的關注點，建議後續進行代碼審查時確認。

## 11. 單元測試與驗證

- [ ] 11.1 測試 CreateMaintainWork：確保異常時交易回滾
- [ ] 11.2 測試 MaterialGunForRepair：確保異常時交易回滾
- [ ] 11.3 測試 MaterialGunRepair：確保授權檢查日誌正確記錄
- [ ] 11.4 測試 GetGunByBarcode：驗證異常時返回通用訊息，未暴露技術細節
- [ ] 11.5 測試 Null safety：驗證 Session 為 null 時正確重導向登入
- [ ] 11.6 測試 Anti-CSRF：驗證無效 token 時請求被拒

## 12. 日誌與監控配置驗證

- [ ] 12.1 確認 Serilog 配置支援 ForContext() 結構化日誌
- [ ] 12.2 確認 userId、UDeptNo、MaterialGunSno、operation 等欄位在日誌中可查詢
- [ ] 12.3 確認異常堆疊追蹤被正確記錄到伺服器日誌
- [ ] 12.4 驗證生產環境日誌格式與保留策略符合要求

## 13. 程式碼審查與集成測試

- [ ] 13.1 手動審查所有修改，驗證是否遵循 Karpathy Guidelines（小步快跑、簡單優於聰明）
- [ ] 13.2 執行現有單元測試套件，確保無迴歸
- [ ] 13.3 執行集成測試：驗證交易、異常處理、日誌在實際資料庫操作中正常運作
- [ ] 13.4 驗證所有受影響的 View 正確顯示錯誤訊息

## 14. 文件與知識轉移

- [ ] 14.1 更新 MaterialGunController 類別級註解，說明交易保護策略
- [ ] 14.2 為各個 POST 方法添加 XML 文件註解，說明例外處理行為
- [ ] 14.3 更新開發文件，記錄新增的 Serilog 上下文欄位用途
- [ ] 14.4 準備變更日誌，說明後向相容性和任何必要的環境配置

## 15. 部署與驗證

- [ ] 15.1 準備 UAT 環境測試計畫（交易回滾、異常恢復、授權檢查）
- [ ] 15.2 執行煙霧測試（smoke test）：基本功能是否受影響
- [ ] 15.3 監控生產環境日誌，確保異常被正確捕捉與記錄
- [ ] 15.4 驗證無意外的性能影響（交易鎖定、日誌寫入開銷）
