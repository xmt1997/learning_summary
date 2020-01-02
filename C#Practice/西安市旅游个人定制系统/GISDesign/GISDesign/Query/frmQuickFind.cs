using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;

namespace GISDesign.Query
{
    public partial class frmQuickFind : Form
    {
        private GISDesign.MainForm pMainform = null;//����һ��mainform��������ȡ������mainfrom�е��������
        private AxMapControl MainAxMapControl;             //������ȡ�������е��Ǹ���Ҫ��mapcontrol
        IFeatureCursor FeatureCursor;      //Ϊ�˲�д�ظ������ȵ�ѭ�����,�����������λȫ�ֱ���.����
        //��ѯ�����Ҫһ��ȫ��ѭ��,˫�������ʱ����һ��ȫ��ѭ��

        public frmQuickFind(AxMapControl mapcontrol, MainForm _pMainForm)
        {
            InitializeComponent();
            pMainform = _pMainForm;
            MainAxMapControl = mapcontrol;
        }

        private void frmQuickFind_Load(object sender, EventArgs e)
        {
            AddAllLayerstoComboBox();
            if (comboBoxLayers.Items.Count != 0)
            {
                comboBoxLayers.SelectedIndex = 0;//��combox�ĵ�ǰѡ�е���Ŀ��Ϊ��һ�������Ĭ��ѡ�е�һ�����Ļ�Ϊ�գ�������
                //�任checkboxShowVectorOnly�����listBox�б���գ��Ѿ��Ľ���������⣩��������listBOXfields������
                //��comboxΪ�յ�����������������������ʱ����Ҫ�г�����ֵ��������Ҫ����combox��
                //��ǰֵ����ȡͼ�㣬�����listBoxFields_SelectedIndexChanged(object sender, EventArgs e)
                //������ֵΪ�գ��ͻ������
                AddFieldtoCombobox();
                buttonNewSearch.Enabled = true;
                buttonSearch.Enabled = true;
            }
        }

