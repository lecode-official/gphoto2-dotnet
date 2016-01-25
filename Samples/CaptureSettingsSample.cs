
#region Using Directives

using System;
using System.Devices;
using System.Globalization;
using System.Linq;
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
        public async Task ExecuteAsync()
        {
            // Since the connection to the camera via USB can be highly volatile, exceptions can be raised all the time, therefore all calls to the
            // gPhoto2.NET should be wrapped in try-catch-clauses
            Camera camera = null;
            try
            {
                // Gets the first camera attached to the system
                camera = (await Camera.GetCamerasAsync()).FirstOrDefault();
                
                // Checks if a camera was found, if no camera was found, then an error message is printed out and the program is quit
                if (camera == null)
                {
                    Console.WriteLine("No camera detected!");
                    return;
                }
                
                // Gets some information about the capture settings of the camera and prints it out
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "ISO speed: {0}", await camera.GetIsoSpeedAsync()));
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Shutter speed: {0}", await camera.GetShutterSpeedAsync()));
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Aperture: {0}", await camera.GetApertureAsync()));
            }
            catch (CameraException exception)
            {
                // If an exception was caught, e.g. because the camera was unplugged, an error message is printed out
                Console.WriteLine(string.Concat("An error occurred:", Environment.NewLine, exception.Details));
            }
            finally
            {
                // If a camera was acquired, then it is safely disposed of
                if (camera != null)
                    camera.Dispose();
            }
        }
        
        #endregion
    }
}