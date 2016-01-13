
# Contains the names of the output directories, which will contain all the generated assemblies
OUTPUTDIRECTORY           = Build
RELEASE                   = Release
DEBUG                     = Debug

# Contains the input files for the gPhotoSharp library
GPHOTOSHARPINPUTFILES     = Source/AssemblyInfo.cs Source/Camera.cs Source/CameraException.cs Source/CameraSetting.cs Source/CameraSettingType.cs Source/GPhoto2IpcWrapper.cs

# Contains the input files for the gPhotoSharp test applications
TESTAPPLICATIONINPUTFILES = Test/CameraSettingsTest.cs

# The targets, that create the gPhotoSharp library and the test applications in both debug and release configuration
All: LibraryRelease TestRelease LibraryDebug TestDebug
AllRelease: LibraryRelease TestRelease
AllDebug: LibraryDebug TestDebug

# The target, that creates the release version of the gPhotoSharp library
LibraryRelease: $(GPHOTOSHARPINPUTFILES) OutputDirectoryRelease
	mcs $(GPHOTOSHARPINPUTFILES) /target:library /out:$(OUTPUTDIRECTORY)/$(RELEASE)/GPhotoSharp.dll /nologo /fullpaths /reference:System.Core.dll /reference:System.Threading.Tasks.Dataflow.dll

# The target, that creates the release version of the gPhotoSharp test applications
TestRelease: $(TESTAPPLICATIONINPUTFILES) OutputDirectoryRelease LibraryRelease
	mcs $(TESTAPPLICATIONINPUTFILES) /target:exe /out:$(OUTPUTDIRECTORY)/$(RELEASE)/TestProgram.exe /nologo /fullpaths /reference:System.Core.dll /reference:GPhotoSharp.dll /lib:$(OUTPUTDIRECTORY)/$(RELEASE)

# The target, that creates the debug version of the gPhotoSharp library
LibraryDebug: $(GPHOTOSHARPINPUTFILES) OutputDirectoryDebug
	mcs $(GPHOTOSHARPINPUTFILES) /target:library /out:$(OUTPUTDIRECTORY)/$(DEBUG)/GPhotoSharp.dll /nologo /fullpaths /debug /reference:System.Core.dll /reference:System.Threading.Tasks.Dataflow.dll

# The target, that creates the debug version of the gPhotoSharp test applications
TestDebug: $(TESTAPPLICATIONINPUTFILES) OutputDirectoryDebug LibraryDebug
	mcs $(TESTAPPLICATIONINPUTFILES) /target:exe /out:$(OUTPUTDIRECTORY)/$(DEBUG)/TestProgram.exe /nologo /fullpaths /debug /reference:System.Core.dll /reference:GPhotoSharp.dll /lib:$(OUTPUTDIRECTORY)/$(DEBUG)

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