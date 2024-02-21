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


    /*   IOC Inversion of Control 控制反转:
     *      1.IOC意味着将你设计好的对象交给容器控制，而不是传统的在你的对象内部直接控制。
     *      什么是反转? 依赖对象的获取被反转了
     *   DI—Dependency Injection 依赖注入:
     *      件之间依赖关系由容器在运行期决定，形象的说，即由容器动态的将某个依赖关系注入到组件之中。
     *      依赖注入的目的并非为软件系统带来更多功能，而是为了提升组件重用的频率，并为系统搭建一个灵活、可扩展的平台
     *   理解DI的四个关键问题:
     *      ●依赖于谁：当然是应用程序依赖于 IoC 容器；

　　        ●为什么需要依赖：应用程序需要 IoC 容器来提供对象需要的外部资源；

　　        ●谁注入谁：很明显是 IoC 容器注入应用程序里依赖的对象；

　　        ●注入了什么：就是注入某个对象所需要的外部资源/依赖。

         IOC和DI的关系:依赖注入是控制反转设计思想的一种实现
     */
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
            containerBuilder.AddScoped<IFly, Superman>();
            containerBuilder.AddScoped<Test>();
            containerBuilder.AddScoped<HasDependencyTest>();
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
            /*   Assert.Throws 是一个断言方法，用于在单元测试中验证特定的异常是否被抛出。
             * 它允许你编写测试代码来验证在执行某个操作时是否会引发期望的异常。
             */
            //Assert.Throws<InvalidOperationException>(()=>_container.ResolveService<IFly>());
            //Assert.Throws<InvalidOperationException>(() => _container.ResolveRequiredService<IDependencyResolver>());
            using(var scope=_container.CreateScope())
            {
                Test test = scope.ResolveService<Test>();
                var config = scope.ResolveService<IConfiguration>();
                Assert.Equal(rootConfig, config);
                var fly1=scope.ResolveRequiredService<IFly>();
                if (fly1 is Superman superman)
                {
                    Assert.Null(superman.Configuration);
                    //Assert.NotNull(superman.Configuration1);
                }
                var fly2=scope.ResolveRequiredService<IFly>();
                Assert.Equal(fly1,fly2);
                var wukong1 = scope.ResolveRequiredService<WuKong>();
                var wukong2 = scope.ResolveRequiredService<WuKong>();
                Assert.NotEqual(wukong1,wukong2);
                var bajie=scope.ResolveRequiredService<BaJie>();
                var bajie1 = scope.ResolveRequiredService<BaJie>();
                //Assert.NotEqual(bajie, bajie1);
                var s0=scope.ResolveRequiredService<HasDependencyTest>();
                s0.Test();
                Assert.Equal(s0._fly, fly1);
                var s1= scope.ResolveRequiredService<HasDependencyTest1>();
                s1.Test();
                var s2 = scope.ResolveRequiredService<HasDependencyTest2>();
                s2.Test();
                var s3 = scope.ResolveRequiredService<HasDependencyTest3>();
                s3.Test();
                var s4 = scope.ResolveRequiredService<HasDependencyTest4<string>>();
                s4.Test();
                using(var innerScope=scope.CreateScope())
                {
                    var config2 = innerScope.ResolveService<IConfiguration>();
                    Assert.True(rootConfig == config2);
                    var fly3 = innerScope.ResolveService<IFly>();
                    fly3.Fly();
                    Assert.NotEqual(fly1, fly3);
                }
                var flySvcs = scope.ResolveServices<IFly>();
                foreach (var fly in flySvcs)
                {
                    fly.Fly();
                }
                var genericService1=_container.ResolveRequiredService<GenericServiceTest<int>>();
                genericService1.Test();
                var genericService2 = _container.ResolveRequiredService<GenericServiceTest<string>>();
                genericService2.Test();
            }
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
            Console.WriteLine("八戒投胎");
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
    public class Superman : IFly
    {
        public IConfiguration? Configuration { get; set; }
        [FromService]  //.net core中此特性用于指示将动作方法的参数从依赖注入（DI）容器中解析出来。
        public IConfiguration? Configuration1 { get; set; }
        public string Name => "超人";

        public void Fly()
        {
            Console.WriteLine($"{Name}飞行");
        }
    }
    public class HasDependencyTest
    {
        public readonly IFly _fly;
        public HasDependencyTest(IFly fly)
        {
            _fly = fly;
        }
        public void Test()
        {
            Console.WriteLine($"test in {nameof(HasDependencyTest)}");
            _fly.Fly();
        }
    }
    public class GenericServiceTest<T>
    {
        public GenericServiceTest()
        {
            Console.WriteLine("构造函数被执行");
        }
        public void Test()
        {
            Console.WriteLine($"generic type:{typeof(T).FullName}");
        }
    }
    public class HasDependencyTest1
    {
        // IReadOnlyCollection表示元素的强类型的只读集合。
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
            _flys = flys;
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
    public class Test
    {
       public Test()
        {
            Console.WriteLine("空白");
        }
    }
}
