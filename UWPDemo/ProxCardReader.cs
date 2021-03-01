using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace UWPDemo
{
    public class ProxCardReader
    {
        public UInt16 Vid { get; set; } = 0x1A86;
        public UInt16 Pid { get; set; } = 0x7523;
        public DateTime LastReceived { get; private set; }

        #region EventHandlers
        public delegate void CardReceivedEvent(object sender, CardDataEventArgs e);
        public event CardReceivedEvent CardReceived = null;
        protected virtual void OnCardReceived(CardDataEventArgs args) => CardReceived?.Invoke(this, args);
        #endregion

        #region DeviceWatcher
        private DeviceWatcher watcher = null;
        public void InitWatcher()
        {
            string devSelector = SerialDevice.GetDeviceSelectorFromUsbVidPid(Vid, Pid);
            watcher = DeviceInformation.CreateWatcher(devSelector);
            watcher.Added += Watcher_Added;
            watcher.Removed += Watcher_Removed;
            watcher.EnumerationCompleted += Watcher_EnumerationCompleted;
            watcher.Start();

            InitTimer();
        }
        private async void Watcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            //throw new NotImplementedException();
        }

        private async void Watcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            try
            {
                devInfo = args;
                device = await SerialDevice.FromIdAsync(devInfo.Id);

                if (device != null)
                    InitDevice();

            }
            catch (Exception ex)
            {

            }

        }
        private async void Watcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            //throw new NotImplementedException();
        }





        #endregion

        #region Device
        private DeviceInformation devInfo = null;
        private SerialDevice device = null;
        public Boolean IsDeviceConnected => (device != null);

        public void InitDevice()
        {
            try
            {
                if (IsDeviceConnected)
                {
                    device.BaudRate = 9600;
                    device.Parity = SerialParity.None;
                    device.StopBits = SerialStopBitCount.One;
                    device.Handshake = SerialHandshake.None;
                    device.DataBits = 8;
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region DataReader
        private DataReader reader = null;
        private List<byte> InputBuffer = null;
        private void InitReader()
        {
            try
            {
                reader = new DataReader(device.InputStream);
                reader.InputStreamOptions = InputStreamOptions.Partial;
            }
            catch (Exception ex)
            {

            }
        }

        private bool IsReading = false;

        private async void ReadData()
        {
            try
            {
                if (device == null)
                    return;

                if (reader == null)
                    InitReader();

                if (IsReading)
                    return;

                if (InputBuffer == null)
                    InputBuffer = new List<byte>();

                IsReading = true;

                reader.InputStreamOptions = InputStreamOptions.Partial;

                //reader.loa
                uint bitCount = await reader.LoadAsync(1); //.AsTask();
                LastReceived = DateTime.Now;

                while (true)
                {
                    byte b = reader.ReadByte();
                    if (b == 6)
                    {
                        // Do Nothing - Unit is jusr responding to a keep alive request.
                    }
                    else if (b == 2)
                    {
                        InputBuffer.Clear();
                        bool ContinueRead = true;
                        while (ContinueRead)
                        {
                            bitCount = await reader.LoadAsync(1); //.AsTask();
                            b = reader.ReadByte();

                            if (System.Diagnostics.Debugger.IsAttached)
                                System.Diagnostics.Debug.Write(b);

                            if (b != 3)
                                InputBuffer.Add(b);
                            else
                                ContinueRead = false;
                        }                       
                        string result = System.Text.Encoding.UTF8.GetString(InputBuffer.ToArray());
                        CardDataEventArgs args = new CardDataEventArgs(result);
                        OnCardReceived(args);
                    }
                    else
                    {
                        bitCount = await reader.LoadAsync(1); //.AsTask();

                    }

                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                IsReading = false;
            }
        }
        #endregion

        #region Timer
        private DispatcherTimer readtimer = null;
        private void InitTimer()
        {
            try
            {
                readtimer = new DispatcherTimer();
                readtimer.Tick += ReadTimer_Tick;
                readtimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                readtimer.Start();
            }
            catch (Exception ex)
            {

            }
        }
        private void ReadTimer_Tick(object sender, object e)
        {
            try
            {
                ReadData();
            }
            catch (Exception ex)
            {

            }
        }       
        #endregion



    }
}
