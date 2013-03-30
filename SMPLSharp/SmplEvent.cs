using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMPLSharp.Objects
{
    // Model's event
    // A change of state of any system entity, active or passive
    // Can be, for example, Arrival of task or Computing completion
    //
    public class SmplEvent
    {
        
        #region Public Properties

            /// <summary>
            /// Идентификатор типа события
            /// </summary>
 
            virtual public int EventID { get; set; }

            /// <summary>
            /// Время регистрации в модели
            /// </summary>
 
            virtual public int TimeRegistred { get; set; }

            /// <summary>
            /// Время возникновения в модели (TimeRegistred + время ожидания)
            /// </summary>
 
            virtual public int TimeCaused { get; set; }

            /// <summary>
            /// Доп. параметр события 
            /// </summary>
 
            virtual public object Param { get; set; }

        #endregion

        #region Constructors

             
            /// <summary>
            /// Конструктор:
            /// </summary>
            /// <param name="event_id">идентификатор события</param>
            /// <param name="current_time">текущее модельное время</param>
            /// <param name="waiting_time">время ожидания возникновения</param>
            /// <param name="param">доп. параметр события</param>
 
            internal SmplEvent(int event_id, int current_time, int waiting_time, object param = null)
            {
                EventID = event_id;
                TimeRegistred = current_time;
                TimeCaused = current_time + waiting_time;
                Param = param;
            }


        #endregion

    }
}
