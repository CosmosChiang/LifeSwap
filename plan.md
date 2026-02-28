---
post_title: "LifeSwap 開發計劃（分階段）"
author1: "LifeSwap Team"
post_slug: "lifeswap-development-plan"
microsoft_alias: "n/a"
featured_image: "n/a"
categories: ["engineering"]
tags: ["planning", "vue3", "dotnet", "sqlite", "mvp"]
ai_note: "AI-assisted drafting"
summary: "LifeSwap 以產品完整性優先的 30 天衝刺計劃，已完成 P0 核心項目並持續推進 P1/P2。"
post_date: "2026-03-01"
---

## 計劃目標（30 天）

- 以「產品完整性優先」為原則，先補齊核心流程缺口，再擴充體驗與維運能力。
- 30 天內達成：申請→審核→通知→報表→自動化 的可用閉環。
- 任務粒度以可直接開工為標準，包含 API、前端、測試與文件同步。

## 目前狀態（截至 2026-02-28）

- 已有基礎可用：申請與審核流程、RBAC、站內通知、報表頁面與 API。
- 已部分完成：通知整合（Teams Webhook 骨架）、自動化排程、法規預警與趨勢分析。
- 主要缺口：跨模組一致性、例外情境覆蓋、端到端驗證、文件與驗收標準對齊。

## 執行進度（截至 2026-03-01）

- 已完成 P0-1：申請狀態機與取消邊界一致化（Returned 可重送、不可取消）。
- 已完成 P0-2：提交/核准/拒絕/退回/取消事件的站內通知補齊；Teams 發送改為失敗不阻塞主流程。
- 已完成 P0-3：報表與預警口徑一致化（requestType/departmentCode 篩選與工時計算對齊）。
- 已完成 P0-4：自動化流程可運行版（重試、執行狀態追蹤、手動重跑 API）。
- 已完成前端管理介面：新增「自動化管理」頁與管理員首頁快捷入口。
- 測試現況：後端測試全數通過；前端新增 API 測試檔，待在 Vitest 環境完成整包驗證。

## 測試覆蓋地圖（截至 2026-03-01）

- 請求流程與狀態機：`backend/tests/LifeSwap.Api.Tests/RequestsControllerWorkflowTests.cs`、`backend/tests/LifeSwap.Api.Tests/RequestWorkflowServiceTests.cs`
- 報表與預警：`backend/tests/LifeSwap.Api.Tests/ReportsControllerTests.cs`
- 自動化與手動重跑：`backend/tests/LifeSwap.Api.Tests/AutomationWorkflowServiceTests.cs`、`backend/tests/LifeSwap.Api.Tests/AutomationExecutionServiceTests.cs`、`backend/tests/LifeSwap.Api.Tests/AutomationControllerTests.cs`
- 控制器覆蓋與 ProblemDetails 契約：`backend/tests/LifeSwap.Api.Tests/ControllersCoverageTests.cs`、`backend/tests/LifeSwap.Api.Tests/ProblemDetailsContractTests.cs`
- 認證與密碼流程：`backend/tests/LifeSwap.Api.Tests/AuthControllerTests.cs`

## 優先級定義

- P0：阻擋上線或造成核心流程中斷的項目，必須在 30 天內完成。
- P1：提升穩定性與可維運性，應於 P0 完成後立即完成。
- P2：體驗與優化項目，不阻擋上線但需排入本次衝刺尾段。

## 實作任務清單（Implementation-ready）

### P0（產品完整性必做）

- 任務：統一申請狀態流轉與驗證規則（含 Returned 重送、取消邊界）。
  - 交付：後端狀態轉移檢查、前端按鈕顯示規則一致、錯誤訊息一致化。
  - 完成日：D1-D5
- 任務：補齊通知事件閉環（站內通知必達，Teams 失敗不阻塞主流程）。
  - 交付：提交/核准/拒絕/退回/重送/取消均產生可追蹤通知事件。
  - 完成日：D3-D8
- 任務：報表與預警資料一致性修正（與申請資料同一口徑）。
  - 交付：期間、部門、類型篩選一致；預警門檻與 API 回傳欄位固定。
  - 完成日：D6-D11
- 任務：自動化流程最小可運行版本（排程提醒與定期報告）。
  - 交付：排程啟停、錯誤重試、基本執行日誌與手動重跑入口。
  - 完成日：D9-D14
- 任務：核心流程測試補齊（後端 + 前端）。
  - 交付：關鍵路徑測試通過，覆蓋提交流程、審核流程、通知與報表查詢。
  - 完成日：D12-D18

### P1（穩定性與維運）

- 任務：權限矩陣補強（跨角色查詢與操作邊界）。
  - 交付：管理者、主管、員工的可見範圍與可操作行為可測試且可追溯。
  - 完成日：D15-D20
- 任務：API 錯誤模型與文件對齊（Problem Details / 狀態碼一致）。
  - 交付：常見錯誤案例文件化，前端可正確顯示使用者可理解訊息。
  - 完成日：D18-D22
