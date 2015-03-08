
#region Using Directives

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace System.Devices
{
	/// <summary>
	/// Represents a helper class for inter-process communication with a command line interface program.
	/// </summary>
	internal class IpcWrapper
	{
		#region Constructors

		/// <summary>
		/// Initializes a new <see cref="IpcWrapper" /> instance.
		/// </summary>
		/// <param name="programFileName">
		/// The file name of the program to which the <see cref="IpcWrapper" /> should establish a connection.
		/// </param>
		/// <param name="culture">
		/// The culture in which the application is to be started (this can be helpful when parsing the output of the application,
		/// because sometimes it can get hard to parse the output if the language is unknown).
		/// </param>
		public IpcWrapper(string programFileName, CultureInfo culture)
		{
			this.ProgramFileName = programFileName;
			this.Culture = culture;
		}

		/// <summary>
		/// Initializes a new <see cref="IpcWrapper" /> instance.
		/// </summary>
		/// <param name="programFileName">
		/// The file name of the program to which the <see cref="IpcWrapper" /> should establish a connection.
		/// </param>
		public IpcWrapper(string programFileName)
			: this(programFileName, CultureInfo.CurrentCulture)
		{
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the file name of the program to which the <see cref="IpcWrapper" /> should establish a connection.
		/// </summary>
		public string ProgramFileName { get; set; }

		/// <summary>
		/// Gets or sets the culture in which the application is to be started (this can be helpful when parsing the output of the
		/// application, because sometimes it can get hard to parse the output if the language is unknown).
		/// </summary>
		public CultureInfo Culture { get; set; }

		#endregion

		#region Public Methods

		/// <summary>
		/// Executes the process and retrieves its output.
		/// </summary>
		/// <param name="commandLineParameters">The command line paramters, which are passed to the process.</param>
		/// <returns>Returns what the process wrote to the standard output.</returns>
		public Task<string> ExecuteAsync(string commandLineParameters)
		{
			// Executes the command in a background thread and returns a task, which can be awaited by the 
			return Task<string>.Run(() => {
				// Creates the start information that are needed to run the process
				ProcessStartInfo processStartInfo = new ProcessStartInfo
				{
					Arguments = commandLineParameters,
					FileName = this.ProgramFileName,
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					RedirectStandardInput = true,
					StandardOutputEncoding = Encoding.UTF8,
				};

				// Sets the language in which the application is to be started
				string languageValue = string.Format(CultureInfo.InvariantCulture, "{0}.UTF-8", this.Culture.Name.Replace("-", "_"));
				if (processStartInfo.EnvironmentVariables.ContainsKey("LANG"))
					processStartInfo.EnvironmentVariables["LANG"] = languageValue;
				else
					processStartInfo.EnvironmentVariables.Add("LAND", languageValue);

				// Starts the process
				Process process = new Process();
				process.StartInfo = processStartInfo;
				process.Start();

				// Reads the output that the process wrote to the standard output
				string output = process.StandardOutput.ReadToEnd();

				// Waits till the process has finished
				process.WaitForExit();

				// Returns the output of the process, so that the user is able to parse it
				return output;
			});
		}

		/// <summary>
		/// Executes the process and parses its output.
		/// </summary>
		/// <typeparam name="T">The type of the result of the parser.</typeparam>
		/// <param name="commandLineParameters">The command line paramters, which are passed to the process.</param>
		/// <param name="parser">A delegate that is used to parse the output of the process.</param>
		/// <returns>Returns the result of the parser.</returns>
		public async Task<T> ExecuteAsync<T>(string commandLineParameters, Func<string, Task<T>> parser)
		{
			// Executes the process in order to get it's output
			string processOutput = await this.ExecuteAsync(commandLineParameters);

			// Parses the output of the process and returns the parsed object
			return await parser(processOutput);
		}

		/// <summary>
		/// Executes the process and parses its output.
		/// </summary>
		/// <param name="commandLineParameters">The command line paramters, which are passed to the process.</param>
		/// <param name="parser">A delegate that is used to parse the output of the process.</param>
		public async Task ExecuteAsync(string commandLineParameters, Func<string, Task> parser)
		{
			// Executes the process in order to get it's output
			string processOutput = await this.ExecuteAsync(commandLineParameters);

			// Parses the output of the process
			await parser(processOutput);
		}

		#endregion
	}
}