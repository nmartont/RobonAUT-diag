using BlueToothDesktop.Serial;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace BlueToothDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, LSTWindowCallback
    {
        private bool logData = true;
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int lineLimit = 500;
        private string newLine = "\n";
        private string[] PortNames;
        private LSTSerialHandler SerHandler;

        public MainWindow()
        {
            InitializeComponent();
            RefreshPortDropDown();
            dropdownPorts.SelectedIndex = PortNames.Length - 2;

            // create SerialHandler
            SerHandler = new LSTSerialHandler(this);
            
            SetBindings();

            AppendLog("LST BlueTooth Client ready...");
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
            monitorStartButton.SetBinding(IsEnabledProperty, ConnectedBinding);
            monitorStopButton.SetBinding(IsEnabledProperty, ConnectedBinding);
            statusButton.SetBinding(IsEnabledProperty, ConnectedBinding);
            varListButton.SetBinding(IsEnabledProperty, ConnectedBinding);

            // bind tables
            varListView.DataContext = SerHandler.VarTypeList;
            varDataTable.DataContext = SerHandler.VarData.DefaultView;
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

        private void statusButton_Click(object sender, RoutedEventArgs e)
        {
            SerHandler.SendStatusRequest();
        }

        private void varListButton_Click(object sender, RoutedEventArgs e)
        {
            SerHandler.SendVarListRequest();
        }

        private void monitorStartButton_Click(object sender, RoutedEventArgs e)
        {
            SerHandler.SendMonitorStartRequest();
        }

        private void monitorStopButton_Click(object sender, RoutedEventArgs e)
        {
            SerHandler.SendMonitorStopRequest();
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

                if (numLines > lineLimit)
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

        public void SetStatus(string status)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                lblStatus.Text = status;
            }));
        }

        // ugly hacks but who cares
        public void AddColumn(string colName)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                SerHandler.VarData.Columns.Add(colName);
                varDataTable.Columns.Add(new DataGridTextColumn() { Binding = new Binding(colName), Header = colName });
            }));
        }

        public void AddRow(params object[] values)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                SerHandler.VarData.Rows.Add(values);

                if (varDataTable.Items.Count > 0)
                {
                    var border = VisualTreeHelper.GetChild(varDataTable, 0) as Decorator;
                    if (border != null)
                    {
                        var scroll = border.Child as ScrollViewer;
                        if (scroll != null) scroll.ScrollToEnd();
                    }
                }
            }));
        }

        public void ClearColumns()
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                SerHandler.VarData.Columns.Clear();
                varDataTable.Columns.Clear();
            }));
        }
    }
}
