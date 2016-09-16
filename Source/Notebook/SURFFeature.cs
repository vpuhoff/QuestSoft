//----------------------------------------------------------------------------
//  Copyright (C) 2004-2016 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.CV.Cuda;
using SURFFeature;

namespace SURFFeatureExample
{
   static class Program
   {
      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main()
      {
         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);
            var f = new Form1();
            //if (Environment.CommandLine.Replace(Application.ExecutablePath,"").Length  >7)
            //{
            //    f.Top = 0;
            //    f.Left = 0;
            //    f.Width = Screen.PrimaryScreen.Bounds.Width;
            //    f.Height = Screen.PrimaryScreen.Bounds.Width;
            //}
            Application.Run(f);
      }
      
      
   }
}
