using peak;
using peak.core;
using peak.core.nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Camera.IDSpeak
{
    /* Provides methods for listing all available devices. */
    public class DeviceEnumerator
    {
        /* Data class used for holding device data. */
        public class Device
        {
            public Device(uint index, DeviceDescriptor deviceInfoHandle)
            {
                this.Index = index;
                this.DeviceInfoHandle = deviceInfoHandle;
                this.Name = deviceInfoHandle.DisplayName();
                this.SerialNumber = deviceInfoHandle.SerialNumber();
                this.ModelName = deviceInfoHandle.ModelName();
                this.Key = deviceInfoHandle.Key();
            }
            public readonly uint Index;
            public readonly DeviceDescriptor DeviceInfoHandle;
            public readonly string Name; 
            public readonly string SerialNumber;
            public readonly string ModelName;
            public readonly string Key;
        }
        private static DeviceEnumerator _deviceEnumeratorInstance;
        private static List<Device> _devices;
        private static readonly object _lock = new object();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static DeviceEnumerator Instance()
        {
            lock (_lock)
            {
                if (_deviceEnumeratorInstance == null)
                {
                    _deviceEnumeratorInstance = new DeviceEnumerator();
                }
                return _deviceEnumeratorInstance;
            }
        }
        public DeviceEnumerator()
        {
            // ids_peak provides several events that you can subscribe to in order
            // to be notified when the connection status of a device changes.
            //
            // The 'found' event is triggered if a new device is found upon calling
            // `DeviceManager.Update()`
            DeviceManager.Instance().DeviceFoundEvent += DeviceManager_DeviceFoundEvent;
        }
        ~DeviceEnumerator()
        {
            lock (_lock)
            {
                _deviceEnumeratorInstance = null;
            }
        }
        /* Queries the number of available devices and creates a list with device data. */
        public List<Device> EnumerateDevices()
        {
            /* Create a list for the device data. */
            _devices = new List<Device>();

            // Update the device manager
            DeviceManager.Instance().Update(DeviceManager.UpdatePolicy.ScanEnvironmentForProducerLibraries);
            if (!DeviceManager.Instance().Devices().Any())
                return _devices;
            // Open the first openable device in the device manager's device list
            int count = DeviceManager.Instance().Devices().Count();

            /* Get device data from all devices. */
            for (int i = 0; i < count; ++i)
            {
                /* Create a new data packet. */
                Device device = new Device((uint)i, DeviceManager.Instance().Devices()[i]);

                /* Add to the list. */
                _devices.Add(device);
            }
            return _devices;
        }

        public DeviceDescriptor GetDeviceByIndex(uint index)
        {
            if (_devices == null)
                return null;
            if (index >= _devices.Count)
                return null;
            return _devices[(int)index].DeviceInfoHandle;
        }

        #region DeviceManager events
        private void DeviceManager_DeviceFoundEvent(object sender, peak.core.DeviceDescriptor foundDevice)
        {
            log.InfoFormat("Found-Device-Callback: Key={0}", foundDevice.Key());
        }
        #endregion
    }
}
