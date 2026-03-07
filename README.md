# LifeSwap

> LifeSwap 是一套以「申請、審核、通知、報表、自動化」為核心的全端管理系統，支援多語言、RBAC 權限、端到端流程，適用於企業請假、審核、通知與自動化場景。

[![CI](https://github.com/CosmosChiang/LifeSwap/actions/workflows/ci.yml/badge.svg)](https://github.com/CosmosChiang/LifeSwap/actions/workflows/ci.yml)
[![CodeQL](https://github.com/CosmosChiang/LifeSwap/actions/workflows/codeql.yml/badge.svg)](https://github.com/CosmosChiang/LifeSwap/actions/workflows/codeql.yml)

---

## 專案架構

```text
LifeSwap/
├── backend/      # ASP.NET Core Web API，資料庫、業務邏輯、驗證、通知、報表
│   └── src/
│       └── LifeSwap.Api/   # 主 API 專案
│           └── Contracts/  # DTO 與資料結構
│           └── Controllers/ # REST API 控制器
│           └── Domain/     # 核心業務物件
│           └── Services/   # 服務層
│           └── tests/      # 單元測試
├── frontend/     # Vue 3 + TypeScript + Vite，SPA 前端
│   └── src/      # 主程式碼、組件、views、composables
│   └── public/   # 靜態資源
│   └── __tests__/ # 前端測試
└── docker-compose.yml # 一鍵啟動全端
```

## 功能特色

- **申請流程**：支援多種請假/申請類型，狀態機管理，流程自動化
- **審核機制**：多層級 RBAC 權限，審核、退回、取消、通知
- **通知整合**：站內通知、Teams webhook、即時推播
- **報表分析**：多維度統計、趨勢分析、法規預警
- **自動化排程**：自動審核、定時通知、API 整合
- **多語言支援**：內建繁體中文、英文，易於擴充
- **測試覆蓋率**：前後端皆有單元測試與覆蓋率報告
- **安全性**：依賴安全掃描、CodeQL 靜態分析、最小權限原則

## 快速開始

```bash
# 啟動全端（需安裝 Docker）
docker-compose up -d

# 前端開發
cd frontend
npm install
npm run dev

# 後端開發
cd backend/src/LifeSwap.Api
 dotnet restore
 dotnet run
```

## 前端技術

- Vue 3 + TypeScript + Vite
- 組件化設計、i18n 多語言、RBAC 權限
- API 整合、通知、報表、測試覆蓋率

## 後端技術

- ASP.NET Core 10
- EF Core 資料庫、RBAC、通知、報表
- 單元測試、覆蓋率、CodeQL 安全分析

## CI/CD 與測試

- [CI](https://github.com/CosmosChiang/LifeSwap/actions/workflows/ci.yml)：自動建置、測試、覆蓋率
- [CodeQL](https://github.com/CosmosChiang/LifeSwap/actions/workflows/codeql.yml)：靜態安全分析
- 依賴安全掃描、NuGet/npm 快取、併發控制

---

## 常見問題

:::note
如需自訂流程、整合第三方通知、或有特殊需求，請參考各目錄下的 README 或聯絡開發團隊。
:::

---

## 參考文件

- [前端 README](frontend/README.md)
- [後端 API 文件](backend/src/LifeSwap.Api/README.md)
- [開發計劃](plan.md)

---

> 本專案以產品完整性優先，持續優化流程、測試與安全性。
