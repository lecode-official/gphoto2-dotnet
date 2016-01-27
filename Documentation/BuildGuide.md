# Building the Project

First of all you should make sure that your system is up-to-date.

```bash
sudo apt-get update
sudo apt-get upgrade 
```

If you are building gPhoto2.NET on your Raspberry Pi and you have `rpi-update` installed, then I recommend that you upgrade your Raspberry Pi
firmware as well:

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

Besides the gPhoto2.NET library, there is a sample application, which contains several samples. Attach a camera to your system and run the sample
application, to make sure that everything is working as expected:

```bash
mono Build/Release/SampleApplication.exe
```