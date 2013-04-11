// Содержание:
// - class SmplModel
// - delegate EventCausedHandler
// - class EventCausedEventArgs
///@page smplexample Пример программы на SMPL
///@section task Задача
///Задание 12. Специализированное вычислительное устройство, работающее в режиме реального времени, имеет в своем составе два процессора, соединенные с общей оперативной памятью. В режиме нормальной эксплуатации задания выполняются на пер¬вом процессоре, а второй является резервным. Первый процессор характеризуется низкой надежностью и работает безотказно лишь в течение 150 ± 20 мин. Если отказ происходит во время решения задания, в течение 2 мин производится включение второго про¬цессора, который продолжает решение прерванного задания, а также решает и последующие задания до восстановления первого процессора. Это восстановление происходит за 20 ± 10 мин, после чего начинается решение очередного задания на первом процессо¬ре, а резервный выключается. Задания поступают на устройство каждые 10 ± 5 мин и решаются за 5 ± 2 мин. Надежность резервно¬го процессора считается идеальной.
///Смоделировать процесс работы устройства в течение 50 ч. Под¬считать число решенных заданий, число отказов процессора и число прерванных заданий. Определить максимальную длину оче¬реди заданий и коэффициент загрузки резервного процессора.
/// @section examplesolution Пример решения
/// @code
///using System;
///using System.Collections.Generic;
///using System.Linq;
///using System.Text;
///using System.Threading.Tasks;
///using SMPLSharp;
///using SMPLSharp.Objects;
///using SMPLSharp.Utils;
///namespace KM1
///{
///    class Program
///    {
///        static void Main(string[] args)
///        {
///            const int modelingtime = 3000;//время моделирования
///            const int zm = 0, newzad = 1, proc1craked = 2, proc1repared = 3, zo1 = 4, zo2 = 5, proc2on = 6;
///            SmplModel model = new SmplModel();
///            model.CreateDevice("proc1");//создание proc1
///            model.CreateDevice("proc2");//создание proc2
///            model.CreateQueue("line");//создание line
///            model.Schedule(zm, modelingtime);
///            model.Schedule(newzad, model.IRandom(5, 15));
///            model.Schedule(proc1craked, model.IRandom(130, 170));
///            bool p1cracked = false, proc2ison = false, proc2needreserv = false;
///            int reshzad = 0, otkaz = 0, prerv = 0;
///            float proc2koef = 0;
///            SmplEvent e;                   
///            while (true)
///            {
///                model.Cause(out e);                         
///                switch (e.EventID)
///                {
///                    case zm://завершение моделирования
///                        Console.WriteLine("Число решенных задач: {0}\nЧисло отказов процессора: {1}\nЧисло прерванных задач: {2}\nМаксимальная длина очереди: {3}\nКоэффициент загрузки резервного процессора: {4}\n", reshzad, otkaz, prerv, model.Queues["line"].MaxLength, proc2koef / (float)reshzad);
///                        Console.WriteLine("\n" + model.Report());
///                        Console.ReadKey();
///                        Environment.Exit(0);
///                        break;
///                    case newzad://поступление новой задачи
///                        string dev = "";
///                        int zo = 0;
///                        if (p1cracked)
///                        {
///                            dev = "proc2";
///                            zo = zo2;
///                        }
///                        else
///                        {
///                            dev = "proc1";
///                            zo = zo1;
///                        }
///                        if ((!model.Devices[dev].IsBusy)&&!(dev == "proc2" && !proc2ison/*proc1 уже сломан, а proc2 еще не включен*/))
///                        {
///                            //dev не занят
///                            model.Devices[dev].Reserve();
///                            model.Schedule(zo, model.IRandom(3, 7));                          
///                        }
///                        else
///                        {
///                            //dev занят
///                            model.Queues["line"].Enqueue();
///                        }
///                        model.Schedule(newzad, model.IRandom(5, 15));
///                        break;
///                    case zo1://завершение облуживания на proc1
///                        model.Devices["proc1"].Release();
///                        reshzad++;
///                        if (!model.Queues["line"].IsEmpty)
///                        {
///                            //очередь не пуста
///                            model.Queues["line"].Head();
///                            model.Schedule(zo1, model.IRandom(3, 7));
///                            model.Devices["proc1"].Reserve();                          
///                        }                       
///                        break;
///                    case proc1craked://proc1 сломался
///                        otkaz++;
///                        p1cracked = true;
///                        model.Schedule(proc2on, 2);
///                        if (model.Devices["proc1"].IsBusy)
///                        {
///                            //proc1 занят
///                            model.Devices["proc1"].Release();
///                            int zotime = model.Cancel(zo1);
///                            //на proc1 была задача
///                            model.Schedule(zo2, zotime + 2);
///                            proc2needreserv = true;                           
///                        }
///                        model.Schedule(proc1repared, model.IRandom(10, 30));
///                        break;
///                    case zo2://завершения обслуживаняи на proc2
///                        model.Devices["proc2"].Release();
///                        reshzad++;
///                        if (!model.Queues["line"].IsEmpty)
///                        {
///                            //очередь не пуста
///                            model.Queues["line"].Head();
///                            model.Schedule(zo2, model.IRandom(3, 7));
///                            model.Devices["proc2"].Reserve();
///                        }                        
///                        proc2koef++;
///                        break;
///                    case proc1repared://proc1 восстановился                       
///                        if (model.Devices["proc2"].IsBusy)
///                        {
///                            //proc2 занят
///                            model.Devices["proc2"].Release();
///                            model.Cancel(zo2);
///                            proc2ison = false;
///                            prerv++;
///                        }
///                        if (!model.Queues["line"].IsEmpty)
///                        {
///                            //очередь не пуста
///                            model.Queues["line"].Head();
///                            model.Devices["proc1"].Reserve();
///                            model.Schedule(zo1, model.IRandom(3, 7));
///                        }
///                        model.Schedule(proc1craked, model.IRandom(130, 170));
///                        p1cracked = false;
///                        break;
///                    case proc2on://proc2 включился
///                        if (proc2needreserv)
///                        {
///                            //нужно зарезервировать proc2, т.к. на него поступила задача с proc1
///                            model.Devices["proc2"].Reserve();
///                            proc2needreserv = false;
///                        }
///                        else
///                        {
///                            //нужно взять новую задачу
///                            if (!model.Queues["line"].IsEmpty)
///                            {
///                                model.Queues["line"].Head();
///                                model.Schedule(zo2, model.IRandom(3, 7));
///                                model.Devices["proc2"].Reserve();
///                            }
///                        }
///                        proc2ison = true;
///                        break;
///                }               
///            }            
///        }
///    }
///}
///@endcode
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMPLSharp.Objects;
using SMPLSharp.Utils;

