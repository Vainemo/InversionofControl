using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InversionofControl
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceKey : IEquatable<ServiceKey>
    {
        /// <summary>
        /// 服务的抽象类型
        /// </summary>
        public Type ServiceType { get; }
        /// <summary>
        /// 实现类型
        /// </summary>
        public Type ImplementType { get; }

        //IEquatable<T>接口解决了Object基类的Equals方法存在的两个问题
        //1.缺乏类型安全性
        //2.对于值类型而言,在进行比较时需要装箱
        public bool Equals(ServiceKey other)
        {
            return ServiceType==other?.ServiceType&&ImplementType==other?.ImplementType;
        }
        public ServiceKey(Type serviceType,ServiceDefinition definition)
        {
            ServiceType= serviceType;
            //获取服务的实现类型
            ImplementType = definition.GetImplementType();
        }
        public override bool Equals(object obj)
        {
            return Equals((ServiceKey)obj);
        }
        public override int GetHashCode()
        {
            var key = $"{ServiceType.FullName}_{ImplementType.FullName}";
            return key.GetHashCode();
        }
    }
}
