using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Bonliva.ConfigurationAutoBinder;
using Microsoft.Extensions.CommandLineUtils;
using MoreLinq;
using Newtonsoft.Json;

namespace ConfigurationAutoBinderTool
{
    class Program
    {
        static void Main(params string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: true)
            {
                Name = "Config generator"
            };

            app.HelpOption("-?|-h|--help");

            app.Command("generate", command =>
            {
                var projectsOption = command.Option("-p|--project <PROJECTNAME>",
                    "Project names to extract config from",
                    CommandOptionType.MultipleValue);

                var outArg = command.Argument("[out]",
                    "Output file. Will display config on screen if missing");

                command.OnExecute(() =>
                {
                    var projects = projectsOption.Values;
                    var outPath = outArg.Value;

                    var configJson = AutoConfigGenerator.GenerateConfig(projects);

                    if (outPath == null)
                    {
                        Console.WriteLine(configJson);
                    }
                    else
                    {
                        var filePath =
                            Path.Combine(Environment.CurrentDirectory, outPath);
                        File.WriteAllText(filePath, configJson);
                    }

                    return 0;
                });
            });

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 0;
            });

            try
            {
                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine(ex.Message);
                app.ShowHelp();
            }
        }
    }

    public class AutoConfigGenerator
    {
        public static string GenerateConfig(IEnumerable<string> projectNames)
        {
            var assemblies =
                GetProjectAssemblies(Environment.CurrentDirectory, projectNames);
            var configRecords = GetConfigRecordsInAssemblies(assemblies);
            return ConfigRecordsToJson(configRecords);
        }

        private static List<Assembly> GetProjectAssemblies(string solutionFolder,
            IEnumerable<string> includeProjects)
        {
            var includeProjectDlls = includeProjects.Select(x => $"{x}.dll")
                .ToList();

            var dllFileNames =
                Directory
                    .GetFiles(solutionFolder, "*.dll", SearchOption.AllDirectories)
                    .Where(x =>
                    {
                        var fileName = Path.GetFileName(x);
                        return includeProjectDlls.Contains(fileName);
                    })
                    .DistinctBy(Path.GetFileName)
                    .ToList();

            if (dllFileNames.Count == 0)
            {
                throw new Exception("no projects found");
            }

            Console.WriteLine($"DLLs found:\n {string.Join("\n", dllFileNames)}");

            return dllFileNames.Select(x =>
                {
                    return Assembly.LoadFrom(x);
                    // return AssemblyLoadContext.Default.LoadFromAssemblyPath(x))
                })
                .ToList();
        }


        public static List<ConfigRecord> GetConfigRecordsInAssemblies(
            List<Assembly> assemblies)
        {
            var attrType = typeof(AutoBindConfigurationAttribute);

            var attrs = assemblies.SelectMany(x => x.GetTypes())
                .Where(x => x.IsDefined(attrType, true))
                .Select(delegate(Type x)
                {
                    var attr =
                        (AutoBindConfigurationAttribute?) Attribute.GetCustomAttribute(
                            x,
                            typeof(AutoBindConfigurationAttribute));

                    return new AttributeOnType(x, attr);
                });

            var configRecords = attrs.SelectMany(attrType =>
                {
                    var props = attrType.Type.GetProperties();

                    return props.Select(propInfo => new ConfigRecord(
                        attrType.Attr.ConfigRoot ?? "",
                        propInfo.Name,
                        propInfo.PropertyType));
                })
                .ToList();

            return configRecords;
        }

        private static string ConfigRecordsToJson(List<ConfigRecord> records)
        {
            var dynamicObject = new ExpandoObject();

            records.ForEach(record =>
            {
                var fieldName = string.IsNullOrEmpty(record.Root)
                    ? record.Name
                    : $"{record.Root}:{record.Name}";
                dynamicObject.TryAdd(fieldName, "");
            });

            return JsonConvert.SerializeObject(dynamicObject, Formatting.Indented);
        }

        private class AttributeOnType
        {
            public AttributeOnType(Type type, AutoBindConfigurationAttribute attr)
            {
                Type = type;
                Attr = attr;
            }

            public Type Type { get; set; }
            public AutoBindConfigurationAttribute Attr { get; set; }
        }

        public class ConfigRecord
        {
            public ConfigRecord(string root, string name, Type type)
            {
                Root = root;
                Name = name;
                Type = type;
            }

            public string Root { get; set; }
            public string Name { get; set; }
            public Type Type { get; set; }
        }
    }
}