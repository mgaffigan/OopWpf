﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Input;
using System.Diagnostics;
using System.Collections;
using System.Windows.Media;
using System.Windows.Controls;

namespace OopWpf
{
    [ContentProperty(nameof(Child))]
    public class HwndSourceHost : HwndHost
    {
        #region Child

        public static DependencyProperty ChildProperty
            = DependencyProperty.Register(nameof(Child), typeof(FrameworkElement), typeof(HwndSourceHost),
                new PropertyMetadata(null, (d, e) => ((HwndSourceHost)d).Child_Changed((FrameworkElement)e.NewValue)));

        public FrameworkElement Child
        {
            get { return (FrameworkElement)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        private void Child_Changed(FrameworkElement child)
        {
            if (root == null)
            {
                return;
            }

            root.Child = child;
            InvalidateMeasure();
        }

        #endregion

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var wsp = new HwndSourceParameters();
            wsp.WindowStyle = 0x56000000 /* WS_VISIBLE | WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN */;
            wsp.ParentWindow = hwndParent.Handle;

            hwndSource = new HwndSource(wsp);
            hwndSource.SizeToContent = SizeToContent.Manual;
            hwndInputSink.KeyboardInputSite = new SiteProxy(this, hwndInputSink);

            root = new HwndSourceHostRoot();
            hwndSource.RootVisual = root;
            root.OnMeasure += (_1, _2) => InvalidateMeasure();
            root.Child = Child;
            AddLogicalChild(hwndSource.RootVisual);
            InvalidateMeasure();

            return new HandleRef(this, hwndSource.Handle);
        }

        private class SiteProxy : IKeyboardInputSite
        {
            private HwndSourceHost host;

            public IKeyboardInputSink Sink { get; private set; }

            public SiteProxy(HwndSourceHost host, IKeyboardInputSink keyboardInputSink)
            {
                this.host = host;
                this.Sink = keyboardInputSink;
            }

            public bool OnNoMoreTabStops(TraversalRequest request)
            {
                return host?.MoveFocus(request) ?? false;
            }

            public void Unregister()
            {
                this.Sink.KeyboardInputSite = null;
                this.Sink = null;
                this.host = null;
            }
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            hwndSource?.Dispose();
            hwndSource = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                hwndSource?.Dispose();
                hwndSource = null;
            }

            base.Dispose(disposing);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (hwndSource == null)
            {
                // We don't have a child yet.
                return new Size();
            }

            var root = (HwndSourceHostRoot)hwndSource.RootVisual;
            root.Measure(constraint);
            return root.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (hwndSource == null)
            {
                // We don't have a child yet.
                return finalSize;
            }

            var root = (UIElement)hwndSource.RootVisual;
            root.Arrange(new Rect(finalSize));
            return root.RenderSize;
        }

        protected sealed override IEnumerator LogicalChildren
            => new[] { root }.Where(n => n != null).GetEnumerator();

        #region IKeyboardInputSink

        protected sealed override bool HasFocusWithinCore()
            => hwndSource == null ? base.HasFocusWithinCore() : hwndInputSink.HasFocusWithin();

        protected sealed override bool OnMnemonicCore(ref MSG msg, ModifierKeys modifiers)
            => hwndSource == null ? base.OnMnemonicCore(ref msg, modifiers) : hwndInputSink.OnMnemonic(ref msg, modifiers);

        protected sealed override bool TabIntoCore(TraversalRequest request)
            => hwndSource == null ? base.TabIntoCore(request) : hwndInputSink.TabInto(request);

        protected sealed override bool TranslateCharCore(ref MSG msg, ModifierKeys modifiers)
            => hwndSource == null ? base.TranslateCharCore(ref msg, modifiers) : hwndInputSink.TranslateChar(ref msg, modifiers);

        protected sealed override bool TranslateAcceleratorCore(ref MSG msg, ModifierKeys modifiers)
        {
            if (hwndSource == null)
            {
                return base.TranslateAcceleratorCore(ref msg, modifiers);
            }

            if (!root.IsLogicalParentEnabled)
            {
                throw new InvalidOperationException();
            }
            root.IsLogicalParentEnabled = false;
            try
            {
                return hwndInputSink.TranslateAccelerator(ref msg, modifiers);
            }
            finally
            {
                root.IsLogicalParentEnabled = true;
            }
        }

        #endregion

        protected HwndSource hwndSource;
        private HwndSourceHostRoot root;

        protected IKeyboardInputSink hwndInputSink => hwndSource;

        private class HwndSourceHostRoot : Decorator
        {
            internal bool IsLogicalParentEnabled { get; set; } = true;

            protected override DependencyObject GetUIParentCore()
            {
                if (!IsLogicalParentEnabled)
                {
                    return null;
                }

                return base.GetUIParentCore();
            }

            public event EventHandler OnMeasure;

            protected override void OnChildDesiredSizeChanged(UIElement child)
            {
                OnMeasure?.Invoke(this, EventArgs.Empty);

                base.OnChildDesiredSizeChanged(child);
            }
        }
    }
}
