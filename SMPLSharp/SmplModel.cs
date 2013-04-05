// Содержание:
// - class SmplModel
// - delegate EventCausedHandler
// - class EventCausedEventArgs
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
            public int IRandom(int a, int b)
            {
                return RandGen.IRandom(a, b);
            }

            /// <summary>
            /// Генерирует число по равномерному распределению в диапозоне [0, a] включительно
            /// </summary>
            /// <param name="a">Правая граница диапазона</param>
            /// <returns></returns>
            public int IRandom(int a)
            {
                return RandGen.IRandom(a);
            }

            /// <summary>
            /// Генерирует число по отрицательному экспоненциальному распределению со средней точкой m
            /// </summary>
            /// <param name="m">Средняя точка</param>
            /// <returns></returns>
 
            public int NexExp(int m)
            {
                return RandGen.NegExp(m);
            }

            /// <summary>
            /// Генерирует стандартный отчет о модели
            /// </summary>
            /// <returns>Строка, содержащая весь отчет</returns>
 
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
            public EventCausedEventArgs(SmplEvent model_event)
            {
                Event = model_event;
                StopModel = false;
            }

        #endregion

    }
}
