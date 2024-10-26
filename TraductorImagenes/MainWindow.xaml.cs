using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Drawing;
using Point = System.Windows.Point;
using System.Diagnostics;
using System.Text;
using Azure.AI.Vision.ImageAnalysis;
using System.IO;
using Azure;
using TraductorImagenes.Properties;
using System.Net.Http;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Rectangle = System.Drawing.Rectangle;
using System.Drawing.Imaging;
using System.Windows.Interop;
using System.Threading;
using System.Net;
using System.Text.Json;
using System.Runtime.InteropServices;


namespace TraductorImagenes
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private bool isResizing = false;
        private bool isMoving = false;
        private Point startPoint;
        private ResizeDirection currentResizeDirection;
        private const int RESIZE_BORDER = 5;
        private enum ResizeDirection
        {
            None, Left, Right, Top, Bottom,
            TopLeft, TopRight, BottomLeft, BottomRight
        }

        public MainWindow()
        {
            InitializeComponent();

            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
            KeyDown += OnKeyDown;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(this);
                currentResizeDirection = GetResizeDirection(mousePos);

                if (currentResizeDirection != ResizeDirection.None)
                {
                    isResizing = true;
                    startPoint = mousePos;
                }
                else
                {
                    isMoving = true;
                    // Guardamos la posición del ratón relativa a la ventana
                    startPoint = mousePos; // <--- Update this line
                }

                CaptureMouse();
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isResizing = false;
            isMoving = false;
            ReleaseMouseCapture();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!isResizing && !isMoving)
            {
                // Actualizar el cursor según la posición
                Cursor = GetResizeCursor(GetResizeDirection(e.GetPosition(this)));
                return;
            }

            if (isResizing)
            {
                Point currentPoint = e.GetPosition(this);
                double deltaX = currentPoint.X - startPoint.X;
                double deltaY = currentPoint.Y - startPoint.Y;

                switch (currentResizeDirection)
                {
                    case ResizeDirection.Right:
                        Width = Math.Max(20, Width + deltaX);
                        break;
                    case ResizeDirection.Bottom:
                        Height = Math.Max(20, Height + deltaY);
                        break;
                    case ResizeDirection.Left:
                        double newWidthLeft = Math.Max(20, Width - deltaX);
                        Left += Width - newWidthLeft;
                        Width = newWidthLeft;
                        break;
                    case ResizeDirection.Top:
                        double newHeightTop = Math.Max(20, Height - deltaY);
                        Top += Height - newHeightTop;
                        Height = newHeightTop;
                        break;
                    case ResizeDirection.TopLeft:
                        newWidthLeft = Math.Max(20, Width - deltaX);
                        newHeightTop = Math.Max(20, Height - deltaY);
                        Left += Width - newWidthLeft;
                        Top += Height - newHeightTop;
                        Width = newWidthLeft;
                        Height = newHeightTop;
                        break;
                    case ResizeDirection.TopRight:
                        Width = Math.Max(20, Width + deltaX);
                        newHeightTop = Math.Max(20, Height - deltaY);
                        Top += Height - newHeightTop;
                        Height = newHeightTop;
                        break;
                    case ResizeDirection.BottomLeft:
                        newWidthLeft = Math.Max(20, Width - deltaX);
                        Width = newWidthLeft;
                        Left += deltaX;
                        Height = Math.Max(20, Height + deltaY);
                        break;
                    case ResizeDirection.BottomRight:
                        Width = Math.Max(20, Width + deltaX);
                        Height = Math.Max(20, Height + deltaY);
                        break;
                }

                startPoint = currentPoint;
            }
            else if (isMoving)
            {
                // Obtener la posición actual del ratón RELATIVA A LA VENTANA
                Point currentPoint = e.GetPosition(this);

                // Calcular el desplazamiento
                Left = Left + currentPoint.X - startPoint.X;
                Top = Top + currentPoint.Y - startPoint.Y;
            }
        }


        private ResizeDirection GetResizeDirection(Point mousePos)
        {
            if (mousePos.X <= RESIZE_BORDER && mousePos.Y <= RESIZE_BORDER)
                return ResizeDirection.TopLeft;
            if (mousePos.X >= Width - RESIZE_BORDER && mousePos.Y <= RESIZE_BORDER)
                return ResizeDirection.TopRight;
            if (mousePos.X <= RESIZE_BORDER && mousePos.Y >= Height - RESIZE_BORDER)
                return ResizeDirection.BottomLeft;
            if (mousePos.X >= Width - RESIZE_BORDER && mousePos.Y >= Height - RESIZE_BORDER)
                return ResizeDirection.BottomRight;
            if (mousePos.X <= RESIZE_BORDER)
                return ResizeDirection.Left;
            if (mousePos.X >= Width - RESIZE_BORDER)
                return ResizeDirection.Right;
            if (mousePos.Y <= RESIZE_BORDER)
                return ResizeDirection.Top;
            if (mousePos.Y >= Height - RESIZE_BORDER)
                return ResizeDirection.Bottom;
            return ResizeDirection.None;
        }

        private Cursor GetResizeCursor(ResizeDirection direction)
        {
            switch (direction)
            {
                case ResizeDirection.Left:
                case ResizeDirection.Right:
                    return Cursors.SizeWE;
                case ResizeDirection.Top:
                case ResizeDirection.Bottom:
                    return Cursors.SizeNS;
                case ResizeDirection.TopLeft:
                case ResizeDirection.BottomRight:
                    return Cursors.SizeNWSE;
                case ResizeDirection.TopRight:
                case ResizeDirection.BottomLeft:
                    return Cursors.SizeNESW;
                default:
                    return Cursors.Arrow;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
            var captura = CaptureScreen();
            var cap = GetCapturedImageBytes(captura);
            string textoImagen = AnalyzeImage(cap);
            string response = TranslatorAzure(textoImagen).Result;

            //string responseContent = "You are a translation assistant. Please only provide literal translations from English to Spanish.";
            //string responseUser = $"Translate this literally to Spanish: {textoImagen}";
            //string response = ChatGPTRequest(responseContent, responseUser).Result;

            //string descriptionContent = "You are a translation assistant. Please provide translations from English to Spanish.";
            //string descriptionUser = $"Translate this to Spanish and give an example: {textoImagen}";
            //string description = ChatGPTRequest(descriptionContent, descriptionUser).Result;

            TraduccionWindow tw = new TraduccionWindow(response);
            tw.Show();
            this.Close();
        }
        public byte[] GetCapturedImageBytes(Bitmap captura)
        {
            if (captura == null)
                return null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    captura.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.ToString()); 
            }
            return null;
        }
        private Bitmap CaptureScreen()
        {
            try
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;

                // Obtener el rectángulo de la ventana incluyendo sombras y bordes
                RECT rect = new RECT();
                int DWMWA_EXTENDED_FRAME_BOUNDS = 9;
                DwmGetWindowAttribute(handle, DWMWA_EXTENDED_FRAME_BOUNDS, out rect, Marshal.SizeOf(typeof(RECT)));

                // Calcular las dimensiones reales
                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;

                // Logging para diagnóstico
                Debug.WriteLine($"Dimensiones de captura: Width = {width}, Height = {height}");
                Debug.WriteLine($"Coordenadas: Left = {rect.Left}, Top = {rect.Top}, Right = {rect.Right}, Bottom = {rect.Bottom}");

                // Crear el bitmap para la captura
                using (Bitmap capturedImage = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(capturedImage))
                    {
                        // Capturar el área de la pantalla usando las coordenadas correctas
                        g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);
                    }

                    return new Bitmap(capturedImage);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en CaptureScreen: {ex}");
                return null;
            }
        }
        public static string AnalyzeImage(byte[] image)
        {
            if (image == null) return null;
            try
            {
                //NuGet\Install-Package Azure.AI.Vision.ImageAnalysis -Version 1.0.0-beta.2
                StringBuilder texto = new StringBuilder();

                ImageAnalysisClient client = new ImageAnalysisClient(
                    new Uri(ClavesSingleton.Instancia.VisionEndpoint),
                    new AzureKeyCredential(ClavesSingleton.Instancia.VisionKey));

                //byte[] imageData = File.ReadAllBytes(pathImage);
                BinaryData imageBinaryData = new BinaryData(image);
                ImageAnalysisResult result = client.Analyze(imageBinaryData,
                    VisualFeatures.Caption | VisualFeatures.Read,
                    new ImageAnalysisOptions { GenderNeutralCaption = true });

                foreach (DetectedTextBlock block in result.Read.Blocks)
                    foreach (DetectedTextLine line in block.Lines)
                    {
                        texto.AppendLine(line.Text);
                    }
                return texto.ToString();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
            return null;
        }
        public static async Task<string> TranslatorAzure(string text)
        {
            string route = "/translate?api-version=3.0&from=en&to=es";
            object[] body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(ClavesSingleton.Instancia.TranslateEndpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", ClavesSingleton.Instancia.TranslateKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", ClavesSingleton.Instancia.Region);

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                string responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonDocument.Parse(responseBody);
                var translatedText = jsonResponse.RootElement[0].GetProperty("translations")[0].GetProperty("text").GetString();
                return translatedText;
            }
        }
        public static async Task<string> ChatGPTRequest(string conten, string user)
        {
            var httpClient = new HttpClient();
            try
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ClavesSingleton.Instancia.ChatGPTApiKey}");
                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = conten },
                        new { role = "user", content = user }
                    },
                    max_tokens = 100,
                    temperature = 0.0
                };
                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(ClavesSingleton.Instancia.ChatGPTApiURL, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                return jsonResponse.choices[0].message.content.Trim();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
        }
    }
}
