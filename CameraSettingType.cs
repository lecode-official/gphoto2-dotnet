
namespace System.Devices
{
    /// <summary>
    /// Represents an enumeration for the data type of a setting (e.g. text or option).
    /// </summary>
    internal enum CameraSettingType
    {
        /// <summary>
        /// Represents a setting type that is unkwnon to the software.
        /// </summary>
        Unknown,
        
        /// <summary>
        /// Represents a setting, that has a textual value.
        /// </summary>
        Text,
        
        /// <summary>
        /// Represents a setting, that has a value with a well-defined, non-infinite value domain.
        /// </summary>
        Option,
        
        /// <summary>
        /// Represents a setting, whose value can be toggled, e.g. on and off (the actual values that should be used when setting them with gPhoto2 are 0 and 1).
        /// </summary>
        Toggle,
        
        /// <summary>
        /// Represents a setting, that has a temporal value (the actual values that should be used when setting them with gPhoto2 are dates in the UNIX format, i.e. the
        /// number of ticks since the UNIX epoch, also "now" can be used to set the current date and time).
        /// </summary>
        DateTime
    }
}
