using Interop.UIautomationCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UiaViewer
{
    public class AutomationItem : INotifyPropertyChanged, IUIAutomationPropertyChangedEventHandler
    {
        IUIAutomationElement _element;
        public AutomationItem(IUIAutomationElement element)
        {
            _element = element;
            Name = _element.CurrentName;

            UiaReader.Automation.AddPropertyChangedEventHandler(
                _element,
                TreeScope.TreeScope_Element,
                null, // bugbug: cacheRequest 
                this,
                new int[] { UiaReader.NamePropertyId });

        }

        public IUIAutomationElement Element { get { return _element; } }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
                return Name;

            return base.ToString();
        }

        string _name = null;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();

                if (NameChanged != null)
                {
                    MainWindow.Instance.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        NameChanged(this, null);
                    }));
                }
            }
        }

        public event EventHandler<EventArgs> NameChanged;

        public int ControlType { get; internal set; }

        IUIAutomationInvokePattern _invokePattern = null;

        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        Thread _invokeThread = null;
        public bool TryInvoke()
        {
            if (_invokePattern == null)
            {
                var cacheRequest = UiaReader.Automation.CreateCacheRequest();
                cacheRequest.TreeScope = TreeScope.TreeScope_Element;
                cacheRequest.AddPattern(UiaReader.InvokePatternId);
                //cacheRequest.AddProperty(_controlTypePropertyId);

                var element = _element.BuildUpdatedCache(cacheRequest);
                _invokePattern = element.GetCachedPattern(UiaReader.InvokePatternId);

                _invokeThread = Thread.CurrentThread;
            }

            if (_invokePattern != null)
            {
                _invokePattern.Invoke();
                return true;
            }

            return false;
        }
        public void HandlePropertyChangedEvent(IUIAutomationElement sender, int propertyId, object newValue)
        {
            Name = (string)newValue;
        }

    }

}
