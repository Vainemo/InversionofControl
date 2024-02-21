using System;
using System.Collections;
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
        /// <summary>
        /// 类型容器
        /// </summary>
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
            _services= serviceContainer._services;
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
        private object? EnrichObject(object? obj)
        {
            if(obj is not null)
            {
                var type=obj.GetType();
                foreach(var property in CacheUtil.GetTypeProperties(type).Where(x=>x.IsDefined(typeof(FromServiceAttribute))))
                {
                    if (property.GetValueGetter()?.Invoke(obj)==null)
                    {
                        property.GetValueSetter()?.Invoke(obj,GetService(property.PropertyType));
                    }
                }
            }
            return obj; 
        }
        /// <summary>
        /// 获得服务实例对象
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="serviceDefinition"></param>
        /// <returns></returns>
        private object? GetServiceInstance(Type serviceType,ServiceDefinition serviceDefinition)
        {
            return EnrichObject(GetServiceInstanceInternal(serviceType, serviceDefinition));
        }
        private object GetServiceInstanceInternal(Type serviceType,ServiceDefinition serviceDefinition)
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
            //implementType.IsGenericType:表示该类型是否是泛型类型
            if (implementType.IsGenericType)
            {
                implementType = implementType.MakeGenericType(serviceType.GetGenericArguments());
            }
            var newFunc = CacheUtil.TypeNewFuncCache.GetOrAdd(implementType, (serviceContainer) =>
            {
                if (CacheUtil.TypeEmptyConstructorFuncCache.TryGetValue(implementType,out var emptyFunc))
                {
                    return emptyFunc.Invoke();
                }
                var ctor = CacheUtil.TypeConstructorCache.GetOrAdd(implementType, t =>
                {
                    //GetConstructors():获取当前 Type 的构造函数
                    var ctorInfos =t.GetConstructors(BindingFlags.Instance|BindingFlags.Public);
                    if (ctorInfos.Length==0)
                    {
                        return null;
                    }
                    ConstructorInfo ctorInfo;
                    if (ctorInfos.Length==1)
                    {
                        ctorInfo = ctorInfos[0];
                    }
                    else
                    {
                        ctorInfo=ctorInfos.FirstOrDefault(x=>x.IsDefined(typeof(ServiceConstructorAttribute)))??ctorInfos.OrderBy(x=>x.GetParameters().Length).First();
                    }
                    return ctorInfo;
                });
                if (ctor==null)
                {
                    throw new InvalidOperationException($"service{serviceType.FullName}没有任何公共构造函数");
                }
                var parameters = ctor.GetParameters();
                if (parameters.Length==0)
                {
                    var func00=Expression.Lambda<Func<object>>(Expression.New(ctor)).Compile();
                    CacheUtil.TypeEmptyConstructorFuncCache.TryAdd(implementType, func00);
                    return func00.Invoke();
                }
                var ctorParams = new object?[parameters.Length];
                for(var index=0;index<parameters.Length;index++)
                {
                    var param = serviceContainer.GetService(parameters[index].ParameterType);
                    if (param != null && parameters[index].HasDefaultValue)
                    {
                        param = parameters[index].DefaultValue;
                    }
                    ctorParams[index] = param;
                }
                var func = CacheUtil.TypeConstructorFuncCache.GetOrAdd(implementType, t =>
                {
                    if(!CacheUtil.TypeConstructorCache.TryGetValue(t,out var ctorInfo)||ctorInfo is null)
                    {
                        return null;
                    }
                    var innerParameters=ctorInfo.GetParameters();
                    var parameterExpression = Expression.Parameter(typeof(object[]),"arguments");
                    var argExpressions=new Expression[innerParameters.Length];
                    for (int i = 0; i < innerParameters.Length; i++)
                    {
                        var indexedAccess = Expression.ArrayIndex(parameterExpression, Expression.Constant(i));
                        if (!innerParameters[i].ParameterType.IsClass)
                        {
                            var localVaruable = Expression.Variable(innerParameters[i].ParameterType, "loacalVariable");
                            var block = Expression.Block(new[] { localVaruable },
                                Expression.IfThenElse(Expression.Equal(indexedAccess, Expression.Constant(null)),
                                Expression.Assign(localVaruable, Expression.Default(innerParameters[i].ParameterType)),
                                Expression.Assign(localVaruable, Expression.Convert(indexedAccess, innerParameters[i].ParameterType))
                                ),
                                localVaruable);
                            argExpressions[i] = block;
                        }
                        else
                        {
                            argExpressions[i] = Expression.Convert(indexedAccess, innerParameters[i].ParameterType);
                        }
                    }
                    var newExpressipn = Expression.New(ctor, argExpressions);
                    return Expression.Lambda<Func<object?[],object>>(newExpressipn,parameterExpression).Compile();  
                });
                return func.Invoke(ctorParams);
            });
            return newFunc?.Invoke(this);
        }
        /// <summary>
        /// 获取实例对象
        /// </summary>
        /// <param name="serviceType">所要实例化的构造方法的参数类型</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public object? GetService(Type serviceType)
        {
            if (_disposed)
            {
                throw new InvalidOperationException($"无法获取被释放的局部容器");
            }
            //判断容器中是否已经存在目标对象
            var serviceDefinition = _services.LastOrDefault(x => x.ServiceType == serviceType);
            if (null==serviceDefinition)
            {
                if (serviceType.IsGenericType)
                {
                    /*Type.GetGenericArguments方法:
                     *  该方法用于获取泛型类型的实际类型参数获取泛型集合里数据的具体类型(List<int>中的"int")
                     * Type.MakeGenericType方法:
                     *  该方法用于创建泛型类型的实例。该方法允许在运行时动态指定泛型类型的类型参数，以创建具体的泛型类型。
                     * Type.IsAssignableFrom方法:
                     *   该方法用于判断某个类型是不是另一个类型的派生类、实现接口或相同类型
                     */
                    //GetGenericTypeDefinition():返回一个表示可用于构造当前泛型类型的泛型类型定义的 Type 对象。
                    var genericType = serviceType.GetGenericTypeDefinition();
                    serviceDefinition = _services.LastOrDefault(x => x.ServiceType == genericType);
                    if (null == serviceDefinition)
                    {
                        ///需要注入的类型(GenericServiceTest<T>)
                        var innerServiceType = serviceType.GetGenericArguments().First();
                        //serviceType(IEnumerable)
                        if (typeof(IEnumerable<>).MakeGenericType(innerServiceType).IsAssignableFrom(serviceType))
                        {
                            var innerRegType = innerServiceType;
                            //如果innerServiceType是一个泛型类
                            if (innerServiceType.IsGenericType)
                            {
                                //拿到具体的类型
                                innerRegType = innerServiceType.GetGenericTypeDefinition();
                            }
                            var list = new List<object>(4);
                            foreach (var def in _services.Where(x => x.ServiceType == innerRegType))
                            {
                                //要注入的参数
                                object? svc;
                                if (def.ServiceLifeTime == ServiceLifeTime.Singleton)
                                {
                                    svc = _singletonInstances.GetOrAdd(new ServiceKey(innerServiceType, def), (t) => GetServiceInstance(innerServiceType, def));
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
                            //Enumerable.Cast():方法用于将一个实现 IEnumerable 接口的集合（或集合中的元素类型是 object）转换为指定类型 T 的集合。
                            var methodInfo = typeof(Enumerable).GetMethod("Cast", BindingFlags.Static | BindingFlags.Public);
                            if (methodInfo != null)
                            {
                                //MakeGenericMethod():用类型数组的元素替代当前泛型方法定义的类型参数，并返回表示结果构造方法的 MethodInfo 对象(将Cast泛型替换成指定类型)
                                var genericMethod = methodInfo.MakeGenericMethod(innerServiceType);
                                var castedValue = genericMethod.Invoke(null, new object[] { list });
                                if (typeof(IEnumerable<>).MakeGenericType(innerServiceType) == serviceType)
                                {
                                    return castedValue;
                                }
                                var toArrayMethod = typeof(Enumerable).GetMethod("ToArray", BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(innerServiceType);
                                return toArrayMethod?.Invoke(null, new object[] { castedValue });
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
            var svc1=GetServiceInstance(serviceType, serviceDefinition);
            if (svc1 is IDisposable)
            {
              _trasientDisposables.Add(svc1);
            }
            return svc1;
        }
       
    }

}
