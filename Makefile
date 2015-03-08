
# Contains the name of the output directory, which will contain all the generated assemblies
OUTPUTDIRECTORY           = Build

# Contains the input files for the gPhotoSharp library
GPHOTOSHARPINPUTFILES     = Camera.cs CameraSetting.cs CameraSettingType.cs IpcWrapper.cs AssemblyInfo.cs

# Contains the input files for the gPhotoShart library test application
TESTAPPLICATIONINPUTFILES = TestProgram.cs

# The target, that creates the gPhotoSharp library and the test application
All: Library Test

# The target, that creates the gPhotoSharp library
Library: $(GPHOTOSHARPINPUTFILES) OutputDirectory
	mcs $(GPHOTOSHARPINPUTFILES) /target:library /out:$(OUTPUTDIRECTORY)/GPhotoSharp.dll /nologo /reference:System.Core.dll /reference:System.Threading.Tasks.Dataflow.dll

# The target, that creates the gPhotoSharp test application
Test: $(TESTAPPLICATIONINPUTFILES) OutputDirectory Library
	mcs $(TESTAPPLICATIONINPUTFILES) /target:exe /out:$(OUTPUTDIRECTORY)/TestProgram.exe /nologo /reference:System.Core.dll /reference:GPhotoSharp.dll /lib:$(OUTPUTDIRECTORY)

# The target, that creates the output directory, which will contain all the generated assemblies
.PHONY: OutputDirectory
OutputDirectory:
	mkdir -p $(OUTPUTDIRECTORY)

# The target that cleans up the environment by removing all files that have been created by the compiler
.PHONY: clean
clean:
	rm -rf $(OUTPUTDIRECTORY)