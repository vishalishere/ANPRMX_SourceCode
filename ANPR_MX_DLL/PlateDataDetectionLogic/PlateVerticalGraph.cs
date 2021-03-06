﻿// ANPR_MX is a simple Automatic Plate Recognition System
// for North American type plates.
// Developed by Jivan Miranda Rodriguez
// For suggestions & feedback contact me at
// jivanro@hotmail.com
// Copyright (c) 2012, Jivan Miranda Rodriguez
// All rights reserved

// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
// * Redistributions of source code must retain the above copyright notice, this list
//   of conditions and the following disclaimer.
// * Redistributions in binary form must reproduce the above copyright notice, this list
//   of conditions and the following disclaimer in the documentation and/or other 
//   materials provided with the distribution.
// * Neither the name of Jivan Miranda Rodriguez nor the names of its contributors may be used
//   to endorse or promote products derived from this software without specific prior written permission.

// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS
// BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY,
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// ---- N O T E S ----
// This project source code links to the original version of the following libraries:
// OpenCV 2.3 - license type: BSD 2 - http://opencv.willowgarage.com
// OpenCvSharp 2.3 - license type: LGPL 3 - http://code.google.com/p/opencvsharp
// Third party copyrights are property of their respective owners.
// ---- N O T E S ----

// ---- I M P O R T A N T ----
// This work is a derivate of the JavaANPR. JavaANPR is a intellectual 
// property of Ondrej Martinsky. Please visit http://javaanpr.sourceforge.net 
// for more info about JavaANPR. 
// ---- I M P O R T A N T ----

// If you want to alter upon this work, you MUST attribute it in 
// a) all source files
// b) on every place, where is the copyright of derivated work
// exactly by the following label :

// ---- label begin ----
// This work is a derivate of ANPR_MX & the JavaANPR. ANPR_MX is an intellectual property
// of Jivan Miranda Rodriguez while JavaANPR is an intellectual 
// property of Ondrej Martinsky. Please visit http://javaanpr.sourceforge.net 
// for more info about JavaANPR. 
// ----  label end  ----

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ANPR_MX.Globals;

namespace ANPR_MX.PlateDataDetectionLogic
{
    public class PlateVerticalGraph: Graph
    {
        private static Double peakFootConstant = Constants.PLATE_VERTICAL_GRAPH_PEAK_FOOT_CONSTANT;
        private Plate handle = null;

        public PlateVerticalGraph(Plate pHandle)
        {
          handle = pHandle;
        }


        public List<Peak> FindPeak(Int32 count)
        {
            List<Peak> outPeaks = new List<Peak>();

            Int32 peakSize = yValues.Count;

            for (Int32 i = 0; i < peakSize; i++)
            {
                yValues[i] = (yValues[i] - GetMinValue());
            }

            for (Int32 c = 0; c < count; c++)
            {
                Double maxValue = 0.0;
                Int32 maxIndex = 0;

                for (Int32 i = 0; i < peakSize; i++)
                {
                    if (AllowedInterval(outPeaks, i))
                    {
                        if (yValues[i] >= maxValue)
                        {
                            maxValue = yValues[i];
                            maxIndex = i;
                        }
                    }
                }

                if (yValues[maxIndex] < 0.05 * GetMaxValue())
                    break;

                Int32 leftIndex = IndexOfLeftPeakRel(maxIndex, peakFootConstant);
                Int32 rightIndex = IndexOfRightPeakRel(maxIndex, peakFootConstant);

                outPeaks.Add(new Peak(Math.Max(0,leftIndex), maxIndex, Math.Min(peakSize-1,rightIndex)));


            }
            
            outPeaks.Sort(new PeakComparer2(this));
            if(outPeaks[0].Left < 10)
                outPeaks.Sort(new PeakComparer(this));
            base.peaks = outPeaks;
            return outPeaks;
        }


        private class PeakComparer2 : IComparer<Peak>
        {
            private PlateVerticalGraph graphHandle = null;

            public PeakComparer2(PlateVerticalGraph pGraph)
            {
                graphHandle = pGraph;
            }

            public Int32 Compare(Peak peak1, Peak peak2)
            {
                Double comparison = peak2.GetDiff - peak1.GetDiff;
                if (comparison < 0.0) return -1;
                if (comparison > 0.0) return 1;
                return 0;
            }

         
        }

        private class PeakComparer : IComparer<Peak>
        {
            private PlateVerticalGraph graphHandle = null;

            public PeakComparer(PlateVerticalGraph pGraph)
            {
                graphHandle = pGraph;
            }

            
            public Int32 Compare(Peak peak1, Peak peak2)
            {
                Double comparison = GetPeakValue(peak2) - GetPeakValue(peak1);
                if (comparison < 0.0) return -1;
                if (comparison > 0.0) return 1;
                return 0;
            }

            private Double GetPeakValue(Peak peak)
            {
                return (graphHandle.yValues[peak.Center]);
            }
        }

    }
}
