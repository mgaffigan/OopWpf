using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Itp.WpfCrossProcess;

public class ResizeInsulator : ContentControl
{
    private bool Inhibit;

    public ResizeInsulator()
    {
        PresentationSource.AddSourceChangedHandler(this, OnSourceChanged);
    }

    private void OnSourceChanged(object sender, SourceChangedEventArgs e)
    {
        if (e.OldSource is HwndSource oldSource)
        {
            oldSource.RemoveHook(WndProc);
        }

        if (e.NewSource is HwndSource source)
        {
            source.AddHook(WndProc);
        }
    }

    public bool TakeAllSpace
    {
        get { return (bool)GetValue(TakeAllSpaceProperty); }
        set { SetValue(TakeAllSpaceProperty, value); }
    }

    public static readonly DependencyProperty TakeAllSpaceProperty =
        DependencyProperty.Register(nameof(TakeAllSpace), typeof(bool), typeof(ResizeInsulator),
            new PropertyMetadata(true));

    private Size lastMeasure;
    protected override Size MeasureOverride(Size constraint)
    {
        if (this.Inhibit) return TakeAllSpace ? constraint : lastMeasure;
        return lastMeasure = base.MeasureOverride(constraint);
    }

    private Size lastArrange;
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        if (this.Inhibit) return TakeAllSpace ? arrangeBounds : lastArrange;
        return lastArrange = base.ArrangeOverride(arrangeBounds);
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == 0x0231 /* WM_ENTERSIZEMOVE */)
        {
            this.Inhibit = true;
        }
        else if (msg == 0x0232 /* WM_EXITSIZEMOVE */)
        {
            this.Inhibit = false;
            InvalidateMeasure();
            InvalidateArrange();
        }

        return 0;
    }
}
