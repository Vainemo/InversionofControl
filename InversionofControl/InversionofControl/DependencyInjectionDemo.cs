using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static System.Formats.Asn1.AsnWriter;

namespace InversionofControl
{
    /// <summary>
    /// 依赖注入
    /// </summary>
    public class DependencyInjectionDemo : IDisposable
    {
        /// <summary>
        /// DI容器
        /// </summary>
        private readonly IServiceContainer _container;
        public void Dispose()
        {
            _container.Dispose();
        }
        public DependencyInjectionDemo()
        {
            var containerBuilder = new SeviceContainerBuilder();
            containerBuilder.AddSingleton<IConfiguration>(x=>new ConfigurationBuilder().Build());
            containerBuilder.AddScoped<IFly, MonkeyKing>();
            containerBuilder.AddScoped<HasDependencyTest1>();
            containerBuilder.AddScoped<HasDependencyTest2>();
            containerBuilder.AddScoped<HasDependencyTest3>();
            containerBuilder.AddScoped(typeof(HasDependencyTest4<>));
            containerBuilder.AddTransient<WuKong>();
            containerBuilder.AddScoped<BaJie>(x => new BaJie());
            containerBuilder.AddSingleton(typeof(GenericServiceTest<>));
            _container = containerBuilder.Build();
        }
        /// <summary>
        /// [Fact] 是 xUnit.net 测试框架中的一个特性，用于标记测试方法。它指示测试运行器该方法是一个测试方法，并应该被执行和验证
        /// </summary>
        [Fact]
        public void Test()
        {
            var rootConfig=_container.ResolveService<IConfiguration>();
            Assert.Throws<InvalidOperationException>(()=>_container.ResolveService<IFly>());
            Assert.Throws<InvalidOperationException>(() => _container.ResolveRequiredService<IDependencyResolver>());
        }
    }
    public interface IServiceContainer : IScope, IServiceProvider
    {
        IServiceContainer CreateScope();
    }
    public interface IScope : IDisposable
    {
    }
    public class WuKong:IDisposable
    {
        public WuKong()
        {
            Console.WriteLine("猴王降世");
        }
        public void Jump()
        {
            Console.WriteLine("筋斗云");
        }
        public void Dispose()
        {
            Console.WriteLine("归隐山林");
        }
    }
    public class BaJie : IDisposable
    {
        public BaJie()
        {
            Console.WriteLine("转世投胎");
        }
        public void Eat()
        {
            Console.WriteLine("八戒吃西瓜");
        }
        public void Dispose()
        {
            Console.WriteLine("八戒归隐山林");
        }
    }
    public interface IFly
    {
        string Name { get; }
        void Fly();
    }
    public class MonkeyKing : IFly, IDisposable
    {
        public string Name => "美猴王";

        public void Dispose()
        {
            Console.WriteLine($"{Name}归隐山林");
        }

        public void Fly()
        {
            Console.WriteLine($"{Name}驾云");
        }
    }
    public class Superman : IFly, IDisposable
    {
        public string Name => "超人";

        public void Dispose()
        {
            Console.WriteLine($"{Name}归隐山林");
        }

        public void Fly()
        {
            Console.WriteLine($"{Name}飞行");
        }
    }
    public class GenericServiceTest<T>
    {
        
        public void Test()
        {
            Console.WriteLine($"generic type:{typeof(T).FullName}");
        }
    }
    public class HasDependencyTest1
    {
        private readonly IReadOnlyCollection<IFly> _flys;
        public HasDependencyTest1(IEnumerable<IFly> flys)
        {
            _flys = flys.ToArray();
        }
        public void Test()
        {
            Console.WriteLine($"test in {nameof(HasDependencyTest1)}");
            foreach(IFly fly in _flys)
            {
                fly.Fly();
            }
        }
    }
    public class HasDependencyTest2
    {
        private readonly IReadOnlyCollection<IFly> _flys;
        public HasDependencyTest2(IReadOnlyCollection<IFly> flys)
        {
            _flys = flys.ToArray();
        }
        public void Test()
        {
            Console.WriteLine($"test in {nameof(HasDependencyTest2)}");
            foreach (IFly fly in _flys)
            {
                fly.Fly();
            }
        }
    }
    public class HasDependencyTest3
    {
        private readonly IReadOnlyCollection<GenericServiceTest<int>> _svcs;
        public HasDependencyTest3(IEnumerable<GenericServiceTest<int>> svcs)
        {
            _svcs = svcs.ToArray();
        }
        public void Test()
        {
            Console.WriteLine($"test in {nameof(HasDependencyTest3)}");
            foreach (var item in _svcs)
            {
                item.Test();
            }
        }
    }
    public class HasDependencyTest4<T>
    {
        private readonly IReadOnlyCollection<GenericServiceTest<T>> _svcs;
        public HasDependencyTest4(IEnumerable<GenericServiceTest<T>> svcs)
        {
            _svcs = svcs.ToArray();
        }
        public void Test()
        {
            Console.WriteLine($"test in {GetType().FullName}");
            foreach (var item in _svcs)
            {
                item.Test();
            }
        }
    }
}
