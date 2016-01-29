
#region Using Directives

using System;
using System.Devices;
using System.Globalization;
using System.Threading.Tasks;

#endregion

namespace SamplesApplication
{
    /// <summary>
    /// Represents a gPhoto2.NET sample, which gets the first camera attached to the system and prints out some information about the camera.
    /// </summar>
    public class CameraInformationSample : ISample
    {
        #region ISample Implementation
        
        /// <summary>
        /// Gets the title of the sample.
        /// </summary>
        public string Title
        {
            get
            {
                return "Camera information";
            }
        }
        
        /// <summary>
        /// Executes the sample.
        /// </summary>
        /// <param name="camera">The camera with which the sample is to be executed.</param>
        public async Task ExecuteAsync(Camera camera)
        {
            // Gathers some information about the camera and prints it out
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Manufacturer: {0}", await camera.GetManufacturerAsync()));
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Camera model: {0}", await camera.GetCameraModelAsync()));
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Lens name: {0}", await camera.GetLensNameAsync()));
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Battery level: {0}", await camera.GetBatteryLevelAsync()));
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Owner name: {0}", await camera.GetOwnerNameAsync()));
        }
        
        #endregion
    }
}