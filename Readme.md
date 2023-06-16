
# 一、本地开发调试
## 1、初始化本地调试环境
- 更新PowerShell到7.0 ，安装时设置7.0版本为默认程序  
安装包位置：/softwarePackage/PowerShell-7.2.4-win-x64.msi  

- 安装dapr  
```
powershell -Command "iwr -useb https://raw.githubusercontent.com/dapr/cli/master/install/install.ps1 | iex"
```
- 验证dapr  
打开命令行输入dapr

- 初始dapr  
```
dapr init --slim
```
## 2、启动调试
-  服务启动前需要先启动dapr服务，否则服务无法启动  
在项目的dapr目录下找到对应项目的脚本文件，双击启动。如果报错，需要设置PowerShell 7为默认程序打开。
- 在对应的dapr服务启动后，将项目设置为启动项，点击调试

# 二、认证中心

## 1、其他服务对接认证中心的方式

### 基于底层运维平台的服务

- Start.cs=>ConfigureServices方法中添加
``` c#
using ConcreteCloud.IdentityServer4.Extensions;

// 添加身份认证
services.AddCustomIdentityServerAuthentication(Configuration);

//添加授权认证
services.AddAuthorization(options =>
{
    options.AddPolicy("permission", policy =>
    {
        policy.RequireAuthenticatedUser();
        olicy.RequireClaim("scope", "businessApi");
    });
});
```
- appsetting.json 配置文件中增加配置
``` json
"IdentityServer": {
    "AuthenticationScheme": "Bearer",
    "ApiName": "businessApiRes",
    "ApiSecret": "api1_secret",
    "Authority": "http://www.concrete-cloud.com/interfaceApi/auth",
    "RequireHttpsMetadata": false,
    "ValidateIssuerName": false,
    "EnableCaching": true,
    "CacheDuration": 1,
    "CacheKeyPrefix": "ids:token:",
    "SaveToken": true,
    "MySqlConnectionString": "server=rm-2zenhdoh61cft57e7do.mysql.rds.aliyuncs.com;port=3306;database=concretecloud_framework;uid=tproot;pwd=JiYunCloud2022!-0322;characterset=utf8;"
  }
```

- 最后控制器中增加特性  
``` c#
[Authorize("permission")]
```

### 其他服务
- 参考一下文章：

https://www.cnblogs.com/nsky/p/10312101.html 

https://www.cnblogs.com/lechengbo/p/9860711.html

## 2、请求上下文中获取当前用户信息
### 基于底层运维平台的服务
- 在类中通过构造方法注入 IUser接口 获取当前用户信息

### 其他服务
- 可以在请求上下文中的 HttpContext.User属性中获取用户信息

## 3、请求上下文中获取当前租户信息
### 基于多租户的服务
- 在类中通过构造方法注入 ICurrentTenant接口 获取当前租户信息
- 一般控制器中可以直接注入IUser接口获取用户以及租户服务，但是有些请求没有用户信息，但是需要租户，比如发布订阅可以注入ICurrentTenant接口，获取租户信息


# 三、微服务集成多租户独立数据库根据租户id切换数据连接

## 1、解析租户信息，按照以下排序，解析到token后返回不再继续解析:
 TenantId对应参数key：\_\_tenant 

 TenantCode对应参数key：\_\_tenantCode

- 通过Url的QueryString参数方式，如：/sale/contract?\_\_tenant=123456&\_\_tenantCode=SP
- 通过Form表单方式
- 通过Route路由方式
- 通过Header方式
- 通过Cookie方式
- 通过用户token方式解析
- 通过发布订阅消息的基类【IntegrationEvent】中的【TenantId】字段解析

## 2、数据库上下文
### 销售服务数据库上下文,
- 其中【ConnectionStringName("Sale")】对应服务类别
- 数据库模型类创建完后，需要再此配置对应的数据库映射
``` C# 
using ConcreteCloud.Core.Data;
using ConcreteCloud.ORM.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ConcreteCloud.Sale.WebApi.EntityFrameworkCore;

[ConnectionStringName("Sale")]
public class SaleDbContext : EfCoreDbContext<SaleDbContext>
{
    public DbSet<EfDemo> FeaturesManages { get; set; }

    public SaleDbContext(DbContextOptions<SaleDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<EfDemo>(b =>
        {
            b.ToTable("t_efdemo_temp");
            b.HasKey(x => x.Id);
            b.Property(x => x.code).HasMaxLength(255);
            b.Property(x => x.name).HasMaxLength(255);
            b.Property(x => x.TenantId).HasMaxLength(50);
        });
    }
}
```
- Start.cs=>ConfigureServices方法中添加
``` C#
    services.AddEfDbContext<SaleDbContext>(options =>
    {
        options.AddDefaultRepositories(true);
    });

    services.Configure<EfDbContextOptions>(options =>
    {
        options.UseMySQL();
    });
```

## 3、Service集成

### 销售合同Demo IContractService接口
``` C# 
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConcreteCloud.Sale.IServices
{
    /// <summary>
    /// 销售合同Demo
    /// </summary>
    public interface IContractService
    {
    }
}
```
### 销售合同Demo IContractService接口实现
``` C#
using ConcreteCloud.DbContext.SqlSugar;
using ConcreteCloud.OrmFoundation.Service;
using ConcreteCloud.Sale.IServices;
using ConcreteCloud.Sale.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConcreteCloud.Sale.Services
{
    /// <summary>
    /// 销售合同Demo
    /// </summary>
    public class ContractService : IContractService
    {
        private readonly IRepository<ContractInfo> _repository;
        public ContractService(IRepository<ContractInfo> repository)
        {
            _repository = repository;
        }
    }
}

```

