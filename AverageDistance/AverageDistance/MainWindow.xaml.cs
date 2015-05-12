using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AverageDistance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor nui = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeNui();
        }

        private void InitializeNui()
        {
            this.nui = KinectSensor.KinectSensors[0];
            nui.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            nui.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(nui_DepthFrameReady);
            nui.SkeletonStream.Enable();
            
        }

        private void nui_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame ImagePara = e.OpenDepthImageFrame())
            {
                if (ImagePara == null) return;

                short[] ImageBits = new short[ImagePara.PixelDataLength];
                ImagePara.CopyPixelDataTo(ImageBits);

                WriteableBitmap wb = new WriteableBitmap(ImagePara.Width, ImagePara.Height, 96, 96, PixelFormats.Bgr32, null);
                wb.WritePixels(new Int32Rect(0, 0, ImagePara.Width, ImagePara.Height), Getplayer(ImagePara, ImageBits, ((KinectSensor)sender).DepthStream), ImagePara.Width * 4, 0);
                this.img1.Source = wb;

                //int stride = ImagePara.Width * ImagePara.BytesPerPixel;
                //this.img1.Source = BitmapSource.Create(ImagePara.Width, ImagePara.Height, 96, 96, PixelFormats.Gray16, null, ImageBits, stride);
                


            }
        }

       byte[] Getplayer(DepthImageFrame pImage, short[] depthFrame, DepthImageStream depthStream)
        {
            byte[] playerCoded = new byte[pImage.Width * pImage.Height * 4];
            long lPixel = 0;
            long lDist = 0;
            int nPlayer = -1;

            for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < playerCoded.Length; i16++, i32 += 4)
            {
                
                int player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;
                
                int nDistance = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                setRGB(playerCoded, i32, 0x00, 0x00, 0x00);

                if (player > 0 && nPlayer <= 0) nPlayer = player;
                
                if (player == nPlayer)
                {
                    if (nDistance < depthStream.TooFarDepth && nDistance > depthStream.TooFarDepth)
                    {
                        lDist += nDistance;
                        lPixel += 1;
                        setRGB(playerCoded, i32, 0xFF, 0xFF, 0xFF);
                    }
                }

                //Console.WriteLine(nPlayer);
                if (nPlayer > 0)
                {
                    
                    txtNumberOfPixel.Text = lPixel.ToString();
                    if (lPixel != 0)
                    {
                        txtAvgDistance.Text = (lDist / lPixel).ToString();
                    }
                    
                }

            }

            return playerCoded;
        }

        private void setRGB(byte[] nPlayers, int nPos, byte r, byte g, byte b)
        {
            nPlayers[nPos + 2] = r;
            nPlayers[nPos + 1] = g;
            nPlayers[nPos + 0] = b;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            nui.Start();
        }
    }
}
