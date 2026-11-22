using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace UIComponents.Wpf
{
    // Hosts the panel on the trigger's local AdornerLayer (inside MainWindow.Content), so
    // --capture, FindById and UIA can all see it and it scrolls/clips with the page.
    internal sealed class DropdownAdorner : Adorner
    {
        private readonly FrameworkElement _panel;
        private readonly FrameworkElement _anchor;

        public DropdownAdorner(FrameworkElement anchor, FrameworkElement panel) : base(anchor)
        {
            _anchor = anchor;
            _panel = panel;
            AddVisualChild(panel);
        }

        protected override int VisualChildrenCount { get { return 1; } }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException("index");
            return _panel;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _panel.Measure(new Size(_anchor.ActualWidth, double.PositiveInfinity));
            return constraint;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _panel.Arrange(new Rect(0, _anchor.ActualHeight + 6, _anchor.ActualWidth, _panel.DesiredSize.Height));
            return finalSize;
        }
    }

    // Flex-wrap with a fixed gap and no trailing margin, so two 24.8 rows plus one 6px gap
    // give the web's 75.6-high multi trigger.
    internal sealed class ChipWrapPanel : Panel
    {
        public double GapX { get; set; }
        public double GapY { get; set; }

        protected override Size MeasureOverride(Size availableSize)
        {
            double x = 0, y = 0, rowH = 0, maxW = 0;
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double w = child.DesiredSize.Width, h = child.DesiredSize.Height;
                if (x > 0 && x + GapX + w > availableSize.Width) { y += rowH + GapY; x = 0; rowH = 0; }
                if (x > 0) x += GapX;
                x += w;
                if (h > rowH) rowH = h;
                if (x > maxW) maxW = x;
            }
            return new Size(double.IsInfinity(availableSize.Width) ? maxW : availableSize.Width, y + rowH);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0, y = 0, rowH = 0;
            foreach (UIElement child in InternalChildren)
            {
                double w = child.DesiredSize.Width, h = child.DesiredSize.Height;
                if (x > 0 && x + GapX + w > finalSize.Width) { y += rowH + GapY; x = 0; rowH = 0; }
                if (x > 0) x += GapX;
                child.Arrange(new Rect(x, y, w, h));
                x += w;
                if (h > rowH) rowH = h;
            }
            return finalSize;
        }
    }

    // General-purpose tree walk for outside-click and focus-out checks in the dropdown family.
    internal static class DropdownTree
    {
        public static bool IsWithin(DependencyObject target, DependencyObject root)
        {
            while (target != null)
            {
                if (target == root) return true;
                // A ContentElement (e.g. a Run) has no Visual parent; walk the logical tree there.
                target = target is Visual ? VisualTreeHelper.GetParent(target) : LogicalTreeHelper.GetParent(target);
            }
            return false;
        }
    }

    // Reusable Window.PreviewMouseDown hook: calls close for a press outside the control and its panel.
    internal sealed class DropdownOutsideClick
    {
        private readonly FrameworkElement _control;
        private readonly Func<DependencyObject> _panel;
        private readonly Action _close;
        private Window _window;
        private MouseButtonEventHandler _handler;

        public DropdownOutsideClick(FrameworkElement control, Func<DependencyObject> panel, Action close)
        {
            _control = control;
            _panel = panel;
            _close = close;
        }

        public void Hook()
        {
            _window = Window.GetWindow(_control);
            if (_window == null) return;
            _handler = OnPreviewMouseDown;
            _window.PreviewMouseDown += _handler;
        }

        public void Unhook()
        {
            if (_window != null && _handler != null) _window.PreviewMouseDown -= _handler;
            _window = null;
            _handler = null;
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject target = e.OriginalSource as DependencyObject;
            if (target == null) return;
            DependencyObject panel = _panel();
            if (DropdownTree.IsWithin(target, _control) || (panel != null && DropdownTree.IsWithin(target, panel))) return;
            _close();
        }
    }
}
