---
post_title: "LifeSwap 開發計劃（分階段）"
author1: "LifeSwap Team"
post_slug: "lifeswap-development-plan"
microsoft_alias: "n/a"
featured_image: "n/a"
categories: ["engineering"]
tags: ["planning", "vue3", "dotnet", "sqlite", "mvp"]
ai_note: "AI-assisted drafting"
summary: "LifeSwap 的分階段開發計劃，定義 MVP 與後續擴充流程、驗收方式與風險控管。"
post_date: "2026-02-15"
---

## LifeSwap 開發計劃（供後續開發對齊）

## TL;DR

本計劃採分階段路線：先完成可上線的 MVP（申請、審核、記錄、
權限、站內通知），再逐步擴充報表分析、法規預警、Teams 整合與
自動化。

法規基準以台灣勞基法為主，前端測試採 Vitest，後端測試採 xUnit。

## 目標與範圍

### MVP（Phase 1）

1. 加班申請：建立、送審、查詢、撤回（規則可控）
2. 補休申請：建立、送審、查詢、撤回（規則可控）
3. 申請審核：主管審核（核准、拒絕、退回）
4. 歷史記錄：員工與主管可查詢狀態與細節
5. 權限控制：員工、主管、HR、系統管理者的基本 RBAC
6. 通知：站內通知（申請狀態變更）

### 擴充（Phase 2）

1. 報表生成（部門、期間、類型）
2. 法規上限預警與趨勢分析
3. Teams API 通知整合
4. 外部系統整合（HR、ERP API）
5. 自動化流程（排程提醒、定期報告）

## 功能需求對應

1. 加班申請（Phase 1）
2. 補休申請（Phase 1）
3. 申請審核（Phase 1）
4. 加班和補休記錄（Phase 1）
5. 通知系統（Phase 1 站內通知；Phase 2 Teams）
6. 報表生成（Phase 2）
7. 用戶管理（Phase 1 基礎管理；Phase 2 強化）
8. 權限控制（Phase 1）
9. 使用網頁技術（Phase 1）
10. 資料安全（Phase 1）
11. 多語言支援（Phase 2）
12. 移動端適配（Phase 2）
13. API 接口（Phase 1）
14. 資料分析與法規上限警告（Phase 2）
15. 自動化流程（Phase 2）

## 技術選型

- 前端：Vue.js（TypeScript）
- 後端：.NET（ASP.NET Core）
- 資料庫：SQLite
- 認證：JWT
- 部署：Docker
- 測試：Vitest（前端）與 xUnit（後端）
- 持續整合：GitHub Actions
- API 文檔：Swagger / OpenAPI
- 多語言支援：i18n 套件
- 通知系統：Phase 1 站內通知；Phase 2 Teams API
- 資料分析：內建分析工具或第三方開源套件
- 自動化流程：排程任務（Cron）與事件驅動流程
- 安全措施：HTTPS、敏感資料保護、RBAC、定期安全審計

## 核心流程設計

### 申請狀態機（草案）

Draft → Submitted → Approved / Rejected / Returned → Cancelled

### 角色權限（草案）

- 員工：建立、查詢個人申請；撤回未審結案件
- 主管：審核所屬範圍案件
- HR：跨部門查詢與報表查看
- 系統管理者：帳號、角色、系統設定

## 里程碑

### M0：規則與模型對齊

- 完成法規口徑（台灣勞基法）與補休換算規則表
- 完成資料模型與欄位字典
- 完成驗收標準草案

### M1：MVP 功能可用

- 申請與審核流程全鏈路完成
- 站內通知可用
- RBAC 最小可用
- API 文件可供前後端協作

### M2：品質與上線準備

- 關鍵流程測試覆蓋
- Docker 化與 CI 基線
- 安全檢查（基本權限、敏感資料保護）

### M3：Phase 2 擴充

- 報表、分析、預警
- Teams 與外部 API 整合
- 自動化排程與運維指標

## 驗收標準（Definition of Done）

- 每項功能具備：需求描述、流程、API、測試案例、驗收條件
- 角色授權符合矩陣，不可越權查詢或操作
- 申請狀態流轉符合狀態機規則
- 核心流程可追溯（時間、操作人、結果）
- 文件與技術選型一致（含測試框架）

## 主要風險與對策

- 法規計算歧異：固定法域與計算口徑，規則表版本化
- 權限邊界不清：先完成角色矩陣再開發 API
- 整合依賴不確定：Phase 1 不阻塞外部整合，採延後策略
- 需求擴張：以 MVP 驗收準則控制範圍

## 開發協作規範

- 需求新增需標註所屬階段（MVP / Phase 2）
- 變更需同步更新流程、狀態機與驗收條件
- 每次迭代同步更新本文件，確保與實作一致
