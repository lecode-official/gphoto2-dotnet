
#region Using Directives

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#endregion

namespace System.Devices
{
    /// <summary>
    /// Represents a setting of a camera, whose value can be read or written (some are read-only).
    /// </summary>
    public class CameraSetting
    {
        #region Constructors
        
		/// <summary>
		/// Intializes a new <see cref="CameraSetting" /> instance. The constructor is made private, so that the factory pattern, which
        /// is used to instantiate new instances of <see cref="CameraSetting" />, can be enforced.
		/// </summary>
		/// <param name="settingName">The name of the setting.</param>
		/// <param name="gPhoto2IpcWrapper">
		/// The IPC wrapper, which is to be used to interface with gPhoto2. The IPC wrapper must be injected, because the setting should
        /// use the exact same IPC wrapper used by the camera (the IPC wrapper ensures that only one operation at a time is executed,
        /// which is important when interfacing with the camera). If two operations, e.g. setting a value and capturing an image, would
        /// be performed at the same time, the program would crash, because gPhoto2 can only do one thing at a time).
		/// </param>
		private CameraSetting(string settingName, GPhoto2IpcWrapper gPhoto2IpcWrapper)
		{
            // Stores the all information about the setting for later use
            this.Name = settingName;
		    this.gPhoto2IpcWrapper = gPhoto2IpcWrapper;
		}

        #endregion
        
        #region Private Fields
        
		/// <summary>
		/// Contains the IPC wrapper, which is used to interface with gPhoto2.
		/// </summary>
		private GPhoto2IpcWrapper gPhoto2IpcWrapper;
		
		/// <summary>
		/// Contains a value that determines whether the setting has already been initialized.
		/// </summary>
		private bool isInitialized;
		
        /// <summary>
        /// Contains the type of the setting, e.g. text or option.
        /// </summary>
        private CameraSettingType settingType;
        
        /// <summary>
        /// Contains a human-readable name for the setting.
        /// </summary>
        private string label;
        
        /// <summary>
        /// Contains a list of all possible choices (only set if the camera setting type is Option, otherwise this list is empty).
        /// </summary>
        private IReadOnlyCollection<string> choices;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Gets the name of the setting.
        /// </summary>
        public string Name { get; private set; }
        
        #endregion
        
        #region Private Methods
        
