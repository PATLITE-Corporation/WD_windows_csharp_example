using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace WDR_Sample_CSharp
{
    class Program
    {
        private static Socket sock = null;
        private static int WSAETIMEDOUT = 10060;

        // Product category
        public static ushort WDR_PRODUCT_ID = 0x5842;

        // identifier
        private static readonly byte WDR_COMMAND = 0x01;

        // Expansion
        private static readonly byte WDR_EXPANSION = 0x00;

        // Command type
        private static readonly byte WDR_COMMAND_KIND_NOTICE = 0x10;                        // Notification command
        private static readonly byte WDR_COMMAND_KIND_REQUEST = 0x20;                       // Request command
        private static readonly byte WDR_COMMAND_KIND_RESPONSE = 0x30;                      // Response command

        // Command mode
        private static ushort WDR_COMMAND_MODE_STATUS_CHANGE_NOTICE = 0x2001;               // Transmitter status change notification
        private static ushort WDR_COMMAND_MODE_COUNT_NOTICE = 0x2007;                       // Count value notification
        private static ushort WDR_COMMAND_MODE_SIGNAL_LIGHT_CHANGE_NOTICE = 0x2008;         // Signal light display change notification
        private static ushort WDR_COMMAND_MODE_TRANSMITTER_STATUS_REQUEST = 0x2002;         // Transmitter status acquisition
        private static ushort WDR_COMMAND_MODE_TRANSMITTER_LIST_REQUEST = 0x2003;           // Get transmitter list
        private static ushort WDR_COMMAND_MODE_TRANSMITTER_DATA_REQUEST = 0x2004;           // Transmitter information acquisition
        private static ushort WDR_COMMAND_MODE_TRANSMITTER_CALL_REQUEST = 0x4010;           // Transmitter call display
        private static ushort WDR_COMMAND_MODE_SERIAL_OUTPUT_REQUEST = 0x9011;              // Serial data output
        private static ushort WDR_COMMAND_MODE_SIGNAL_LIGHT_CONTROL_REQUEST = 0xE001;       // Signal light display control request
        private static ushort WDR_COMMAND_MODE_SIGNAL_LIGHT_CONTROL_RESPONSE = 0xE000;      // Signal light display control response
        private static ushort WDR_COMMAND_MODE_SIGNAL_LIGHT_LIFT_REQUEST = 0xE0FF;          // Signal light display cancellation
        private static ushort WDR_COMMAND_MODE_SIGNAL_LIGHT_COUNT_SET_REQUEST = 0x6001;     // Count value registration
        private static ushort WDR_COMMAND_MODE_RECEIVER_DATA_REQUEST = 0x2005;              // Receiver information acquisition
        private static ushort WDR_COMMAND_MODE_RECEIVER_RESET_REQUEST = 0x2006;             // Receiver reset
        private static ushort WDR_COMMAND_MODE_SIGNAL_LIGHT_COUNT_GET_REQUEST = 0x2009;     // Get count value
        private static ushort WDR_COMMAND_MODE_SIGNAL_LIGHT_DATA_GET_REQUEST = 0x200A;      // Get signal light display

        // Receive timeout time by command mode
        private static int WDR_STATUS_CHANGE_NOTICE_TIMEOUT = 60 * 1000;                    // For transmitter status change notification
        private static int WDR_COUNT_NOTICE_TIMEOUT = 60 * 1000;                            // Count value notification
        private static int WDR_SIGNAL_LIGHT_CHANGE_NOTICE_TIMEOUT = 60 * 1000;              // Signal light display change notification
        private static int WDR_TRANSMITTER_STATUS_REQUEST_TIMEOUT = 2 * 1000;               // Transmitter status acquisition
        private static int WDR_TRANSMITTER_LIST_REQUEST_TIMEOUT = 2 * 1000;                 // Get transmitter list
        private static int WDR_TRANSMITTER_DATA_REQUEST_TIMEOUT = 2 * 1000;                 // Transmitter information acquisition
        private static int WDR_TRANSMITTER_CALL_REQUEST_TIMEOUT = 15 * 1000;                // Transmitter call display
        private static int WDR_SERIAL_OUTPUT_REQUEST_TIMEOUT = 15 * 1000;                   // Serial data output
        private static int WDR_SIGNAL_LIGHT_CONTROL_TIMEOUT = 15 * 1000;                    // For signal light display control
        private static int WDR_SIGNAL_LIGHT_LIFT_TIMEOUT = 15 * 1000;                       // Signal light display cancellation
        private static int WDR_SIGNAL_LIGHT_COUNT_SET_TIMEOUT = 2 * 1000;                   // Count value registration
        private static int WDR_RECEIVER_DATA_REQUEST_TIMEOUT = 2 * 1000;                    // Receiver information acquisition
        private static int WDR_RECEIVER_RESET_REQUEST_TIMEOUT = 2 * 1000;                   // Receiver reset
        private static int WDR_SIGNAL_LIGHT_COUNT_GET_TIMEOUT = 2 * 1000;                   // Get count value
        private static int WDR_SIGNAL_LIGHT_DATA_GET_TIMEOUT = 2 * 1000;                    // Get signal light display

        // PNS command response data
        private static readonly byte PNS_ACK = 0x06;     // Normal response
        private static readonly byte PNS_NAK = 0x15;     // Abnormal response

        // Version structure
        public class WDR_VERSION_DATA
        {
            // Major version
            public byte major = 0;

            // Minor version
            public byte minor = 0;
        };

        // WDT version information structure
        public class WDR_WDT_VERSION_DATA
        {
            // Major version
            public byte major = 0;

            // Minor version
            public byte minor = 0;

            // Dummy data
            public byte dummy = 0;
        };

        // WDT information structure
        public class WDR_INFO_DATA
        {
            // version information
            public WDR_WDT_VERSION_DATA version = null;

            // Status information
            public byte status = 0;
        };

        // Base unit information structure
        public class WDR_BASEUNIT_DATA
        {
            // Unit type
            public byte format = 0;

            // version information
            public WDR_WDT_VERSION_DATA version = null;

            // DIP switch information
            public byte dipSwitch = 0;
        };

        // WDT state information structure
        public class WDR_WDT_STATUS_DATA
        {
            // WDT IEEE address
            public ulong IEEEAddress = 0;

            // WDT registration status
            public byte registration = 0;

            // WDT connection status
            public byte connect = 0;
        };

        // RS232C data information structure
        public class WDR_RS232C_DATA
        {
            // Input information size
            public byte size = 0;

            // serial number
            public byte serialNumber = 0;

            // Input information
            public byte[] data = new byte [60];
        };

        // Transmitter state change notification structure
        public class WDR_STATUS_CHANGE_NOTICE_RECV_DATA
        {
            // IEEE address
            public ulong IEEEAddress = 0;

            // serial number
            public uint serialNumber = 0;

            // Time information
            public ulong time = 0;

            // version information
            public WDR_VERSION_DATA version = null;

            // action mode
            public byte actionMode = 0;

            // WDT information
            public WDR_INFO_DATA wdtData = null;

            // Base unit information
            public WDR_BASEUNIT_DATA baseUnitData = null;

            // Signal light information (red)
            public byte redUnit = 0;

            // Signal light information (yellow)
            public byte yellowUnit = 0;

            // Signal light information (green)
            public byte greenUnit = 0;

            // Signal light information (blue)
            public byte blueUnit = 0;

            // Signal light information (white)
            public byte whiteUnit = 0;

            // Buzzer information
            public byte buzzerUnit = 0;

            // WDT monitoring information
            public byte surveillance = 0;

            // External input information
            public byte externalInput = 0;

            // RS232C data
            public WDR_RS232C_DATA RS232CData = null;
        };

        // Count value notification structure
        public class WDR_COUNT_NOTICE_RECV_DATA
        {
            // IEEE address
            public ulong IEEEAddress = 0;

            // Time information
            public ulong time = 0;

            // version information
            public WDR_VERSION_DATA version = null;

            // action mode
            public byte actionMode = 0;

            // WDT information
            public WDR_INFO_DATA wdtData = null;

            // Base unit information
            public WDR_BASEUNIT_DATA baseUnitData = null;

            // Count value
            public uint countValue = 0;

        };

        // Signal light display change notification structure
        public class WDR_SIGNAL_LIGHT_CHANGE_NOTICE_RECV_DATA
        {
            // IEEE address
            public ulong IEEEAddress = 0;

            // Time information
            public ulong time = 0;

            // version information
            public WDR_VERSION_DATA version = null;

            // action mode
            public byte actionMode = 0;

            // WDT information
            public WDR_INFO_DATA wdtData = null;

            // Base unit information
            public WDR_BASEUNIT_DATA baseUnitData = null;

            // Red unit
            public byte redUnit = 0;

            // Yellow unit
            public byte yellowUnit = 0;

            // Green unit
            public byte greenUnit = 0;

            // Blue unit
            public byte blueUnit = 0;

            // White unit
            public byte whiteUnit = 0;

            // Buzzer unit
            public byte buzzerUnit = 0;
        };

        // Transmitter state acquisition structure
        public class WDR_TRANSMITTER_STATUS_REQUEST_RES_DATA
        {
            // Response status
            public byte controlState = 0;

            // Time information
            public ulong time = 0;

            // version information
            public WDR_VERSION_DATA version = null;

            // action mode
            public byte actionMode = 0;

            // WDT information
            public WDR_INFO_DATA wdtData = null;

            // Base unit information
            public WDR_BASEUNIT_DATA baseUnitData = null;

            // Signal light information (red)
            public byte redUnit = 0;

            // Signal light information (yellow)
            public byte yellowUnit = 0;

            // Signal light information (green)
            public byte greenUnit = 0;

            // Signal light information (blue)
            public byte blueUnit = 0;

            // Signal light information (white)
            public byte whiteUnit = 0;

            // Buzzer information
            public byte buzzerUnit = 0;

            // WDT monitoring information
            public byte surveillance = 0;

            // External input information
            public byte externalInput = 0;

            // RS232C data
            public WDR_RS232C_DATA RS232CData = null;
        };

        // Transmitter list acquisition structure
        public class WDR_TRANSMITTER_LIST_REQUEST_RES_DATA
        {
            // Response status
            public byte controlState = 0;

            // Number of acquisitions
            public byte unitCount = 0;

            // WDT status information
            public WDR_WDT_STATUS_DATA[] wdtStatus = new WDR_WDT_STATUS_DATA[70];

        };

        // Transmitter information acquisition structure
        public class WDR_TRANSMITTER_DATA_REQUEST_RES_DATA
        {
            // Response status
            public byte controlState = 0;

            // Username
            public byte[] userName = new byte[121];

            // version information
            public WDR_VERSION_DATA version = null;

            // action mode
            public byte actionMode = 0;

            // WDT information
            public WDR_INFO_DATA wdtData = null;

            // Base unit information
            public WDR_BASEUNIT_DATA baseUnitData = null;

            // ExtendedPanID
            public ulong extendedPanID = 0;

            // Frequency channel
            public uint frequencyChannel = 0;

            // Signal light input judgment
            public byte signalLightInputJudge = 0;

            // Power settings
            public byte powerSetting = 0;

            // Counter setting
            public byte counterSetting = 0;

            // Send mode
            public ushort sendMode = 0;

        };

        // Transmitter information acquisition extended structure
        public class WDR_TRANSMITTER_DATA_REQUEST_RES_ADD_DATA
        {
            // Input information transmission method
            public byte inputDataTranform = 0;

            // Signal light format
            public byte signalLightFormat = 0;

            // Periodic transmission
            public byte regularSend = 0;

            // Simultaneous input judgment sensitivity setting
            public byte concInputSensitiveSetting = 0;

            // Received data file format
            public byte recvDataFileFormat = 0;

            // Communication setting baud rate
            public byte baudrate = 0;

            // Communication setting data length
            public byte dataLength = 0;

            // Communication setting parity
            public byte parity = 0;

            // Communication setting stop bit
            public byte stopBit = 0;
        };

        // Transmitter call display structure
        public class WDR_TRANSMITTER_CALL_REQUEST_RES_DATA
        {
            // Response status
            public byte controlState = 0;
        };

        // Serial data output request structure
        public class WDR_SERIAL_OUTPUT_REQ_DATA
        {
            // IEEE address
            public ulong IEEEAddress = 0;

            // serial number
            public byte serialNumber = 0;

            // Output information
            public List<byte> outputData = new List<byte>();
        };

        // Serial data output response structure
        public class WDR_SERIAL_OUTPUT_RES_DATA
        {
            // Response status
            public byte controlState = 0;
        };

        // Signal light display control request structure
        public class WDR_SIGNAL_LIGHT_CONTROL_REQ_DATA
        {
            // IEEE address
            public ulong IEEEAddress = 0;

            // Control time
            public byte controlTime = 0;

            // Red unit
            public byte redUnit = 0;

            // Yellow unit
            public byte yellowUnit = 0;

            // Green unit
            public byte greenUnit = 0;

            // Blue unit
            public byte blueUnit = 0;

            // White unit
            public byte whiteUnit = 0;

            // Buzzer unit
            public byte buzzerUnit = 0;
        };

        // Signal light display control response structure
        public class WDR_SIGNAL_LIGHT_CONTROL_RES_DATA
        {
            // Response status
            public byte recvState = 0;

            // Control state
            public byte controlState = 0;

            // Red unit
            public byte redUnit = 0;

            // Yellow unit
            public byte yellowUnit = 0;

            // Green unit
            public byte greenUnit = 0;

            // Blue unit
            public byte blueUnit = 0;

            // White unit
            public byte whiteUnit = 0;

            // Buzzer unit
            public byte buzzerUnit = 0;
        };

        // Signal light display cancellation structure
        public class WDR_SIGNAL_LIGHT_LIFT_RES_DATA
        {
            // Response status
            public byte recvState = 0;

            // Control state
            public byte controlState = 0;

            // Red unit
            public byte redUnit = 0;

            // Yellow unit
            public byte yellowUnit = 0;

            // Green unit
            public byte greenUnit = 0;

            // Blue unit
            public byte blueUnit = 0;

            // White unit
            public byte whiteUnit = 0;

            // Buzzer unit
            public byte buzzerUnit = 0;
        };

        // Count value registration request structure
        public class WDR_SIGNAL_LIGHT_COUNT_SET_REQ_DATA
        {
            // IEEE address
            public ulong IEEEAddress = 0;

            // Count registration value
            public uint setCount = 0;
        };

        // Count value registration response structure
        public class WDR_SIGNAL_LIGHT_COUNT_SET_RES_DATA
        {
            // Response status
            public byte controlState = 0;
        };

        // Receiver information acquisition structure
        public class WDR_RECEIVER_DATA_REQUEST_RES_DATA
        {
            // Response status
            public byte controlState = 0;

            // ExtendedPanID
            public ulong extendedPanID = 0;

            // Frequency channel
            public uint frequencyChannel = 0;

            // Firmware version
            public WDR_VERSION_DATA version = null;

            // Network status
            public byte networkStatus = 0;

            // How to boot the network
            public byte networkBoot = 0;

            // Running ExtendedPanID
            public ulong actionExtendedPanID = 0;

            // Operating frequency channel
            public byte actionFrequencyChannel = 0;

        };

        // Receiver reset structure
        public class WDR_RECEIVER_RESET_RES_DATA
        {
            // Response status
            public byte controlState = 0;
        };

        // Count value acquisition structure
        public class WDR_SIGNAL_LIGHT_COUNT_GET_RES_DATA
        {
            // Response status
            public byte controlState = 0;

            // Time information
            public ulong time = 0;

            // version information
            public WDR_VERSION_DATA version = null;

            // action mode
            public byte actionMode = 0;

            // WDT information
            public WDR_INFO_DATA wdtData = null;

            // Base unit information
            public WDR_BASEUNIT_DATA baseUnitData = null;

            // Count value
            public uint count = 0;
        };

        // Signal light display acquisition structure
        public class WDR_SIGNAL_LIGHT_DATA_GET_RES_DATA
        {
            // Response status
            public byte controlState = 0;

            // Time information
            public ulong time = 0;

            // version information
            public WDR_VERSION_DATA version = null;

            // action mode
            public byte actionMode = 0;

            // WDT information
            public WDR_INFO_DATA wdtData = null;

            // Base unit information
            public WDR_BASEUNIT_DATA baseUnitData = null;

            // Red unit
            public byte redUnit = 0;

            // Yellow unit
            public byte yellowUnit = 0;

            // Green unit
            public byte greenUnit = 0;

            // Blue unit
            public byte blueUnit = 0;

            // White unit
            public byte whiteUnit = 0;

            // Buzzer unit
            public byte buzzerUnit = 0;
        };


        /// <summary>
        /// Main function
        /// </summary>
        static void Main()
        {
            int ret;
            string WDR_IPAddress ="";
            int WDR_PortNo = 0;
            string commandId = "";
            string inputData = "";

            // Obtaining an IP address
            Console.WriteLine("Enter the IP address of the receiver (example: 192.168.10.1)");
            WDR_IPAddress = Console.ReadLine();

            // Get the port number
            Console.WriteLine("Enter the receiver port number (example: 10002)");
            inputData = Console.ReadLine();
            Int32.TryParse(inputData, out WDR_PortNo);

            // Connect to WDR
            ret = SocketOpen(WDR_IPAddress, WDR_PortNo);
            if (ret == -1)
                return;

            // Get command identifier
            Console.WriteLine("Select the command to execute");
            Console.WriteLine(" 1: Transmitter status change notification");
            Console.WriteLine(" 2: Count value notification");
            Console.WriteLine(" 3: Notification of change in signal light display");
            Console.WriteLine(" 4: Transmitter status acquisition request / response");
            Console.WriteLine(" 5: Transmitter list acquisition request / response");
            Console.WriteLine(" 6: Transmitter information acquisition request / response");
            Console.WriteLine(" 7: Transmitter call display request / response");
            Console.WriteLine(" 8: Serial data output request / response");
            Console.WriteLine(" 9: Signal light display control request / response");
            Console.WriteLine("10: Request / response to cancel signal lamp display");
            Console.WriteLine("11: Count value registration request / response");
            Console.WriteLine("12: Receiver information acquisition request / response");
            Console.WriteLine("13: Receiver reset request / response");
            Console.WriteLine("14: Count value acquisition request / response");
            Console.WriteLine("15: Signal light display acquisition request / response");
            commandId = Console.ReadLine();

            switch (commandId)
            {
                case "1":
                    {
                        // Transmitter status change notification
                        WDR_STATUS_CHANGE_NOTICE_RECV_DATA Data = new WDR_STATUS_CHANGE_NOTICE_RECV_DATA();
                        ret = WDR_StatusChangeNoticeCommand(out Data);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("");
                            Console.WriteLine("Transmitter status change notification data");

                            // IEEE address
                            Console.WriteLine("IEEE address: 0x" + Data.IEEEAddress.ToString("X16"));

                            // serial number
                            Console.WriteLine("Serial number: 0x" + Data.serialNumber.ToString("X8"));

                            // Time information
                            Console.WriteLine("Time information: 0x" + Data.time.ToString("X16"));

                            // version information
                            Console.WriteLine("version information");
                            // Major version
                            Console.WriteLine(" Major version: 0x" + Data.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine(" Minor version: 0x" + Data.version.minor.ToString("X2"));

                            // action mode
                            Console.WriteLine("Operation mode: 0x" + Data.actionMode.ToString("X2"));

                            // WDT information
                            Console.WriteLine("WDT information");
                            // version information
                            Console.WriteLine(" version information");
                            // Major version
                            Console.WriteLine("  Major version: 0x" + Data.wdtData.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine("  Minor version: 0x" + Data.wdtData.version.minor.ToString("X2"));
                            // Status information
                            Console.WriteLine(" Status information: 0x" + Data.wdtData.status.ToString("X2"));

                            // Base unit information
                            Console.WriteLine("Base unit information");
                            // Unit type
                            Console.WriteLine(" Unit type: 0x" + Data.baseUnitData.format.ToString("X2"));
                            // version information
                            Console.WriteLine(" version information");
                            // Major version
                            Console.WriteLine("  Major version: 0x" + Data.baseUnitData.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine("  Minor version: 0x" + Data.baseUnitData.version.minor.ToString("X2"));
                            // DIP switch information
                            Console.WriteLine(" DIP switch information");
                            // Switch 3
                            Console.WriteLine("  Switch 3: 0b" + ((Data.baseUnitData.dipSwitch & 0x08) != 0 ? 1 : 0).ToString());
                            // Switch 2
                            Console.WriteLine("  Switch 2: 0b" + ((Data.baseUnitData.dipSwitch & 0x04) != 0 ? 1 : 0).ToString());
                            // Switch 1
                            Console.WriteLine("  Switch 1: 0b" + ((Data.baseUnitData.dipSwitch & 0x02) != 0 ? 1 : 0).ToString());
                            // Switch 0
                            Console.WriteLine("  Switch 0: 0b" + ((Data.baseUnitData.dipSwitch & 0x01) != 0 ? 1 : 0).ToString());

                            // Signal light information (red)
                            Console.WriteLine("Signal light information (red): 0x" + Data.redUnit.ToString("X2"));

                            // Signal light information (yellow)
                            Console.WriteLine("Signal light information (yellow): 0x" + Data.yellowUnit.ToString("X2"));

                            // Signal light information (green)
                            Console.WriteLine("Signal light information (green): 0x" + Data.greenUnit.ToString("X2"));

                            // Signal light information (blue)
                            Console.WriteLine("Signal light information (blue): 0x" + Data.blueUnit.ToString("X2"));

                            // Signal light information (white)
                            Console.WriteLine("Signal light information (white): 0x" + Data.whiteUnit.ToString("X2"));

                            // Buzzer information
                            Console.WriteLine("Buzzer information: 0x" + Data.buzzerUnit.ToString("X2"));

                            // WDT monitoring information
                            Console.WriteLine("WDT monitoring information: 0x" + Data.surveillance.ToString("X2"));

                            // External input information
                            Console.WriteLine("External input information");
                            // External input 8
                            Console.WriteLine(" External input 8: 0b" + ((Data.externalInput & 0x80) != 0 ? 1 : 0).ToString());
                            // External input 7
                            Console.WriteLine(" External input 7: 0b" + ((Data.externalInput & 0x40) != 0 ? 1 : 0).ToString());
                            // External input 6
                            Console.WriteLine(" External input 6: 0b" + ((Data.externalInput & 0x20) != 0 ? 1 : 0).ToString());
                            // External input 5
                            Console.WriteLine(" External input 5: 0b" + ((Data.externalInput & 0x10) != 0 ? 1 : 0).ToString());
                            // External input 4
                            Console.WriteLine(" External input 4: 0b" + ((Data.externalInput & 0x08) != 0 ? 1 : 0).ToString());
                            // External input 3
                            Console.WriteLine(" External input 3: 0b" + ((Data.externalInput & 0x04) != 0 ? 1 : 0).ToString());
                            // External input 2
                            Console.WriteLine(" External input 2: 0b" + ((Data.externalInput & 0x02) != 0 ? 1 : 0).ToString());
                            // External input 1
                            Console.WriteLine(" External input 1: 0b" + ((Data.externalInput & 0x01) != 0 ? 1 : 0).ToString());

                            // RS232C data
                            Console.WriteLine("RS232C data");
                            // Input information size
                            Console.WriteLine("Input information size: 0x" + Data.RS232CData.size.ToString("X2"));
                            // serial number
                            Console.WriteLine("Serial number: 0x" + Data.RS232CData.serialNumber.ToString("X2"));
                            // Input information
                            Console.Write("Input information:");
                            foreach (var item in Data.RS232CData.data)
                            {
                                Console.Write("0x" + item.ToString("X2") + " ");
                            }
                        }
                        break;
                    }

                case "2":
                    {
                        // Count value notification
                        WDR_COUNT_NOTICE_RECV_DATA Data = new WDR_COUNT_NOTICE_RECV_DATA();
                        ret = WDR_CountNoticeCommand(out Data);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("");
                            Console.WriteLine("Count value notification data");

                            // IEEE address
                            Console.WriteLine("IEEE address: 0x" + Data.IEEEAddress.ToString("X16"));

                            // Time information
                            Console.WriteLine("Time information: 0x" + Data.time.ToString("X16"));

                            // version information
                            Console.WriteLine("version information");
                            // Major version
                            Console.WriteLine(" Major version: 0x" + Data.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine(" Minor version: 0x" + Data.version.minor.ToString("X2"));

                            // action mode
                            Console.WriteLine("Operation mode: 0x" + Data.actionMode.ToString("X2"));

                            // WDT information
                            Console.WriteLine("WDT information");
                            // version information
                            Console.WriteLine(" version information");
                            // Major version
                            Console.WriteLine("  Major version: 0x" + Data.wdtData.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine("  Minor version: 0x" + Data.wdtData.version.minor.ToString("X2"));
                            // Status information
                            Console.WriteLine(" Status information: 0x" + Data.wdtData.status.ToString("X2"));

                            // Base unit information
                            Console.WriteLine("Base unit information");
                            // Unit type
                            Console.WriteLine(" Unit type: 0x" + Data.baseUnitData.format.ToString("X2"));
                            // version information
                            Console.WriteLine(" version information");
                            // Major version
                            Console.WriteLine("  Major version: 0x" + Data.baseUnitData.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine("  Minor version: 0x" + Data.baseUnitData.version.minor.ToString("X2"));
                            // DIP switch information
                            Console.WriteLine(" DIP switch information");
                            // Switch 3
                            Console.WriteLine("  Switch 3: 0b" + ((Data.baseUnitData.dipSwitch & 0x08) != 0 ? 1 : 0).ToString());
                            // Switch 2
                            Console.WriteLine("  Switch 2: 0b" + ((Data.baseUnitData.dipSwitch & 0x04) != 0 ? 1 : 0).ToString());
                            // Switch 1
                            Console.WriteLine("  Switch 1: 0b" + ((Data.baseUnitData.dipSwitch & 0x02) != 0 ? 1 : 0).ToString());
                            // Switch 0
                            Console.WriteLine("  Switch 0: 0b" + ((Data.baseUnitData.dipSwitch & 0x01) != 0 ? 1 : 0).ToString());

                            // Count value
                            Console.WriteLine("Count value: 0x" + Data.countValue.ToString("X8"));
                        }
                        break;
                    }

                case "3":
                    {
                        // Signal light display change notification
                        WDR_SIGNAL_LIGHT_CHANGE_NOTICE_RECV_DATA Data = new WDR_SIGNAL_LIGHT_CHANGE_NOTICE_RECV_DATA();
                        ret = WDR_SignalLightChangeNoticeCommand(out Data);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("");
                            Console.WriteLine("Signal light display change notification data");

                            // IEEE address
                            Console.WriteLine("IEEE address: 0x" + Data.IEEEAddress.ToString("X16"));

                            // Time information
                            Console.WriteLine("Time information: 0x" + Data.time.ToString("X16"));

                            // version information
                            Console.WriteLine("version information");
                            // Major version
                            Console.WriteLine(" Major version: 0x" + Data.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine(" Minor version: 0x" + Data.version.minor.ToString("X2"));

                            // action mode
                            Console.WriteLine("Operation mode: 0x" + Data.actionMode.ToString("X2"));

                            // WDT information
                            Console.WriteLine("WDT information");
                            // version information
                            Console.WriteLine(" version information");
                            // Major version
                            Console.WriteLine("  Major version: 0x" + Data.wdtData.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine("  Minor version: 0x" + Data.wdtData.version.minor.ToString("X2"));
                            // Status information
                            Console.WriteLine(" Status information: 0x" + Data.wdtData.status.ToString("X2"));

                            // Base unit information
                            Console.WriteLine("Base unit information");
                            // Unit type
                            Console.WriteLine(" Unit type: 0x" + Data.baseUnitData.format.ToString("X2"));
                            // version information
                            Console.WriteLine(" version information");
                            // Major version
                            Console.WriteLine("  Major version: 0x" + Data.baseUnitData.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine("  Minor version: 0x" + Data.baseUnitData.version.minor.ToString("X2"));
                            // DIP switch information
                            Console.WriteLine(" DIP switch information");
                            // Switch 3
                            Console.WriteLine("  Switch 3: 0b" + ((Data.baseUnitData.dipSwitch & 0x08) != 0 ? 1 : 0).ToString());
                            // Switch 2
                            Console.WriteLine("  Switch 2: 0b" + ((Data.baseUnitData.dipSwitch & 0x04) != 0 ? 1 : 0).ToString());
                            // Switch 1
                            Console.WriteLine("  Switch 1: 0b" + ((Data.baseUnitData.dipSwitch & 0x02) != 0 ? 1 : 0).ToString());
                            // Switch 0
                            Console.WriteLine("  Switch 0: 0b" + ((Data.baseUnitData.dipSwitch & 0x01) != 0 ? 1 : 0).ToString());

                            // Red unit
                            Console.WriteLine("Red unit: 0x" + Data.redUnit.ToString("X2"));

                            // Yellow unit
                            Console.WriteLine("Yellow unit: 0x" + Data.yellowUnit.ToString("X2"));

                            // Green unit
                            Console.WriteLine("Green unit: 0x" + Data.greenUnit.ToString("X2"));

                            // Blue unit
                            Console.WriteLine("Blue unit: 0x" + Data.blueUnit.ToString("X2"));

                            // White unit
                            Console.WriteLine("White unit: 0x" + Data.whiteUnit.ToString("X2"));

                            // Buzzer unit
                            Console.WriteLine("Buzzer unit: 0x" + Data.buzzerUnit.ToString("X2"));
                        }
                        break;
                    }

                case "4":
                    {
                        // Get IEEE
                        long IEEEAddress = 0;
                        Console.WriteLine("Enter the IEEE address of the transmitter (example: 6CE4DAFFFE010101)");
                        inputData = Console.ReadLine();
                        Int64.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out IEEEAddress);

                        // Transmitter status acquisition request / response
                        WDR_TRANSMITTER_STATUS_REQUEST_RES_DATA Data = new WDR_TRANSMITTER_STATUS_REQUEST_RES_DATA();
                        ret = WDR_TransmitterStatusRequest((ulong)IEEEAddress, out Data);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("");
                            Console.WriteLine("Transmitter status acquisition response data");

                            // Response status
                            Console.WriteLine("Response status: 0x" + Data.controlState.ToString("X2"));

                            // Time information
                            Console.WriteLine("Time information: 0x" + Data.time.ToString("X16"));

                            // version information
                            Console.WriteLine("version information");
                            // Major version
                            Console.WriteLine(" Major version: 0x" + Data.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine(" Minor version: 0x" + Data.version.minor.ToString("X2"));

                            // action mode
                            Console.WriteLine("Operation mode: 0x" + Data.actionMode.ToString("X2"));

                            // WDT information
                            Console.WriteLine("WDT information");
                            // version information
                            Console.WriteLine(" version information");
                            // Major version
                            Console.WriteLine("  Major version: 0x" + Data.wdtData.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine("  Minor version: 0x" + Data.wdtData.version.minor.ToString("X2"));
                            // Status information
                            Console.WriteLine(" Status information: 0x" + Data.wdtData.status.ToString("X2"));

                            // Base unit information
                            Console.WriteLine("Base unit information");
                            // Unit type
                            Console.WriteLine(" Unit type: 0x" + Data.baseUnitData.format.ToString("X2"));
                            // version information
                            Console.WriteLine(" version information");
                            // Major version
                            Console.WriteLine("  Major version: 0x" + Data.baseUnitData.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine("  Minor version: 0x" + Data.baseUnitData.version.minor.ToString("X2"));
                            // DIP switch information
                            Console.WriteLine(" DIP switch information");
                            // Switch 3
                            Console.WriteLine("  Switch 3: 0b" + ((Data.baseUnitData.dipSwitch & 0x08) != 0 ? 1 : 0).ToString());
                            // Switch 2
                            Console.WriteLine("  Switch 2: 0b" + ((Data.baseUnitData.dipSwitch & 0x04) != 0 ? 1 : 0).ToString());
                            // Switch 1
                            Console.WriteLine("  Switch 1: 0b" + ((Data.baseUnitData.dipSwitch & 0x02) != 0 ? 1 : 0).ToString());
                            // Switch 0
                            Console.WriteLine("  Switch 0: 0b" + ((Data.baseUnitData.dipSwitch & 0x01) != 0 ? 1 : 0).ToString());

                            // Signal light information (red)
                            Console.WriteLine("Signal light information (red): 0x" + Data.redUnit.ToString("X2"));

                            // Signal light information (yellow)
                            Console.WriteLine("Signal light information (yellow): 0x" + Data.yellowUnit.ToString("X2"));

                            // Signal light information (green)
                            Console.WriteLine("Signal light information (green): 0x" + Data.greenUnit.ToString("X2"));

                            // Signal light information (blue)
                            Console.WriteLine("Signal light information (blue): 0x" + Data.blueUnit.ToString("X2"));

                            // Signal light information (white)
                            Console.WriteLine("Signal light information (white): 0x" + Data.whiteUnit.ToString("X2"));

                            // Buzzer information
                            Console.WriteLine("Buzzer information: 0x" + Data.buzzerUnit.ToString("X2"));

                            // WDT monitoring information
                            Console.WriteLine("WDT monitoring information: 0x" + Data.surveillance.ToString("X2"));

                            // External input information
                            Console.WriteLine("External input information");
                            // External input 8
                            Console.WriteLine(" External input 8: 0b" + ((Data.externalInput & 0x80) != 0 ? 1 : 0).ToString());
                            // External input 7
                            Console.WriteLine(" External input 7: 0b" + ((Data.externalInput & 0x40) != 0 ? 1 : 0).ToString());
                            // External input 6
                            Console.WriteLine(" External input 6: 0b" + ((Data.externalInput & 0x20) != 0 ? 1 : 0).ToString());
                            // External input 5
                            Console.WriteLine(" External input 5: 0b" + ((Data.externalInput & 0x10) != 0 ? 1 : 0).ToString());
                            // External input 4
                            Console.WriteLine(" External input 4: 0b" + ((Data.externalInput & 0x08) != 0 ? 1 : 0).ToString());
                            // External input 3
                            Console.WriteLine(" External input 3: 0b" + ((Data.externalInput & 0x04) != 0 ? 1 : 0).ToString());
                            // External input 2
                            Console.WriteLine(" External input 2: 0b" + ((Data.externalInput & 0x02) != 0 ? 1 : 0).ToString());
                            // External input 1
                            Console.WriteLine(" External input 1: 0b" + ((Data.externalInput & 0x01) != 0 ? 1 : 0).ToString());

                            // RS232C data
                            Console.WriteLine("RS232C data");
                            // Input information size
                            Console.WriteLine("Input information size: 0x" + Data.RS232CData.size.ToString("X2"));
                            // serial number
                            Console.WriteLine("Serial number: 0x" + Data.RS232CData.serialNumber.ToString("X2"));
                            // Input information
                            Console.Write("Input information:");
                            foreach (var item in Data.RS232CData.data)
                            {
                                Console.Write("0x" + item.ToString("X2") + " ");
                            }
                        }

                        break;
                    }

                case "5":
                    {
                        // Transmitter list acquisition request / response
                        WDR_TRANSMITTER_LIST_REQUEST_RES_DATA Data = new WDR_TRANSMITTER_LIST_REQUEST_RES_DATA();
                        ret = WDR_TransmitterListRequest(out Data);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("");
                            Console.WriteLine("Response data for getting a list of transmitters");

                            // Response status
                            Console.WriteLine("Response status: 0x" + Data.controlState.ToString("X2"));

                            // Number of acquisitions
                            Console.WriteLine("Number of acquisitions: 0x" + Data.unitCount.ToString("X2"));

                            // WDT status information
                            for (int count = 0; count < Data.unitCount; count++)
                            {
                                int unitNo = count + 1;
                                Console.WriteLine("WDT status information" + unitNo.ToString());

                                // IEEE address
                                Console.WriteLine(" IEEE address: 0x" + Data.wdtStatus[count].IEEEAddress.ToString("X16"));

                                // Registration status
                                Console.WriteLine(" Registration status: 0x" + Data.wdtStatus[count].registration.ToString("X2"));

                                // Connection Status
                                Console.WriteLine(" Connection status: 0x" + Data.wdtStatus[count].connect.ToString("X2"));
                            }

                        }

                        break;
                    }

                case "6":
                    {
                        // Get IEEE
                        long IEEEAddress = 0;
                        Console.WriteLine("Enter the IEEE address of the transmitter (example: 6CE4DAFFFE010101)");
                        inputData = Console.ReadLine();
                        Int64.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out IEEEAddress);

                        // Transmitter information acquisition request / response
                        WDR_TRANSMITTER_DATA_REQUEST_RES_DATA Data = new WDR_TRANSMITTER_DATA_REQUEST_RES_DATA();
                        WDR_TRANSMITTER_DATA_REQUEST_RES_ADD_DATA addData = new WDR_TRANSMITTER_DATA_REQUEST_RES_ADD_DATA();
                        ret = WDR_TransmitterDataRequest((ulong)IEEEAddress, out Data, out addData);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("");
                            Console.WriteLine("Response data for transmitter information acquisition");

                            // Response status
                            Console.WriteLine("Response status: 0x" + Data.controlState.ToString("X2"));

                            // User name
                            Console.WriteLine("User name :" + Encoding.Default.GetString(Data.userName));

                            // version information
                            Console.WriteLine("version information");
                            // Major version
                            Console.WriteLine(" Major version: 0x" + Data.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine(" Minor version: 0x" + Data.version.minor.ToString("X2"));

                            // action mode
                            Console.WriteLine("Operation mode: 0x" + Data.actionMode.ToString("X2"));

                            // WDT information
                            Console.WriteLine("WDT information");
                            // version information
                            Console.WriteLine(" version information");
                            // Major version
                            Console.WriteLine("  Major version: 0x" + Data.wdtData.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine("  Minor version: 0x" + Data.wdtData.version.minor.ToString("X2"));
                            // Status information
                            Console.WriteLine(" Status information: 0x" + Data.wdtData.status.ToString("X2"));

                            // Base unit information
                            Console.WriteLine("Base unit information");
                            // Unit type
                            Console.WriteLine(" Unit type: 0x" + Data.baseUnitData.format.ToString("X2"));
                            // version information
                            Console.WriteLine(" version information");
                            // Major version
                            Console.WriteLine("  Major version: 0x" + Data.baseUnitData.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine("  Minor version: 0x" + Data.baseUnitData.version.minor.ToString("X2"));
                            // DIP switch information
                            Console.WriteLine(" DIP switch information");
                            // Switch 3
                            Console.WriteLine("  Switch 3: 0b" + ((Data.baseUnitData.dipSwitch & 0x08) != 0 ? 1 : 0).ToString());
                            // Switch 2
                            Console.WriteLine("  Switch 2: 0b" + ((Data.baseUnitData.dipSwitch & 0x04) != 0 ? 1 : 0).ToString());
                            // Switch 1
                            Console.WriteLine("  Switch 1: 0b" + ((Data.baseUnitData.dipSwitch & 0x02) != 0 ? 1 : 0).ToString());
                            // Switch 0
                            Console.WriteLine("  Switch 0: 0b" + ((Data.baseUnitData.dipSwitch & 0x01) != 0 ? 1 : 0).ToString());

                            // ExtendedPanID
                            Console.WriteLine("ExtendedPanID : 0x" + Data.extendedPanID.ToString("X16"));

                            // Frequency channel
                            Console.WriteLine("Frequency channel");
                            int param = 0x04000000;
                            for (int count = 26; count > 10 ; count--)
                            {
                                // Channel
                                Console.WriteLine("　" + count.ToString() + "Channel" + ": 0b" + ((Data.frequencyChannel & param) != 0 ? 1 : 0).ToString());
                                param = param >> 1;

                            }

                            // Signal light input judgment
                            Console.WriteLine("Signal light input judgment: 0x" + Data.signalLightInputJudge.ToString("X2"));

                            // Power settings
                            Console.WriteLine("Power settings: 0x" + Data.powerSetting.ToString("X2"));

                            // Counter setting
                            Console.WriteLine("Counter setting: 0x" + Data.counterSetting.ToString("X2"));

                            // Send mode
                            Console.WriteLine("Send mode: 0x" + Data.sendMode.ToString("X4"));

                            // In the case of WDT-6LR-Z2-PRO, the extended structure is also displayed.
                            if (Data.actionMode == 0xFF)
                            {
                                // Input information transmission method
                                Console.WriteLine("Input information transmission method: 0x" + addData.inputDataTranform.ToString("X2"));

                                // Signal light format
                                Console.WriteLine("Signal light format: 0x" + addData.signalLightFormat.ToString("X2"));

                                // Periodic transmission
                                Console.WriteLine("Periodic transmission: 0x" + addData.regularSend.ToString("X2"));

                                // Simultaneous input judgment sensitivity setting
                                Console.WriteLine("Simultaneous input judgment sensitivity setting: 0x" + addData.concInputSensitiveSetting.ToString("X2"));

                                // Received data file format
                                Console.WriteLine("Received data file format: 0x" + addData.recvDataFileFormat.ToString("X2"));

                                // Communication setting baud rate
                                Console.WriteLine("Communication setting baud rate: 0x" + addData.baudrate.ToString("X2"));

                                // Communication setting data length
                                Console.WriteLine("Communication setting data length: 0x" + addData.dataLength.ToString("X2"));

                                // Communication setting parity
                                Console.WriteLine("Communication setting parity: 0x" + addData.parity.ToString("X2"));

                                // Communication setting stop bit
                                Console.WriteLine("Communication setting stop bit: 0x" + addData.stopBit.ToString("X2"));
                            }
                        }

                        break;
                    }

                case "7":
                    {
                        // Get IEEE
                        long IEEEAddress = 0;
                        Console.WriteLine("Enter the IEEE address of the transmitter (example: 6CE4DAFFFE010101)");
                        inputData = Console.ReadLine();
                        Int64.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out IEEEAddress);

                        // Transmitter call display request / response
                        WDR_TRANSMITTER_CALL_REQUEST_RES_DATA Data = new WDR_TRANSMITTER_CALL_REQUEST_RES_DATA();
                        ret = WDR_TransmitterCallRequest((ulong)IEEEAddress, out Data);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("Transmitter call display response data");
                            // Response status
                            Console.WriteLine("Response status: 0x" + Data.controlState.ToString("X2"));
                        }

                        break;
                    }

                case "8":
                    {
                        WDR_SERIAL_OUTPUT_REQ_DATA outputData = new WDR_SERIAL_OUTPUT_REQ_DATA();

                        // Get IEEE
                        long IEEEAddress = 0;
                        Console.WriteLine("Enter the IEEE address of the transmitter (example: 6CE4DAFFFE010101)");
                        inputData = Console.ReadLine();
                        Int64.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out IEEEAddress);
                        outputData.IEEEAddress = (ulong)IEEEAddress;

                        // Get serial number
                        Console.WriteLine("Please enter the serial number");
                        inputData = Console.ReadLine();
                        byte serialnumber = 0;
                        byte.TryParse(inputData, System.Globalization.NumberStyles.AllowDecimalPoint, null, out serialnumber);
                        outputData.serialNumber = serialnumber;

                        // Get output information
                        string getByte;
                        Console.WriteLine("Enter the output information in hexadecimal not more than 40 bytes (example: 01020304a0b1c3).");
                        inputData = Console.ReadLine();
                        for (int count = 0; count < (inputData.Length / 2); count++)
                        {
                            getByte = inputData.Substring(count * 2, 2);
                            outputData.outputData.Add(byte.Parse(getByte, System.Globalization.NumberStyles.HexNumber));
                        }

                        // Serial data output request / response
                        WDR_SERIAL_OUTPUT_RES_DATA Data = new WDR_SERIAL_OUTPUT_RES_DATA();
                        ret = WDR_SerialOutputRequest(outputData, out Data);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("");
                            Console.WriteLine("Response data of serial data output");

                            // Response status
                            Console.WriteLine("Response status: 0x" + Data.controlState.ToString("X2"));
                        }

                        break;
                    }

                case "9":
                    {
                        WDR_SIGNAL_LIGHT_CONTROL_REQ_DATA controlData = new WDR_SIGNAL_LIGHT_CONTROL_REQ_DATA();

                        // Get IEEE
                        long IEEEAddress = 0;
                        Console.WriteLine("Enter the IEEE address of the transmitter (example: 6CE4DAFFFE010101)");
                        inputData = Console.ReadLine();
                        Int64.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out IEEEAddress);
                        controlData.IEEEAddress = (ulong)IEEEAddress;

                        // Get control time
                        Console.WriteLine("Enter the control time in hexadecimal (example: FF)");
                        inputData = Console.ReadLine();
                        byte controlTime = 0;
                        byte.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out controlTime);
                        controlData.controlTime = controlTime;

                        // Get the lighting pattern of the red unit
                        Console.WriteLine("Enter the lighting pattern of the red unit in hexadecimal (control by control line: 00, off: 10, lighting: 11, blinking: 12, triple flash: 13)");
                        inputData = Console.ReadLine();
                        byte redUnit = 0;
                        byte.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out redUnit);
                        controlData.redUnit = redUnit;

                        // Get the lighting pattern of the yellow unit
                        Console.WriteLine("Enter the lighting pattern of the yellow unit in hexadecimal (control by control line: 00, off: 10, lighting: 11, blinking: 12, triple flash: 13)");
                        inputData = Console.ReadLine();
                        byte yellowUnit = 0;
                        byte.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out yellowUnit);
                        controlData.yellowUnit = yellowUnit;

                        // Get the lighting pattern of the green unit
                        Console.WriteLine("Enter the lighting pattern of the green unit in hexadecimal (control by control line: 00, off: 10, lighting: 11, blinking: 12, triple flash: 13)");
                        inputData = Console.ReadLine();
                        byte greenUnit = 0;
                        byte.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out greenUnit);
                        controlData.greenUnit = greenUnit;

                        // Get the lighting pattern of the blue unit
                        Console.WriteLine("Enter the lighting pattern of the blue unit in hexadecimal (control by control line: 00, off: 10, lighting: 11, blinking: 12, triple flash: 13)");
                        inputData = Console.ReadLine();
                        byte blueUnit = 0;
                        byte.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out blueUnit);
                        controlData.blueUnit = blueUnit;

                        // Get the lighting pattern of the white unit
                        Console.WriteLine("Enter the lighting pattern of the white unit in hexadecimal (control by control line: 00, off: 10, lighting: 11, blinking: 12, triple flash: 13)");
                        inputData = Console.ReadLine();
                        byte whiteUnit = 0;
                        byte.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out whiteUnit);
                        controlData.whiteUnit = whiteUnit;

                        // Get the buzzer unit pattern
                        Console.WriteLine("Enter the buzzer unit pattern in hexadecimal (control by control line: 00, non-sound: 10, sound: 11, intermittent sound: 12)");
                        inputData = Console.ReadLine();
                        byte buzzerUnit = 0;
                        byte.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out buzzerUnit);
                        controlData.buzzerUnit = buzzerUnit;

                        // Signal light display control request / response
                        WDR_SIGNAL_LIGHT_CONTROL_RES_DATA Data = new WDR_SIGNAL_LIGHT_CONTROL_RES_DATA();
                        ret = WDR_SignalLightControlRequest(controlData, out Data);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("");
                            Console.WriteLine("Signal light display control response data");

                            // Response status
                            Console.WriteLine("Response status: 0x" + Data.recvState.ToString("X2"));

                            // Control state
                            Console.WriteLine("Control state: 0x" + Data.controlState.ToString("X2"));

                            // Red unit
                            Console.WriteLine("Red unit: 0x" + Data.redUnit.ToString("X2"));

                            // Yellow unit
                            Console.WriteLine("Yellow unit: 0x" + Data.yellowUnit.ToString("X2"));

                            // Green unit
                            Console.WriteLine("Green unit: 0x" + Data.greenUnit.ToString("X2"));

                            // Blue unit
                            Console.WriteLine("Blue unit: 0x" + Data.blueUnit.ToString("X2"));

                            // White unit
                            Console.WriteLine("White unit: 0x" + Data.whiteUnit.ToString("X2"));

                            // Buzzer unit
                            Console.WriteLine("Buzzer unit: 0x" + Data.buzzerUnit.ToString("X2"));
                        }

                        break;
                    }

                case "10":
                    {
                        // Get IEEE
                        long IEEEAddress = 0;
                        Console.WriteLine("Enter the IEEE address of the transmitter (example: 6CE4DAFFFE010101)");
                        inputData = Console.ReadLine();
                        Int64.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out IEEEAddress);

                        // Signal light display cancellation request / response
                        WDR_SIGNAL_LIGHT_LIFT_RES_DATA Data = new WDR_SIGNAL_LIGHT_LIFT_RES_DATA();
                        ret = WDR_SignalLightLiftRequest((ulong)IEEEAddress, out Data);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("");
                            Console.WriteLine("Response data for canceling the signal light display");

                            // Response status
                            Console.WriteLine("Response status: 0x" + Data.recvState.ToString("X2"));

                            // Control state
                            Console.WriteLine("Control state: 0x" + Data.controlState.ToString("X2"));

                            // Red unit
                            Console.WriteLine("Red unit: 0x" + Data.redUnit.ToString("X2"));

                            // Yellow unit
                            Console.WriteLine("Yellow unit: 0x" + Data.yellowUnit.ToString("X2"));
                            // Green unit
                            Console.WriteLine("Green unit: 0x" + Data.greenUnit.ToString("X2"));
                            // Blue unit
                            Console.WriteLine("Blue unit: 0x" + Data.blueUnit.ToString("X2"));
                            // White unit
                            Console.WriteLine("White unit: 0x" + Data.whiteUnit.ToString("X2"));
                            // Buzzer unit
                            Console.WriteLine("Buzzer unit: 0x" + Data.buzzerUnit.ToString("X2"));
                        }

                        break;
                    }

                case "11":
                    {
                        WDR_SIGNAL_LIGHT_COUNT_SET_REQ_DATA setData = new WDR_SIGNAL_LIGHT_COUNT_SET_REQ_DATA();

                        // Get IEEE
                        long IEEEAddress = 0;
                        Console.WriteLine("Enter the IEEE address of the transmitter (example: 6CE4DAFFFE010101)");
                        inputData = Console.ReadLine();
                        Int64.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out IEEEAddress);
                        setData.IEEEAddress = (ulong)IEEEAddress;

                        // Get the count registration value
                        int setCount = 0;
                        Console.WriteLine("Enter the count registration value in hexadecimal (example: 0000FFFF)");
                        inputData = Console.ReadLine();
                        Int32.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out setCount);
                        setData.setCount = (uint)setCount;

                        // Count value registration request / response
                        WDR_SIGNAL_LIGHT_COUNT_SET_RES_DATA Data = new WDR_SIGNAL_LIGHT_COUNT_SET_RES_DATA();
                        ret = WDR_SignalLightCountSetRequest(setData, out Data);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("");
                            Console.WriteLine("Response data for count value registration");

                            // Response status
                            Console.WriteLine("Response status: 0x" + Data.controlState.ToString("X2"));
                        }

                        break;
                    }

                case "12":
                    {
                        // Receiver information acquisition request / response
                        WDR_RECEIVER_DATA_REQUEST_RES_DATA Data = new WDR_RECEIVER_DATA_REQUEST_RES_DATA();
                        ret = WDR_ReceiveDataRequest(out Data);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("");
                            Console.WriteLine("Response data for receiver information acquisition");

                            // Response status
                            Console.WriteLine("Response status: 0x" + Data.controlState.ToString("X2"));

                            // ExtendedPanID
                            Console.WriteLine("ExtendedPanID : 0x" + Data.extendedPanID.ToString("X16"));

                            // Frequency channel
                            Console.WriteLine("Frequency channel");
                            int param = 0x04000000;
                            for (int count = 26; count > 10; count--)
                            {
                                // Channel
                                Console.WriteLine("　" + count.ToString() + "Channel" + ": 0b" + ((Data.frequencyChannel & param) != 0 ? 1 : 0).ToString());
                                param = param >> 1;

                            }

                            // Firmware version
                            Console.WriteLine("Firmware version information");
                            // Major version
                            Console.WriteLine(" Major version: 0x" + Data.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine(" Minor version: 0x" + Data.version.minor.ToString("X2"));

                            // Network status
                            Console.WriteLine("Network status: 0x" + Data.networkStatus.ToString("X2"));

                            // How to boot the network
                            Console.WriteLine("Network boot method: 0x" + Data.networkBoot.ToString("X2"));

                            // Running ExtendedPanID
                            Console.WriteLine("Running ExtendedPanID: 0x" + Data.actionExtendedPanID.ToString("X16"));

                            // Operating frequency channel
                            Console.WriteLine("Operating frequency channel: 0x" + Data.actionFrequencyChannel.ToString("X2"));

                        }

                        break;
                    }

                case "13":
                    {
                        // Receiver reset request / response
                        WDR_RECEIVER_RESET_RES_DATA Data = new WDR_RECEIVER_RESET_RES_DATA();
                        ret = WDR_ReceiverResetRequest(out Data);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("");
                            Console.WriteLine("Receiver reset response data");

                            // Response status
                            Console.WriteLine("Response status: 0x" + Data.controlState.ToString("X2"));
                        }

                        break;
                    }

                case "14":
                    {
                        // Get IEEE
                        long IEEEAddress = 0;
                        Console.WriteLine("Enter the IEEE address of the transmitter (example: 6CE4DAFFFE010101)");
                        inputData = Console.ReadLine();
                        Int64.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out IEEEAddress);

                        // Count value acquisition request / response
                        WDR_SIGNAL_LIGHT_COUNT_GET_RES_DATA Data = new WDR_SIGNAL_LIGHT_COUNT_GET_RES_DATA();
                        ret = WDR_SignalLightCountGetRequest((ulong)IEEEAddress, out Data);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("");
                            Console.WriteLine("Response data for count value acquisition");

                            // Response status
                            Console.WriteLine("Response status: 0x" + Data.controlState.ToString("X2"));

                            // Time information
                            Console.WriteLine("Time information: 0x" + Data.time.ToString("X16"));

                            // version information
                            Console.WriteLine("version information");
                            // Major version
                            Console.WriteLine(" Major version: 0x" + Data.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine(" Minor version: 0x" + Data.version.minor.ToString("X2"));

                            // action mode
                            Console.WriteLine("Operation mode: 0x" + Data.actionMode.ToString("X2"));

                            // WDT information
                            Console.WriteLine("WDT information");
                            // version information
                            Console.WriteLine(" version information");
                            // Major version
                            Console.WriteLine("  Major version: 0x" + Data.wdtData.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine("  Minor version: 0x" + Data.wdtData.version.minor.ToString("X2"));
                            // Status information
                            Console.WriteLine(" Status information: 0x" + Data.wdtData.status.ToString("X2"));

                            // Base unit information
                            Console.WriteLine("Base unit information");
                            // Unit type
                            Console.WriteLine(" Unit type: 0x" + Data.baseUnitData.format.ToString("X2"));
                            // version information
                            Console.WriteLine(" version information");
                            // Major version
                            Console.WriteLine("  Major version: 0x" + Data.baseUnitData.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine("  Minor version: 0x" + Data.baseUnitData.version.minor.ToString("X2"));
                            // DIP switch information
                            Console.WriteLine(" DIP switch information");
                            // Switch 3
                            Console.WriteLine("  Switch 3: 0b" + ((Data.baseUnitData.dipSwitch & 0x08) != 0 ? 1 : 0).ToString());
                            // Switch 2
                            Console.WriteLine("  Switch 2: 0b" + ((Data.baseUnitData.dipSwitch & 0x04) != 0 ? 1 : 0).ToString());
                            // Switch 1
                            Console.WriteLine("  Switch 1: 0b" + ((Data.baseUnitData.dipSwitch & 0x02) != 0 ? 1 : 0).ToString());
                            // Switch 0
                            Console.WriteLine("  Switch 0: 0b" + ((Data.baseUnitData.dipSwitch & 0x01) != 0 ? 1 : 0).ToString());

                            // Count value
                            Console.WriteLine("Count value: 0x" + Data.count.ToString("X8"));

                        }

                        break;
                    }

                case "15":
                    {
                        // Get IEEE
                        long IEEEAddress = 0;
                        Console.WriteLine("Enter the IEEE address of the transmitter (example: 6CE4DAFFFE010101)");
                        inputData = Console.ReadLine();
                        Int64.TryParse(inputData, System.Globalization.NumberStyles.HexNumber, null, out IEEEAddress);

                        // Signal light display acquisition request / response
                        WDR_SIGNAL_LIGHT_DATA_GET_RES_DATA Data = new WDR_SIGNAL_LIGHT_DATA_GET_RES_DATA();
                        ret = WDR_SignalLightDataGetRequest((ulong)IEEEAddress, out Data);
                        if (ret == 0)
                        {
                            // Display acquired data
                            Console.WriteLine("");
                            Console.WriteLine("Response data for signal light display acquisition");

                            // Response status
                            Console.WriteLine("Response status: 0x" + Data.controlState.ToString("X2"));

                            // Time information
                            Console.WriteLine("Time information: 0x" + Data.time.ToString("X16"));

                            // version information
                            Console.WriteLine("version information");
                            // Major version
                            Console.WriteLine(" Major version: 0x" + Data.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine(" Minor version: 0x" + Data.version.minor.ToString("X2"));

                            // action mode
                            Console.WriteLine("Operation mode: 0x" + Data.actionMode.ToString("X2"));

                            // WDT information
                            Console.WriteLine("WDT information");
                            // version information
                            Console.WriteLine(" version information");
                            // Major version
                            Console.WriteLine("  Major version: 0x" + Data.wdtData.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine("  Minor version: 0x" + Data.wdtData.version.minor.ToString("X2"));
                            // Status information
                            Console.WriteLine(" Status information: 0x" + Data.wdtData.status.ToString("X2"));

                            // Base unit information
                            Console.WriteLine("Base unit information");
                            // Unit type
                            Console.WriteLine(" Unit type: 0x" + Data.baseUnitData.format.ToString("X2"));
                            // version information
                            Console.WriteLine(" version information");
                            // Major version
                            Console.WriteLine("  Major version: 0x" + Data.baseUnitData.version.major.ToString("X2"));
                            // Minor version
                            Console.WriteLine("  Minor version: 0x" + Data.baseUnitData.version.minor.ToString("X2"));
                            // DIP switch information
                            Console.WriteLine(" DIP switch information");
                            // Switch 3
                            Console.WriteLine("  Switch 3: 0b" + ((Data.baseUnitData.dipSwitch & 0x08) != 0 ? 1 : 0).ToString());
                            // Switch 2
                            Console.WriteLine("  Switch 2: 0b" + ((Data.baseUnitData.dipSwitch & 0x04) != 0 ? 1 : 0).ToString());
                            // Switch 1
                            Console.WriteLine("  Switch 1: 0b" + ((Data.baseUnitData.dipSwitch & 0x02) != 0 ? 1 : 0).ToString());
                            // Switch 0
                            Console.WriteLine("  Switch 0: 0b" + ((Data.baseUnitData.dipSwitch & 0x01) != 0 ? 1 : 0).ToString());

                            // Red unit
                            Console.WriteLine("Red unit: 0x" + Data.redUnit.ToString("X2"));

                            // Yellow unit
                            Console.WriteLine("Yellow unit: 0x" + Data.yellowUnit.ToString("X2"));

                            // Green unit
                            Console.WriteLine("Green unit: 0x" + Data.greenUnit.ToString("X2"));

                            // Blue unit
                            Console.WriteLine("Blue unit: 0x" + Data.blueUnit.ToString("X2"));

                            // White unit
                            Console.WriteLine("White unit: 0x" + Data.whiteUnit.ToString("X2"));

                            // Buzzer unit
                            Console.WriteLine("Buzzer unit: 0x" + Data.buzzerUnit.ToString("X2"));

                        }

                        break;
                    }

                default:
                    {
                        // Since it is an unexpected identifier, it ends without doing anything
                        Console.WriteLine("An unexpected command identifier was entered, so it exits without doing anything.");
                        break;
                    }
            }

            // Close the socket
            SocketClose();
         }

        /// <summary>
        /// Connect to WDR
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <param name="port">port number</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int SocketOpen(string ip, int port)
        {
            try
            {
                // Set IP address and port
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Creating a socket
                sock = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                if (sock == null)
                {
                    Console.WriteLine("failed to create socket");
                    return -1;
                }

                // Connect to WDR
                sock.Connect(remoteEP);
            }
            catch (Exception ex)
            {
                Console.WriteLine("socket open error");
                if (sock != null)
                {
                    sock.Close();
                }
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Close socket
        /// </summary>
        public static void SocketClose()
        {
            if (sock != null)
            {
                //Closing the socket。
                sock.Shutdown(SocketShutdown.Both);
                sock.Close();
            }

        }

        /// <summary>
        /// Divide the received data for each command and judge whether it is the command you want to receive
        /// </summary>
        /// <param name="recvdata">received data</param>
        /// <param name="recvSize">Received size</param>
        /// <param name="mode">Target command</param>
        /// <param name="startPos">Target data start position</param>
        /// <param name="getSize">Target data size</param>
        /// <returns>If there is no data: 0, if there is data: other than 0</returns>
        public static bool RecvDatagujde(byte[] recvdata, int recvSize, ushort mode, out int startPos, out int getSize)
        {
            bool ret = false;
            ushort resSize = 0;
            ushort resMode = 0;

            startPos = 0;
            getSize = 0;

            // If the reception size is 0 or less, it has not been received, so it ends as it is
            if (recvSize <= 0)
            {
                return ret;
            }

            do
            {
                // Get the size of one response
                resSize = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvdata, startPos + 4));

                // Get command
                resMode = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvdata, startPos + 15));

                // Command judgment
                if (resMode == mode)
                {
                    // If it is the target command, set the size for one response to the acquisition size, set the return value to true, and exit.
                    getSize = resSize + 6;
                    ret = true;
                    break;
                }

                // Add the size of one response to startPos
                startPos += (resSize + 6);

            } while (recvSize > startPos);  // When startPos becomes larger than recvSize, there is no target command in the received data, so it ends.

            return ret;
        }

        /// <summary>
        /// Send a command and receive a response
        /// </summary>
        /// <param name="mode">Command mode</param>
        /// <param name="sendData">Transmission data</param>
        /// <param name="recvData">received data</param>
        /// <param name="recvTimeout">Receive timeout (specified in milliseconds)</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int SendCommand(ushort mode, byte[] sendData, out byte[] recvData, int recvTimeout)
        {
            int ret;
            recvData = null;

            try
            {
                if (sock == null)
                {
                    Console.WriteLine("socket is not");
                    return -1;
                }

                // Set timeout
                sock.SendTimeout = 1000;
                sock.ReceiveTimeout = recvTimeout;

                // send
                ret = sock.Send(sendData);
                if (ret < 0)
                {
                    Console.WriteLine("failed to send");
                    return -1;
                }

                // Receive response data
                byte[] bytes = new byte[1024];
                int recvSize = 0;
                int pos;
                while (true)
                {
                    // Data reception
                    recvSize = sock.Receive(bytes);
                    if (recvSize < 0)
                    {
                        Console.WriteLine("failed to recv");
                        return -1;
                    }

                    // Get the data of the target command from the received data
                    if (true == RecvDatagujde(bytes, recvSize, mode, out pos, out recvSize))
                    {
                        break;
                    }
                }
                recvData = new byte[recvSize];
                Array.Copy(bytes, pos, recvData, 0, recvSize);

                // Judgment of command error response
                ushort commandSize = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvData, 4));
                bool errorFlag = false;
                string errorMessage = "";
                if (commandSize == 0x000C)
                {
                    errorFlag = true;
                    switch (recvData[17])
                    {
                        case 0x80:
                            errorMessage = "Command error";
                            break;

                        case 0x81:
                            errorMessage = "Mode error";
                            break;

                        case 0x82:
                            errorMessage = "Data error";
                            break;

                        case 0x83:
                            errorMessage = "Connection unit error";
                            break;

                        case 0x84:
                            errorMessage = "Wireless module response error";
                            break;

                        case 0x86:
                            errorMessage = "Data acquisition error";
                            break;

                        case 0xC0:
                            errorMessage = "Initialization abnormality";
                            break;

                        case 0xFF:
                            errorMessage = "Exception anomaly";
                            break;

                        default:
                            errorFlag = false;
                            break;
                    }
                }

                if (errorFlag)
                {
                    Console.WriteLine(errorMessage);
                    return -1;
                }

            }
            catch (SocketException e)
            {
                if (e.ErrorCode == WSAETIMEDOUT)
                {
                    Console.WriteLine("TimeOut");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Receive command
        /// </summary>
        /// <param name="mode">Command mode</param>
        /// <param name="recvData">received data</param>
        /// <param name="recvTimeout">Receive timeout (specified in milliseconds)</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int RecvCommand(ushort mode, out byte[] recvData, int recvTimeout)
        {
            recvData = null;

            try
            {
                if (sock == null)
                {
                    Console.WriteLine("socket is not");
                    return -1;
                }

                // Set timeout
                sock.ReceiveTimeout = recvTimeout;

                // Receive notification data
                byte[] bytes = new byte[1024];
                int recvSize = 0;
                int pos;
                while (true)
                {
                    // Data reception
                    recvSize = sock.Receive(bytes);
                    if (recvSize < 0)
                    {
                        Console.WriteLine("failed to recv");
                        return -1;
                    }

                    // Get the data of the target command from the received data
                    if (true == RecvDatagujde(bytes, recvSize, mode, out pos, out recvSize))
                    {
                        break;
                    }
                }
                recvData = new byte[recvSize];
                Array.Copy(bytes, pos, recvData, 0, recvSize);
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == WSAETIMEDOUT)
                {
                    Console.WriteLine("TimeOut");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Receive transmitter status change notification
        /// </summary>
        /// <param name="Data">received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_StatusChangeNoticeCommand(out WDR_STATUS_CHANGE_NOTICE_RECV_DATA Data)
        {
            int ret;
            Data = new WDR_STATUS_CHANGE_NOTICE_RECV_DATA();

            // Command reception
            byte[] recvData;
            ret = RecvCommand(WDR_COMMAND_MODE_STATUS_CHANGE_NOTICE, out recvData, WDR_STATUS_CHANGE_NOTICE_TIMEOUT);

            // Check for response data
            if (recvData == null)
            {
                // Exception error (including timeout) occurred
                return -1;
            }

            // Check the response data
            if (recvData[0] == PNS_NAK)
            {
                // Receive an abnormal response
                Console.WriteLine("negative acknowledge");
                return -1;
            }

            // IEEE address
            Data.IEEEAddress = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(recvData, 7));

            // serial number
            Data.serialNumber = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvData, 18));

            // Time information
            Data.time = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(recvData, 22));

            // version information
            Data.version = new WDR_VERSION_DATA
            {
                major = recvData[30],      // Major version
                minor = recvData[31],      // Minor version
            };

            // action mode
            Data.actionMode = recvData[32];

            // WDT information
            Data.wdtData = new WDR_INFO_DATA
            {
                version = new WDR_WDT_VERSION_DATA  // version information
                {
                    major = recvData[33],   // Major version
                    minor = recvData[34],   // Minor version
                    dummy = recvData[35]    // Fixed value
                },
                status = recvData[36],      // Status information
            };

            // Base unit information
            Data.baseUnitData = new WDR_BASEUNIT_DATA
            {
                format = recvData[37],                  // Unit type
                version = new WDR_WDT_VERSION_DATA      // version information
                {
                    major = recvData[38],               // Major version
                    minor = recvData[39],               // Minor version
                    dummy = recvData[40]                // Fixed value
                },
                dipSwitch = recvData[41]                // DIP switch information
            };

            // Signal light information (red)
            Data.redUnit = recvData[47];

            // Signal light information (yellow)
            Data.yellowUnit = recvData[48];

            // Signal light information (green)
            Data.greenUnit = recvData[49];

            // Signal light information (blue)
            Data.blueUnit = recvData[50];

            // Signal light information (white)
            Data.whiteUnit = recvData[51];

            // Buzzer information
            Data.buzzerUnit = recvData[52];

            // WDT monitoring information
            Data.surveillance = recvData[53];

            // External input information
            Data.externalInput = recvData[54];

            // RS232C data
            Data.RS232CData = new WDR_RS232C_DATA();
            Data.RS232CData.size = recvData[55];                                                // Input information size
            Data.RS232CData.serialNumber = recvData[56];                                        // serial number
            Data.RS232CData.data = new byte[60];
            Array.Copy(recvData, 57, Data.RS232CData.data, 0, Data.RS232CData.data.Length);     // Input information

            return 0;
        }

        /// <summary>
        /// Receive count value notification
        /// </summary>
        /// <param name="Data">received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_CountNoticeCommand(out WDR_COUNT_NOTICE_RECV_DATA Data)
        {
            int ret;
            Data = new WDR_COUNT_NOTICE_RECV_DATA();

            // Command reception
            byte[] recvData;
            ret = RecvCommand(WDR_COMMAND_MODE_COUNT_NOTICE, out recvData, WDR_COUNT_NOTICE_TIMEOUT);

            // Check for response data
            if (recvData == null)
            {
                // Exception error (including timeout) occurred
                return -1;
            }

            // Check for response data
            if (recvData == null)
            {
                // Exception error (including timeout) occurred
                return -1;
            }

            // Check the response data
            if (recvData[0] == PNS_NAK)
            {
                // Receive an abnormal response
                Console.WriteLine("negative acknowledge");
                return -1;
            }

            // IEEE address
            Data.IEEEAddress = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(recvData, 7));

            // Time information
            Data.time = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(recvData, 22));

            // version information
            Data.version = new WDR_VERSION_DATA
            {
                major = recvData[30],      // Major version
                minor = recvData[31],      // Minor version
            };

            // action mode
            Data.actionMode = recvData[32];

            // WDT information
            Data.wdtData = new WDR_INFO_DATA
            {
                version = new WDR_WDT_VERSION_DATA  // version information
                {
                    major = recvData[33],   // Major version
                    minor = recvData[34],   // Minor version
                    dummy = recvData[35]    // Fixed value
                },
                status = recvData[36],      // Status information
            };

            // Base unit information
            Data.baseUnitData = new WDR_BASEUNIT_DATA
            {
                format = recvData[37],                  // Unit type
                version = new WDR_WDT_VERSION_DATA      // version information
                {
                    major = recvData[38],               // Major version
                    minor = recvData[39],               // Minor version
                    dummy = recvData[40]                // Fixed value
                },
                dipSwitch = recvData[41]                // DIP switch information
            };

            // Count value
            Data.countValue = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvData, 47));

            return 0;
        }

        /// <summary>
        /// Receive signal light display change notification
        /// </summary>
        /// <param name="Data">received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_SignalLightChangeNoticeCommand(out WDR_SIGNAL_LIGHT_CHANGE_NOTICE_RECV_DATA Data)
        {
            int ret;
            Data = new WDR_SIGNAL_LIGHT_CHANGE_NOTICE_RECV_DATA();

            // Command reception
            byte[] recvData;
            ret = RecvCommand(WDR_COMMAND_MODE_SIGNAL_LIGHT_CHANGE_NOTICE, out recvData, WDR_SIGNAL_LIGHT_CHANGE_NOTICE_TIMEOUT);

            // Check for response data
            if (recvData == null)
            {
                // Exception error (including timeout) occurred
                return -1;
            }

            // Check the response data
            if (recvData[0] == PNS_NAK)
            {
                // Receive an abnormal response
                Console.WriteLine("negative acknowledge");
                return -1;
            }

            // IEEE address
            Data.IEEEAddress = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(recvData, 7));

            // Time information
            Data.time = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(recvData, 22));

            // version information
            Data.version = new WDR_VERSION_DATA
            {
                major = recvData[30],      // Major version
                minor = recvData[31],      // Minor version
            };

            // action mode
            Data.actionMode = recvData[32];

            // WDT information
            Data.wdtData = new WDR_INFO_DATA
            {
                version = new WDR_WDT_VERSION_DATA  // version information
                {
                    major = recvData[33],   // Major version
                    minor = recvData[34],   // Minor version
                    dummy = recvData[35]    // Fixed value
                },
                status = recvData[36],      // Status information
            };

            // Base unit information
            Data.baseUnitData = new WDR_BASEUNIT_DATA
            {
                format = recvData[37],                  // Unit type
                version = new WDR_WDT_VERSION_DATA      // version information
                {
                    major = recvData[38],               // Major version
                    minor = recvData[39],               // Minor version
                    dummy = recvData[40]                // Fixed value
                },
                dipSwitch = recvData[41]                // DIP switch information
            };

            // Red unit
            Data.redUnit = recvData[47];

            // Yellow unit
            Data.yellowUnit = recvData[48];

            // Green unit
            Data.greenUnit = recvData[49];

            // Blue unit
            Data.blueUnit = recvData[50];

            // White unit
            Data.whiteUnit = recvData[51];

            // Buzzer unit
            Data.buzzerUnit = recvData[52];

            return 0;
        }

        /// <summary>
        /// Transmitter status acquisition request / response
        /// </summary>
        /// <param name="IEEEAddress">Transmitter IEEE address</param>
        /// <param name="Data">received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_TransmitterStatusRequest(ulong IEEEAddress, out WDR_TRANSMITTER_STATUS_REQUEST_RES_DATA Data)
        {
            int ret;
            Data = new WDR_TRANSMITTER_STATUS_REQUEST_RES_DATA();

            try
            {
                byte[] sendData = { };

                // Product category
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_PRODUCT_ID).Reverse()).ToArray();

                // identifier
                sendData = sendData.Concat(new byte[] { WDR_COMMAND }).ToArray();

                // Expansion
                sendData = sendData.Concat(new byte[] { WDR_EXPANSION }).ToArray();

                // size
                sendData = sendData.Concat(BitConverter.GetBytes((ushort)0x000B).Reverse()).ToArray();

                // Command type
                sendData = sendData.Concat(new byte[] { WDR_COMMAND_KIND_REQUEST }).ToArray();

                // IEEE address
                sendData = sendData.Concat(BitConverter.GetBytes((ulong)IEEEAddress).Reverse()).ToArray();

                // Command mode
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_COMMAND_MODE_TRANSMITTER_STATUS_REQUEST).Reverse()).ToArray();

                // Send request command
                byte[] recvData;
                ret = SendCommand(WDR_COMMAND_MODE_TRANSMITTER_STATUS_REQUEST, sendData, out recvData, WDR_TRANSMITTER_STATUS_REQUEST_TIMEOUT);
                if (ret != 0)
                {
                    Console.WriteLine("failed to send data");
                    return -1;
                }

                // Check for response data
                if (recvData == null)
                {
                    // Exception error (including timeout) occurred
                    return -1;
                }

                // Check the response data
                if (recvData[0] == PNS_NAK)
                {
                    // Receive an abnormal response
                    Console.WriteLine("negative acknowledge");
                    return -1;
                }

                // Response status
                Data.controlState = recvData[17];

                // Time information
                Data.time = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(recvData, 22));

                // version information
                Data.version = new WDR_VERSION_DATA
                {
                    major = recvData[30],      // Major version
                    minor = recvData[31],      // Minor version
                };

                // action mode
                Data.actionMode = recvData[32];

                // WDT information
                Data.wdtData = new WDR_INFO_DATA
                {
                    version = new WDR_WDT_VERSION_DATA  // version information
                    {
                        major = recvData[33],   // Major version
                        minor = recvData[34],   // Minor version
                        dummy = recvData[35]    // Fixed value
                    },
                    status = recvData[36],      // Status information
                };

                // Base unit information
                Data.baseUnitData = new WDR_BASEUNIT_DATA
                {
                    format = recvData[37],                  // Unit type
                    version = new WDR_WDT_VERSION_DATA      // version information
                    {
                        major = recvData[38],               // Major version
                        minor = recvData[39],               // Minor version
                        dummy = recvData[40]                // Fixed value
                    },
                    dipSwitch = recvData[41]                // DIP switch information
                };

                // Signal light information (red)
                Data.redUnit = recvData[47];

                // Signal light information (yellow)
                Data.yellowUnit = recvData[48];

                // Signal light information (green)
                Data.greenUnit = recvData[49];

                // Signal light information (blue)
                Data.blueUnit = recvData[50];

                // Signal light information (white)
                Data.whiteUnit = recvData[51];

                // Buzzer information
                Data.buzzerUnit = recvData[52];

                // WDT monitoring information
                Data.surveillance = recvData[53];

                // External input information
                Data.externalInput = recvData[54];

                // RS232C data
                Data.RS232CData = new WDR_RS232C_DATA();
                Data.RS232CData.size = recvData[55];                                            // Input information size
                Data.RS232CData.serialNumber = recvData[56];                                    // serial number
                Data.RS232CData.data = new byte[60];
                Array.Copy(recvData, 57, Data.RS232CData.data, 0, Data.RS232CData.data.Length); // Input information


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Transmitter list acquisition request / response
        /// </summary>
        /// <param name="Data">received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_TransmitterListRequest(out WDR_TRANSMITTER_LIST_REQUEST_RES_DATA Data)
        {
            int ret;
            Data = new WDR_TRANSMITTER_LIST_REQUEST_RES_DATA();

            try
            {
                byte[] sendData = { };

                // Product category
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_PRODUCT_ID).Reverse()).ToArray();

                // identifier
                sendData = sendData.Concat(new byte[] { WDR_COMMAND }).ToArray();

                // Expansion
                sendData = sendData.Concat(new byte[] { WDR_EXPANSION }).ToArray();

                // size
                sendData = sendData.Concat(BitConverter.GetBytes((ushort)0x000B).Reverse()).ToArray();

                // Command type
                sendData = sendData.Concat(new byte[] { WDR_COMMAND_KIND_REQUEST }).ToArray();

                // IEEE address
                byte[] IEEEEdata = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                sendData = sendData.Concat(IEEEEdata).ToArray();

                // Command mode
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_COMMAND_MODE_TRANSMITTER_LIST_REQUEST).Reverse()).ToArray();

                // Send request command
                byte[] recvData;
                ret = SendCommand(WDR_COMMAND_MODE_TRANSMITTER_LIST_REQUEST, sendData, out recvData, WDR_TRANSMITTER_LIST_REQUEST_TIMEOUT);
                if (ret != 0)
                {
                    Console.WriteLine("failed to send data");
                    return -1;
                }

                // Check for response data
                if (recvData == null)
                {
                    // Exception error (including timeout) occurred
                    return -1;
                }

                // Check the response data
                if (recvData[0] == PNS_NAK)
                {
                    // Receive an abnormal response
                    Console.WriteLine("negative acknowledge");
                    return -1;
                }

                // Response status
                Data.controlState = recvData[17];

                // Number of acquisitions
                Data.unitCount = recvData[18];

                // WDT status information
                for (int count = 0; count < Data.unitCount; count++)
                {
                    // Object creation
                    WDR_WDT_STATUS_DATA setData = new WDR_WDT_STATUS_DATA();

                    // IEEE address
                    setData.IEEEAddress = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(recvData, 19 + (count * 10)));

                    // Registration status
                    setData.registration = recvData[27 + (count * 10)];

                    // Connection Status
                    setData.connect = recvData[28 + (count * 10)];

                    // Set in an array
                    Data.wdtStatus[count] = setData;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Transmitter information acquisition request / response
        /// </summary>
        /// <param name="IEEEAddress">Transmitter IEEE address</param>
        /// <param name="Data">received data</param>
        /// <param name="addData">Extended received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_TransmitterDataRequest(ulong IEEEAddress, out WDR_TRANSMITTER_DATA_REQUEST_RES_DATA Data, out WDR_TRANSMITTER_DATA_REQUEST_RES_ADD_DATA addData)
        {
            int ret;
            Data = new WDR_TRANSMITTER_DATA_REQUEST_RES_DATA();
            addData = null;

            try
            {
                byte[] sendData = { };

                // Product category
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_PRODUCT_ID).Reverse()).ToArray();

                // identifier
                sendData = sendData.Concat(new byte[] { WDR_COMMAND }).ToArray();

                // Expansion
                sendData = sendData.Concat(new byte[] { WDR_EXPANSION }).ToArray();

                // size
                sendData = sendData.Concat(BitConverter.GetBytes((ushort)0x000B).Reverse()).ToArray();

                // Command type
                sendData = sendData.Concat(new byte[] { WDR_COMMAND_KIND_REQUEST }).ToArray();

                // IEEE address
                sendData = sendData.Concat(BitConverter.GetBytes((ulong)IEEEAddress).Reverse()).ToArray();

                // Command mode
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_COMMAND_MODE_TRANSMITTER_DATA_REQUEST).Reverse()).ToArray();

                // Send request command
                byte[] recvData;
                ret = SendCommand(WDR_COMMAND_MODE_TRANSMITTER_DATA_REQUEST, sendData, out recvData, WDR_TRANSMITTER_DATA_REQUEST_TIMEOUT);
                if (ret != 0)
                {
                    Console.WriteLine("failed to send data");
                    return -1;
                }

                // Check for response data
                if (recvData == null)
                {
                    // Exception error (including timeout) occurred
                    return -1;
                }

                // Check the response data
                if (recvData[0] == PNS_NAK)
                {
                    // Receive an abnormal response
                    Console.WriteLine("negative acknowledge");
                    return -1;
                }

                // Response status
                Data.controlState = recvData[17];

                // User name
                Data.userName = new byte[121];
                Array.Copy(recvData, 18, Data.userName, 0, Data.userName.Length);

                // version information
                Data.version = new WDR_VERSION_DATA
                {
                    major = recvData[139],      // Major version
                    minor = recvData[140],      // Minor version
                };

                // action mode
                Data.actionMode = recvData[141];

                // WDT information
                Data.wdtData = new WDR_INFO_DATA
                {
                    version = new WDR_WDT_VERSION_DATA  // version information
                    {
                        major = recvData[142],   // Major version
                        minor = recvData[143],   // Minor version
                        dummy = recvData[144]    // Fixed value
                    },
                    status = recvData[145],      // Status information
                };

                // Base unit information
                Data.baseUnitData = new WDR_BASEUNIT_DATA
                {
                    format = recvData[146],                 // Unit type
                    version = new WDR_WDT_VERSION_DATA      // version information
                    {
                        major = recvData[147],              // Major version
                        minor = recvData[148],              // Minor version
                        dummy = recvData[149]               // Fixed value
                    },
                    dipSwitch = recvData[150]               // DIP switch information
                };

                // ExtendedPanID
                Data.extendedPanID = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(recvData, 156));

                // Frequency channel
                Data.frequencyChannel = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvData, 164));

                // Signal light input judgment
                Data.signalLightInputJudge = recvData[168];

                // Power settings
                Data.powerSetting = recvData[169];

                // Counter setting
                Data.counterSetting = recvData[170];

                // Send mode
                Data.sendMode = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvData, 171));

                // In the case of WDT-6LR-Z2-PRO, the expansion structure is also set.
                if (Data.actionMode == 0xFF)
                {
                    addData = new WDR_TRANSMITTER_DATA_REQUEST_RES_ADD_DATA();

                    // Input information transmission method
                    addData.inputDataTranform = recvData[173];

                    // Signal light format
                    addData.signalLightFormat = recvData[174];

                    // Periodic transmission
                    addData.regularSend = recvData[175];

                    // Simultaneous input judgment sensitivity setting
                    addData.concInputSensitiveSetting = recvData[176];

                    // Received data file format
                    addData.recvDataFileFormat = recvData[177];

                    // Communication setting baud rate
                    addData.baudrate = recvData[178];

                    // Communication setting data length
                    addData.dataLength = recvData[179];

                    // Communication setting parity
                    addData.parity = recvData[180];

                    // Communication setting stop bit
                    addData.stopBit = recvData[181];
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Transmitter call display request / response
        /// </summary>
        /// <param name="IEEEAddress">Transmitter IEEE address</param>
        /// <param name="Data">received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_TransmitterCallRequest(ulong IEEEAddress, out WDR_TRANSMITTER_CALL_REQUEST_RES_DATA Data)
        {
            int ret;
            Data = new WDR_TRANSMITTER_CALL_REQUEST_RES_DATA();

            try
            {
                byte[] sendData = { };

                // Product category
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_PRODUCT_ID).Reverse()).ToArray();

                // identifier
                sendData = sendData.Concat(new byte[] { WDR_COMMAND }).ToArray();

                // Expansion
                sendData = sendData.Concat(new byte[] { WDR_EXPANSION }).ToArray();

                // size
                sendData = sendData.Concat(BitConverter.GetBytes((ushort)0x000B).Reverse()).ToArray();

                // Command type
                sendData = sendData.Concat(new byte[] { WDR_COMMAND_KIND_REQUEST }).ToArray();

                // IEEE address
                sendData = sendData.Concat(BitConverter.GetBytes((ulong)IEEEAddress).Reverse()).ToArray();

                // Command mode
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_COMMAND_MODE_TRANSMITTER_CALL_REQUEST).Reverse()).ToArray();

                // Send request command
                byte[] recvData;
                ret = SendCommand(WDR_COMMAND_MODE_TRANSMITTER_CALL_REQUEST, sendData, out recvData, WDR_TRANSMITTER_CALL_REQUEST_TIMEOUT);
                if (ret != 0)
                {
                    Console.WriteLine("failed to send data");
                    return -1;
                }

                // Check for response data
                if (recvData == null)
                {
                    // Exception error (including timeout) occurred
                    return -1;
                }

                // Check the response data
                if (recvData[0] == PNS_NAK)
                {
                    // Receive an abnormal response
                    Console.WriteLine("negative acknowledge");
                    return -1;
                }

                // Response status
                Data.controlState = recvData[17];

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Serial data output request / response
        /// </summary>
        /// <param name="outputData">
        /// Specification of serial number and output information
        /// Serial number (0x00 to 0xFF)
        /// Output information (0x00 to 0xFF, up to 40 bytes)
        /// </param>
        /// <param name="Data">received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_SerialOutputRequest(WDR_SERIAL_OUTPUT_REQ_DATA outputData, out WDR_SERIAL_OUTPUT_RES_DATA Data)
        {
            int ret;
            Data = new WDR_SERIAL_OUTPUT_RES_DATA();

            try
            {
                byte[] sendData = { };

                // Product category
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_PRODUCT_ID).Reverse()).ToArray();

                // identifier
                sendData = sendData.Concat(new byte[] { WDR_COMMAND }).ToArray();

                // Expansion
                sendData = sendData.Concat(new byte[] { WDR_EXPANSION }).ToArray();

                // size
                ushort sendSize = (ushort)(14 + outputData.outputData.Count);
                sendData = sendData.Concat(BitConverter.GetBytes(sendSize).Reverse()).ToArray();

                // Command type
                sendData = sendData.Concat(new byte[] { WDR_COMMAND_KIND_REQUEST }).ToArray();

                // IEEE address
                sendData = sendData.Concat(BitConverter.GetBytes((ulong)outputData.IEEEAddress).Reverse()).ToArray();

                // Command mode
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_COMMAND_MODE_SERIAL_OUTPUT_REQUEST).Reverse()).ToArray();

                // Dummy data
                sendData = sendData.Concat(BitConverter.GetBytes((ushort)0x0000).Reverse()).ToArray();

                // serial number
                sendData = sendData.Concat(new byte[] { outputData.serialNumber }).ToArray();

                // Output information
                sendData = sendData.Concat(outputData.outputData.ToArray()).ToArray();

                // Send request command
                byte[] recvData;
                ret = SendCommand(WDR_COMMAND_MODE_SERIAL_OUTPUT_REQUEST, sendData, out recvData, WDR_SERIAL_OUTPUT_REQUEST_TIMEOUT);
                if (ret != 0)
                {
                    Console.WriteLine("failed to send data");
                    return -1;
                }

                // Check for response data
                if (recvData == null)
                {
                    // Exception error (including timeout) occurred
                    return -1;
                }

                // Check the response data
                if (recvData[0] == PNS_NAK)
                {
                    // Receive an abnormal response
                    Console.WriteLine("negative acknowledge");
                    return -1;
                }

                // Response status
                Data.controlState = recvData[17];

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Signal light display control request / response
        /// </summary>
        /// <param name="controlData">
        /// Control time, LED unit lighting pattern, buzzer pattern specification
        /// Control time (no time specified: 0x00, control time specified: 0x01 to 0xFF
        /// LED unit lighting pattern (common to red, yellow, green, blue, and white: control by control line: 0x00, off: 0x10, lighting: 0x11, blinking: 0x12, triple flash: 0x13)
        /// Buzzer pattern (control by control line: 0x00, non-buzzing: 0x10, buzzing: 0x11, intermittent buzzing: 0x12)
        /// </param>
        /// <param name="Data">received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_SignalLightControlRequest(WDR_SIGNAL_LIGHT_CONTROL_REQ_DATA controlData, out WDR_SIGNAL_LIGHT_CONTROL_RES_DATA Data)
        {
            int ret;
            Data = new WDR_SIGNAL_LIGHT_CONTROL_RES_DATA();

            try
            {
                byte[] sendData = { };

                // Product category
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_PRODUCT_ID).Reverse()).ToArray();

                // identifier
                sendData = sendData.Concat(new byte[] { WDR_COMMAND }).ToArray();

                // Expansion
                sendData = sendData.Concat(new byte[] { WDR_EXPANSION }).ToArray();

                // size
                sendData = sendData.Concat(BitConverter.GetBytes((ushort)0x0012).Reverse()).ToArray();

                // Command type
                sendData = sendData.Concat(new byte[] { WDR_COMMAND_KIND_REQUEST }).ToArray();

                // IEEE address
                sendData = sendData.Concat(BitConverter.GetBytes((ulong)controlData.IEEEAddress).Reverse()).ToArray();

                // Command mode
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_COMMAND_MODE_SIGNAL_LIGHT_CONTROL_REQUEST).Reverse()).ToArray();

                // Data size, data area
                byte[] data = {
                    controlData.controlTime,    // Control time
                    controlData.redUnit,        // Red unit lighting pattern
                    controlData.yellowUnit,     // Yellow unit lighting pattern
                    controlData.greenUnit,      // Green unit lighting pattern
                    controlData.blueUnit,       // Blue unit lighting pattern
                    controlData.whiteUnit,      // White unit lighting pattern
                    controlData.buzzerUnit      // Buzzer unit pattern
                };
                sendData = sendData.Concat(data).ToArray();

                // Send request command
                byte[] recvData;
                ret = SendCommand(WDR_COMMAND_MODE_SIGNAL_LIGHT_CONTROL_RESPONSE, sendData, out recvData, WDR_SIGNAL_LIGHT_CONTROL_TIMEOUT);
                if (ret != 0)
                {
                    Console.WriteLine("failed to send data");
                    return -1;
                }

                // Check for response data
                if (recvData == null)
                {
                    // Exception error (including timeout) occurred
                    return -1;
                }

                // Check the response data
                if (recvData[0] == PNS_NAK)
                {
                    // Receive an abnormal response
                    Console.WriteLine("negative acknowledge");
                    return -1;
                }

                // Response status
                Data.recvState = recvData[17];

                // Control state
                Data.controlState = recvData[18];

                // Red unit
                Data.redUnit = recvData[19];

                // Yellow unit
                Data.yellowUnit = recvData[20];

                // Green unit
                Data.greenUnit = recvData[21];

                // Blue unit
                Data.blueUnit = recvData[22];

                // White unit
                Data.whiteUnit = recvData[23];

                // Buzzer unit
                Data.buzzerUnit = recvData[24];

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }


        /// <summary>
        /// Signal light display cancellation request / response
        /// </summary>
        /// <param name="IEEEAddress">Transmitter IEEE address</param>
        /// <param name="Data">received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_SignalLightLiftRequest(ulong IEEEAddress, out WDR_SIGNAL_LIGHT_LIFT_RES_DATA Data)
        {
            int ret;
            Data = new WDR_SIGNAL_LIGHT_LIFT_RES_DATA();

            try
            {
                byte[] sendData = { };

                // Product category
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_PRODUCT_ID).Reverse()).ToArray();

                // identifier
                sendData = sendData.Concat(new byte[] { WDR_COMMAND }).ToArray();

                // Expansion
                sendData = sendData.Concat(new byte[] { WDR_EXPANSION }).ToArray();

                // size
                sendData = sendData.Concat(BitConverter.GetBytes((ushort)0x000B).Reverse()).ToArray();

                // Command type
                sendData = sendData.Concat(new byte[] { WDR_COMMAND_KIND_REQUEST }).ToArray();

                // IEEE address
                sendData = sendData.Concat(BitConverter.GetBytes((ulong)IEEEAddress).Reverse()).ToArray();

                // Command mode
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_COMMAND_MODE_SIGNAL_LIGHT_LIFT_REQUEST).Reverse()).ToArray();

                // Send request command
                byte[] recvData;
                ret = SendCommand(WDR_COMMAND_MODE_SIGNAL_LIGHT_CONTROL_RESPONSE, sendData, out recvData, WDR_SIGNAL_LIGHT_LIFT_TIMEOUT);
                if (ret != 0)
                {
                    Console.WriteLine("failed to send data");
                    return -1;
                }

                // Check for response data
                if (recvData == null)
                {
                    // Exception error (including timeout) occurred
                    return -1;
                }

                // Check the response data
                if (recvData[0] == PNS_NAK)
                {
                    // Receive an abnormal response
                    Console.WriteLine("negative acknowledge");
                    return -1;
                }

                // Response status
                Data.recvState = recvData[17];

                // Control state
                Data.controlState = recvData[18];

                // Red unit
                Data.redUnit = recvData[19];

                // Yellow unit
                Data.yellowUnit = recvData[20];

                // Green unit
                Data.greenUnit = recvData[21];

                // Blue unit
                Data.blueUnit = recvData[22];

                // White unit
                Data.whiteUnit = recvData[23];

                // Buzzer unit
                Data.buzzerUnit = recvData[24];

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Count value registration request / response
        /// </summary>
        /// <param name="setData">
        /// Specifying the count registration value
        /// Count registration value (0x00000000 to 0xFFFFFFFF)
        /// </param>
        /// <param name="Data">received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_SignalLightCountSetRequest(WDR_SIGNAL_LIGHT_COUNT_SET_REQ_DATA setData, out WDR_SIGNAL_LIGHT_COUNT_SET_RES_DATA Data)
        {
            int ret;
            Data = new WDR_SIGNAL_LIGHT_COUNT_SET_RES_DATA();

            try
            {
                byte[] sendData = { };

                // Product category
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_PRODUCT_ID).Reverse()).ToArray();

                // identifier
                sendData = sendData.Concat(new byte[] { WDR_COMMAND }).ToArray();

                // Expansion
                sendData = sendData.Concat(new byte[] { WDR_EXPANSION }).ToArray();

                // size
                sendData = sendData.Concat(BitConverter.GetBytes((ushort)0x000F).Reverse()).ToArray();

                // Command type
                sendData = sendData.Concat(new byte[] { WDR_COMMAND_KIND_REQUEST }).ToArray();

                // IEEE address
                sendData = sendData.Concat(BitConverter.GetBytes((ulong)setData.IEEEAddress).Reverse()).ToArray();

                // Command mode
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_COMMAND_MODE_SIGNAL_LIGHT_COUNT_SET_REQUEST).Reverse()).ToArray();

                // Count registration value
                sendData = sendData.Concat(BitConverter.GetBytes(setData.setCount).Reverse()).ToArray();

                // Send request command
                byte[] recvData;
                ret = SendCommand(WDR_COMMAND_MODE_SIGNAL_LIGHT_COUNT_SET_REQUEST, sendData, out recvData, WDR_SIGNAL_LIGHT_COUNT_SET_TIMEOUT);
                if (ret != 0)
                {
                    Console.WriteLine("failed to send data");
                    return -1;
                }

                // Check for response data
                if (recvData == null)
                {
                    // Exception error (including timeout) occurred
                    return -1;
                }

                // Check the response data
                if (recvData[0] == PNS_NAK)
                {
                    // Receive an abnormal response
                    Console.WriteLine("negative acknowledge");
                    return -1;
                }

                // Response status
                Data.controlState = recvData[17];

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }


        /// <summary>
        /// Receiver information acquisition request / response
        /// </summary>
        /// <param name="Data">received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_ReceiveDataRequest(out WDR_RECEIVER_DATA_REQUEST_RES_DATA Data)
        {
            int ret;
            Data = new WDR_RECEIVER_DATA_REQUEST_RES_DATA();

            try
            {
                byte[] sendData = { };

                // Product category
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_PRODUCT_ID).Reverse()).ToArray();

                // identifier
                sendData = sendData.Concat(new byte[] { WDR_COMMAND }).ToArray();

                // Expansion
                sendData = sendData.Concat(new byte[] { WDR_EXPANSION }).ToArray();

                // size
                sendData = sendData.Concat(BitConverter.GetBytes((ushort)0x000B).Reverse()).ToArray();

                // Command type
                sendData = sendData.Concat(new byte[] { WDR_COMMAND_KIND_REQUEST }).ToArray();

                // IEEE address
                byte[] IEEEEdata = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                sendData = sendData.Concat(IEEEEdata).ToArray();

                // Command mode
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_COMMAND_MODE_RECEIVER_DATA_REQUEST).Reverse()).ToArray();

                // Send request command
                byte[] recvData;
                ret = SendCommand(WDR_COMMAND_MODE_RECEIVER_DATA_REQUEST, sendData, out recvData, WDR_RECEIVER_DATA_REQUEST_TIMEOUT);
                if (ret != 0)
                {
                    Console.WriteLine("failed to send data");
                    return -1;
                }

                // Check for response data
                if (recvData == null)
                {
                    // Exception error (including timeout) occurred
                    return -1;
                }

                // Check the response data
                if (recvData[0] == PNS_NAK)
                {
                    // Receive an abnormal response
                    Console.WriteLine("negative acknowledge");
                    return -1;
                }

                // Response status
                Data.controlState = recvData[17];

                // ExtendedPanID
                Data.extendedPanID = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(recvData, 18));

                // Frequency channel
                Data.frequencyChannel = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvData, 26));

                // Firmware version
                Data.version = new WDR_VERSION_DATA
                {
                    major = recvData[30],      // Major version
                    minor = recvData[31],      // Minor version
                };

                // Network status
                Data.networkStatus = recvData[32];

                // How to boot the network
                Data.networkBoot = recvData[33];

                // Running ExtendedPanID
                Data.actionExtendedPanID = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(recvData, 34));

                // Operating frequency channel
                Data.actionFrequencyChannel = recvData[42];

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Receiver reset request / response
        /// </summary>
        /// <param name="Data">received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_ReceiverResetRequest(out WDR_RECEIVER_RESET_RES_DATA Data)
        {
            int ret;
            Data = new WDR_RECEIVER_RESET_RES_DATA();

            try
            {
                byte[] sendData = { };

                // Product category
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_PRODUCT_ID).Reverse()).ToArray();

                // identifier
                sendData = sendData.Concat(new byte[] { WDR_COMMAND }).ToArray();

                // Expansion
                sendData = sendData.Concat(new byte[] { WDR_EXPANSION }).ToArray();

                // size
                sendData = sendData.Concat(BitConverter.GetBytes((ushort)0x000B).Reverse()).ToArray();

                // Command type
                sendData = sendData.Concat(new byte[] { WDR_COMMAND_KIND_REQUEST }).ToArray();

                // IEEE address
                byte[] IEEEEdata = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                sendData = sendData.Concat(IEEEEdata).ToArray();

                // Command mode
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_COMMAND_MODE_RECEIVER_RESET_REQUEST).Reverse()).ToArray();

                // Send request command
                byte[] recvData;
                ret = SendCommand(WDR_COMMAND_MODE_RECEIVER_RESET_REQUEST, sendData, out recvData, WDR_RECEIVER_RESET_REQUEST_TIMEOUT);
                if (ret != 0)
                {
                    Console.WriteLine("failed to send data");
                    return -1;
                }

                // Check for response data
                if (recvData == null)
                {
                    // Exception error (including timeout) occurred
                    return -1;
                }

                // Check the response data
                if (recvData[0] == PNS_NAK)
                {
                    // Receive an abnormal response
                    Console.WriteLine("negative acknowledge");
                    return -1;
                }

                // Response status
                Data.controlState = recvData[17];

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Count value acquisition request / response
        /// </summary>
        /// <param name="IEEEAddress">Transmitter IEEE address</param>
        /// <param name="Data">received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_SignalLightCountGetRequest(ulong IEEEAddress, out WDR_SIGNAL_LIGHT_COUNT_GET_RES_DATA Data)
        {
            int ret;
            Data = new WDR_SIGNAL_LIGHT_COUNT_GET_RES_DATA();

            try
            {
                byte[] sendData = { };

                // Product category
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_PRODUCT_ID).Reverse()).ToArray();

                // identifier
                sendData = sendData.Concat(new byte[] { WDR_COMMAND }).ToArray();

                // Expansion
                sendData = sendData.Concat(new byte[] { WDR_EXPANSION }).ToArray();

                // size
                sendData = sendData.Concat(BitConverter.GetBytes((ushort)0x000B).Reverse()).ToArray();

                // Command type
                sendData = sendData.Concat(new byte[] { WDR_COMMAND_KIND_REQUEST }).ToArray();

                // IEEE address
                sendData = sendData.Concat(BitConverter.GetBytes((ulong)IEEEAddress).Reverse()).ToArray();

                // Command mode
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_COMMAND_MODE_SIGNAL_LIGHT_COUNT_GET_REQUEST).Reverse()).ToArray();

                // Send request command
                byte[] recvData;
                ret = SendCommand(WDR_COMMAND_MODE_SIGNAL_LIGHT_COUNT_GET_REQUEST, sendData, out recvData, WDR_SIGNAL_LIGHT_COUNT_GET_TIMEOUT);
                if (ret != 0)
                {
                    Console.WriteLine("failed to send data");
                    return -1;
                }

                // Check for response data
                if (recvData == null)
                {
                    // Exception error (including timeout) occurred
                    return -1;
                }

                // Check the response data
                if (recvData[0] == PNS_NAK)
                {
                    // Receive an abnormal response
                    Console.WriteLine("negative acknowledge");
                    return -1;
                }

                // Response status
                Data.controlState = recvData[17];

                // Time information
                Data.time = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(recvData, 22));

                // version information
                Data.version = new WDR_VERSION_DATA
                {
                    major = recvData[30],      // Major version
                    minor = recvData[31],      // Minor version
                };

                // action mode
                Data.actionMode = recvData[32];

                // WDT information
                Data.wdtData = new WDR_INFO_DATA
                {
                    version = new WDR_WDT_VERSION_DATA  // version information
                    {
                        major = recvData[33],   // Major version
                        minor = recvData[34],   // Minor version
                        dummy = recvData[35]    // Fixed value
                    },
                    status = recvData[36],      // Status information
                };

                // Base unit information
                Data.baseUnitData = new WDR_BASEUNIT_DATA
                {
                    format = recvData[37],                  // Unit type
                    version = new WDR_WDT_VERSION_DATA      // version information
                    {
                        major = recvData[38],               // Major version
                        minor = recvData[39],               // Minor version
                        dummy = recvData[40]                // Fixed value
                    },
                    dipSwitch = recvData[41]                // DIP switch information
                };

                // Count value
                Data.count = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvData, 47));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Signal light display acquisition request / response
        /// </summary>
        /// <param name="IEEEAddress">Transmitter IEEE address</param>
        /// <param name="Data">received data</param>
        /// <returns>Success: 0, Failure: Other than 0</returns>
        public static int WDR_SignalLightDataGetRequest(ulong IEEEAddress, out WDR_SIGNAL_LIGHT_DATA_GET_RES_DATA Data)
        {
            int ret;
            Data = new WDR_SIGNAL_LIGHT_DATA_GET_RES_DATA();

            try
            {
                byte[] sendData = { };

                // Product category
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_PRODUCT_ID).Reverse()).ToArray();

                // identifier
                sendData = sendData.Concat(new byte[] { WDR_COMMAND }).ToArray();

                // Expansion
                sendData = sendData.Concat(new byte[] { WDR_EXPANSION }).ToArray();

                // size
                sendData = sendData.Concat(BitConverter.GetBytes((ushort)0x000B).Reverse()).ToArray();

                // Command type
                sendData = sendData.Concat(new byte[] { WDR_COMMAND_KIND_REQUEST }).ToArray();

                // IEEE address
                sendData = sendData.Concat(BitConverter.GetBytes((ulong)IEEEAddress).Reverse()).ToArray();

                // Command mode
                sendData = sendData.Concat(BitConverter.GetBytes(WDR_COMMAND_MODE_SIGNAL_LIGHT_DATA_GET_REQUEST).Reverse()).ToArray();

                // Send request command
                byte[] recvData;
                ret = SendCommand(WDR_COMMAND_MODE_SIGNAL_LIGHT_DATA_GET_REQUEST, sendData, out recvData, WDR_SIGNAL_LIGHT_DATA_GET_TIMEOUT);
                if (ret != 0)
                {
                    Console.WriteLine("failed to send data");
                    return -1;
                }

                // Check for response data
                if (recvData == null)
                {
                    // Exception error (including timeout) occurred
                    return -1;
                }

                // Check the response data
                if (recvData[0] == PNS_NAK)
                {
                    // Receive an abnormal response
                    Console.WriteLine("negative acknowledge");
                    return -1;
                }

                // Response status
                Data.controlState = recvData[17];

                // Time information
                Data.time = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(recvData, 22));

                // version information
                Data.version = new WDR_VERSION_DATA
                {
                    major = recvData[30],      // Major version
                    minor = recvData[31],      // Minor version
                };

                // action mode
                Data.actionMode = recvData[32];

                // WDT information
                Data.wdtData = new WDR_INFO_DATA
                {
                    version = new WDR_WDT_VERSION_DATA  // version information
                    {
                        major = recvData[33],   // Major version
                        minor = recvData[34],   // Minor version
                        dummy = recvData[35]    // Fixed value
                    },
                    status = recvData[36],      // Status information
                };

                // Base unit information
                Data.baseUnitData = new WDR_BASEUNIT_DATA
                {
                    format = recvData[37],                  // Unit type
                    version = new WDR_WDT_VERSION_DATA      // version information
                    {
                        major = recvData[38],               // Major version
                        minor = recvData[39],               // Minor version
                        dummy = recvData[40]                // Fixed value
                    },
                    dipSwitch = recvData[41]                // DIP switch information
                };

                // Red unit
                Data.redUnit = recvData[47];

                // Yellow unit
                Data.yellowUnit = recvData[48];

                // Green unit
                Data.greenUnit = recvData[49];

                // Blue unit
                Data.blueUnit = recvData[50];

                // White unit
                Data.whiteUnit = recvData[51];

                // Buzzer unit
                Data.buzzerUnit = recvData[52];

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }

    }
}
