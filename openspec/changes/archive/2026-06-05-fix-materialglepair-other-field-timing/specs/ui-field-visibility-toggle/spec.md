# UI Field Visibility Toggle Capability Spec

## Overview

支援基於特定觸發條件（如下拉選單值變化）動態顯示或隱藏表單欄位的通用機制。初始化時和用戶交互時均能正確響應。

## Requirements

### R1. 初始化時欄位可見性正確

**Given** 頁面載入時，下拉選單已設定初始值為 99（"其他"）  
**When** 相關表單欄位的邏輯依賴該下拉選單值  
**Then** 相關欄位應自動顯示（不需用戶重新選擇）

**驗收標準**：
- 頁面載入完成後 100ms 內，Other 欄位若應顯示則 display != "none"
- 無 JavaScript 錯誤或警告信息

### R2. 用戶交互時欄位可見性動態切換

**Given** 用戶選擇下拉選單中的選項  
**When** 選項變更觸發 change 事件  
**Then** 相關欄位應根據新選項值動態顯示或隱藏

**驗收標準**：
- 選擇 99（"其他"）時，Other 欄位應顯示（display: block 或等效值）
- 選擇其他選項時，Other 欄位應隱藏（display: none）
- 反覆切換時邏輯應一致

### R3. 邏輯獨立於事件系統

**Given** 相同的欄位可見性邏輯需在多個場景中執行（AJAX 初始化、事件監聽）  
**When** 改動其中一個場景的實現  
**Then** 不應影響其他場景的行為

**驗收標準**：
- 邏輯應抽取為獨立函式（toggleOtherField）
- 多個呼叫點可安全地調用該函式
- 無副作用（不修改 DOM 以外的狀態）

### R4. 型別安全的值比較

**Given** 下拉選單的值可能來自不同的型別來源（Razor ViewBag int、JSON 數字、HTML attribute 字串）  
**When** 比較下拉選單值與條件值  
**Then** 應使用明確的型別轉換而非隱含的型別強制

**驗收標準**：
- 比較前應明確轉型（parseInt、String 等）
- 使用嚴格相等 `===` 而非寬鬆相等 `==`

## Verification Scenarios

| # | 場景 | 預期結果 | 驗證方式 |
|---|------|--------|--------|
| S1 | 新增維修紀錄，MaintenanceResult 選 99 | Other 欄位顯示 | 瀏覽器開發者工具檢查 display 屬性 |
| S2 | 編輯既存紀錄（sno=1783），主鍵欄位已填 | Other 欄位自動顯示（如 MaintenanceResult=99） | 頁面載入後不操作，檢查 Other 可見性 |
| S3 | 用戶選擇 99，然後選其他值 | Other 欄位先顯示後隱藏 | 逐步選擇，觀察 Other 欄位行為 |
| S4 | 頁面 console 檢查 | 無 JavaScript 錯誤 | 開發者工具 Console tab |
| S5 | 提交表單（MaintenanceResult=99，Other 填值） | 表單成功提交，Controller 收到 Other 值 | 提交後檢查 server 日誌或回應 |

## Non-Functional Requirements

- **Performance**: toggleOtherField() 執行時間應 < 5ms
- **Compatibility**: 支援 jQuery 3.4.1、IE 11+、Chrome/Firefox 最新兩版本
- **Code Quality**: JavaScript 應符合現有 project 的編碼規範（如有）

## Dependencies

- jQuery 3.4.1（已存在）
- MaterialGunRepair.cshtml 的現有 HTML 結構

## Scope Notes

- 此 capability 通用設計，未來可應用於其他表單欄位時序問題
- 本次實現先限定於 MaterialGunRepair 的 Other 欄位
- 不修改 Controller 或資料庫邏輯
