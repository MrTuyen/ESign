using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Position
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnPosition_Click(object sender, EventArgs e)
        {
            PdfReader reader = null;
            //======================================================
            // Mở file PDF, lấy các trường chữ ký
            //======================================================

            //1. mở file, đưa vào PdfReader
            reader = new PdfReader(txtFile.Text);
            AcroFields af = reader.AcroFields;

            //Lấy danh sách tên trường chữ ký
            List<string> SignatureNames = af.GetSignatureNames();

            //vtnam test: lấy thông tin vị trí chữ ký
            foreach (string signName in SignatureNames)
            {
                //lấy vị trí của trường chữ ký: tìm theo tên trường
                IList<AcroFields.FieldPosition> positions = af.GetFieldPositions(signName);
                //vị trí lưu trong phần tử đầu tiên
                iTextSharp.text.Rectangle rect = positions[0].position;
                //lấy góc dưới trái, góc trên  phải, rộng và cao
                float llX = rect.Left;
                float llY = rect.Bottom;
                float urX = rect.Right;
                float urY = rect.Top;
                txtllX.Text = llX.ToString();
                txtllY.Text = llY.ToString();
                txturX.Text = urX.ToString();
                txturY.Text = urY.ToString();

                //float _width = rect.Width;
                //float _height = rect.Height;
                //if (signFields[indx].Contains(KYNHAY_FIELD) == true) { nextKynhay++; };
                break;
            }
        }
    }
}