namespace SMPLSharp
{
    // Simulation model
    // Provides set of function for building event-based, discrete-event simulation model
    /// <summary>
    /// Simulation model. Provides set of function for building event-based, discrete-event simulation model
    /// </summary>
    public class SmplModel
    {

        #region Public Properties

            /// <summary>
            /// Генератор случайных чисел
            /// </summary> 
            virtual public SmplRandomGenerator RandGen
            {
                get;
                protected set;
            }

            /// <summary>
            /// Модельное время
            /// </summary> 
            virtual public int Time
            {
                get;
                protected set;
            }

            /// <summary>
            /// Время следующего события
            /// </summary>
            virtual public int NextEventTime
            {
                get
                {
                    if (futureEvents.Count > 0) return futureEvents[0].TimeCaused;
                    else return Time;
                }
            }

            /// <summary>
            /// Модель была остановлена
            /// </summary>
            virtual public bool IsStopped
            {
                get;
                protected set;
            }
            
            /// <summary>
            /// Очереди
            /// </summary>
            virtual public Dictionary<string, SmplQueue> Queues
            {
                get { return queues; }
            }

            /// <summary>
            ///  Приборы
            /// </summary>
            virtual public Dictionary<string, SmplDevice> Devices
            {
                get { return devices; }
            }

            /// <summary>
            /// Многоканальные приборы
            /// </summary> 
            virtual public Dictionary<string, SmplMultiDevice> MultiDevices
            {
                get { return multiDevices; }
            }


        #endregion

        #region Constructors

            /// <summary>
            /// Конструктор
            /// </summary>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// @endcode
            public SmplModel()
            {
                RandGen = new SmplRandomGenerator();
                Time = 0;
                IsStopped = false;
                queues = new Dictionary<string, SmplQueue>();
                devices = new Dictionary<string, SmplDevice>();
                futureEvents = new List<SmplEvent>();
                multiDevices = new Dictionary<string, SmplMultiDevice>();
            }

        #endregion

        #region Protected/Private Data Fields

            /// <summary>
            /// Очереди модели
            /// </summary> 
            protected Dictionary<string, SmplQueue> queues;

            /// <summary>
            /// Приборы модели
            /// </summary> 
            protected Dictionary<string, SmplDevice> devices;

            /// <summary>
            /// Журнал будущих событий модели
            /// </summary>
            protected List<SmplEvent> futureEvents;

