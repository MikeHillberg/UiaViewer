using System;
using System.Collections.Generic;
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
    /// Interaction logic for MessageBar.xaml
    /// </summary>
    public partial class MessageBar : UserControl
    {
        public MessageBar()
        {
            InitializeComponent();

            _root.DataContext = this;

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Tick += _timer_Tick;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
            _timer.Stop();
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(MessageBar), new PropertyMetadata(null, (s,e) => (s as MessageBar).MessageChanged()));

        DispatcherTimer _timer = null;

        void MessageChanged()
        {
            if (_timer.IsEnabled)
                _timer.Stop();
            else
                VisualStateManager.GoToState(this, "Showing", true);

            _timer.Start();
        }


    }
}
