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
    
    /// <summary>
    /// Статистика по прибору
    /// </summary>
 
    public class SmplRDeviceStatisic
    {
        #region Public Properties

            /// <summary>
            /// Имя
            /// </summary>
 
            public string Name
            {
                get;
                set;
            }

            /// <summary>
            /// Среднее время обслуживания заявки
            /// </summary>
 
            public double AverageTimeReserved
            {
                get;
                set;
            }

            /// <summary>
            /// Занятость прибора (0..1)
            /// </summary>
 
            public double BusyIndex
            {
                get;
                set;
            }

            /// <summary>
            /// Количество обслужанных заявок
            /// </summary>
 
            public int QueryCount
            {
                get;
                set;
            }

        #endregion
    }

    /// <summary>
    /// Статистика по многоканальному прибору
    /// </summary>
 
    public class SmplRMultiDeviceStatisic : SmplRDeviceStatisic
    {
        #region Public Properties

        /// <summary>
        /// Количество каналов
        /// </summary>
 
        public int ChannelCount
        {
            get;
            set;
        }

        #endregion
    }


    /// <summary>
    /// Статистика по очереди
    /// </summary>
 
    public class SmplRQueueStatisic
    {
        #region Public Properties

            /// <summary>
            /// Имя
            /// </summary>
 
            public string Name
            {
                get;
                set;
            }

            /// <summary>
            /// Среднее время ожидания
            /// </summary>
 
            public double AverageTimeWaiting
            {
                get;
                set;
            }
        
            /// <summary>
            /// Разброс времени ожидания
            /// </summary>
 
            public double DispersalTimeWaiting
            {
                get;
                set;
            }

            /// <summary>
            /// Средняя длина очереди
            /// </summary>
 
            public double AverageLength
            {
                get;
                set;
            }

            /// <summary>
            /// Максимальная длина очереди
            /// </summary>
 
            public int MaxLength
            {
                get;
                set;
            }


            /// <summary>
            /// Число заявок, попавших в очередь
            /// </summary>
 
            public int TotalCount
            {
                get;
                set;
            }


        #endregion
    }

    /// <summary>
    /// Give model statistic
    /// </summary> 
    public class SmplReporter
    {
        #region Public Properties

            /// <summary>
            /// Время моделирования
            /// </summary> 
            public int ModelTime
            {
                get { return Model.Time; }
            }

            /// <summary>
            /// Информация о приборах
            /// </summary> 
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

            /// <summary>
            /// Информация о многоканальных приборах
            /// </summary>
 
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

            /// <summary>
            /// Информация о очередях
            /// </summary>
 
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


            /// <summary>
            /// Модель, с которой связан прибор
            /// </summary>
 
            public SmplModel Model
            {
                get;
                protected set;
            }


        #endregion


        #region Constructors
        /// <summary>
        /// Конструктор для SmplReporter
        /// </summary>
        /// <param name="model">Модель</param>
            public SmplReporter(SmplModel model)
            {
                Model = model;
            }

        #endregion



    }

}
