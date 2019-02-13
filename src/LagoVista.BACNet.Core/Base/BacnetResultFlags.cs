using System;

namespace LagoVista.BACNet.Core
{
    [Flags]
    public enum BacnetResultFlags
    {
        NONE = 0,
        FIRST_ITEM = 1,
        LAST_ITEM = 2,
        MORE_ITEMS = 4,
    }
}