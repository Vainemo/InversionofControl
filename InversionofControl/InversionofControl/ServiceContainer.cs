using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace InversionofControl
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceContainer:IServiceContainer
    {
        internal readonly IReadOnlyList<ServiceDefinition> _services;
        private readonly ConcurrentDictionary<ServiceKey, object> _singletonInstances;
        private readonly ConcurrentDictionary<ServiceKey, object> _scopedInstances;
        //线程安全的无序集合
        private ConcurrentBag<object> _trasientDisposables = new ConcurrentBag<object>();
        private readonly bool _isRootScope;
        private bool _disposed;
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            if(_isRootScope)
            {
                lock(_singletonInstances)
                {
                    if (_disposed)
                    {
                        return;
                    }
                }
                _disposed = true;
                foreach(var instance in _singletonInstances.Values)
                {
                    (instance as IDisposable)?.Dispose();
                }
                foreach(var instance in _trasientDisposables)
                {
                    (instance as IDisposable)?.Dispose(); 
                }
                _singletonInstances.Clear();
                _trasientDisposables =null;
            }
            else
            {
                lock(_scopedInstances)
                {
                    if (_disposed)
                    {
                        return;
                    }
                    _disposed= true;
                    foreach(var instance in _scopedInstances.Values)
                    {
                        (instance as IDisposable)?.Dispose(); 
                    }
                    foreach (var instance in _trasientDisposables)
                    {
                        (instance as IDisposable)?.Dispose();
                    }
                    _scopedInstances.Clear();
                    _trasientDisposables = null;
                }

            }
        }
        public ServiceContainer(ServiceContainer serviceContainer)
        {
            _isRootScope=false;
            _singletonInstances = serviceContainer._singletonInstances;
            _scopedInstances=new ConcurrentDictionary<ServiceKey, object>();
        }
        public ServiceContainer(IReadOnlyList<ServiceDefinition> serviceDefinitions)
        {
            _services = serviceDefinitions;
            _isRootScope = true;
            _singletonInstances = new ConcurrentDictionary<ServiceKey, object>();
        }
        
        

        public IServiceContainer CreateScope()
        {
           return new ServiceContainer(this);
        }
        private object GetServiceInstance(Type serviceType,ServiceDefinition serviceDefinition)
        {
            if (serviceDefinition.ImplementationInstance!=null)
            {
                return serviceDefinition.ImplementationInstance;
            }
            if (serviceDefinition.ImplementationInstanceFactory!=null)
            {
                return serviceDefinition.ImplementationInstanceFactory.Invoke(this);
            }
            var implementType = (serviceDefinition.ImplementType ?? serviceType);
            if (implementType.IsInterface||implementType.IsAbstract)
            {
                throw new InvalidOperationException($"注册服务无效,serviceType:{serviceType.FullName},implementType:{serviceDefinition.ImplementType}");
            }
            if (implementType.IsGenericType)
            {
                implementType = implementType.MakeGenericType(serviceType.GetGenericArguments());
            }
            var ctorInfos = implementType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            if (ctorInfos.Length==0)
            {
                throw new InvalidOperationException($"service{serviceType.FullName}没有任何公共构造函数");
            }
            ConstructorInfo ctor;
            if (ctorInfos.Length==0) 
            {
                ctor= ctorInfos[0];
            }
            else
            {
                ctor= ctorInfos.OrderBy(x=>x.GetParameters().Length).First();
            }
            var parameters=ctor.GetParameters();
            if (parameters.Length==0)
            {
                return Expression.Lambda<Func<object>>(Expression.New(ctor)).Compile().Invoke();
            }
            else
            {
                var ctorParams = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    var param = GetService(parameter.ParameterType);
                    //HasDefaultValue:获取一个值,指示此参数是否有默认值
                    if (param != null&&parameter.HasDefaultValue)
                    {
                        param = parameter.DefaultValue;
                    }
                    ctorParams[i] = parameter;
                }
                return Expression.Lambda<Func<object>>(Expression.New(ctor,ctorParams.Select(Expression.Constant))).Compile().Invoke();
            }
        }
        public object? GetService(Type serviceType)
        {
            if (_disposed)
            {
                throw new InvalidOperationException($"无法获取被释放的局部容器");
            }
            var serviceDefinition = _services.LastOrDefault(x => x.ServiceType == serviceType);
            if (null==serviceDefinition)
            {
                if (serviceType.IsGenericType)
                {
                    /*Type.GetGenericArguments方法:
                     *  该方法用于获取泛型类型的实际类型参数获取泛型集合里数据的具体类型
                     * Type.MakeGenericType方法:
                     *  该方法用于创建泛型类型的实例。该方法允许在运行时动态指定泛型类型的类型参数，以创建具体的泛型类型。
                     * Type.IsAssignableFrom方法:
                     *   该方法用于判断某个类型是不是另一个类型的派生类、实现接口或相同类型
                     */
                    var genericType = serviceType.GetGenericTypeDefinition();
                    serviceDefinition = _services.LastOrDefault(x => x.ServiceType == genericType);
                    if (null == serviceDefinition)
                    {
                        var innerServiceType = serviceType.GetGenericArguments().First();
                        if (typeof(IEnumerable<>).MakeGenericType(innerServiceType).IsAssignableFrom(serviceType))
                        {
                            var innerRegType = innerServiceType;
                            if (innerServiceType.IsGenericType)
                            {
                                innerRegType = innerServiceType.GetGenericTypeDefinition();
                            }
                            var list = new List<object>(4);
                            foreach (var def in _services.Where(x => x.ServiceType == innerServiceType))
                            {
                                object svc;
                                if (def.ServiceLifeTime == ServiceLifeTime.Singleton)
                                {
                                    svc = _scopedInstances.GetOrAdd(new ServiceKey(innerServiceType, def), (t) => GetServiceInstance(innerServiceType, def));
                                }
                                else if (def.ServiceLifeTime == ServiceLifeTime.Scoped)
                                {
                                    svc = _scopedInstances.GetOrAdd(new ServiceKey(innerServiceType, def), (t) => GetServiceInstance(innerServiceType, def));
                                }
                                else
                                {
                                    svc = GetServiceInstance(innerServiceType, def);
                                    if (svc is IDisposable)
                                    {
                                        _trasientDisposables.Add(svc);
                                    }
                                }
                                if (null != svc)
                                {
                                    list.Add(svc);
                                }
                            }
                            var methodInfo = typeof(Enumerable).GetMethod("Cast", BindingFlags.Static | BindingFlags.Public);
                            if (methodInfo != null)
                            {
                                var genericMethod = methodInfo.MakeGenericMethod(innerRegType);
                                var castedValue = genericMethod.Invoke(null, new object[] { list });
                                if (typeof(IEnumerable<>).MakeGenericType(innerServiceType) == serviceType)
                                {
                                    return castedValue;
                                }
                                var toArrayMethod = typeof(IEnumerable<>).GetMethod(" ToArray", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(innerServiceType);
                                return toArrayMethod.Invoke(null, new object[] { list });
                            }
                            return list;
                        }
                        return null;
                    }

                    
                }
                else
                {
                    return null;
                }
            }
            if (_isRootScope&&serviceDefinition.ServiceLifeTime==ServiceLifeTime.Scoped)
            {
                throw new InvalidOperationException($"can not get scope service from the root scope,ServiceType:{serviceType.FullName}");
            }
            if(serviceDefinition.ServiceLifeTime==ServiceLifeTime.Singleton)
            {
                var svc = _singletonInstances.GetOrAdd(new ServiceKey(serviceType, serviceDefinition), (t) => GetServiceInstance(t.ServiceType, serviceDefinition));
                return svc;
            }
            else if(serviceDefinition.ServiceLifeTime == ServiceLifeTime.Scoped)
            {
                var svc = _scopedInstances.GetOrAdd(new ServiceKey(serviceType, serviceDefinition), (t) => GetServiceInstance(t.ServiceType, serviceDefinition));
                return svc;
            }
            else
            {
                var svc=GetServiceInstance(serviceType, serviceDefinition);
                if (svc is IDisposable)
                {
                    _trasientDisposables.Add(svc);
                }
                return svc;
            }
        }
       
    }

}
