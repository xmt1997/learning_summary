using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;

namespace GISDesign.Query
{
    public partial class frmLocationQuery : Form
    {
        private GISDesign.MainForm pMainform = null;//����һ��mainform��������ȡ������mainfrom�е��������
        private AxMapControl MainAxMapControl;             //������ȡ�������е��Ǹ���Ҫ��mapcontrol
        private esriSpatialRelEnum pSpatialRel;            //ȫ�ֱ���������ÿռ��ϵ
        //private IFeatureSelection pResultFeatureSelection;       //������¼���յĽ��


        public frmLocationQuery(AxMapControl mapcontrol,GISDesign.MainForm _pMainForm)
        {
            InitializeComponent();
            pMainform = _pMainForm;
            MainAxMapControl = mapcontrol;
        }

        #region �������Լ��ĺ���
        private ISpatialFilter GenerateSpatialFilter()
        {
            frmPromptQuerying frmPrompt = new frmPromptQuerying(4);//��ʾ����������ͼ���� Ϊ�����쳣ʱ�ܽ�����ʾ���ڹرգ����Է���try����
            try
            {
                frmPrompt.Show();
                System.Windows.Forms.Application.DoEvents();//'ת�ÿ���Ȩ��û����һ��Ļ���ʾ���ڲ���������ʾ
                //����Ҫ�����spatialfilter
                //��ù���spatialfilter.geometry������feature
                //IMap pMap;                        //����QI��IEnumLayer
                //IEnumLayer pEnumLayer;            //����������Ϊ��ѡ���layers
                //ILayer pLayer;
                IFeatureLayer pFeatureLayer = null;
                IGeometry pGeometry = null;
                IEnumFeature pEnumFeature = null;          //�����зֱ�����ʹ�úͲ�ʹ��
                IFeatureCursor pFeatureCursor = null;      //�Ѿ�ѡ���Ԫ�����ɡ�ɸѡͼ�Ρ������
                IFeatureClass pFeatureClass;
                IFeature pFeature;
                string strLayerName = comboBoxConditionLayer.Text;
                if (checkBoxUseSelectedOnly.Checked == false)    //����û��Լ�ѡ������ͼ�㡱,��������ѡ�е�Ԫ��
                {
                    pFeatureLayer = GetLayerbyName(strLayerName) as IFeatureLayer;  //���layer����������spatialfilter.geometry��
                    //���Ǹ�������ͼ�㡱

                    //���ﲻ��featureselection�����enumfeature,�Ӷ����������geometry�ϲ�������ȡfeaturecursor������Ϊ
                    //��featureselection�Ļ�����ô��ϲ�������feature��һ�������ôд��featureselection=mainaxmapcontrol.map.featureselection
                    //������ָ��featureselection��map�����һ���㣡���������featureselection=pfeaturelayer.featureselection��
                    //Ȼ��enumfeature=featureselection����ʱenumfeature����null�����е�pfeaturelayer�Ѿ���map���һ��ָ���Ĳ��ˣ�
                    //�����featureselection=mainaxmapcontrol.map.featureselection���ͻ������⣺Ҳ��map�����Ѿ��б�ѡ�е�Ԫ���ˣ�����ʱ�����ַ���
                    //��õ�enumfeatureֻ������Щ�Ѿ�ѡ�е�Ԫ�أ�����Ӧ����������Ӧ�û�õ���ָ��Ϊ������ͼ�㡱������feature������ֻ����featurecursor
                    //�����featureselection�Ļ���Ҫ��ôд��
                    //IFeatureSelection pFeatureSelection = MainaxMapControl.get_Layer(strLayerName) as IFeatureSelection;
                    //pFeatureSelection.SelectFeatures(null, esriSelectionResultEnum.esriSelectionResultNew, false);//��ʹ���ѱ�ѡ�е�Ԫ��ʱ������仰Խ��ȥ
                    //IEnumFeature pEnumFeature = MainaxMapControl.Map.FeatureSelection as IEnumFeature;
                    pFeatureClass = pFeatureLayer.FeatureClass;
                    pFeatureCursor = pFeatureClass.Search(null, true);
                    pFeature = pFeatureCursor.NextFeature();
                }
                else            //���ʹ�õ�ͼ����ѡ�е�Ԫ����Ϊ��ɸѡͼ�㡱
                {
                    pEnumFeature = MainAxMapControl.Map.FeatureSelection as IEnumFeature;
                    pFeature = pEnumFeature.Next();
                }

                pGeometry = pFeature.Shape;
                ITopologicalOperator pTopologicalOperator;
                //�ϲ������Ե���ɸ�ӡ���geometry
                while (pFeature != null)
                {
                    
                    //�����һ���ǹؼ����������յ�pgeometry���Ի���������geometry
                    //��ΪTopologicalOperator�������һ��shape��һ��geometry�����Խ����������������ĺϲ�.
                    //ֻ����shapecopy���ܹ���ȷ����shape�Ļ����ļ��ͳ���Ϊ��������ⵢ����һ���ڣ�����
                    //����һ��˼·Ҳ���ԡ������˼·����ѭ���ϲ�����geometry����һ��˼·����ѭ���õ�����ÿ��
                    //geometryȥɸѡͼ�����featuresȻ��ϲ����
                    pTopologicalOperator = pFeature.ShapeCopy as ITopologicalOperator;
                    //if (checkBoxBuffer.Checked == true)
                    //{
                    //    double ddistance = Convert.ToDouble(textBoxBuffer.Text);
                    //    pGeometry = pTopologicalOperator.Buffer(ddistance);
                    //    //pTopologicalOperator.Simplify();        //ʹ���˹�ϵ�����ȷ
                    //}
                    pGeometry = pTopologicalOperator.Union(pGeometry as IGeometry);
                    if (checkBoxUseSelectedOnly.Checked == false)//�õ���featurecursor
                        pFeature = pFeatureCursor.NextFeature();
                    else pFeature = pEnumFeature.Next();
                }
                //pFeatureSelection.Clear();//������������������geometry��������featureselection�Ļ�����Ҳ��һ��ؼ���
                //�������յ���ʾҲ�������featureselection��ʾ����

                //���ɻ����������������ѭ��֮��ִ�У������Ӹ���Ч�ʣ����Ҹ������׳���
                if (checkBoxBuffer.Checked == true)
                {
                    double ddistance = Convert.ToDouble(textBoxBuffer.Text);
                    pTopologicalOperator = pGeometry as ITopologicalOperator;
                    pGeometry = pTopologicalOperator.Buffer(ddistance);
                    pTopologicalOperator.Simplify();        //ʹ���˹�ϵ�����ȷ
                    //MainAxMapControl.FlashShape(pGeometry, 2, 500, null);//��˸����֤��������ȷ���
                }               
                frmPrompt.WindowState = FormWindowState.Minimized;
                frmPrompt.Dispose();
                System.Windows.Forms.Application.DoEvents();
                object m_fillsymbol;                                //�����drawshape�Ĳ���������object��

                IRgbColor pRgb = new RgbColorClass();               // ��ȡIRGBColor�ӿ�
                pRgb.Red = 255;                                     // ������ɫ����
                ILineSymbol outline = new SimpleLineSymbolClass();          // ��ȡILine���Žӿ�
                outline.Width = 5;
                outline.Color = pRgb;
                ISimpleFillSymbol simpleFillSymbol = new SimpleFillSymbolClass();// ��ȡIFillSymbol�ӿ�
                simpleFillSymbol.Outline = outline;
                simpleFillSymbol.Color = pRgb;
                simpleFillSymbol.Style = esriSimpleFillStyle.esriSFSBackwardDiagonal;
                m_fillsymbol = simpleFillSymbol;
                //MainAxMapControl.DrawShape(pGeometry, ref m_fillsymbol);
                MainAxMapControl.FlashShape(pGeometry, 3, 500, m_fillsymbol);//��˸�����û�֪����ǰ��"����ͼ��"����״

                ISpatialFilter pSpatialFilter = new SpatialFilterClass();
                pSpatialFilter.Geometry = pGeometry;
                pSpatialFilter.SpatialRel = pSpatialRel;        //���Ǹ�ȫ�ֱ���
                if (checkBoxUseSelectedOnly.Checked == true)
                    pSpatialFilter.GeometryField = "Shape";     //��ʹ����ѡ�е�Ԫ����Ϊ��ɸѡͼ�㡱ʱ��
                //�޷����������ȥ���geometryfield����Ϊpfeaturelayer(����ͼ��)Ϊ��
                else
                    pSpatialFilter.GeometryField = pFeatureLayer.FeatureClass.ShapeFieldName;

                return pSpatialFilter;
            }
            catch (Exception ex)
            {
                MessageBox.Show("���ɡ�����ͼ�㡱����!���� | " + ex.Message);
                frmPrompt.Dispose();
                return null;//�����򲻷���spatialfitler
            }
        }

