using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SMPLSharp;
using System.ComponentModel;

namespace SMPLSharpWPFTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window ,INotifyPropertyChanged
    {
        public enum ModelEvents { PZ, ZO, ZM }

        public SmplModel Model { get; set; }
        public SmplModelViewer Viewer { get; set; }
        public int MaxTime { get { return 1000; } }
        private string output;
        public string Output
        {
            get { return output; }
            set
            {
                output = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Output"));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null) PropertyChanged(this, e);
        }

        public MainWindow()
        {
            InitializeComponent();


            var smpl = new SmplModel();

            var MuDev = smpl.CreateMultiDevice("eq", 4);
            var queue = smpl.CreateQueue("qu");

            smpl.EventCaused += smpl_EventCaused;

            smpl.Schedule((int)ModelEvents.PZ, smpl.IRandom(5, 10));
            smpl.Schedule((int)ModelEvents.ZM, MaxTime); //10000);

            //while (smpl.Cause()) ;
            Model = smpl;
            Viewer = new SmplModelViewer(smpl);

            DataContext = this;

            /*// Initialize model
            var smpl = new SmplModel();
            
            var vs = smpl.CreateDevice("re");
            var qu = smpl.CreateQueue("er");

            smpl.Schedule((int)ModelEvents.PZ, smpl.IRandom(1, 2));
            smpl.Schedule((int)ModelEvents.ZM, 10000);

            smpl.EventCaused += smpl_EventCaused;

            // Launch model
            while (smpl.Cause()) ;

            // Report
            smpl.Report();*/
        }

        public void smpl_EventCaused(object sender, EventCausedEventArgs e)
        {
            var smpl = (SmplModel)sender;
            var e_id = (ModelEvents)e.Event.EventID;
            var MuDev = smpl.MultiDevice("eq");
            var queue = smpl.Queue("qu");
            Output += "Event: " + e_id + " - Time: " + smpl.Time + "\n";
            switch (e_id)
            {
                case ModelEvents.PZ:
                    smpl.Schedule((int)ModelEvents.PZ, smpl.IRandom(2, 8));
                    if (!MuDev.IsBusy)
                    {
                        int id = MuDev.Reserve();
                        smpl.Schedule((int)ModelEvents.ZO, smpl.IRandom(12, 30), id);
                    }
                    else queue.Enqueue();
                    break;

                case ModelEvents.ZO:
                    MuDev.Release((int)e.Event.Param);
                    if (!queue.IsEmpty)
                    {
                        int id = MuDev.Reserve();
                        queue.Head();
                        smpl.Schedule((int)ModelEvents.ZO, smpl.IRandom(12, 30), id);
                    }
                    break;

                case ModelEvents.ZM:
                    Output += smpl.Report();
                    e.StopModel = true;
                    break;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           Viewer.Play();
        }

        private void TT_TextChanged(object sender, TextChangedEventArgs e)
        {
            TT.ScrollToEnd();
        }
    }
}
