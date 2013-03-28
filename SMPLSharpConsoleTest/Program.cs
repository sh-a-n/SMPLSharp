using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMPLSharp;
using SMPLSharp.Objects;


namespace SMPLSharpConsoleTest
{
    class Program
    {
        public enum ModelEvents { PZ = 1, ZO, ZM }
        
        static void Main(string[] args)
        {
            
            /*var gen = new SMPLSharp.Utils.SmplRandomGenerator();
            for (var i = 0; i < 50; i++)
            {
                Console.WriteLine(gen.IRandom(12, 24));
                Console.WriteLine(gen.IRandom(12, 20));
            }
            Console.Read();*/
            
            
            // Initialize model
            var smpl = new SmplModel();
            
            var MuDev = smpl.CreateMultiDevice("eq", 4);
            var queue = smpl.CreateQueue("qu");

            smpl.EventCaused += smpl_EventCaused;

            smpl.Schedule((int)ModelEvents.PZ, smpl.IRandom(5, 10));
            smpl.Schedule((int)ModelEvents.ZM, 100); //10000);

            //while (smpl.Cause()) ;
            var viewer = new SmplModelViewer(smpl);
            viewer.Play(1000, false);


            /*
            // Launch model
            int e = 0;
            object j;
            while (e != (int)ModelEvents.ZM)
            {
                smpl.Cause(out e, out j);

                // Hande model events
                switch ((ModelEvents)e)
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
                        MuDev.Release( (int)j);
                        if (!queue.IsEmpty)
                        {
                            int id = MuDev.Reserve();
                            smpl.Schedule((int)ModelEvents.ZO, smpl.IRandom(12, 30), id);
                        }
                        break;
                }
            }

             */
            // Report
            Console.Write(smpl.Report());
             
        }


        static public void smpl_EventCaused(object sender, EventCausedEventArgs e)
        {
            var smpl = (SmplModel)sender;
            var e_id = (ModelEvents)e.Event.EventID;
            var MuDev = smpl.MultiDevice("eq");
            var queue = smpl.Queue("qu");
            Console.WriteLine("Event: " + e_id + " - Time: " + smpl.Time);
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
                    e.StopModel = true;
                    break;
            }

        }
    }
}
