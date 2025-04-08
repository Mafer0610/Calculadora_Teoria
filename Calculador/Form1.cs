using System;
using System.Data;
using System.Speech.Recognition;
using System.Text.RegularExpressions;
using AForge.Video;
using AForge.Video.DirectShow;
using Tesseract;
namespace Calculador
{
    public partial class Calculadora : Form
    {
        private double memoria = 0;
        private SpeechRecognitionEngine? reconocedor;
        private bool escuchando = false;
        private FilterInfoCollection? dispositivosVideo;
        private VideoCaptureDevice? fuenteVideo;
        private Bitmap? imagenActual;

        public Calculadora()
        {
            InitializeComponent();
            ConfigurarReconocimientoVoz();
        }

        private void ConfigurarReconocimientoVoz()
        {
            try
            {
                // 1. Verificar soporte de voz
                if (!SpeechRecognitionEngine.InstalledRecognizers()
                    .Any(r => r.Culture.TwoLetterISOLanguageName == "es"))
                {
                    DesactivarFuncionVoz("Idioma no soportado");
                    return;
                }

                // 2. Configurar reconocedor
                reconocedor = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("es-ES"));

                // 3. Mejorar gramática con dictado
                var gramatica = new DictationGrammar();
                reconocedor.LoadGrammar(gramatica);

                // 4. Agregar gramática específica
                var gramaticaCalculadora = CrearGramaticaCalculadora();
                reconocedor.LoadGrammar(gramaticaCalculadora);

                // 5. Configurar eventos
                reconocedor.SpeechRecognized += Reconocedor_SpeechRecognized;
                reconocedor.SpeechRecognitionRejected += Reconocedor_Rechazado;

                // 6. Configurar parámetros de audio
                reconocedor.SetInputToDefaultAudioDevice();
                reconocedor.BabbleTimeout = TimeSpan.FromSeconds(2);
                reconocedor.InitialSilenceTimeout = TimeSpan.FromSeconds(3);
            }
            catch (Exception ex)
            {
                DesactivarFuncionVoz($"Error: {ex.Message}");
            }
        }

        private Grammar CrearGramaticaCalculadora()
        {
            var numeros = new Choices("0", "1", "2", "3", "4", "5", "6", "7", "8", "9");
            var operaciones = new Choices("más", "menos", "por", "dividido", "entre", "sin", "raíz", "sqrt");
            var comandos = new Choices("punto", "pi", "abre paréntesis", "cierra paréntesis",
                                    "borrar", "borra todo", "igual", "memoria", "recupera memoria",
                                    "clear", "eliminar");

            var gramatica = new GrammarBuilder();
            gramatica.Append(new Choices(numeros, operaciones, comandos));
            gramatica.Culture = new System.Globalization.CultureInfo("es-ES");

            return new Grammar(gramatica);
        }

        private void DesactivarFuncionVoz(string mensaje)
        {
            btnAudio.Enabled = false;
            btnAudio.Text = mensaje;
            btnAudio.BackColor = Color.LightGray;
            reconocedor = null;
        }

        private void Reconocedor_Rechazado(object? sender, SpeechRecognitionRejectedEventArgs e)
        {
            MostrarMensaje("No entendí, por favor repite");
        }

        private void Reconocedor_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result == null || e.Result.Confidence < 0.7) return;

            string texto = e.Result.Text.ToLower();

            Invoke((MethodInvoker)(() => ProcesarComandoVoz(texto)));
        }

        private void ProcesarComandoVoz(string comando)
        {
            switch (comando)
            {
                case "0":
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                    txtOpe.Text += comando;
                    break;
                case "punto":
                    txtOpe.Text += ".";
                    break;
                case "pi":
                    txtOpe.Text += "3.14159";
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
                case "dividido":
                case "entre":
                    txtOpe.Text += "/";
                    break;
                case "sin":
                    txtOpe.Text += "sin";
                    break;
                case "raíz":
                case "sqrt":
                    txtOpe.Text += "raiz";
                    break;
                case "borrar":
                case "eliminar":
                    if (txtOpe.Text.Length > 0)
                        txtOpe.Text = txtOpe.Text[..^1];
                    break;
                case "borra todo":
                case "clear":
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
            txtRespu.Text = mensaje;
            Task.Delay(2000).ContinueWith(_ => Invoke((MethodInvoker)(() => txtRespu.Text = "")));
        }

        private void btnAudio_Click(object? sender, EventArgs e)
        {
            if (reconocedor == null)
            {
                MessageBox.Show("Reconocimiento de voz no disponible");
                return;
            }

            if (!escuchando)
            {
                try
                {
                    reconocedor.RecognizeAsync(RecognizeMode.Multiple);
                    btnAudio.BackColor = Color.LimeGreen;
                    btnAudio.Text = "Escuchando...";
                    escuchando = true;
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
                reconocedor.RecognizeAsyncStop();
                btnAudio.BackColor = Color.FromArgb(153, 153, 153);
                btnAudio.Text = "Voz";
                escuchando = false;
                MostrarMensaje("Micrófono desactivado");
            }
        }

        // Métodos existentes de los botones (se mantienen igual)
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            reconocedor?.RecognizeAsyncStop();
            reconocedor?.Dispose();
        }

        private void btnCamara_Click(object sender, EventArgs e)
        {
            IniciarCamara();
            btnAnalizar.Visible = true;
            btnApagar.Visible = true;
            btnPausar.Visible = true;
            pictureBoxCamara.Visible = true;
        }
        private void btnApagar_Click(object sender, EventArgs e)
        {
            DetenerCamara();
            btnAnalizar.Visible = false;
            btnApagar.Visible = false;
            btnPausar.Visible = false;
            pictureBoxCamara.Visible = false;
        }
        private void btnAnalizar_Click(object sender, EventArgs e)
        {
            CapturarYLeerTexto();
        }

        private void IniciarCamara()
        {
            dispositivosVideo = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (dispositivosVideo.Count == 0)
            {
                MessageBox.Show("No se detectó cámara.");
                return;
            }

            fuenteVideo = new VideoCaptureDevice(dispositivosVideo[0].MonikerString);
            fuenteVideo.NewFrame += new NewFrameEventHandler(Video_NuevoFrame);
            fuenteVideo.Start();
        }

        private void Video_NuevoFrame(object sender, NewFrameEventArgs eventArgs)
        {
            imagenActual?.Dispose();
            imagenActual = (Bitmap)eventArgs.Frame.Clone();
            pictureBoxCamara.Image = (Bitmap)imagenActual.Clone();
        }
        private void CapturarYLeerTexto()
        {
            if (imagenActual == null)
            {
                MessageBox.Show("No hay imagen para procesar.");
                return;
            }

            try
            {
                using var engine = new TesseractEngine(@"./tessdata", "spa", EngineMode.Default);
                using var img = PixConverter.ToPix(imagenActual);
                using var page = engine.Process(img);
                string textoReconocido = page.GetText();

                txtOpe.Text = textoReconocido.Replace("\n", "").Replace(" ", "");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al leer texto: " + ex.Message);
            }
        }
        private void DetenerCamara()
        {
            if (fuenteVideo != null && fuenteVideo.IsRunning)
            {
                fuenteVideo.SignalToStop();
                fuenteVideo.WaitForStop();
            }
        }


    }
}