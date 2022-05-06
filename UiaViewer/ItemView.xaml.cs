using Interop.UIautomationCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UiaViewer
{
    /// <summary>
    /// Interaction logic for ItemView.xaml
    /// </summary>
    public partial class ItemView : UserControl
    {
        public ItemView()
        {
            InitializeComponent();
        }

        public AutomationItem AutomationItem
        {
            get { return (AutomationItem)GetValue(AutomationItemProperty); }
            set { SetValue(AutomationItemProperty, value); }
        }
        public static readonly DependencyProperty AutomationItemProperty =
            DependencyProperty.Register("AutomationItem", typeof(AutomationItem), typeof(ItemView),
                new PropertyMetadata(null, (s, e) => (s as ItemView).AutomationItemChanged()));



        void AutomationItemChanged()
        {
            _root.Children.Clear();

            switch (AutomationItem.ControlType)
            {
                case UiaReader.ButtonControlTypeId:
                    {
                        var button = new Button()
                        {
                            Content = AutomationItem.Name,
                            MinWidth = 40
                        };
                        button.Click += (s, e) => AutomationItem.TryInvoke();
                        _root.Children.Add(button);
                    }
                    break;

                case UiaReader.TextControlTypeId:
                    var textBlock = new TextBlock() { Text = AutomationItem.Name };
                    _root.Children.Add(textBlock);

                    AutomationItem.NameChanged += (s, e) =>
                    {
                        textBlock.Text = AutomationItem.Name;
                        MainWindow.MessageBar.Message = AutomationItem.Name;
                    };

                    //UiaReader.Automation.AddPropertyChangedEventHandler(
                    //    AutomationItem.Element,
                    //    TreeScope.TreeScope_Element,
                    //    null, // bugbug: cacheRequest 
                    //    this,
                    //    new int[] { UiaReader.NamePropertyId });

                    break;

                case UiaReader.ListControlTypeId:
                    _root.Children.Add(new TextBlock() { Text = AutomationItem.Name });
                    break;


                default:
                    Debug.Assert(false);
                    _root.Children.Add(new TextBlock() { Text = AutomationItem.Name });
                    break;

            }

        }

        //public void HandlePropertyChangedEvent(IUIAutomationElement sender, int propertyId, object newValue)
        //{
        //    Dispatcher.BeginInvoke(
        //        DispatcherPriority.Normal,
        //        (Action) (() => MainWindow.MessageBar.Message = (string)newValue));
        //}
    }
}