        private void PerformLocationQuery(ISpatialFilter pSpatialFilter)
        {
            try
            {
                frmPromptQuerying frmPrompt = new frmPromptQuerying();//��ʾ���ռ��ѯ��
                frmPrompt.Show();
                System.Windows.Forms.Application.DoEvents();//'ת�ÿ���Ȩ��û����һ��Ļ���ʾ���ڲ���������ʾ
                string strLayerName;
                IFeatureLayer pFeatureLayer;
                IFeatureSelection pFeatureSelection;//������¼��ѯ�Ľ��
                foreach (object itemChecked in checkedListBoxFeaturesLayer1.CheckedItems)    //����ÿһ��ѡ�еĲ㶼ִ�в�ѯ������ϲ�
                {


                    strLayerName = itemChecked.ToString();    //������Ǵ���ѯ��layer
                    pFeatureLayer = GetLayerbyName(strLayerName) as IFeatureLayer;
                    pFeatureSelection = pFeatureLayer as IFeatureSelection;
                    pFeatureSelection.SelectFeatures(pSpatialFilter, esriSelectionResultEnum.esriSelectionResultAdd, false);//��ѡ�Ĳ���ܲ�ֹһ��
                    //ͨ��ѭ�����β�ѯÿ���㣬�ѽ��add����
                    
                }
                //�����ѡ��ѡ�У���λ��ѡ����
                if (checkBoxZoomtoSelected.Checked == true)
                {
                    IEnumFeature pEnumFeature = MainAxMapControl.Map.FeatureSelection as IEnumFeature;
                    IFeature pFeature = pEnumFeature.Next();
                    ESRI.ArcGIS.Geometry.IEnvelope pEnvelope = new ESRI.ArcGIS.Geometry.EnvelopeClass();
                    while (pFeature != null)
                    {
                        pEnvelope.Union(pFeature.Extent);
                        pFeature = pEnumFeature.Next();
                    }
                    MainAxMapControl.ActiveView.Extent = pEnvelope;
                    MainAxMapControl.ActiveView.Refresh();//���������ˢ�£�ֻҪ��ѯǰ��ͼ�Ѿ����Ŵ���Ч���Ļ�����λ��
                    //��ͼû��ˢ�£�ѡ�񼯵��Ƕ�λ��ˢ����
                }
                frmPrompt.Dispose();
                //�ѽ����ʾ����
                MainAxMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);

                //double i = MainAxMapControl.Map.SelectionCount;
                //i = Math.Round(i, 0);//С�����ָ��Ϊ��λ����
                //pMainform.toolStripStatusLabel1.Text = "��ǰ����" + i.ToString() + "����ѯ���";
            }
            catch (Exception ex)
            { 
                MessageBox.Show("���ɲ�ѯ�������!���� | "+ex.Message);
                return;
            }
        }

