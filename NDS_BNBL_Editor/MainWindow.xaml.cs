using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NDS_BNBL_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (App.ExeArgs.Length > 0)
            {
                loadBNBLfile(App.ExeArgs[0]);
            }
        }

        Button[] objn_button = new Button[256];
        bool allowedToCreateButtons = true;
        byte[] objn_xPos = new byte[256];
        byte[] objn_yPos = new byte[256];
        byte[] objn_width = new byte[256];
        byte[] objn_height = new byte[256];

        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.Filter = "BNBL files|*.bnbl";
            bool? openFileDlg_result = openFileDlg.ShowDialog();

            if (openFileDlg_result == true)
            {
                allowedToCreateButtons = false;

                for (int i = 1; i < objn_button.Length; i++)
                {
                    WindowGrid.Children.Remove(objn_button[i]);
                    objn_button[i] = null;
                }

                loadBNBLfile(openFileDlg.FileName);
            }
        }

        public void loadBNBLfile(string BNBLpath)
        {
            allowedToCreateButtons = false;

            BinaryReader openedFileData_reader = new BinaryReader(File.Open(BNBLpath, FileMode.Open));
            string v1 = Encoding.UTF8.GetString(openedFileData_reader.ReadBytes(4));

            openedFileData_reader.BaseStream.Position = 0x6;
            numberOfTouchObjs_UpDown.Value = openedFileData_reader.ReadByte();

            if (v1 != "JNBL")
            {
                MessageBox.Show("This is not a valid BNBL file!", "Ups!", MessageBoxButton.OK, MessageBoxImage.Information);
                openedFileData_reader.Close();
                return;
            }

            for (int i = 1; i <= numberOfTouchObjs_UpDown.Value; i++)
            {
                objn_button[i] = new Button()
                {
                    Opacity = 0.75,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Content = string.Format("Object {0}", i),
                    Name = string.Format("ObjectN_Button_{0}", i),
                    Tag = i
                };
                objn_button[i].Click += new RoutedEventHandler(objn_Click);
                WindowGrid.Children.Add(objn_button[i]);

                openedFileData_reader.BaseStream.Position = 0x8 - 0x6 + (i * 0x6);
                ushort xPosUInt16 = openedFileData_reader.ReadUInt16();
                byte xPosUInt12 = (byte)(xPosUInt16 & 0xFFF);
                byte xPosAlignmentByte = (byte)(xPosUInt16 >> 12 & 3);
                objn_xPos[i] = xPosUInt12;

                openedFileData_reader.BaseStream.Position = 0xA - 0x6 + (i * 0x6);
                ushort yPosUInt16 = openedFileData_reader.ReadUInt16();
                byte yPosUInt12 = (byte)(yPosUInt16 & 0xFFF);
                byte yPosAlignmentByte = (byte)(yPosUInt16 >> 12 & 3);
                objn_yPos[i] = yPosUInt12;

                openedFileData_reader.BaseStream.Position = 0xC - 0x6 + (i * 0x6);
                byte widthByte = openedFileData_reader.ReadByte();
                objn_width[i] = widthByte;

                openedFileData_reader.BaseStream.Position = 0xD - 0x6 + (i * 0x6);
                byte heightByte = openedFileData_reader.ReadByte();
                objn_height[i] = heightByte;

                //Do some calculations
                if (xPosAlignmentByte == 1) //If X is centered
                {
                    objn_xPos[i] -= (byte)((widthByte + 1) / 2);
                }
                else if (xPosAlignmentByte == 2) //If X is set to Bottom/Right
                {
                    objn_xPos[i] -= widthByte;
                }

                if (yPosAlignmentByte == 1) //If Y is centered
                {
                    objn_yPos[i] -= (byte)((heightByte + 1) / 2);
                }
                else if (yPosAlignmentByte == 2) //If Y is set to Bottom/Right
                {
                    objn_yPos[i] -= heightByte;
                }

                objn_button[i].Margin = new Thickness(objn_xPos[i] + 264, objn_yPos[i] + 93, 0, 0);
                objn_button[i].Width = objn_width[i];
                objn_button[i].Height = objn_height[i];

                xPos_UpDown.Value = objn_xPos[1];
                yPos_UpDown.Value = objn_yPos[1];
                width_UpDown.Value = objn_width[1];
                height_UpDown.Value = objn_height[1];

                objn_button[1].Background = Brushes.Yellow;
            }
            openedFileData_reader.Close();
            currentTouchObj_UpDown.Value = 1;
            allowedToCreateButtons = true;
        }

        void objn_Click(object sender, RoutedEventArgs e)
        {
            currentTouchObj_UpDown.Value = (int)(sender as Button).Tag;
        }

        private void ObjectClicked(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if ( currentTouchObj_UpDown != null && numberOfTouchObjs_UpDown != null)
            {
                currentTouchObj_UpDown.Maximum = numberOfTouchObjs_UpDown.Value;
                if(currentTouchObj_UpDown.Value > currentTouchObj_UpDown.Maximum && currentTouchObj_UpDown.Value != 1)
                {
                    currentTouchObj_UpDown.Value = currentTouchObj_UpDown.Maximum;
                }

                if(xPos_UpDown != null && yPos_UpDown != null && width_UpDown != null && height_UpDown != null)
                {
                    xPos_UpDown.Value = objn_xPos[(byte)currentTouchObj_UpDown.Value];
                    yPos_UpDown.Value = objn_yPos[(byte)currentTouchObj_UpDown.Value];
                    width_UpDown.Value = objn_width[(byte)currentTouchObj_UpDown.Value];
                    height_UpDown.Value = objn_height[(byte)currentTouchObj_UpDown.Value];
                }

                for (int i = 1; i < objn_button.Length; i++)
                {
                    if(objn_button[i] != null)
                    {
                        if (currentTouchObj_UpDown.Value == i) { objn_button[i].Background = Brushes.Yellow; }
                        else
                        {
                            objn_button[i].Background = new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
                        }

                        if (i > numberOfTouchObjs_UpDown.Value)
                        {
                            WindowGrid.Children.Remove(objn_button[i]);
                            objn_button[i] = null;
                        }
                    }
                    else if (objn_button[i] == null && i <= numberOfTouchObjs_UpDown.Value)
                    {
                        if(allowedToCreateButtons == true)
                        {
                            objn_button[i] = new Button()
                            {
                                Width = 75,
                                Height = 50,
                                Margin = new Thickness(264, 93, 0, 0),
                                Opacity = 0.75,
                                VerticalAlignment = VerticalAlignment.Top,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Content = string.Format("Object {0}", i),
                                Name = string.Format("ObjectN_Button_{0}", i),
                                Tag = i
                            };
                            objn_button[i].Click += new RoutedEventHandler(objn_Click);
                            WindowGrid.Children.Add(objn_button[i]);

                            objn_xPos[i] = 0;
                            objn_yPos[i] = 0;
                            objn_width[i] = 75;
                            objn_height[i] = 50;
                        }
                    }
                }
            }
        }

        private void openImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.Filter = "All supported image formats|*bmp; *gif; *.jpg; *.jpeg; *.jpe; *.jif; *.jfif; *.jfi; *png; *.tiff; *.tif|" +
                "Bitmap images|*.bmp|" +
                "GIF images|*.gif|" +
                "JPEG images|*.jpg; *.jpeg; *.jpe; *.jif; *.jfif; *.jfi|" +
                "PNG images|*.png|" +
                "TIFF images|*.tiff; *.tif|" +
                "All files|*.*";
            bool? openFileDlg_result = openFileDlg.ShowDialog();

            if (openFileDlg_result == true)
            {
                guideImage.Source = new BitmapImage(new Uri(openFileDlg.FileName));
            }
        }

        async private void valueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            byte v1 = (byte)currentTouchObj_UpDown.Value;
            if (objn_button[v1] != null)
            {
                await Task.Delay(1);
                objn_xPos[v1] = (byte)xPos_UpDown.Value;
                objn_yPos[v1] = (byte)yPos_UpDown.Value;
                objn_height[v1] = (byte)height_UpDown.Value;
                objn_width[v1] = (byte)width_UpDown.Value;

                objn_button[v1].Margin = new Thickness(objn_xPos[v1] + 264, objn_yPos[v1] + 93, 0, 0);
                objn_button[v1].Width = objn_width[v1];
                objn_button[v1].Height = objn_height[v1];
            }

            for (int i = 1; i < objn_button.Length; i++)
            {
                if(objn_button[i] != null)
                {
                    if (objn_width[i] <= 48)
                    {
                        objn_button[i].Content = i;
                    }

                    if (objn_width[i] > 48)
                    {
                        objn_button[i].Content = string.Format("Object {0}", i);
                    }
                }
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            new About().ShowDialog();
        }

        private void saveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "BNBL files|*.bnbl";
            bool? saveFileDialog1_result = saveFileDialog1.ShowDialog();

            if(saveFileDialog1_result == true)
            {
                BinaryWriter saveFileFromDialog_writer = new BinaryWriter(File.Open(saveFileDialog1.FileName, FileMode.Create));

                byte[] JNBL = Encoding.ASCII.GetBytes("JNBL");
                for (int i = 0; i < JNBL.Length; i++)
                {
                    saveFileFromDialog_writer.BaseStream.WriteByte(JNBL[i]);
                    saveFileFromDialog_writer.BaseStream.Position = 0x1 + i;
                }

                saveFileFromDialog_writer.BaseStream.Position = 0x6;
                saveFileFromDialog_writer.BaseStream.WriteByte((byte)numberOfTouchObjs_UpDown.Value);

                if (numberOfTouchObjs_UpDown.Value == 0)
                {
                    saveFileFromDialog_writer.Close();
                    return;
                }

                for (int i = 1; i <= numberOfTouchObjs_UpDown.Value; i++)
                {
                    saveFileFromDialog_writer.BaseStream.Position = 0x8 - 0x6 + (i * 0x6);
                    saveFileFromDialog_writer.BaseStream.WriteByte(objn_xPos[i]);
                    saveFileFromDialog_writer.BaseStream.Position = 0xA - 0x6 + (i * 0x6);
                    saveFileFromDialog_writer.BaseStream.WriteByte(objn_yPos[i]);
                    saveFileFromDialog_writer.BaseStream.Position = 0xC - 0x6 + (i * 0x6);
                    saveFileFromDialog_writer.BaseStream.WriteByte(objn_width[i]);
                    saveFileFromDialog_writer.BaseStream.Position = 0xD - 0x6 + (i * 0x6);
                    saveFileFromDialog_writer.BaseStream.WriteByte(objn_height[i]);
                }

                saveFileFromDialog_writer.Close();
            }
        }
    }
}