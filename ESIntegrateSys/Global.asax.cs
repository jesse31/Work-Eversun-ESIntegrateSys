using ESIntegrateSys.Models;
using ESIntegrateSys.Services.ManpowerAllocationServices;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;

namespace ESIntegrateSys
{
    public class MvcApplication : System.Web.HttpApplication
    {

        private BackgroundJobServer _backgroundJobServer; // Hangfire 伺服器實例


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            #region
            var builder = new ContainerBuilder();

            // 自動註冊所有 Controller
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            // 自動註冊所有 Service（命名空間下所有型別）
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.Namespace != null && t.Namespace.Contains("Services"))
                .AsImplementedInterfaces()
                .InstancePerRequest();

            // 註冊資料庫物件
            builder.RegisterType<ESIntegrateSysEntities>().InstancePerRequest();

            // 建立容器並設為 MVC 依賴解析器
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            #endregion

            // DependencyConfig.RegisterDependencies(); // 已由 Autofac 取代



            #region Delete Lod Log
            // --- Hangfire 設定開始 ---
            try
            {
                // 1. 設定 Hangfire 使用 SQL Server 儲存
                //    請將 "YourConnectionStringName" 替換為您 Web.config 中的資料庫連線字串名稱
                //    或者直接提供完整的連線字串
                string hangfireConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ESIntegrateSys"].ConnectionString;
                GlobalConfiguration.Configuration.UseSqlServerStorage(hangfireConnectionString);

                // 2. 啟動 Hangfire 伺服器
                _backgroundJobServer = new BackgroundJobServer();

                // 3. 註冊週期性作業來刪除舊日誌
                //    "delete-old-logs-quarterly" 是此作業的唯一ID，您可以自訂
                //    Cron "0 0 1 */3 *" 表示每三個月的第一天午夜執行
                //    (例如 1月1日, 4月1日, 7月1日, 10月1日 的 00:00)
                RecurringJob.AddOrUpdate<ManpowerAllocationServices>(
                    "delete-old-logs-quarterly", // 作業 ID
                    service => service.DeleteLogsOlderThanThreeMonths(), // 要執行的方法
                    "0 0 1 */3 *", // Cron 表示式
                    TimeZoneInfo.Local // 使用伺服器的本地時區
                );
                // Cron 解釋:
                // 0: 分鐘 (0)
                // 0: 小時 (0 - 午夜)
                // 1: 每月的第幾天 (1號)
                // */3: 每隔三個月 (每三個月執行一次)
                // *: 星期幾 (任何)

                Console.WriteLine("Hangfire 已成功設定並啟動，週期性刪除日誌作業已註冊。");
            }
            catch (Exception ex)
            {
                // 記錄 Hangfire 設定錯誤
                Console.WriteLine($"Hangfire 設定失敗: {ex.Message}");
                // 您可能需要更正式的日誌記錄機制
            }
            // --- Hangfire 設定結束 ---
        }

        protected void Application_End()
        {
            // 當應用程式關閉時，優雅地停止 Hangfire 伺服器
            _backgroundJobServer?.Dispose();
        }
        #endregion
    }
}

