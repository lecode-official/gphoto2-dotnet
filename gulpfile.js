
var gulp = require("gulp");
var spawn = require('child_process').spawn;
var gutil = require('gulp-util');

// Represents a task, which launches the camera settings test application in debug mode
gulp.task("launch-debug", ["build-debug"], function(done) {
    
    // Launches the camera settings test application in debug mode using the mono virtual machine
    var monoProcess = spawn("mono", [
        "--debug",
        "--debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555",
        "Build/Debug/CameraSettingsTest.exe"
    ], { cwd: process.cwd() });

    // Reads the output of the C# compiler on the standard output and logs it using gulp
    monoProcess.stdout.setEncoding('utf8');
    monoProcess.stdout.on('data', function(data) {
        gutil.log(data);
    });
    
    // Reads the output of the C# compiler on the standard error output and logs it using gulp
    monoProcess.stderr.setEncoding('utf8');
    monoProcess.stderr.on('data', function(data) {
        gutil.log(gutil.colors.red(data));
    });
    
    // Waits till the camera settings test application is done and signals gulp that the task has finished
    monoProcess.on('close', done);
});

// Represents a task, which compiles the gPhoto2.NET library and all its test applications in a debug configuration
gulp.task("build-debug", ["build-library-debug", "build-camera-settings-test-debug"]);

// Represents a task, which compiles the gPhoto2.NET library in a debug configuration
gulp.task("build-library-debug", function(done) {
    
    // Creates a new process for the C# compiler, which compiles the source code and outputs the gPhoto2.NET DLL, and spawns its
    var cSharpCompilerProcess = spawn("mcs", [
        "Source/AssemblyInfo.cs",
        "Source/Camera.cs",
        "Source/CameraException.cs",
        "Source/CameraSetting.cs",
        "Source/CameraSettingType.cs",
        "Source/GPhoto2IpcWrapper.cs",
        "/fullpaths",
        "/debug",
        "/target:library",
        "/reference:System.Core.dll",
        "/reference:System.Threading.Tasks.Dataflow.dll",
        "/out:Build/Debug/GPhoto2DotNet.dll"
    ], { cwd: process.cwd() });

    // Reads the output of the C# compiler on the standard output and logs it using gulp
    cSharpCompilerProcess.stdout.setEncoding('utf8');
    cSharpCompilerProcess.stdout.on('data', function(data) {
        gutil.log(data);
    });
    
    // Reads the output of the C# compiler on the standard error output and logs it using gulp
    cSharpCompilerProcess.stderr.setEncoding('utf8');
    cSharpCompilerProcess.stderr.on('data', function(data) {
        gutil.log(gutil.colors.red(data));
    });
    
    // Waits till the C# compiler is done and signals gulp that the task has finished
    cSharpCompilerProcess.on('close', done);
});

// Represents a task, which compiles the gPhoto2.NET camera settings test application in a debug configuration
gulp.task("build-camera-settings-test-debug", ["build-library-debug"], function(done) {
    
    // Creates a new process for the C# compiler, which compiles the source code and outputs the gPhoto2.NET DLL, and spawns its
    var cSharpCompilerProcess = spawn("mcs", [
        "Test/CameraSettingsTest.cs",
        "/fullpaths",
        "/debug",
        "/target:exe",
        "/reference:System.Core.dll",
        "/reference:GPhoto2DotNet.dll",
        "/lib:Build/Debug",
        "/out:Build/Debug/CameraSettingsTest.exe"
    ], { cwd: process.cwd() });

    // Reads the output of the C# compiler on the standard output and logs it using gulp
    cSharpCompilerProcess.stdout.setEncoding('utf8');
    cSharpCompilerProcess.stdout.on('data', function(data) {
        gutil.log(data);
    });
    
    // Reads the output of the C# compiler on the standard error output and logs it using gulp
    cSharpCompilerProcess.stderr.setEncoding('utf8');
    cSharpCompilerProcess.stderr.on('data', function(data) {
        gutil.log(gutil.colors.red(data));
    });
    
    // Waits till the C# compiler is done and signals gulp that the task has finished
    cSharpCompilerProcess.on('close', done);
});

// Represents a task, which compiles the gPhoto2.NET library and all its test applications in a release configuration
gulp.task("build-release", ["build-library-release", "build-camera-settings-test-release"]);

// Represents a task, which compiles the gPhoto2.NET library in a release configuration
gulp.task("build-library-release", function(done) {
    
    // Creates a new process for the C# compiler, which compiles the source code and outputs the gPhoto2.NET DLL, and spawns its
    var cSharpCompilerProcess = spawn("mcs", [
        "Source/AssemblyInfo.cs",
        "Source/Camera.cs",
        "Source/CameraException.cs",
        "Source/CameraSetting.cs",
        "Source/CameraSettingType.cs",
        "Source/GPhoto2IpcWrapper.cs",
        "/fullpaths",
        "/target:library",
        "/reference:System.Core.dll",
        "/reference:System.Threading.Tasks.Dataflow.dll",
        "/out:Build/Release/GPhoto2DotNet.dll"
    ], { cwd: process.cwd() });

    // Reads the output of the C# compiler on the standard output and logs it using gulp
    cSharpCompilerProcess.stdout.setEncoding('utf8');
    cSharpCompilerProcess.stdout.on('data', function(data) {
        gutil.log(data);
    });
    
    // Reads the output of the C# compiler on the standard error output and logs it using gulp
    cSharpCompilerProcess.stderr.setEncoding('utf8');
    cSharpCompilerProcess.stderr.on('data', function(data) {
        gutil.log(gutil.colors.red(data));
    });
    
    // Waits till the C# compiler is done and signals gulp that the task has finished
    cSharpCompilerProcess.on('close', done);
});

// Represents a task, which compiles the gPhoto2.NET camera settings test application in a release configuration
gulp.task("build-camera-settings-test-release", ["build-library-release"], function(done) {
    
    // Creates a new process for the C# compiler, which compiles the source code and outputs the gPhoto2.NET DLL, and spawns its
    var cSharpCompilerProcess = spawn("mcs", [
        "Test/CameraSettingsTest.cs",
        "/fullpaths",
        "/target:exe",
        "/reference:System.Core.dll",
        "/reference:GPhoto2DotNet.dll",
        "/lib:Build/Release",
        "/out:Build/Release/CameraSettingsTest.exe"
    ], { cwd: process.cwd() });

    // Reads the output of the C# compiler on the standard output and logs it using gulp
    cSharpCompilerProcess.stdout.setEncoding('utf8');
    cSharpCompilerProcess.stdout.on('data', function(data) {
        gutil.log(data);
    });
    
    // Reads the output of the C# compiler on the standard error output and logs it using gulp
    cSharpCompilerProcess.stderr.setEncoding('utf8');
    cSharpCompilerProcess.stderr.on('data', function(data) {
        gutil.log(gutil.colors.red(data));
    });
    
    // Waits till the C# compiler is done and signals gulp that the task has finished
    cSharpCompilerProcess.on('close', done);
});