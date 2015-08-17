
#region Using Directives

using System;
using System.Devices;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace Application
{
	public class TestProgram
	{
		public static void Main()
		{
			TestProgram.MainAsync().Wait();
		}

		public static async Task MainAsync()
		{
			Camera camera = (await Camera.GetCamerasAsync()).FirstOrDefault();

			if (camera == null)
			{
				Console.WriteLine("No camera detected!");
				return;
			}

			try
			{
				foreach (CameraSetting setting in camera.Settings)
					Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}",
						await setting.GetLabelAsync(), await setting.GetCurrentValueAsync()));
			}
			catch (CameraException exception)
			{
				Console.WriteLine("An error occurred:");
				Console.WriteLine(exception.Message);
				Console.WriteLine(exception.Details);
			}
		}
	}
}