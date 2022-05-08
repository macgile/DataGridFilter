// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : FilterHelpers.cs
// Created    : 20/01/2021
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ExcessiveIndentation
// ReSharper disable UsePatternMatching
// ReSharper disable CheckNamespace

namespace FilterDataGrid
{
    public static class CollectionExtentions
    {
        #region Public Methods

        public static void DisposeSequence<T>(this IEnumerable<T> source)
        {
            foreach (IDisposable disposableObject in source.OfType<IDisposable>())
            {
                disposableObject.Dispose();
            };
        }

        #endregion Public Methods
    }

    public static class Extensions
    {
        #region Public Methods

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }

        #endregion Public Methods
    }

    public static class Helpers
    {
        #region Public Methods

        /// <summary>
        ///     Print elapsed time
        /// </summary>
        /// <param name="label"></param>
        /// <param name="start"></param>
        public static void Elapsed(string label, DateTime start)
        {
            var span = DateTime.Now - start;
            Debug.WriteLine($"{label,-20}{span:mm\\:ss\\.ff}");
        }

        #endregion Public Methods
    }

    /// <summary>
    /// ScrollBar to Top
    /// https://itecnote.com/tecnote/r-wpf-reset-listbox-scroll-position-when-itemssource-changes/
    /// </summary>
    public static class ScrollToTopBehavior
    {
        #region Public Fields

        public static readonly DependencyProperty ScrollToTopProperty =
        DependencyProperty.RegisterAttached
        (
            "ScrollToTop",
            typeof(bool),
            typeof(ScrollToTopBehavior),
            new UIPropertyMetadata(false, OnScrollToTopPropertyChanged)
        );

        #endregion Public Fields

        #region Public Methods

        public static bool GetScrollToTop(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollToTopProperty);
        }

        public static void SetScrollToTop(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollToTopProperty, value);
        }

        #endregion Public Methods

        #region Private Methods

        private static void ItemsSourceChanged(object o, EventArgs eArgs)
        {
            ItemsControl itemsControl = o as ItemsControl;

            if (itemsControl == null) return;

            void EventHandler(object sender, EventArgs e)
            {
                if (itemsControl.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    ScrollViewer scrollViewer = itemsControl.FindVisualChild<ScrollViewer>();
                    scrollViewer.ScrollToTop();
                    itemsControl.ItemContainerGenerator.StatusChanged -= EventHandler;
                }
            }

            itemsControl.ItemContainerGenerator.StatusChanged += EventHandler;
        }

        private static void OnScrollToTopPropertyChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl itemsControl = dpo as ItemsControl;

            if (itemsControl != null)
            {
                DependencyPropertyDescriptor dependencyPropertyDescriptor =
                        DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl));

                if (dependencyPropertyDescriptor != null)
                {
                    if ((bool)e.NewValue)
                    {
                        dependencyPropertyDescriptor.AddValueChanged(itemsControl, ItemsSourceChanged);
                    }
                    else
                    {
                        dependencyPropertyDescriptor.RemoveValueChanged(itemsControl, ItemsSourceChanged);
                    }
                }
            }
        }

        #endregion Private Methods
    }

    public static class StopwatchExention
    {
        #region Public Methods

        public static void Display(this Stopwatch watch, string message, bool hidden = false)
        {
            watch.Stop();
            //  int length = message?.Length ?? 0;

            if (hidden)
                Debug.WriteLine($"{message}\t\tTime:{watch.Elapsed:ss\\.f}");
        }

        #endregion Public Methods
    }

    public static class VisualTreeHelpers
    {
        #region Public Methods

        /// <summary>
        ///     Returns the first ancester of specified type
        /// </summary>
        public static T FindAncestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            current = VisualTreeHelper.GetParent(current);

            while (current != null)
            {
                if (current is T) return (T)current;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        /// <summary>
        ///     Returns a specific ancester of an object
        /// </summary>
        public static T FindAncestor<T>(DependencyObject current, T lookupItem)
            where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T && current == lookupItem) return (T)current;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        /// <summary>
        ///     Finds an ancestor object by name and type
        /// </summary>
        public static T FindAncestor<T>(DependencyObject current, string parentName)
            where T : DependencyObject
        {
            while (current != null)
            {
                if (!string.IsNullOrEmpty(parentName))
                {
                    var frameworkElement = current as FrameworkElement;
                    if (current is T && frameworkElement != null && frameworkElement.Name == parentName)
                        return (T)current;
                }
                else if (current is T)
                {
                    return (T)current;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        /// <summary>
        ///     Looks for a child control within a parent by name
        /// </summary>
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid.
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }

                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        ///     Looks for a child control within a parent by type
        /// </summary>
        public static T FindChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            // Confirm parent is valid.
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        public static T FindVisualChild<T>(this DependencyObject dependencyObject, string name)
                                                            where T : DependencyObject
        {
            // Search immediate children first (breadth-first)
            var childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);

            //http://stackoverflow.com/questions/12304904/why-visualtreehelper-getchildrencount-returns-0-for-popup

            if (childrenCount == 0 && dependencyObject is Popup)
            {
                var popup = dependencyObject as Popup;
                return popup.Child?.FindVisualChild<T>(name);
            }

            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                var nameOfChild = child.GetValue(FrameworkElement.NameProperty) as string;

                if (child is T && (name == string.Empty || name == nameOfChild))
                    return (T)child;
                var childOfChild = child.FindVisualChild<T>(name);
                if (childOfChild != null)
                    return childOfChild;
            }

            return null;
        }

        public static T FindVisualChild<T>(this DependencyObject dependencyObject) where T : DependencyObject
        {
            return dependencyObject.FindVisualChild<T>(string.Empty);
        }

        public static Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;
            Visual foundElement = null;
            if (element is FrameworkElement frameworkElement) frameworkElement.ApplyTemplate();
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null) break;
            }

            return foundElement;
        }

        public static DataGridColumnHeader GetHeader(DataGridColumn column, DependencyObject reference)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(reference); i++)
            {
                var child = VisualTreeHelper.GetChild(reference, i);

                if (child is DataGridColumnHeader colHeader && colHeader.Column == column) return colHeader;

                colHeader = GetHeader(column, child);
                if (colHeader != null) return colHeader;
            }

            return null;
        }

        /// <summary>
        ///     Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">
        ///     A direct or indirect child of the
        ///     queried item.
        /// </param>
        /// <returns>
        ///     The first parent item that matches the submitted
        ///     type parameter. If not matching item can be found, a null
        ///     reference is being returned.
        /// </returns>
        public static T TryFindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            //get parent item
            var parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            var parent = parentObject as T;
            if (parent != null)
                return parent;
            return TryFindParent<T>(parentObject);
        }

        #endregion Public Methods

        #region Private Methods

        private static IEnumerable<T> GetChildrenOf<T>(this DependencyObject obj, bool recursive) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T) yield return (T)child;

                if (recursive)
                    foreach (var item in child.GetChildrenOf<T>())
                        yield return item;
            }
        }

        private static IEnumerable<T> GetChildrenOf<T>(this DependencyObject obj) where T : DependencyObject
        {
            return obj.GetChildrenOf<T>(false);
        }

        /// <summary>
        ///     This method is an alternative to WPF's
        ///     <see cref="VisualTreeHelper.GetParent" /> method, which also
        ///     supports content elements. Keep in mind that for content element,
        ///     this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>
        ///     The submitted item's parent, if available. Otherwise
        ///     null.
        /// </returns>
        private static DependencyObject GetParentObject(this DependencyObject child)
        {
            if (child == null) return null;

            //handle content elements separately
            var contentElement = child as ContentElement;
            if (contentElement != null)
            {
                var parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                var fce = contentElement as FrameworkContentElement;
                return fce?.Parent;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            var frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                var parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }

        #endregion Private Methods
    }

    /// <summary>
    /// Attached Property "FilterState" to Filter Button
    /// </summary>
    public class FilterState : DependencyObject
    {
        #region Public Fields

        public static readonly DependencyProperty IsFilteredProperty = DependencyProperty.RegisterAttached("IsFiltered",
            typeof(bool), typeof(FilterState), new UIPropertyMetadata(false));

        #endregion Public Fields

        #region Public Methods

        public static bool GetIsFiltered(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFilteredProperty);
        }

        public static void SetIsFiltered(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFilteredProperty, value);
        }

        #endregion Public Methods
    }

    /// <summary>
    ///     Base class for all ViewModel classes in the application. Provides support for
    ///     property changes notification.
    /// </summary>
    [Serializable]
    public abstract class NotifyProperty : INotifyPropertyChanged
    {
        #region Public Events

        /// <summary>
        ///     Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Methods

        /// <summary>
        ///     Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has a new value.</param>
        public void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        ///     Warns the developer if this object does not have a public property with
        ///     the specified name. This method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        private void VerifyPropertyName(string propertyName)
        {
            // verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
                Debug.Fail("Invalid property name: " + propertyName);
        }

        #endregion Private Methods
    }
}