# gPhoto2.NET

## Introduction
gPhoto2.NET is an easy-to-use .NET wrapper around the popular gPhoto2. It can be used to easily interface with DSLRs or other types of cameras from
a .NET application. gPhoto2.NET was developed using Mono on Linux and was especially made for the use on Raspberry Pi.

## Current Status
The project is currently not yet ready for prime time. It is in an early alpha stage where a lot of vital features are still missing or do not work
as expected. Ultimately the library should be usable on Raspberry Pi, but I am not sure if it runs or builds on a Raspberry Pi right now, since the
project is currently developed in a Linux desktop environment. Any help, e.g. feature request or bug reports, is greatly appreciated. Please use
GitHub's issue system for that. If you want to improve on my work or fix bugs, then feel free to fork this project and send me a pull request.

## Using the Library
The following example showcases how cameras that are attached to the system can be enumerated. After retrieving the first camera all camera settings
are retrieved an displayed on the screen.

```csharp
// Since the connection to the camera via USB can be highly volatile, exceptions can be raised all the time, therefore all
// calls to the gphoto2-dotnet should be wrapped in try-catch-clauses, gphoto2-dotnet always throws CameraException
Camera camera = null;
try
{
	// Gets the first camera attached to the computer
	camera = (await Camera.GetCamerasAsync()).FirstOrDefault();
	
	// Checks if a camera was found, if no camera was found, then an error message is printed out and the program is quit
	if (camera == null)
	{
		Console.WriteLine("No camera detected!");
		return;
	}

	// Gets all settings of the attached camera, cycles over them and prints out all settings and their current values
	foreach (CameraSetting setting in camera.Settings)
		Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}",
			await setting.GetLabelAsync(), await setting.GetValueAsync()));
}
catch (CameraException exception)
{
	// If an exception was caught, e.g. because the camera was unplugged, an error message is printed out
	Console.WriteLine(string.Concat("An error occurred:", Environment.NewLine, exception.Details));
}
finally
{
	// If a camera was acquired, then it is safely disposed of
	if (camera != null)
		camera.Dispose();
}
```

## Building the Project
(Coming soon)

## Setting up the Library on the Raspberry Pi
(Coming soon)