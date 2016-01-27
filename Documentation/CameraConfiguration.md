# Configuring the Camera

The following example showcases how cameras that are attached to the system can be enumerated. After retrieving the first camera all available
configurations for the camera are retrieved and printed out with their respective values.

```csharp
// Since the connection to the camera via USB can be highly volatile, exceptions can be raised all the time, therefore all calls to the gPhoto2.NET should be wrapped in try-catch-clauses
Camera camera = null;
try
{
    // Gets the first camera attached to the system
    camera = (await Camera.GetCamerasAsync()).FirstOrDefault();
    
    // Checks if a camera was found, if no camera was found, then an error message is printed out and the program is quit
    if (camera == null)
    {
        Console.WriteLine("No camera detected!");
        return;
    }
    
    // Gets all camera configuration and prints them out
    foreach (CameraConfiguration cameraConfiguration in await camera.GetSupportedConfigurationAsync())
        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", await cameraConfiguration.GetLabelAsync(), await cameraConfiguration.GetValueAsync()));
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