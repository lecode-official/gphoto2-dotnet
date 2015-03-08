
#region Using Directives

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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
			// Stores the program file name and the culture for later use
			this.ProgramFileName = programFileName;
			this.Culture = culture;

			// Initializes the action block, which is used to synchronize the access to the camera (only one camera command may be
			// executed at a time)
			this.cameraActionBlock = new ActionBlock<CameraCommand>(new Func<CameraCommand, Task>(async cameraCommand =>
				{
					// Executes the camera command
					string output = await cameraCommand.Command();

					// Resolves the task completion source so that the caller knows that the command was executed on the camera
					cameraCommand.TaskCompletionSource.SetResult(output);
				}));
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

		#region Private Fields

		/// <summary>
		/// Contains the action block, which is used to access the camera. Since only one command can be send to a camera at a time,
		/// this action block is used, to synchronize the calls to the camera. All methods, sending commands to the camera can just
		/// post commands to this action block and the action block guarantees that only one command is sent to the camera at a time.
		/// </summary>
		private ActionBlock<CameraCommand> cameraActionBlock;

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

		/// <summary>
		/// Gets or sets the command line parameters, which are always appended to the command line parameters when executing a
		/// command. This makes it easy to use command line parameters, that are always needed.
		/// </summary>
		public string StandardCommandLineParameters { get; set; }

		#endregion

		#region Public Methods

		/// <summary>
		/// Executes the process and retrieves its output.
		/// </summary>
		/// <param name="commandLineParameters">The command line paramters, which are passed to the process.</param>
		/// <returns>Returns what the process wrote to the standard output.</returns>
		public Task<string> ExecuteAsync(string commandLineParameters)
		{
			// Creates a camera command, which is scheduled to run on the camera, it contains a task completion source, which resolves
			// when the command has been executed 
			CameraCommand cameraCommand = new CameraCommand
				{
					Command = () => Task<string>.Run(() =>
					{
						// Creates the start information that are needed to run the process
						ProcessStartInfo processStartInfo = new ProcessStartInfo
						{
							Arguments = string.IsNullOrWhiteSpace(this.StandardCommandLineParameters) ?
								commandLineParameters : string.Concat(this.StandardCommandLineParameters, " ", commandLineParameters),
							FileName = this.ProgramFileName,
							UseShellExecute = false,
							CreateNoWindow = true,
							RedirectStandardOutput = true,
							RedirectStandardError
								= true,
							RedirectStandardInput = true,
							StandardOutputEncoding = Encoding.UTF8,
						};

						// Sets the language in which the application is to be started
						string languageValue = string.Format(CultureInfo.InvariantCulture,
							"{0}.UTF-8", this.Culture.Name.Replace("-", "_"));
						if (processStartInfo.EnvironmentVariables.ContainsKey("LANG"))
							processStartInfo.EnvironmentVariables["LANG"] = languageValue;
						else
							processStartInfo.EnvironmentVariables.Add("LANG", languageValue);

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
					})
				};

			// Schedules the camera command, so that it can run on the camera (it must be scheduled to ensure that only one command
			// is executed at a time)
			this.cameraActionBlock.Post(cameraCommand);

			// Awaits the execution of the scheduled camera command
			return cameraCommand.TaskCompletionSource.Task;
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

		#region Nested Classes

		/// <summary>
		/// Represents a command, which is send to the camera. This class encapsulates the information that is necessary to send a
		/// command to a camera. It is used as a wrapper, which is send to the action block, that synchronizes the calls to the
		/// camera.
		/// </summary>
		private class CameraCommand
		{
			#region Constructors

			/// <summary>
			/// Initializes a new <see cref="CameraCommand"/> instance.
			/// </summary>
			public CameraCommand()
			{
				this.TaskCompletionSource = new TaskCompletionSource<string>();
			}

			#endregion

			#region Public Properties

			/// <summary>
			/// Gets or sets the command that operates on the camera.
			/// </summar>
			public Func<Task<string>> Command { get; set; }

			/// <summary>
			/// Gets the task completion source, which contains the task, that resolves once the command was executed.
			/// </summary>
			public TaskCompletionSource<string> TaskCompletionSource { get; private set; }

			#endregion
		}

		#endregion
	}
}