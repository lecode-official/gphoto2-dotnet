
#region Using Directives

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#endregion

namespace System.Devices
{
	/// <summary>
	/// Represents a camera device that is connected to the computer. It can be used to retrieve information about the camera, download pictures from the camera, and remote-control the camera. The degree to which this functionality can be provided depends on the
	/// camera model.
	/// </summary>
	public class Camera
	{
		#region Constructors

		/// <summary>
		/// Intializes a new <see cref="Camera" /> instance. The constructor is made private, so that the factory pattern, which is used to instantiate new instances of <see cref="Camera" />, can be enforced.
		/// </summary>
		private Camera() { }

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the name of the camera.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the port to which the camera is attached to.
		/// </summary>
		public string Port { get; private set; }

		#endregion

		#region Internal Methods

		/// <summary>
		/// Initializes the camera.
		/// </summary>
		/// <param name="name">The name of the camera.</param>
		/// <param name="port">The port to which the camera is attached to.</param>
		internal Task InitializeAsync(string name, string port)
		{
			// Stores the initial information about the camera for later use
			this.Name = name;
			this.Port = port;

			return Task.FromResult(0);
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Intializes a new <see cref="Camera" /> instance. The constructor is made private, so that the factory pattern, which is used to instantiate new instances of <see cref="Camera" />, can be enforced.
		/// </summary>
		public static async Task<IReadOnlyCollection<Camera>> GetCamerasAsync()
		{
			// Creates a new IPC wrapper, which is used to invoke gPhoto2 in order to retrieve all cameras currently attached to the computer
			IpcWrapper ipcWrapper = new IpcWrapper("gphoto2");

			// Gets all the cameras attached to the computer and returns them
			return await ipcWrapper.ExecuteAsync("--auto-detect", async output =>
				{
					// Creates a new result list for the cameras
					List<Camera> cameras = new List<Camera>();

					// Creates a string reader, so that the output of gPhoto2 can be read line by line
					using (StringReader stringReader = new StringReader(output))
					{
						// Dismisses the first two lines because they only contain the header of the table containing the cameras
						stringReader.ReadLine();
						stringReader.ReadLine();

						// Cycles over the rest of the lines (each line representes a row in the table containing the cameras)
						string line;
						while (!string.IsNullOrWhiteSpace(line = stringReader.ReadLine()))
						{
							// Trims the line, because it might have leading or trailing whitespaces (the regular expression would be more complex with them)
							line = line.Trim();

							// The line contains the name of the camera and its port separated by multiple whitespaces, this regular expression is used to split them
							Regex cameraAndPortRegex = new Regex("^(?<Name>((\\S\\s\\S)|\\S)+)\\s\\s+(?<Port>((\\S\\s\\S)|\\S)+)$");

							// Reads the name and the port of the camera
							Match match = cameraAndPortRegex.Match(line);
							string cameraName = match.Groups["Name"].Value;
							string cameraPort = match.Groups["Port"].Value;

							// If either the camera name or the port is null or whitespace, then the camera could not be matched
							if (string.IsNullOrWhiteSpace(cameraName) || string.IsNullOrWhiteSpace(cameraPort))
								continue;

							// Creates the new camera and adds it to the result set
							Camera camera = new Camera();
							await camera.InitializeAsync(cameraName, cameraPort);
							cameras.Add(camera);
						}
					}

					// Returns all cameras that have been found by gPhoto2
					return cameras;
				});
		}

		#endregion
	}
} 
