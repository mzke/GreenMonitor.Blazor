using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;

namespace GreenMonitor.Blazor.Data
{
    public class MosquittoService
    {
        public event EventHandler MensagemRecebida;
        public double Usolar {get;set;}
        public double Isolar {get;set;}
        public double Psolar {get;set;}
        public double Ubateria { get; set; }
        public double CargaBateria { get; set; }
        public double Ibateria { get; set; }
        public double Uinversor {get;set;}
        public double Iinversor {get;set;}
        public double Pinversor {get;set;}
        public int Status { get; set; }
        private NumberFormatInfo NFI { get; set; }

        public MosquittoService()
        {
            MosquittoServiceAsync().Wait();
        }

        private async Task MosquittoServiceAsync()
        {
            NFI = new NumberFormatInfo();
            NFI.NumberDecimalSeparator = ".";
            var options = new ManagedMqttClientOptionsBuilder()
               .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
               .WithClientOptions(new MqttClientOptionsBuilder()
                   .WithClientId(Guid.NewGuid().ToString())
                   .WithCredentials("usuario", "senha")
                   .WithTcpServer("0.0.0.0").Build()
                   )
               .Build();

            var mqttClient = new MqttFactory().CreateManagedMqttClient();
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("solar/tensao").Build());
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("solar/corrente").Build());
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("bateria/tensao").Build());
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("bateria/carga").Build());
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("bateria/status").Build());
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("bateria/corrente").Build());
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("inversor/tensao").Build());
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("inversor/corrente").Build());
            
            mqttClient.UseApplicationMessageReceivedHandler(e => {
                string p;
                switch (e.ApplicationMessage.Topic.ToLower())
                {
                    case "solar/tensao":
                        p = Encoding.Default.GetString(e.ApplicationMessage.Payload);
                        Usolar = Math.Round(double.Parse(p, NFI));
                        SetPotenciaSolar();
                        break;
                    case "solar/corrente":
                        p = Encoding.Default.GetString(e.ApplicationMessage.Payload);
                        Isolar = double.Parse(p, NFI);
                        SetPotenciaSolar();
                        break;
                    case "bateria/carga":
                        p = Encoding.Default.GetString(e.ApplicationMessage.Payload);
                        CargaBateria = double.Parse(p, NFI);
                        break;
                    case "bateria/status":
                        p = Encoding.Default.GetString(e.ApplicationMessage.Payload);
                        Status = Convert.ToInt32(p);
                        break;
                    case "bateria/tensao":
                        p = Encoding.Default.GetString(e.ApplicationMessage.Payload);
                        Ubateria = Math.Round(double.Parse(p, NFI), 1);
                        break;
                    case "bateria/corrente":
                        p = Encoding.Default.GetString(e.ApplicationMessage.Payload);
                        Ibateria = double.Parse(p, NFI);
                        break;
                    case "inversor/tensao":
                        p = Encoding.Default.GetString(e.ApplicationMessage.Payload);
                        Uinversor = double.Parse(p, NFI);
                        SetPotenciaInversor();
                        break;
                    case "inversor/corrente":
                        p = Encoding.Default.GetString(e.ApplicationMessage.Payload);
                        Iinversor = double.Parse(p, NFI);
                        SetPotenciaInversor();
                        break;
                }
                var handler = MensagemRecebida;
                handler?.Invoke(this, e);

            });
            await mqttClient.StartAsync(options);
        }
       
        private void SetPotenciaSolar()
        {
            Psolar = Math.Round(Usolar * Isolar);
        }

        private void SetPotenciaInversor()
        {
            Pinversor = Math.Round( Uinversor * Iinversor);
        }

    }
        
}