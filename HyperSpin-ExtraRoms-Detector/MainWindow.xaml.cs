using System;
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using System.Collections.Generic;
using System.IO;

namespace HyperSpin_ExtraRoms_Detector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void open_xml_button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "XML Files (.xml)|*.xml";
            dialog.FilterIndex = 1;
            
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                xml_file_box.Text = dialog.FileName;
            }
        }

        private void open_scan_folder_button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                scan_folder_file_box.Text = dialog.SelectedPath;
            }
        }

        private void start_scan_button_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument xml_file = new XmlDocument();

            // Load the file
            try { xml_file.Load(xml_file_box.Text); }
            catch
            {
                result_box.Text = "Invalid XML File!";
                return;
            }

            // Check that the folder exists
            if (!Directory.Exists(scan_folder_file_box.Text))
            {
                result_box.Text = "Invalid Scan Folder!";
                return;
            }

            // Parse the XML to get the game names
            XmlNodeList xml_data = xml_file.GetElementsByTagName("game");
            List<string> game_names_xml = new List<string>();
            foreach (XmlNode game_data in xml_data)
                game_names_xml.Add(game_data.Attributes.GetNamedItem("name").Value);

            // Get file names in the folder
            string[] folder_data = Directory.GetFiles(scan_folder_file_box.Text);
            List<string> game_names_folder = new List<string>();

            // Clean file names, leaving just game names
            for (int i = 0; i < folder_data.Length; ++i)
            {
                int last_slash_index = folder_data[i].LastIndexOf("\\");
                int last_dot_index = folder_data[i].LastIndexOf('.');
                game_names_folder.Add( folder_data[i].Substring(last_slash_index + 1, last_dot_index - last_slash_index - 1) );
            }

            // Using XML data, remove matching folder data
            // Left with any file names (games in folder) that are not in the XML
            foreach (string game in game_names_xml)
                game_names_folder.Remove(game);

            // Print result
            if (game_names_folder.Count > 0)
            {
                // Create backup directory
                Directory.CreateDirectory(scan_folder_file_box.Text + "\\ExtraRoms");

                result_box.Text = "The following games were extra and moved to \"ExtraRoms\":" + Environment.NewLine;
                foreach (string game in game_names_folder)
                {
                    // Add message to the text box
                    result_box.AppendText(game + Environment.NewLine);

                    // Find full file name (with extension)
                    String[] path1 = Directory.GetFiles(scan_folder_file_box.Text, game + "*");
                    if (path1.Length > 1 || path1.Length == 0)
                        result_box.Text = "Found " + game + " twice!  Stopping search!";

                    // Setup move path
                    int last_dot_index = path1[0].LastIndexOf('.');
                    String path2 = scan_folder_file_box.Text + "\\ExtraRoms\\" + game + path1[0].Substring(last_dot_index);

                    // Move file
                    Directory.Move(path1[0], path2);
                }
            }
            else
                result_box.Text = "You have no games in the folder that are not in the XML";
        }
    }
}
