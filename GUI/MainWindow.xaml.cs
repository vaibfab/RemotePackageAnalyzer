using Environ;
using MessagePassingComm;
using Navigator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CodeAnalysis;
using System.Windows.Media;
using System.Threading.Tasks;

namespace GUI
{
    /// <summary>
    /// File: MainWindow.xaml
    /// Version: 1
    /// Developed by: Vaibhav Kumar, Syracuse University
    /// Date created: 10-April-2018
    /// Objective: To perform interaction logic for MainWindow.xaml
    /// ------------------------------------------------------------
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private IFileMgr fileMgr { get; set; } = null;  // note: Navigator just uses interface declarations
        Comm comm { get; set; } = null;

        List<string> selectedFiles = new List<string>();
        Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        Thread rcvThread = null;
        public MainWindow()
        {
            InitializeComponent();
            initializeEnvironment();
            //Console.Title = "Navigator Client";
            fileMgr = FileMgrFactory.create(FileMgrType.Local); // uses Environment
            comm = new Comm(ClientEnvironment.address, ClientEnvironment.port);
            initializeMessageDispatcher();
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
            AutomatedTests();
        }

        void rcvThreadProc()
        {
            Console.Write("\n  starting client's receive thread");
            while (true)
            {
                CommMessage msg = comm.getMessage();
                msg.show();
                if (msg.command == null)
                    continue;

                // pass the Dispatcher's action value to the main thread for execution

                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }
        void initializeEnvironment()
        {
            Environ.Environment.root = ClientEnvironment.root;
            Environ.Environment.address = ClientEnvironment.address;
            Environ.Environment.port = ClientEnvironment.port;
            Environ.Environment.endPoint = ClientEnvironment.endPoint;
        }
        void initializeMessageDispatcher()
        {
            messageDispatcher["connectCS"] = (CommMessage msg) =>{Console.WriteLine("Acknowledgement from server\n\n"); msg.show();BindFileExplorer(msg.arguments);            };
            messageDispatcher["analyze"] = (CommMessage msg) =>{Console.WriteLine("Acknowledgement from server\n\n");PopulateOutput(msg.arguments);};
            messageDispatcher["moveIntoFolderFiles"] = (CommMessage msg) =>{TreeViewItem item = new TreeViewItem();                List<string> files = msg.arguments.ToList<string>();
                foreach (string file in files.ToList<string>())
                    if (!(file.ToLower().Contains(".cs")) || (file.ToLower().Contains(".csproj")))
                        files.RemoveAt(files.IndexOf(file));
                foreach (TreeViewItem objTreeviewItem in lstClient.Items)
                {
                    if ((objTreeviewItem.Tag.ToString().ToLower() == msg.parentFolder.ToLower()))
                    {
                        item = objTreeviewItem;
                        break;
                    }
                }
                List<string> dirs = new List<string>();
                files.ForEach(dirPath =>
                {
                    var subItem = new TreeViewItem()
                    {
                        Header = GetFolderName(dirPath),
                        Tag = dirPath
                    };
                    item.Items.Add(subItem);
                });
            };
            messageDispatcher["moveIntoFolderDirs"] = (CommMessage msg) =>{TreeViewItem item = new TreeViewItem();
                foreach (TreeViewItem objTreeviewItem in lstClient.Items)
                {
                    if ((objTreeviewItem.Header.ToString().ToLower() == msg.parentFolder.ToLower()))
                    {
                        item = objTreeviewItem;
                        break;
                    }
                }
                List<string> dirs = new List<string>();
                msg.arguments.ForEach(dirPath =>
                {
                    var subItem = new TreeViewItem()
                    {
                        Header = GetFolderName(dirPath),
                        Tag = dirPath
                    };
                    subItem.Items.Add(null);
                    subItem.Expanded += ShowChildren;
                    item.Items.Add(subItem);
                });
            };
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            comm.close();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        //----< not currently being used >-------------------------------
        void reset()
        {
            clearTabContent();
            tbDepAnal.Visibility = Visibility.Hidden;
            tbScc.Visibility = Visibility.Hidden;
            tbTypeAnal.Visibility = Visibility.Hidden;
            lstClient.Items.Clear();
            selectedFiles.Clear();
            lstSelectedFiles.Items.Clear();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        void PopulateOutput(List<string> result)
        {
            string typeTables = result[0];
            string depResult = result[1];
            string scc = result[2];
            PopulateTypeAnalysis(typeTables);
            PopulateDepAnalysis(depResult);
            PopulateSCC(scc);
        }

        void PopulateSCC(string scc)
        {
            List<string> lstSCC = scc.Split('%').ToList<string>();
            HashSet<List<string>> sccData = new HashSet<List<string>>();
            foreach (string oScc in lstSCC)
            {
                List<string> lstComp = oScc.Split(',').ToList<string>();
                sccData.Add(lstComp);
            }
            TextBlock pageHeader = new TextBlock();
            pageHeader.Text = "Strong Components";
            pageHeader.VerticalAlignment = VerticalAlignment.Center;
            pageHeader.HorizontalAlignment = HorizontalAlignment.Center;
            pageHeader.FontSize = 20;
            pageHeader.FontWeight = FontWeights.UltraBold;
            pageHeader.Foreground = Brushes.LightSkyBlue;
            stackPanelScc.Children.Add(pageHeader);
            if (sccData.Count > 0)
            {
                foreach (List<string> sccSet in sccData)
                    if (sccSet.Count > 2)
                        BuildSCC(sccSet);
            }
            else
            {
                TextBlock txtMsg = new TextBlock();
                txtMsg.Text = "No set of strongly connected components with more than one component in it.";
                stackPanelScc.Children.Add(txtMsg);
            }
        }
        void PopulateTypeAnalysis(string typeTables)
        {
            Dictionary<string, List<Elem>> oTable = new Dictionary<string, List<Elem>>();
            List<string> Tables = typeTables.Split('%').ToList<string>();
            foreach (string tab in Tables)
            {
                if (!String.IsNullOrEmpty(tab))
                {
                    string[] parsed = tab.Split(':');
                    string key = parsed[0];
                    List<string> elements = parsed[1].Split('|').ToList<string>();
                    List<Elem> valueList = new List<Elem>();
                    foreach (string element in elements)
                    {
                        if (!String.IsNullOrEmpty(element))
                        {
                            Elem item = new Elem();
                            string[] elemParsed = element.Split('@');
                            item.name = elemParsed[0];
                            item.type = elemParsed[1];
                            valueList.Add(item);
                        }
                    }
                    oTable.Add(key, valueList);
                }
            }
            TextBlock pageHeader = new TextBlock();
            pageHeader.Text = "Type Analysis";
            pageHeader.VerticalAlignment = VerticalAlignment.Center;
            pageHeader.HorizontalAlignment = HorizontalAlignment.Center;
            pageHeader.FontSize = 20;
            pageHeader.FontWeight = FontWeights.UltraBold;
            pageHeader.Foreground = Brushes.LightSkyBlue;
            stackPanel.Children.Add(pageHeader);
            BuildTable(oTable);

        }

        void BuildTable(Dictionary<string, List<Elem>> dicTable)
        {
            Grid parent = new Grid();
            CustomizeParentGrid(ref parent);
            StackPanel runTime = new StackPanel();
            runTime.Height = 700;
            foreach (KeyValuePair<string, List<Elem>> table in dicTable)
            {
                runTime.Children.Add(MakeHeaderGrid());
                Grid DynamicGrid = new Grid();
                CustomizeDynamicGrid(ref DynamicGrid, table.Value.Count);
                int row = -1;
                foreach (Elem item in table.Value)
                {
                    int col = 0;
                    ++row;
                    TextBlock txtFilename = new TextBlock();
                    txtFilename.HorizontalAlignment = HorizontalAlignment.Center;
                    txtFilename.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetRow(txtFilename, row);
                    Grid.SetColumn(txtFilename, col);
                    txtFilename.Text = GetTrimmedName(table.Key);
                    DynamicGrid.Children.Add(txtFilename);
                    ++col;
                    TextBlock txtType = new TextBlock();
                    Grid.SetRow(txtType, row);
                    Grid.SetColumn(txtType, col);
                    txtType.Text = item.type;
                    txtType.HorizontalAlignment = HorizontalAlignment.Center;
                    txtType.VerticalAlignment = VerticalAlignment.Center;
                    DynamicGrid.Children.Add(txtType);
                    ++col;
                    TextBlock txtName = new TextBlock();
                    Grid.SetRow(txtName, row);
                    Grid.SetColumn(txtName, col);
                    txtName.Text = item.name;
                    txtName.HorizontalAlignment = HorizontalAlignment.Center;
                    txtName.VerticalAlignment = VerticalAlignment.Center;
                    DynamicGrid.Children.Add(txtName);
                    ++col;
                }
                runTime.Children.Add(DynamicGrid);
            }
            Grid.SetColumn(runTime, 0);
            Grid.SetColumn(runTime, 0);
            ScrollViewer scr = new ScrollViewer();
            scr.Content = runTime;
            parent.Children.Add(scr);
            stackPanel.Children.Add(parent);
        }

        void CustomizeDynamicGrid(ref Grid DynamicGrid, int count)
        {
            DynamicGrid.Width = 500;
            DynamicGrid.HorizontalAlignment = HorizontalAlignment.Left;
            DynamicGrid.VerticalAlignment = VerticalAlignment.Top;
            DynamicGrid.ShowGridLines = true;
            DynamicGrid.Background = new SolidColorBrush(Colors.White);
            for (int i = 0; i < 3; i++)
            {
                ColumnDefinition gridCol1 = new ColumnDefinition();
                DynamicGrid.ColumnDefinitions.Add(gridCol1);
            }
            for (int i = 1; i <= count; i++)
            {
                RowDefinition gridRow = new RowDefinition();
                gridRow.Height = new GridLength(20);
                DynamicGrid.RowDefinitions.Add(gridRow);
            }

        }
        void CustomizeParentGrid(ref Grid parent)
        {
            ColumnDefinition pCol = new ColumnDefinition();
            RowDefinition pRow = new RowDefinition();
            pRow.Height = new GridLength(550);
            pCol.Width = new GridLength(500);
            parent.ColumnDefinitions.Add(pCol);
            parent.RowDefinitions.Add(pRow);
        }
        Grid MakeHeaderGrid()
        {

            Grid HeaderGrid = new Grid();
            HeaderGrid.Width = 500;
            for (int i = 0; i < 3; i++)
            {
                ColumnDefinition gridCol = new ColumnDefinition();
                HeaderGrid.ColumnDefinitions.Add(gridCol);
            }
            RowDefinition gridRow1 = new RowDefinition();
            gridRow1.Height = new GridLength(25);
            HeaderGrid.RowDefinitions.Add(gridRow1);
            HeaderGrid.Background = new SolidColorBrush(Colors.LightSkyBlue);
            TextBlock hdrFilename = new TextBlock();
            hdrFilename.Foreground = Brushes.White;
            hdrFilename.FontWeight = FontWeights.UltraBold;
            Grid.SetRow(hdrFilename, 0);
            Grid.SetColumn(hdrFilename, 0);
            hdrFilename.Text = "Filename";
            hdrFilename.VerticalAlignment = VerticalAlignment.Center;
            hdrFilename.HorizontalAlignment = HorizontalAlignment.Center;
            HeaderGrid.Children.Add(hdrFilename);
            TextBlock hdrCat = new TextBlock();
            hdrCat.Foreground = Brushes.White;
            hdrCat.FontWeight = FontWeights.UltraBold;
            hdrCat.VerticalAlignment = VerticalAlignment.Center;
            hdrCat.HorizontalAlignment = HorizontalAlignment.Center;
            Grid.SetRow(hdrCat, 0);
            Grid.SetColumn(hdrCat, 1);
            hdrCat.Text = "Category";
            HeaderGrid.Children.Add(hdrCat);
            TextBlock hdrName = new TextBlock();
            hdrName.Foreground = Brushes.White;
            hdrName.FontWeight = FontWeights.UltraBold;
            hdrName.VerticalAlignment = VerticalAlignment.Center;
            hdrName.HorizontalAlignment = HorizontalAlignment.Center;
            Grid.SetRow(hdrName, 0);
            Grid.SetColumn(hdrName, 2);
            hdrName.Text = "Name";
            HeaderGrid.Children.Add(hdrName);
            return HeaderGrid;
        }
        void BuildDep(KeyValuePair<string, HashSet<string>> depRes)
        {
            Border container = new Border();
            container.BorderBrush = new SolidColorBrush(Colors.LightSkyBlue);
            container.BorderThickness = new Thickness(2, 2, 2, 2);
            container.CornerRadius = new CornerRadius(15);
            container.Width = 700;
            #region Grid
            Grid depGrid = new Grid();
            depGrid.HorizontalAlignment = HorizontalAlignment.Center;
            depGrid.VerticalAlignment = VerticalAlignment.Center;
            depGrid.Width = 500;
            RowDefinition gridRow1 = new RowDefinition();
            gridRow1.Height = new GridLength(40);
            depGrid.RowDefinitions.Add(gridRow1);
            ColumnDefinition col = new ColumnDefinition();
            depGrid.ColumnDefinitions.Add(col);
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append(depRes.Key).Append(" dependes on ");
            foreach (string dependency in depRes.Value)
                result.Append(GetTrimmedName(dependency)).Append(" ");
            TextBlock txtRes = new TextBlock();
            txtRes.Text = result.ToString();
            txtRes.HorizontalAlignment = HorizontalAlignment.Left;
            txtRes.VerticalAlignment = VerticalAlignment.Center;
            txtRes.Foreground = Brushes.Black;
            Grid.SetColumn(txtRes, 0);
            Grid.SetColumn(txtRes, 0);
            depGrid.Children.Add(txtRes);
            #endregion
            container.Child = depGrid;
            stackPanelDep.Children.Add(container);
        }
        void BuildSCC(List<string> sccData)
        {
            Border container = new Border();
            container.BorderBrush = new SolidColorBrush(Colors.LightSkyBlue);
            container.BorderThickness = new Thickness(2, 2, 2, 2);
            container.CornerRadius = new CornerRadius(15);
            container.Width = 700;
            Grid depGrid = new Grid();
            depGrid.HorizontalAlignment = HorizontalAlignment.Center;
            depGrid.VerticalAlignment = VerticalAlignment.Center;
            depGrid.Width = 500;
            RowDefinition gridRow1 = new RowDefinition();
            gridRow1.Height = new GridLength(40);
            depGrid.RowDefinitions.Add(gridRow1);
            ColumnDefinition col = new ColumnDefinition();
            depGrid.ColumnDefinitions.Add(col);
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            foreach (string scc in sccData)
            {
                if (!String.IsNullOrEmpty(scc))
                    result.Append(GetTrimmedName(scc)).Append(" ");
            }
            TextBlock txtRes = new TextBlock();
            txtRes.Text = result.ToString();
            txtRes.HorizontalAlignment = HorizontalAlignment.Left;
            txtRes.VerticalAlignment = VerticalAlignment.Center;
            txtRes.Foreground = Brushes.Black;
            Grid.SetColumn(txtRes, 0);
            Grid.SetColumn(txtRes, 0);
            depGrid.Children.Add(txtRes);
            container.Child = depGrid;
            stackPanelScc.Children.Add(container);
        }
        void PopulateDepAnalysis(string depResult)
        {
            Dictionary<string, HashSet<string>> depData = new Dictionary<string, HashSet<string>>();

            List<string> Dependencies = depResult.Split('%').ToList<string>();
            foreach (string depen in Dependencies)
            {
                if (!String.IsNullOrEmpty(depen))
                {
                    string[] parsedDepen = depen.Split(':');
                    string key = parsedDepen[0];
                    HashSet<string> valueList = new HashSet<string>((parsedDepen[1].Split(',').ToList<string>()));
                    depData.Add(GetTrimmedName(key), valueList);
                }
            }
            TextBlock pageHeader = new TextBlock();
            pageHeader.Text = "Dependency Analysis";
            pageHeader.VerticalAlignment = VerticalAlignment.Center;
            pageHeader.HorizontalAlignment = HorizontalAlignment.Center;
            pageHeader.FontSize = 20;
            pageHeader.FontWeight = FontWeights.UltraBold;
            pageHeader.Foreground = Brushes.LightSkyBlue;
            stackPanelDep.Children.Add(pageHeader);
            foreach (KeyValuePair<string, HashSet<string>> depRes in depData)
                BuildDep(depRes);
        }
        /// <summary>
        /// function: btnConnect_Click
        /// Developed by: Vaibhav Kumar. Syracuse University
        /// Date: 10-April-2018
        /// Objective: Event handler for btnConnect to connect to the server
        /// </summary>
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {

            if (btnConnect.Content.ToString() == "Connect")
            {
                tbSelection.Visibility = Visibility.Visible;
                CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
                msg1.from = ClientEnvironment.endPoint;
                msg1.to = ServerEnvironment.endPoint;
                msg1.author = "Vaibhav Kumar";
                msg1.command = "connectCS";
                msg1.arguments.Add("");
                comm.postMessage(msg1);
                btnConnect.Content = "Disconnect";
                tbControl.SelectedIndex = 1;
            }
            else
            {
                reset();
                tbSelection.Visibility = Visibility.Hidden;
                btnConnect.Content = "Connect";
            }

        }

        public void BindSelectedFiles()
        {
            lstSelectedFiles.Items.Clear();
            foreach (string file in selectedFiles)
            {
                lstSelectedFiles.Items.Add(file);
            }
        }
        /// <summary>
        /// function: tabVisible
        /// Developed by: Vaibhav Kumar. Syracuse University
        /// Date: 10-April-2018
        /// Objective: Makes all the tabs visible.
        /// </summary>
        void tabVisible()
        {
            tbSelection.Visibility = Visibility.Visible;
            tbTypeAnal.Visibility = Visibility.Visible;
            tbScc.Visibility = Visibility.Visible;
            tbDepAnal.Visibility = Visibility.Visible;
        }

        void BindFileExplorer(List<string> lstPath)
        {
            if (lstPath.Count > 0)
            {
                foreach (string folder in lstPath)
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = GetFolderName(folder);
                    item.Tag = folder;
                    item.Items.Add(null);
                    item.Expanded += ShowChildren;
                    lstClient.Items.Add(item);
                }
            }
        }


        private void ShowChildren(object sender, RoutedEventArgs e)
        {
            TreeViewItem item;
            if (e != null)
                item = (TreeViewItem)e.Source;
            else
                item = (TreeViewItem)sender;
            if (item.Items.Count > 0)
            {
                if (item.Items[0] == null)//item.Items.Count == 1 && 
                {
                    item.Items.Clear();
                    CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
                    msg1.from = ClientEnvironment.endPoint;
                    msg1.to = ServerEnvironment.endPoint;
                    msg1.author = "Vaibhav Kumar";
                    msg1.command = "moveIntoFolderFiles";
                    msg1.arguments.Add(item.Tag.ToString());
                    msg1.parentFolder = item.Tag.ToString();
                    comm.postMessage(msg1);

                    CommMessage msg2 = new CommMessage(CommMessage.MessageType.request);
                    msg2.from = ClientEnvironment.endPoint;
                    msg2.to = ServerEnvironment.endPoint;
                    msg2.author = "Vaibhav Kumar";
                    msg2.command = "moveIntoFolderDirs";
                    msg2.arguments.Add(item.Tag.ToString());
                    msg2.parentFolder = item.Tag.ToString();
                    comm.postMessage(msg2);
                }
            }
        }

        public static string GetFolderName(string dirPath)
        {
            if (String.IsNullOrEmpty(dirPath))
                return String.Empty;
            //string folderName = dirPath.Replace('/', '\\');
            int index = dirPath.LastIndexOf('/');
            if (index <= 0)
                return dirPath;
            string newName = dirPath.Remove(index);
            if (index <= 0)
                return newName;
            index = newName.LastIndexOf('/');
            return newName.Substring(index + 1);
        }

        public static string GetTrimmedName(string dirPath)
        {
            if (String.IsNullOrEmpty(dirPath))
                return String.Empty;
            int index = dirPath.LastIndexOf('/');
            if (index <= 0)
                return dirPath;

            return dirPath.Substring(index + 1);
        }
        private string removeFirstDir(string path)
        {
            string modifiedPath = path;
            int pos = path.IndexOf("/");
            modifiedPath = path.Substring(pos + 1, path.Length - pos - 1);
            return modifiedPath;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox oBox = (CheckBox)e.Source;
            if (!(((TextBlock)(oBox.Content)).Text.ToString().Contains(".cs")))
            {
                MessageBox.Show("Folder selection not implemented. Please select files individually");
                oBox.IsChecked = false;
                return;
            }
            string filename = String.Empty;
            filename = ((TextBlock)oBox.Content).DataContext.ToString();
            if (!selectedFiles.Contains(filename))
            {
                selectedFiles.Add(filename);
                BindSelectedFiles();
            }

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox oBox = (CheckBox)e.Source;
            string filename = String.Empty;
            filename = ((TextBlock)oBox.Content).DataContext.ToString();
            if (selectedFiles.Contains(filename))
            {
                selectedFiles.RemoveAt(selectedFiles.IndexOf(filename));
                BindSelectedFiles();
            }
        }

        private void btnAnal_Click(object sender, RoutedEventArgs e)
        {
            clearTabContent();
            if (selectedFiles.Count > 0)
            {
                CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
                msg1.from = ClientEnvironment.endPoint;
                msg1.to = ServerEnvironment.endPoint;
                msg1.author = "Vaibhav Kumar";
                msg1.command = "analyze";
                foreach (string filename in selectedFiles)
                    msg1.arguments.Add(ServerEnvironment.root + filename);
                comm.postMessage(msg1);
                tabVisible();
            }
            else
                MessageBox.Show("Please select file(s) for analysis");
        }

        private void btnTypeAnal_Click(object sender, RoutedEventArgs e)
        {
            clearTabContent();
            tbTypeAnal.Visibility = Visibility.Visible;
            tbDepAnal.Visibility = Visibility.Collapsed;
            tbScc.Visibility = Visibility.Collapsed;
            tbControl.SelectedIndex = 2;
            if (selectedFiles.Count > 0)
            {
                CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
                msg1.from = ClientEnvironment.endPoint;
                msg1.to = ServerEnvironment.endPoint;
                msg1.author = "Vaibhav Kumar";
                msg1.command = "analyze";
                foreach (string filename in selectedFiles)
                    msg1.arguments.Add(ServerEnvironment.root + filename);
                comm.postMessage(msg1);
            }
            else
                MessageBox.Show("Please select file(s) for analysis");
        }

        private void btnSCC_Click(object sender, RoutedEventArgs e)
        {
            clearTabContent();
            tbTypeAnal.Visibility = Visibility.Collapsed;
            tbDepAnal.Visibility = Visibility.Collapsed;
            tbScc.Visibility = Visibility.Visible;
            tbControl.SelectedIndex = 4;
            if (selectedFiles.Count > 0)
            {
                CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
                msg1.from = ClientEnvironment.endPoint;
                msg1.to = ServerEnvironment.endPoint;
                msg1.author = "Vaibhav Kumar";
                msg1.command = "analyze";
                foreach (string filename in selectedFiles)
                    msg1.arguments.Add(ServerEnvironment.root + filename);
                comm.postMessage(msg1);
            }
            else
                MessageBox.Show("Please select file(s) for analysis");
        }

        void clearTabContent()
        {
            stackPanelScc.Children.Clear();
            stackPanel.Children.Clear();
            stackPanelDep.Children.Clear();
        }
        private void btnDepAnal_Click(object sender, RoutedEventArgs e)
        {
            clearTabContent();
            tbTypeAnal.Visibility = Visibility.Collapsed;
            tbDepAnal.Visibility = Visibility.Visible;
            tbScc.Visibility = Visibility.Collapsed;
            tbControl.SelectedIndex = 3;
            if (selectedFiles.Count > 0)
            {
                CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
                msg1.from = ClientEnvironment.endPoint;
                msg1.to = ServerEnvironment.endPoint;
                msg1.author = "Vaibhav Kumar";
                msg1.command = "analyze";
                foreach (string filename in selectedFiles)
                    msg1.arguments.Add(ServerEnvironment.root + filename);
                comm.postMessage(msg1);
            }
            else
                MessageBox.Show("Please select file(s) for analysis");
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            reset();
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Vaibhav Kumar";
            msg1.command = "connectCS";
            msg1.arguments.Add("");
            comm.postMessage(msg1);
        }

        /// <summary>
        /// function: AutomatedTests
        /// Developed by: Vaibhav Kumar. Syracuse University
        /// Date: 5th-Dec-2018
        /// Objective: Performs automated test for the application without user interference
        /// </summary>
        private async void AutomatedTests()
        {
            TimedMsgBox.Show("Initiating Automated Demonstration. It will take about 50 seconds to complete.", "Auto Demo", 4000);
            tbControl.SelectedIndex = 0;
            await Task.Delay(2000);
            btnConnect_Click(btnConnect, null);
            TimedMsgBox.Show("Connection complete\nDemonstrating file selection", "Auto Demo", 4000);
            tbControl.SelectedIndex = 1;
            await Task.Delay(2000);
            if (lstClient.Items.Count > 0)
            {
                TreeViewItem item = (TreeViewItem)lstClient.Items[0];
                item.IsExpanded = true;
                item.ExpandSubtree();
                selectedFiles.Add("File1.cs");
                selectedFiles.Add("File2.cs");
                selectedFiles.Add("File3.cs");
                selectedFiles.Add("File4.cs");
                selectedFiles.Add("File5.cs");
                selectedFiles.Add("File6.cs");
                BindSelectedFiles();
                await Task.Delay(4000);
                btnTypeAnal_Click(btnTypeAnal, null);
                await Task.Delay(4000);
                TimedMsgBox.Show("Type Analysis complete\n Demonstrating Dependency Analysis", "Auto Demo", 4000);
                tbControl.SelectedIndex = 1;
                await Task.Delay(4000);
                btnDepAnal_Click(btnDepAnal, null);
                await Task.Delay(4000);
                TimedMsgBox.Show("Dependency Analysis complete\n Demonstrating Strong Component", "Auto Demo", 4000);
                tbControl.SelectedIndex = 1;
                btnSCC_Click(btnSCC, null);
                await Task.Delay(5000);
                TimedMsgBox.Show("Strong Component complete\n Demonstrating Total Analysis", "Auto Demo", 4000);
                btnAnal_Click(btnAnal, null);
                await Task.Delay(4000);
                tbControl.SelectedIndex = 2;
                await Task.Delay(4000);
                tbControl.SelectedIndex = 3;
                await Task.Delay(4000);
                tbControl.SelectedIndex = 4;
                TimedMsgBox.Show("Demonstration Complete!", "Auto Demo", 4000);
            }
            else
            {
                MessageBox.Show("No files present in the root folder");
                this.Close();
            }

        }

    }
    class TimedMsgBox
    {
        System.Threading.Timer _timeoutTimer;
        string _caption;
        TimedMsgBox(string text, string caption, int timeout)
        {
            _caption = caption;
            _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                null, timeout, System.Threading.Timeout.Infinite);
            MessageBox.Show(text, caption);
        }
        public static void Show(string text, string caption, int timeout)
        {
            new TimedMsgBox(text, caption, timeout);
        }
        void OnTimerElapsed(object state)
        {
            IntPtr mbWnd = FindWindow(null, _caption);
            if (mbWnd != IntPtr.Zero)
                SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            _timeoutTimer.Dispose();
        }
        const int WM_CLOSE = 0x0010;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
    }
}
