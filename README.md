# gPhoto2.NET

![gPhoto2.NET Logo](https://github.com/lecode-official/gphoto2-dotnet/blob/master/Documentation/Images/Banner.png "gPhoto2.NET Logo")

## Introduction

gPhoto2.NET is an easy-to-use .NET wrapper for the popular gPhoto2. It can be used to easily interface with DSLRs or other types of cameras from a
.NET application. gPhoto2.NET was developed using Mono on Linux and was specifically made for the use on Raspberry Pi.

## Current Status

The project is currently not yet ready for prime time. It is in an early alpha stage where a lot of vital features are still missing or do not work
as expected. Ultimately the library should be usable on Raspberry Pi, but I am not sure if it runs or builds on a Raspberry Pi right now, since the
project is currently developed in a Linux desktop environment. Any help, e.g. feature requests or bug reports, is greatly appreciated. Please use
GitHub's issue system for that. If you want to improve on my work or fix bugs, then feel free to fork this project and send me a pull request.

## Acknowledgements

This project would not be possible without the great contributions of the open source community. gPhoto2.NET was build using these awesome open
source projects:

 - **[gPhoto2](http://www.gphoto.org/)** - gPhoto2 is a free, redistributable, ready to use set of digital camera software applications for Unix-like systems, written by a whole team of dedicated volunteers around the world. It supports more than 2100 cameras

## Using the Library

The following example showcases how cameras that are attached to the system can be enumerated. After retrieving the first camera all an image is
captured and stored on the camera.

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

    // Captures an image and stores it on the camera
    string fileName = await camera.CaptureImageAsync();
    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Image captured and stored on the camera: {0}", fileName));
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

First of all you should make sure that your system is up-to-date.

```bash
sudo apt-get update
sudo apt-get upgrade 
```

If you have `rpi-update` installed, then I recommend that you upgrade your Raspberry Pi firmware as well.

```bash
sudo rpi-update
sudo reboot
```

There are a few prerequisites for building gPhoto2.NET on Linux. You'll need Mono as well as gPhoto2 in order for gPhoto2.NET to compile and
properly function. If you do not want the latest version of either of them, then you can install them via `apt-get`. Since gPhoto2.NET uses some
advanced features you'll probably need the `mono-complete` package.

```bash
sudo apt-get install mono-complete
sudo apt-get install gphoto2
```

You can pull the latest version of the gPhoto2.NET repository from GitHub using the following command:

```bash
git pull https://github.com/lecode-official/gphoto2-dotnet.git
```

Now you are ready to build gPhoto2.NET. This will create a folder called "Build", which contains two folders "Release" and "Debug" which contain
a release and a debug version of gPhoto2.NET respectively.

```bash
make All
```

Besides the gPhoto2.NET library, there are several test applications. Attach a camera to your system and run the test applications, to make sure
that everything is working properly, e.g. like this:

```bash
mono Build/Release/SampleApplication.exe
```

## Using gPhoto2.NET

In order to be able to use gPhoto2.NET you have to add an assembly reference when compiling. You can do that using the following command:

```bash
mcs Test.cs /target:exe /out:TestProgram.exe /nologo /reference:System.Core.dll /reference:GPhotoSharp.dll /lib:Build/Release
```

## Trouble Shooting

If you run into problems, i.e. `CameraException` is thrown, although the camera is properly attached to your system, this might be due to the
`gvfs-gphoto2-volume-monitor`, which comes pre-installed with several Linux distributions, including Raspbian. The `gvfs-gphoto2-volume-monitor`
captures the USB port that the camera is attached to and therefore gPhoto2.NET can not access the camera. You'll have to kill the
`gvfs-gphoto2-volume-monitor` process before you are able to use gPhoto2.NET. To find out if `gvfs-gphoto2-volume-monitor` is blocking you from
accessing the camera you can use `ps`:

```bash
user@machine ~ $ ps -aux | grep gphoto
user     1901  0.0  0.1 210060  6140 ?        Sl   21:06   0:00 /usr/lib/gvfs/gvfs-gphoto2-volume-monitor
user     2350  0.0  0.0  13260  2152 pts/2    S+   21:10   0:00 grep --colour=auto gphoto
```

You can kill the process by calling:

```bash
user@machine ~ $ kill -SIGTERM 1901
```

Please make sure to replace the process ID with the process ID that you got from `ps`.