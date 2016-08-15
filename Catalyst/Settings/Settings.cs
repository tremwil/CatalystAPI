using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Catalyst.Settings
{
    /// <summary>
    /// A class to read or change game settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The path to the game's setting file.
        /// </summary>
        public static string SettingsPath => 
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
            "\\Mirrors Edge Catalyst\\settings\\PROF_SAVE_profile";

        /// <summary>
        /// The raw game settings as JSON. Do not modify 
        /// unless you know what you are doing.
        /// </summary>
        public JObject rawSettings;
        
        /// <summary>
        /// Creates an instance of the Settings class.
        /// </summary>
        public Settings()
        {
            rawSettings = new JObject();

            Audio = new AudioSettings(this);
            Video = new VideoSettings(this);
            Mouse = new MouseSettings(this);
            Controls = new ControlSettings(this);
        }

        /// <summary>
        /// Load the settings.
        /// </summary>
        public void Load()
        {
            rawSettings.RemoveAll(); // Clear the settings first
            rawSettings = JsonPathList.FromFile(SettingsPath);
        }

        /// <summary>
        /// Apply the settings to game files.
        /// </summary>
        public void Apply()
        {
            JsonPathList.ToFile(rawSettings, SettingsPath);
        }

        /// <summary>
        /// Get a generic category instead of messing around with JSON data.
        /// Useful for custom categories.
        /// </summary>
        /// <typeparam name="CategoryType">The type of the category. Must inherit
        /// from <see cref="SettingsCategory"/>.</typeparam>
        /// <returns>An instance of the category.</returns>
        public CategoryType Get<CategoryType>() where CategoryType : SettingsCategory
        {
            // Use Activator to create an instance of our generic
            return (CategoryType)Activator.CreateInstance(typeof(CategoryType), this);
        }

        /// <summary>
        /// The audio settings.
        /// </summary>
        public readonly AudioSettings Audio;
        /// <summary>
        /// The video settings.
        /// </summary>      
        public readonly VideoSettings Video;
        /// <summary>
        /// The mouse settings.
        /// </summary> 
        public readonly MouseSettings Mouse;
        /// <summary>
        /// The keybindings.
        /// </summary>
        public readonly ControlSettings Controls;
    }
}
