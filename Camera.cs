
#region Using Directives

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#endregion

namespace System.Devices
{
	/// <summary>
	/// Represents a camera device that is connected to the computer. It can be used to retrieve information about the camera, download
	/// pictures from the camera, and remote-control the camera. The degree to which this functionality can be provided depends on the
	/// camera model.
	/// </summary>
	public class Camera : IDisposable
	{
		#region Constructors

		/// <summary>
		/// Intializes a new <see cref="Camera" /> instance. The constructor is made private, so that the factory pattern, which is used
		/// to instantiate new instances of <see cref="Camera" />, can be enforced.
		/// </summary>
		/// <param name="name">The name of the camera.</param>
		/// <param name="port">The port with which the camera is connected to the machine.</param>
		private Camera(string name, string port)
		{
			// Stores the initial information about the camera for later use
			this.Name = name;
			this.Port = port;
		}

		#endregion
		
		#region Private Fields
		
		/// <summary>
		/// Contains the IPC wrapper, which is used to interface with gPhoto2.
		/// </summary>
		private GPhoto2IpcWrapper gPhoto2IpcWrapper = new GPhoto2IpcWrapper();
		
		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the name of the camera.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the port with which the camera is connected to the machine.
		/// </summary>
		public string Port { get; private set; }

		/// <summary>
		/// Gets a value that determines whether the camera has the ability to be configured, i.e. the values of settings can be read
		/// and set.
		/// </summary>
		public bool CanBeConfigured { get; private set; }
		
		/// <summary>
		/// Gets a value that determines whether the camera has the ability to capture images.
		/// </summary>
		public bool CanCaptureImages { get; private set; }

		/// <summary>
		/// Gets a value that determines whether the camera has the ability to caputre preview images.
		/// </summary>
		public bool CanCapturePreviews { get; private set; }
		
		/// <summary>
		/// Gets a value that determines whether the camera has the ability to delete files from the camera.
		/// </summary>
		public bool CanDeleteFiles { get; private set; }
		
		/// <summary>
		/// Gets a value that determines whether the camera has the ability to delete all files from the camera.
		/// </summary>
		public bool CanDeleteAllFiles { get; private set; }
		
		/// <summary>
		/// Gets a value that determines whether the camera has the ability to preview files (thumbnails).
		/// </summary>
		public bool CanPreviewFiles { get; private set; }
		
		/// <summary>
		/// Gets a value that determines whether the camera has the ability to upload files to the camera.
		/// </summary>
		public bool CanUploadFiles { get; private set; }
		
		/// <summary>
		/// Gets a list of all the settings of the camera.
		/// </summary>
		public IReadOnlyCollection<CameraSetting> Settings { get; private set; }
		
		#endregion
		
		#region IDisposable Implementation
		
		/// <summary>
		/// Disposes of all the resources that have been allocated by the <see cref="Camera" />.
		/// </summary>
		public void Dispose()
		{
			// Checks if the IPC wrapper for gPhoto2 has already been disposed of, if not then it is disposed of
			if (this.gPhoto2IpcWrapper != null)
			{
				this.gPhoto2IpcWrapper.Dispose();
				this.gPhoto2IpcWrapper = null;
			}
		}
		
		#endregion

		#region Private Methods

