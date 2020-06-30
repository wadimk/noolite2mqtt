using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using Noolite2Mqtt.Core.Plugins;
using Noolite2Mqtt.Core.Plugins.Utils;
using Noolite2Mqtt.Plugins.Timer;

namespace Noolite2Mqtt.Plugins.Mqtt
{
    public class MqttPlugin : PluginBase
    {
        #region settings

        private const string DEFAULT_HOST = "localhost";
        private const int DEFAULT_PORT = 1883;

        private bool reconnectEnabled;
        
        private IMqttClientOptions options;

        private List<MqttMessageHandlerDelegate> handlers;

        public string Host => Configuration.GetValue("host", DEFAULT_HOST);
        public int Port => Configuration.GetValue("port", DEFAULT_PORT);
        public string Login => Configuration["login"];
        public string Password => Configuration["password"];
        public string[] Topics => Configuration.GetSection("topics").Get<string[]>() ?? new[] { "#" };


        #endregion

        private IMqttClient client;

        public override async Task InitPlugin()
        {
            var clientId = Guid.NewGuid().ToString();

            Logger.LogInformation($"init MQTT client: {Host}:{Port} (ID: {{{clientId}}})");

            var Factory = new MqttFactory();

            options = new MqttClientOptionsBuilder()
                .WithTcpServer(Host, Port)
                .WithClientId(clientId)
                .WithCredentials(Login, Password)
                .Build();


            client = Factory.CreateMqttClient();
            client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(OnAppMessage);
            client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);

            handlers = RegisterHandlers();
        }

        public override async Task StartPlugin()
        {
            reconnectEnabled = true;
            ReConnect();
        }

        public override async Task StopPlugin()
        {
            reconnectEnabled = false;

            if (client.IsConnected)
            {
                lock (client)
                {
                    if (client.IsConnected)
                    {
                        client.DisconnectAsync().Wait();
                    }
                }
            }
        }

        [TimerCallback(60000)]
        public void ConnectionChecking(DateTime now)
        {
            ReConnect();
        }

        public void TryPublish(string topic, string payload, bool retain = false)
        {
            TryPublish(topic, Encoding.UTF8.GetBytes(payload), retain);
        }

        public void TryPublish(string topic, byte[] payload, bool retain = false)
        {
            var msg = new MqttApplicationMessage() { Payload = payload, Topic = topic, QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce, Retain = retain };

            var ex = client.PublishAsync(msg).Exception;

            if (ex != null)
            {
                throw ex;
            }
        }

        #region private

        private List<MqttMessageHandlerDelegate> RegisterHandlers()
        {
            var list = new List<MqttMessageHandlerDelegate>();

            foreach (var plugin in Context.GetAllPlugins())
            {
                var pluginType = plugin.GetType();

                foreach (var mi in plugin.FindMethods<MqttMessageHandlerAttribute, MqttMessageHandlerDelegate>())
                {
                    Logger.LogInformation($"register mqtt message handler: \"{mi.Method.Method.Name}\" ({pluginType.FullName})");
                    list.Add(mi.Method);
                }
            }

            return list;
        }

        private void ReConnect()
        {
            if (client != null && !client.IsConnected && reconnectEnabled)
            {
                lock (client)
                {
                    if (client != null && !client.IsConnected && reconnectEnabled)
                    {
                        try
                        {
                            Logger.LogInformation("connect to MQTT broker");

                            var task = client.ConnectAsync(options);
                            task.Wait();

                            if (task.Exception != null) throw task.Exception;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning(ex.Message);
                        }
                    }
                }
            }
        }

        private async void OnConnected(MqttClientConnectedEventArgs e)
        {
            Logger.LogInformation("MQTT client is connected");

            Logger.LogInformation($"Subscribe: {string.Join(", ", Topics)}");

            var filters = Topics
                .Select(topic => new MqttTopicFilter() { Topic = topic, QualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce })
                .ToArray();

            await client.SubscribeAsync(filters);

            Logger.LogInformation("MQTT client is subscribed");
        }

        private void OnDisconnected(MqttClientDisconnectedEventArgs e)
        {
            Logger.LogInformation("MQTT connection closed");
        }

        private void OnAppMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            var msg = e.ApplicationMessage;
            var payload = Encoding.UTF8.GetString(msg.Payload);

            Logger.LogDebug($"topic: {msg.Topic}, payload: {payload}, qos: {msg.QualityOfServiceLevel}, retain: {msg.Retain}");

            // events
            SafeInvoke(handlers, h => h(msg.Topic, msg.Payload), true);
        }

        #endregion
    }
}
