using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ZuneModdingHelper.Behaviors
{
    // https://stackoverflow.com/a/5910833
    public class FadeAnimateItemsBehavior : Behavior<ListBox>
    {
        public DoubleAnimation Animation { get; set; }
        public TimeSpan Tick { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            IEnumerable<ListBoxItem> items;
            if (AssociatedObject.ItemsSource == null)
            {
                items = AssociatedObject.Items.Cast<ListBoxItem>();
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
                            var itemContainer = AssociatedObject.ItemContainerGenerator.ContainerFromItem(cce.NewItems[0]) as ListBoxItem;
                            itemContainer.BeginAnimation(ListBoxItem.OpacityProperty, Animation);
                        }
                    };

                }
                ListBoxItem[] itemsSub = new ListBoxItem[AssociatedObject.Items.Count];
                for (int i = 0; i < itemsSub.Length; i++)
                {
                    itemsSub[i] = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                }
                items = itemsSub;
            }
            foreach (var item in items)
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
                    item.BeginAnimation(ListBoxItem.OpacityProperty, Animation);
                    if (!enumerator.MoveNext())
                    {
                        timer.Stop();
                    }
                };
                timer.Start();
            }
        }
    }
}
