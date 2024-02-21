using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InversionofControl
{
 
   public interface IServiceContainerBuilder : IEnumerable<ServiceDefinition>
   {
        IServiceContainerBuilder Add(ServiceDefinition serviceDefinition);
        IServiceContainerBuilder TryAdd(ServiceDefinition serviceDefinition);
        IServiceContainer Build();
    }
   public sealed class SeviceContainerBuilder : IServiceContainerBuilder
    {
        //c#9.0新语法new(),在引用确定类型的情况下,创建对象可以省略类型
        private readonly List<ServiceDefinition> _services = new();
        public IServiceContainer Build()
        {
            return new ServiceContainer(_services);
        }

        public IEnumerator<ServiceDefinition> GetEnumerator()
        {
            return _services.GetEnumerator();
        }

         public IServiceContainerBuilder Add(ServiceDefinition serviceDefinition)
        {
            if (_services.Any(x => x.ServiceType == serviceDefinition.ServiceType && x.GetImplementType() == serviceDefinition.GetImplementType()))
            {
                return this;
            }
            _services.Add(serviceDefinition);
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IServiceContainerBuilder TryAdd(ServiceDefinition serviceDefinition)
        {
            if (_services.Any(x => x.ServiceType == serviceDefinition.ServiceType))
            {
                return this;
            }
            _services.Add(serviceDefinition);
            return this;
        }
    }
}
