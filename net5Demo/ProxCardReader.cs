using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;


namespace net5Demo
{
    public class ProxCardReader
    {
        public UInt16 Vid { get; set; } = 0x1A86;
        public UInt16 Pid { get; set; } = 0x7523;
        public int DeviceCount {get; private set; } = 0;
        public string ComPort { get; private set; } = string.Empty;
        public string DeviceId { get; private set; } = string.Empty;
        public string DeviceName { get; private set; } = string.Empty;
        public DateTime LastReceived { get; private set; }

        #region EventHandlers
        public delegate void CardReceivedEvent(object sender, CardDataEventArgs e);
        public event CardReceivedEvent CardReceived = null;
        protected virtual void OnCardReceived(CardDataEventArgs args) => CardReceived?.Invoke(this, args);
        #endregion
        #region watcher
        private System.Threading.Timer watcherTimer;
        public void InitWatcher() => InitWatcher(new TimeSpan(0,0,5));
        public void InitWatcher(TimeSpan interval)
        {
            TimerCallback callback = new TimerCallback(Search);
            watcherTimer = new Timer(callback, null, new TimeSpan(0,0,0), interval);
        }
        private void Search(Object o)
        {           
            ManagementScope connectionScope = new ManagementScope();
            SelectQuery mosQuery = new SelectQuery($@"SELECT * FROM Win32_PnPEntity where DeviceID like '%VID_{Vid.ToString("X4")}&PID_{Pid.ToString("X4")}%'");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, mosQuery);
            ManagementObjectCollection results = searcher.Get();
            ManagementObjectCollection.ManagementObjectEnumerator enumerator = results.GetEnumerator();

            DeviceCount = results.Count;
            if (DeviceCount > 0)
            {
                enumerator.MoveNext();
                InitDevice((ManagementObject)enumerator.Current);
            }

            if (DeviceCount == 0 && !String.IsNullOrWhiteSpace(DeviceId))
                Disconnect();

        }
        private void InitDevice(ManagementObject device)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(DeviceId))
                    return;

                if (DeviceId != device["DeviceID"].ToString())
                    Disconnect();

                DeviceId = device["DeviceID"].ToString();
                DeviceName = device["Name"].ToString();                
                /* Extract the ComPort using a regular expression
                \(          # Escaped parenthesis, means "starts with a '(' character"
                    (       # Parentheses in a regex mean "put (capture) the stuff in between into the Groups array
                    [^)]    # Any character that is not a ')' character
                    *       # Zero or more occurrences of the aforementioned "non ')' char"
                    )       # Close the capturing group
                \)          # "Ends with a ')' character"
                */
                ComPort = Regex.Match(DeviceName, @"\(([^)]*)\)", RegexOptions.IgnoreCase).Groups[1].Value;                
                InitReader();                
            }
            catch(System.Exception ex)
            {
                DeviceId = string.Empty;
                DeviceName = string.Empty;
            }
        }
        #endregion
        #region Device
        private System.IO.Ports.SerialPort serialPort;
        private bool IsReading = false;
        private List<byte> InputBuffer = new List<byte>();

        
               
        private void InitReader()
        {
            try
            {
                serialPort = new System.IO.Ports.SerialPort();
                serialPort.PortName = ComPort;
                
                serialPort.BaudRate = 9600;
                serialPort.Parity = System.IO.Ports.Parity.None;
                serialPort.StopBits = System.IO.Ports.StopBits.One;
                serialPort.Handshake = System.IO.Ports.Handshake.None; 
                serialPort.DataBits = 8;        
                serialPort.Open();

                ReadData();
            }
            catch (Exception ex)
            {

            }
        }
        private void Disconnect()
        {
            try
            {
                DeviceId = string.Empty;
                DeviceName = string.Empty;
                ComPort = string.Empty;
                
                serialPort.Dispose();
            }
            catch(Exception)
            {
                
            }
        }
        private async void ReadData()
        {
            try
            {
                if (serialPort == null)
                    return;

                if (!serialPort.IsOpen)
                    throw new Exception($"serialPort {ComPort} Not Open!");
                
                if (IsReading)
                    return;

                IsReading = true;

                while (true)
                {
                    int b = serialPort.ReadByte();
                    if (b == 6) // Do Nothing - Unit is jusr responding to a keep alive request.
                        continue;                  
                    else if (b == 2)
                    {
                        InputBuffer.Clear();
                        bool ContinueRead = true;
                        while (ContinueRead)
                        {
                            //bitCount = await reader.LoadAsync(1); //.AsTask();
                            b = serialPort.ReadByte();

                            if (System.Diagnostics.Debugger.IsAttached)
                                System.Diagnostics.Debug.Write(b);

                            if (b != 3)
                                InputBuffer.Add((byte)b);
                            else
                                ContinueRead = false;
                        }                       
                        string result = System.Text.Encoding.UTF8.GetString(InputBuffer.ToArray());
                        CardDataEventArgs args = new CardDataEventArgs(result);
                        OnCardReceived(args);
                    }
                }
            }
            catch (Exception ex)
            {
                Disconnect();
            }
            finally
            {
                IsReading = false;
            }
        }

        #endregion
        /*


using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
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


*/
    }
}
