using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Otor.MsixHero.App.Helpers
{
    public static class TypedParentExtension
    {
        public static T GetVisualParent<T>(this DependencyObject element) where T : DependencyObject
        {
            return element?.GetVisualParents().OfType<T>().FirstOrDefault();
        }

        public static T GetVisualParent<T>(this DependencyObject element, Predicate<DependencyObject> predicate) where T : DependencyObject
        {
            return element?.GetVisualParents().OfType<T>().FirstOrDefault(d => predicate(d));
        }

        public static T GetLogicalParent<T>(this DependencyObject element) where T : DependencyObject
        {
            return element?.GetLogicalParents().OfType<T>().FirstOrDefault();
        }

        public static T GetLogicalParent<T>(this DependencyObject element, Predicate<DependencyObject> predicate) where T : DependencyObject
        {
            return element?.GetLogicalParents().OfType<T>().FirstOrDefault(d => predicate(d));
        }

        private static IEnumerable<DependencyObject> GetVisualParents(this DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            while ((element = element.GetVisualParent()) != null)
            {
                yield return element;
            }
        }

        public static IEnumerable<DependencyObject> GetLogicalParents(this DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            while ((element = element.GetLogicalParent()) != null)
            {
                yield return element;
            }
        }

        private static DependencyObject GetVisualParent(this DependencyObject element)
        {
            try
            {
                return VisualTreeHelper.GetParent(element);
            }
            catch (InvalidOperationException)
            {
                if (element is FrameworkElement frameworkElement)
                {
                    return frameworkElement.Parent;
                }

                if (element is FrameworkContentElement frameworkContentElement)
                {
                    return frameworkContentElement.Parent;
                }
            }

            return null;
        }

        private static DependencyObject GetLogicalParent(this DependencyObject element)
        {
            try
            {
                return LogicalTreeHelper.GetParent(element);
            }
            catch (InvalidOperationException)
            {
                if (element is FrameworkElement frameworkElement)
                {
                    return frameworkElement.Parent;
                }

                if (element is FrameworkContentElement frameworkContentElement)
                {
                    return frameworkContentElement.Parent;
                }
            }

            return null;
        }
    }
}
