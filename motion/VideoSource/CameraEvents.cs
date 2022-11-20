// Motion Detector
//
// Copyright © Andrew Kirillov, 2005
// andrew.kirillov@gmail.com
//
namespace VideoSource
{
    using AForge.Imaging.Filters;
    using OtsuThreshold;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    // NewFrame delegate
    public delegate void CameraEventHandler(object sender, CameraEventArgs e);

	/// <summary>
	/// Camera event arguments
	/// </summary>
	public class CameraEventArgs : EventArgs
	{
		private System.Drawing.Bitmap bmp;

		// Constructor
		public CameraEventArgs(System.Drawing.Bitmap bmp)
		{
            Bitmap temp = (Bitmap)bmp.Clone();
            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory+"Frames\\f0.bmp") || File.Exists(AppDomain.CurrentDomain.BaseDirectory+"Frames\\f00.bmp"))
            {
                Bitmap srcImage = (Bitmap)Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "Frames\\f0.bmp", true);
                ThresholdedEuclideanDifference skThreshold = new ThresholdedEuclideanDifference(60);
                skThreshold.OverlayImage = srcImage;
                Bitmap extractedImage = skThreshold.Apply(temp);
                Erosion e = new Erosion();
                Bitmap temp1 = e.Apply(extractedImage);
                Bitmap temp2 = Dilate(temp1);
                this.bmp = temp2;
            }
            else if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Frames\\f0") && !(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Frames\\f01.bmp") || File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Frames\\f0.bmp")))
            {
                Bitmap srcImage = (Bitmap)Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "Frames\\f0", true);
                ThresholdedEuclideanDifference skThreshold = new ThresholdedEuclideanDifference(60);
                skThreshold.OverlayImage = srcImage;
                Bitmap extractedImage = skThreshold.Apply(temp);
                Erosion e = new Erosion();
                Bitmap temp1 = e.Apply(extractedImage);
                Bitmap temp2 = Dilate(temp1);
                this.bmp = temp2;
            }
            else
            {
                this.bmp = bmp;
            }
            
		}

		// Bitmap property
		public System.Drawing.Bitmap Bitmap
		{
			get { return bmp; }
		}

       
        public static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            using (Graphics g = Graphics.FromImage(newBitmap))
            {

                //create the grayscale ColorMatrix
                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                   {
             new float[] {.3f, .3f, .3f, 0, 0},
             new float[] {.59f, .59f, .59f, 0, 0},
             new float[] {.11f, .11f, .11f, 0, 0},
             new float[] {0, 0, 0, 1, 0},
             new float[] {0, 0, 0, 0, 1}
                   });

                //create some image attributes
                using (ImageAttributes attributes = new ImageAttributes())
                {

                    //set the color matrix attribute
                    attributes.SetColorMatrix(colorMatrix);

                    //draw the original image on the new image
                    //using the grayscale color matrix
                    g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }

        public Bitmap Dilate(Bitmap SrcImage)
        {
            // Create Destination bitmap.
            Bitmap tempbmp = new Bitmap(SrcImage.Width, SrcImage.Height);

            // Take source bitmap data.
            BitmapData SrcData = SrcImage.LockBits(new Rectangle(0, 0,
                SrcImage.Width, SrcImage.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            // Take destination bitmap data.
            BitmapData DestData = tempbmp.LockBits(new Rectangle(0, 0, tempbmp.Width,
                tempbmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // Element array to used to dilate.
            byte[,] sElement = new byte[5, 5] {
        {0,0,1,0,0},
        {0,1,1,1,0},
        {1,1,1,1,1},
        {0,1,1,1,0},
        {0,0,1,0,0}
    };

            // Element array size.
            int size = 5;
            byte max, clrValue;
            int radius = size / 2;
            int ir, jr;

            unsafe
            {

                // Loop for Columns.
                for (int colm = radius; colm < DestData.Height - radius; colm++)
                {
                    // Initialise pointers to at row start.
                    byte* ptr = (byte*)SrcData.Scan0 + (colm * SrcData.Stride);
                    byte* dstPtr = (byte*)DestData.Scan0 + (colm * SrcData.Stride);

                    // Loop for Row item.
                    for (int row = radius; row < DestData.Width - radius; row++)
                    {
                        max = 0;
                        clrValue = 0;

                        // Loops for element array.
                        for (int eleColm = 0; eleColm < 5; eleColm++)
                        {
                            ir = eleColm - radius;
                            byte* tempPtr = (byte*)SrcData.Scan0 +
                                ((colm + ir) * SrcData.Stride);

                            for (int eleRow = 0; eleRow < 5; eleRow++)
                            {
                                jr = eleRow - radius;

                                // Get neightbour element color value.
                                clrValue = (byte)((tempPtr[row * 3 + jr] +
                                    tempPtr[row * 3 + jr + 1] + tempPtr[row * 3 + jr + 2]) / 3);

                                if (max < clrValue)
                                {
                                    if (sElement[eleColm, eleRow] != 0)
                                        max = clrValue;
                                }
                            }
                        }

                        dstPtr[0] = dstPtr[1] = dstPtr[2] = max;

                        ptr += 3;
                        dstPtr += 3;
                    }
                }
            }

            // Dispose all Bitmap data.
            SrcImage.UnlockBits(SrcData);
            tempbmp.UnlockBits(DestData);

            // return dilated bitmap.
            return tempbmp;
        }


    }
}