		/// <summary>
		/// Initializes a new setting (the intialization must be performed before getting or setting the value).
		/// </summary>
        private async Task InitializeAsync()
        {
            // Checks if the setting has already been initialized, if so then the setting does not need to be initialized again
            if (!this.isInitialized)
            {
                // Gets all the information about the setting
                await this.gPhoto2IpcWrapper.ExecuteInteractiveAsync(string.Format(CultureInfo.InvariantCulture, "get-config {0}", this.Name), output =>
                    {   
                        // Creates a string reader, so that the output of gPhoto2 can be read line by line
					    using (StringReader stringReader = new StringReader(output))
					    {
					        // Reads the first line, which contains the label, a human-readable name, of the setting
					        string line = stringReader.ReadLine();
					        string[] splittedLine = string.IsNullOrWhiteSpace(line) ? null : line.Split(':');
					        this.label = splittedLine.Length != 2 ? string.Empty : splittedLine[1].Trim();
					        
					        // Reads the second line, which contains the type of the setting
					        line = stringReader.ReadLine();
					        splittedLine = string.IsNullOrWhiteSpace(line) ? null : line.Split(':');
					        string typeName = splittedLine.Length != 2 ? string.Empty : splittedLine[1].Trim().ToUpperInvariant();
					        if (typeName == "TEXT")
					            this.settingType = CameraSettingType.Text;
				            else if (typeName == "RADIO" || typeName == "MENU")
					            this.settingType = CameraSettingType.Option;
				            else if (typeName == "DATE")
					            this.settingType = CameraSettingType.DateTime;
				            else if (typeName == "TOGGLE")
					            this.settingType = CameraSettingType.Text;
				            else
					            this.settingType = CameraSettingType.Unknown;
					        
					        // Reads the third line, which contains the current value, which is not needed right now and therefore discarded
					        stringReader.ReadLine();
					        
					        // Reads all following lines, which each contain a value choice, which is useful, when the setting is of type Option
					        List<string> choices = new List<string>();
					        if (this.settingType == CameraSettingType.Option)
					        {
						        // The line contains the number of the choice and the value as a string
						        Regex choiceRegex = new Regex("^Choice: [0-9]+ (?<Choice>(.+))$");
						        
				                // Cycles over each line to extract the choices
						        while (!string.IsNullOrWhiteSpace(line = stringReader.ReadLine()))
						        {
							        // Trims the line, because it might have leading or trailing whitespaces (the regular expression would be more complex with them)
							        line = line.Trim();

							        // Reads the choice from the match and adds it to the list of choices
							        Match match = choiceRegex.Match(line);
							        string choice = match.Groups["Choice"].Value;
							        if (string.IsNullOrWhiteSpace(choice))
								        continue;
							        choices.Add(choice);
						        }
						    }
						    this.choices = choices;
					    }
                        
        				// Since no asynchronous operation was performed, an already resolved task is returned
        				return Task.FromResult(0);
                    });
                    
                // In order to make sure, that the initialize method is only called once, we store a value that states that the setting is already initialized
                this.isInitialized = true;
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Retrieves the type of the setting asynchronously.
        /// </summary>
        /// <returns>Returns the type of the setting.</returns>
        public async Task<CameraSettingType> GetTypeAsync()
        {
            // Makes sure that the setting has been initialized
            await this.InitializeAsync();
            
            // Returns the type of the setting
            return this.settingType;
        }
        
        /// <summary>
        /// Retrieves the label of the setting, which is a human-readable name for the setting, asynchronously.
        /// </summary>
        /// <returns>Returns the label of the setting.</returns>
        public async Task<string> GetLabelAsync()
        {
            // Makes sure that the setting has been initialized
            await this.InitializeAsync();
            
            // Returns the label of the setting
            return this.label;
        }
        
        /// <summary>
        /// Retrieves the choices for the values of the setting, which are only relevant if the setting is of type option, asynchronously.
        /// </summary>
        /// <returns>Returns the choices of the setting.</returns>
        public async Task<IReadOnlyCollection<string>> GetChoicesAsync()
        {
            // Makes sure that the setting has been initialized
            await this.InitializeAsync();
            
            // Returns the choices of the setting
            return this.choices;
        }
        
        #endregion
        
        #region Internal Static Methods
        
		/// <summary>
		/// Iterates all settings of the specified camera and initializes them.
		/// </summary>
		/// <param name="gPhoto2IpcWrapper">
		/// The IPC wrapper, which is to be used to interface with gPhoto2. The IPC wrapper must be injected, because the settings should use the exact same IPC wrapper
		/// used by the camera (the IPC wrapper ensures that only one operation at a time is executed, which is important when interfacing with the camera). If two
		/// operations, e.g. setting a value and capturing an image, would be performed at the same time, the program would crash, because gPhoto2 can only do one thing
		/// at a time).
		/// </param>
		/// <returns>Returns a read-only list containing all settings of the specified camera.</returns>
		internal static async Task<IEnumerable<CameraSetting>> GetCameraSettingsAsync(GPhoto2IpcWrapper gPhoto2IpcWrapper)
		{
		    // Gets all the settings of the specified camera and returns them
			return await gPhoto2IpcWrapper.ExecuteInteractiveAsync("list-config", output =>
				{
					// Creates a new result list for the camera settings
					List<CameraSetting> cameraSettings = new List<CameraSetting>();

					// Creates a string reader, so that the output of gPhoto2 can be read line by line
					using (StringReader stringReader = new StringReader(output))
					{
					    // Cycles over the each line of the output and creates a new setting (each line contains the name of the setting)
						string line;
						while (!string.IsNullOrWhiteSpace(line = stringReader.ReadLine()))
						    cameraSettings.Add(new CameraSetting(line.Trim(), gPhoto2IpcWrapper));
					}

					// Returns all settings that have been found by gPhoto2 for the specified camera
					return Task.FromResult(cameraSettings);
				});
		}
		
        #endregion
    }
}