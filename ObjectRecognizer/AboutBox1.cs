using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ObjectRecognizer
{
    partial class AboutBox1 : Form
    {
        public AboutBox1()
        {
            InitializeComponent();
            this.Text = String.Format("О программе {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Версия {0}", AssemblyVersion);
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyCompany;
            this.textBoxDescription.Text = AssemblyDescription;
        }

        #region Методы доступа к атрибутам сборки

        public string AssemblyTitle
        {
            get
            {
                return "Распознавание объектов";
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return "Версия 1,0";
            }
        }

        public string AssemblyDescription
        {
            get
            {
                return "Данная программа предназначена для " +
                        "распознавания объектов на картинках и видео в режиме реального времени " +
                        "и нахождения их площади, отновительно ширины и высоты кадра.";
            }
        }

        public string AssemblyProduct
        {
            get
            {
                return string.Empty;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                return "Авторские права принадлежат студенту 2 курса ИКПИ-84 Цывакину Даниилу";
            }
        }

        public string AssemblyCompany
        {
            get
            {
                return string.Empty;
            }
        }
        #endregion

        private void tableLayoutPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
