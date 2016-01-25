
# Contains the names of the output directories, which will contain all the generated assemblies
OUTPUTDIRECTORY              = Build
RELEASE                      = Release
DEBUG                        = Debug

# Contains the input files for the gPhoto2.NET library
GPHOTODOTNETINPUTFILES       = Source/AssemblyInfo.cs Source/Camera.cs Source/CameraException.cs Source/CameraConfiguration.cs Source/CameraConfigurations.cs Source/CameraConfigurationType.cs Source/GPhoto2IpcWrapper.cs Source/IsoSpeeds.cs

# Contains the input files for the gPhoto2.NET samples application
SAMPLESAPPLICATIONINPUTFILES = Samples/AssemblyInfo.cs Samples/CameraInformationSample.cs Samples/CameraConfigurationSample.cs Samples/CaptureSettingsSample.cs Samples/ImageCapturingSample.cs Samples/ISample.cs Samples/SampleApplication.cs

# The targets, that create the gPhoto2.NET library and the samples application in both debug and release configuration
All: AllRelease AllDebug
AllRelease: LibraryRelease SamplesRelease
AllDebug: LibraryDebug SamplesDebug

# The target, that creates the release version of the gPhoto2.NET library
LibraryRelease: $(GPHOTODOTNETINPUTFILES) OutputDirectoryRelease
	mcs $(GPHOTODOTNETINPUTFILES) /target:library /out:$(OUTPUTDIRECTORY)/$(RELEASE)/GPhotoSharp.dll /nologo /fullpaths /reference:System.Core.dll /reference:System.Threading.Tasks.Dataflow.dll

# The target, that creates the release version of the gPhoto2.NET samples application
SamplesRelease: $(SAMPLESAPPLICATIONINPUTFILES) OutputDirectoryRelease LibraryRelease
	mcs $(SAMPLESAPPLICATIONINPUTFILES) /target:exe /out:$(OUTPUTDIRECTORY)/$(RELEASE)/SampleApplication.exe /nologo /fullpaths /reference:System.Core.dll /reference:GPhotoSharp.dll /lib:$(OUTPUTDIRECTORY)/$(RELEASE)

# The target, that creates the debug version of the gPhoto2.NET library
LibraryDebug: $(GPHOTODOTNETINPUTFILES) OutputDirectoryDebug
	mcs $(GPHOTODOTNETINPUTFILES) /target:library /out:$(OUTPUTDIRECTORY)/$(DEBUG)/GPhotoSharp.dll /nologo /fullpaths /debug /reference:System.Core.dll /reference:System.Threading.Tasks.Dataflow.dll

# The target, that creates the debug version of the gPhoto2.NET samples application
SamplesDebug: $(SAMPLESAPPLICATIONINPUTFILES) OutputDirectoryDebug LibraryDebug
	mcs $(SAMPLESAPPLICATIONINPUTFILES) /target:exe /out:$(OUTPUTDIRECTORY)/$(DEBUG)/SampleApplication.exe /nologo /fullpaths /debug /reference:System.Core.dll /reference:GPhotoSharp.dll /lib:$(OUTPUTDIRECTORY)/$(DEBUG)

# The target, that creates the output directory for the release version of the application, which will contain all the generated assemblies
.PHONY: OutputDirectoryRelease
OutputDirectoryRelease:
	mkdir -p $(OUTPUTDIRECTORY)/$(RELEASE)

# The target, that creates the output directory for the debug version of the application, which will contain all the generated assemblies
.PHONY: OutputDirectoryDebug
OutputDirectoryDebug:
	mkdir -p $(OUTPUTDIRECTORY)/$(DEBUG)

# The target that cleans up the environment by removing all files that have been created by the compiler
.PHONY: Clean
Clean:
	rm -rf $(OUTPUTDIRECTORY)