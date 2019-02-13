using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.BACNet.Core.Logger
{
    public class ConsoleLogger : ILogger
    {
        public bool DebugMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void AddCustomEvent(LogLevel level, string tag, string customEvent, params KeyValuePair<string, string>[] args)
        {

            switch(level)
            {
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Message:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }

            Console.WriteLine(level.ToString());
            Console.WriteLine($"\tTag:    {tag}");
            Console.WriteLine($"\tMsg:    {customEvent}");
            foreach(var arg in args)
            {
                Console.WriteLine($"\t\t{arg.Key}:    {arg.Value}");
            }
            Console.ResetColor();
        }

        public void AddException(string tag, Exception ex, params KeyValuePair<string, string>[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Exception");
            Console.WriteLine($"\tTag:    {tag}");
            Console.WriteLine($"\tEx:      {ex.Message}");
            foreach (var arg in args)
            {
                Console.WriteLine($"\t\t{arg.Key}:    {arg.Value}");
            }
            Console.ResetColor();
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
