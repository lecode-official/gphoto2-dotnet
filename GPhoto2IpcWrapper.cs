
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
	/// Represents a helper class for inter-process communication with a the gPhoto2 command line program.
	/// </summary>
	internal class GPhoto2IpcWrapper : IDisposable
	{
		#region Constructors

		/// <summary>
		/// Initializes a new <see cref="IpGPhoto2IpcWrappercWrapper" /> instance.
		/// </summary>
		public GPhoto2IpcWrapper()
		{
			// Initializes the action block, which is used to synchronize the access t gPhoto2 (only one command may be executed at a
			// time, because only one application may access the camera via USB at a time, also this ensures a causal chain of events
			this.cameraActionBlock = new ActionBlock<GPhoto2Command>(new Func<GPhoto2Command, Task>(async gPhoto2Command =>
				{
					// Executes the gPhoto2 command
					string output = await gPhoto2Command.Command();

					// Resolves the task completion source so that the caller knows that the command was executed on the camera
					gPhoto2Command.TaskCompletionSource.SetResult(output);
				}));
		}
		
		#endregion

		#region Private Fields

		/// <summary>
		/// Contains the action block, which is used to access the camera. Since only one command can be send to a camera at a time,
		/// this action block is used, to synchronize the calls to the camera. All methods, sending commands to the camera can just
		/// post commands to this action block and the action block guarantees that only one command is sent to the camera at a time.
		/// </summary>
		private ActionBlock<GPhoto2Command> cameraActionBlock;

		/// <summary>
		/// Contains the gPhoto2 interactive shell process. gPhoto2 supports an interactive shell mode, where the process is kept alive,
		/// and commands can be executed by writing them to the standard input of the process. The interactive shell mode is used
		/// where ever possible because it is much more performant because the overhead of starting a new process is removed.
		/// </summary>
		private Process gPhoto2InteractiveShellProcess;

		/// <summary>
		/// Gets or sets the command line parameters, which are always appended to the command line parameters when executing a
		/// command. This makes it easy to use command line parameters, that are always needed.
		/// </summary>
		public string StandardCommandLineParameters { get; set; }

		#endregion
		
		#region IDisposable Implementation
		
		/// <summary>
		/// Disposes of all the resources that have been allocated by the <see cref="GPhoto2IpcWrapper" />.
		/// </summary>
		public void Dispose()
		{
			// Checks if the gPhoto2 interactive shell process is still running, if so then the process is terminated
			if (this.gPhoto2InteractiveShellProcess != null)
			{
				this.gPhoto2InteractiveShellProcess.Kill();
				this.gPhoto2InteractiveShellProcess.Close();
				this.gPhoto2InteractiveShellProcess = null;
			}
		}
		
		#endregion

		#region Public Methods

		/// <summary>
		/// Executes a new gPhoto2 process and retrieves its output.
		/// </summary>
		/// <param name="commandLineParameters">The command line paramters, which are passed to the process.</param>
		/// <returns>Returns what the process wrote to the standard output.</returns>
		public Task<string> ExecuteAsync(string commandLineParameters)
		{
			// Creates a gPhoto2 command, which is scheduled to run on the camera, it contains a task completion source, which resolves
			// when the command has been executed 
			GPhoto2Command gPhoto2Command = new GPhoto2Command
				{
					Command = () => Task<string>.Run(() =>
					{
						// Creates the start information that are needed to run the process
						ProcessStartInfo processStartInfo = new ProcessStartInfo
						{
							Arguments = string.IsNullOrWhiteSpace(this.StandardCommandLineParameters) ?
								commandLineParameters : string.Concat(this.StandardCommandLineParameters, " ", commandLineParameters),
							FileName = "gphoto2",
							UseShellExecute = false,
							CreateNoWindow = true,
							RedirectStandardOutput = true,
							RedirectStandardError = true,
							RedirectStandardInput = true,
							StandardOutputEncoding = Encoding.UTF8,
						};

						// Sets the language in which the application is to be started (the languages is set to american english in order
						// to make it easier to parse the output regardless of the system language)
						if (processStartInfo.EnvironmentVariables.ContainsKey("LANG"))
							processStartInfo.EnvironmentVariables["LANG"] = "en_US.UTF-8";
						else
							processStartInfo.EnvironmentVariables.Add("LANG", "en_US.UTF-8");

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
			this.cameraActionBlock.Post(gPhoto2Command);

			// Awaits the execution of the scheduled camera command
			return gPhoto2Command.TaskCompletionSource.Task;
		}

		/// <summary>
		/// Executes a new gPhoto2 process and parses its output.
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
		/// Executes a new gPhoto2 process and parses its output.
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
		
		/// <summary>
		/// Executes the specified command line parameters in the gPhoto2 interactive shell process. This method of executing a command
		/// has better performace, because the shell mode process has only to be spawned once and can then be reused.
		/// </summary>
		/// <param name="commandLineParameters">The command line paramters, which are passed to the process.</param>
		public Task<string> ExecuteInteractiveAsync(string commandLineParameters)
		{
			// Creates a gPhoto2 command, which is scheduled to run on the camera, it contains a task completion source, which resolves
			// when the command has been executed 
			GPhoto2Command gPhoto2Command = new GPhoto2Command
				{
					Command = () => Task<string>.Run(() =>
					{
						// Checks whether the gPhoto2 interactive shell process has been started, yet, if not then a new one is spawned
						if (this.gPhoto2InteractiveShellProcess == null)
						{
							// Creates the start information that are needed to run the gPhoto2 process in the interactive shell mode
							ProcessStartInfo processStartInfo = new ProcessStartInfo
							{
								Arguments = "--shell",
								FileName = "gphoto2",
								UseShellExecute = false,
								CreateNoWindow = true,
								RedirectStandardOutput = true,
								RedirectStandardError = true,
								RedirectStandardInput = true,
								StandardOutputEncoding = Encoding.UTF8,
							};
			
							// Sets the language in which the application is to be started (the languages is set to american english in order
							// to make it easier to parse the output regardless of the system language)
							if (processStartInfo.EnvironmentVariables.ContainsKey("LANG"))
								processStartInfo.EnvironmentVariables["LANG"] = "en_US.UTF-8";
							else
								processStartInfo.EnvironmentVariables.Add("LANG", "en_US.UTF-8");
			
							// Starts the process gPhoto2 process in the interactive shell mode
							this.gPhoto2InteractiveShellProcess = new Process();
							this.gPhoto2InteractiveShellProcess.StartInfo = processStartInfo;
							this.gPhoto2InteractiveShellProcess.Start();
						}
						
						// Executes the specified command by writing it to the standard input of the gPhoto2 interactive shell process
						this.gPhoto2InteractiveShellProcess.StandardInput.WriteLine(
							string.IsNullOrWhiteSpace(this.StandardCommandLineParameters) ?
								commandLineParameters : string.Concat(this.StandardCommandLineParameters, " ", commandLineParameters));
						
						// Reads the output of the process line by line and returns it, the end of the output is detected by checking if
						// the read output line starts with "gphoto2:" (in the interactive shell mode of gPhoto2 prints out the prompt
						// "gphoto2:" followed by the current path)
						string outputLine, output = string.Empty;
			            while ((outputLine = this.gPhoto2InteractiveShellProcess.StandardOutput.ReadLine()) != null) 
			            {
							// Checks if the end of the output has been reached (by detecting the prompt of the interactive shell mode)
			                if (outputLine.ToUpperInvariant().StartsWith("GPHOTO2:"))
			                	break;
								
							// Since the end of the output has not yet been reached, the line is added to the output
							output = string.Concat(output, Environment.NewLine, outputLine);
			            }

						// Returns the output of the process, so that the user is able to parse it
						return output;
					})
				};
			
			// Schedules the camera command, so that it can run on the camera (it must be scheduled to ensure that only one command
			// is executed at a time)
			this.cameraActionBlock.Post(gPhoto2Command);

			// Awaits the execution of the scheduled camera command
			return gPhoto2Command.TaskCompletionSource.Task;
		}

		/// <summary>
		/// Executes the specified command line parameters in the gPhoto2 interactive shell process and parses the output. This method of
		/// executing a command has better performace, because the shell mode process has only to be spawned once and can then be reused.
		/// </summary>
		/// <typeparam name="T">The type of the result of the parser.</typeparam>
		/// <param name="commandLineParameters">The command line paramters, which are passed to the process.</param>
		/// <param name="parser">A delegate that is used to parse the output of the process.</param>
		/// <returns>Returns the result of the parser.</returns>
		public async Task<T> ExecuteInteractiveAsync<T>(string commandLineParameters, Func<string, Task<T>> parser)
		{
			// Executes the command in order to get it's output
			string processOutput = await this.ExecuteInteractiveAsync(commandLineParameters);

			// Parses the output of the process and returns the parsed object
			return await parser(processOutput);
		}

		/// <summary>
		/// Executes the specified command line parameters in the gPhoto2 interactive shell process and parses the output. This method of
		/// executing a command has better performace, because the shell mode process has only to be spawned once and can then be reused.
		/// </summary>
		/// <param name="commandLineParameters">The command line paramters, which are passed to the process.</param>
		/// <param name="parser">A delegate that is used to parse the output of the process.</param>
		public async Task ExecuteInteractiveAsync(string commandLineParameters, Func<string, Task> parser)
		{
			// Executes the command in order to get it's output
			string processOutput = await this.ExecuteInteractiveAsync(commandLineParameters);

			// Parses the output of the process
			await parser(processOutput);
		}
		
		#endregion

		#region Nested Classes

		/// <summary>
		/// Represents a command, which is send to gPhoto2. This class encapsulates the information that is necessary to send a command
		/// to gPhoto2. It is used as a wrapper, which is send to the action block, that synchronizes the calls to gPhoto2.
		/// </summary>
		private class GPhoto2Command
		{
			#region Constructors

			/// <summary>
			/// Initializes a new <see cref="GPhoto2Command"/> instance.
			/// </summary>
			public GPhoto2Command()
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