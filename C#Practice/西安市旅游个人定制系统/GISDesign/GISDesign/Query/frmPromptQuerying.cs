using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GISDesign.Query
{
    public partial class frmPromptQuerying : Form
    {
        public frmPromptQuerying()
        {
            InitializeComponent();
        }
        public frmPromptQuerying(int i)
        {
            InitializeComponent();
            if (i == 1)
                label.Text = "���ڼ��ص�ͼ�����Ժ�...";
            if (i == 2)
                label.Text = "���ڼ������ݣ����Ժ�...";
            if (i == 3)
                label.Text = "���ڽ��д������Ժ�...";
            if(i==4)
                label.Text = "�����ѯ���������Ժ�...";
            if (i == 5)
                label.Text = "���пռ��ѯ�����Ժ�...";
        }

        private void frmPromptQuerying_Load(object sender, EventArgs e)
        {

        }
    }
}