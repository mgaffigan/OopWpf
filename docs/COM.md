# Usage with COM IPC

1. Create a new WPF Application project which will host the out-of-process control
2. Add
a reference to the `Itp.WpfCrossProcess.Common` NuGet package.  You will also need
a way to register a COM Server (e.g.: [`Esatto.Win32.Com`](https://www.nuget.org/packages/Esatto.Win32.Com), 
[`dscom`](https://github.com/dspace-group/dscom), or P/Invoke).

```MSBuild
<ItemGroup>
	<PackageReference Include="Itp.WpfCrossProcess.Common" />
	<PackageReference Include="Esatto.Win32.Com" />
</ItemGroup>
```

3. Add a `UserControl` which contains the functionality you want to expose.
4. Expose the `UserControl` in `App.xaml.cs`.  Multiple controls can be exposed
   from the same OOP host by adding multiple `ClassObjectRegistration` instances.
   Make sure to make up a unique CLSID for each control.

```C#
public partial class App : Application
{
    private readonly ClassObjectRegistration res;

    public App()
    {
        var ExampleControlClsid = Guid.Parse("A87689E8-F7C6-34E0-938F-BF17FE842740");
        res = new ClassObjectRegistration(ExampleControlClsid,
            ComInterop.CreateClassFactoryFor(() => new ExportedVisual(new ExampleControl())),
            CLSCTX.LOCAL_SERVER, REGCLS.MULTIPLEUSE | REGCLS.SUSPENDED);

        // After all registrations are complete, start accepting requests
        ComInterop.CoResumeClassObjects();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        res.Dispose();
        base.OnExit(e);
    }
}
```

5. Delete `MainWindow.xaml` and `MainWindow.xaml.cs` from the project.
6. Remove the `StartupUri` attribute from `App.xaml` and set `ShutdownMode` 
   to `OnExplicitShutdown` or `OnLastWindowClose`.

```XAML
<Application x:Class="Itp.WpfCrossProcess.DemoServer.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ShutdownMode="OnLastWindowClose">
</Application>
```

7. In the main application, add the same nuget references
8. In codebehind of a WPF window other control, create an instance of the
   CLSID you created in step 4 and add it to the visual tree with 
   `ImportedVisualHost`.  This might be into a `ContentControl`, `Grid`, or
   other layout container defined in a xaml file.

```C#
var child = ComInterop.CreateLocalServer<IWpfCrossChild>(Guid.Parse("A87689E8-F7C6-34E0-938F-BF17FE842740"));
contentControl.Content = new ImportedVisualHost(child);
```

9. Register `Itp.WpfCrossProcess.Common` on the system with 
   `regasm /tlb /codebase Itp.WpfCrossProcess.Common.dll`.  This must be run as 
   administrator.  See [Setup Considerations](#setup-considerations) for deployment
   considerations.
10. Start the out-of-process host, then the main application and observe the 
   control appears as part of the main application window.

## Setup Considerations

`Itp.WpfCrossProcess.Common.tlb` must be registered on the system in order for
COM marshalling to occur.  This can be done with `regasm /tlb /codebase Itp.WpfCrossProcess.Common.dll`,
but in a normal deployment would happen as part of a setup application.  Add
the following keys to the registry to register the type library:

```reg
Windows Registry Editor Version 5.00

[HKEY_CLASSES_ROOT\TypeLib\{8A2080D6-53A7-4B6D-97D3-E635F35577F2}\2.0]
@="Itp_WpfCrossProcess_Common"

[HKEY_CLASSES_ROOT\TypeLib\{8A2080D6-53A7-4B6D-97D3-E635F35577F2}\2.0\0\win64]
@="C:\\path\\to\\Itp.WpfCrossProcess.Common.tlb"

[HKEY_CLASSES_ROOT\TypeLib\{8A2080D6-53A7-4B6D-97D3-E635F35577F2}\2.0\FLAGS]
@="0"

[HKEY_CLASSES_ROOT\TypeLib\{8A2080D6-53A7-4B6D-97D3-E635F35577F2}\2.0\HELPDIR]
@="C:\\dev\\source\\repos\\OopWpf\\OopWpfCommon\\bin\\Debug\\net462"

[HKEY_CLASSES_ROOT\Interface\{6A3968B5-FEBC-417F-8294-32C3D94B184C}]
@="IWpfCrossChild"

[HKEY_CLASSES_ROOT\Interface\{6A3968B5-FEBC-417F-8294-32C3D94B184C}\ProxyStubClsid32]
@="{00020424-0000-0000-C000-000000000046}"

[HKEY_CLASSES_ROOT\Interface\{6A3968B5-FEBC-417F-8294-32C3D94B184C}\TypeLib]
@="{8A2080D6-53A7-4B6D-97D3-E635F35577F2}"
"Version"="2.0"

[HKEY_CLASSES_ROOT\WOW6432Node\Interface\{6A3968B5-FEBC-417F-8294-32C3D94B184C}]
@="IWpfCrossChild"

[HKEY_CLASSES_ROOT\WOW6432Node\Interface\{6A3968B5-FEBC-417F-8294-32C3D94B184C}\ProxyStubClsid32]
@="{00020424-0000-0000-C000-000000000046}"

[HKEY_CLASSES_ROOT\WOW6432Node\Interface\{6A3968B5-FEBC-417F-8294-32C3D94B184C}\TypeLib]
@="{8A2080D6-53A7-4B6D-97D3-E635F35577F2}"
"Version"="2.0"

[HKEY_CLASSES_ROOT\Interface\{6A3968B5-FEBC-417F-8294-32C3D94B184D}]
@="IWpfCrossHost"

[HKEY_CLASSES_ROOT\Interface\{6A3968B5-FEBC-417F-8294-32C3D94B184D}\ProxyStubClsid32]
@="{00020424-0000-0000-C000-000000000046}"

[HKEY_CLASSES_ROOT\Interface\{6A3968B5-FEBC-417F-8294-32C3D94B184D}\TypeLib]
@="{8A2080D6-53A7-4B6D-97D3-E635F35577F2}"
"Version"="2.0"

[HKEY_CLASSES_ROOT\WOW6432Node\Interface\{6A3968B5-FEBC-417F-8294-32C3D94B184D}]
@="IWpfCrossHost"

[HKEY_CLASSES_ROOT\WOW6432Node\Interface\{6A3968B5-FEBC-417F-8294-32C3D94B184D}\ProxyStubClsid32]
@="{00020424-0000-0000-C000-000000000046}"

[HKEY_CLASSES_ROOT\WOW6432Node\Interface\{6A3968B5-FEBC-417F-8294-32C3D94B184D}\TypeLib]
@="{8A2080D6-53A7-4B6D-97D3-E635F35577F2}"
"Version"="2.0"
```

You should also register the CLSID of your out-of-process host to avoid having
to start the process manually.  An example registration is:

```reg
Windows Registry Editor Version 5.00

[HKEY_CLASSES_ROOT\CLSID\{A87689E8-F7C6-34E0-938F-BF17FE842740}\LocalServer32]
@="C:\\path\\to\\yourOop.exe"

[HKEY_CLASSES_ROOT\WOW6432Node\CLSID\{A87689E8-F7C6-34E0-938F-BF17FE842740}\LocalServer32]
@="C:\\path\\to\\yourOop.exe"
```