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
    //AllowMultiple:是否允许多次应用相同类型的属性,Inherited:是否允许派生类继承该属性
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Parameter,AllowMultiple =false,Inherited =false)]
    public class FromServiceAttribute:Attribute
    {
    }
}
