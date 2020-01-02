using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;

namespace GISDesign.Query
{
    public partial class frmAttriQuery : Form
    {

        private GISDesign.MainForm pMainform = null;//����һ��mainform��������ȡ������mainfrom�е��������
        private AxMapControl MainAxMapControl;             //������ȡ�������е��Ǹ���Ҫ��mapcontrol
        private esriSelectionResultEnum selectmethod=esriSelectionResultEnum.esriSelectionResultNew;//������¼�������ķ�����������whereclause��ѯ�ĵط�
        private IFeatureSelection pFeatureSelection;       //������¼���յĽ���������û������ڲ��رմ˴��ڵ�����½���
                                                           //��β�ѯ������ڶ��β�ѯ�Ľ����ӵ���һ�β�ѯ�Ľ���У�����
                                                           //��Ҫһ��ȫ�ֱ������洢���
        public frmAttriQuery(AxMapControl mapcontrol,GISDesign.MainForm mf)
        {
            InitializeComponent();
            MainAxMapControl = mapcontrol;
            pMainform = mf;
        }

        private void frmAttriQuery_Load(object sender, EventArgs e)
        {
            AddAllLayerstoComboBox(comboBoxLayers);
            if (comboBoxLayers.Items.Count != 0)
            {
                comboBoxLayers.SelectedIndex = 0;//��combox�ĵ�ǰѡ�е���Ŀ��Ϊ��һ�������Ĭ��ѡ�е�һ�����Ļ�Ϊ�գ�������
                //�任checkboxShowVectorOnly�����listBox�б���գ��Ѿ��Ľ���������⣩��������listBOXfields������
                //��comboxΪ�յ�����������������������ʱ����Ҫ�г�����ֵ��������Ҫ����combox��
                //��ǰֵ����ȡͼ�㣬�����listBoxFields_SelectedIndexChanged(object sender, EventArgs e)
                //������ֵΪ�գ��ͻ������
                comboBoxMethod.Enabled = true;
                comboBoxMethod.SelectedIndex = 0;

                buttonOk.Enabled = true;
                buttonClear.Enabled = true;
                buttonApply.Enabled = true;
            }
        }