# 四、微服务中集成日志
## 1、开始添加日志

### 添加日志项目引用
>ConcreteCloud.Serilog  
该项目位置：ConcreteCloud.Framework.DevOps/basicService/ConcreteCloud.Serilog

### 添加appsettings.json中的基础配置** 

- 注意：只监控错误时，日志级别调整为“Error” 

- 以下代码替换【Project】节点中的【Name】为自己项目的名称

``` json
 "Logging": {
    "LogLevel": {
      "Default": "Information",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "Project": {
    "Name": "ConcreteCloud.Message.WebApi",
    "Log_EsUri": "http://192.168.2.56:9200/",
    "Log_EsUser": "elastic",
    "Log_EsPwd": "hTeMlWfVJ30OF4IY"
  }
```
Log_EsUri：es连接地址，默认为http://192.168.2.56:9200/
Log_EsUser：es用户名，默认为"elastic"
Log_EsPwd: es密码，默认为"hTeMlWfVJ30OF4IY"

###  添加Startup.cs中事件注册

- 添加必要引用

``` C#
using Serilog;
using ConcreteCloud.Serilog;
```

- 在Startup构造函数中添加如下代码

``` C#
 public Startup(IConfiguration configuration)
 {
    Configuration = configuration;
    SerilogClient.CreatSerilog(configuration);
 }
```

- 在Configure方法中添加Serilog的使用  
>关键代码：loggerFactory.AddSerilog();

``` C#
 public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    loggerFactory.AddSerilog();
    app.UseMvc();
}
```
### 手动调用代码如下

``` C#
ILogger<ValuesController> _logger;
public ValuesController(ILogger<ValuesController> logger)
{
    _logger = logger;
}

// GET api/values
[HttpGet]
public ActionResult<IEnumerable<string>> Get()
{
    _logger.LogInformation("321321");
    _logger.LogError(Guid.NewGuid().ToString(),e);
    return new string[] { "value1", "value2" };
}
```

- 日志查看地址（kibana）：http://182.92.135.186:5601/

### 其他说明
- es中的index规则为：logstash-{yyyy.mm.dd}   例：logstash-2022.10.29
- appsettings.json中配置的ProjectName对应日志中fields.AppName节点，查找时检索对应项目即可  
- 增加日志的控制台输出  
- 增加异常控制，在与es的通信异常或无法连接情况下，日志会执行文本文件记录，该文件位于：failures.txt

# 五、服务集成声明式API调用-DaprRPC
- 添加项目引用 ConcreteCloud.RPC.Dapr
- Startup->ConfigureServices->添加全家桶服务
``` C#
//添加DaprRPC
microService.AddDaprRPC((builder, opts) => { });
```
- 集成参考项目：ConcreteCloud.Production.sln

# 六、集成Dapr发布订阅
## 1、消息发布
- 添加项目引用 ConcreteCloud.EventBus
- 消息统一定义到 ConcreteCloud.EventBus.EventsLibrary 项目，并以文件夹的形式按照业务进行分组，消息类继承基类【IntegrationEvent】
- 集成项目作为生产者且不处理任何消息

- 在Startup->ConfigureServices->注册
``` C#
// 添加服务总线
services.AddEventBus(Configuration);
```
- 发布消息
``` C#
private readonly IDistributedEventBus busControl;
public demo(IDistributedEventBus busControl)
{
    this.busControl = busControl;
}

public void publish(){
     busControl.PublishAsync(new PlanModifyStatusMessage()
     {
            key_id = item.control_instance_code,
            template_code = item.warning_msg_code,
            status = 30
     });
}
```

## 2、消息订阅

- 集成项目作为消息消费者 asp.net core 项目集成 
- 在Startup->ConfigureServices->注册
``` C#
    // 添加消息订阅
    services.AddSubscriber();
```

- 消费者
``` C#
public class UpdateCustomerConsumer : IDistributedEventHandler<UpdateCustomerAddress>
{
    //当前租户信息
    private readonly ICurrentTenant _currentTenant;
    public UpdateCustomerConsumer(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    [TopicSub(typeof(UpdateCustomerAddress))]
    public async Task HandleEventAsync(UpdateCustomerAddress message)
    {
        await Console.Out.WriteLineAsync($"Updating customer: {message.CustomerId}");

        // update the customer address
    }
}
```

# 七、集成Yitter.IdGenerator雪花id
## 1、添加项目引用 Yitter.IdGenerator
- 类库位置在 \src\Utility\Yitter.IdGenerator
## 2、项目配置Yitter.IdGenerator
- 在Startup->ConfigureServices->注册
```c#
services.AddYitterIdGenerator(Configuration);
```
- 在Startup->Configure->注册
```C#
app.UseYitterIdGenerator();
```
- 配置文件，其中【WorkerIdCachingKey】需要改成对应服务的名称
```Json
  "IdGenerator": {
    "Yitter": {
      "WorkerIdCachingKey": "idgenerator:workerid:sale",
      "MaxWorkerId": 1024,
      "BaseTime": "2022-01-01 00:00:00"
    }
  }

```
## 3、使用Yitter.IdGenerator
- 生成long类型
```C#
YitIdHelper.NextId();
```
- 生成Guid类型
```C#
YitIdHelper.NewGuid();
```