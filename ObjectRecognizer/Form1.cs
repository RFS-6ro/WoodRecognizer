using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;

namespace ObjectRecognizer
{
    public partial class Form1 : Form
    {
        private bool _isPicture = true;

        private string _outputMessage = @"Площадь леса составит {0} квадратных метров";

        #region image   

        private Image<Bgr, byte> _inputImage = null;

        #endregion

        #region video

        private VideoCapture _capture = null;

        private double _frames;
        private double _framesCounter;

        private bool _isPlaying = false;

        #endregion
        
        #region FirstInitializing

        public Form1()
        {
            InitializeComponent();
            SetVideoControls(false);
            SetImageControls(false);
            checkBox1.Enabled = false;
            toolStripButton2.Enabled = false;
            toolStripButton3.Enabled = false;
            numericUpDown2.Enabled = false;
            label2.Enabled = false;
            label3.Enabled = false;
            numericUpDown4.Enabled = false;
            numericUpDown5.Enabled = false;
            richTextBox1.Enabled = false;

            начатьАнализToolStripMenuItem.Enabled = false;
            сохранитьКакToolStripMenuItem.Enabled = false;
        }

        private void SetVideoControls(bool state)
        {
            toolStripButton4.Enabled = state;
            toolStripButton5.Enabled = state;
            toolStripButton6.Enabled = state;
            toolStripButton7.Enabled = state;
            numericUpDown1.Enabled = state;
            progressBar1.Enabled = state;

            if (state)
            {
                checkBox1.Enabled = true;
                toolStripButton2.Enabled = true;
                начатьАнализToolStripMenuItem.Enabled = true;
                сохранитьКакToolStripMenuItem.Enabled = true;
                toolStripButton3.Enabled = true;
                numericUpDown2.Enabled = true;
                label2.Enabled = true;
                label3.Enabled = true;
                numericUpDown4.Enabled = true;
                numericUpDown5.Enabled = true;
                richTextBox1.Enabled = true;
            }
        }

        private void SetImageControls(bool state)
        {
            label1.Enabled = state;
            if (state)
            {
                начатьАнализToolStripMenuItem.Enabled = true;
                сохранитьКакToolStripMenuItem.Enabled = true;

                checkBox1.Enabled = true;
                toolStripButton2.Enabled = true;
                toolStripButton3.Enabled = true;
                numericUpDown2.Enabled = true;
                label2.Enabled = true;
                label3.Enabled = true;
                numericUpDown4.Enabled = true;
                numericUpDown5.Enabled = true;
                richTextBox1.Enabled = true;
            }
        }

        #endregion