        private void AddAllLayerstoCheckListBox()
        {
            checkedListBoxFeaturesLayer1.Items.Clear();
            try
            {
                int pLayerCount = MainAxMapControl.LayerCount;
                if (pLayerCount > 0)
                {
                    checkedListBoxFeaturesLayer1.Enabled = true;//ѡ��ͼ������
                    checkBoxShowVectorOnly1.Enabled = true;//����ʾʸ����ѡ�����

                    for (int i = 0; i <= pLayerCount - 1; i++)
                    {
                        if (checkBoxShowVectorOnly1.Checked)
                        {
                            if (MainAxMapControl.get_Layer(i) is IFeatureLayer)  //ֻ���ʸ��ͼ�㣬դ��ͼ��û�����Ա�
                                checkedListBoxFeaturesLayer1.Items.Add(MainAxMapControl.get_Layer(i).Name);
                        }

                        else
                            checkedListBoxFeaturesLayer1.Items.Add(MainAxMapControl.get_Layer(i).Name);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("����ͼ����� | "+ex.Message);
                return;
            }
        }

        private void AddConditionLayers()           //�������ͼ�㣬��Ҫ���˵���checkedlistbox���Ѿ���ѡ�е�ͼ��
        {
            comboBoxConditionLayer.Items.Clear();//������ա�����ͼ�㡱
            try
            {
                int pLayerCount = MainAxMapControl.LayerCount;
                if (pLayerCount > 0)
                {
                    for (int i = 0; i <= pLayerCount - 1; i++)       //�Ȱ�����ͼ��ӵ�combox��
                    {
                        if(MainAxMapControl.get_Layer(i) is IFeatureLayer)
                        comboBoxConditionLayer.Items.Add(MainAxMapControl.get_Layer(i).Name);
                    }

                    foreach (object itemChecked in checkedListBoxFeaturesLayer1.CheckedItems)    //��ɾ�����Ѿ�ѡ�еĲ�
                    {
                        //���ѭ���е�ÿ���㶼��Ҫɾȥ��
                        comboBoxConditionLayer.Items.Remove(itemChecked);
                    }

                    if (comboBoxConditionLayer.Items.Count > 0)
                        comboBoxConditionLayer.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private ILayer GetLayerbyName(string strLayerName)
        {
            ILayer pLayer = null;
            for (int i = 0; i <= MainAxMapControl.LayerCount - 1; i++)
            {
                if (strLayerName == MainAxMapControl.get_Layer(i).Name)
                { pLayer = MainAxMapControl.get_Layer(i); break; }
            }
            return pLayer;
        }

        #endregion

        #region �¼�
        private void checkBoxBuffer_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxBuffer.Checked == true)
                textBoxBuffer.Enabled = true;
            else
            {
                textBoxBuffer.Enabled = false;
            }
        }

        private void comboBoxSpatialRel_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBoxSpatialRel.SelectedIndex)
            {
                case 0: pSpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    labelDescription.Text = "�Ӵ�ѡ��ͼ����ѡ����룢����ͼ�㣢�����ѱ�ѡ�е�Ԫ�أ��ཻ����ЩԪ��"; break;
                case 1: pSpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                    labelDescription.Text = "�Ӵ�ѡ��ͼ����ѡ�����Щ��������ͼ�㣢�����ѱ�ѡ�е�Ԫ�أ���������Ԫ��"; break;
                case 2: pSpatialRel = esriSpatialRelEnum.esriSpatialRelWithin;
                    labelDescription.Text = "�Ӵ�ѡͼ����ѡ�����Щ��ȫ����������ͼ�㣢�����ѱ�ѡ�е�Ԫ�أ���Ԫ��"; break;
                case 3: pSpatialRel = esriSpatialRelEnum.esriSpatialRelTouches;
                    labelDescription.Text = "�������ɱ߽�Ӵ������"; break;
            }
        }

        private void buttonCancel1_Click(object sender, EventArgs e)
        {
            this.Dispose();
            pMainform.frmLocationisOpen = false;//����������ǰû�д򿪵�λ�ò�ѯ����
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;//ͨ��λ�ò�ѯ������С��
            PerformLocationQuery(GenerateSpatialFilter());
            this.Dispose();
            pMainform.frmLocationisOpen = false;

        }

        private void frmLocationQuery_Load(object sender, EventArgs e)
        {
            AddAllLayerstoCheckListBox();
            if(checkedListBoxFeaturesLayer1.Items.Count>0)
                buttonOk.Enabled = true;
        }

        private void checkBoxShowVectorOnly1_CheckedChanged(object sender, EventArgs e)
        {
            AddAllLayerstoCheckListBox();
            AddConditionLayers();
        }

        private void checkBoxUseSelectedOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUseSelectedOnly.Checked == true)
                comboBoxConditionLayer.Enabled = false;
            else comboBoxConditionLayer.Enabled = true;
        }

        private void comboBoxConditionLayer_DropDown(object sender, EventArgs e)
        {
            //��conditionlayer��combox�������ͼ����¼�һ��Ҫ�������dropdown to show��ʱ���������
            //checkedListBoxFeaturesLayer1_ItemCheck��ʱ����������ֵ����⣨û��ϸ�룩
            AddConditionLayers();
        }

        private void textBoxBuffer_KeyDown(object sender, KeyEventArgs e)
        {
            //����ֻ����������
            // keyValue>47&&<58   :   0-9   
            // keyValue=190   :   Decimal   Point
            // keyValue=189   :   "-"
            // keyValue>34&&   keyValue<41   :Home   ,End   and   Arrow   Keys   
            // keyValue==8   :Backspace   Key  
            if ((e.KeyValue > 47 && e.KeyValue < 58) || (e.KeyValue == 190) || (e.KeyValue > 34 && e.KeyValue < 41) || (e.KeyValue == 8) || (e.KeyCode == Keys.Delete) || (e.KeyValue == 189))
            { 
            }
            else
            {
                MessageBox.Show("����ֻ���������֣������ԣ�");
                textBoxBuffer.Clear();
                return;
            }
        }

        #endregion

        private void frmLocationQuery_FormClosed(object sender, FormClosedEventArgs e)
        {
            pMainform.frmLocationisOpen = false;//����������ǰû�д򿪵�λ�ò�ѯ����
        }

    }
}
