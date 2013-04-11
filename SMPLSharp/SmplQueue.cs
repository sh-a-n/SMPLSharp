// Содержание:
// - class SmplQueue
// - class SmplQueueElement
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMPLSharp.Objects
{
    // Queue
    /// <summary>
    /// Очередь. Represents place where tokens are waiting when equipment is busy
    /// </summary>
 
    public class SmplQueue
    {

        #region Public Properties

            /// <summary>
            /// Имя очереди
            /// </summary>
 
            virtual public string Name
            {
                get;
                protected set;
            }

            /// <summary>
            /// Длина очереди
            /// </summary>
 
            public int Length
            {
                get { return Elements.Count(); }
            }

            /// <summary>
            /// Максимальная длина очереди
            /// </summary>
 
            virtual public int MaxLength
            {
                get;
                protected set;
            }
        
            /// <summary>
            /// Количество элементов, удаленных из очереди
            /// </summary>
 
            virtual public int CountPassed
            {
                get;
                protected set;
            }

            /// <summary>
            /// Время последнего изменения длины очереди
            /// </summary>
 
            virtual public int TimeLastChanged
            {
                get;
                protected set;
            }

            /// <summary>
            /// Сумма периодов ожиданий
            /// </summary>
 
            virtual public int WaitingPeriodSum
            {
                get;
                protected set;
            }

            /// <summary>
            /// Сумма квадратов периодов ожиданий
            /// </summary>
 
            virtual public int WaitingPeriodSq2Sum
            {
                get;
                protected set;
            }

            // 
        /// <summary>
        /// Сумма длин очереди * соответсвующие периоды изменения. Интерграл Q(t), где Q - длина очереди, t - модельное время. Используется для вычисления средней длины очереди
        /// </summary>
            virtual public int LengthOfTimeIntegral
            {
                get;
                protected set;
            }

            /// <summary>
            ///  Очередь пуста?
            /// </summary>

            public bool IsEmpty
            {
                get { return Length == 0; }
            }

            /// <summary>
            /// Элементы очереди
            /// </summary>
 
            virtual public ICollection<SmplQueueElement> Elements
            {
                get { return elements; }
            }

            /// <summary>
            /// Модель, с которой связана очередь
            /// </summary>
 
            public SmplModel Model
            {
                get;
                protected set;
            }

        #endregion

        #region Constructors

            /// <summary>
            /// Конструктор очереди. Экземпляры создаются через SMPLModel
            /// </summary>
            /// <param name="model">модель</param>
            /// <param name="name">имя очереди</param>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// SmplQueue queue = new SmplQueue(model, "queue1");
            /// @endcode
            internal SmplQueue(SmplModel model, string name)
            {
                Model = model;
                Name = name;
                elements = new List<SmplQueueElement>();
            }

        #endregion

        #region Protected/Private Data Fields

            // Элементы очереди
            // [2][2][2][1][1] (Указаны приоритеты)
            // 
            // При извлечении берем первый из элементов
            // При вставке добавляем в конец списка (пропускаем элементы с более низким приоритетом)
            /// <summary>
            /// Элементы очереди
            /// [2][2][2][1][1] (Указаны приоритеты)
            /// 
            /// При извлечении берем первый из элементов
            /// При вставке добавляем в конец списка (пропускаем элементы с более низким приоритетом)
            /// </summary>
            private List<SmplQueueElement> elements;

        #endregion

        #region Public Methods

            /// <summary>
            /// Добавить новый элемент (1) в очередь
            /// </summary>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// SmplQueue queue = new SmplQueue(model, "queue1");
            /// queue.Enqueue();
            /// @endcode
            virtual public void Enqueue()
            {
                Enqueue(1);
            }

            /// <summary>
            /// Добавить новый элемент token в очередь с заданным приоритетом
            /// </summary>
            /// <param name="token">элемент</param>
            /// <param name="priority">приоритет</param>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// SmplQueue queue = new SmplQueue(model, "queue1");
            /// queue.Enqueue("elem1", 1);
            /// @endcode
            virtual public void Enqueue(object token, int priority = 0)
            {
                var new_element = new SmplQueueElement(token, priority);
                new_element.TimeAdded = Model.Time;
                
                // Пропуск элементов с меньшим приоритетом
                int p;
                for (p = elements.Count - 1; p >= 0 && elements[p].Priority < priority; p--) ;

                // Вставка
                var l = Length;
                elements.Insert(p + 1, new_element);

                // Изменение статистики
                updateStatiscticIncrease(l);
            }

            
        /// <summary>
        /// Извлечь первый элемент очереди
        /// </summary>
        /// <returns>Возвращает элемент очереди. В случае пустой очереди генерирует исключение</returns>
            /// Пример использования:
            /// @code
            /// SmplModel model = new SmplModel();
            /// SmplQueue queue = new SmplQueue(model, "queue1");
            /// queue.Enqueue("elem1", 1);
            /// var element = queue.Head();
            /// @endcode
            virtual public object Head()
            {
                var l = Length;
                if (l > 0)
                {
                    // Извлечение
                    var element = elements[0];
                    elements.RemoveAt(0);

                    // Изменение статистики
                    updateStatiscticDecrease(l, element);
                    
                    return element;
                }
                else throw new Exception("Trying get from empty queue");
            }

        #endregion

        #region Protected/Private Methods

            /// <summary>
            /// Обновление статистики при добавлении в очередь
            /// </summary>
            /// <param name="prev_length"></param>
 
            protected void updateStatiscticIncrease(int prev_length)
            {
                if (MaxLength < Length) MaxLength = Length;
                LengthOfTimeIntegral += prev_length * (Model.Time - TimeLastChanged);
                TimeLastChanged = Model.Time;
            }

            /// <summary>
            /// Обновление статистики при извлечении из очереди
            /// </summary>
            /// <param name="prev_length"></param>
            /// <param name="element"></param>
 
            protected void updateStatiscticDecrease(int prev_length, SmplQueueElement element)
            {
                LengthOfTimeIntegral += prev_length * (Model.Time - TimeLastChanged);
                TimeLastChanged = Model.Time;
                CountPassed++;
                var w = Model.Time - element.TimeAdded;
                WaitingPeriodSum += w;
                WaitingPeriodSq2Sum += w * w;
            }

        #endregion
    }

    /// <summary>
    /// Элемент очереди
    /// </summary>
    public class SmplQueueElement
    {

        #region Public Properties

            /// <summary>
            /// Содержимое элемента
            /// </summary>
 
            virtual public object Value
            {
                get;
                protected set;
            }

            /// <summary>
            /// Приоритет элемента
            /// </summary>
 
            virtual public int Priority
            {
                get;
                protected set;
            }

            /// <summary>
            /// Время добавления в очередь
            /// </summary>
 
            virtual public int TimeAdded
            {
                get;
                set;
            }

        #endregion

        #region Constructors

            
        /// <summary>
        /// Конструктор элемента
        /// </summary>
        /// <param name="token">содержимое элемента</param>
        /// <param name="priority">приоритет элемента</param>
            internal SmplQueueElement(object token, int priority)
            {
                Value = token;
                Priority = priority;
            }

        #endregion

    }
}
