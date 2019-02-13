﻿namespace LagoVista.BACNet.Core
{
    public enum BacnetBvlcResults : ushort
    {
        BVLC_RESULT_SUCCESSFUL_COMPLETION = 0x0000,
        BVLC_RESULT_WRITE_BROADCAST_DISTRIBUTION_TABLE_NAK = 0x0010,
        BVLC_RESULT_READ_BROADCAST_DISTRIBUTION_TABLE_NAK = 0x0020,
        BVLC_RESULT_REGISTER_FOREIGN_DEVICE_NAK = 0X0030,
        BVLC_RESULT_READ_FOREIGN_DEVICE_TABLE_NAK = 0x0040,
        BVLC_RESULT_DELETE_FOREIGN_DEVICE_TABLE_ENTRY_NAK = 0x0050,
        BVLC_RESULT_DISTRIBUTE_BROADCAST_TO_NETWORK_NAK = 0x0060
    }
}