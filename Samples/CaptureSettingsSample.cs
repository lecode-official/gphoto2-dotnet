
#region Using Directives

using System;
using System.Devices;
using System.Globalization;
using System.Threading.Tasks;

#endregion

namespace SamplesApplication
{
    /// <summary>
    /// Represents a gPhoto2.NET sample, which gets the first camera attached to the system and prints out the current capture settings of the camera.
    /// </summar>
    public class CaptureSettingsSample : ISample
    {
        #region ISample Implementation
        
        /// <summary>
        /// Gets the title of the sample.
        /// </summary>
        public string Title
        {
            get
            {
                return "Capture settings";
            }
        }
        
        /// <summary>
        /// Executes the sample.
        /// </summary>
        /// <param name="camera">The camera with which the sample is to be executed.</param>
        public async Task ExecuteAsync(Camera camera)
        {
            // Gets some information about the capture settings of the camera and prints it out
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "ISO speed: {0}", await camera.GetIsoSpeedAsync()));
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Shutter speed: {0}", await camera.GetShutterSpeedAsync()));
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Aperture: {0}", await camera.GetApertureAsync()));
        }
        
        #endregion
    }
}