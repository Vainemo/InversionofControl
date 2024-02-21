using System.Diagnostics.CodeAnalysis;

namespace InversionofControl
{
    /// <summary>
    /// 
    /// </summary>
    public static class StaticServiceHelper
    {
        public static Tservice ResolveService<Tservice>([NotNull] this IServiceProvider serviceProvider)
        {
            return (Tservice)serviceProvider.GetService(typeof(Tservice));
        }
        public static Tservice ResolveRequiredService<Tservice>([NotNull] this IServiceProvider serviceProvider)
        {
            var serviceType=typeof(Tservice);
            var svc = serviceProvider.GetService(serviceType);
            if (null==svc)
            {
                throw new InvalidOperationException($"service had not been registered, serviceType: {serviceType}");
            }
            return (Tservice)svc;
        }
        public static IEnumerable<Tservice> ResolveServices<Tservice>([NotNull]this IServiceProvider serviceProvider)
        {
            return serviceProvider.ResolveService<IEnumerable<Tservice>>();
        }
        public static IServiceContainerBuilder AddSingleton(this IServiceContainerBuilder builder,Type serviceType)
        {
            builder.Add(new ServiceDefinition(serviceType, ServiceLifeTime.Singleton));
            return builder;
        }
        public static IServiceContainerBuilder AddSingleton(this IServiceContainerBuilder builder, Type serviceType,Type implementType)
        {
            builder.Add(new ServiceDefinition(serviceType,implementType,ServiceLifeTime.Singleton));
            return builder;
        }
        public static IServiceContainerBuilder AddSingleton<TService>(this IServiceContainerBuilder builder,Func<IServiceProvider,object> func)
        {
            builder.Add(ServiceDefinition.Singleton<TService>(func));
            return builder;
        }
        public static IServiceContainerBuilder AddSingleton<Tservice>(this IServiceContainerBuilder builder)
        {
            builder.Add(ServiceDefinition.Singleton<Tservice>());
            return builder;
        }
        public static IServiceContainerBuilder AddSingleton<Tservice,TServiceImpLement>(this IServiceContainerBuilder builder) where TServiceImpLement:Tservice
        {
            builder.Add(ServiceDefinition.Singleton<Tservice,TServiceImpLement>());
            return builder;
        }
        public static IServiceContainerBuilder AddScoped(this IServiceContainerBuilder builder,Type serviceType)
        {
            builder.Add(new ServiceDefinition(serviceType, ServiceLifeTime.Scoped));
            return builder;
        }
        public static IServiceContainerBuilder AddScoped(this IServiceContainerBuilder builder, Type serviceType, Type implementType )
        {
            builder.Add(new ServiceDefinition(serviceType,implementType, ServiceLifeTime.Scoped));
            return builder;
        }
        public static IServiceContainerBuilder AddScoped<Tservice>(this IServiceContainerBuilder builder,Func<IServiceProvider,object> func)
        {
            builder.Add(ServiceDefinition.Scoped<Tservice>(func));
            return builder;
        }
        public static IServiceContainerBuilder AddScoped<Tservice>(this IServiceContainerBuilder builder)
        {
            builder.Add(ServiceDefinition.Scoped<Tservice>());
            return builder;
        }
        public static IServiceContainerBuilder AddScoped<Tservice, IServiceImplement>(this IServiceContainerBuilder builder)where IServiceImplement:Tservice
        {
            builder.Add(ServiceDefinition.Scoped<Tservice,IServiceImplement>());
            return builder;
        }
        public static IServiceContainerBuilder AddTransient<Tservice, IServiceImplement>(this IServiceContainerBuilder builder) where IServiceImplement : Tservice
        {
            builder.Add(ServiceDefinition.Transient<Tservice, IServiceImplement>());
            return builder;
        }

