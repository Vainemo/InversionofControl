using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InversionofControl
{
    /// <summary>
    /// 服务注册定义
    /// </summary>
    public class ServiceDefinition
    {
        /// <summary>
        /// 服务生命周期
        /// </summary>
        public ServiceLifeTime ServiceLifeTime { get; }
        /// <summary>
        /// 实现类型
        /// </summary>
        public Type ImplementType { get; }
        /// <summary>
        /// 服务类型
        /// </summary>
        public Type ServiceType { get; }
        /// <summary>
        /// 实现实例
        /// </summary>
        public object ImplementationInstance { get; }
        /// <summary>
        /// 实现工厂
        /// </summary>
        public Func<IServiceContainer, object> ImplementationInstanceFactory { get; }
        /// <summary>
        /// 获取实现类型
        /// </summary>
        /// <returns></returns>
        public Type GetImplementType()
        {
            if (ImplementationInstance != null)
            {
                return ImplementationInstance.GetType();
            }
            if (ImplementationInstanceFactory != null)
            {
                return ImplementationInstanceFactory.Method.DeclaringType;
            }
            if (ImplementType != null)
            {
                return ImplementType;
            }
            return ServiceType;
        }
        //初始化服务
        public ServiceDefinition(object instance, Type serviceType)
        {
            ImplementationInstance = instance;
            ServiceType = serviceType;
            ServiceLifeTime = ServiceLifeTime.Singleton;
        }
        public ServiceDefinition(Type serviceType, Type implementType, ServiceLifeTime serviceLifeTime)
        {
            ServiceType = serviceType;
            ImplementType = implementType ?? serviceType;
            ServiceLifeTime = serviceLifeTime;
        }
        public ServiceDefinition(Type serviceType, ServiceLifeTime serviceLifeTime) : this(serviceType, serviceType, serviceLifeTime)
        {

        }
        public ServiceDefinition(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifeTime serviceLifeTime)
        {
            ServiceType = serviceType;
            ImplementationInstanceFactory = factory;
            ServiceLifeTime = serviceLifeTime;
        }
        public static ServiceDefinition Singleton<TService>(Func<IServiceProvider, object> factory)
        {
            return new ServiceDefinition(typeof(TService), factory, ServiceLifeTime.Singleton);
        }
        public static ServiceDefinition Scoped<TService>(Func<IServiceProvider, object> factory)
        {
            return new ServiceDefinition(typeof(TService), factory, ServiceLifeTime.Scoped);
        }
        public static ServiceDefinition Transient<TService>(Func<IServiceProvider, object> factory)
        {
            return new ServiceDefinition(typeof(TService), factory, ServiceLifeTime.Transient);
        }
        public static ServiceDefinition Singleton<TService>()
        {
            return new ServiceDefinition(typeof(TService), ServiceLifeTime.Singleton);
        }
        public static ServiceDefinition Scoped<TService>()
        {
            return new ServiceDefinition(typeof(TService), ServiceLifeTime.Scoped);
        }
        public static ServiceDefinition Transient<TService>()
        {
            return new ServiceDefinition(typeof(TService), ServiceLifeTime.Transient);
        }
        public static ServiceDefinition Singleton<Tservice, TserviceImplement>() where TserviceImplement:Tservice
        {
            return new ServiceDefinition(typeof(Tservice), typeof(TserviceImplement),  ServiceLifeTime.Singleton);
        }
        public static ServiceDefinition Scoped<Tservice, TserviceImplement>() where TserviceImplement : Tservice
        {
            return new ServiceDefinition(typeof(Tservice), typeof(TserviceImplement), ServiceLifeTime.Scoped);
        }
        public static ServiceDefinition Transient<Tservice, TserviceImplement>() where TserviceImplement : Tservice
        {
            return new ServiceDefinition(typeof(Tservice), typeof(TserviceImplement), ServiceLifeTime.Transient);
        }

    }
}
