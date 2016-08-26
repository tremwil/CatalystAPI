using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Catalyst.Settings
{
    /// <summary>
    /// An abstract class to create setting categories.
    /// <para></para>NOTE: You can create your own setting categories
    /// using <see cref="Settings.Get{T}"/>.
    /// </summary>
    public abstract class SettingsCategory
    {
        /// <summary>
        /// The namespace of the settings (GstInput, GstRender, etc.)
        /// </summary>
        protected static string @namespace;

        /// <summary>
        /// The namespace of the settings (GstInput, GstRender, etc.)
        /// </summary>
        public string Namespace => @namespace;

        /// <summary>
        /// The parent of this category.
        /// </summary>
        protected internal Settings parent;
        
        /// <summary>
        /// The setting values.
        /// </summary>
        protected internal JObject settingValues
        {
            get
            {
                try
                { return (JObject)parent.rawSettings.SelectToken(Namespace); }

                catch (Exception e)
                { throw new InvalidOperationException("Settings have not been initialized.", e); }
            }
        }
        
        /// <summary>
        /// Create a new instance of the category.
        /// </summary>
        /// <param name="parent">The parent of this object</param>
        protected internal SettingsCategory(Settings parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// Gets a setting from the JSON structure.
        /// <para></para>Do not use this unless you know what you are doing.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public JToken this[string setting]
        {
            get
            { 
                if (settingValues.GetValue(setting) != null)
                    return settingValues[setting];
                else
                    throw new ArgumentException("You cannot get a setting that has not been initialized.", "setting");
            }
            // Do not allow to set a value that is not already in the dict
            set
            {
                if (settingValues.GetValue(setting) != null)
                    settingValues[setting] = value;
                else
                    throw new ArgumentException("You cannot set a setting that has not been initialized.", "setting");
            }
        }
    }
}
