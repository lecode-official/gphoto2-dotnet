
#region Using Directives

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
    /// Represents a configuration of a camera, whose value can be read or written (some are read-only).
    /// </summary>
    public class CameraConfiguration
    {
        #region Constructors
        
        /// <summary>
        /// Intializes a new <see cref="CameraConfiguration" /> instance. The constructor is made internal, so that the factory pattern,
        /// which is used to instantiate new instances of <see cref="CameraConfiguration" />, can be enforced.
        /// </summary>
        /// <param name="configurationName">The name of the configuration.</param>
        /// <param name="gPhoto2IpcWrapper">
        /// The IPC wrapper, which is to be used to interface with gPhoto2. The IPC wrapper must be injected, because the configuration
        /// should use the exact same IPC wrapper used by the camera (the IPC wrapper ensures that only one operation at a time is executed,
        /// which is important when interfacing with the camera). If two operations, e.g. setting a value and capturing an image, would
        /// be performed at the same time, the program would crash, because gPhoto2 can only do one thing at a time).
        /// </param>
        internal CameraConfiguration(string configurationName, GPhoto2IpcWrapper gPhoto2IpcWrapper)
        {
            // Stores the all information about the configuration for later use
            this.Name = configurationName;
            this.gPhoto2IpcWrapper = gPhoto2IpcWrapper;
        }

        #endregion
        
        #region Private Fields
        
        /// <summary>
        /// Contains the IPC wrapper, which is used to interface with gPhoto2.
        /// </summary>
        private GPhoto2IpcWrapper gPhoto2IpcWrapper;
        
        /// <summary>
        /// Contains a value that determines whether the configuration has already been initialized.
        /// </summary>
        private bool isInitialized;
        
        /// <summary>
        /// Contains the type of the configuration, e.g. text or option.
        /// </summary>
        private CameraConfigurationType configurationType;
        
        /// <summary>
        /// Contains a human-readable name for the configuration.
        /// </summary>
        private string label;
        
        /// <summary>
        /// Contains the last read current value of the configuration.
        /// </summary>
        private string currentValue;
        
        /// <summary>
        /// Contains a list of all possible choices (only set if the camera configuration type is Option, otherwise this list is empty).
        /// </summary>
        private IEnumerable<string> choices;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Gets the name of the configuration.
        /// </summary>
        public string Name { get; private set; }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Initializes a new configuration (the intialization must be performed before getting or configuration the value).
        /// </summary>
        /// <exception cref="CameraConfiguration">
        /// If anything goes wrong during the initialization of the camera configuration, then a <see cref="CameraConfiguration" /> exception is
        /// thrown.
        /// </exception>
        private async Task InitializeAsync()
        {
            // Gets all the information about the configuration
            await this.gPhoto2IpcWrapper.ExecuteInteractiveAsync(string.Format(CultureInfo.InvariantCulture, "get-config {0}", this.Name), output =>
                {   
                    // Creates a string reader, so that the output of gPhoto2 can be read line by line
                    using (StringReader stringReader = new StringReader(output))
                    {
                        // Reads the first line, which contains the label, a human-readable name, of the configuration
                        string line = stringReader.ReadLine();
                        string[] splittedLine = string.IsNullOrWhiteSpace(line) ? null : line.Split(':');
                        this.label = splittedLine.Length != 2 ? string.Empty : splittedLine[1].Trim();
                        
                        // Reads the second line, which contains the type of the configuration
                        line = stringReader.ReadLine();
                        splittedLine = string.IsNullOrWhiteSpace(line) ? null : line.Split(':');
                        string typeName = splittedLine.Length != 2 ? string.Empty : splittedLine[1].Trim().ToUpperInvariant();
                        if (typeName == "TEXT")
                            this.configurationType = CameraConfigurationType.Text;
                        else if (typeName == "RADIO" || typeName == "MENU")
                            this.configurationType = CameraConfigurationType.Option;
                        else if (typeName == "DATE")
                            this.configurationType = CameraConfigurationType.DateTime;
                        else if (typeName == "TOGGLE")
                            this.configurationType = CameraConfigurationType.Text;
                        else
                            this.configurationType = CameraConfigurationType.Unknown;
                        
                        // Reads the third line, which contains the current value, parses it and stores the current value of the configuration
                        Regex currentValueRegex = new Regex("^Current: (?<Value>(.*))$");
                        line = stringReader.ReadLine().Trim();
                        Match match = currentValueRegex.Match(line);
                        this.currentValue = match.Groups["Value"].Value;
                        
                        // Reads all following lines, which each contain a value choice, which is useful, when the configuration is of type
                        // Option
                        List<string> choices = new List<string>();
                        if (this.configurationType == CameraConfigurationType.Option)
                        {
                            // The line contains the number of the choice and the value as a string
                            Regex choiceRegex = new Regex("^Choice: [0-9]+ (?<Choice>(.+))$");
                            
                            // Cycles over each line to extract the choices
                            while (!string.IsNullOrWhiteSpace(line = stringReader.ReadLine()))
                            {
                                // Trims the line, because it might have leading or trailing whitespaces (the regular expression would
                                // be more complex with them)
                                line = line.Trim();

                                // Reads the choice from the match and adds it to the list of choices
                                match = choiceRegex.Match(line);
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
                
            // In order to make sure, that the initialize method is only called once, we store a value that states that the configuration is
            // already initialized
            this.isInitialized = true;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Retrieves the type of the configuration asynchronously.
        /// </summary>
        /// <exception cref="CameraConfiguration">
        /// If anything goes wrong during the retrieval of the type, then a <see cref="CameraConfiguration" /> exception is thrown.
        /// </exception>
        /// <returns>Returns the type of the configuration.</returns>
        public async Task<CameraConfigurationType> GetTypeAsync()
        {
            // Checks if the configuration has already been initialized, if so then the configuration does not need to be initialized again
            if (!this.isInitialized)
                await this.InitializeAsync();
            
            // Returns the type of the configuration
            return this.configurationType;
        }
        
        /// <summary>
        /// Retrieves the label of the configuration, which is a human-readable name for the configuration, asynchronously.
        /// </summary>
        /// <exception cref="CameraConfiguration">
        /// If anything goes wrong during the retrieval of the label, then a <see cref="CameraConfiguration" /> exception is thrown.
        /// </exception>
        /// <returns>Returns the label of the configuration.</returns>
        public async Task<string> GetLabelAsync()
        {
            // Checks if the configuration has already been initialized, if so then the configuration does not need to be initialized again
            if (!this.isInitialized)
                await this.InitializeAsync();
            
            // Returns the label of the configuration
            return this.label;
        }
        
        /// <summary>
        /// Retrieves the choices for the values of the configuration, which are only relevant if the configuration is of type option, asynchronously.
        /// </summary>
        /// <exception cref="CameraConfiguration">
        /// If anything goes wrong during the retrieval of the choices, then a <see cref="CameraConfiguration" /> exception is thrown.
        /// </exception>
        /// <returns>Returns the choices of the configuration.</returns>
        public async Task<IEnumerable<string>> GetChoicesAsync()
        {
            // Checks if the configuration has already been initialized, if so then the configuration does not need to be initialized again
            if (!this.isInitialized)
                await this.InitializeAsync();
            
            // Returns the choices of the configuration
            return this.choices;
        }
        
        /// <summary>
        /// Retrieves the current value of the configuration asynchronously.
        /// </summary>
        /// <exception cref="CameraConfiguration">
        /// If anything goes wrong during the retrieval of the current value, then a <see cref="CameraConfiguration" /> exception is thrown.
        /// </exception>
        /// <returns>Returns the current value of the configuration.</returns>
        public async Task<string> GetValueAsync()
        {
            // Initializes the configuration (during the initialization the current value of the configuration is read, therefore the configuration is
            // re-initialized, even if it has been initialized before, so that the current value is retrieved)
            await this.InitializeAsync();
            
            // Returns the current value of the configuration
            return this.currentValue;
        }
        
        /// <summary>
        /// Sets a new value for the specified camera configuration.
        /// </summary>
        /// <param name="value">The new value that is to be set.</param>
        /// <exception cref="CameraConfiguration">
        /// If the value is not valid or anything goes wrong during configuration the value, then a <see cref="CameraConfiguration" /> exception is
        /// thrown.
        /// </exception>
        public async Task SetValueAsync(string value)
        {
            // Checks if the configuration has already been initialized, if so then the configuration does not need to be initialized again
            if (!this.isInitialized)
                await this.InitializeAsync();
            
            // Validates the value that is to be set, if the value could not be validated, then a camera exception is thrown
            bool isValueValid = false;
            switch (this.configurationType)
            {
                case CameraConfigurationType.Unknown:
                    throw new CameraException("The value of the configuration could not be set, because its type is Unknown.");
                case CameraConfigurationType.Text:
                    isValueValid = true;
                    break;
                case CameraConfigurationType.Option:
                    isValueValid = this.choices.Any(choice => choice == value);
                    break;
                case CameraConfigurationType.Toggle:
                    isValueValid = value == "0" || value == "1";
                    break;
                case CameraConfigurationType.DateTime:
                    isValueValid = value.All(character => char.IsDigit(character));
                    break;
            }
            if (!isValueValid)
                throw new CameraException("The value of the configuration could not be set, because the specified value is invalid for the camera configuration.");
            
            // Executes the set value command on the camera
            await gPhoto2IpcWrapper.ExecuteInteractiveAsync(string.Format(CultureInfo.InvariantCulture, "set-config {0}={1}",
                this.Name, value));
        }
        
        #endregion
        
        #region Internal Static Methods
        
        /// <summary>
        /// Iterates all configurations of the specified camera and initializes them.
        /// </summary>
        /// <param name="gPhoto2IpcWrapper">
        /// The IPC wrapper, which is to be used to interface with gPhoto2. The IPC wrapper must be injected, because the configurations should
        /// use the exact same IPC wrapper used by the camera (the IPC wrapper ensures that only one operation at a time is executed,
        /// which is important when interfacing with the camera). If two operations, e.g. configuration a value and capturing an image, would
        /// be performed at the same time, the program would crash, because gPhoto2 can only do one thing at a time).
        /// </param>
        /// <exception cref="CameraConfiguration">
        /// If anything goes wrong during the retrieval of the camera configurations, then a <see cref="CameraConfiguration" /> exception is thrown.
        /// </exception>
        /// <returns>Returns a read-only list containing all configurations of the specified camera.</returns>
        internal static async Task<IEnumerable<CameraConfiguration>> GetCameraConfigurationsAsync(GPhoto2IpcWrapper gPhoto2IpcWrapper)
        {
            // Gets all the configurations of the specified camera and returns them
            return await gPhoto2IpcWrapper.ExecuteInteractiveAsync("list-config", output =>
                {
                    // Creates a new result list for the camera configurations
                    List<CameraConfiguration> CameraConfigurations = new List<CameraConfiguration>();

                    // Creates a string reader, so that the output of gPhoto2 can be read line by line
                    using (StringReader stringReader = new StringReader(output))
                    {
                        // Cycles over the each line of the output and creates a new configuration (each line contains the name of the configuration)
                        string line;
                        while (!string.IsNullOrWhiteSpace(line = stringReader.ReadLine()))
                            CameraConfigurations.Add(new CameraConfiguration(line.Trim(), gPhoto2IpcWrapper));
                    }

                    // Returns all configurations that have been found by gPhoto2 for the specified camera
                    return Task.FromResult(CameraConfigurations);
                });
        }
        
        #endregion
    }
}