            /// <summary>
            /// Многоканальные приборы модели
            /// </summary> 
            protected Dictionary<string, SmplMultiDevice> multiDevices;

        #endregion

        #region Events

            /// <summary>
            /// Событие соершено
            /// </summary>
            public event EventCausedHandler EventCaused;
        /// <summary>
        /// Событие возникновения события модели. Возникает при вызове Cause
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
            protected virtual bool OnEventCaused(EventCausedEventArgs e)
            {
                if (EventCaused != null)
                {
                    EventCaused(this, e);
                    return e.StopModel;
                }
                else return false;
            }

        #endregion

        #region Public Methods

            /// <summary>
            /// Добавить очередь в модель
            /// </summary>
            /// <param name="name">Имя очереди</param>
            /// <returns>Новая очередь</returns>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// SmplQueue q1 = model.CreateQueue("q1");
            /// @endcode
            virtual public SmplQueue CreateQueue(string name)
            {
                if (!queues.ContainsKey(name))
                {
                    var q = new SmplQueue(this, name);
                    queues.Add(name, q);
                    return q;
                }
                else throw new Exception("Queue '" + name + "' already exist");
            }

            /// <summary>
            /// Возвратить существующую очередь
            /// </summary>
            /// <param name="name">Имя очереди</param>
            /// <returns>Новая очередь</returns>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// model.CreateQueue("q1");
            /// SmplQueue q1 = model.Queue("q1");
            /// @endcode
            virtual public SmplQueue Queue(string name)
            {
                if (queues.ContainsKey(name)) return queues[name];
                else throw new Exception("Queue '" + name + "' is not exist");
            }

            /// <summary>
            /// Добавить прибор в модель
            /// </summary>
            /// <param name="name">Имя прибора</param>
            /// <returns>Новый прибор</returns>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// SmplDevice d1 = model.CreateDevice("device1");
            /// @endcode
            virtual public SmplDevice CreateDevice(string name)
            {
                if (!devices.ContainsKey(name))
                {
                    var e = new SmplDevice(this, name);
                    devices.Add(name, e);
                    return e;
                }
                else throw new Exception("Device '" + name + "' already exist");
            }

            /// <summary>
            /// Возвратить существующий прибор
            /// </summary>
            /// <param name="name">Имя прибора</param>
            /// <returns>Прибор</returns>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// model.CreateDevice("device1");
            /// SmplDevice d1 = model.Device("device1");
            /// @endcode
            virtual public SmplDevice Device(string name)
            {
                if (devices.ContainsKey(name)) return devices[name];
                else throw new Exception("Device '" + name + "' is not exist");
            }


            /// <summary>
            /// Добавить многоканальный прибор в модель
            /// </summary>
            /// <param name="name">Имя многоканального прибора</param>
            /// <param name="countAmbary">Количество каналов</param>
            /// <returns>Новый многоканальный прибор</returns>
            /// Пример использования:
            /// @code
            /// SmplMpdel model =  new SmplModel();
            /// SmplMultyDevice multydev = model.CreateMultyDevice("dev1", 2);
            /// @endcode
            virtual public SmplMultiDevice CreateMultiDevice(string name, int countAmbary)
            {
                if (!multiDevices.ContainsKey(name))
                {
                    var e = new SmplMultiDevice(this, name, countAmbary);
                    multiDevices.Add(name, e);
                    return e;
                }
                else throw new Exception("MultiDevice '" + name + "' already exist");
            }


