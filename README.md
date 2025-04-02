# MySurvey 问卷调查系统

MySurvey是基于[Biwen.QuickApi](https://github.com/vipwan/Biwen.QuickApi)的示例项目,是一个简易现代化的问卷调查系统，支持多种问题类型，提供问卷创建、发布、收集和分析等功能。

## 功能特点

### 问卷管理
- 创建、编辑、删除问卷
- 支持问卷复制
- 问卷状态管理（草稿、已发布、已结束）
- 问卷预览功能

### 问题类型
- 文本输入题
- 单选题
- 多选题
- 评分题
- 矩阵题(待实现)

### 问卷收集
- 支持匿名填写
- 问卷分享（链接和二维码）
- 答卷导出（Excel格式）
- 实时统计(待实现)

### 用户管理
- 用户注册和登录
- 邮箱验证(待实现)
- 密码重置(待实现)
- 个人信息管理(待实现)

## 技术架构

### 后端
- .NET Core 9.0
- Entity Framework Core
- Biwen.AutoClassGen(生成自动注入代码)
- Biwen.QuickApi（API开发框架）
- Biwen.Settings（配置中心）
- EPPlus（Excel导出）

### 前端
- React 18
- Ant Design 5.x
- ahooks 3
- React Router 6
- Axios

## 项目结构

```
MySurvey/
├── MySurvey.Core/          # 核心业务逻辑
│   ├── Data/               # 数据访问层
│   ├── Entities/           # 实体模型
│   ├── Interfaces/         # 接口定义
│   └── Services/           # 业务服务实现
├── MySurvey.Server/        # 后端服务
│   ├── Endpoints/          # API端点
│   └── ViewModels/         # 视图模型
└── MySurvey.Client/        # 前端应用
    ├── src/
    │   ├── components/     # React组件
    │   ├── services/       # API服务
    │   └── utils/          # 工具函数
    └── public/             # 静态资源
```

## 快速开始

### 环境要求
- .NET Core 9.0 SDK
- Node.js 18+
- Sqlite

### 后端设置
1. 克隆项目
```bash
git clone https://github.com/vipwan/MySurvey.git
cd MySurvey
```

2. 还原依赖
```bash
dotnet restore
```

3. 运行项目
```bash
cd MySurvey.Server
dotnet run
```
4. 接口文档
```txt
运行成功后直接访问 http://localhost:5289/openapi/scalar/v1
```

### 前端设置
1. 进入前端目录
```bash
cd MySurvey.Client
```

2. 安装依赖
```bash
npm install
```

3. 运行开发服务器
```bash
npm start
```

## 使用说明

### 创建问卷
1. 登录系统
2. 点击"添加问卷"按钮
3. 填写问卷基本信息
4. 添加问题
5. 设置问卷状态（草稿/发布/结束）

### 分享问卷
1. 在问卷列表中找到要分享的问卷
2. 点击"分享"按钮
3. 选择分享方式（链接/二维码）
4. 复制链接或扫描二维码分享给受访者
5. 同时提供Razor Pages形式的答卷链接 `~/s/{surveyId}`

### 导出答卷
1. 在问卷列表中找到要导出的问卷
2. 点击"导出"按钮
3. 系统会自动下载Excel格式的答卷数据

## 开发指南

### 添加新功能
1. 在`MySurvey.Core`中定义接口和实体
2. 在`MySurvey.Core/Services`中实现业务逻辑
3. 在`MySurvey.Server/Endpoints`中添加API端点
4. 在`MySurvey.Client`中实现前端界面

### 数据库迁移
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```


## 部署

### 后端部署
1. 发布项目
```bash
dotnet publish -c Release
```

2. 配置数据库连接字符串
3. 配置JWT密钥
4. 运行应用

### 前端部署
1. 构建项目
```bash
npm run build
```

2. 将`build`目录部署到Web服务器

## 贡献指南
1. Fork项目
2. 创建特性分支
3. 提交更改
4. 推送到分支
5. 创建Pull Request

## 许可证
MIT License

## 作者
万雅虎 (https://github.com/vipwan)