		/// <summary>
		/// Initializes a new camera (this is the internal factory method for instantiating new cameras).
		/// </summary>
		private async Task InitializeAsync()
		{
			// Sets the camera name and its port as standard command line parameters, so that they do not have to be stated over and
			// over again explictly
			this.gPhoto2IpcWrapper.StandardCommandLineParameters = string.Format(CultureInfo.InvariantCulture,
				"--camera \"{0}\" --port \"{1}\"", this.Name, this.Port);
			    
			// Gets the abilities of the camera, so that we know which actions can be executed
			await this.gPhoto2IpcWrapper.ExecuteAsync("--abilities",
			    output =>
			    {
			        // Creates a new dictionary for the abilities, where the key is the name and the value is list of values of the
					// ability (some abilities have multiple abilities)
			        Dictionary<string, List<string>> abilities = new Dictionary<string, List<string>>();
			        string currentAbilityName = string.Empty;
			        
			        // Creates a string reader, so that the output of gPhoto2 can be read line by line
					using (StringReader stringReader = new StringReader(output))
					{
					    // Cycles over the each line of the output
						string line;
						while (!string.IsNullOrWhiteSpace(line = stringReader.ReadLine()))
						{
						    // Each line consists of an ability name and a value, which are separated by a colon (abilities with multiple
							// values have multiple lines, where only the first line contains the ability name, all following lines
							// contain an empty ability name and only the colon followed by the ability value
						    string[] splittedLine = line.Split(':');
						    if (splittedLine.Length != 2)
    						    continue;
    						string abilityName = splittedLine[0].Trim().ToUpperInvariant();
    						string abilityValue = splittedLine[1].Trim().ToUpperInvariant();
    						
    						// Checks if the current line is a new ability or just another value for the ability from the last line
    						if (!string.IsNullOrWhiteSpace(abilityName))
    						{
    						    abilities.Add(abilityName, new List<string>() { abilityValue });
    						    currentAbilityName = abilityName;
    						}
    						else if (string.IsNullOrWhiteSpace(abilityName) && !string.IsNullOrWhiteSpace(currentAbilityName))
    						{
    						    abilities[currentAbilityName].Add(abilityValue);
    						}
    						
    						// Processes the abilities, by reading out the values and storing them
    						if (abilities.ContainsKey("CAPTURE CHOICES") && abilities["CAPTURE CHOICES"].Contains("IMAGE"))
        						this.CanCaptureImages = true;
    						if (abilities.ContainsKey("CAPTURE CHOICES") && abilities["CAPTURE CHOICES"].Contains("PREVIEW"))
        						this.CanCapturePreviews = true;
        					if (abilities.ContainsKey("CONFIGURATION SUPPORT") && abilities["CONFIGURATION SUPPORT"].Contains("YES"))
            					this.CanBeConfigured = true;
        					if (abilities.ContainsKey("DELETE SELECTED FILES ON CAMERA") && abilities["DELETE SELECTED FILES ON CAMERA"].Contains("YES"))
            					this.CanDeleteFiles = true;
        					if (abilities.ContainsKey("DELETE ALL FILES ON CAMERA") && abilities["DELETE ALL FILES ON CAMERA"].Contains("YES"))
            					this.CanDeleteAllFiles = true;
        					if (abilities.ContainsKey("FILE PREVIEW (THUMBNAIL) SUPPORT") && abilities["FILE PREVIEW (THUMBNAIL) SUPPORT"].Contains("YES"))
            					this.CanPreviewFiles = true;
        					if (abilities.ContainsKey("FILE UPLOAD SUPPORT") && abilities["FILE UPLOAD SUPPORT"].Contains("YES"))
            					this.CanUploadFiles = true;
					    }
					    
        				// Since no asynchronous operation was performed, an already resolved task is returned
        				return Task.FromResult(0);
					}
			    });

			// Gets all of the settings of the camera
			this.Settings = await CameraSetting.GetCameraSettingsAsync(this.gPhoto2IpcWrapper);
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Iterates all cameras attached to the system and initializes them.
		/// </summary>
		/// <returns>Returns a read-only list of all cameras attached to the system.</returns>
		public static async Task<IReadOnlyCollection<Camera>> GetCamerasAsync()
		{
		    // Creates a new IPC wrapper, which can be used to interface with gPhoto2
		    GPhoto2IpcWrapper gPhoto2IpcWrapper = new GPhoto2IpcWrapper();
		    
			// Gets all the cameras attached to the computer
			List<Camera> createdCameras = await gPhoto2IpcWrapper.ExecuteAsync("--auto-detect", output =>
				{
					// Creates a new result list for the cameras
					List<Camera> cameras = new List<Camera>();

					// Creates a string reader, so that the output of gPhoto2 can be read line by line
					using (StringReader stringReader = new StringReader(output))
					{
						// Dismisses the first two lines because they only contain the header of the table containing the cameras
						stringReader.ReadLine();
						stringReader.ReadLine();

						// The line contains the name of the camera and its port separated by multiple whitespaces, this regular
						//expression is used to split them
						Regex cameraAndPortRegex = new Regex("^(?<Name>((\\S\\s\\S)|\\S)+)\\s\\s+(?<Port>((\\S\\s\\S)|\\S)+)$");

						// Cycles over the rest of the lines (each line representes a row in the table containing the cameras)
						string line;
						while (!string.IsNullOrWhiteSpace(line = stringReader.ReadLine()))
						{
							// Trims the line, because it might have leading or trailing whitespaces (the regular expression would be
							// more complex with them)
							line = line.Trim();

							// Reads the name and the port of the camera
							Match match = cameraAndPortRegex.Match(line);
							string cameraName = match.Groups["Name"].Value;
							string cameraPort = match.Groups["Port"].Value;

							// If either the camera name or the port is null or whitespace, then the camera could not be matched
							if (string.IsNullOrWhiteSpace(cameraName) || string.IsNullOrWhiteSpace(cameraPort))
								continue;

							// Creates the new camera and adds it to the result set
							Camera camera = new Camera(cameraName, cameraPort);
							cameras.Add(camera);
						}
					}

					// Returns all cameras that have been found by gPhoto2
					return Task.FromResult(cameras);
				});

			// Initializes the cameras that have been created (since all camera commands are executed in an action block, they must not
			// be nested, because otherwise they would end up in a dead lock, therefore the initialization is done outside of the
			// camera command)
			await Task.WhenAll(createdCameras.Select(createdCamera => createdCamera.InitializeAsync()));

			// Returns the initialized cameras
			return createdCameras;
		}

		#endregion
	}
}