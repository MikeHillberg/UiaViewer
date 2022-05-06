using Interop.UIautomationCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        TextBlock _currentParagraphTextBlock;
        ParagraphRow _currentParagraphRow;

        static Dispatcher _dispatcher;

        bool _drawClient = true;

        TextBlock _selectedTextBlock;


        public static MainWindow Instance;
        public MainWindow()
        {
            Instance = this;

            _dispatcher = Dispatcher.CurrentDispatcher;

            DataContext = this;
            InitializeComponent();
            MessageBar = _messageBar;

        }

        static public MessageBar MessageBar { get; private set; }




        public bool CleanUpSpacing
        {
            get { return (bool)GetValue(CleanUpSpacingProperty); }
            set { SetValue(CleanUpSpacingProperty, value); }
        }
        public static readonly DependencyProperty CleanUpSpacingProperty =
            DependencyProperty.Register("CleanUpSpacing", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));




        public ObservableCollection<object> Headings
        {
            get { return (ObservableCollection<object>)GetValue(HeadingsProperty); }
            set { SetValue(HeadingsProperty, value); }
        }
        public static readonly DependencyProperty HeadingsProperty =
            DependencyProperty.Register("Headings", typeof(ObservableCollection<object>), typeof(MainWindow), new PropertyMetadata(null));



        public bool LineBreaks
        {
            get { return (bool)GetValue(LineBreaksProperty); }
            set { SetValue(LineBreaksProperty, value); }
        }
        public static readonly DependencyProperty LineBreaksProperty =
            DependencyProperty.Register("LineBreaks", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));




        public bool DrawClient
        {
            get { return (bool)GetValue(DrawClientProperty); }
            set { SetValue(DrawClientProperty, value); }
        }

        public static readonly DependencyProperty DrawClientProperty =
            DependencyProperty.Register("DrawClient", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));




        public bool ShowEmbeddedObjects
        {
            get { return (bool)GetValue(ShowEmbeddedObjectsProperty); }
            set { SetValue(ShowEmbeddedObjectsProperty, value); }
        }
        public static readonly DependencyProperty ShowEmbeddedObjectsProperty =
            DependencyProperty.Register("ShowEmbeddedObjects", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));




        public bool IncludeSetInformation
        {
            get { return (bool)GetValue(IncludeSetInformationProperty); }
            set { SetValue(IncludeSetInformationProperty, value); }
        }
        public static readonly DependencyProperty IncludeSetInformationProperty =
            DependencyProperty.Register("IncludeSetInformation", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));




        public bool LoadFirstParagraphOnly
        {
            get { return (bool)GetValue(LoadFirstParagraphOnlyProperty); }
            set { SetValue(LoadFirstParagraphOnlyProperty, value); }
        }
        public static readonly DependencyProperty LoadFirstParagraphOnlyProperty =
            DependencyProperty.Register("LoadFirstParagraphOnly", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));





        public bool ShowHiddenText
        {
            get { return (bool)GetValue(ShowHiddenTextProperty); }
            set { SetValue(ShowHiddenTextProperty, value); }
        }
        public static readonly DependencyProperty ShowHiddenTextProperty =
            DependencyProperty.Register("ShowHiddenText", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));



        public bool FormattedText
        {
            get { return (bool)GetValue(FormattedTextProperty); }
            set { SetValue(FormattedTextProperty, value); }
        }
        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.Register("FormattedText", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));




        public bool HighlightRuns
        {
            get { return (bool)GetValue(HighlightRunsProperty); }
            set { SetValue(HighlightRunsProperty, value); }
        }
        public static readonly DependencyProperty HighlightRunsProperty =
            DependencyProperty.Register("HighlightRuns", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));




        public string LoadTime
        {
            get { return (string)GetValue(LoadTimeProperty); }
            set { SetValue(LoadTimeProperty, value); }
        }
        public static readonly DependencyProperty LoadTimeProperty =
            DependencyProperty.Register("LoadTime", typeof(string), typeof(MainWindow), new PropertyMetadata(null));




        public int EmbeddedObjectCount
        {
            get { return (int)GetValue(EmbeddedObjectCountProperty); }
            set { SetValue(EmbeddedObjectCountProperty, value); }
        }
        public static readonly DependencyProperty EmbeddedObjectCountProperty =
            DependencyProperty.Register("EmbeddedObjectCount", typeof(int), typeof(MainWindow), new PropertyMetadata(0));



        public int HiddenFormatCount
        {
            get { return (int)GetValue(HiddenFormatCountProperty); }
            set { SetValue(HiddenFormatCountProperty, value); }
        }
        public static readonly DependencyProperty HiddenFormatCountProperty =
            DependencyProperty.Register("HiddenFormatCount", typeof(int), typeof(MainWindow), new PropertyMetadata(0));


        public int TotalFormatCount
        {
            get { return (int)GetValue(TotalFormatCountProperty); }
            set { SetValue(TotalFormatCountProperty, value); }
        }
        public static readonly DependencyProperty TotalFormatCountProperty =
            DependencyProperty.Register("TotalFormatCount", typeof(int), typeof(MainWindow), new PropertyMetadata(0));




        public ObservableCollection<object> Rows
        {
            get { return (ObservableCollection<object>)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(ObservableCollection<object>), typeof(MainWindow), new PropertyMetadata(null));




        int _loadCounter = 0;
        private void Load(WhatToLoad whatToLoad)
        {
            bool synchronous = Keyboard.Modifiers == ModifierKeys.Shift;

            // bugbug: Share one copy of all this
            var task = new UiaReader(this, ++_loadCounter)
            {
                CleanUpSpacing = CleanUpSpacing,
                FormattedText = FormattedText,
                ShowHiddenText = ShowHiddenText,
                Synchronous = synchronous,
                WhatToLoad = whatToLoad,
                HighlightRuns = HighlightRuns,
                ShowEmbeddedObjects = ShowEmbeddedObjects,
                IncludeSetInformation = IncludeSetInformation,
                LoadFirstParagraphOnly = LoadFirstParagraphOnly
            };


            LoadTime = "Loading ...";
            HiddenFormatCount = 0;
            TotalFormatCount = 0;
            EmbeddedObjectCount = 0;

            Rows = new ObservableCollection<object>();

            Headings = null;
            HeadingsWidth = 0;

            task.LoadContent();
        }




        public double HeadingsWidth
        {
            get { return (double)GetValue(HeadingsWidthProperty); }
            set { SetValue(HeadingsWidthProperty, value); }
        }
        public static readonly DependencyProperty HeadingsWidthProperty =
            DependencyProperty.Register("HeadingsWidth", typeof(double), typeof(MainWindow), new PropertyMetadata(0.0));



        public int DispatcherBacklog
        {
            get { return (int)GetValue(DispatcherBacklogProperty); }
            set { SetValue(DispatcherBacklogProperty, value); }
        }
        public static readonly DependencyProperty DispatcherBacklogProperty =
            DependencyProperty.Register("DispatcherBacklog", typeof(int), typeof(MainWindow), new PropertyMetadata(0));


        Queue<Action> _actions = new Queue<Action>();
        int postCount = 0;
        int dispatchCount = 0;
        bool _posted = false;


        public void CreateRun(string text, string toolTip)
        {
            _run = new Run()
            {
                Text = text,
                ToolTip = toolTip
            };
            SetRunHighlighting(hidden: false);

        }

        public void NotifyRunIsHidden()
        {
            if (ShowHiddenText)
            {
                _run.Background = Brushes.LightGray;

                SetRunHighlighting(hidden: true);
            }
            else
                _run.Text = null;
        }

        void SetRunHighlighting( bool hidden)
        {
            if (HighlightRuns)
            {
                _run.MouseEnter += (s, e) => (s as Run).Background = Brushes.Yellow;
                _run.MouseLeave += (s, e) => (s as Run).Background = hidden ? Brushes.LightGray : null;
            }

        }


        public void InsertRunIntoParagraph()
        {
            if (_run != null)
            {
                if (LineBreaks)
                    _run.Text += "\n";

                _currentParagraphTextBlock.Inlines.Add(_run);
                _run = null;
            }
        }

        public void RemoveCurrentParagraph()
        {
            if (Headings != null)
            {
                var headingInfo = Headings.Last() as HeadingInfo;
                if (headingInfo.Target == _currentParagraphTextBlock)
                    Headings.Remove(headingInfo);
            }

            Rows.Remove(_currentParagraphRow);
        }


        public void NotifyComplete(TimeSpan deltaTime, int formatCount = 0, int hiddenCount = 0, int totalEmbeddedObjectCount = 0)
        {
            TotalFormatCount = formatCount;
            HiddenFormatCount = hiddenCount;
            EmbeddedObjectCount = totalEmbeddedObjectCount;
            LoadTime = deltaTime.TotalMilliseconds.ToString();

            if (Headings != null && Headings.Count != 0)
                HeadingsWidth = 200;
            else
                HeadingsWidth = 0;
        }

        public void NotifyEndOfParagraph(double paragraphDelta, int paragraphFormatCount)
        {
            _currentParagraphRow.ParagraphTiming = paragraphDelta.ToString();
            _currentParagraphRow.ParagraphFormatCount = paragraphFormatCount;
        }

        public void InsertImage(string name, int position, int sizeOfSet)
        {
            var run = new Run();
            run.Foreground = Brushes.Red;
            run.FontSize = 14;
            run.FontFamily = new FontFamily("Courier");
            run.Text = "[X]";
            run.ToolTip = "Name: " + name + ", Position: " + position + "/" + sizeOfSet;

            _currentParagraphTextBlock.Inlines.Add(run);
        }

        public void InsertHyperlink(string value, string name, int position, int sizeOfSet)
        {
            var hyperlink = new Hyperlink();
            if (!string.IsNullOrEmpty(value) && Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute))
            {
                hyperlink.NavigateUri = new Uri(value);
                hyperlink.Inlines.Add(new Run() { Text = name });
                hyperlink.ToolTip = "Name: " + name + ", Position: " + position + "/" + sizeOfSet;
            }

            _currentParagraphTextBlock.Inlines.Add(hyperlink);

        }

        public void InsertRadioButton(bool isSelected, int position, int sizeOfSet)
        {
            var radioButton = new RadioButton() { IsChecked = isSelected };
            var inlineContainer = new InlineUIContainer() { Child = WrapWithBorder(radioButton) };
            inlineContainer.ToolTip = "Position: " + position + "/" + sizeOfSet;
            _currentParagraphTextBlock.Inlines.Add(inlineContainer);

        }

        private Border WrapWithBorder(Control control)
        {
            var border = new Border();
            border.Padding = new Thickness(5, 2, 5, 2);
            border.Child = control;

            if (HighlightRuns)
            {
                border.MouseEnter += (s, e) => border.Background = Brushes.Yellow;
                border.MouseLeave += (s, e) => border.Background = null;
            }

            return border;
        }

        public void InsertEditControl(string currentValue, int position, int sizeOfSet)
        {
            var textBox = new TextBox();
            textBox.Text = currentValue;

            var inlineContainer = new InlineUIContainer() { Child = WrapWithBorder(textBox) };
            inlineContainer.ToolTip = "Position: " + position + "/" + sizeOfSet;
            _currentParagraphTextBlock.Inlines.Add(inlineContainer);

        }


        public void InsertButton(string buttonContent, int position, int sizeOfSet)
        {
            var button = new Button() { Content = buttonContent };

            var inlineContainer = new InlineUIContainer() { Child = WrapWithBorder(button) };
            inlineContainer.ToolTip = "Position: " + position + "/" + sizeOfSet;
            _currentParagraphTextBlock.Inlines.Add(inlineContainer);

        }

        public void InsertCheckBox(bool? isChecked, int positionInSet, int sizeOfSet)
        {
            var checkBox = new CheckBox();
            checkBox.IsChecked = isChecked;

            var inlineContainer = new InlineUIContainer() { Child = WrapWithBorder(checkBox) };
            inlineContainer.ToolTip = "Position: " + positionInSet + "/" + sizeOfSet;

            _currentParagraphTextBlock.Inlines.Add(inlineContainer);

        }

        public void SetFontFamily(string fontFamilyString)
        {
            _run.FontFamily = new FontFamily(fontFamilyString);
        }

        public void SetFontStyle(FontStyle fontStyle)
        {
            _run.FontStyle = fontStyle;
        }

        public void SetFontSize(double fontSize)
        {
            if (fontSize == 0)
            {
                // FontSize can't be zero
                _run.Text = null;
            }
            else
                _run.FontSize = fontSize * 96f / 72f;
        }

        public void SetHeading(string headingLevelString, string paragraphText)
        {
            _currentParagraphTextBlock.Margin = new Thickness(0, 10, 0, 0);
            _currentParagraphRow.ParagraphType = headingLevelString;

            if (Headings == null)
            {
                Headings = new ObservableCollection<object>();
            }

            var headingInfo = new HeadingInfo
            {
                Target = _currentParagraphTextBlock,
                Text = headingLevelString + "\n" + paragraphText

            };
            Headings.Add(headingInfo);
        }

        public void RunOnUI(int loadCounter, Action action, bool synchronous, bool ignoreDrawClient = false)
        {
            if (!_drawClient && !ignoreDrawClient)
                return;

            postCount++;
            //Debug.WriteLine("Post delta: " + (postCount - dispatchCount).ToString());

            lock (_actions)
            {
                _actions.Enqueue(action);

                if (_posted)
                    return;

                _posted = true;
            }


            var uiAction = new Action(() =>
            {

                Queue<Action> actions = null;
                lock (_actions)
                {
                    actions = new Queue<Action>(_actions);
                    _posted = false;
                    _actions.Clear();
                }

                if (loadCounter != _loadCounter)
                    return;

                if (!DrawClient && !ignoreDrawClient)
                    return;

                while (actions.Count != 0)
                {
                    dispatchCount++;
                    DispatcherBacklog = postCount - dispatchCount;

                    var a = actions.Dequeue();
                    a();
                }
            });

            if (!synchronous)
                _dispatcher.BeginInvoke(DispatcherPriority.Input, uiAction);
            else
                uiAction();
        }


        Run _run = null;



        public void CreateParagraph()
        {
            _currentParagraphRow = new ParagraphRow();
            _currentParagraphTextBlock = _currentParagraphRow.ParagraphTextBlock;

            Rows.Add(_currentParagraphRow);
        }


        public bool IsTaskCurrent(int taskID)
        {
            return taskID == _loadCounter;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as ListBoxItem;
            var headerInfo = menuItem.Content as HeadingInfo;
            headerInfo.Target.BringIntoView();

            if (_selectedTextBlock != null)
                _selectedTextBlock.Background = null;

            _selectedTextBlock = headerInfo.Target;
            _selectedTextBlock.Background = Brushes.LightBlue;

            menuItem.IsSelected = false;


        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {

            Title = "UIA View (IE)";
            Load( WhatToLoad.IE);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Title = "UIA View (Edge)";
            Load(WhatToLoad.Edge);
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            Title = "UIA View (Calculator)";
            Load(WhatToLoad.Calculator);
        }


        internal void SetFontWeight(FontWeight fontWeight)
        {
            _run.FontWeight = fontWeight;
        }

        internal void SetBaselineAlignment(BaselineAlignment alignment)
        {
            _run.BaselineAlignment = alignment;
        }

        internal void SetTextDecorations(TextDecorationCollection decorations)
        {
            _run.TextDecorations = decorations;
        }

    }


    public enum WhatToLoad
    {
        IE,
        Edge,
        Calculator
    }




}
