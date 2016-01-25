
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
                
                // Gets all camera configuration and prints them out
                foreach (CameraConfiguration cameraConfiguration in camera.Configurations)
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", await cameraConfiguration.GetLabelAsync(), await cameraConfiguration.GetValueAsync()));
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