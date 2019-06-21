using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fttd
{
    internal class Device
    {
        public Device()
        {
        }

        public Device(string deviceName, string deviceIndex, string deviceDeveloper, string dataAdd)
        {
            DeviceName = deviceName ?? throw new ArgumentNullException(nameof(deviceName));
            DeviceIndex = deviceIndex ?? throw new ArgumentNullException(nameof(deviceIndex));
            DeviceDeveloper = deviceDeveloper ?? throw new ArgumentNullException(nameof(deviceDeveloper));
            DataAdd = dataAdd ?? throw new ArgumentNullException(nameof(dataAdd));
        }

        public string DeviceName { get; set; }
        public string DeviceIndex { get; set; }
        public string DeviceDeveloper { get; set; }
        public string DataAdd { get; set; }

        public override string ToString()
        {
            string description = "Приспособление: " + DeviceName + "\nИндекс: " + DeviceIndex + "\nРазработал: " + DeviceDeveloper + "\nДата добавления: " + DataAdd;
            return description;
        }
    }
}
