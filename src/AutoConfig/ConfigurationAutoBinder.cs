using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoConfig
{
    public class AutoConfigAttribute : Attribute
    {
        /// <summary>
        /// Bind configuration object to values at the path
        /// </summary>
        public string? ConfigRoot { get; set; }

        /// <summary>
        /// Require configuration to be present in environments. Optional everywhere by default
        /// </summary>
        public string[]? RequiredInEnv { get; set; }

        public AutoConfigAttribute(
            // string? configRoot,
            // bool? required,
            // IEnumerable<string>? requiredInEnv
        )
        {
        }
    }

    public static class ConfigurationAutoBinderServiceCollectionExtension
    {
        public static IServiceCollection AddAutoConfig(this IServiceCollection services,
            IConfiguration configuration, string env)
        {
            var attrType = typeof(AutoConfigAttribute);
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsDefined(attrType, true))
                .ToList()
                .ForEach(typeClass => BindConfiguration(services, typeClass, configuration, env));

            return services;
        }

        private static void BindConfiguration(IServiceCollection services, Type configClassType,
            IConfiguration configuration, string env)
        {
            var attr =
                (AutoConfigAttribute?) Attribute.GetCustomAttribute(configClassType,
                    typeof(AutoConfigAttribute));

            if (attr == null)
            {
                throw new Exception("cannot find attribute");
            }

            var isConfigRequired =
                attr.RequiredInEnv != null && attr.RequiredInEnv.Any(x => x == env);


            var properties = configClassType.GetProperties();
            var configObject = Activator.CreateInstance(configClassType);

            foreach (PropertyInfo property in properties)
            {
                var configSectionName = string.IsNullOrEmpty(attr.ConfigRoot)
                    ? property.Name
                    : $"{attr.ConfigRoot}:{property.Name}";

                // var value = configuration.GetValue(property.PropertyType, configSectionName);

                var value = configuration.GetSection(configSectionName)
                    .Get(property.PropertyType);

                if (isConfigRequired && value == null)
                {
                    throw new AutoConfigurationException(attr.ConfigRoot ?? "(root)", property.Name);
                }
                
                property.SetValue(configObject, value);
            }

            services.AddSingleton(configClassType, configObject);
        }
    }

    internal class AutoConfigurationException : Exception
    {
        public AutoConfigurationException(string configRoot, string propertyName) : base(
            $"missing configuration value for {configRoot}:{propertyName}")
        {
        }
    }
}
