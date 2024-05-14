using Microsoft.Xaml.Behaviors;
using OwlCore.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ZuneModdingHelper.Behaviors;

// https://stackoverflow.com/a/5910833
public class FadeAnimateItemsBehavior : Behavior<ItemsControl>
{
    public DoubleAnimation Animation { get; set; }
    public TimeSpan Tick { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += AssociatedObject_Loaded;
    }

    void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
    {
        IEnumerable<UIElement> items;
        if (AssociatedObject.ItemsSource == null)
        {
            items = AssociatedObject.Items.Cast<UIElement>();
        }
        else
        {
            var itemsSource = AssociatedObject.ItemsSource;
            if (itemsSource is INotifyCollectionChanged)
            {
                var collection = itemsSource as INotifyCollectionChanged;
                collection.CollectionChanged += (s, cce) =>
                {
                    if (cce.Action == NotifyCollectionChangedAction.Add)
                    {
                        var itemContainer = AssociatedObject.ItemContainerGenerator.ContainerFromItem(cce.NewItems[0]) as UIElement;
                        itemContainer.BeginAnimation(UIElement.OpacityProperty, Animation);
                    }
                };

            }
            UIElement[] itemsSub = new UIElement[AssociatedObject.Items.Count];
            for (int i = 0; i < itemsSub.Length; i++)
            {
                itemsSub[i] = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(i) as UIElement;
            }
            items = itemsSub;
        }
        foreach (var item in items.PruneNull())
        {
            item.Opacity = 0;
        }
        var enumerator = items.GetEnumerator();
        if (enumerator.MoveNext())
        {
            DispatcherTimer timer = new() { Interval = Tick };
            timer.Tick += (s, timerE) =>
            {
                var item = enumerator.Current;
                if (item is null) return;

                item.BeginAnimation(UIElement.OpacityProperty, Animation);
                if (!enumerator.MoveNext())
                {
                    timer.Stop();
                }
            };
            timer.Start();
        }
    }
}

public class FadeAnimateItemsPanelBehavior : Behavior<Panel>
{
    public DoubleAnimation Animation { get; set; }
    public TimeSpan Tick { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += AssociatedObject_Loaded;
    }

    void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
    {
        var items = AssociatedObject.Children.OfType<UIElement>().PruneNull();

        foreach (var item in items)
            item.Opacity = 0;

        var enumerator = items.GetEnumerator();
        if (enumerator.MoveNext())
        {
            DispatcherTimer timer = new() { Interval = Tick };
            timer.Tick += (s, timerE) =>
            {
                var item = enumerator.Current;
                if (item is null) return;

                item.BeginAnimation(UIElement.OpacityProperty, Animation);
                if (!enumerator.MoveNext())
                {
                    timer.Stop();
                }
            };
            timer.Start();
        }
    }
}
