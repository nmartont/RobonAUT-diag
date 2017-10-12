using BlueToothDesktop;
using BlueToothDummyClient.Serial;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace BlueToothDummyClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, WindowCallback
    {
        private bool logData = true;
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int lineLimit = 500;
        private string newLine = "\n";
        private string[] PortNames;
        private DummySerialHandler SerHandler;

        public MainWindow()
        {
            InitializeComponent();
            RefreshPortDropDown();
            dropdownPorts.SelectedIndex = PortNames.Length - 1;
            
            // create SerialHandler
            SerHandler = new DummySerialHandler(this);
            SetBindings();
            
            AppendLog("LST BlueTooth Dummy Client ready...");
            
        }

        private void SetBindings()
        {
            // set bindings to UI elements
            Binding NotConnectedBinding = new Binding();
            NotConnectedBinding.Source = SerHandler;
            NotConnectedBinding.Path = new PropertyPath("NotConnected");
            NotConnectedBinding.Mode = BindingMode.OneWay;
            NotConnectedBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            Binding ConnectedBinding = new Binding();
            ConnectedBinding.Source = SerHandler;
            ConnectedBinding.Path = new PropertyPath("IsConnected");
            ConnectedBinding.Mode = BindingMode.OneWay;
            ConnectedBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            dropdownPorts.SetBinding(IsEnabledProperty, NotConnectedBinding);
            refreshPortsBtn.SetBinding(IsEnabledProperty, NotConnectedBinding);
            buttonConnect.SetBinding(IsEnabledProperty, NotConnectedBinding);
            buttonDisconnect.SetBinding(IsEnabledProperty, ConnectedBinding);
        }

        private void refreshPortsBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshPortDropDown();
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            connectToPort(dropdownPorts.Text);
        }

        private void connectToPort(string portName)
        {
            SerHandler.Connect(portName);
        }

        private void buttondisconnect_Click(object sender, RoutedEventArgs e)
        {
            disconnectFromPort();
        }

        private void disconnectFromPort()
        {
            SerHandler.Disconnect();
        }
        
        // helpers
        private void RefreshPortDropDown()
        {
            // get COM ports, set it to the dropdown
            PortNames = System.IO.Ports.SerialPort.GetPortNames();
            dropdownPorts.ItemsSource = PortNames;
        }
        
        // interface functions
        public void AppendLog(string toAppend, bool newLineBool = true, bool timeStamp = true, bool scrollToEnd = true)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                string textNewLine = "";
                string textTimeStamp = "";
                if (newLineBool) textNewLine = "\n";
                if (timeStamp) textTimeStamp = DateTime.Now.ToString("HH:mm:ss.ff", CultureInfo.InvariantCulture) + ": ";
                string textToAppend = textTimeStamp + toAppend + textNewLine;
                textBox.AppendText(textToAppend);

                // log
                if (logData) log.Info(textToAppend);

                // limit to 1000 lines
                string text = textBox.Text;
                int numLines = text.Length - text.Replace(newLine, string.Empty).Length;

                if(numLines > lineLimit)
                {
                    string[] lines = textBox.Text
                        .Split(newLine.ToCharArray())
                        .Skip(numLines - lineLimit)
                        .ToArray();

                    textBox.Text = string.Join(newLine, lines);
                }
                
                if (scrollToEnd) textBox.ScrollToEnd();
            }));
        }
    }

}