        #region �������Լ��ĺ���
        private void AddAllLayerstoComboBox(ComboBox combox)
        {
            try
            {
                combox.Items.Clear();

                int pLayerCount = MainAxMapControl.LayerCount;
                if (pLayerCount > 0)
                {
                    combox.Enabled = true;//�����˵�����
                    checkBoxShowVectorOnly.Enabled = true;//��ѡ�����

                    for (int i = 0; i <= pLayerCount - 1; i++)
                    {
                        if (checkBoxShowVectorOnly.Checked)
                        {
                            if (MainAxMapControl.get_Layer(i) is IFeatureLayer)  //ֻ���ʸ��ͼ�㣬դ��ͼ��û�����Ա�
                                combox.Items.Add(MainAxMapControl.get_Layer(i).Name);
                        }

                        else
                            combox.Items.Add(MainAxMapControl.get_Layer(i).Name);
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
            ILayer pLayer=null;
            for (int i = 0; i <= MainAxMapControl.LayerCount - 1; i++)
            {
                if (strLayerName == MainAxMapControl.get_Layer(i).Name)
                { pLayer = MainAxMapControl.get_Layer(i); break; }
            }
            return pLayer;
        }

        private void PerformAttributeSelect()
        {
            try
            {
                IQueryFilter pQueryFilter=new QueryFilterClass();
                IFeatureLayer pFeatureLayer;

                pQueryFilter.WhereClause = textBoxWhereClause.Text;
                pFeatureLayer = GetLayerByName(comboBoxLayers.SelectedItem.ToString()) as IFeatureLayer;
                pFeatureSelection = pFeatureLayer as IFeatureSelection;

                int iSelectedFeaturesCount = pFeatureSelection.SelectionSet.Count;
                pFeatureSelection.SelectFeatures(pQueryFilter, selectmethod, false);//ִ�в�ѯ
                
                //������β�ѯ�󣬲�ѯ�Ľ����Ŀû�иı䣬����Ϊ���β�ѯû�в����µĽ��
                if (pFeatureSelection.SelectionSet.Count == iSelectedFeaturesCount || pFeatureSelection.SelectionSet.Count == 0)
                {
                    MessageBox.Show("û�з��ϱ��β�ѯ�����Ľ����");
                    return;
                }
                
                //�����ѡ��ѡ�У���λ��ѡ����
                if (checkBoxZoomtoSelected.Checked == true)
                {
                    IEnumFeature pEnumFeature = MainAxMapControl.Map.FeatureSelection as IEnumFeature;
                    IFeature pFeature = pEnumFeature.Next();
                    ESRI.ArcGIS.Geometry.IEnvelope pEnvelope=new ESRI.ArcGIS.Geometry.EnvelopeClass();
                    while (pFeature != null)
                    {
                        pEnvelope.Union(pFeature.Extent);
                        pFeature = pEnumFeature.Next();
                    }
                    MainAxMapControl.ActiveView.Extent = pEnvelope;
                    MainAxMapControl.ActiveView.Refresh();//���������ˢ�£�ֻҪ��ѯǰ��ͼ�Ѿ����Ŵ���Ч���Ļ�����λ��
                                                          //��ͼû��ˢ�£�ѡ�񼯵��Ƕ�λ��ˢ����
                }
                else MainAxMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);

                //double i = MainAxMapControl.Map.SelectionCount;
                //i = Math.Round(i, 0);//С�����ָ��Ϊ��λ����
                //pMainform = "��ǰ����" + i.ToString() + "����ѯ���";
            }
            catch (Exception ex)
            {
                MessageBox.Show("���Ĳ�ѯ����������,���� | " + ex.Message);
                return;
            }
        }
        #endregion

        private void checkBoxShowVectorOnly_CheckedChanged(object sender, EventArgs e)
        {
            //��ֻ��ʾʸ�����ݵĸ�ѡ��ı��ʱ��,��Ҫ���¼���ͼ������ͼ���б���
            AddAllLayerstoComboBox(comboBoxLayers);
            if (comboBoxLayers.Items.Count != 0)
                comboBoxLayers.SelectedIndex = 0;//��combox�ĵ�ǰѡ�е���Ŀ��Ϊ��һ�������Ĭ��ѡ�е�һ�����Ļ�Ϊ�գ�������
            //�任checkboxShowVectorOnly�����listBox�б���գ��Ѿ��Ľ���������⣩��������listBOXfields������
            //��comboxΪ�յ�����������������������ʱ����Ҫ�г�����ֵ��������Ҫ����combox��
            //��ǰֵ����ȡͼ�㣬�����listBoxFields_SelectedIndexChanged(object sender, EventArgs e)
            //������ֵΪ�գ��ͻ������
            listBoxFields.Items.Clear();//Ϊʲô��ô������AddAllLayersCombox��ע��
        }

        private void comboBoxLayers_SelectedIndexChanged(object sender, EventArgs e)
        {   
            listBoxFields.Items.Clear();
            listBoxValues.Items.Clear();
            string strSelectedLayerName = comboBoxLayers.Text;
            IFeatureLayer pFeatureLayer;

            try
            {
                for (int i = 0; i <= MainAxMapControl.LayerCount-1; i++)
                {
                    if (MainAxMapControl.get_Layer(i).Name == strSelectedLayerName)
                    {
                        if (MainAxMapControl.get_Layer(i) is IFeatureLayer)
                        {
                            pFeatureLayer = MainAxMapControl.get_Layer(i) as IFeatureLayer;

                            for (int j = 0; j <= pFeatureLayer.FeatureClass.Fields.FieldCount - 1; j++)
                            {
                                listBoxFields.Items.Add(pFeatureLayer.FeatureClass.Fields.get_Field(j).Name);
                            }

                            labelDescription2.Text = strSelectedLayerName;
                        }
                        else
                        { MessageBox.Show("��ѡ���ͼ�㲻�ܹ��������Բ�ѯ!������ѡ��"); break; }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void buttonGetValue_Click(object sender, EventArgs e)
        {
            if (listBoxFields.Text == "")
                MessageBox.Show("��ѡ��һ�������ֶΣ�");
            else
            {
                try
                {
                    frmPromptGetValues frm = new frmPromptGetValues();//�����Լ�¼�ǳ����ʱ�򣨱���˵������������
                    //���ٶȻ��������arcmap��Ҳ���������������һ����ʾ���ڣ�����������������
                    frm.Show();
                    System.Windows.Forms.Application.DoEvents();
                    string strSelectedFieldName = listBoxFields.Text;//���������ѡ�е������ֶε�����
                    listBoxValues.Items.Clear();
                    label1.Text = "";

                    //IQueryFilter pQueryFilter = new QueryFilterClass();
                    IFeatureCursor pFeatureCursor;
                    IFeatureClass pFeatureClass;
                    IFeature pFeature;
                    double i = 0;//��¼����
                    if (strSelectedFieldName != null)
                    {

                        pFeatureClass = (GetLayerByName(comboBoxLayers.Text) as IFeatureLayer).FeatureClass;
                        pFeatureCursor = pFeatureClass.Search(null, true);
                        pFeature = pFeatureCursor.NextFeature();
                        int index = pFeatureClass.FindField(strSelectedFieldName);
                        while (pFeature != null)
                        {
                            i++;
                            string strValue = pFeature.get_Value(index).ToString();
                            if (checkBoxGetUniqueValue.Checked)   //���ȥ���ظ���ֵ
                            {
                                if (pFeature.Fields.get_Field(index).Type == esriFieldType.esriFieldTypeString)
                                    //��ΪpFeature.get_Field().����û��Type�����ԣ����Ե���pFeature.fileds.XXXX
                                    strValue = "'" + strValue + "'";//�������ֵ���ַ��������' '���������whereclause�ĸ�ʽ
                                if (listBoxValues.FindStringExact(strValue) == ListBox.NoMatches)
                                {
                                    listBoxValues.Items.Add(strValue);
                                }
                            }
                            else                                  //����������е�ֵ��������û���ظ�
                            {
                                if (pFeature.Fields.get_Field(index).Type == esriFieldType.esriFieldTypeString)
                                    strValue = "'" + strValue + "'";//�������ֵ���ַ��������' '���������whereclause�ĸ�ʽ
                                listBoxValues.Items.Add(strValue);
                            }

                            if (i % 50 == 0)    //ÿ��ʮ����¼��ʾ���ڸ���һ��
                            {
                                System.Windows.Forms.Application.DoEvents();//���ģ��ⷽ�������ҷ����ˣ����������﷢�ֵģ�
                                //���doevents�Ĺ����ǣ�When you run a Windows Form, it creates the new form, 
                                //which then waits for events to handle. Each time the form handles an event, 
                                //it processes all the code associated with that event. All other events wait in the queue.
                                //While your code handles the event, your application does not respond. For example, 
                                //    the window does not repaint if another window is dragged on top.
                                //�������Ϊ���ѵ�ǰ�Ŀ���Ȩ���������������Ĵ��루�¼��������������ʾ���������label�ſ��ܲ��ϱ仯
                                //���򣬾͸�ʧȥ��Ӧ��һ�������ǽ���������ʾ���ڣ�����label�ǲ�����µ�
                                frm.labelname.Text = strValue;      //��ʾ������ʾ��ǰ������ӵ��ֶ�
                                frm.labelcount.Text = i.ToString();//��ʾ������ʾ��ǰ������ӵĵڼ�����¼
                            }

                            pFeature = pFeatureCursor.NextFeature();
                        }
                    }
                    frm.Dispose();
                    label1.Text = listBoxValues.Items.Count.ToString() + "����¼";

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }

        //private void listBoxFields_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    ԭ�����������ȡĳ�ֶε�����ֵ���������Ļ������˫����ҵ�ض������˴��¼���������ֵ����ʱ�ͺ��鷳��
        //}

        private void listBoxFields_DoubleClick(object sender, EventArgs e)
        {
            //�˴���selectedtext����ֱ�ӻ����Ҫ��Ч��������textbox.text�Ļ���Ҫ����һЩ�����Ƚ��鷳
            //������selectedtext�ڰ����������ȴ����������Ϊʲô����
            textBoxWhereClause.SelectedText = listBoxFields.SelectedItem.ToString() + " ";
        }

        #region ��굥��/˫������Whereclause�Ĳ���
        private void listBoxValues_DoubleClick(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = " " + listBoxValues.SelectedItem.ToString();
        }

        private void buttonEqual_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = " = ";
        }

        private void buttonNotEqual_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = " <> ";
        }

        private void buttonBig_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = " > ";
        }

        private void buttonBigEqual_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = " >= ";
        }

        private void buttonSmall_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = " < ";
        }

        private void buttonSmallEqual_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = " <= ";
        }

        private void buttonChars_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = "%";
        }

        private void buttonChar_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = "_";
        }

