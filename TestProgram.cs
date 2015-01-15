
#region Using Directives

using System;
using System.Devices;
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
			Console.WriteLine(camera.Name + " " + camera.Port);
			Console.ReadLine();
		}
	}
}
