//----------------------------------------------------------------------------
//  Copyright (C) 2004-2016 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
#if !__IOS__
using Emgu.CV.Cuda;
#endif
using Emgu.CV.XFeatures2D;

namespace SURFFeatureExample
{
   public static class DrawMatches
   {
      public static void FindMatch(Mat modelImage, Mat observedImage, out long matchTime, out VectorOfKeyPoint modelKeyPoints, out VectorOfKeyPoint observedKeyPoints, VectorOfVectorOfDMatch matches, out Mat mask, out Mat homography)
      {
         int k = 2;
         double uniquenessThreshold = 0.8;
         double hessianThresh = 300;
         
         Stopwatch watch;
         homography = null;

         modelKeyPoints = new VectorOfKeyPoint();
         observedKeyPoints = new VectorOfKeyPoint();

         #if !__IOS__
         if ( CudaInvoke.HasCuda)
         {
            CudaSURF surfCuda = new CudaSURF((float) hessianThresh);
            using (GpuMat gpuModelImage = new GpuMat(modelImage))
            //extract features from the object image
            using (GpuMat gpuModelKeyPoints = surfCuda.DetectKeyPointsRaw(gpuModelImage, null))
            using (GpuMat gpuModelDescriptors = surfCuda.ComputeDescriptorsRaw(gpuModelImage, null, gpuModelKeyPoints))
            using (CudaBFMatcher matcher = new CudaBFMatcher(DistanceType.L2))
            {
               surfCuda.DownloadKeypoints(gpuModelKeyPoints, modelKeyPoints);
               watch = Stopwatch.StartNew();

               // extract features from the observed image
               using (GpuMat gpuObservedImage = new GpuMat(observedImage))
               using (GpuMat gpuObservedKeyPoints = surfCuda.DetectKeyPointsRaw(gpuObservedImage, null))
               using (GpuMat gpuObservedDescriptors = surfCuda.ComputeDescriptorsRaw(gpuObservedImage, null, gpuObservedKeyPoints))
               //using (GpuMat tmp = new GpuMat())
               //using (Stream stream = new Stream())
               {
                  matcher.KnnMatch(gpuObservedDescriptors, gpuModelDescriptors, matches, k);

                  surfCuda.DownloadKeypoints(gpuObservedKeyPoints, observedKeyPoints);

                  mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                  mask.SetTo(new MCvScalar(255));
                  Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                  int nonZeroCount = CvInvoke.CountNonZero(mask);
                  if (nonZeroCount >= 4)
                  {
                     nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
                        matches, mask, 1.5, 20);
                     if (nonZeroCount >= 4)
                        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
                           observedKeyPoints, matches, mask, 2);
                  }
               }
                  watch.Stop();
               }
            }
         else
         #endif
         {
            using (UMat uModelImage = modelImage.ToUMat(AccessType.Read))
            using (UMat uObservedImage = observedImage.ToUMat(AccessType.Read))
            {
               SURF surfCPU = new SURF(hessianThresh);
               //extract features from the object image
               UMat modelDescriptors = new UMat();
               surfCPU.DetectAndCompute(uModelImage, null, modelKeyPoints, modelDescriptors, false);

               watch = Stopwatch.StartNew();

               // extract features from the observed image
               UMat observedDescriptors = new UMat();
               surfCPU.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);
               BFMatcher matcher = new BFMatcher(DistanceType.L2);
               matcher.Add(modelDescriptors);

               matcher.KnnMatch(observedDescriptors, matches, k, null);
               mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
               mask.SetTo(new MCvScalar(255));
               Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);
                    
