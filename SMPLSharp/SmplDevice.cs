﻿// Содержание:
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

            /// <summary>
            /// Имя прибора
            /// </summary> 
            virtual public string Name
            {
                get;
                private set;
            }

            /// <summary>
            /// Состояние прибора. Если Status == null, прибор свободен
            /// </summary> 
            virtual public object Status
            {
                get;
                private set;
            }
        
            /// <summary>
            ///  Прибор занят?
            /// </summary>
            public bool IsBusy
            {
                get { return Status != null; }
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
            /// <param name="model">Модель</param>
            /// <param name="name">Имя прибора</param>
 
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

        /// <summary>
        /// Зарезервировать прибор (param = 1)
        /// </summary>
 
            public void Reserve()
            {
                Reserve(1);
            }

            /// <summary>
            /// Зарезервировать прибор и поменять его статус на param. В случае передачи token == null, прибор будет освобожден
            /// </summary>
            /// <param name="token"></param>
 
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

            /// <summary>
            /// Освоболить прибор
            /// </summary>
 
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
