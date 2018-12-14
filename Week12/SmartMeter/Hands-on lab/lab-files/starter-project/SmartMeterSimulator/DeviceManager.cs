using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client.Exceptions;

namespace SmartMeterSimulator
{
    class DeviceManager
    {

        static string connectionString;
        static RegistryManager registryManager;

        public static string HostName { get; set; }

        public static void IotHubConnect(string cnString)
        {
            connectionString = cnString;

            //xTODO: 1.Create an instance of RegistryManager from connectionString
            registryManager = RegistryManager.CreateFromConnectionString(connectionString); ;

            var builder = IotHubConnectionStringBuilder.Create(cnString);

            HostName = builder.HostName;
        }
        
        /// <summary>
        /// Register a single device with the IoT hub. The device is initially registered in a
        /// disabled state.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async static Task<string> RegisterDevicesAsync(string connectionString, string deviceId)
        {
            //Make sure we're connected
            if (registryManager == null)
                IotHubConnect(connectionString);


            //xTODO: 2.Create new device
            Device device = new Device(deviceId);

            //xTODO: 3.Initialize device with a status of Disabled 
            //Enabled in a subsequent step
            device.Status = DeviceStatus.Disabled;

            try
            {
                //xTODO: 4.Register the new device
                device = await registryManager.AddDeviceAsync(device);
            }
            catch (Exception ex)
            {
                if (ex is DeviceAlreadyExistsException ||
                    ex.Message.Contains("DeviceAlreadyExists"))
                {
                    //xTODO: 5.Device already exists, get the registered device
                    device = await registryManager.GetDeviceAsync(deviceId);

                    //xTODO: 6.Ensure the device is disabled until Activated later
                    device.Status = DeviceStatus.Disabled;

                    //xTODO: 7.Update IoT Hubs with the device status change
                    await registryManager.UpdateDeviceAsync(device);
                }
                else
                {
                    MessageBox.Show($"An error occurred while registering one or more devices:\r\n{ex.Message}");
                }
            }

            //return the device key
            return device.Authentication.SymmetricKey.PrimaryKey;
        }

        /// <summary>
        /// Activate an already registered device by changing its status to Enabled.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="deviceId"></param>
        /// <param name="deviceKey"></param>
        /// <returns></returns>
        public async static Task<bool> ActivateDeviceAsync(string connectionString, string deviceId, string deviceKey)
        {
            //Server-side management function to enable the provisioned device 
            //to connect to IoT Hub after it has been installed locally. 
            //If device id device key are valid, Activate (enable) the device.

            //Make sure we're connected
            if (registryManager == null)
                IotHubConnect(connectionString);


            bool success = false;
            Device device = null;

            try
            {
                //xTODO: 8.Fetch the device
                device = await registryManager.GetDeviceAsync(deviceId);

                //xTODO: 9.Verify the device keys match
                if (deviceKey == device.Authentication.SymmetricKey.PrimaryKey)
                {
                    //xTODO: 10.Enable the device
                    device.Status = DeviceStatus.Enabled;

                    //xTODO: 11.Update IoT Hubs
                    await registryManager.UpdateDeviceAsync(device);

                    success = true;
                }
            }
            catch(Exception)
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Deactivate a single device in the IoT Hub registry.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async static Task<bool> DeactivateDeviceAsync(string connectionString, string deviceId)
        {
            //Make sure we're connected
            if (registryManager == null)
                IotHubConnect(connectionString);

            bool success = false;
            Device device;

            try
            {
                //xTODO: 12.Lookup the device from the registry by deviceId
                device = await registryManager.GetDeviceAsync(deviceId);

                //xTODO: 13.Disable the device
                device.Status = DeviceStatus.Disabled;

                //xTODO: 14.Update the registry 
                await registryManager.UpdateDeviceAsync(device);

                success = true;
            }
            catch (Exception)
            {
                success = false;
            }

            return success;

        }

        /// <summary>
        /// Unregister a single device from the IoT Hub Registry
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async static Task UnregisterDevicesAsync(string connectionString, string deviceId)
        {
            //Make sure we're connected
            if (registryManager == null)
                IotHubConnect(connectionString);

            //xTODO: 15.Remove the device from the Registry
            await registryManager.RemoveDeviceAsync(deviceId);
        }

        /// <summary>
        /// Unregister all the devices managed by the Smart Meter Simulator
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async static Task UnregisterAllDevicesAsync(string connectionString)
        {
            //Make sure we're connected
            if (registryManager == null)
                IotHubConnect(connectionString);

            for(int i = 0; i <= 9; i++)
            {
                string deviceId = "Device" + i.ToString();

                //xTODO: 16.Remove the device from the Registry
                await registryManager.RemoveDeviceAsync(deviceId);
            }

        }
    }
}
