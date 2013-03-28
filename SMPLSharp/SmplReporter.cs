// Содержание:
// - class SmplReporter
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMPLSharp.Objects;

namespace SMPLSharp.Utils
{
    
    // Статистика по прибору
    public class SmplRDeviceStatisic
    {
        #region Public Properties

            // Имя
            public string Name
            {
                get;
                set;
            }

            // Среднее время обслуживания заявки
            public double AverageTimeReserved
            {
                get;
                set;
            }

            // Занятость прибора (0..1)
            public double BusyIndex
            {
                get;
                set;
            }

            // Количество обслужанных заявок
            public int QueryCount
            {
                get;
                set;
            }

        #endregion
    }

    // Статистика по многоканальному прибору
    public class SmplRMultiDeviceStatisic : SmplRDeviceStatisic
    {
        #region Public Properties

        // Количество каналов
        public int ChannelCount
        {
            get;
            set;
        }

        #endregion
    }


    // Статистика по очереди
    public class SmplRQueueStatisic
    {
        #region Public Properties

            // Имя
            public string Name
            {
                get;
                set;
            }

            // Среднее время ожидания
            public double AverageTimeWaiting
            {
                get;
                set;
            }
        
            // Разброс времени ожидания
            public double DispersalTimeWaiting
            {
                get;
                set;
            }

            // Средняя длина очереди
            public double AverageLength
            {
                get;
                set;
            }

            // Максимальная длина очереди
            public int MaxLength
            {
                get;
                set;
            }


            // Число заявок, попавших в очередь
            public int TotalCount
            {
                get;
                set;
            }


        #endregion
    }

    // Give model statistic
    public class SmplReporter
    {
        #region Public Properties

            // Время моделирования
            public int ModelTime
            {
                get { return Model.Time; }
            }

            // Информация о приборах
            public List<SmplRDeviceStatisic> DeviceStatistic
            {
                get
                {
                    var ret = new List<SmplRDeviceStatisic>();
                    foreach (var ekv in Model.Devices)
                    {
                        var e = ekv.Value;
                        var stat = new SmplRDeviceStatisic();
                        stat.Name = e.Name;
                        stat.QueryCount = e.QueryCounter;
                        stat.AverageTimeReserved = ((double)e.TimeTotalReserved) / e.QueryCounter;
                        stat.BusyIndex = ((double)e.TimeTotalReserved) / ModelTime;
                        ret.Add(stat);
                    }
                    return ret;
                }
            }

            // Информация о многоканальных приборах
            public List<SmplRMultiDeviceStatisic> MultiDeviceStatistic
            {
                get
                {
                    var ret = new List<SmplRMultiDeviceStatisic>();
                    foreach (var ekv in Model.MultiDevices)
                    {
                        var e = ekv.Value;
                        var stat = new SmplRMultiDeviceStatisic();
                        stat.Name = e.Name;
                        stat.QueryCount = e.QueryCounter;
                        stat.AverageTimeReserved = ((double)e.TimeTotalReserved) / e.QueryCounter * e.CountAmbary;
                        stat.BusyIndex = ((double)e.TimeTotalReserved) / ModelTime;
                        stat.ChannelCount = e.CountAmbary;
                        ret.Add(stat);
                    }
                    return ret;
                }
            }

            // Информация о очередях
            public List<SmplRQueueStatisic> QueueStatistic
            {
                get
                {
                    var ret = new List<SmplRQueueStatisic>();
                    foreach (var qkv in Model.Queues)
                    {
                        var q = qkv.Value;
                        var stat = new SmplRQueueStatisic();
                        stat.Name = q.Name;
                        stat.AverageLength = ((double)q.LengthOfTimeIntegral) / ModelTime;
                        stat.MaxLength = q.MaxLength;
                        stat.AverageTimeWaiting = ((double)q.WaitingPeriodSum) / q.CountPassed;
                        stat.DispersalTimeWaiting = Math.Sqrt(q.WaitingPeriodSq2Sum) / q.CountPassed;
                        stat.TotalCount = q.CountPassed + q.Length;
                        ret.Add(stat);
                    }
                    return ret;
                }
            }


            // Модель, с которой связан прибор
            public SmplModel Model
            {
                get;
                protected set;
            }


        #endregion


        #region Constructors

            public SmplReporter(SmplModel model)
            {
                Model = model;
            }

        #endregion



    }

}
