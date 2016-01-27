# Using gPhoto2.NET

In order to be able to use gPhoto2.NET you have to add an assembly reference when compiling. You can do that using the following command:

```bash
mcs <list-of-input-files> /target:exe /out:<output-file-name>.exe /nologo /reference:System.Core.dll /reference:GPhotoSharp.dll /lib:Build/Release
```

Please make sure to change `<list-of-input-files>` to a white space separated list of input `*.cs` files. Also change `<output-file-name>`
to the name of the EXE file that is to be generated.