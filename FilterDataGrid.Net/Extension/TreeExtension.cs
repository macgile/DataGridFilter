using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FilterDataGrid.Extension
{
    /// <summary>
    /// Builds a tree structure from a collection of filter items.
    /// </summary>
    internal static class TreeExtension
    {
        private static List<FilterItemDate> ChildrenCheck(this List<FilterItemDate> childrens)
        {
            childrens.ForEach(children =>
            {
                children.Children.ForEach(childChildren => childChildren.ChildrenCheck(children));
                children.Initialize = children.IsChecked;
            }
            );
            return childrens;
        }

        private static void ChildrenCheck(this FilterItemDate children, FilterItemDate parent)
        {
            children.Parent = parent;
            if (children.Children?.Any()??false)
            {
                children.Children.ForEach(child => ChildrenCheck(child, children));
            }
            else if (!children.Item.IsChecked)
            {
                // call the SetIsChecked method of the FilterItemDate class
                children.IsChecked = false;
                // reset with new state (isChanged == false)
                children.Initialize = children.IsChecked;
            }
            // reset with new state
            children.Initialize = children.IsChecked;
        }


        /// <summary>
        /// Builds a tree structure from a collection of filter items.
        /// </summary>
        /// <param name="dates">The collection of filter items.</param>
        /// <returns>A list of FilterItemDate representing the tree structure.</returns>
        internal static Task<List<FilterItemDate>> BuildTreeAsync(this IEnumerable<FilterItem> dates, Loc Translate, Type fieldType)
        {
#if (NET6_0_OR_GREATER)

            if (fieldType == typeof(DateOnly))
                return BuildTree_DateOnlyAsync(dates, Translate, fieldType);
            if (fieldType == typeof(TimeOnly))
                return BuildTree_TimeOnlyAsync(dates, Translate, fieldType);
#endif

            if (fieldType == typeof(TimeSpan))
                return BuildTree_TimeSpanAsync(dates, Translate, fieldType);
            return BuildTree_DateTimeAsync(dates, Translate, fieldType);
        }

        /// <summary>
        /// Builds a tree structure from a collection of filter items.
        /// </summary>
        /// <param name="dates">The collection of filter items.</param>
        /// <returns>A list of FilterItemDate representing the tree structure.</returns>
        internal static async Task<List<FilterItemDate>> BuildTree_DateTimeAsync(this IEnumerable<FilterItem> dates, Loc Translate, Type fieldType)
        {
            var tree = new List<FilterItemDate>
            {
                new FilterItemDate
                {
                    Label = Translate.All, Level = 0, Initialize = true, FieldType = fieldType
                }
            };

            if (dates == null) return tree;

            try
            {
                var dateTimes = dates.Where(x => x.Level > 0).ToList();

                var years = dateTimes.GroupBy(
                    x => ((DateTime)x.Content).Year,
                    (key, group) => new FilterItemDate
                    {
                        Level = 1,
                        Content = key,
                        Label = key.ToString(Translate.Culture),
                        Initialize = true,
                        FieldType = fieldType,
                        Children = group.GroupBy(
                            x => ((DateTime)x.Content).Month,
                            (monthKey, monthGroup) => new FilterItemDate
                            {
                                Level = 2,
                                Content = monthKey,
                                Label = new DateTime(key, monthKey, 1).ToString("MMMM", Translate.Culture),
                                Initialize = true,
                                FieldType = fieldType,
                                Children = monthGroup.Select(x => new FilterItemDate
                                {
                                    Level = 3,
                                    Content = ((DateTime)x.Content).Day,
                                    Label = ((DateTime)x.Content).ToString("dd", Translate.Culture),
                                    Initialize = true,
                                    FieldType = fieldType,
                                    Item = x
                                }).ToList()
                            }).ToList()
                    }).ToList();

                tree.AddRange(years.ChildrenCheck());

                if (dates.Any(x => x.Level == -1))
                {
                    var emptyItem = dates.First(x => x.Level == -1);
                    tree.Add(new FilterItemDate
                    {
                        Label = Translate.Empty,
                        Content = null,
                        Level = -1,
                        FieldType = fieldType,
                        Initialize = emptyItem.IsChecked,
                        Item = emptyItem,
                        Children = new List<FilterItemDate>()
                    });
                }

                tree.First().Tree = tree;
                return await Task.FromResult(tree);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterCommon.BuildTree : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Builds a tree structure from a collection of filter items.
        /// </summary>
        /// <param name="dates">The collection of filter items.</param>
        /// <returns>A list of FilterItemDate representing the tree structure.</returns>
        internal static async Task<List<FilterItemDate>> BuildTree_TimeSpanAsync(this IEnumerable<FilterItem> dates, Loc Translate, Type fieldType)
        {
            var tree = new List<FilterItemDate>
            {
                new FilterItemDate
                {
                    Label = Translate.All, Level = 0, Initialize = true, FieldType = fieldType
                }
            };

            if (dates == null) return tree;

            try
            {
                var dateTimes = dates.Where(x => x.Level > 0).ToList();

                if (dateTimes.Any(x => ((TimeSpan)x.Content).Days > 0))
                {

                    var days = dateTimes.GroupBy(
                        x => ((TimeSpan)x.Content).Days,
                        (daysKey, daysGroup) => new FilterItemDate
                        {
                            Level = 1,
                            Content = daysKey,
                            Label = daysKey.ToString(Translate.Culture),
                            Initialize = true,
                            FieldType = fieldType,
                            Children = daysGroup.GroupBy(
                        x => ((TimeSpan)x.Content).Hours,
                            (hoursKey, hoursGroup) => new FilterItemDate
                            {
                                Level = 1,
                                Content = hoursKey,
                                Label = hoursKey.ToString(Translate.Culture),
                                Initialize = true,
                                FieldType = fieldType,
                                Children = hoursGroup.GroupBy(
                                    x => ((TimeSpan)x.Content).Minutes,
                                    (monthKey, monthGroup) => new FilterItemDate
                                    {
                                        Level = 2,
                                        Content = monthKey,
                                        Label = monthKey,
                                        Initialize = true,
                                        FieldType = fieldType,
                                        Children = monthGroup.Select(x => new FilterItemDate
                                        {
                                            Level = 3,
                                            Content = ((TimeSpan)x.Content).Seconds,
                                            Label = ((TimeSpan)x.Content).Seconds,
                                            Initialize = true,
                                            FieldType = fieldType,
                                            Item = x
                                        }).ToList()
                                    }).ToList()
                            }).ToList()
                        }).ToList();
                    tree.AddRange(days.ChildrenCheck());
                }
                else
                {
                    var hours = dateTimes.GroupBy(
                        x => ((TimeSpan)x.Content).Hours,
                        (key, group) => new FilterItemDate
                        {
                            Level = 1,
                            Content = key,
                            Label = key.ToString(Translate.Culture),
                            Initialize = true,
                            FieldType = fieldType,
                            Children = group.GroupBy(
                                x => ((TimeSpan)x.Content).Minutes,
                                (minutesKey, minutesGroup) => new FilterItemDate
                                {
                                    Level = 2,
                                    Content = minutesKey,
                                    Label = minutesKey,
                                    Initialize = true,
                                    FieldType = fieldType,
                                    Children = minutesGroup.Select(x => new FilterItemDate
                                    {
                                        Level = 3,
                                        Content = ((TimeSpan)x.Content).Seconds,
                                        Label = ((TimeSpan)x.Content).Seconds,
                                        Initialize = true,
                                        FieldType = fieldType,
                                        Item = x
                                    }).ToList()
                                }).ToList()
                        }).ToList();

                    tree.AddRange(hours.ChildrenCheck());
                }

                if (dates.Any(x => x.Level == -1))
                {
                    var emptyItem = dates.First(x => x.Level == -1);
                    tree.Add(new FilterItemDate
                    {
                        Label = Translate.Empty,
                        Content = null,
                        Level = -1,
                        FieldType = fieldType,
                        Initialize = emptyItem.IsChecked,
                        Item = emptyItem,
                        Children = new List<FilterItemDate>()
                    });
                }

                tree.First().Tree = tree;
                return await Task.FromResult(tree);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterCommon.BuildTree : {ex.Message}");
                throw;
            }
        }
#if NET6_0_OR_GREATER
        /// <summary>
        /// Builds a tree structure from a collection of filter items.
        /// </summary>
        /// <param name="dates">The collection of filter items.</param>
        /// <returns>A list of FilterItemDate representing the tree structure.</returns>
        internal static async Task<List<FilterItemDate>> BuildTree_DateOnlyAsync(this IEnumerable<FilterItem> dates, Loc Translate, Type fieldType)
        {
            var tree = new List<FilterItemDate>
            {
                new FilterItemDate
                {
                    Label = Translate.All, Level = 0, Initialize = true, FieldType = fieldType
                }
            };

            if (dates == null) return tree;

            try
            {
                var dateTimes = dates.Where(x => x.Level > 0).ToList();

                var years = dateTimes.GroupBy(
                    x => ((DateOnly)x.Content).Year,
                    (key, group) => new FilterItemDate
                    {
                        Level = 1,
                        Content = key,
                        Label = key.ToString(Translate.Culture),
                        Initialize = true,
                        FieldType = fieldType,
                        Children = group.GroupBy(
                            x => ((DateOnly)x.Content).Month,
                            (monthKey, monthGroup) => new FilterItemDate
                            {
                                Level = 2,
                                Content = monthKey,
                                Label = new DateOnly(key, monthKey, 1).ToString("MMMM", Translate.Culture),
                                Initialize = true,
                                FieldType = fieldType,
                                Children = monthGroup.Select(x => new FilterItemDate
                                {
                                    Level = 3,
                                    Content = ((DateOnly)x.Content).Day,
                                    Label = ((DateOnly)x.Content).ToString("dd", Translate.Culture),
                                    Initialize = true,
                                    FieldType = fieldType,
                                    Item = x
                                }).ToList()
                            }).ToList()
                    }).ToList();

                tree.AddRange(years.ChildrenCheck());

                if (dates.Any(x => x.Level == -1))
                {
                    var emptyItem = dates.First(x => x.Level == -1);
                    tree.Add(new FilterItemDate
                    {
                        Label = Translate.Empty,
                        Content = null,
                        Level = -1,
                        FieldType = fieldType,
                        Initialize = emptyItem.IsChecked,
                        Item = emptyItem,
                        Children = new List<FilterItemDate>()
                    });
                }

                tree.First().Tree = tree;
                return await Task.FromResult(tree);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterCommon.BuildTree : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Builds a tree structure from a collection of filter items.
        /// </summary>
        /// <param name="dates">The collection of filter items.</param>
        /// <returns>A list of FilterItemDate representing the tree structure.</returns>
        internal static async Task<List<FilterItemDate>> BuildTree_TimeOnlyAsync(this IEnumerable<FilterItem> dates, Loc Translate, Type fieldType)
        {
            var tree = new List<FilterItemDate>
            {
                new FilterItemDate
                {
                    Label = Translate.All, Level = 0, Initialize = true, FieldType = fieldType
                }
            };

            if (dates == null) return tree;

            try
            {
                var dateTimes = dates.Where(x => x.Level > 0).ToList();

                var hours = dateTimes.GroupBy(
                    x => ((TimeOnly)x.Content).Hour,
                    (key, group) => new FilterItemDate
                    {
                        Level = 1,
                        Content = key,
                        Label = key.ToString(Translate.Culture),
                        Initialize = true,
                        FieldType = fieldType,
                        Children = group.GroupBy(
                            x => ((TimeOnly)x.Content).Minute,
                            (minuteKey, minuteGroup) => new FilterItemDate
                            {
                                Level = 2,
                                Content = minuteKey,
                                Label = minuteKey,
                                Initialize = true,
                                FieldType = fieldType,
                                Children = minuteGroup.Select(x => new FilterItemDate
                                {
                                    Level = 3,
                                    Content = ((TimeOnly)x.Content).Second,
                                    Label = ((TimeOnly)x.Content).Second,
                                    Initialize = true,
                                    FieldType = fieldType,
                                    Item = x
                                }).ToList()
                            }).ToList()
                    }).ToList();

                tree.AddRange(hours.ChildrenCheck());

                if (dates.Any(x => x.Level == -1))
                {
                    var emptyItem = dates.First(x => x.Level == -1);
                    tree.Add(new FilterItemDate
                    {
                        Label = Translate.Empty,
                        Content = null,
                        Level = -1,
                        FieldType = fieldType,
                        Initialize = emptyItem.IsChecked,
                        Item = emptyItem,
                        Children = new List<FilterItemDate>()
                    });
                }

                tree.First().Tree = tree;
                return await Task.FromResult(tree);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterCommon.BuildTree : {ex.Message}");
                throw;
            }
        }
#endif
    }
}
