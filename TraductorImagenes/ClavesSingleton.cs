using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace TraductorImagenes
{
    public class ClavesSingleton
    {
        public string VisionEndpoint { get; private set; }
        public string VisionKey { get; private set; }
        public string TranslateKey { get; private set; }
        public string TranslateEndpoint { get; private set; }
        public string SpeechKey { get; private set; }
        public string Region { get; private set; }
        public string ChatGPTApiKey { get; private set; }
        public string ChatGPTApiURL { get; private set; }

        private static readonly Lazy<ClavesSingleton> instancia = new Lazy<ClavesSingleton>(() => new ClavesSingleton());
        private string nombreFichero = "claves.json";

        private ClavesSingleton()
        {
            CargarClaves();
        }

        public static ClavesSingleton Instancia
        {
            get
            {
                return instancia.Value;
            }
        }

        private void CargarClaves()
        {
            string directorioProyecto = Directory.GetCurrentDirectory();
            string rutaFichero = Path.Combine(directorioProyecto, nombreFichero);
            try
            {
                if (!File.Exists(rutaFichero))
                {
                    Console.WriteLine("No se encontró el fichero 'claves.json'. Creando uno nuevo...");
                    var claves = new Claves
                    {
                        VisionEndpoint = "https://tuproyecto.api.cognitive.microsoft.com/vision",
                        VisionKey = "tu-vision-key",
                        SpeechKey = "tu-speech-key",
                        TranslateKey = "tu-translate-key",
                        TranslateEndpoint = "tu-translate-key",
                        Region = "tu-region",
                        ChatGPTApiKey = "tu-api_key",
                        ChatGPTApiURL = "tu-api-URL"
                    };
                    string json = JsonConvert.SerializeObject(claves, Formatting.Indented);
                    File.WriteAllText(rutaFichero, json);
                    MessageBox.Show($"Se ha creado el fichero en la ruta {rutaFichero}");
                    AsignarClaves(claves);
                }
                else
                {
                    string contenidoJson = File.ReadAllText(rutaFichero);
                    var claves = JsonConvert.DeserializeObject<Claves>(contenidoJson);
                    AsignarClaves(claves);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void AsignarClaves(Claves claves)
        {
            VisionEndpoint = claves.VisionEndpoint;
            VisionKey = claves.VisionKey;
            TranslateEndpoint = claves.TranslateEndpoint;
            TranslateKey = claves.TranslateKey;
            SpeechKey = claves.SpeechKey;
            Region = claves.Region;
            ChatGPTApiKey = claves.ChatGPTApiKey;
            ChatGPTApiURL = claves.ChatGPTApiURL;
        }

        private class Claves
        {
            public string VisionEndpoint { get; set; }
            public string VisionKey { get; set; }
            public string TranslateEndpoint { get; set; }
            public string TranslateKey { get; set; }
            public string SpeechKey { get; set; }
            public string Region { get; set; }
            public string ChatGPTApiKey { get; set; }
            public string ChatGPTApiURL { get; set; }
        }
    }
}