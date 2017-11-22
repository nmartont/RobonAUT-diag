using BlueToothDesktop.Serial;
using GamePad.PadHandler;
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
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int lineLimit = 500;
        private string newLine = "\n";
        private string[] PortNames;
        private LSTBlueToothHandler BlueToothHandler;
        private GamePadHandler GamePHandler;
        private string[] PadNames;
        private bool PerformanceMode = false;

        public MainWindow()
        {
            InitializeComponent();
            RefreshPortDropDown();
            dropdownPorts.SelectedIndex = PortNames.Length - 2;

            // create SerialHandler
            BlueToothHandler = new LSTBlueToothHandler(this);

            // create GamePad handler
            GamePHandler = new GamePadHandler(BlueToothHandler);
            RefreshPadDropDown();
            dropdownPads.SelectedIndex = 0;

            SetBindings();

            PerformanceMode = (bool)cbPerformance.IsChecked;

            AppendLog("LST BlueTooth Client ready...");
        }
        
        private void SetBindings()
        {
            // set bindings to UI elements
            Binding NotConnectedBinding = new Binding();
            NotConnectedBinding.Source = BlueToothHandler;
            NotConnectedBinding.Path = new PropertyPath("NotConnected");
            NotConnectedBinding.Mode = BindingMode.OneWay;
            NotConnectedBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            Binding ConnectedBinding = new Binding();
            ConnectedBinding.Source = BlueToothHandler;
            ConnectedBinding.Path = new PropertyPath("IsConnected");
            ConnectedBinding.Mode = BindingMode.OneWay;
            ConnectedBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            Binding NotConnectedPadBinding = new Binding();
            NotConnectedPadBinding.Source = GamePHandler;
            NotConnectedPadBinding.Path = new PropertyPath("NotConnected");
            NotConnectedPadBinding.Mode = BindingMode.OneWay;
            NotConnectedPadBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            Binding ConnectedPadBinding = new Binding();
            ConnectedPadBinding.Source = GamePHandler;
            ConnectedPadBinding.Path = new PropertyPath("IsConnected");
            ConnectedPadBinding.Mode = BindingMode.OneWay;
            ConnectedPadBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            dropdownPorts.SetBinding(IsEnabledProperty, NotConnectedBinding);
            refreshPortsBtn.SetBinding(IsEnabledProperty, NotConnectedBinding);
            buttonConnect.SetBinding(IsEnabledProperty, NotConnectedBinding);
            buttonDisconnect.SetBinding(IsEnabledProperty, ConnectedBinding);
            dropdownPads.SetBinding(IsEnabledProperty, NotConnectedPadBinding);
            refreshPadsBtn.SetBinding(IsEnabledProperty, NotConnectedPadBinding);
            buttonPadConnect.SetBinding(IsEnabledProperty, NotConnectedPadBinding);
            buttonPadDisconnect.SetBinding(IsEnabledProperty, ConnectedPadBinding);
            monitorStartButton.SetBinding(IsEnabledProperty, ConnectedBinding);
            monitorStopButton.SetBinding(IsEnabledProperty, ConnectedBinding);
            statusButton.SetBinding(IsEnabledProperty, ConnectedBinding);
            varListButton.SetBinding(IsEnabledProperty, ConnectedBinding);

            // bind tables
            varListView.DataContext = BlueToothHandler.VarTypeList;
            varDataTable.DataContext = BlueToothHandler.VarData.DefaultView;

            varDataTable.EnableColumnVirtualization = true;
            varDataTable.EnableRowVirtualization = true;
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
            BlueToothHandler.Connect(portName);
        }

        private void buttondisconnect_Click(object sender, RoutedEventArgs e)
        {
            disconnectFromPort();
        }

        private void disconnectFromPort()
        {
            BlueToothHandler.Disconnect();
        }


        private void buttonPadConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GamePHandler.ConnectJoystick(dropdownPads.Text);
            }
            catch { }
        }

        private void buttonPadDisconnect_Click(object sender, RoutedEventArgs e)
        {
            GamePHandler.ReleaseJoystick();
        }

        private void refreshPadsBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshPadDropDown();
        }

        private void statusButton_Click(object sender, RoutedEventArgs e)
        {
            BlueToothHandler.SendStatusRequest();
        }

        private void varListButton_Click(object sender, RoutedEventArgs e)
        {
            BlueToothHandler.SendVarListRequest();
        }

        private void monitorStartButton_Click(object sender, RoutedEventArgs e)
        {
            BlueToothHandler.SendMonitorStartRequest();
        }

        private void monitorStopButton_Click(object sender, RoutedEventArgs e)
        {
            BlueToothHandler.SendMonitorStopRequest();
        }
        
        private void cbPerformance_Checked(object sender, RoutedEventArgs e)
        {
            PerformanceMode = true;
        }

        private void cbPerformance_Unchecked(object sender, RoutedEventArgs e)
        {
            PerformanceMode = false;
        }

        // helpers
        private void RefreshPortDropDown()
        {
            // get COM ports, set it to the dropdown
            PortNames = System.IO.Ports.SerialPort.GetPortNames();
            dropdownPorts.ItemsSource = PortNames;
        }

        private void RefreshPadDropDown()
        {
            // get gamepads, set it to the dropdown
            PadNames = GamePHandler.GetGuids();
            dropdownPads.ItemsSource = PadNames;
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
                // if (logData) log.Info(textToAppend);

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
                BlueToothHandler.VarData.Columns.Add(colName);
                varDataTable.Columns.Add(new DataGridTextColumn() { Binding = new Binding(colName), Header = colName });
            }));
        }

        public void AddRow(params object[] values)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                // logging
                string s = "";

                foreach (object o in values)
                {
                    s += o.ToString() + ";";
                }
                log.Info(s);

                // add value to the datatable
                BlueToothHandler.VarData.Rows.Add(values);

                if (!PerformanceMode)
                {
                    // scroll to bottom
                    if (varDataTable.Items.Count > 0)
                    {
                        var border = VisualTreeHelper.GetChild(varDataTable, 0) as Decorator;
                        if (border != null)
                        {
                            var scroll = border.Child as ScrollViewer;
                            if (scroll != null) scroll.ScrollToEnd();
                        }
                    }
                }
                else
                {
                    // update statusbar with fresh data
                    lblData.Text = s;
                }
            }));
        }

        public void ClearColumns()
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                BlueToothHandler.VarData.Columns.Clear();
                varDataTable.Columns.Clear();
            }));
        }

        public void SetPadControlText(string v)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                lblPadControl.Text = v;
            }));
        }

    }
    
}
