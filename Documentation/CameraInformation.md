# Retrieving Camera Information

The following example showcases how cameras that are attached to the system can be enumerated. After retrieving the first camera all some useful
information about the camera, e.g. the manufacturer, model, and owner name, is retrieved and printed out.

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
    
    // Gathers some information about the camera and prints it out
    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Manufacturer: {0}", await camera.GetManufacturerAsync()));
    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Camera model: {0}", await camera.GetCameraModelAsync()));
    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Lens name: {0}", await camera.GetLensNameAsync()));
    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Battery level: {0}", await camera.GetBatteryLevelAsync()));
    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Owner name: {0}", await camera.GetOwnerNameAsync()));
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