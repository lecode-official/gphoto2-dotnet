
#region Using Directives

using System;
using System.Devices;
using System.Globalization;
using System.Threading.Tasks;

#endregion

namespace SamplesApplication
{
    /// <summary>
    /// Represents a gPhoto2.NET sample, which gets the first camera attached to the system and prints out all available camera configurations.
    /// </summar>
    public class CameraConfigurationSample : ISample
    {
        #region ISample Implementation
        
        /// <summary>
        /// Gets the title of the sample.
        /// </summary>
        public string Title
        {
            get
            {
                return "Camera configuration";
            }
        }
        
        /// <summary>
        /// Executes the sample.
        /// </summary>
        /// <param name="camera">The camera with which the sample is to be executed.</param>
        public async Task ExecuteAsync(Camera camera)
        {
            // Gets all camera configuration and prints them out
            foreach (CameraConfiguration cameraConfiguration in await camera.GetSupportedConfigurationAsync())
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", await cameraConfiguration.GetLabelAsync(), await cameraConfiguration.GetValueAsync()));
        }
        
        #endregion
    }
}