            /// <summary>
            /// Возвратить существующий многоканальный прибор
            /// </summary>
            /// <param name="name">имя прибора</param>
            /// <returns>Многоканаьный прибор</returns>
            /// Пример использования:
            /// @code
            /// SmplMpdel model =  new SmplModel();
            /// model.CreateMultyDevice("dev1", 2);
            /// SmplMultyDevice dev1 = model.MultyDevice("dev1");
            /// @endcode
            virtual public SmplMultiDevice MultiDevice(string name)
            {
                if (multiDevices.ContainsKey(name)) return multiDevices[name];
                else throw new Exception("MultiDevice '" + name + "' is not exist");
            }

            
        /// <summary>
        /// Запланировать событие
        /// </summary>
        /// <param name="event_id">идентификатор типа события</param>
        /// <param name="wait_time">время, через которе событие вызовется</param>
        /// <param name="param">параметр, передаваемый событию</param>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// model.CreateDevice("device1");
            /// model.Devices["device1"].Reserve();
            /// model.Schedule(0,30);
            /// @endcode
            public void Schedule(int event_id, int wait_time, object param = null)
            {
                var e = new SmplEvent(event_id, Time, wait_time, param);
                for (int i = futureEvents.Count - 1; i >= 0; i--)
                {
                    if (futureEvents[i].TimeCaused <= e.TimeCaused)
                    {
                        futureEvents.Insert(i + 1, e);
                        return;
                    }
                }
                // Else make it first
                futureEvents.Insert(0, e);
            }
        
            
        /// <summary>
        /// Вызвать ближайшее событие модели
        /// </summary>
        /// <param name="model_event">вызванное событие модели</param>
            /// <returns>Если обработчик EventCaused задаст в e.StopModel значение false, функция возвратит false. true, если событие существует. В случае, если ни одно событие не запланировано, кидается исключение</returns>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// model.CreateDevice("device1");
            /// model.Devices["device1"].Reserve();
            /// model.Schedule(0,30);
            /// SmplEvent e;
            /// bool res = model.Cause(e);
            /// @endcode
            public bool Cause(out SmplEvent model_event)
            {
                if (futureEvents.Count > 0)
                {
                    model_event = futureEvents[0];
                    futureEvents.RemoveAt(0);
                    Time = model_event.TimeCaused;
                    IsStopped = OnEventCaused(new EventCausedEventArgs(model_event));
                    return !IsStopped;
                }
                else
                    throw new Exception("Not possible events!");
            }


            
        /// <summary>
        /// Вызвать ближайшее событие модели
        /// </summary>
            /// <returns>Если обработчик EventCaused задаст в e.StopModel значение false, функция возвратит false. true, если событие существует. В случае, если ни одно событие не запланировано, кидается исключение</returns>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// model.CreateDevice("device1");
            /// model.Devices["device1"].Reserve();
            /// model.Schedule(0,30);
            /// bool res = model.Cause();
            /// @endcode
            public bool Cause()
            {
                SmplEvent e;
                return Cause(out e);
            }

            
        /// <summary>
        /// Вызвать ближайшее событие модели
        /// </summary>
        /// <param name="event_id">идентификатор вызванного события</param>
        /// <param name="param">параметр вызванного события</param>
            /// <returns>Если обработчик EventCaused задаст в e.StopModel значение false, функция возвратит false. true, если событие существует. В случае, если ни одно событие не запланировано, кидается исключение</returns>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// model.CreateDevice("device1");
            /// model.Devices["device1"].Reserve();
            /// model.Schedule(0,30,"string");
            /// SmplEvent e;
            /// string str;
            /// bool res = model.Cause(e,str);
            /// @endcode
            public bool Cause(out int event_id, out object param)
            {
                SmplEvent e;
                var res = Cause(out e);
                event_id = e.EventID;
                param = e.Param;
                return res;
            }

            /// <summary>
            /// Отменить ближайшее запланированное событие с указанными параметрами
            /// </summary>
            /// <param name="event_id">Идентификатор события</param>
            /// <param name="param">Параметры события</param>
            /// <returns>Время до окончания отменяемого события или -1 в случае, если событие не найдено</returns>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// model.CreateDevice("device1");
            /// model.Devices["device1"].Reserve();
            /// model.Schedule(0,30,"string");
            /// int time = model.Cancel(0,"string");
            /// model.Devices["device1"].Release();
            /// @endcode
            public int Cancel(int event_id, object param = null)
            {
                foreach (SmplEvent e in futureEvents)
                {
                    if ((e.EventID == event_id) && (e.Param == param))
                    {
                        int ind = futureEvents.IndexOf(e);
                        futureEvents.RemoveAt(ind);
                        return e.TimeCaused - this.Time;
                    }
                }
                return -1;
                
            }

            // 
        /// <summary>
        /// Генерирует число по равномерному распределению в диапозоне [a, b] включительно
        /// </summary>
        /// <param name="a">Левая граница диапазона</param>
        /// <param name="b">Правая граница диапазона</param>
        /// <returns></returns>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// model.CreateDevice("device1");
            /// model.Devices["device1"].Reserve();
            /// model.Schedule(0,model.IRandom(15,25));
            /// @endcode
            public int IRandom(int a, int b)
            {
                return RandGen.IRandom(a, b);
            }

            /// <summary>
            /// Генерирует число по равномерному распределению в диапозоне [0, a] включительно
            /// </summary>
            /// <param name="a">Правая граница диапазона</param>
            /// <returns></returns>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// model.CreateDevice("device1");
            /// model.Devices["device1"].Reserve();
            /// model.Schedule(0,model.IRandom(30));
            /// @endcode
            public int IRandom(int a)
            {
                return RandGen.IRandom(a);
            }

