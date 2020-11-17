using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bonliva.ConfigurationAutoBinder
{
    public class AutoBindConfigurationAttribute : Attribute
    {
        /// <summary>
        /// Bind configuration object to values at the path
        /// </summary>
        public string? ConfigRoot { get; set; }

        /// <summary>
        /// Require configuration to be present in environments. Optional everywhere by default
        /// </summary>
        public string[]? RequiredInEnv { get; set; }

        public AutoBindConfigurationAttribute(
            // string? configRoot,
            // bool? required,
            // IEnumerable<string>? requiredInEnv
        )
        {
        }
    }

    public static class ConfigurationAutoBinderServiceCollectionExtension
    {
        public static IServiceCollection AddAutoBoundConfiguration(this IServiceCollection services,
            IConfiguration configuration, string env)
        {
            var attrType = typeof(AutoBindConfigurationAttribute);
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
                (AutoBindConfigurationAttribute?) Attribute.GetCustomAttribute(configClassType,
                    typeof(AutoBindConfigurationAttribute));

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

                var value = configuration.GetValue(property.PropertyType, configSectionName);

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
