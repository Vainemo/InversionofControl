using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InversionofControl
{
    /// <summary>
    /// 服务生命周期
    /// </summary>
    public enum ServiceLifeTime:sbyte
    {
        /// <summary>
        /// 单例模式:整个根容器的生命周期内是同一个对象
        /// </summary>
        Singleton=0,

        /// <summary>
        /// 作用域模式:在容器或子容器的生命周期内,对象保持一致,如果容器释放掉,就意味着对象也会释放
        /// </summary>
        Scoped=1,

        /// <summary>
        /// 瞬时模式:每次使用都会创建新的实例
        /// </summary>
        Transient=2,
    }
}
