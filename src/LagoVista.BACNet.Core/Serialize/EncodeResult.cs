using System;

namespace LagoVista.BACNet.Core.Serialize
{
    [Flags]
    public enum EncodeResult
    {
        Good = 0,
        NotEnoughBuffer = 1
    }
}