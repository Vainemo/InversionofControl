using Microsoft.Extensions;
using Microsoft.Extensions.Configuration;

namespace InversionofControl
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DependencyInjectionDemo dependencyInjectionDemo = new DependencyInjectionDemo();
            dependencyInjectionDemo.Test();
        }
    }
}