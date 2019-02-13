using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LagoVista.BACNet.Core.Logger
{
    public class DebugLogger : ILogger
    {
        public bool DebugMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void AddCustomEvent(LogLevel level, string tag, string customEvent, params KeyValuePair<string, string>[] args)
        {
            Debug.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Debug.WriteLine(level.ToString());
            Debug.WriteLine($"\tTag:    {tag}");
            Debug.WriteLine($"\tMsg:    {customEvent}");
            foreach(var arg in args)
            {
                Debug.WriteLine($"\t\t{arg.Key}:    {arg.Value}");
            }
            Debug.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
        }

        public void AddException(string tag, Exception ex, params KeyValuePair<string, string>[] args)
        {
            Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Debug.WriteLine("Exception");
            Debug.WriteLine($"\tTag:    {tag}");
            Debug.WriteLine($"\tEx:      {ex.Message}");
            foreach (var arg in args)
            {
                Debug.WriteLine($"\t\t{arg.Key}:    {arg.Value}");
            }
            Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }

        public void AddKVPs(params KeyValuePair<string, string>[] args)
        {
            throw new NotImplementedException();
        }

        public void EndTimedEvent(TimedEvent evt)
        {
            throw new NotImplementedException();
        }

        public TimedEvent StartTimedEvent(string area, string description)
        {
            throw new NotImplementedException();
        }

        public void TrackEvent(string message, Dictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }
    }
}
