// Содержание:
// - class SmplEquipment
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMPLSharp.Objects
{
    // Equipment or facility
    // Typically represents some work-performing resource of system being modeled
    // The Interconnection of facilities is not explicit, but can be determined 
    //   by the model’s routing of tokens between facilities
    //
    public class SmplDevice
    {

        #region Public Properties

            // Имя прибора
            virtual public string Name
            {
                get;
                private set;
            }

            // Состояние прибора
            // Если Status == null, прибор свободен
            virtual public object Status
            {
                get;
                private set;
            }
        
            // Прибор занят?
            public bool IsBusy
            {
                get { return Status != null; }
            }

            // Время последнего вызова Reserve
            virtual public int TimeLastReserved
            {
                get;
                protected set;
            }

            // Суммарное время, пока прибор занят
            virtual public int TimeTotalReserved
            {
                get;
                protected set;
            }

            // Счетчик запросов (пар вызовов reserve/release)
            virtual public int QueryCounter
            {
                get;
                protected set;
            }

            // Модель, с которой связан прибор
            public SmplModel Model
            {
                get;
                protected set;
            }

        #endregion

        #region Constructors

            // Конструктор прибора. 
            // Экземпляры создаются через SMPLModel
            internal SmplDevice(SmplModel model, string name)
            {
                Model = model;
                Name = name;
                TimeTotalReserved = 0;
                QueryCounter = 0;
                TimeLastReserved = 0;
                Status = null;
            }

        #endregion

        #region Public Methods

        // Зарезервировать прибор (param = 1)
            public void Reserve()
            {
                Reserve(1);
            }

            // Зарезервировать прибор и поменять его статус на param
            // В случае передачи token == null, прибор будет освобожден
            public void Reserve(object token)
            {
                if (token != null)
                {
                    TimeLastReserved = Model.Time;
                    Status = token;
                    //throw new NotImplementedException();
                }
                else
                    Release();
            }

            // Освоболить прибор
            public void Release()
            {
                if (Status != null)
                {
                    QueryCounter++;
                    TimeTotalReserved += Model.Time - TimeLastReserved;
                    Status = null;
                    //throw new NotImplementedException();
                }
            }

        #endregion

    }
}
