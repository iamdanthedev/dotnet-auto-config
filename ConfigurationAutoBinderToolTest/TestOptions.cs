using System.Collections.Generic;
using Bonliva.ConfigurationAutoBinder;

namespace ConfigurationAutoBinderToolTest
{
    [AutoBindConfiguration]
    public class HelloWorldOptionsRoot
    {
        public string SomeVar1 { get; set; }
        public IEnumerable<string> SomeList1 { get; set; }
    }

    [AutoBindConfiguration(ConfigRoot = "Namespace1")]
    public class HelloWorkOptionsNamespaced
    {
        public string SomeVar1 { get; set; }
        public IEnumerable<string> SomeList1 { get; set; }
        public string SomeVar2 { get; set; }
        public IEnumerable<string> SomeList2 { get; set; }
    }
}