- 任務：通知與自動化監控基線。
  - 交付：失敗事件計數、最近執行狀態、手動重試流程文件。
  - 完成日：D20-D24

### P2（體驗與優化）

- 任務：報表 UI 可用性優化（查詢條件保留、空資料提示、多語系字串補齊）。
  - 交付：報表頁可穩定呈現空態與錯誤態；i18n key 完整。
  - 完成日：D22-D26
- 任務：通知中心操作優化（已讀批次、篩選與排序）。
  - 交付：通知清單互動流暢，API 與前端參數一致。
  - 完成日：D24-D28
- 任務：衝刺收斂與發布清單。
  - 交付：版本變更摘要、已知限制、回滾步驟。
  - 完成日：D28-D30

## 30 天時程切分

### D1-D10（閉環打底）

- 完成 P0 前三項：狀態機、通知閉環、報表口徑一致。
- 每日檢查：API 行為、前端流程、通知事件是否一致。

### D11-D20（品質補齊）

- 完成 P0 測試補齊與 P1 權限矩陣。
- 建立自動化流程最小監控與錯誤處理。

### D21-D30（收斂與上線準備）

- 完成 P1 剩餘項與 P2 體驗優化。
- 進行衝刺驗收、文件收斂與發布準備。

## 任務與關鍵檔案對應

### Backend

- 狀態流轉與驗證：`backend/src/LifeSwap.Api/Domain/TimeOffRequest.cs`、`backend/src/LifeSwap.Api/Services/RequestWorkflowService.cs`、`backend/src/LifeSwap.Api/Controllers/RequestsController.cs`
- 通知閉環：`backend/src/LifeSwap.Api/Controllers/NotificationsController.cs`、`backend/src/LifeSwap.Api/Services/AutomationWorkflowService.cs`
- 報表與預警：`backend/src/LifeSwap.Api/Controllers/ReportsController.cs`、`backend/src/LifeSwap.Api/Contracts/ReportDtos.cs`
- 自動化排程：`backend/src/LifeSwap.Api/Services/AutomationSchedulerHostedService.cs`、`backend/src/LifeSwap.Api/Services/AutomationOptions.cs`

### Frontend

- 申請與審核流程：`frontend/src/views/MyRequests.vue`、`frontend/src/views/ToReview.vue`、`frontend/src/composables/useRequestWorkflow.ts`
- 通知：`frontend/src/views/Notifications.vue`、`frontend/src/api.ts`
- 報表與預警：`frontend/src/views/Reports.vue`、`frontend/src/__tests__/Reports.test.ts`
- 權限與角色管理：`frontend/src/views/AdminUsers.vue`、`frontend/src/views/AdminRoles.vue`、`frontend/src/composables/useAuth.ts`

### Tests

- 後端流程與控制器：`backend/tests/LifeSwap.Api.Tests/RequestWorkflowServiceTests.cs`、`backend/tests/LifeSwap.Api.Tests/ReportsControllerTests.cs`、`backend/tests/LifeSwap.Api.Tests/ControllersCoverageTests.cs`
- 前端 API 與頁面：`frontend/src/__tests__/api.notifications.test.ts`、`frontend/src/__tests__/api.reports.test.ts`、`frontend/src/__tests__/Notifications.test.ts`

### Docs

- 衝刺計劃與驗收：`plan.md`
- 前端使用與啟動資訊：`frontend/README.md`
- API 行為對齊（以 Swagger 實際輸出為準）：`backend/src/LifeSwap.Api/Program.cs`

## 驗證計劃（Verification）

- 單元驗證：後端 xUnit 與前端 Vitest 皆需通過本次新增與既有關鍵測試。
- 整合驗證：以「建立申請→審核→通知→報表反映」進行端到端手動測試。
- 角色驗證：員工/主管/管理者三角色執行同場景，確認無越權與資料外洩。
- 失敗驗證：模擬 Teams 通知失敗、自動化任務失敗，主流程不得中斷。
- 回歸驗證：對已部分完成功能（報表/通知/自動化）做回歸，避免退化。

## 驗收標準（Acceptance Criteria）

- 核心流程可用：申請、審核、查詢、通知、報表在預期條件下可完整完成。
- 資料一致：同一條件下 API 與前端顯示結果一致，報表與預警口徑一致。
- 權限正確：不同角色僅可存取授權範圍，越權請求回傳正確錯誤碼。
- 可維運：排程與通知失敗可觀測、可重試、可追蹤。
- 可交付：測試通過、文件更新、已知限制與回滾步驟已記錄。

## Execution Notes

- 先做 P0，不同功能線可並行，但每日以「狀態機與資料口徑一致」為合併門檻。
- 每完成一項任務即同步更新測試與 `plan.md`，避免最後集中補文件。
- 報表/通知/自動化既有能力以「補齊與穩定」為主，不重寫已可運作模組。
