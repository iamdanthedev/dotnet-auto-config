using System;

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
}