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
    //
    public class SmplModel
    {

        #region Public Properties

            // Генератор случайных чисел
            virtual public SmplRandomGenerator RandGen
            {
                get;
                protected set;
            }

            // Модельное время
            virtual public int Time
            {
                get;
                protected set;
            }

            // Время следующего события
            virtual public int NextEventTime
            {
                get
                {
                    if (futureEvents.Count > 0) return futureEvents[0].TimeCaused;
                    else return Time;
                }
            }

            // Модель была остановлена
            virtual public bool IsStopped
            {
                get;
                protected set;
            }
            
            // Очереди
            virtual public Dictionary<string, SmplQueue> Queues
            {
                get { return queues; }
            }

            // Приборы
            virtual public Dictionary<string, SmplDevice> Devices
            {
                get { return devices; }
            }

            // Многоканальные приборы
            virtual public Dictionary<string, SmplMultiDevice> MultiDevices
            {
                get { return multiDevices; }
            }


        #endregion

        #region Constructors

            // Конструктор
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

            // Очереди модели
            protected Dictionary<string, SmplQueue> queues;

            // Приборы модели
            protected Dictionary<string, SmplDevice> devices;

            // Журнал будущих событий модели
            protected List<SmplEvent> futureEvents;

            // Многоканальные приборы модели
            protected Dictionary<string, SmplMultiDevice> multiDevices;

        #endregion

        #region Events

            // Событие возникновения события модели
            // Возникает при вызове Cause
            public event EventCausedHandler EventCaused;
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

            // Добавить очередь в модель
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

            // Возвратить существующую очередь
            virtual public SmplQueue Queue(string name)
            {
                if (queues.ContainsKey(name)) return queues[name];
                else throw new Exception("Queue '" + name + "' is not exist");
            }

            // Добавить прибор в модель
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

            // Возвратить существующий прибор
            virtual public SmplDevice Device(string name)
            {
                if (devices.ContainsKey(name)) return devices[name];
                else throw new Exception("Device '" + name + "' is not exist");
            }


            // Добавить многоканальный прибор в модель
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


            // Возвратить существующий многоканальный прибор
            virtual public SmplMultiDevice MultiDevice(string name)
            {
                if (multiDevices.ContainsKey(name)) return multiDevices[name];
                else throw new Exception("MultiDevice '" + name + "' is not exist");
            }

            // Запланировать событие:
            //  event_id  - идентификатор типа события
            //  wait_time - время, через которе событие вызовется
            //  param     - параметр, передаваемый событию
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
        
            // Вызвать ближайшее событие модели
            // Через параметры возвращается:
            //  model_event - вызванное событие модели
            // 
            // Вызывает событие EventCaused
            // Если обработчик EventCaused задаст в e.StopModel значение false, функция возвратит false
            //
            // В случае, если ни одно событие не запланировано, кидается исключение
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


            // Вызвать ближайшее событие модели
            //
            // Вызывает событие EventCaused
            // Если обработчик EventCaused задаст в e.StopModel значение false, функция возвратит false
            //
            // В случае, если ни одно событие не запланировано, кидается исключение
            public bool Cause()
            {
                SmplEvent e;
                return Cause(out e);
            }

            // Вызвать ближайшее событие модели
            // Через параметры возвращается:
            //  event_id - идентификатор вызванного события
            //  param    - параметр вызванного события
            // 
            // Вызывает событие EventCaused
            // Если обработчик EventCaused задаст в e.StopModel значение false, функция возвратит false
            //
            // В случае, если ни одно событие не запланировано, кидается исключение
            public bool Cause(out int event_id, out object param)
            {
                SmplEvent e;
                var res = Cause(out e);
                event_id = e.EventID;
                param = e.Param;
                return res;
            }

            // Отменить ближайшее запланированное событие с указанными параметрами
            public void Cancel(int event_id, object param = null)
            {
                //foreach (SmplEvent e in futureEvents) 
                //{
                //    if ((e.EventID == event_id) && (e.Param == param))
                //    {
                //        int ind = futureEvents.IndexOf(e);
                //        futureEvents.RemoveAt(ind);
                //    }
                //}
                int i=0;
                while (i < futureEvents.Count())
                {
                    if ((futureEvents[i].EventID == event_id) && (futureEvents[i].Param == param))
                        break;
                    i++;
                }
                if (i < futureEvents.Count())
                {
                    futureEvents.RemoveAt(i);
                }
            }

            // Генерирует число по равномерному распределению 
            // в диапозоне [a, b] включительно
            public int IRandom(int a, int b)
            {
                return RandGen.IRandom(a, b);
            }

            // Генерирует число по равномерному распределению 
            // в диапозоне [0, a] включительно
            public int IRandom(int a)
            {
                return RandGen.IRandom(a);
            }

            // Генерирует число по отрицательному экспоненциальному распределению
            // со средней точкой m
            public int NexExp(int m)
            {
                return RandGen.NegExp(m);
            }

            // Генерирует стандартный отчет о модели
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

    // Delegate for EventCaused
    public delegate void EventCausedHandler(object o, EventCausedEventArgs e);

    // Event when model's event is caused
    public class EventCausedEventArgs : EventArgs
    {

        #region Public Properties

            // Событие модели
            public SmplEvent Event
            {
                get;
                private set;
            }

            // Если StopModel == false, остановить выполнение модели
            // (SmplModel.Cause вернет false)
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
            public EventCausedEventArgs(SmplEvent model_event)
            {
                Event = model_event;
                StopModel = false;
            }

        #endregion

    }
}
