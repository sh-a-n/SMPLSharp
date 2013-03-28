// Содержание:
// - class SmplMultiDevice
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
    public class SmplMultiDevice
    {

        #region Public Properties

            // Имя прибора
            virtual public string Name
            {
                get;
                private set;
            }

            // Канал прибора
            public struct Ambary
            {
                // Если Status == null, канал свободен
                public bool Status
                {
                    get;
                    set;
                }
                // Время последнего резервирования
                public int TimeLastReserved
                {
                    get;
                    set;
                }
                // Суммарное время, пока канал занят
                public int TimeTotalReserved
                {
                    get;
                    set;
                }
                // Счетчик запросов (пар вызовов reserve/release)
                public int QueryCounter
                {
                    get;
                    set;
                }

            }
            
            // Количество каналов в приборе
            public int CountAmbary
            {
                get;
                private set;
            }

            // Массив каналов прибора
            public Ambary [] ArrAmbary;
        
            // Все каналы прибора заняты?
            public bool IsBusy
            {
                get 
                {
                    foreach(Ambary Amb in ArrAmbary)
                    {
                        if(Amb.Status == false) return false; 
                    }
                    return true;
                }
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
            internal SmplMultiDevice(SmplModel model, string name, int countAmbary)
            {
                Model = model;
                Name = name;
                TimeTotalReserved = 0;
                QueryCounter = 0;
                TimeLastReserved = 0;
                CountAmbary = countAmbary;
                ArrAmbary =  new Ambary[CountAmbary];
            }

        #endregion

        #region Public Methods

            // Зарезервировать прибор (param = 1)
            public int Reserve()
            {
                return Reserve(1);
            }

            // Зарезервировать прибор и поменять статус первого свободного канала на true
            // В случае передачи token == null, прибор будет освобожден
            public int Reserve(object token, int id = -1)
            {
                if ((id < 0) || (id > (CountAmbary - 1)))
                    id = -1;
                if (token != null)
                {
                    if (id == -1)
                    {
                        for (int i = 0; i < CountAmbary; i++)
                        {
                            if (ArrAmbary[i].Status == false)
                            {
                                ArrAmbary[i].TimeLastReserved = Model.Time;
                                ArrAmbary[i].Status = true;
                                TimeLastReserved = Model.Time;
                                return i;
                            }
                        }
                    }
                    else
                    {
                        if (ArrAmbary[id].Status == false)
                        {
                            ArrAmbary[id].TimeLastReserved = Model.Time;
                            ArrAmbary[id].Status = true;
                            TimeLastReserved = Model.Time;
                            return id;
                        }
                    }
                }
                else Release(id);
                return -1;
            }

            // Освоболить канал прибора с индексом id
            public void Release(int id)
            {
                if (ArrAmbary[id].Status != false)
                {
                    ArrAmbary[id].QueryCounter++;
                    ArrAmbary[id].TimeTotalReserved += Model.Time - ArrAmbary[id].TimeLastReserved;
                    ArrAmbary[id].Status = false;
                    QueryCounter++;
                    TimeTotalReserved += Model.Time - TimeLastReserved;
                }
            }

        #endregion

    }
}