        private void buttonLike_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = " Like ";
        }

        private void buttonAnd_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = " And ";
        }

        private void buttonOr_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = " Or ";
        }

        private void buttonNot_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = " Not ";
        }

        private void buttonIs_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = " Is ";
        }


        private void buttonBrace_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.SelectedText = "(  )";
            //�������λ��ǡ�ô��ڣ������棬��ͬarcmap��Ч��һ��
            textBoxWhereClause.SelectionStart = textBoxWhereClause.Text.Length - 2;
        }
        #endregion

        private void comboBoxMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBoxMethod.SelectedIndex)
            {
                case 0: selectmethod = esriSelectionResultEnum.esriSelectionResultNew; break;
                case 1: selectmethod = esriSelectionResultEnum.esriSelectionResultAdd; break;
                case 2: selectmethod = esriSelectionResultEnum.esriSelectionResultSubtract; break;
                case 3: selectmethod = esriSelectionResultEnum.esriSelectionResultAnd; break;
            }
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (textBoxWhereClause.Text == "")
            {
                MessageBox.Show("�����ɲ�ѯ��䣡");
                return;
            }
            this.WindowState = FormWindowState.Minimized;//ͨ��λ�ò�ѯ������С��

            frmPromptQuerying frmPrompt = new frmPromptQuerying();
            frmPrompt.Show();
            System.Windows.Forms.Application.DoEvents();//'ת�ÿ���Ȩ��û����һ��Ļ���ʾ���ڲ���������ʾ
            
            PerformAttributeSelect();
            frmPrompt.Dispose();
            this.WindowState = FormWindowState.Normal;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;//ͨ��λ�ò�ѯ������С��

            frmPromptQuerying frmPrompt = new frmPromptQuerying();
            frmPrompt.Show();
            System.Windows.Forms.Application.DoEvents();//'ת�ÿ���Ȩ��û����һ��Ļ���ʾ���ڲ���������ʾ
            
            PerformAttributeSelect();
            frmPrompt.Dispose();
            this.Dispose();
            pMainform.frmAttriQueryisOpen = false;//�����ڹر�ʱ���˱���Ϊfalse��֪ϵͳ��ǰû�д򿪵����Բ�ѯ����
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
            pMainform.frmAttriQueryisOpen = false;//�����ڹر�ʱ���˱���Ϊfalse��֪ϵͳ��ǰû�д򿪵����Բ�ѯ����
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxWhereClause.Clear();
        }

        private void frmAttriQuery_FormClosed(object sender, FormClosedEventArgs e)
        {
            pMainform.frmAttriQueryisOpen = false;//�����ڹر�ʱ���˱���Ϊfalse��֪ϵͳ��ǰû�д򿪵����Բ�ѯ����
        }
    }
}