# ITP WPF Out-of-process

[![Nuget](https://img.shields.io/nuget/v/Itp.WpfCrossProcess.Common)](https://www.nuget.org/packages/Itp.WpfCrossProcess.Common)

Library which allows for hosting a WPF control in a different process 
than the main application.  This can be useful if the control is prone to
crashing, or if the control is running in a different bitness or runtime.
For example, a legacy .Net Framework app can embed .Net Core controls and
benefit from API's which are exclusive to .Net Core.

The user cannot tell what controls are out-of-process.  Focus and keyboard
shortcuts are forwarded across process boundaries.

![Diagram](docs/Diagram.png)

## IPC via COM (IDispatch registration-free)

The example application demonstrates using `IDispatch` to avoid having to
register COM Interfaces.  Note: IDispatch is not possible between netcore 
and netfx.  Use `IUnknown` with registration instead.

## IPC via COM (Traditional Registration)

You may host an out-of-process server via COM.  See [COM.md](docs/COM.md) 
for details.

## Resize insulator

To improve performance, use `ResizeInsulator` to prevent the out-of-process
control from changing size during a resize move.  During the resize, layout
is suspended and the resize is not applied till the resize ends.

`ResizeInsulator` must be placed in the hosting window above `ImportedVisualHost`.
See the example application for usage.