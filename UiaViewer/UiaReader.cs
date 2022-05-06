
//
// UiaReader uses the UIA APIs to read the target app.
//

using Interop.UIautomationCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace UiaViewer
{
    public class UiaReader
    {
        MainWindow _window;
        int _loadID;


        static UiaReader()
        {
            Automation = new CUIAutomation();

            _conditionTextPattern = Automation.CreatePropertyCondition(_isTextPatternAvailablePropertyId, true);
            _conditionSelectionItemPattern = Automation.CreatePropertyCondition(_isSelectionItemPatternAvailablePropertyId, true);
            _conditionValuePattern = Automation.CreatePropertyCondition(_isValuePatternAvailablePropertyId, true);
            _conditionTogglePattern = Automation.CreatePropertyCondition(_isTogglePatternAvailablePropetyId, true);
        }


        public UiaReader(MainWindow window, int loadID)
        {
            _window = window;
            _loadID = loadID;
        }

        public bool CleanUpSpacing;
        public bool FormattedText;
        public bool ShowHiddenText;
        public bool Synchronous;
        public WhatToLoad WhatToLoad;
        public bool HighlightRuns;
        public bool ShowEmbeddedObjects;
        public bool IncludeSetInformation;
        public bool LoadFirstParagraphOnly;

        public static IUIAutomation Automation; // Main UIA object required by any UIA client app.

        static IUIAutomationCondition _conditionTextPattern;
        static IUIAutomationCondition _conditionSelectionItemPattern;
        static IUIAutomationCondition _conditionValuePattern;
        static IUIAutomationCondition _conditionTogglePattern;

        public const int NamePropertyId = 30005; // UIA_NamePropertyId
        const int _classNamePropertyId = 30012; // UIA_ClassNamePropertyId
        const int _frameworkIdPropertyId = 30024;
        const int _controlTypePropertyId = 30003; // UIA_ControlTypePropertyId
        const int _postionInSetPropertyId = 30152; // UIA_PositionInSetPropertyId
        const int _sizeOfSetPropertyId = 30153; // UIA_SizeOfSetPropertyId
        const int _toggleToggleStatePropertyId = 30086; // UIA_ToggleToggleStatePropertyId
        const int _valueValuePropertyId = 30045; // UIA_ValueValuePropertyId
        const int _selectionItemIsSelectedPropertyId = 30079; // UIA_SelectionItemIsSelectedPropertyId



        const int _isTextPatternAvailablePropertyId = 30040; // UIA_IsTextPatternAvailablePropertyId
        const int _textPatternPatternId = 10014; // UIA_TextPatternId

        const int _isSelectionItemPatternAvailablePropertyId = 30036; // UIA_IsSelectionItemPatternAvailablePropertyId  30036
        const int _selectionItemPatternId = 10010; // UIA_SelectionItemPatternId  10010

        const int _isTogglePatternAvailablePropetyId = 30041; // UIA_IsTogglePatternAvailablePropertyId 30041
        const int _togglePatternId = 10015; // UIA_TogglePatternId 10015

        const int _isValuePatternAvailablePropertyId = 30043; // UIA_IsValuePatternAvailablePropertyId  30043
        public const int ValuePatternId = 10002; // UIA_ValuePatternId 10002
        public const int InvokePatternId = 10000; // UIA_InvokePatternId



        private int _fontWeightAttributeId = 40007; // UIA_FontWeightAttributeId  400007
        private int _superscriptAttributeId = 40017; // UIA_IsSuperscriptAttributeId 40017
        private int _subscriptAttributeId = 40016; // UIA_IsSubscriptAttributeId 40016
        private int _fontNameAttributeId = 40005;
        private int _italicAttributeId = 40014; // UIA_IsItalicAttributeId  40014
        private int _underlineStyleAttributeId = 40030; // UIA_UnderlineStyleAttributeId
        private int _fontSizeAttributeId = 40006; // UIA_FontSizeAttributeId
        private int _isHiddenAttributeId = 40013; // UIA_IsHiddenAttributeId
        private int _styleIdAttributeId = 40034; // UIA_StyleIdAttributeId 

        public const int _imageControlTypeId = 0xC356;
        public const int _hyperlinkControlTypeId = 0xC355;
        public const int _radioButtonControlTypeId = 0xC35D;
        public const int _editControlTypeId = 0xC354;
        public const int ButtonControlTypeId = 0xC350;
        public const int _checkBoxControlTypeId = 0xC352;
        public const int TextControlTypeId = 50020; // UIA_TextControlTypeId
        public const int ListControlTypeId = 50008; // UIA_ListControlTypeId



        public void LoadContent()
        {
            if (Synchronous)
            {
                LoadContentInternal();
            }
            else
            {
                Task.Run(() =>
                {
                    try
                    {
                        LoadContentInternal();
                    }
                    catch (Exception e)
                    {
                        RunOnUI(() => MessageBox.Show("Failed:  " + e.Message));
                    }
                });
            }

        }
        private void LoadContentInternal()
        {
            IUIAutomationElement mainPane = null;
            switch (WhatToLoad)
            {
                case WhatToLoad.Edge:
                    mainPane = GetEdgeMainPane();
                    break;

                case WhatToLoad.IE:
                    mainPane = GetIEMainPane();
                    break;

                case WhatToLoad.Calculator:
                    mainPane = GetCalcMainPane();
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            Debug.Assert(mainPane != null);
            Debug.WriteLine(mainPane.CurrentProviderDescription);
            Debug.WriteLine(mainPane.CurrentProcessId);


            // bugbug
            IUIAutomationCacheRequest cacheRequest = Automation.CreateCacheRequest();
            cacheRequest.TreeScope = TreeScope.TreeScope_Element; // rs: follow up
            cacheRequest.AddPattern(_textPatternPatternId);

            var textElement = mainPane.FindFirstBuildCache(TreeScope.TreeScope_Element, _conditionTextPattern, cacheRequest);

            if (textElement == null)
                DisplayByItem(mainPane);
            else
                DisplayWithTextPattern(textElement);

        }

        private void DisplayByItem(IUIAutomationElement mainPane)
        {
            var startTime = DateTime.Now;

            var children = mainPane.GetCachedChildren();
            if (children == null)
                return;

            for (int i = 0; i < children.Length; i++)
            {
                var child = children.GetElement(i);
                var item = new AutomationItem(child);

                //item.Name = child.CurrentName;
                item.ControlType = child.CurrentControlType;


                RunOnUI(() => MainWindow.Instance.Rows.Add(item));
            }

            var deltaTime = DateTime.Now - startTime;

            RunOnUI(() =>
            {
                _window.NotifyComplete(deltaTime);
            });
        }

        private void DisplayWithTextPattern(IUIAutomationElement textElement)
        {
            int totalFormatCount = 0;
            int hiddenFormatCount = 0;
            int totalEmbeddedObjectCount = 0;

            var start = DateTime.Now;

            var textPattern = (IUIAutomationTextPattern)textElement.GetCachedPattern(_textPatternPatternId);

            var textRange = textPattern.DocumentRange;

            textRange.ExpandToEnclosingUnit(TextUnitSize);// (TextUnit.TextUnit_Format);
            var paragraphRange = textRange.Clone();
            paragraphRange.ExpandToEnclosingUnit(TextUnit.TextUnit_Paragraph);


            int moved = 1;
            ParagraphInfo paragraphInfo = null;
            int paragraphFormatCount = 0;

            while (moved != 0)
            {
                if (!_window.IsTaskCurrent(_loadID))
                    return; // Abort worker thread

                totalFormatCount++;
                paragraphFormatCount++;

                var textRangeInfo = new TextRangeInfo();

                // rs: -1 works.  Options to do non-formatted.  Do range from child.  GetVisibleRanges (but too big).  Do move by line then by format?
                var text = textRange.GetText(-1);
                Debug.WriteLine(text);

                if (paragraphInfo == null)
                {
                    paragraphInfo = new ParagraphInfo();
                    paragraphInfo.StartTime = DateTime.Now;

                    RunOnUI(() =>
                    {
                        _window.CreateParagraph();
                    });

                    if (FormattedText)
                    {
                        var attributeValue = GetTextAttribute(textRange, _styleIdAttributeId);
                        if (attributeValue != null)
                        {
                            var styleId = (int)attributeValue;
                            if (styleId >= 70001 && styleId <= 70009)
                            {
                                paragraphInfo.HeadingLevel = styleId - 70001 + 1;

                                var paragraphText = paragraphRange.GetText(200);

                                var headingLevel = styleId - 70001 + 1;
                                RunOnUI(() =>
                                {
                                    var headingLevelString = "H" + headingLevel.ToString();
                                    _window.SetHeading(headingLevelString, paragraphText);
                                });
                            }
                        }

                    }

                }



                if (!string.IsNullOrEmpty(text))
                {
                    if (CleanUpSpacing)
                    {
                        var sb = new StringBuilder();
                        var lines = text.Split('\n');
                        if (lines.Length != 1)
                        {
                            for (int i = 0; i < lines.Length; i++)
                            {
                                sb.Append(lines[i].Trim());

                                if (i < lines.Length - 1 || lines[i].EndsWith(" "))
                                    sb.Append(" ");
                            }
                            text = sb.ToString();
                        }
                    }


                    var boundingRectangle = textRange.GetBoundingRectangles();
                    string toolTip = null;
                    if (boundingRectangle != null && boundingRectangle.Length == 4)
                    {
                        var rect = new Rect(boundingRectangle[0], boundingRectangle[1], boundingRectangle[2], boundingRectangle[3]);

                        RunOnUI(() =>
                        {
                            toolTip = "Bounds: " + rect.ToString();
                        });
                    }

                    RunOnUI(() =>
                    {
                        _window.CreateRun(text, toolTip);
                    });



                    var attributeValue = GetTextAttribute(textRange, _isHiddenAttributeId);
                    if (attributeValue != null)
                    {
                        if ((bool)attributeValue == true)
                        {
                            hiddenFormatCount++;

                            textRangeInfo.IsHidden = true;

                            RunOnUI(() =>
                            {
                                _window.NotifyRunIsHidden();
                            });
                        }
                    }

                    if (!textRangeInfo.IsHidden)
                        paragraphInfo.IsVisible = true;


                    // Format the text

                    if (!textRangeInfo.IsHidden || ShowHiddenText)
                    {
                        if (FormattedText)
                        {
                            FormatText(textRange);
                        }
                    }

                    // Check for embedded objects 
                    totalEmbeddedObjectCount += ProcessEmbeddedObjects(textRange, textPattern);

                    RunOnUI(() =>
                    {
                        _window.InsertRunIntoParagraph();
                    });
                }

                moved = textRange.Move(TextUnitSize/*TextUnit.TextUnit_Format*/, 1);
                if (moved == 0)
                {
                    paragraphInfo.EndTime = DateTime.Now;
                    FinalizeParagraphStats(paragraphInfo, paragraphFormatCount);
                    break;
                }

                var compare = textRange.CompareEndpoints(
                    TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start,
                    paragraphRange,
                    TextPatternRangeEndpoint.TextPatternRangeEndpoint_End);

                if (compare > 0)
                {
                    if (!paragraphInfo.IsVisible)
                    {
                        RunOnUI(() =>
                        {
                            _window.RemoveCurrentParagraph();
                        });
                    }
                    else
                    {
                        paragraphInfo.EndTime = DateTime.Now;

                        var paragraphInfoTemp = paragraphInfo;
                        var finalParagraphFormatCount = paragraphFormatCount;

                        FinalizeParagraphStats(paragraphInfoTemp, finalParagraphFormatCount);

                        if (LoadFirstParagraphOnly)
                            break;
                    }

                    paragraphRange = textRange.Clone();
                    paragraphRange.ExpandToEnclosingUnit(TextUnit.TextUnit_Paragraph);

                    paragraphInfo = null;
                    paragraphFormatCount = 0;

                }
            }

            var deltaTime = DateTime.Now - start;

            RunOnUI(() =>
            {
                _window.NotifyComplete(deltaTime, totalFormatCount, hiddenFormatCount, totalEmbeddedObjectCount);
            });
        }

        TextUnit TextUnitSize
        {
            get
            {
                // This seems to slow things down
                //if (!FormattedText && !ShowEmbeddedObjects)
                //    return TextUnit.TextUnit_Line;
                //else
                return TextUnit.TextUnit_Format;
            }
        }


        IUIAutomationElement GetEdgeMainPane()
        {
            var hwnd = FindWindow("ApplicationFrameWindow", "- Microsoft Edge");
            //IntPtr hwnd = Win32.FindWindow("ApplicationFrameWindow", null);

            if (hwnd != IntPtr.Zero)
            {
                IUIAutomationElement appWindow = Automation.ElementFromHandle(hwnd);

                var className = appWindow.CurrentName; // Bugbug: why can't I use CachedName?

                var serverWindow = appWindow.FindFirst(
                    TreeScope.TreeScope_Descendants,
                    Automation.CreatePropertyCondition(_classNamePropertyId, "Internet Explorer_Server"));
                Debug.WriteLine("Server window: " + serverWindow.CurrentProviderDescription);


                var condition = Automation.CreateAndCondition(
                    Automation.CreatePropertyCondition(_frameworkIdPropertyId, "MicrosoftEdge"),
                    Automation.CreatePropertyCondition(_classNamePropertyId, "")
                    );


                return appWindow.FindFirst(
                    TreeScope.TreeScope_Descendants,
                    condition);

            }

            return null;
        }

        IntPtr FindWindow(string windowClassName, string windowTitleSuffix)
        {
            IntPtr hwnd = IntPtr.Zero;

            Win32.EnumWindows(
                (currentHwnd, lParam) =>
                {

                    var sb = new StringBuilder(256);
                    if (Win32.GetClassName(currentHwnd, sb, sb.Capacity) == 0)
                        return true;

                    if (sb.ToString() != windowClassName)
                        return true;

                    sb.Clear();
                    if (Win32.GetWindowText(currentHwnd, sb, sb.Capacity) == 0)
                        return true;

                    if (!sb.ToString().EndsWith(windowTitleSuffix))
                        return true;


                    hwnd = currentHwnd;
                    return false;

                },
               IntPtr.Zero);

            return hwnd;
        }



        IUIAutomationElement GetIEMainPane()
        {
            //IntPtr hwnd = Win32.FindWindow("IEFrame", null);
            var hwnd = FindWindow("IEFrame", "- Internet Explorer");

            if (hwnd != IntPtr.Zero)
            {
                var appWindow = Automation.ElementFromHandle(hwnd);

                var serverWindow = appWindow.FindFirst(
                    TreeScope.TreeScope_Descendants,
                    Automation.CreatePropertyCondition(_classNamePropertyId, "Internet Explorer_Server"));


                var mainPane = serverWindow.FindFirst(
                    TreeScope.TreeScope_Descendants,
                    Automation.CreatePropertyCondition(_frameworkIdPropertyId, "InternetExplorer"));

                var className = mainPane.CurrentName;
                var providerDescription = mainPane.CurrentProviderDescription;

                return mainPane;
            }

            return null;
        }



        IUIAutomationElement GetCalcMainPane()
        {
            IntPtr hwnd = Win32.FindWindow("ApplicationFrameWindow", "Calculator");
            if (hwnd != IntPtr.Zero)
            {
                IUIAutomationElement appWindow = Automation.ElementFromHandle(hwnd);

                var className = appWindow.CurrentName; // Bugbug: why can't I use CachedName?

                IUIAutomationCacheRequest cacheRequest = Automation.CreateCacheRequest();
                cacheRequest.TreeScope = TreeScope.TreeScope_Subtree;

                var serverWindow = appWindow.FindFirstBuildCache(
                    TreeScope.TreeScope_Descendants,
                    Automation.CreatePropertyCondition(_classNamePropertyId, "Windows.UI.Core.CoreWindow"),
                    cacheRequest);

                return serverWindow;

            }

            return null;
        }
        void RunOnUI(Action action)
        {
            _window.RunOnUI(_loadID, action, Synchronous);
        }




        object GetTextAttribute(IUIAutomationTextRange element, int id)
        {
            object attributeValue = element.GetAttributeValue(id);

            if (attributeValue == Automation.ReservedMixedAttributeValue
                || attributeValue == Automation.ReservedNotSupportedValue)
            {
                return null;
            }
            else
            {
                return attributeValue;
            }
        }

        FontWeight ConvertLogFontWeight(int value)
        {
            switch (value)
            {
                case 100:
                    return FontWeights.Thin;
                case 200:
                    return FontWeights.ExtraLight;
                case 300:
                    return FontWeights.Light;
                case 500:
                    return FontWeights.Medium;
                case 600:
                    return FontWeights.SemiBold;
                case 700:
                    return FontWeights.Bold;
                case 800:
                    return FontWeights.ExtraBold;
                case 900:
                    return FontWeights.Heavy;

                case 0:
                case 400:
                default:
                    return FontWeights.Normal;

            }
        }

        private int ProcessEmbeddedObjects(IUIAutomationTextRange textRange, IUIAutomationTextPattern textPattern)
        {
            if (!ShowEmbeddedObjects)
                return 0;

            IUIAutomationCacheRequest cacheRequest = Automation.CreateCacheRequest();
            cacheRequest.TreeScope = TreeScope.TreeScope_Element;
            cacheRequest.AddProperty(NamePropertyId);

            cacheRequest.AddPattern(_togglePatternId);
            cacheRequest.AddPattern(ValuePatternId);
            cacheRequest.AddPattern(_selectionItemPatternId);
            cacheRequest.AddProperty(_controlTypePropertyId);

            cacheRequest.AddProperty(_toggleToggleStatePropertyId);
            cacheRequest.AddProperty(_valueValuePropertyId);
            cacheRequest.AddProperty(_selectionItemIsSelectedPropertyId);

            if (IncludeSetInformation)
            {
                cacheRequest.AddProperty(_postionInSetPropertyId);
                cacheRequest.AddProperty(_sizeOfSetPropertyId);
            }


            var children = textRange.GetChildren();
            for (int i = 0; i < children.Length; i++)
            {
                var child = children.GetElement(i).BuildUpdatedCache(cacheRequest);
                var child4 = child as IUIAutomationElement4;

                var enclosingRange = textPattern.RangeFromChild(child);

                //var clone = textRange.Clone();
                //clone.MoveEndpointByRange(
                //    TextPatternRangeEndpoint.TextPatternRangeEndpoint_End, 
                //    enclosingRange, 
                //    TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start);


                var controlType = child.CachedControlType;
                switch (controlType)
                {
                    case _imageControlTypeId:
                        InsertImage(child);
                        break;

                    case _hyperlinkControlTypeId:
                        InsertHyperlink(child);
                        break;

                    case _radioButtonControlTypeId:
                        InsertRadioButton(child);
                        break;

                    case _editControlTypeId:
                        InsertEditControl(child);
                        break;

                    case ButtonControlTypeId:
                        InsertButton(child);
                        break;

                    case _checkBoxControlTypeId:
                        InsertCheckBox(child);
                        break;

                    default:
                        Debug.WriteLine("");
                        break;
                }

            }

            return children.Length;
        }

        private void FormatText(IUIAutomationTextRange textRange)
        {
            object fontFamilyValue = textRange.GetAttributeValue(_fontNameAttributeId);
            if (fontFamilyValue != Automation.ReservedMixedAttributeValue
                && fontFamilyValue != Automation.ReservedNotSupportedValue)
            {
                // bugbug:  How can it be Mixed?

                var fontFamily = (string)fontFamilyValue;

                RunOnUI(() =>
                {
                    _window.SetFontFamily(fontFamily);
                });
            }

            object attributeValue = GetTextAttribute(textRange, _italicAttributeId);
            if (attributeValue != null)
            {
                if (((bool)attributeValue))
                {
                    RunOnUI(() =>
                    {
                        _window.SetFontStyle(FontStyles.Italic);
                    });
                }
            }

            attributeValue = GetTextAttribute(textRange, _fontSizeAttributeId);
            if (attributeValue != null)
            {
                var fontSize = (double)attributeValue;
                RunOnUI(() =>
                {
                    _window.SetFontSize(fontSize);
                });
            }



            attributeValue = GetTextAttribute(textRange, _fontWeightAttributeId);
            if (attributeValue != null)
            {
                var fontWeight = (Int32)attributeValue;
                RunOnUI(() =>
                {
                    _window.SetFontWeight(ConvertLogFontWeight(fontWeight));
                });
            }

            attributeValue = GetTextAttribute(textRange, _superscriptAttributeId);
            if (attributeValue != null && (bool)attributeValue == true)
            {
                RunOnUI(() =>
                {
                    _window.SetBaselineAlignment(BaselineAlignment.Superscript);
                });
            }

            attributeValue = GetTextAttribute(textRange, _subscriptAttributeId);
            if (attributeValue != null && (bool)attributeValue == true)
            {
                RunOnUI(() =>
                {
                    _window.SetBaselineAlignment(BaselineAlignment.Subscript);
                });
            }


            attributeValue = GetTextAttribute(textRange, _underlineStyleAttributeId);
            if (attributeValue != null)
            {
                if ((int)attributeValue != 0)
                {
                    RunOnUI(() =>
                    {
                        _window.SetTextDecorations(TextDecorations.Underline);
                    });
                }
            }
        }



        private void InsertCheckBox(IUIAutomationElement child)
        {
            var pattern = (IUIAutomationTogglePattern)child.GetCachedPattern(_togglePatternId);

            int sizeOfSet = 0, positionInSet = 0;
            if (IncludeSetInformation)
            {
                sizeOfSet = child.GetSizeOfSet();
                positionInSet = child.GetPositionInSet();
            }

            bool? isChecked = null;
            switch (pattern.CachedToggleState) // bugbug: cache
            {
                case ToggleState.ToggleState_On:
                    isChecked = true;
                    break;

                case ToggleState.ToggleState_Off:
                    isChecked = false;
                    break;

                    // otherwise null
            }

            RunOnUI(() =>
            {
                _window.InsertCheckBox(isChecked, positionInSet, sizeOfSet);

            });
        }

        private void InsertButton(IUIAutomationElement child)
        {
            int sizeOfSet = 0, positionInSet = 0;
            if (IncludeSetInformation)
            {
                sizeOfSet = child.GetSizeOfSet();
                positionInSet = child.GetPositionInSet();
            }

            var buttonContent = child.CachedName;

            RunOnUI(() =>
            {
                _window.InsertButton(buttonContent, positionInSet, sizeOfSet);
            });
        }

        private void InsertEditControl(IUIAutomationElement child)
        {
            int sizeOfSet = 0, positionInSet = 0;
            if (IncludeSetInformation)
            {
                sizeOfSet = child.GetSizeOfSet();
                positionInSet = child.GetPositionInSet();
            }

            // bugbug
            string currentValue = GetValueFromValuePattern(child);

            RunOnUI(() =>
            {
                _window.InsertEditControl(currentValue, positionInSet, sizeOfSet);
            });
        }

        private string GetValueFromValuePattern(IUIAutomationElement child)
        {
            if (child == null)
                return ""; // bugbug: expected?

            IUIAutomationValuePattern pattern =
                (IUIAutomationValuePattern)child.GetCachedPattern(ValuePatternId);

            if (pattern != null) // bugbug: Why does this happen in Edge?
            {
                var currentValue = pattern.CachedValue;
                return currentValue;
            }

            return "";
        }

        private void InsertRadioButton(IUIAutomationElement child)
        {
            int sizeOfSet = 0, positionInSet = 0;
            if (IncludeSetInformation)
            {
                sizeOfSet = child.GetSizeOfSet();
                positionInSet = child.GetPositionInSet();
            }

            IUIAutomationSelectionItemPattern selectionItemPattern =
                (IUIAutomationSelectionItemPattern)child.GetCachedPattern(_selectionItemPatternId);

            var isSelected = selectionItemPattern.CachedIsSelected == 1;

            RunOnUI(() =>
            {
                _window.InsertRadioButton(isSelected, positionInSet, sizeOfSet);
            });
        }


        private void InsertHyperlink(IUIAutomationElement child)
        {
            var name = child.CachedName;
            var value = GetValueFromValuePattern(child);

            int sizeOfSet = 0, positionInSet = 0;
            if (IncludeSetInformation)
            {
                sizeOfSet = child.GetSizeOfSet();
                positionInSet = child.GetPositionInSet();
            }

            RunOnUI(() =>
            {
                _window.InsertHyperlink(value, name, positionInSet, sizeOfSet);
            });
        }

        private void InsertImage(IUIAutomationElement child)
        {
            var name = child.CachedName;

            int sizeOfSet = 0, positionInSet = 0;
            if (IncludeSetInformation)
            {
                sizeOfSet = child.GetSizeOfSet();
                positionInSet = child.GetPositionInSet();
            }

            RunOnUI(() =>
            {
                _window.InsertImage(name, positionInSet, sizeOfSet);
            });
        }
        private void FinalizeParagraphStats(ParagraphInfo paragraphInfoTemp, int finalParagraphFormatCount)
        {
            RunOnUI(() =>
            {
                var paragraphDelta = Math.Round((paragraphInfoTemp.EndTime - paragraphInfoTemp.StartTime).TotalMilliseconds, 0);
                _window.NotifyEndOfParagraph(paragraphDelta, finalParagraphFormatCount);
            });
        }





    }



    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class LOGFONT
    {
        public int lfHeight = 0;
        public int lfWidth = 0;
        public int lfEscapement = 0;
        public int lfOrientation = 0;
        public int lfWeight = 0;
        public byte lfItalic = 0;
        public byte lfUnderline = 0;
        public byte lfStrikeOut = 0;
        public byte lfCharSet = 0;
        public byte lfOutPrecision = 0;
        public byte lfClipPrecision = 0;
        public byte lfQuality = 0;
        public byte lfPitchAndFamily = 0;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string lfFaceName = string.Empty;
    }


    static class UIAutomationHelpers
    {
        public static int GetSizeOfSet(this IUIAutomationElement element)
        {
            return (element as IUIAutomationElement4).CachedSizeOfSet; // bugbug
        }
        public static int GetPositionInSet(this IUIAutomationElement element)
        {
            return (element as IUIAutomationElement4).CachedPositionInSet;
        }
    }



}
