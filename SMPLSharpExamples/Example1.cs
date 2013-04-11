using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMPLSharp;
using SMPLSharp.Objects;
using SMPLSharp.Utils;

namespace KM1
{
    class Program
    {
        static void Main(string[] args)
        {
            const int modelingtime = 3000;//время моделирования
            const int zm = 0, newzad = 1, proc1craked = 2, proc1repared = 3, zo1 = 4, zo2 = 5, proc2on = 6;
            SmplModel model = new SmplModel();
            model.CreateDevice("proc1");//создание proc1
            model.CreateDevice("proc2");//создание proc2
            model.CreateQueue("line");//создание line
            model.Schedule(zm, modelingtime);
            model.Schedule(newzad, model.IRandom(5, 15));
            model.Schedule(proc1craked, model.IRandom(130, 170));
            bool p1cracked = false, proc2ison = false, proc2needreserv = false;
            int reshzad = 0, otkaz = 0, prerv = 0;
            float proc2koef = 0;
            SmplEvent e;
            
            
            while (true)
            {
                model.Cause(out e);
                           
                switch (e.EventID)
                {
                    case zm://завершение моделирования
                        Console.WriteLine("Число решенных задач: {0}\nЧисло отказов процессора: {1}\nЧисло прерванных задач: {2}\nМаксимальная длина очереди: {3}\nКоэффициент загрузки резервного процессора: {4}\n", reshzad, otkaz, prerv, model.Queues["line"].MaxLength, proc2koef / (float)reshzad);
                        Console.WriteLine("\n" + model.Report());
                        Console.ReadKey();
                        Environment.Exit(0);
                        break;
                    case newzad://поступление новой задачи
                        string dev = "";
                        int zo = 0;
                        if (p1cracked)
                        {
                            dev = "proc2";
                            zo = zo2;
                        }
                        else
                        {
                            dev = "proc1";
                            zo = zo1;
                        }

                        if ((!model.Devices[dev].IsBusy)&&!(dev == "proc2" && !proc2ison/*proc1 уже сломан, а proc2 еще не включен*/))
                        {
                            //dev не занят
                            model.Devices[dev].Reserve();
                            model.Schedule(zo, model.IRandom(3, 7));
                            
                        }
                        else
                        {
                            //dev занят
                            model.Queues["line"].Enqueue();
                        }
                        model.Schedule(newzad, model.IRandom(5, 15));
                        break;
                    case zo1://завершение облуживания на proc1
                        model.Devices["proc1"].Release();
                        reshzad++;
                        if (!model.Queues["line"].IsEmpty)
                        {
                            //очередь не пуста
                            model.Queues["line"].Head();
                            model.Schedule(zo1, model.IRandom(3, 7));
                            model.Devices["proc1"].Reserve();
                           
                        }
                        
                        break;
                    case proc1craked://proc1 сломался
                        otkaz++;
                        p1cracked = true;
                        model.Schedule(proc2on, 2);
                        if (model.Devices["proc1"].IsBusy)
                        {
                            //proc1 занят
                            model.Devices["proc1"].Release();
                            int zotime = model.Cancel(zo1);
                            //на proc1 была задача
                            model.Schedule(zo2, zotime + 2);
                            proc2needreserv = true;
                            
                        }
                        model.Schedule(proc1repared, model.IRandom(10, 30));
                        break;
                    case zo2://завершения обслуживаняи на proc2
                        model.Devices["proc2"].Release();
                        reshzad++;
                        if (!model.Queues["line"].IsEmpty)
                        {
                            //очередь не пуста
                            model.Queues["line"].Head();
                            model.Schedule(zo2, model.IRandom(3, 7));
                            model.Devices["proc2"].Reserve();
                        }
                        
                        proc2koef++;
                        break;
                    case proc1repared://proc1 восстановился
                        
                        if (model.Devices["proc2"].IsBusy)
                        {
                            //proc2 занят
                            model.Devices["proc2"].Release();
                            model.Cancel(zo2);
                            proc2ison = false;
                            prerv++;
                        }
                        if (!model.Queues["line"].IsEmpty)
                        {
                            //очередь не пуста
                            model.Queues["line"].Head();
                            model.Devices["proc1"].Reserve();
                            model.Schedule(zo1, model.IRandom(3, 7));
                        }
                        model.Schedule(proc1craked, model.IRandom(130, 170));
                        p1cracked = false;
                        break;
                    case proc2on://proc2 включился
                        if (proc2needreserv)
                        {
                            //нужно зарезервировать proc2, т.к. на него поступила задача с proc1
                            model.Devices["proc2"].Reserve();
                            proc2needreserv = false;
                        }
                        else
                        {
                            //нужно взять новую задачу
                            if (!model.Queues["line"].IsEmpty)
                            {
                                model.Queues["line"].Head();
                                model.Schedule(zo2, model.IRandom(3, 7));
                                model.Devices["proc2"].Reserve();
                            }
                        }
                        proc2ison = true;
                        break;
                }
                
            }

            
        }
    }
}
