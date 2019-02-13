using System.Collections.Generic;

namespace LagoVista.BACNet.Core
{
    public struct BacnetObjectDescription
    {
        public BacnetObjectTypes typeId;
        public List<BacnetPropertyIds> propsId;
    }
}