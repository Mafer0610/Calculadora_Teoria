using System.Data;
using System.Speech.Recognition;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Calculador
{
    public partial class Calculadora : Form
    {
        private double memoria = 0;
        private SpeechRecognitionEngine? reconocedor;
        private bool escuchando = false;

        public Calculadora()
        {
            InitializeComponent();
            ConfigurarReconocimientoVoz();
            
            btnAudio.Click += BtnAudio_Click;
        }

        private void ConfigurarReconocimientoVoz()
        {
            try
            {
                if (!SpeechRecognitionEngine.InstalledRecognizers().Any())
                {
                    DesactivarFuncionVoz("No hay reconocedores instalados");
                    return;
                }

                var recognizerInfo = SpeechRecognitionEngine.InstalledRecognizers()
                    .FirstOrDefault(r => r.Culture.TwoLetterISOLanguageName == "es") ?? 
                    SpeechRecognitionEngine.InstalledRecognizers().First();

                reconocedor = new SpeechRecognitionEngine(recognizerInfo.Id);
                
                var gramatica = new GrammarBuilder();
                gramatica.Append(CrearGramaticaCalculadora());
                gramatica.Culture = recognizerInfo.Culture;

                reconocedor.LoadGrammar(new Grammar(gramatica));
                reconocedor.SpeechRecognized += Reconocedor_SpeechRecognized;
                reconocedor.SpeechRecognitionRejected += Reconocedor_Rechazado;
                
                reconocedor.SetInputToDefaultAudioDevice();
                reconocedor.BabbleTimeout = TimeSpan.FromSeconds(1.5);
                reconocedor.InitialSilenceTimeout = TimeSpan.FromSeconds(2);
            }
            catch (Exception ex)
            {
                DesactivarFuncionVoz($"Error: {ex.Message}");
            }
        }

        private Choices CrearGramaticaCalculadora()
        {
            var numeros = new Choices("0", "1", "2", "3", "4", "5", "6", "7", "8", "9");
            var operaciones = new Choices("más", "menos", "por", "dividido", "entre", "sin", "raíz", "sqrt");
            var comandos = new Choices("punto", "pi", "abre paréntesis", "cierra paréntesis", 
                                     "borrar", "borra todo", "igual", "memoria", "recupera memoria",
                                     "clear", "eliminar");

            return new Choices(numeros, operaciones, comandos);
        }

        private void DesactivarFuncionVoz(string mensaje)
        {
            if (btnAudio.InvokeRequired)
            {
                btnAudio.Invoke(new Action(() => DesactivarFuncionVoz(mensaje)));
                return;
            }
            
            btnAudio.Enabled = false;
            btnAudio.Text = mensaje;
            btnAudio.BackColor = Color.LightGray;
            
            reconocedor?.Dispose();
            reconocedor = null;
        }

        private void Reconocedor_Rechazado(object? sender, SpeechRecognitionRejectedEventArgs e)
        {
            MostrarMensaje("No entendí, por favor repite");
        }

        private void Reconocedor_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            this.Invoke((MethodInvoker)(() => 
            {
                if (e.Result?.Text != null)
                {
                    ProcesarComandoVoz(e.Result.Text.ToLower());
                }
            }));
        }

        private void ProcesarComandoVoz(string comando)
        {
            switch (comando)
            {
                case "0": case "1": case "2": case "3": case "4":
                case "5": case "6": case "7": case "8": case "9":
                    txtOpe.Text += comando;
                    break;
                case "punto":
                    txtOpe.Text += ".";
                    break;
                case "pi":
                    txtOpe.Text += Math.PI.ToString();
                    break;
                case "abre paréntesis":
                    txtOpe.Text += "(";
                    break;
                case "cierra paréntesis":
                    txtOpe.Text += ")";
                    break;
                case "más":
                    txtOpe.Text += "+";
                    break;
                case "menos":
                    txtOpe.Text += "-";
                    break;
                case "por":
                    txtOpe.Text += "*";
                    break;
                case "dividido": case "entre":
                    txtOpe.Text += "/";
                    break;
                case "sin":
                    txtOpe.Text += "sin(";
                    break;
                case "raíz": case "sqrt":
                    txtOpe.Text += "sqrt(";
                    break;
                case "borrar": case "eliminar":
                    if (txtOpe.Text.Length > 0)
                        txtOpe.Text = txtOpe.Text[..^1];
                    break;
                case "borra todo": case "clear":
                    txtOpe.Text = "";
                    txtRespu.Text = "";
                    break;
                case "igual":
                    btnIgual_Click(this, EventArgs.Empty);
                    break;
                case "memoria":
                    btnM_Click(this, EventArgs.Empty);
                    break;
                case "recupera memoria":
                    btnMR_Click(this, EventArgs.Empty);
                    break;
                default:
                    MostrarMensaje($"Comando no reconocido: {comando}");
                    break;
            }
        }

        private void MostrarMensaje(string mensaje)
        {
            if (txtRespu.InvokeRequired)
            {
                txtRespu.Invoke(new Action(() => MostrarMensaje(mensaje)));
                return;
            }
            
            txtRespu.Text = mensaje;
            var timer = new System.Windows.Forms.Timer { Interval = 2000 };
            timer.Tick += (s, e) => { txtRespu.Text = ""; timer.Stop(); };
            timer.Start();
        }

        private void BtnAudio_Click(object? sender, EventArgs e)
        {
            if (reconocedor == null)
            {
                MessageBox.Show("Reconocimiento de voz no disponible");
                return;
            }

            escuchando = !escuchando;
            
            if (escuchando)
            {
                try
                {
                    reconocedor.RecognizeAsync(RecognizeMode.Multiple);
                    btnAudio.BackColor = Color.LimeGreen;
                    btnAudio.Text = "Escuchando...";
                    MostrarMensaje("Di tu operación (ej: '5 más 3 igual')");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                    DesactivarFuncionVoz("Error voz");
                }
            }
            else
            {
                reconocedor?.RecognizeAsyncStop();
                btnAudio.BackColor = Color.FromArgb(153, 153, 153);
                btnAudio.Text = "Voz";
                MostrarMensaje("Micrófono desactivado");
            }
        }

        // Métodos de los botones 
        private void btn9_Click(object sender, EventArgs e) => txtOpe.Text += "9";
        private void btn8_Click(object sender, EventArgs e) => txtOpe.Text += "8";
        private void btn7_Click(object sender, EventArgs e) => txtOpe.Text += "7";
        private void btn6_Click(object sender, EventArgs e) => txtOpe.Text += "6";
        private void btn5_Click(object sender, EventArgs e) => txtOpe.Text += "5";
        private void btn4_Click(object sender, EventArgs e) => txtOpe.Text += "4";
        private void btn3_Click(object sender, EventArgs e) => txtOpe.Text += "3";
        private void btn2_Click(object sender, EventArgs e) => txtOpe.Text += "2";
        private void btn1_Click(object sender, EventArgs e) => txtOpe.Text += "1";
        private void btn0_Click(object sender, EventArgs e) => txtOpe.Text += "0";
        private void btnPunto_Click(object sender, EventArgs e) => txtOpe.Text += ".";
        private void btnPi_Click(object sender, EventArgs e) => txtOpe.Text += Math.PI.ToString();
        private void btnAbPa_Click(object sender, EventArgs e) => txtOpe.Text += "(";
        private void btnCiPa_Click(object sender, EventArgs e) => txtOpe.Text += ")";
        private void btnC_Click(object sender, EventArgs e) { txtOpe.Text = ""; txtRespu.Text = ""; }
        private void btnCE_Click(object sender, EventArgs e)
        {
            if (txtOpe.Text.Length > 0)
                txtOpe.Text = txtOpe.Text[..^1];
        }
        private void btnSIN_Click(object sender, EventArgs e) => txtOpe.Text += "sin(";
        private void btnRaiz_Click(object sender, EventArgs e) => txtOpe.Text += "sqrt(";
        private void btnSuma_Click(object sender, EventArgs e) => txtOpe.Text += "+";
        private void btnResta_Click(object sender, EventArgs e) => txtOpe.Text += "-";
        private void btnMulti_Click(object sender, EventArgs e) => txtOpe.Text += "*";
        private void btnDividir_Click(object sender, EventArgs e) => txtOpe.Text += "/";
        
        private void btnM_Click(object sender, EventArgs e)
        {
            if (double.TryParse(txtRespu.Text, out double valorMemoria))
            {
                memoria = valorMemoria;
                MostrarMensaje("Valor guardado en memoria");
            }
        }
        
        private void btnMR_Click(object sender, EventArgs e) => txtOpe.Text += memoria.ToString();
        
        private void btnIgual_Click(object sender, EventArgs e)
        {
            try
            {
                string expresion = txtOpe.Text.Replace("π", Math.PI.ToString());

                expresion = Regex.Replace(
                    expresion,
                    @"sin\(([^)]+)\)",
                    m => Math.Sin(Convert.ToDouble(m.Groups[1].Value) * Math.PI / 180).ToString()
                );

                expresion = Regex.Replace(
                    expresion,
                    @"sqrt\(([^)]+)\)",
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            reconocedor?.RecognizeAsyncStop();
            reconocedor?.Dispose();
            base.OnFormClosing(e);
        }
    }
}