# MySurvey 项目

## 项目简介

这是一个基于 Biwen.QuickApi 库开发的问卷调查系统示例项目。该项目展示了如何使用 Biwen.QuickApi 快速构建现代化的 Web API。

### Biwen.QuickApi 简介

Biwen.QuickApi 是一个开箱即用的 .NET Web API 微型开发框架，它提供了一种简单而优雅的方式来构建 Web API。主要特点包括：

1. **快速开发**
   - 使用特性（Attribute）快速定义 API 路由和元数据
   - 支持 OpenAPI文档自动生成
   - 内置请求验证和响应处理

2. **功能丰富**
   - 支持 API 审计（AuditApi）
   - 支持特性开关（FeatureGate）
   - 集成 FluentValidation 进行请求验证
   - 支持 Mapster 进行对象映射

3. **易于集成**
   - 与 ASP.NET Core 完美集成
   - 支持依赖注入
   - 支持中间件扩展

### 项目实现原理

本项目通过以下方式使用 Biwen.QuickApi：

1. **API 定义**
   ```csharp
   [QuickApi("/api/endpoint")]
   [OpenApiMetadata("API 名称", "API 描述")]
   [AuditApi]
   public class MyApi : BaseQuickApi
   ```

2. **依赖注入配置**
   ```csharp
   builder.Services.AddBiwenQuickApis(o => o.RoutePrefix = "api");
   ```

3. **中间件配置**
   ```csharp
   app.UseBiwenQuickApis();
   ```

### 项目初衷

本项目旨在展示 Biwen.QuickApi 在实际应用中的使用方式，通过一个完整的问卷调查系统来演示：

1. 如何快速构建 RESTful API
2. 如何处理复杂的业务逻辑
3. 如何实现 API 的安全性和可维护性
4. 如何集成其他 Biwen 生态系统的组件（如 Biwen.Settings）

### 部署说明

如果你需要部署这个项目, 请按照以下步骤进行:

1.发布项目到服务器
2.复制客户端项目(mysurvey.client)生成的dist文件夹到服务器wwwroot文件夹