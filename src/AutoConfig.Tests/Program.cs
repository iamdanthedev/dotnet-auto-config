using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AutoConfig.Tests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json");
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddAutoConfig(context.Configuration, "Development");
                    services.AddHostedService<ProgramService>();

                })
                .RunConsoleAsync();
        }

        public class ProgramService : IHostedService
        {
            private readonly Test1Options _test1Options;
            private readonly Test2Options _test2Options;

            public ProgramService(Test1Options test1Options, Test2Options test2Options)
            {
                _test1Options = test1Options;
                _test2Options = test2Options;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        [AutoConfig(ConfigRoot = "Test1")]
        public class Test1Options
        {
            public string StringVal { get; set; }
            public int IntVal { get; set; }
            public IEnumerable<string> ListItems { get; set; }
            public IEnumerable<string> ListItems1 { get; set; }
            public Subclass Sub1 { get; set; }
            public IEnumerable<Subclass> Subs { get; set; }

            public class Subclass
            {
                public string Name { get; set; }
                public IEnumerable<string> Items { get; set; }
            }
        }

        [AutoConfig(ConfigRoot = "Test2")]
        public class Test2Options
        {
            public IEnumerable<string> Names { get; set; }
        }
    }
}