            /// <summary>
            /// Генерирует число по отрицательному экспоненциальному распределению со средней точкой m
            /// </summary>
            /// <param name="m">Средняя точка</param>
            /// <returns></returns>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// model.CreateDevice("device1");
            /// model.Devices["device1"].Reserve();
            /// model.Schedule(0,model.IRandom(5));
            /// @endcode 
            public int NexExp(int m)
            {
                return RandGen.NegExp(m);
            }

            /// <summary>
            /// Генерирует стандартный отчет о модели
            /// </summary>
            /// <returns>Строка, содержащая весь отчет</returns>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// model.CreateDevice("device1");
            /// model.Devices["device1"].Reserve();
            /// model.Schedule(0,30);
            /// SmplEvent e;
            /// model.Cause(e);
            /// model.Devices["device1"].Release();
            /// string str = model.Report();
            /// Console.WriteLine(str);
            /// @endcode
            public string Report()
            {
                var ret = "";
                var report = new SmplReporter(this);
                ret += "Время моделирования: " + report.ModelTime + "\n";
                ret += "\n";
                
                ret += "-------------------------------------------------------------------------------\n";
                ret += "                               ПРИБОРЫ                                         \n";
                ret += " n) <Имя>(<занятость>): count = <Кол-во_обслуж> , av.time = <Cр_время>         \n";
                ret += "-------------------------------------------------------------------------------\n";
                ret += "\n";
                int c = 1;
                foreach (var stat in report.DeviceStatistic)
                {
                    ret += String.Format(" {0}) {1}({2:0.00}): count = {3}, av.time = {4:0.00}\n", c, stat.Name, stat.BusyIndex, stat.QueryCount, stat.AverageTimeReserved);
                    c++;
                }
                ret += "\n";

                ret += "-------------------------------------------------------------------------------\n";
                ret += "                МНОГОКАНАЛЬНЫЕ ПРИБОРЫ                                         \n";
                ret += " n) <Имя>[число к.](<занятость>): count = <Кол-во_обслуж> , av.time = Cр_время \n";
                ret += "-------------------------------------------------------------------------------\n";
                ret += "\n";
                c = 1;
                foreach (var stat in report.MultiDeviceStatistic)
                {
                    ret += String.Format(" {0}) {1}[{5}]({2:0.00}): count = {3}, av.time = {4:0.00}\n", c, stat.Name, stat.BusyIndex, stat.QueryCount, stat.AverageTimeReserved, stat.ChannelCount);
                    c++;
                }
                ret += "\n";

                ret += "-------------------------------------------------------------------------------\n";
                ret += "                               ОЧЕРЕДИ                                         \n";
                ret += " n) <Имя>: av.len = <ср_дл>(<макс>), av.w.time = <ср_вр_ожид>(<разброс>)  \n";
                ret += "    t.cnt = <кол_во_попавщих_в_очередь>                                       \n";
                ret += "-------------------------------------------------------------------------------\n";
                ret += "\n";
                c = 1;
                foreach (var stat in report.QueueStatistic)
                {
                    ret += String.Format(" {0}) {1}: av.len = {2:0.00}({3}), av.w.time = {4:0.00}({5:0.00}) t.cnt = {6}\n", 
                                           c, stat.Name, stat.AverageLength, stat.MaxLength, stat.AverageTimeWaiting, stat.DispersalTimeWaiting, stat.TotalCount);
                    c++;
                }
                ret += "\n";
                return ret;
            }

        #endregion

    }

    /// <summary>
    /// Delegate for EventCaused
    /// </summary>
    /// <param name="o"></param>
    /// <param name="e"></param> 
    public delegate void EventCausedHandler(object o, EventCausedEventArgs e);

    /// <summary>
    /// Event when model's event is caused
    /// </summary> 
    public class EventCausedEventArgs : EventArgs
    {

        #region Public Properties

            /// <summary>
            /// Событие модели
            /// </summary>
 
            public SmplEvent Event
            {
                get;
                private set;
            }

            /// <summary>
            /// Если StopModel == false, остановить выполнение модели (SmplModel.Cause вернет false)
            /// </summary>
 
            public bool StopModel
            {
                get;
                set;
            }

        #endregion

        #region Constructors

            // Конструктор:
            //  event_id - идентификатор события модели
            //  param    - параметр события
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="model_event">Событие</param>
        /// @code
        /// 
        /// @endcode
            public EventCausedEventArgs(SmplEvent model_event)
            {
                Event = model_event;
                StopModel = false;
            }

        #endregion

    }
}
