
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
	/// Represents a test application, which gets the first camera attached to the system and prints out all settings of the camera
	/// including their values.
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
			// Since the connection to the camera via USB can be highly volatile, exceptions can be raised all the time, therefore all
			// calls to the gphoto2-dotnet should be wrapped in try-catch-clauses, gphoto2-dotnet always throws CameraException
			try
			{
				// Gets the first camera attached to the computer
				Camera camera = (await Camera.GetCamerasAsync()).FirstOrDefault();
	
				// Checks if a camera was found, if no camera was found, then an error message is printed out and the program is quit
				if (camera == null)
				{
					Console.WriteLine("No camera detected!");
					return;
				}

				// Gets all settings of the attached camera, cycles over them and prints out all settings and their current values
				foreach (CameraSetting setting in camera.Settings)
					Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}",
						await setting.GetLabelAsync(), await setting.GetValueAsync()));
			}
			catch (CameraException exception)
			{
				// If an exception was caught, e.g. because the camera was unplugged, an error message is printed out
				Console.WriteLine(string.Concat("An error occurred:", Environment.NewLine, exception.Details));
			}
		}
		
		#endregion
	}
}