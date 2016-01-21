
#region Using Directives

using System;
using System.Devices;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace Application
{
	/// <summary>
	/// Represents a test application, which gets the first camera attached to the system and prints out some information about the camera.
	/// </summar>
	public class CameraSettingsTest
	{
		#region Public Static Methods
		
		/// <summary>
		/// The entrypoint for the camera settings test program.
		/// </summar>
		public static void Main()
		{
			// Calls the asynchronous version of the main method, so that asynchronous operations can be performed
			CameraSettingsTest.MainAsync().Wait();
		}

		/// <summary>
		/// The asynchronous entrypoint to the camera settings test program.
		/// </summary>
		public static async Task MainAsync()
		{
			// Since the connection to the camera via USB can be highly volatile, exceptions can be raised all the time, therefore all calls to the
            // gphoto2-dotnet should be wrapped in try-catch-clauses
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
                
				// Gathers some information about the camera and prints it out
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Manufacturer: {0}", await camera.GetManufacturerAsync()));
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Camera model: {0}", await camera.GetCameraModelAsync()));
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Lens name: {0}", await camera.GetLensNameAsync()));
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Battery level: {0}", await camera.GetBatteryLevelAsync()));
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Owner name: {0}", await camera.GetOwnerNameAsync()));
                
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