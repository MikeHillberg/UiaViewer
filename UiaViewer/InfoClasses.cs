using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace UiaViewer
{
    public class ParagraphInfo
    {
        static int _globalCounter = 0;
        public int _counter;

        public ParagraphInfo()
        {
            _counter = ++_globalCounter;
        }

        public int HeadingLevel;
        public bool IsVisible;
        public DateTime StartTime;
        public DateTime EndTime;
    }

    public class TextRangeInfo
    {
        public bool IsHidden;
    }


    public class HeadingInfo
    {
        public TextBlock Target { get; set; }
        public string Text { get; set; }
    }


}