                    int nonZeroCount = CvInvoke.CountNonZero(mask);
               if (nonZeroCount >= 4)
               {
                  nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
                     matches, mask, 1.5, 20);
                  if (nonZeroCount >= 4)
                     homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
                        observedKeyPoints, matches, mask, 2);
               }

               watch.Stop();
            }
         }
         matchTime = watch.ElapsedMilliseconds;
      }
        static Bitmap modelbp;
        /// <summary>
        /// Draw the model image and observed image, the matched features and homography projection.
        /// </summary>
        /// <param name="modelImage">The model image</param>
        /// <param name="observedImage">The observed image</param>
        /// <param name="matchTime">The output total time for computing the homography matrix.</param>
        /// <returns>The model image and observed image, the matched features and homography projection.</returns>
        public static Mat Draw(Mat modelImage, Mat observedImage, out long matchTime, out Rectangle rectan,out Bitmap res2,out double  q)
      {
         Mat homography;
         VectorOfKeyPoint modelKeyPoints;
         VectorOfKeyPoint observedKeyPoints; res2 = null;
            q = 0;
            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
         {
            Mat mask;
                FindMatch(modelImage, observedImage, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
                   out mask, out homography);

            //Draw the matched keypoints
            Mat result = new Mat();
               // Features2DToolbox.DrawMatches(observedImage, modelKeyPoints, observedImage, observedKeyPoints,
               //matches, result, new MCvScalar(255, 255, 255), new MCvScalar(255, 255, 255), mask);
                Features2DToolbox.DrawKeypoints(observedImage, observedKeyPoints, result,  new Bgr(255,255,255), Features2DToolbox.KeypointDrawType.Default);
                #region draw the projected region on the image
                rectan = new Rectangle(0,0,0,0);
                if (homography != null)
            {
               //draw a rectangle along the projected model
               Rectangle rect = new Rectangle(Point.Empty, modelImage.Size);
               PointF[] pts = new PointF[]
               {
                  new PointF(rect.Left, rect.Bottom),
                  new PointF(rect.Right, rect.Bottom),
                  new PointF(rect.Right, rect.Top),
                  new PointF(rect.Left, rect.Top)
               };
               pts = CvInvoke.PerspectiveTransform(pts, homography);
                   
                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
               using (VectorOfPoint vp = new VectorOfPoint(points))
               {
                
                        var currentFrame = result.Clone();
                        int minx = MIN(pts[0].X, pts[1].X, pts[2].X, pts[3].X);
                        int miny = MIN(pts[0].Y, pts[1].Y, pts[2].Y, pts[3].Y);
                        int maxx = MAX(pts[0].X, pts[1].X, pts[2].X, pts[3].X);
                        int maxy = MAX(pts[0].Y, pts[1].Y, pts[2].Y, pts[3].Y);
                        rectan = new Rectangle(minx, miny, maxx - minx, maxy - miny);
                        if (rectan.Width > 0 && rectan.Height > 0)
                        {
                            if (rectan.Left + rectan.Width < observedImage.Width)
                            {
                                if (rectan.Top + rectan.Height < observedImage.Height)
                                {
                                    if (rectan.Height / rectan.Width >= 1 && rectan.Height / rectan.Width < 3)
                                    {
                                        Bitmap CroppedImage = CropImage(observedImage.Bitmap, rectan );
                                        var scanbp = new Bitmap(CroppedImage, 16, 16);
                                        if (modelbp==null )
                                        {
                                            modelbp = new Bitmap(modelImage.Bitmap, 16, 16);
                                        }
                                        res2 = scanbp;
                                        q = CompBMPs(scanbp, modelbp);
                                        if (q>30 &&q<120)
                                        {
                                            CvInvoke.Polylines(result, vp, true, new MCvScalar(255, 0, 0, 255), 2);
                                        }
                                    }
                                    else
                                    {
                                        q = 0;
                                    }
                                }
                                else
                                {
                                    q = 0;
                                }
                            }
                            else
                            {
                                q = 0;
                            }
                        }
                        else
                        {
                            q = 0;
                        }
                        // select a ROI
                    }
                }

            #endregion

            return result;

         }
      }
        static public Bitmap CropImage(Bitmap source, Rectangle section)
        {
            // An empty bitmap which will hold the cropped image
            Bitmap bmp = new Bitmap(section.Width, section.Height);

            Graphics g = Graphics.FromImage(bmp);

            // Draw the given area (section) of the source image
            // at location 0,0 on the empty bitmap (bmp)
            g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);

            return bmp;
        }

        static private double CompBMPs(Bitmap img1, Bitmap img2)
        {
            double sum = 0;
            int n = 0;
            if (img1.Width == img2.Width && img1.Height == img2.Height)
            {
                for (int i = 0; i < img1.Width; i++)
                {
                    for (int j = 0; j < img1.Height; j++)
                    {
                        var col1 = img1.GetPixel(i, j);
                        var col2 = img2.GetPixel(i, j);
                        sum += GetD(col1, col2);
                        n++;
                    }
                }
            }
            sum = sum / n;
            return sum;
        }


        private static double GetD(Color col1, Color col2)
        {
            return Math.Sqrt(Math.Pow(col1.R - col2.R, 2) + Math.Pow(col1.G - col2.G, 2) + Math.Pow(col1.G - col2.G, 2));
        }

        private static int MAX(float x1, float x2, float x3, float x4)
        {
            return (int)Math.Max(Math.Max(Math.Max(Math.Abs(x1), Math.Abs(x2)), Math.Abs(x3)), Math.Abs(x4));
        }

        private static int MIN(float x1, float x2, float x3, float x4)
        {
            return (int)Math.Min(Math.Min(Math.Min(Math.Abs(x1), Math.Abs(x2)), Math.Abs(x3)), Math.Abs(x4));
        }
    }
}
