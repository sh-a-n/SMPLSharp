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

            // Идентификатор типа события
            virtual public int EventID { get; set; }

            // Время регистрации в модели
            virtual public int TimeRegistred { get; set; }

            // Время возникновения в модели (TimeRegistred + время ожидания)
            virtual public int TimeCaused { get; set; }

            // Доп. параметр события 
            virtual public object Param { get; set; }

        #endregion

        #region Constructors

            // Конструктор:
            //  current_time - текущее модельное время
            //  waiting_time - время ожидания возникновения
            //  param        - доп. параметр события
            // 
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
