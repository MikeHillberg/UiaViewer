using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace UiaViewer
{
    /// <summary>
    /// Interaction logic for ParagraphRow.xaml
    /// </summary>
    public partial class ParagraphRow : Grid
    {
        public ParagraphRow()
        {
            DataContext = this;
            InitializeComponent();
        }



        public string ParagraphType
        {
            get { return (string)GetValue(ParagraphTypeProperty); }
            set { SetValue(ParagraphTypeProperty, value); }
        }
        public static readonly DependencyProperty ParagraphTypeProperty =
            DependencyProperty.Register("ParagraphType", typeof(string), typeof(ParagraphRow), new PropertyMetadata("P"));




        public string ParagraphTiming
        {
            get { return (string)GetValue(ParagraphTimingProperty); }
            set { SetValue(ParagraphTimingProperty, value); }
        }
        public static readonly DependencyProperty ParagraphTimingProperty =
            DependencyProperty.Register("ParagraphTiming", typeof(string), typeof(ParagraphRow), new PropertyMetadata(null));



        public int ParagraphFormatCount
        {
            get { return (int)GetValue(ParagraphFormatCountProperty); }
            set { SetValue(ParagraphFormatCountProperty, value); }
        }
        public static readonly DependencyProperty ParagraphFormatCountProperty =
            DependencyProperty.Register("ParagraphFormatCount", typeof(int), typeof(ParagraphRow), new PropertyMetadata(0));



    }
}
