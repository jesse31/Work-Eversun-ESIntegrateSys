using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using ESIntegrateSys.Services.ManpowerAllocationServices;

namespace ESIntegrateSys
{
    /// <summary>
    /// 依賴注入配置類
    /// </summary>
    public class DependencyConfig
    {
        /// <summary>
        /// 註冊依賴注入
        /// </summary>
        public static void RegisterDependencies()
        {
            // 創建依賴解析器
            var dependencyResolver = new CustomDependencyResolver();

            // 自動註冊所有服務
            AutoRegisterServices(dependencyResolver);

            // 設置 MVC 依賴解析器
            DependencyResolver.SetResolver(dependencyResolver);
        }

        /// <summary>
        /// 自動註冊所有服務
        /// </summary>
        /// <param name="resolver">依賴解析器</param>
        private static void AutoRegisterServices(CustomDependencyResolver resolver)
        {
            // 獲取當前程序集
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith("ESIntegrateSys"))
                .ToList();

            foreach (var assembly in assemblies)
            {
                // 獲取所有接口
                var interfaces = assembly.GetTypes()
                    .Where(t => t.IsInterface && t.Name.StartsWith("I") && t.Name.EndsWith("Services"))
                    .ToList();

                foreach (var interfaceType in interfaces)
                {
                    // 查找實現該接口的類
                    var implementationType = assembly.GetTypes()
                        .FirstOrDefault(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t));

                    if (implementationType != null)
                    {
                        // 使用泛型方法註冊接口和實現
                        MethodInfo registerMethod = typeof(CustomDependencyResolver).GetMethod("Register");
                        MethodInfo genericRegisterMethod = registerMethod.MakeGenericMethod(interfaceType, implementationType);
                        genericRegisterMethod.Invoke(resolver, null);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 自定義依賴解析器
    /// </summary>
    public class CustomDependencyResolver : IDependencyResolver
    {
        private readonly Dictionary<Type, Type> _typeMap = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        /// <summary>
        /// 註冊介面和實作
        /// </summary>
        /// <typeparam name="TInterface">介面類型</typeparam>
        /// <typeparam name="TImplementation">實作類型</typeparam>
        public void Register<TInterface, TImplementation>() where TImplementation : TInterface
        {
            _typeMap[typeof(TInterface)] = typeof(TImplementation);
        }

        /// <summary>
        /// 註冊介面和實作（非泛型版本）
        /// </summary>
        /// <param name="interfaceType">介面類型</param>
        /// <param name="implementationType">實作類型</param>
        public void RegisterType(Type interfaceType, Type implementationType)
        {
            _typeMap[interfaceType] = implementationType;
        }

        /// <summary>
        /// 解析服務
        /// </summary>
        /// <param name="serviceType">服務類型</param>
        /// <returns>服務實例</returns>
        public object GetService(Type serviceType)
        {
            // 檢查是否已存在實例（單例模式）
            if (_instances.ContainsKey(serviceType))
            {
                return _instances[serviceType];
            }

            // 檢查是否有註冊的實現類型
            if (_typeMap.ContainsKey(serviceType))
            {
                Type implementationType = _typeMap[serviceType];
                var instance = CreateInstance(implementationType);

                // 儲存實例（實現單例模式）
                _instances[serviceType] = instance;
                return instance;
            }

            // 如果是控制器類型，嘗試創建實例並注入依賴
            if (serviceType.IsClass && !serviceType.IsAbstract && typeof(IController).IsAssignableFrom(serviceType))
            {
                return CreateInstance(serviceType);
            }

            return null;
        }

        /// <summary>
        /// 創建實例並注入依賴
        /// </summary>
        /// <param name="type">要創建的類型</param>
        /// <returns>創建的實例</returns>
        private object CreateInstance(Type type)
        {
            // 獲取所有構造函數
            var constructors = type.GetConstructors();
            if (constructors.Length == 0)
            {
                // 如果沒有公共構造函數，使用默認構造函數
                return Activator.CreateInstance(type);
            }

            // 選擇參數最多的構造函數（依賴注入優先）
            var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
            var parameters = constructor.GetParameters();

            if (parameters.Length == 0)
            {
                // 如果沒有參數，直接創建實例
                return Activator.CreateInstance(type);
            }

            // 解析構造函數的所有參數
            var parameterInstances = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameterInstances[i] = GetService(parameters[i].ParameterType);
            }

            // 使用解析的參數創建實例
            return Activator.CreateInstance(type, parameterInstances);
        }

        /// <summary>
        /// 解析服務集合
        /// </summary>
        /// <param name="serviceType">服務類型</param>
        /// <returns>服務實例集合</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new List<object>();
        }
    }
}
