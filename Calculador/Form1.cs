namespace Calculador
{
    public partial class Calculadora : Form
    {
        public Calculadora()
        {
            InitializeComponent();
        }
        private void btn9_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "9";
        }
        private void btn8_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "8";
        }
        private void btn7_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "7";
        }
        private void btn6_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "6";
        }
        private void btn5_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "5";
        }
        private void btn4_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "4";
        }

        private void btn3_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "3";
        }
        private void btn2_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "2";
        }
        private void btn1_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "1";
        }
        private void btn0_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "0";
        }
        private void btnPunto_Click(object sender, EventArgs e)
        {
            txtOpe.Text += ".";
        }
        private void btnPi_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "3.14159";
        }
    }
}
