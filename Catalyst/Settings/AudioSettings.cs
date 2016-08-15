using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalyst.Settings
{
    /// <summary>
    /// The audio channel configuration (stereo, surround, etc.)
    /// </summary>
    public enum AudioChannelConfiguration
    {
        /// <summary>
        /// A surround (multi-channel, 2D) sound system.
        /// </summary>
        Surround,
        /// <summary>
        /// A stereo (2-channel, 1D) sound system.
        /// </summary>
        Stereo,
    }

    /// <summary>
    /// The type of speakers used.
    /// </summary>
    public enum AudioSpeakerType
    {
        /// <summary>
        /// Normal speakers.
        /// </summary>
        FullRange,
        /// <summary>
        /// Television speakers.
        /// </summary>
        Television,
        /// <summary>
        /// Headphones.
        /// </summary>
        Headphones
    }

    /// <summary>
    /// The audio settings.
    /// </summary>
    public class AudioSettings : SettingsCategory
    {
        static AudioSettings()
        {
            @namespace = "GstAudio";
        }

        internal AudioSettings(Settings parent) : base(parent) { }

        /// <summary>
        /// The global game volume (from 0 to 1).
        /// </summary>
        public float MasterVolume
        {
            get { return (float)this["Volume"]; }
            set { this["Volume"] = value; }
        }

        /// <summary>
        /// The music volume (from 0 to 1).
        /// </summary>
        public float MusicVolume
        {
            get { return (float)this["MusicVolume"]; }
            set { this["MusicVolume"] = value; }
        }

        /// <summary>
        /// A volume boost on dialogues (from 0 to 1).
        /// </summary>
        public float DialogueBoost
        {
            get { return (float)this["VOLanguage"]; }
            set { this["VOLanguage"] = value; }
        }

        /// <summary>
        /// The type of speakers used.
        /// </summary>
        public AudioSpeakerType SpeakerType
        {
            get { return (AudioSpeakerType)(int)this["SpeakerType"]; }
            set { this["SpeakerType"] = (int)value; }
        }

        /// <summary>
        /// The current channel configuration.
        /// </summary>
        public AudioChannelConfiguration ChannelConfiguration
        {
            get { return (AudioChannelConfiguration)(int)this["ChannelConfiguration"]; }
            set { this["ChannelConfiguration"] = (int)value; }
        }
    }
}