        #region FileOpening
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            //MessageBox.Show("Файл загружен.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void открытьФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void OpenFile()
        {
            try
            {
                DialogResult result = openFileDialog1.ShowDialog();

                if (result == DialogResult.OK)
                {
                    if (openFileDialog1.FileName.Contains(".jpg"))
                    {
                        SetImageControls(true);

                        _inputImage = new Image<Bgr, byte>(openFileDialog1.FileName);

                        pictureBox1.Image = _inputImage.Bitmap;

                        _isPicture = true;
                    }
                    else
                    {
                        SetVideoControls(true);
                        _isPicture = false;

                        _capture = new VideoCapture(openFileDialog1.FileName);
                        Mat mat = new Mat();
                        _capture.Read(mat);
                        pictureBox1.Image = mat.Bitmap;

                        _frames = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);

                        _framesCounter = 1;
                    }
                }
                else
                {
                    MessageBox.Show("Файл не может быть открыт.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region SaveAs
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void SaveAs()
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (_isPicture)
                    {
                        if (_inputImage == null)
                        {
                            throw new Exception("Картинка не выбрана.");
                        }

                        Image<Bgr, byte> imageToSave = new Image<Bgr, byte>(_inputImage.Bitmap);
                        new Image<Bgr, byte>(Find(imageToSave)).Save(saveFileDialog1.FileName);
                    }
                    else
                    {
                        if (_capture == null)
                        {
                            throw new Exception("Видео не выбрано.");
                        }

                        pictureBox2.Image.Save(saveFileDialog1.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region analizing

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Analize();
        }

        private void начатьАнализToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Analize();
        }

        private void Analize()
        {
            try
            {
                if (_isPicture)
                {
                    if (_inputImage == null)
                    {
                        throw new Exception("Картинка не выбрана.");
                    }

                    Image<Bgr, byte> image = new Image<Bgr, byte>(_inputImage.Bitmap);
                    pictureBox2.Image = Find(image);
                }
                else
                {
                    if (_capture == null)
                    {
                        throw new Exception("Видео не выбрано.");
                    }

                    _isPlaying = true;

                    SplitForFrames();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SplitForFrames()
        {
            Mat mat = new Mat();

            while (_isPlaying && _framesCounter < _frames)
            {
                _framesCounter += 1;

                _capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, _framesCounter);

                _capture.Read(mat);

                pictureBox1.Image = mat.Bitmap;
                Bitmap bitmap = await FindAsync(mat.ToImage<Bgr, byte>());
                pictureBox2.Image = bitmap;

                //отображение прогресса
                progressBar1.Value = (int)(100 * _framesCounter / _frames);
            }
        }
        
        private Bitmap Find(Image<Bgr, byte> image)
        {
            Image<Gray, byte> outputImage = PredProcessing(image);

            return UseEmguCV(outputImage);//PredProcessing2(new Image<Bgr, byte>(UseLaplacian(outputImage))).Bitmap;
        }

        private async Task<Bitmap> FindAsync(Image<Bgr, byte> image)
        {
            Bitmap bitmap = null;

            await Task.Run(() =>
            {
                Image<Gray, byte> outputImage = PredProcessing(image);

                bitmap = UseEmguCV(outputImage);
            });

            return bitmap;
        }
        
        private Bitmap UseEmguCV(Image<Gray, byte> image)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(image, contours, hierarchy, Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxTc89Kcos);
            
            CalculateArea(contours, image);

            if (checkBox1.Checked)
            {
                Image<Gray, byte> blackBgr = new Image<Gray, byte>(image.Width, image.Height, new Gray(0));
                CvInvoke.DrawContours(blackBgr, contours, -1, new MCvScalar(255, 255, 255), 3, Emgu.CV.CvEnum.LineType.Filled);

                return blackBgr.Bitmap;
            }
            else
            {
                CvInvoke.DrawContours(image, contours, -1, new MCvScalar(0, 0, 255), 3);

                return image.Bitmap;
            }
        }

        private Bitmap UseCanny(Image<Gray, byte> image)
        {
            return image.Canny(Convert.ToDouble(numericUpDown2.Value), 0).Bitmap;
        }

        private Bitmap UseHough(Image<Gray, byte> image)
        {
            double fullArea = 0;

            var circles = CvInvoke.HoughCircles(image, Emgu.CV.CvEnum.HoughType.Gradient, 40, 1);

            foreach (var circle in circles)
            {
                image.Draw(circle, new Gray(150), 3);
                fullArea += circle.Area;
            }

            ShowData(fullArea);

            return image.Bitmap;
        }

        private Bitmap UseSobel(Image<Gray, byte> image)
        {
            //Image<Gray, byte> _imgGray = image.Convert<Gray, byte>();
            Image<Gray, float> _imgSobel = new Image<Gray, float>(image.Width, image.Height, new Gray(0));

            _imgSobel = image.Sobel(1, 1, 3);
            return _imgSobel.Bitmap;
        }

        private Bitmap UseLaplacian(Image<Gray, byte> image)
        {
            //Image<Gray, byte> _imgGray = image.Convert<Gray, byte>();
            Image<Gray, float> _imgLaplacian = new Image<Gray, float>(image.Width, image.Height, new Gray(0));

            _imgLaplacian = image.Laplace(7);

            image.Convert<Gray, byte>().ThresholdBinary(new Gray(Convert.ToDouble(numericUpDown2.Value)), new Gray(Convert.ToDouble(numericUpDown3.Value)));

            return _imgLaplacian.Bitmap;
        }

        #endregion

        #region OutData

        private void CalculateArea(VectorOfVectorOfPoint contours, Image<Gray, byte> image)
        {
            double areaInPixels = 0;
            for (int i = 0; i < contours.Size; i++)
            {
                areaInPixels += CvInvoke.ContourArea(contours[i]);
            }

            int widthInPixels = image.Width;
            int heightInPixels = image.Height;

            double wholeAreaInPixels = widthInPixels * heightInPixels;

            double areaPercentage = areaInPixels / wholeAreaInPixels;

            double widthInMeters = Convert.ToDouble(numericUpDown4.Value);
            double heightInMeters = Convert.ToDouble(numericUpDown5.Value);
            double areaInMeters = widthInMeters * heightInMeters * areaPercentage;

            ShowData(areaInMeters);
        }

        private void ShowData(double value)
        {
            string resultAreaMessage = string.Format(_outputMessage, value);

            richTextBox1.Invoke(new Action(() => richTextBox1.Text = resultAreaMessage));
        }

        #endregion

        #region Filters
        private Image<Gray, byte> PredProcessing(Image<Bgr, byte> image)
        {
            //чб фильтр
            Image<Gray, byte> outputImage = image.
                                               Convert<Gray, byte>().
                                               ThresholdBinary(new Gray(Convert.ToDouble(numericUpDown2.Value)), new Gray(255));
            //гауссово размытие
            //outputImage._SmoothGaussian(7);
            //фильтр нерезкого маскирования
            //outputImage = UnsharpMaskingFilter(outputImage);

            return outputImage;
        }

        private Image<Gray, byte> PredProcessing2(Image<Bgr, byte> image)
        {
            //чб фильтр
            Image<Gray, byte> outputImage = image.
                                               Convert<Gray, byte>().
                                               ThresholdBinary(new Gray(Convert.ToDouble(numericUpDown3.Value)), new Gray(255));
            //гауссово размытие
            //outputImage._SmoothGaussian(7);
            //фильтр нерезкого маскирования
            //outputImage = UnsharpMaskingFilter(outputImage);

            return outputImage;
        }

        private Image<Gray, float> OrientedHighPass(Image<Gray, byte> image)
        {
            Image<Gray, float> filtered = new Image<Gray, float>(image.Size);
            Image<Gray, float> final = new Image<Gray, float>(image.Size);

            // import parameters
            int smoothOpSize = 1;
            int filterLength = 22;
            int angleStep = 1;

            // construct the kernel
            float[,,] k3 = new float[1, 5, 1]
            {
                { { -1 }, { -1 }, { 4 }, { -1 }, { -1 } },
            };
            Image<Gray, float> kRow = new Image<Gray, float>(k3);
            Image<Gray, float> kImageOriginal = kRow;
            for (int l = 0; l < filterLength - 1; ++l)
                kImageOriginal = kImageOriginal.ConcateVertical(kRow);

            // first step, smooth image
            image._SmoothGaussian(smoothOpSize);

            for (int angle = 0; angle < 180; angle += angleStep)
            {
                // Create convolution kernel
                Image<Gray, float> kImage = kImageOriginal.Rotate(angle, new Gray(0), false);
                // make sure the average of the kernel stays zero
                kImage = kImage - (kImage.GetSum().Intensity / (kImage.Width * kImage.Height));

                float[,] k = new float[kImage.Height, kImage.Width];
                Buffer.BlockCopy(kImage.Data, 0, k, 0, k.Length * sizeof(float));
                ConvolutionKernelF kernel = new ConvolutionKernelF(k);

                filtered = image.Convolution(kernel);

                final = filtered.Max(final);
            }

            return final;
        }

        private Image<Gray, byte> UnsharpMaskingFilter(Image<Gray, byte> grayImage, int value = 9)
        {
            Mat blurredImage = new Mat();
            Mat lapImage = new Mat();
            CvInvoke.MedianBlur(grayImage, blurredImage, 1);
            CvInvoke.Laplacian(blurredImage, lapImage, blurredImage.Depth);
            blurredImage -= (0.1 * value * lapImage);

            return new Image<Gray, byte>(blurredImage.Bitmap);
        }
        #endregion

        #region ToolStripMenu
        //назад
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            try
            {
                _framesCounter -= Convert.ToDouble(numericUpDown1.Value);
                //отображение прогресса
                progressBar1.Value = (int)(100 * _framesCounter / _frames);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //пауза
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            try
            {
                _isPlaying = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //продолжить просмотр
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            try
            {
                if (_capture == null)
                {
                    throw new Exception("Видео не выбрано.");
                }

                _isPlaying = true;

                SplitForFrames();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //вперед
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            try
            {
                _framesCounter += Convert.ToDouble(numericUpDown1.Value);
                //отображение прогресса
                progressBar1.Value = (int)(100 * _framesCounter / _frames);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void воспроизведениеВидеоToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAboutBox();
        }

        private void ShowAboutBox()
        {
            new AboutBox1().Show();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //none
        }
        
        //при изменении ширины
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            Analize();
        }

        //при изменении высоты
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            Analize();
        }

        #endregion

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            Analize();
        }
    }
}

