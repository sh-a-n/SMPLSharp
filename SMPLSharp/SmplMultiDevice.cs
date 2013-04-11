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
    /// <summary>
    /// Equipment or facility
    /// Typically represents some work-performing resource of system being modeled
    /// The Interconnection of facilities is not explicit, but can be determined 
    ///   by the model’s routing of tokens between facilities
    /// </summary>
    public class SmplMultiDevice
    {

        #region Public Properties

            /// <summary>
            ///  Имя прибора
            /// </summary>

            virtual public string Name
            {
                get;
                private set;
            }

            /// <summary>
            /// Канал прибора
            /// </summary>
 
            public struct Ambary
            {
                /// <summary>
                /// Если Status == null, канал свободен
                /// </summary>
 
                public bool Status
                {
                    get;
                    set;
                }
                /// <summary>
                /// Время последнего резервирования
                /// </summary>
 
                public int TimeLastReserved
                {
                    get;
                    set;
                }
                /// <summary>
                /// Суммарное время, пока канал занят
                /// </summary>
 
                public int TimeTotalReserved
                {
                    get;
                    set;
                }
                /// <summary>
                /// Счетчик запросов (пар вызовов reserve/release)
                /// </summary>
 
                public int QueryCounter
                {
                    get;
                    set;
                }

            }
            
            /// <summary>
            /// Количество каналов в приборе
            /// </summary>
 
            public int CountAmbary
            {
                get;
                private set;
            }

            /// <summary>
            /// Массив каналов прибора
            /// </summary>
 
            public Ambary [] ArrAmbary;
        
            /// <summary>
            /// Все каналы прибора заняты?
            /// </summary>
 
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

            /// <summary>
            /// Время последнего вызова Reserve
            /// </summary>
 
            virtual public int TimeLastReserved
            {
                get;
                protected set;
            }

            /// <summary>
            /// Суммарное время, пока прибор занят
            /// </summary>
 
            virtual public int TimeTotalReserved
            {
                get;
                protected set;
            }

            /// <summary>
            /// Счетчик запросов (пар вызовов reserve/release)
            /// </summary>
 
            virtual public int QueryCounter
            {
                get;
                protected set;
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
            /// Конструктор прибора. Экземпляры создаются через SMPLModel
            /// </summary>
            /// <param name="model">модель</param>
            /// <param name="name">имя прибора</param>
            /// <param name="countAmbary">количество каналов</param>
            /// Пример использования:
            /// @code
            /// SmplMpdel model =  new SmplModel();
            /// SmplMultyDevice mdevice = new MultyDevice(model, "multydevice1", 2);
            /// @endcode 
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

            /// <summary>
            /// Зарезервировать прибор (param = 1)
            /// </summary>
            /// <returns></returns>
            /// Пример использования:
            /// @code
            /// SmplMpdel model =  new SmplModel();
            /// SmplMultyDevice mdevice = new MultyDevice(model, "multydevice1", 2);
            /// mdevice.Reserve();
            /// @endcode
            public int Reserve()
            {
                return Reserve(1);
            }

            /// <summary>
            /// Зарезервировать прибор и поменять статус первого свободного канала на true. В случае передачи token == null, прибор будет освобожден
            /// </summary>
            /// <param name="token"></param>
            /// <param name="id">номер канала</param>
            /// <returns>номер занятого канала прибора</returns>
            /// Пример использования:
            /// @code
            /// SmplMpdel model =  new SmplModel();
            /// SmplMultyDevice mdevice = new MultyDevice(model, "multydevice1", 2);
            /// mdevice.Reserve(1, 0);
            /// @endcode
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

            /// <summary>
            /// Освоболить канал прибора с индексом id
            /// </summary>
            /// <param name="id">индекс канала</param>
            /// Пример использования:
            /// @code
            /// SmplMpdel model =  new SmplModel();
            /// SmplMultyDevice mdevice = new MultyDevice(model, "multydevice1", 2);
            /// mdevice.Reserve(1,0);
            /// mdevice.Release(0);
            /// @endcode 
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
