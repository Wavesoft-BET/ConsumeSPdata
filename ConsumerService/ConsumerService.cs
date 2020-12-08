using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using BLLFeedUpdates;

namespace ConsumerService
{
    public partial class ConsumerService : ServiceBase
    {
        private string _hostName;
        private int _port;
        private string _username;
        private string _password;
        private List<string> _queues;
        private string _exchangeName;
        private int _threads;
        private List<Consumer> _consumers;

        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ConsumerService()
        {
            InitializeComponent();

            log4net.Config.XmlConfigurator.Configure();

            _hostName = ConfigurationManager.AppSettings["HostName"];
            _port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            _username = ConfigurationManager.AppSettings["Username"];
            _password = ConfigurationManager.AppSettings["Password"];
            _exchangeName = ConfigurationManager.AppSettings["ExchangeName"];
            _consumers = new List<Consumer>();
            _queues = new List<string>();

            try
            {
                foreach (var key in ConfigurationManager.AppSettings.AllKeys)
                {
                    if (key.EndsWith("Queue"))
                    {
                        _queues.Add(ConfigurationManager.AppSettings[key]);
                    }
                }
                _threads = _queues.Count;

                for (var i = 0; i < _threads; i++)
                {
                    var consumer = new Consumer();
                    consumer.CreateConnection(_hostName, _username, _password, _port);
                    _consumers.Add(consumer);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
#if DEBUG
            OnStart(null);
#endif
        }

        protected override void OnStart(string[] args)
        {
            Parallel.For(0, _threads, i =>
            {
                try
                {
                    _consumers[i].ConsumeData(_queues[i], _exchangeName);
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString());
                }
            });
        }

        protected override void OnStop()
        {
            try
            {
                for (var i = 0; i < _threads; i++)
                {
                    _consumers[i].CloseConnection();
                }
                _consumers.Clear();
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }
    }
}
