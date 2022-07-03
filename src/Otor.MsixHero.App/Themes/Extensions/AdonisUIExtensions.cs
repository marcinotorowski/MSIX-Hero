using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

// This code has been borrowed from:
// https://github.com/benruehl/adonis-ui
// ReSharper disable once CheckNamespace
namespace AdonisUI.Extensions
{
    public class ValuesToThicknessConverter
        : IValueConverter
        , IMultiValueConverter
    {
        public static ValuesToThicknessConverter Instance = new ValuesToThicknessConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertToThickness(value);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 1)
                return ConvertToThickness(values[0]);

            if (values.Length == 2)
                return ConvertToThickness(values[0], values[1]);

            if (values.Length == 4)
                return ConvertToThickness(values[0], values[1], values[2], values[3]);

            throw new ArgumentException("Invalid amount of values", nameof(values));
        }

        private Thickness ConvertToThickness(object uniformValue)
        {
            double uniformDouble = ToDouble(uniformValue);
            return new Thickness(uniformDouble);
        }

        private Thickness ConvertToThickness(object leftRight, object topBottom)
        {
            double leftRightDouble = ToDouble(leftRight);
            double topBottomDouble = ToDouble(topBottom);

            return new Thickness(leftRightDouble, topBottomDouble, leftRightDouble, topBottomDouble);
        }

        private Thickness ConvertToThickness(object left, object top, object right, object bottom)
        {
            double leftDouble = ToDouble(left);
            double topDouble = ToDouble(top);
            double rightDouble = ToDouble(right);
            double bottomDouble = ToDouble(bottom);

            return new Thickness(leftDouble, topDouble, rightDouble, bottomDouble);
        }

        private double ToDouble(object value)
        {
            return value != DependencyProperty.UnsetValue ? System.Convert.ToDouble(value) : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LayerExtension
    {
        public static int? GetLayer(DependencyObject obj)
        {
            return (int?)obj.GetValue(LayerProperty);
        }

        public static void SetLayer(DependencyObject obj, int? value)
        {
            obj.SetValue(LayerProperty, value);
        }

        public static bool GetIncreaseLayer(DependencyObject obj)
        {
            return (bool)obj.GetValue(IncreaseLayerProperty);
        }

        public static void SetIncreaseLayer(DependencyObject obj, bool value)
        {
            obj.SetValue(IncreaseLayerProperty, value);
        }

        public static int GetComputedLayer(DependencyObject obj)
        {
            return (int)obj.GetValue(ComputedLayerProperty);
        }

        private static void SetComputedLayer(DependencyObject obj, int value)
        {
            obj.SetValue(ComputedLayerPropertyKey, value);
        }

        public static readonly DependencyProperty LayerProperty = DependencyProperty.RegisterAttached("Layer", typeof(int?), typeof(LayerExtension), new PropertyMetadata(null, OnLayerPropertyChanged));

        public static readonly DependencyProperty IncreaseLayerProperty = DependencyProperty.RegisterAttached("IncreaseLayer", typeof(bool), typeof(LayerExtension), new PropertyMetadata(false, OnIncreaseLayerPropertyChanged));

        private static readonly DependencyPropertyKey ComputedLayerPropertyKey = DependencyProperty.RegisterAttachedReadOnly("ComputedLayer", typeof(int), typeof(LayerExtension), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty ComputedLayerProperty = ComputedLayerPropertyKey.DependencyProperty;

        private static void OnLayerPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.NewValue == null)
                return;

            SetComputedLayer(depObj, (int)eventArgs.NewValue);

            int increasedLayer = (int)eventArgs.NewValue + 1;

            if (!(depObj is FrameworkElement targetElement))
                return;

            if (targetElement.IsLoaded)
                SetComputedLayerOfChildren(targetElement, increasedLayer);
            else
                targetElement.Loaded += (sender, args) => SetComputedLayerOfChildren(targetElement, increasedLayer);
        }

        private static void OnIncreaseLayerPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (!(depObj is FrameworkElement targetElement))
                return;

            if (targetElement.IsLoaded)
                SetComputedLayerOfChildren(targetElement, GetComputedLayer(targetElement) + 1);
            else
                targetElement.Loaded += (sender, args) => SetComputedLayerOfChildren(targetElement, GetComputedLayer(targetElement) + 1);
        }

        private static void SetComputedLayerOfChildren(FrameworkElement element, int value)
        {
            foreach (object child in LogicalTreeHelper.GetChildren(element))
            {
                if (!(child is DependencyObject childObject))
                    continue;

                if (GetLayer(childObject) == null)
                    SetComputedLayer(childObject, value);
            }
        }
    }

    /// <summary>
    /// Values for controlling the placement of scroll bars.
    /// </summary>
    public enum ScrollBarPlacement
    {
        /// <summary>
        /// Place the scroll bar next to content (default).
        /// </summary>
        Docked,

        /// <summary>
        /// Place the scroll bar on top of content.
        /// </summary>
        Overlay,
    }

    /// <summary>
    /// Values for controlling when to expand scroll bars.
    /// </summary>
    public enum ScrollBarExpansionMode
    {
        /// <summary>
        /// Never expand the scroll bar. Keep it always collapsed.
        /// </summary>
        NeverExpand,

        /// <summary>
        /// Expand the scroll bar when the mouse hovers over it.
        /// </summary>
        ExpandOnHover,

        /// <summary>
        /// Always expand the scroll bar. Do not collapse it.
        /// </summary>
        AlwaysExpand,
    }

    public class ScrollBarExtension
    {
        /// <summary>
        /// Gets the value of the <see cref="ExpansionModeProperty"/> attached property of the specified ScrollBar.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(ScrollBar))]
        public static ScrollBarExpansionMode GetExpansionMode(DependencyObject obj)
        {
            return (ScrollBarExpansionMode)obj.GetValue(ExpansionModeProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="ExpansionModeProperty"/> attached property of the specified ScrollBar.
        /// </summary>
        public static void SetExpansionMode(DependencyObject obj, ScrollBarExpansionMode value)
        {
            obj.SetValue(ExpansionModeProperty, value);
        }

        /// <summary>
        /// A DependencyProperty that controls when to expand and collapse the scroll bar.
        /// </summary>
        public static readonly DependencyProperty ExpansionModeProperty = DependencyProperty.RegisterAttached("ExpansionMode", typeof(ScrollBarExpansionMode), typeof(ScrollBarExtension), new PropertyMetadata(ScrollBarExpansionMode.ExpandOnHover));
    }

    /// <summary>
    /// Provides attached behaviors related to the ScrollViewer control.
    /// </summary>
    public class ScrollViewerExtension
    {
        /// <summary>
        /// Gets the value of the <see cref="VerticalScrollBarExpansionModeProperty"/> attached property of the specified ScrollViewer.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(ScrollViewer))]
        public static ScrollBarExpansionMode GetVerticalScrollBarExpansionMode(DependencyObject obj)
        {
            return (ScrollBarExpansionMode)obj.GetValue(VerticalScrollBarExpansionModeProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="VerticalScrollBarExpansionModeProperty"/> attached property of the specified ScrollViewer.
        /// </summary>
        public static void SetVerticalScrollBarExpansionMode(DependencyObject obj, ScrollBarExpansionMode value)
        {
            obj.SetValue(VerticalScrollBarExpansionModeProperty, value);
        }

        /// <summary>
        /// Gets the value of the <see cref="HorizontalScrollBarExpansionModeProperty"/> attached property of the specified ScrollViewer.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(ScrollViewer))]
        public static ScrollBarExpansionMode GetHorizontalScrollBarExpansionMode(DependencyObject obj)
        {
            return (ScrollBarExpansionMode)obj.GetValue(HorizontalScrollBarExpansionModeProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="HorizontalScrollBarExpansionModeProperty"/> attached property of the specified ScrollViewer.
        /// </summary>
        public static void SetHorizontalScrollBarExpansionMode(DependencyObject obj, ScrollBarExpansionMode value)
        {
            obj.SetValue(HorizontalScrollBarExpansionModeProperty, value);
        }

        /// <summary>
        /// Gets the value of the <see cref="VerticalScrollBarPlacementProperty"/> attached property of the specified ScrollViewer.
        /// </summary>
        public static ScrollBarPlacement GetVerticalScrollBarPlacement(DependencyObject obj)
        {
            return (ScrollBarPlacement)obj.GetValue(VerticalScrollBarPlacementProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="VerticalScrollBarPlacementProperty"/> attached property of the specified ScrollViewer.
        /// </summary>
        public static void SetVerticalScrollBarPlacement(DependencyObject obj, ScrollBarPlacement value)
        {
            obj.SetValue(VerticalScrollBarPlacementProperty, value);
        }

        /// <summary>
        /// Gets the value of the <see cref="HorizontalScrollBarPlacementProperty"/> attached property of the specified ScrollViewer.
        /// </summary>
        public static ScrollBarPlacement GetHorizontalScrollBarPlacement(DependencyObject obj)
        {
            return (ScrollBarPlacement)obj.GetValue(HorizontalScrollBarPlacementProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="HorizontalScrollBarPlacementProperty"/> attached property of the specified ScrollViewer.
        /// </summary>
        public static void SetHorizontalScrollBarPlacement(DependencyObject obj, ScrollBarPlacement value)
        {
            obj.SetValue(HorizontalScrollBarPlacementProperty, value);
        }

        /// <summary>
        /// Gets the value of the <see cref="HideScrollBarsUntilMouseOverProperty"/> attached property of the specified ScrollViewer.
        /// </summary>
        public static bool GetHideScrollBarsUntilMouseOver(DependencyObject obj)
        {
            return (bool)obj.GetValue(HideScrollBarsUntilMouseOverProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="HideScrollBarsUntilMouseOverProperty"/> attached property of the specified ScrollViewer.
        /// </summary>
        public static void SetHideScrollBarsUntilMouseOver(DependencyObject obj, bool value)
        {
            obj.SetValue(HideScrollBarsUntilMouseOverProperty, value);
        }

        /// <summary>
        /// A DependencyProperty that controls when to expand and collapse the vertical scroll bar.
        /// </summary>
        public static readonly DependencyProperty VerticalScrollBarExpansionModeProperty = DependencyProperty.RegisterAttached("VerticalScrollBarExpansionMode", typeof(ScrollBarExpansionMode), typeof(ScrollViewerExtension), new PropertyMetadata(ScrollBarExpansionMode.ExpandOnHover));

        /// <summary>
        /// A DependencyProperty that controls when to expand and collapse the horizontal scroll bar.
        /// </summary>
        public static readonly DependencyProperty HorizontalScrollBarExpansionModeProperty = DependencyProperty.RegisterAttached("HorizontalScrollBarExpansionMode", typeof(ScrollBarExpansionMode), typeof(ScrollViewerExtension), new PropertyMetadata(ScrollBarExpansionMode.ExpandOnHover));

        /// <summary>
        /// A DependencyProperty that controls the placement of the vertical scroll bar.
        /// </summary>
        public static readonly DependencyProperty VerticalScrollBarPlacementProperty = DependencyProperty.RegisterAttached("VerticalScrollBarPlacement", typeof(ScrollBarPlacement), typeof(ScrollViewerExtension), new PropertyMetadata(ScrollBarPlacement.Docked));

        /// <summary>
        /// A DependencyProperty that controls the placement of the horizontal scroll bar.
        /// </summary>
        public static readonly DependencyProperty HorizontalScrollBarPlacementProperty = DependencyProperty.RegisterAttached("HorizontalScrollBarPlacement", typeof(ScrollBarPlacement), typeof(ScrollViewerExtension), new PropertyMetadata(ScrollBarPlacement.Docked));

        /// <summary>
        /// A DependencyProperty that controls whether to set up fade-in and fade-out animations for both scroll bars when entering or leaving the scroll viewer.
        /// </summary>
        public static readonly DependencyProperty HideScrollBarsUntilMouseOverProperty = DependencyProperty.RegisterAttached("HideScrollBarsUntilMouseOver", typeof(bool), typeof(ScrollViewerExtension), new PropertyMetadata(false));
    }
}