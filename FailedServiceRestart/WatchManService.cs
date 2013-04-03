using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace FailedServiceRestart
{
    public partial class WatchManService : ServiceBase
    {
        public WatchManService()
        {
            InitializeComponent();
        }

        private Watcher _manager;
        protected override void OnStart(string[] args)
        {
            _manager= new Watcher();
            _manager.StartWatcher();
        }

        protected override void OnStop()
        {
            _manager.StopWatcher();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
            _manager.StartWatcher();
        }

        protected override void OnPause()
        {
            base.OnPause();
            _manager.StopWatcher();
        }
    }
}
