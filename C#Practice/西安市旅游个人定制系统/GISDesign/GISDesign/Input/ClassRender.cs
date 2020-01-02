using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.GlobeCore;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Location;




namespace GISDesign
{
    
    public class ClassRender
    {
        public ClassRender(AxMapControl pMapControl, String LayerName, int ClassCount, string Field)
        {
            IGeoFeatureLayer pGeolayer;

            ILayer pLayer;

            IActiveView pActiveView;

            IEnumLayer pEnumLayer;

            pEnumLayer = pMapControl.Map.get_Layers(null, true);

            if (pEnumLayer == null)
            {
                return;
            }

            pEnumLayer.Reset();

            int i = 0;

            int LayerIndex =-1;

            for (pLayer = pEnumLayer.Next(); pLayer != null; pLayer = pEnumLayer.Next())
            {
                i++;

                if (pLayer.Name == LayerName)
                {
                    LayerIndex = i - 1;

                    break;
                }
            }

            if (LayerIndex == -1)
            {
                MessageBox.Show("û���ҵ�Ҫ��Ⱦ��ͼ��");

                return;
            }

            pGeolayer = pMapControl.Map.get_Layer(LayerIndex) as IGeoFeatureLayer;

            pActiveView = pMapControl.ActiveView;

            //������Ϊ��ͳ�ƺͷ�������Ҫ�Ķ���

            ITable pTable;

            IClassifyGEN pClassify;//C#Ҫ�������ͬ��VB�еģ��������

            ITableHistogram pTableHist;//�൱��һ��ͳ�Ʊ�

            IBasicHistogram pBasicHist;//���������һ������Ҫ�ķ���

            double[] ClassNum;

            int ClassCountResult;//���ط��������

            IHsvColor pFromColor;

            IHsvColor pToColor;//���ڹ�������һ����ɫ������

            IAlgorithmicColorRamp pAlgo;

            pTable = pGeolayer as ITable;

            IMap pMap;

            pMap = pMapControl.Map;

            pMap.ReferenceScale = 0;

            pBasicHist = new BasicTableHistogramClass();//Ҳ����ʵ���� pTableHist��ѧ���������˼ά

            pTableHist = pBasicHist as ITableHistogram;

            pTableHist.Table = pTable;

            pTableHist.Field = Field;

            object datavalus;

            object Frenquen;

            pBasicHist.GetHistogram(out datavalus,out  Frenquen);//������ݺ���Ӧ��Ƶ����

            pClassify = new EqualIntervalClass();

            try
            {
                pClassify.Classify(datavalus, Frenquen, ref ClassCount);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            // �������

            ClassNum  = (double[])pClassify.ClassBreaks;

            ClassCountResult = ClassNum.GetUpperBound(0);//���طּ�������

            IClassBreaksRenderer pClassBreak;

            pClassBreak = new ClassBreaksRendererClass();

            pClassBreak.Field = Field;

            pClassBreak.BreakCount = ClassCountResult;

            pClassBreak.SortClassesAscending = true;

            pAlgo = new AlgorithmicColorRampClass();

            pAlgo.Algorithm = esriColorRampAlgorithm.esriHSVAlgorithm;

            pFromColor = Hsv(60, 100, 96);

            pToColor = Hsv(0, 100, 96);

            pAlgo.FromColor = pFromColor;

            pAlgo.ToColor = pToColor;

            pAlgo.Size = ClassCountResult;

            bool ok;

            pAlgo.CreateRamp(out ok);

            IEnumColors pEnumColor;

            pEnumColor = pAlgo.Colors;

            pEnumColor.Reset();

            IColor pColor;

            ISimpleFillSymbol pSimFill;

           /* IRgbColor[] pRgbColor;//���Թ�����ɫ

            pRgbColor = new IRgbColor[ClassCountResult];

            for (int j = 0; j < ClassCountResult; j++)
            {
                int R = 50;

                int G = 100;

                int B = 50;

                R = R + 50;

                if (R > 250)
                {
                    R = 50;
                }
                if (G > 250)
                {
                    G = 100;
                }
                if (B > 250)
                {
                    B = 50;
                }

                G = G + 100;

                B = B + 50;



                pRgbColor[j] = ColorRgb(R, G, B);

            }
            */


            for (int indexColor = 0; indexColor <= ClassCountResult - 1; indexColor++)
            {
                pColor = pEnumColor.Next();

                pSimFill = new SimpleFillSymbolClass();

                 pSimFill.Color = pColor;

               // pSimFill.Color = pRgbColor[indexColor ];

                pSimFill.Style = esriSimpleFillStyle.esriSFSSolid;

                //Ⱦɫ

                pClassBreak.set_Symbol(indexColor, pSimFill as ISymbol);

                pClassBreak.set_Break(indexColor, ClassNum[indexColor + 1]);



            }



            pGeolayer.Renderer = pClassBreak as IFeatureRenderer;

            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);

            new MainForm().axTOCControl1.Update();
           



        }
        public IHsvColor Hsv(int hue, int saturation, int val)
        {
            IHsvColor pHsvC;

            pHsvC = new HsvColorClass();

            pHsvC.Hue = hue;

            pHsvC.Saturation = saturation;

            pHsvC.Value = val;

            return pHsvC;
        }
        public IRgbColor ColorRgb(int r, int g, int b)
        {
            IRgbColor pRGB;

            pRGB = new RgbColorClass();

            pRGB.Red = r;

            pRGB.Green = g;

            pRGB.Blue = b;

            return pRGB;


        }
    }
}