        private void AddAllLayerstoComboBox()
        {
            try
            {
                comboBoxLayers.Items.Clear();

                int pLayerCount = MainAxMapControl.LayerCount;
                if (pLayerCount > 0)
                {
                    comboBoxLayers.Enabled = true;//�����˵�����

                    for (int i = 0; i <= pLayerCount - 1; i++)
                    {
                        if (MainAxMapControl.get_Layer(i) is IFeatureLayer)  //ֻ���ʸ��ͼ�㣬դ��ͼ��û�����Ա�
                            comboBoxLayers.Items.Add(MainAxMapControl.get_Layer(i).Name);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private ILayer GetLayerByName(string strLayerName)
        {
            ILayer pLayer = null;
            for (int i = 0; i <= MainAxMapControl.LayerCount - 1; i++)
            {
                if (strLayerName == MainAxMapControl.get_Layer(i).Name)
                { pLayer = MainAxMapControl.get_Layer(i); break; }
            }
            return pLayer;
        }

        private void AddFieldtoCombobox()
        {
            comboBoxField.Items.Clear();
            string strLayerName = comboBoxLayers.Text;
            IFeatureLayer pFeatureLayer = GetLayerByName(strLayerName) as IFeatureLayer;
            for (int i = 0; i <= pFeatureLayer.FeatureClass.Fields.FieldCount - 1; i++)
                comboBoxField.Items.Add(pFeatureLayer.FeatureClass.Fields.get_Field(i).Name);
        }

        private void PerformSearch()
        {
            if (textBoxKeyword.Text == "")
                MessageBox.Show("��������Ҫ���ҵĹؼ��֣�");
            else
            {
                string strFieldName = comboBoxField.Text;
                if (strFieldName == "")
                {
                    MessageBox.Show("��ѡ����Ҫ���ҵ������ֶΣ�");
                    return;
                }
                listBoxResults.Items.Clear();
                IQueryFilter pQueryFilter = new QueryFilterClass();
                IFeatureClass pFeatureClass = (GetLayerByName(comboBoxLayers.Text) as IFeatureLayer).FeatureClass;
                IFeatureCursor pFeatureCursor;//����Ǿֲ���
                IFeature pFeature;
                int i = 0;//���ڱ�ǽ����Ŀ
                if (strFieldName != "")
                {
                    if (checkBoxBlurSearch.Checked == true)
                        pQueryFilter.WhereClause = strFieldName + " Like " + "'" + "%" + textBoxKeyword.Text + "%" + "'";
                    else if (pFeatureClass.Fields.get_Field(pFeatureClass.Fields.FindField(strFieldName)).Type == esriFieldType.esriFieldTypeString)
                        pQueryFilter.WhereClause = strFieldName + "=" + "'" + textBoxKeyword.Text + "'";
                    else pQueryFilter.WhereClause = strFieldName + "=" + textBoxKeyword.Text;
                    pFeatureCursor = pFeatureClass.Search(pQueryFilter, true);
                    FeatureCursor = pFeatureClass.Search(pQueryFilter, true);//��Ϊcursorһ���ƶ����ף��ǲ��ܹ��ٻص���ͷ�ģ�����������������һ���Ժ�����˫����ʱ����
                    pFeature = pFeatureCursor.NextFeature();
                    if (pFeature == null)
                        MessageBox.Show("�Ҳ��������");
                    else
                    {
                        while (pFeature != null)
                        {
                            //�ѽ����ӵ��б���,�б��е��ַ���ÿ�����ҵ���feature����Ӧ�ֶε�ֵ
                            listBoxResults.Items.Add(pFeature.get_Value(pFeature.Fields.FindField(strFieldName)));
                            pFeature = pFeatureCursor.NextFeature();
                            i++;
                        }
                    }

                    labelResultNumber.Text = "��" + i.ToString() + "��";
                }

            }
        }

        private void frmQuickFind_FormClosed(object sender, FormClosedEventArgs e)
        {
            pMainform.frmQuickFindisOpen = false;
        }

        private void buttonNewSearch_Click(object sender, EventArgs e)
        {
            textBoxKeyword.Clear();
            listBoxResults.Items.Clear();
            labelResultNumber.Text = "��  " + "��";
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private void comboBoxField_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strFieldName = comboBoxField.Text;
            IFeatureClass pFeatureClass = (GetLayerByName(comboBoxLayers.Text) as IFeatureLayer).FeatureClass;
            int index = pFeatureClass.Fields.FindField(strFieldName);
            if (pFeatureClass.Fields.get_Field(index).Type != esriFieldType.esriFieldTypeString)
            {
                checkBoxBlurSearch.Checked = false;
                checkBoxBlurSearch.Enabled = false;//���ַ������ֶβ��ܹ�����ģ������
            }
            else
                checkBoxBlurSearch.Enabled = true;
        }

        private void comboBoxLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            AddFieldtoCombobox();
        }

        private void listBoxResults_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                string strKeyword = listBoxResults.SelectedItem.ToString();//ѡ�еĽ����Ŀ
                if (strKeyword != "")
                {
                    IFeature pFeature;
                    //��������������������ָ����Ԫ�ر��ѡ��״̬�����õģ��Ժ�������������
                    IFeatureSelection pFeatureSelection = (GetLayerByName(comboBoxLayers.Text) as IFeatureLayer) as IFeatureSelection;
                    pFeatureSelection.Clear();//�����Ǹ�����ķ������ڸ㲻����ֻ������������ﵽĿ����
                    ISelectionSet pSelectionSet = pFeatureSelection.SelectionSet;
                    IEnvelope pEnvelope = new EnvelopeClass();
                    pFeature = FeatureCursor.NextFeature();//���featurecursor��ȫ�ֱ�����Ҳ���ǵ�һ�����ҵĽ��
                    int index = (GetLayerByName(comboBoxLayers.Text) as IFeatureLayer).FeatureClass.Fields.FindField(comboBoxField.Text);
                    //int oid=0;//����û���û�йرղ��Ҵ��ڵ�����¶Խ���б����˶��˫������ÿ��˫����Ҫ�����һ��˫����ѡ�е�Ԫ��
                    while (pFeature != null)
                    {
                        if (pFeature.get_Value(index).ToString() == strKeyword)
                        {
                            //if (oid != 0)
                            //    pSelectionSet.RemoveList(1, ref oid);
                            pSelectionSet.Add(pFeature.OID);    //�����feature�ӵ���ǰѡ����
                            //oid = pFeature.OID;
                            if (pFeature.Shape.GeometryType == esriGeometryType.esriGeometryPoint || pFeature.Shape.GeometryType == esriGeometryType.esriGeometryMultipoint)
                            {
                                //���˫���Ķ�����һ���㣬�����Ӳ��ܹ����ŵ��õ�
                                pEnvelope.PutCoords(0, 0, 0.3, 0.3);//ȷ�������������С
                                IPoint pPoint = pFeature.Shape as IPoint;
                                pEnvelope.CenterAt(pPoint);
                            }
                            else
                                pEnvelope = pFeature.Extent;
                            break;
                        }
                        pFeature = FeatureCursor.NextFeature();
                    }

                    MainAxMapControl.Extent = pEnvelope;
                    MainAxMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("���ŵ�ָ��Ԫ�س��� | " + ex.Message);
                return;
            }
        }

        private void listBoxResults_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}