        public static IServiceContainerBuilder AddTransient<Tservice>(this IServiceContainerBuilder builder, Func<IServiceProvider, object> func)
        {
            builder.Add(ServiceDefinition.Transient<Tservice>(func));
            return builder;
        }
        public static IServiceContainerBuilder AddTransient<Tservice>(this IServiceContainerBuilder builder)
        {
            builder.Add(ServiceDefinition.Transient<Tservice>());
            return builder;
        }
        public static IServiceContainerBuilder AddTransient(this IServiceContainerBuilder builder, Type serviceType)
        {
            builder.Add(new ServiceDefinition(serviceType, ServiceLifeTime.Transient));
            return builder;
        }
        public static IServiceContainerBuilder AddTransient(this IServiceContainerBuilder builder,Type serviceType,Type implementType)
        {
            builder.Add(new ServiceDefinition(serviceType, implementType,ServiceLifeTime.Transient));
            return builder;
        }
        public static IServiceContainerBuilder TryAddSingleton(this IServiceContainerBuilder builder, Type serviceType)
        {
            builder.TryAdd(new ServiceDefinition(serviceType, ServiceLifeTime.Singleton));
            return builder;
        }
        public static IServiceContainerBuilder TryAddSingleton(this IServiceContainerBuilder builder, Type serviceType, Type implementType)
        {
            builder.TryAdd(new ServiceDefinition(serviceType, implementType, ServiceLifeTime.Singleton));
            return builder;
        }
        public static IServiceContainerBuilder TryAddSingleton<TService>(this IServiceContainerBuilder builder, Func<IServiceProvider, object> func)
        {
            builder.TryAdd(ServiceDefinition.Singleton<TService>(func));
            return builder;
        }
        public static IServiceContainerBuilder TryAddSingleton<Tservice>(this IServiceContainerBuilder builder)
        {
            builder.TryAdd(ServiceDefinition.Singleton<Tservice>());
            return builder;
        }
        public static IServiceContainerBuilder TryAddSingleton<Tservice, TServiceImpLement>(this IServiceContainerBuilder builder) where TServiceImpLement : Tservice
        {
            builder.TryAdd(ServiceDefinition.Singleton<Tservice, TServiceImpLement>());
            return builder;
        }
        public static IServiceContainerBuilder TryAddScoped(this IServiceContainerBuilder builder, Type serviceType)
        {
            builder.TryAdd(new ServiceDefinition(serviceType, ServiceLifeTime.Scoped));
            return builder;
        }
        public static IServiceContainerBuilder TryAddScoped(this IServiceContainerBuilder builder, Type serviceType, Type implementType)
        {
            builder.TryAdd(new ServiceDefinition(serviceType, implementType, ServiceLifeTime.Scoped));
            return builder;
        }
        public static IServiceContainerBuilder TryAddScoped<Tservice>(this IServiceContainerBuilder builder, Func<IServiceProvider, object> func)
        {
            builder.TryAdd(ServiceDefinition.Scoped<Tservice>(func));
            return builder;
        }
        public static IServiceContainerBuilder TryAddScoped<Tservice>(this IServiceContainerBuilder builder)
        {
            builder.TryAdd(ServiceDefinition.Scoped<Tservice>());
            return builder;
        }
        public static IServiceContainerBuilder TryAddScoped<Tservice, IServiceImplement>(this IServiceContainerBuilder builder) where IServiceImplement : Tservice
        {
            builder.TryAdd(ServiceDefinition.Scoped<Tservice, IServiceImplement>());
            return builder;
        }
        public static IServiceContainerBuilder TryAddTransient<Tservice, IServiceImplement>(this IServiceContainerBuilder builder) where IServiceImplement : Tservice
        {
            builder.TryAdd(ServiceDefinition.Transient<Tservice, IServiceImplement>());
            return builder;
        }

        public static IServiceContainerBuilder TryAddTransient<Tservice>(this IServiceContainerBuilder builder, Func<IServiceProvider, object> func)
        {
            builder.TryAdd(ServiceDefinition.Transient<Tservice>(func));
            return builder;
        }
        public static IServiceContainerBuilder TryAddTransient<Tservice>(this IServiceContainerBuilder builder)
        {
            builder.TryAdd(ServiceDefinition.Transient<Tservice>());
            return builder;
        }
        public static IServiceContainerBuilder TryAddTransient(this IServiceContainerBuilder builder, Type serviceType)
        {
            builder.TryAdd(new ServiceDefinition(serviceType, ServiceLifeTime.Transient));
            return builder;
        }
        public static IServiceContainerBuilder TryAddTransient(this IServiceContainerBuilder builder, Type serviceType, Type implementType)
        {
            builder.TryAdd(new ServiceDefinition(serviceType, implementType, ServiceLifeTime.Transient));
            return builder;
        }

        public static bool TryGetService(this IDependencyResolver resolver,Type serviceType,out object? service)
        {
            try
            {
                service = resolver.GetService(serviceType);
                return service != null;
            }
            catch(Exception ex) 
            {
                service = null;
                InvokeHelper.OnInvokeException?.Invoke(ex);
                return false;
            }
        }
        public static bool TryResolveService<TService>(this IDependencyResolver dependencyResolver,out TService? service)
        {
            var result = dependencyResolver.TryGetService(typeof(TService), out var serviceObj);
            if (result)
            {
                service = (TService)serviceObj!;
            }
            else
            {
                service = default;
            }
            return result;
        }

    }
    public interface IDependencyResolver:IServiceProvider
    {
        IEnumerable<object> GetServices(Type serviceType);
        bool TryInvokeService<Tservice>(Action<Tservice> action);
        Task<bool> TryInvokeServiceAsyn<Tservice>(Func<Tservice, Task> action);
    }
}
