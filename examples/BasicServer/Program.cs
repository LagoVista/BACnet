using LagoVista.BACNet.Core;
using LagoVista.BACNet.Core.Logger;
using LagoVista.BACNet.Core.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BasicServer
{
    class Program
    {
        static BacnetClient bacnet_client;
        static DeviceStorage m_storage;

        /*****************************************************************************************************/
        static void Main(string[] args)
        {
            try
            {
                StartActivity();
                Console.WriteLine("Started");

                BacnetObjectId OBJECT_ANALOG_VALUE_0 = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 0);
                BacnetObjectId OBJECT_ANALOG_INPUT_0 = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 0);

                double count = 0;

                for (; ; )
                {
                    lock (m_storage)         // read and write callback are fired in a separated thread, so multiple access needs protection
                    {
                        // Read the Present Value
                        // index 0 : number of values in the array
                        // index 1 : first value
                        m_storage.ReadProperty(OBJECT_ANALOG_VALUE_0, BacnetPropertyIds.PROP_PRESENT_VALUE, 1, out IList<BacnetValue> valtoread);
                        // Get the first ... and here the only element
                        double coef = Convert.ToDouble(valtoread[0].Value);

                        float sin = (float)(coef * Math.Sin(count));
                        // Write the Present Value
                        IList<BacnetValue> valtowrite = new BacnetValue[1] { new BacnetValue(sin) };
                        m_storage.WriteProperty(OBJECT_ANALOG_INPUT_0, BacnetPropertyIds.PROP_PRESENT_VALUE, 1, valtowrite, true);
                    }
                    Thread.Sleep(1000);
                    count += 0.1;
                }
            }
            catch { }
        }

        /*****************************************************************************************************/
        static void StartActivity()
        {
            // Load the device descriptor from the embedded resource file
            // Get myId as own device id
            m_storage = DeviceStorage.Load("BasicServer.DeviceDescriptor.xml");

            var transport = new BacnetIpUdpProtocolTransport(0xBAC0, false);
            transport.Log = new ConsoleLogger();

            // Bacnet on UDP/IP/Ethernet
            bacnet_client = new BacnetClient(transport);
            bacnet_client.Log = new ConsoleLogger();

            // or Bacnet Mstp on COM4 à 38400 bps, own master id 8
            // m_bacnet_client = new BacnetClient(new BacnetMstpProtocolTransport("COM4", 38400, 8);
            // Or Bacnet Ethernet
            // bacnet_client = new BacnetClient(new BacnetEthernetProtocolTransport("Connexion au réseau local"));    
            // Or Bacnet on IPV6
            // bacnet_client = new BacnetClient(new BacnetIpV6UdpProtocolTransport(0xBAC0));

            bacnet_client.OnWhoIs += new BacnetClient.WhoIsHandler(handler_OnWhoIs);
            bacnet_client.OnIam += new BacnetClient.IamHandler(bacnet_client_OnIam);
            bacnet_client.OnReadPropertyRequest += new BacnetClient.ReadPropertyRequestHandler(handler_OnReadPropertyRequest);
            bacnet_client.OnReadPropertyMultipleRequest += new BacnetClient.ReadPropertyMultipleRequestHandler(handler_OnReadPropertyMultipleRequest);
            bacnet_client.OnWritePropertyRequest += new BacnetClient.WritePropertyRequestHandler(handler_OnWritePropertyRequest);

            bacnet_client.Start();    // go
            // Send Iam
            bacnet_client.Iam(m_storage.DeviceId, new BacnetSegmentations());

            bacnet_client.WhoIs();
        }



        static async void bacnet_client_OnIam(BacnetClient sender, BacnetAddress adr, uint device_id, uint max_apdu, BacnetSegmentations segmentation, ushort vendor_id)
        {
            if (device_id != 12345)
            {
                bacnet_client.Log.AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel.Message, "Program", $"Device Id {device_id} connected");

                var deviceObjId = new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device_id);
                var objectIdList = await sender.ReadPropertyAsync(adr, deviceObjId, BacnetPropertyIds.PROP_OBJECT_LIST);

                foreach (var objId in objectIdList)
                {
                    Console.WriteLine($"\t\t{objId}");

                    LinkedList<BacnetObjectId> object_list = new LinkedList<BacnetObjectId>();

                    if (Enum.IsDefined(typeof(BacnetObjectTypes), ((BacnetObjectId)objId.Value).Type))
                    {
                        var val = (BacnetObjectId)objId.Value;
                        object_list.AddLast(val);
                    }

                    foreach (var objectId in object_list)
                    {
                        var props = new List<BacnetPropertyIds>();

                        if (objectId.type == BacnetObjectTypes.OBJECT_DEVICE)
                        {
                            props.Add(BacnetPropertyIds.PROP_DESCRIPTION);
                            props.Add(BacnetPropertyIds.PROP_SERIAL_NUMBER);
                            props.Add(BacnetPropertyIds.PROP_MODEL_NAME);
                            props.Add(BacnetPropertyIds.PROP_APPLICATION_SOFTWARE_VERSION);
                            props.Add(BacnetPropertyIds.PROP_OUT_OF_SERVICE);
                            props.Add(BacnetPropertyIds.PROP_LOCATION);
                            props.Add(BacnetPropertyIds.PROP_OBJECT_NAME);
                            props.Add(BacnetPropertyIds.PROP_OBJECT_IDENTIFIER);
                        }
                        else
                        {
                            props.Add(BacnetPropertyIds.PROP_PRESENT_VALUE);
                            props.Add(BacnetPropertyIds.PROP_NUMBER_OF_STATES);
                            props.Add(BacnetPropertyIds.PROP_OUT_OF_SERVICE);
                            props.Add(BacnetPropertyIds.PROP_STATE_TEXT);
                            props.Add(BacnetPropertyIds.PROP_RELIABILITY);
                            props.Add(BacnetPropertyIds.PROP_DESCRIPTION);
                            props.Add(BacnetPropertyIds.PROP_UNITS);
                        }

                        var results = await sender.ReadPropertyMultipleAsync(adr, objectId, props.ToArray());

                        var description = results.Where(prop => prop.property.GetPropertyId() == BacnetPropertyIds.PROP_DESCRIPTION).FirstOrDefault();
                        Console.WriteLine($"\t\t\tProperty {description.value[0]}:");


                        foreach (var child in results)
                        {
                            if (child.property.GetPropertyId() != BacnetPropertyIds.PROP_DESCRIPTION)
                            {
                                foreach (var value in child.value)
                                {
                                    if (value.Tag != BacnetApplicationTags.BACNET_APPLICATION_TAG_ERROR)
                                    {
                                        if (child.property.GetPropertyId() == BacnetPropertyIds.PROP_UNITS)
                                        {
                                            var unitInt = int.Parse(value.Value.ToString());
                                            var unit = (BacnetUnitsId)unitInt;
                                            Console.WriteLine($"\t\t\t\t{child.property.GetPropertyId()}:  {unit}");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"\t\t\t\t{child.property.GetPropertyId()}:  {value.Value}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //ignore Iams from other devices. (Also loopbacks)
        }

        /*****************************************************************************************************/
        static void handler_OnWritePropertyRequest(BacnetClient sender, BacnetAddress adr, byte invoke_id, BacnetObjectId object_id, BacnetPropertyValue value, BacnetMaxSegments max_segments)
        {
            // only OBJECT_ANALOG_VALUE:0.PROP_PRESENT_VALUE could be write in this sample code
            if ((object_id.type != BacnetObjectTypes.OBJECT_ANALOG_VALUE) || (object_id.instance != 0) || ((BacnetPropertyIds)value.property.propertyIdentifier != BacnetPropertyIds.PROP_PRESENT_VALUE))
            {
                sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_WRITE_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_WRITE_ACCESS_DENIED);
                return;
            }

            lock (m_storage)
            {
                try
                {
                    DeviceStorage.ErrorCodes code = m_storage.WriteCommandableProperty(object_id, (BacnetPropertyIds)value.property.propertyIdentifier, value.value[0], value.priority);
                    if (code == DeviceStorage.ErrorCodes.NotForMe)
                    {
                        code = m_storage.WriteProperty(object_id, (BacnetPropertyIds)value.property.propertyIdentifier, value.property.propertyArrayIndex, value.value);
                    }

                    if (code == DeviceStorage.ErrorCodes.Good)
                    {
                        sender.SimpleAckResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_WRITE_PROPERTY, invoke_id);
                    }
                    else
                    {
                        sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_WRITE_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                    }
                }
                catch (Exception)
                {
                    sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_WRITE_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
            }
        }
        /*****************************************************************************************************/
        static void handler_OnWhoIs(BacnetClient sender, BacnetAddress adr, int low_limit, int high_limit)
        {
            if (low_limit != -1 && m_storage.DeviceId < low_limit)
            {
                return;
            }
            else if (high_limit != -1 && m_storage.DeviceId > high_limit)
            {
                return;
            }

            sender.Iam(m_storage.DeviceId, new BacnetSegmentations());
        }

        /*****************************************************************************************************/
        static void handler_OnReadPropertyRequest(BacnetClient sender, BacnetAddress adr, byte invoke_id, BacnetObjectId object_id, BacnetPropertyReference property, BacnetMaxSegments max_segments)
        {
            lock (m_storage)
            {
                try
                {
                    DeviceStorage.ErrorCodes code = m_storage.ReadProperty(object_id, (BacnetPropertyIds)property.propertyIdentifier, property.propertyArrayIndex, out IList<BacnetValue> value);
                    if (code == DeviceStorage.ErrorCodes.Good)
                    {
                        sender.ReadPropertyResponse(adr, invoke_id, sender.GetSegmentBuffer(max_segments), object_id, property, value);
                    }
                    else
                    {
                        sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                    }
                }
                catch (Exception)
                {
                    sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
            }
        }

        /*****************************************************************************************************/
        static void handler_OnReadPropertyMultipleRequest(BacnetClient sender, BacnetAddress adr, byte invoke_id, IList<BacnetReadAccessSpecification> properties, BacnetMaxSegments max_segments)
        {
            lock (m_storage)
            {
                try
                {
                    IList<BacnetPropertyValue> value;
                    List<BacnetReadAccessResult> values = new List<BacnetReadAccessResult>();
                    foreach (BacnetReadAccessSpecification p in properties)
                    {
                        if (p.propertyReferences.Count == 1 && p.propertyReferences[0].propertyIdentifier == (uint)BacnetPropertyIds.PROP_ALL)
                        {
                            if (!m_storage.ReadPropertyAll(p.objectIdentifier, out value))
                            {
                                sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROP_MULTIPLE, invoke_id, BacnetErrorClasses.ERROR_CLASS_OBJECT, BacnetErrorCodes.ERROR_CODE_UNKNOWN_OBJECT);
                                return;
                            }
                        }
                        else
                        {
                            m_storage.ReadPropertyMultiple(p.objectIdentifier, p.propertyReferences, out value);
                        }

                        values.Add(new BacnetReadAccessResult(p.objectIdentifier, value));
                    }

                    sender.ReadPropertyMultipleResponse(adr, invoke_id, sender.GetSegmentBuffer(max_segments), values);

                }
                catch (Exception)
                {
                    sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROP_MULTIPLE, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
            }
        }
    }
}
