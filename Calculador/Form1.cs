using System.Windows.Forms;
using System.Data;
using System.Text.RegularExpressions;
using Tesseract;
using System.Speech.Recognition;
using NAudio.Wave;

namespace Calculador
{
    public partial class Calculadora : Form
    {
        private double memoria = 0;
        private WaveInEvent waveIn;
        private WaveFileWriter waveWriter;
        private SpeechRecognitionEngine recognizer;
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
        private void btnAbPa_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "(";
        }
        private void btnCiPa_Click(object sender, EventArgs e)
        {
            txtOpe.Text += ")";
        }

        private void btnC_Click(object sender, EventArgs e)
        {
            txtOpe.Text = "";
            txtRespu.Text = "";
        }
        private void btnCE_Click(object sender, EventArgs e)
        {
            if (txtOpe.Text.Length > 0)
                txtOpe.Text = txtOpe.Text.Substring(0, txtOpe.Text.Length - 1);
        }
        private void btnSIN_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "sin";
        }
        private void btnRaiz_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "raiz";
        }
        private void btnSuma_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "+";
        }
        private void btnResta_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "-";
        }
        private void btnMulti_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "*";
        }
        private void btnDividir_Click(object sender, EventArgs e)
        {
            txtOpe.Text += "/";
        }
        private void btnM_Click(object sender, EventArgs e)
        {
            if (double.TryParse(txtRespu.Text, out double valorMemoria))
            {
                memoria = valorMemoria;
            }
        }
        private void btnMR_Click(object sender, EventArgs e)
        {
            txtOpe.Text += memoria.ToString();
        }
        private void btnIgual_Click(object sender, EventArgs e)
        {
            try
            {
                string expresion = txtOpe.Text.Replace("π", Math.PI.ToString());

                expresion = System.Text.RegularExpressions.Regex.Replace(
                    expresion,
                    @"sin\(([^)]+)\)",
                    m => Math.Sin(Convert.ToDouble(m.Groups[1].Value) * Math.PI / 180).ToString()
                );

                expresion = System.Text.RegularExpressions.Regex.Replace(
                    expresion,
                    @"raiz\(([^)]+)\)",
                    m => Math.Sqrt(Convert.ToDouble(m.Groups[1].Value)).ToString()
                );

                DataTable dt = new DataTable();
                var resultado = dt.Compute(expresion, "");

                txtRespu.Text = resultado.ToString();
            }
            catch (Exception)
            {
                txtRespu.Text = "Error";
            }
        }

        private void btnCamara_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Title = "Seleccionar imagen con expresión matemática";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string tessDataPath = "./tessdata";
                        MessageBox.Show($"Ruta de tessdata: {Path.GetFullPath(tessDataPath)}");

                        using (var engine = new TesseractEngine(tessDataPath, "spa", EngineMode.Default))
                        {
                            using (var img = Pix.LoadFromFile(openFileDialog.FileName))
                            {
                                using (var page = engine.Process(img))
                                {
                                    string texto = page.GetText().Trim();
                                    texto = Regex.Replace(texto, @"[^0-9+\-*/().,]", "");
                                    txtOpe.Text = texto;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error detallado al procesar imagen: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                            "Error Completo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void btnAudio_Click(object sender, EventArgs e)
        {
            try
            {
                recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("es-ES"));

                Choices numeros = new Choices(new string[] { "cero", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve" });
                Choices operadores = new Choices(new string[] { "más", "menos", "por", "dividido", "entre", "abre paréntesis", "cierra paréntesis" });

                GrammarBuilder builder = new GrammarBuilder();
                builder.Append(numeros);
                builder.Append(operadores);

                Grammar grammar = new Grammar(builder);
                recognizer.LoadGrammar(grammar);

                var audioDevices = WaveIn.DeviceCount;
                MessageBox.Show($"Dispositivos de audio detectados: {audioDevices}");

                recognizer.SetInputToDefaultAudioDevice();
                recognizer.SpeechRecognized += Recognizer_SpeechRecognized;

                MessageBox.Show("Comenzando reconocimiento de voz. Hable ahora.", "Reconocimiento de Voz", MessageBoxButtons.OK, MessageBoxIcon.Information);
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error detallado al iniciar reconocimiento de voz: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                    "Error Completo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string palabraReconocida = e.Result.Text;
            string caracterTraducido = TraducirPalabra(palabraReconocida);

            if (!string.IsNullOrEmpty(caracterTraducido))
            {
                txtOpe.Text += caracterTraducido;
            }
        }
        private string TraducirPalabra(string palabra)
        {
            switch (palabra.ToLower())
            {
                case "cero": return "0";
                case "uno": return "1";
                case "dos": return "2";
                case "tres": return "3";
                case "cuatro": return "4";
                case "cinco": return "5";
                case "seis": return "6";
                case "siete": return "7";
                case "ocho": return "8";
                case "nueve": return "9";
                case "más":
                case "mas":
                case "suma":
                case "sumar": return "+";
                case "menos":
                case "resta":
                case "restar": return "-";
                case "por":
                case "multiplicado por":
                case "multiplicar": return "*";
                case "entre":
                case "dividido":
                case "dividido por":
                case "dividir": return "/";
                case "igual":
                case "resultado":
                case "calcular": return "=";
                case "seno": return "sin";
                case "raíz":
                case "raíz cuadrada":
                case "raiz":
                case "raiz cuadrada": return "√";
                case "abrir paréntesis":
                case "abre paréntesis":
                case "parentesis abierto": return "(";
                case "cerrar paréntesis":
                case "cierra paréntesis":
                case "parentesis cerrado": return ")";
                case "punto": return ".";
                case "pi": return "3.14159";
                case "borrar": return "CE";
                case "borrar todo":
                case "limpiar": return "C";
                default: return "";
            }
        }
    }
}
