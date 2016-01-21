
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
		public IEnumerable<CameraSetting> Settings { get; private set; }
		
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

		#region Public Static Methods

		/// <summary>
		/// Iterates all cameras attached to the system and initializes them.
		/// </summary>
		/// <returns>Returns a read-only list of all cameras attached to the system.</returns>
		public static async Task<IEnumerable<Camera>> GetCamerasAsync()
		{
		    // Creates a new IPC wrapper, which can be used to interface with gPhoto2
		    GPhoto2IpcWrapper gPhoto2IpcWrapper = new GPhoto2IpcWrapper
			{
				StandardCommandLineParameters = "--quiet"
			};
		    
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
        
		#region Private Methods

		/// <summary>
		/// Initializes a new camera (this is the internal factory method for instantiating new cameras).
		/// </summary>
		private async Task InitializeAsync()
		{
			// Sets the camera name and its port as standard command line parameters, so that they do not have to be stated over and
			// over again explictly
			this.gPhoto2IpcWrapper.StandardCommandLineParameters = string.Format(CultureInfo.InvariantCulture,
				"{0} --camera \"{1}\" --port \"{2}\"", this.gPhoto2IpcWrapper.StandardCommandLineParameters, this.Name, this.Port);
			    
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
        
        #region Public Image Capturing Methods
        
        /// <summary>
        /// Captures an image and stores it on the camera.
        /// <summary>
        /// <returns>Returns the file name of the image that was captured on the storage of the camera.</returns>
        public async Task<string> CaptureImageAsync()
        {
            // Executes the image capturing on the camera and retrieves the file name of the image that was captured
            return await this.gPhoto2IpcWrapper.ExecuteInteractiveAsync("capture-image", output =>
                {
                    // Creates a regular expression that parses the file name of the image that was captured
                    Regex fileNameRegex = new Regex("^New file is in location (?<FileName>(.+)) on the camera$");
                    
                    // Parses the output of the camera using the file name regular expression and checks if a file name could be retrieved, if not
                    // then an exception is thrown
                    Match match = fileNameRegex.Match(output.Trim());
					string parsedFileName = match.Groups["FileName"].Value;
                    if (string.IsNullOrWhiteSpace(parsedFileName))
                        throw new CameraException("The image could not be properly captured, because of an unknown reason.");
                    
                    // Returns the file name of the image that was captured
                    return Task.FromResult(parsedFileName);
                });
        }
        
        /// <summary>
        /// Captures the specified amount of images in the specified interval.
        /// </summary>
        /// <param name="amount">The number of images that are to be captured.</param>
        /// <param name="interval">The amount of time that is waited between capturing two images.</param>
        /// <exception cref="">If the amount of images or the interval is negative, then a <see cref="CameraException"/> is thrown.</exception>
        public async Task CaptureImagesAsync(int amount, TimeSpan interval)
        {
            // Checks if the amount of image or the interval is negative, if so then an exception is thrown
            if (amount < 0)
                throw new CameraException("The amount of images must not be negative.");
            if (interval < TimeSpan.Zero)
                throw new CameraException("The interval between the capturing of two images must not be negative.");
            
            // If the amount of images that are to be taken is 0, then nothing is done
            if (amount == 0)
                return;
            
            // Takes the specified amount of images
            for (int i = 0; i < amount; i++)
            {
                // Captures the image
                await this.CaptureImageAsync();
                
                // Checks if this is the last image that is to be taken, if not, then the interval is waited
                if (i < amount - 1 && interval > TimeSpan.Zero)
                    await Task.Delay(interval);
            }
        }
        
        /// <summary>
        /// Captures the specified amount of images.
        /// </summary>
        /// <param name="amount">The number of images that are to be captured.</param>
        /// <exception cref="">If the amount of images that are to be cpatured is negative, then a <see cref="CameraException"/> is thrown.</exception>
        public Task CaptureImagesAsync(int amount)
        {
            return this.CaptureImagesAsync(amount, TimeSpan.Zero);
        }
        
        #endregion
        
        #region Public Image Settings Methods
        
        /// <summary>
        /// Retrieves the current ISO speed of the camera.
        /// </summary>
        /// <exception cref="CameraSettingNotSupportedException">
        /// If the camera setting is not supported by the camera, then a <see cref="CameraSettingNotSupportedException"/> exception is thrown.
        /// </exception>
        /// <returns>Returns the current ISO speed of the camera. If the ISO speed is set to Auto, then 0 is returned.</returns>
        public async Task<int> GetIsoSpeedAsync()
        {
            // Gets the ISO speed camera setting and checks if it exists, if it does not exist, then an exception is thrown
            CameraSetting isoSpeedCameraSetting = this.Settings.FirstOrDefault(setting => setting.Name == CameraSettings.IsoSpeed);
            if (isoSpeedCameraSetting == null)
                throw new CameraException("The camera setting for the ISO speed is not supported by this camera");
            
            // Retrieves the current ISO speed of the camera
            string currentIsoSpeedTextualRepresentation = await isoSpeedCameraSetting.GetValueAsync();
            
            // Checks if the ISO speed is set to Auto, if so then 0 is returned, otherwise the ISO speed is converted to an integer and returned
            if (currentIsoSpeedTextualRepresentation.ToUpperInvariant() == "AUTO")
                return IsoSpeeds.Auto;
            else
                return int.Parse(currentIsoSpeedTextualRepresentation, CultureInfo.InvariantCulture);
        }
        
        /// <summary>
        /// Sets the ISO speed of the camera.
        /// </summary>
        /// <param name="isoSpeed">
        /// The ISO speed to which the camera is to be set. An ISO speed of 0 sets the camera to an Auto ISO speed, which means that the camera
        /// determines the correct ISO speed for the current lighting conditions
        /// </param>
        /// <exception cref="CameraSettingNotSupportedException">
        /// If the camera setting is not supported by the camera, then a <see cref="CameraSettingNotSupportedException"/> exception is thrown.
        /// </exception>
        public async Task SetIsoSpeedAsync(int isoSpeed)
        {
            // Gets the ISO speed camera setting and checks if it exists, if it does not exist, then an exception is thrown
            CameraSetting isoSpeedCameraSetting = this.Settings.FirstOrDefault(setting => setting.Name == CameraSettings.IsoSpeed);
            if (isoSpeedCameraSetting == null)
                throw new CameraException("The camera setting for the ISO speed is not supported by this camera");
            
            // Tries to set the new ISO speed, if an exception is thrown, then the ISO speed is not supported by the camera
            try
            {
                await isoSpeedCameraSetting.SetValueAsync(isoSpeed == 0 ? "Auto" : isoSpeed.ToString());
            }
            catch (CameraException exception)
            {
                throw new CameraException(string.Format(CultureInfo.InvariantCulture, "The ISO speed of {0} is not supported by the camera.", isoSpeed == 0 ? "Auto" : isoSpeed.ToString()), exception);
            }
        }
        
        /// <summary>
        /// Gets all ISO speeds, that are supported by the camera.
        /// </summary>
        /// <exception cref="CameraSettingNotSupportedException">
        /// If the camera setting is not supported by the camera, then a <see cref="CameraSettingNotSupportedException"/> exception is thrown.
        /// </exception>
        /// <returns>
        /// Returns a list of all the ISO speeds that are supported by the camera. The ISO speed 0 means that the camera automatically detects the
        /// correct ISO speed for the current lighting conditions (Auto).
        /// </returns>
        public async Task<IEnumerable<int>> GetSupportedIsoSpeedsAsync()
        {
            // Gets the ISO speed camera setting and checks if it exists, if it does not exist, then an exception is thrown
            CameraSetting isoSpeedCameraSetting = this.Settings.FirstOrDefault(setting => setting.Name == CameraSettings.IsoSpeed);
            if (isoSpeedCameraSetting == null)
                throw new CameraException("The camera setting for the ISO speed is not supported by this camera");
            
            // Gets all the choices, converts them to intergers and returns them
            return (await isoSpeedCameraSetting.GetChoicesAsync()).Select(choice => choice.ToUpperInvariant() == "AUTO" ? 0 : int.Parse(choice, CultureInfo.InvariantCulture));
        }
        
        /// <summary>
        /// Gets the current shutter speed of the camera.
        /// </summary>
        /// <exception cref="CameraSettingNotSupportedException">
        /// If the camera setting is not supported by the camera or the shutter speed could not be retrieved properly, then a
        /// <see cref="CameraSettingNotSupportedException"/> exception is thrown.
        /// </exception>
        /// <returns>
        /// Returns the current shutter speed of the camera. A shutter speed of <c>TimeSpan.MaxValue</c> means Bulb mode, where the camera exposes
        /// the image for as long as the release is pressed.
        /// </returns>
        public async Task<TimeSpan> GetShutterSpeedAsync()
        {
            // Gets the shutter speed camera setting and checks if it exists, if it does not exist, then an exception is thrown
            CameraSetting shutterSpeedCameraSetting = this.Settings.FirstOrDefault(setting => setting.Name == CameraSettings.ShutterSpeed);
            if (shutterSpeedCameraSetting == null)
                throw new CameraException("The camera setting for the shutter speed is not supported by this camera");
            
            // Retrieves the current shutter speed of the camera
            string currentShutterSpeedTextualRepresentation = await shutterSpeedCameraSetting.GetValueAsync();
            
            // Checks if the ISO speed is set to Bulb, if so then TimeSpan.MaxValue is returned, otherwise the shutter speed is converted to a TimeSpan and returned
            if (currentShutterSpeedTextualRepresentation.ToUpperInvariant() == "BULB")
            {
                return TimeSpan.MaxValue;
            }
            else
            {
                // Parses the shutter speed, the shutter speed is either specified as floating point number of seconds or as a fraction of seconds
                if (currentShutterSpeedTextualRepresentation.Contains("/"))
                {
                    string[] fractionElements = currentShutterSpeedTextualRepresentation.Split('/');
                    if (fractionElements.Length < 2)
                        throw new CameraException("The shutter speed could not be properly retrieved for an unknown reason.");
                    double numerator = int.Parse(fractionElements[0], CultureInfo.InvariantCulture);
                    double denominator = int.Parse(fractionElements[1], CultureInfo.InvariantCulture);
                    return TimeSpan.FromSeconds(numerator / denominator);
                }
                else
                {
                    return TimeSpan.FromSeconds(double.Parse(currentShutterSpeedTextualRepresentation, CultureInfo.InvariantCulture));
                }
            }
        }
        
        /// <summary>
        /// Sets the shutter speed of the camera.
        /// </summary>
        /// <param name="shutterSpeed">
        /// The shutter speed to which the camera is to be set. A shutter speed of <c>TimeSpan.MaxValue</c> sets the camera to a Bulb shutter
        /// speed, which means that the camera exposes the image for as long as the release is pressed.
        /// </param>
        /// <exception cref="CameraSettingNotSupportedException">
        /// If the camera setting is not supported by the camera, then a <see cref="CameraSettingNotSupportedException"/> exception is thrown.
        /// </exception>
        public async Task SetShutterSpeedAsync(TimeSpan shutterSpeed)
        {
            // Gets the shutter speed camera setting and checks if it exists, if it does not exist, then an exception is thrown
            CameraSetting shutterSpeedCameraSetting = this.Settings.FirstOrDefault(setting => setting.Name == CameraSettings.ShutterSpeed);
            if (shutterSpeedCameraSetting == null)
                throw new CameraException("The camera setting for the shutter speed is not supported by this camera");
            
            // Gets all the choices and converts them to TimeSpans, which is needed to determine the correct value for the shutter speed to set
            Dictionary<TimeSpan, string> supportedShutterSpeeds = new Dictionary<TimeSpan, string>();
            foreach (string choice in await shutterSpeedCameraSetting.GetChoicesAsync())
            {
                if (choice.ToUpperInvariant() == "BULB")
                {
                    supportedShutterSpeeds.Add(TimeSpan.MaxValue, choice);
                }
                else if (choice.Contains("/"))
                {
                    string[] fractionElements = choice.Split('/');
                    if (fractionElements.Length < 2)
                        throw new CameraException("The shutter speed could not be properly set for an unknown reason.");
                    double numerator = double.Parse(fractionElements[0], CultureInfo.InvariantCulture);
                    double denominator = double.Parse(fractionElements[1], CultureInfo.InvariantCulture);
                    supportedShutterSpeeds.Add(TimeSpan.FromTicks(Convert.ToInt64(numerator / denominator * 1000000L)), choice);
                }
                else
                {
                    supportedShutterSpeeds.Add(TimeSpan.FromSeconds(double.Parse(choice, CultureInfo.InvariantCulture)), choice);
                }
            }
            
            // Chooses the correct shutter speed value from the available choices and sets it, if the shutter speed could not be detected,
            // then an exception is thrown
            if (!supportedShutterSpeeds.ContainsKey(shutterSpeed))
                throw new CameraException(string.Format(CultureInfo.InvariantCulture, "The shutter speed {0} is not supported by the camera.", shutterSpeed));
            await shutterSpeedCameraSetting.SetValueAsync(supportedShutterSpeeds[shutterSpeed]);
        }
        
        /// <summary>
        /// Gets all shutter speeds, that are supported by the camera.
        /// </summary>
        /// <exception cref="CameraSettingNotSupportedException">
        /// If the camera setting is not supported by the camera, then a <see cref="CameraSettingNotSupportedException"/> exception is thrown.
        /// </exception>
        /// <returns>
        /// Returns a list of all the shutter speeds that are supported by the camera. A shutter speed of <c>TimeSpan.MaxValue</c> means that the
        /// camera is set Bulb mode, where the camera exposes the image for as long as the release is pressed.
        /// </returns>
        public async Task<IEnumerable<TimeSpan>> GetSupportedShutterSpeedsAsync()
        {
            // Gets the shutter speed camera setting and checks if it exists, if it does not exist, then an exception is thrown
            CameraSetting shutterSpeedCameraSetting = this.Settings.FirstOrDefault(setting => setting.Name == CameraSettings.ShutterSpeed);
            if (shutterSpeedCameraSetting == null)
                throw new CameraException("The camera setting for the shutter speed is not supported by this camera");
            
            // Gets all the choices, converts them to TimeSpans and returns them
            List<TimeSpan> supportedShutterSpeeds = new List<TimeSpan>();
            foreach (string choice in await shutterSpeedCameraSetting.GetChoicesAsync())
            {
                if (choice.ToUpperInvariant() == "BULB")
                {
                    supportedShutterSpeeds.Add(TimeSpan.MaxValue);
                }
                else if (choice.Contains("/"))
                {
                    string[] fractionElements = choice.Split('/');
                    if (fractionElements.Length < 2)
                        throw new CameraException("The shutter speed could not be properly retrieved for an unknown reason.");
                    double numerator = double.Parse(fractionElements[0], CultureInfo.InvariantCulture);
                    double denominator = double.Parse(fractionElements[1], CultureInfo.InvariantCulture);
                    supportedShutterSpeeds.Add(TimeSpan.FromTicks(Convert.ToInt64(numerator / denominator * 1000000L)));
                }
                else
                {
                    supportedShutterSpeeds.Add(TimeSpan.FromSeconds(double.Parse(choice, CultureInfo.InvariantCulture)));
                }
            }
            
            // Returns the list of shutter speeds supported by the camera
            return supportedShutterSpeeds;
        }
        
        /// <summary>
        /// Retrieves the current aperture of the camera.
        /// </summary>
        /// <exception cref="CameraSettingNotSupportedException">
        /// If the camera setting is not supported by the camera, then a <see cref="CameraSettingNotSupportedException"/> exception is thrown.
        /// </exception>
        /// <returns>Returns the current aperture of the camera.</returns>
        public async Task<double> GetApertureAsync()
        {
            // Gets the aperture camera setting and checks if it exists, if it does not exist, then an exception is thrown
            CameraSetting apertureCameraSetting = this.Settings.FirstOrDefault(setting => setting.Name == CameraSettings.Aperture);
            if (apertureCameraSetting == null)
                throw new CameraException("The camera setting for the aperture is not supported by this camera");
            
            // Retrieves the current aperture of the camera
            string currentApertureTextualRepresentation = await apertureCameraSetting.GetValueAsync();
            
            // Parses the aperture and returns it
            return double.Parse(currentApertureTextualRepresentation, CultureInfo.InvariantCulture);
        }
        
        #endregion
        
        #region Public Camera Settings Methods
        
        /// <summary>
        /// Retrieves the name of the owner of the camera.
        /// </summary>
        /// <exception cref="CameraSettingNotSupportedException">
        /// If the camera setting is not supported by the camera, then a <see cref="CameraSettingNotSupportedException"/> exception is thrown.
        /// </exception>
        /// <returns>Returns the name of the owner of the camera.</returns>
        public async Task<string> GetOwnerNameAsync()
        {
            // Gets the owner name camera setting and checks if it exists, if it does not exist, then an exception is thrown
            CameraSetting ownerNameCameraSetting = this.Settings.FirstOrDefault(setting => setting.Name == CameraSettings.OwnerName);
            if (ownerNameCameraSetting == null)
                throw new CameraException("The camera setting for the owner name is not supported by this camera");
            
            // Retrieves the owner name and returns it
            return await ownerNameCameraSetting.GetValueAsync();
        }
        
        /// <summary>
        /// Sets the name of the owner of the camera.
        /// </summary>
        /// <param name="name">The new name of the owner of the camera, that is to be set.</param>
        /// <exception cref="CameraSettingNotSupportedException">
        /// If the camera setting is not supported by the camera, then a <see cref="CameraSettingNotSupportedException"/> exception is thrown.
        /// </exception>
        public async Task SetOwnerNameAsync(string name)
        {
            // Gets the owner name camera setting and checks if it exists, if it does not exist, then an exception is thrown
            CameraSetting ownerNameCameraSetting = this.Settings.FirstOrDefault(setting => setting.Name == CameraSettings.OwnerName);
            if (ownerNameCameraSetting == null)
                throw new CameraException("The camera setting for the owner name is not supported by this camera");

            // Sets the new name of the owner of the camera
            await ownerNameCameraSetting.SetValueAsync(name);
        }
        
        /// <summary>
        /// Retrieves the name of the manufacturer of the camera.
        /// </summary>
        /// <exception cref="CameraSettingNotSupportedException">
        /// If the camera setting is not supported by the camera, then a <see cref="CameraSettingNotSupportedException"/> exception is thrown.
        /// </exception>
        /// <returns>Returns the name of the manufacturer of the camera.</returns>
        public async Task<string> GetManufacturerAsync()
        {
            // Gets the manufacturer camera setting and checks if it exists, if it does not exist, then an exception is thrown
            CameraSetting manufacturerCameraSetting = this.Settings.FirstOrDefault(setting => setting.Name == CameraSettings.Manufacturer);
            if (manufacturerCameraSetting == null)
                throw new CameraException("The camera setting for the camera manufacturer is not supported by this camera");
            
            // Retrieves the manufacturer and returns it
            return await manufacturerCameraSetting.GetValueAsync();
        }
        
        #endregion
        
        #region Public Camera Status Methods
        
        /// <summary>
        /// Retrieves the name of the camera model.
        /// </summary>
        /// <exception cref="CameraSettingNotSupportedException">
        /// If the camera setting is not supported by the camera, then a <see cref="CameraSettingNotSupportedException"/> exception is thrown.
        /// </exception>
        /// <returns>Returns the name of the camera model.</returns>
        public async Task<string> GetCameraModelAsync()
        {
            // Gets the camera model setting and checks if it exists, if it does not exist, then an exception is thrown
            CameraSetting cameraModelCameraSetting = this.Settings.FirstOrDefault(setting => setting.Name == CameraSettings.CameraModel);
            if (cameraModelCameraSetting == null)
                throw new CameraException("The camera setting for the camera model is not supported by this camera");
            
            // Retrieves the camera model and returns it
            return await cameraModelCameraSetting.GetValueAsync();
        }
        
        /// <summary>
        /// Retrieves the name of the lens of the camera.
        /// </summary>
        /// <exception cref="CameraSettingNotSupportedException">
        /// If the camera setting is not supported by the camera, then a <see cref="CameraSettingNotSupportedException"/> exception is thrown.
        /// </exception>
        /// <returns>Returns the name of the lens of the camera.</returns>
        public async Task<string> GetLensNameAsync()
        {
            // Gets the lens name camera setting and checks if it exists, if it does not exist, then an exception is thrown
            CameraSetting lensNameCameraSetting = this.Settings.FirstOrDefault(setting => setting.Name == CameraSettings.LensName);
            if (lensNameCameraSetting == null)
                throw new CameraException("The camera setting for the lens name is not supported by this camera");
            
            // Retrieves the name of the lens of the camera and returns it
            return await lensNameCameraSetting.GetValueAsync();
        }
        
        /// <summary>
        /// Retrieves the battery level of the camera.
        /// </summary>
        /// <exception cref="CameraSettingNotSupportedException">
        /// If the camera setting is not supported by the camera, then a <see cref="CameraSettingNotSupportedException"/> exception is thrown.
        /// </exception>
        /// <returns>Returns the battery level of the camera.</returns>
        public async Task<string> GetBatteryLevelAsync()
        {
            // Gets the battery level camera setting and checks if it exists, if it does not exist, then an exception is thrown
            CameraSetting batteryLevelCameraSetting = this.Settings.FirstOrDefault(setting => setting.Name == CameraSettings.BatteryLevel);
            if (batteryLevelCameraSetting == null)
                throw new CameraException("The camera setting for the battery level is not supported by this camera");
            
            // Retrieves the battery level of the camera and returns it
            return await batteryLevelCameraSetting.GetValueAsync();
        }
        
        #endregion
	}
}