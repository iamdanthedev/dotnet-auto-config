using System.Collections.Generic;

// using MongoDB.Bson;

namespace AutoConfig.DotNetTool.Tests
{
    [AutoConfig]
    public class HelloWorldOptionsRoot
    {
        public string SomeVar1 { get; set; }
        public IEnumerable<string> SomeList1 { get; set; }
    }

    [AutoConfig(ConfigRoot = "Namespace1")]
    public class HelloWorkOptionsNamespaced
    {
        public string SomeVar1 { get; set; }
        public IEnumerable<string> SomeList1 { get; set; }
        public string SomeVar2 { get; set; }
        public IEnumerable<string> SomeList2 { get; set; }
    }

    // public class Foo
    // {
    //     public ObjectId Id { get; set; }
    // }
}