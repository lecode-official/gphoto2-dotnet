
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
			try
			{
				Camera camera = (await Camera.GetCamerasAsync()).FirstOrDefault();
	
				if (camera == null)
				{
					Console.WriteLine("No camera detected!");
					return;
				}

				foreach (CameraSetting setting in camera.Settings)
					Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}",
						await setting.GetLabelAsync(), await setting.GetValueAsync()));
			}
			catch (CameraException exception)
			{
				Console.WriteLine(string.Concat("An error occurred:", Environment.NewLine, exception.Details));
			}
		}
	}
}