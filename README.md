# 昶亨整合系統 (Eversun Enterprise Integration System)

昶亨企業整合管理系統，整合訂單、庫存、財務、人事、生產等核心業務模組。

## 功能模組

- 訂單管理：管理客戶訂單與出貨作業
- 庫存管理：追蹤商品庫存與補貨需求
- 財務報表：查看財務數據與報表分析
- 人事管理：員工資料與排班管理
- 生產排程：生產計劃與進度追蹤
- 系統設定：系統參數與權限設定

## 技術架構

- **後端**：Python / Flask
- **前端**：HTML5 / CSS3 (Jinja2 模板)
- **認證**：Flask-Login

## 快速開始

```bash
# 安裝相依套件
pip install -r requirements.txt

# 啟動開發伺服器
python app.py
```

瀏覽器開啟 `http://localhost:5000`，使用帳號 `admin` 登入。

## 環境變數

| 變數 | 說明 | 預設值 |
|------|------|--------|
| `SECRET_KEY` | Flask session 加密金鑰 | 每次啟動隨機產生 |
| `ADMIN_PASSWORD` | 管理員登入密碼 | `admin123` |

> ⚠️ 正式部署前請務必透過環境變數設定 `SECRET_KEY` 與 `ADMIN_PASSWORD`。
