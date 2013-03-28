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
    // Represents place where tokens are waiting when equipment is busy
    public class SmplQueue
    {

        #region Public Properties

            // Имя очереди
            virtual public string Name
            {
                get;
                protected set;
            }

            // Длина очереди
            public int Length
            {
                get { return Elements.Count(); }
            }

            // Максимальная длина очереди
            virtual public int MaxLength
            {
                get;
                protected set;
            }
        
            // Количество элементов, удаленных из очереди
            virtual public int CountPassed
            {
                get;
                protected set;
            }

            // Время последнего изменения длины очереди
            virtual public int TimeLastChanged
            {
                get;
                protected set;
            }

            // Сумма периодов ожиданий
            virtual public int WaitingPeriodSum
            {
                get;
                protected set;
            }

            // Сумма квадратов периодов ожиданий
            virtual public int WaitingPeriodSq2Sum
            {
                get;
                protected set;
            }

            // Сумма длин очереди * соответсвующие периоды изменения 
            // Интерграл Q(t), где Q - длина очереди, t - модельное время
            // Используется для вычисления средней длины очереди
            virtual public int LengthOfTimeIntegral
            {
                get;
                protected set;
            }

            // Очередь пуста?
            public bool IsEmpty
            {
                get { return Length == 0; }
            }

            // Элементы очереди
            virtual public ICollection<SmplQueueElement> Elements
            {
                get { return elements; }
            }

            // Модель, с которой связана очередь
            public SmplModel Model
            {
                get;
                protected set;
            }

        #endregion

        #region Constructors

            // Конструктор очереди. 
            // Экземпляры создаются через SMPLModel
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
            //
            private List<SmplQueueElement> elements;

        #endregion

        #region Public Methods

            // Добавить новый элемент (1) в очередь
            virtual public void Enqueue()
            {
                Enqueue(1);
            }

            // Добавить новый элемент token в очередь с заданным приоритетом
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

            // Извлечь первый элемент очереди
            // В случае пустой очереди генерирует исключение
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

            // Обновление статистики при добавлении в очередь
            protected void updateStatiscticIncrease(int prev_length)
            {
                if (MaxLength < Length) MaxLength = Length;
                LengthOfTimeIntegral += prev_length * (Model.Time - TimeLastChanged);
                TimeLastChanged = Model.Time;
            }

            // Обновление статистики при извлечении из очереди
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

    // One queue element
    public class SmplQueueElement
    {

        #region Public Properties

            // Содержимое элемента
            virtual public object Value
            {
                get;
                protected set;
            }

            // Приоритет элемента
            virtual public int Priority
            {
                get;
                protected set;
            }

            // Время добавления в очередь
            virtual public int TimeAdded
            {
                get;
                set;
            }

        #endregion

        #region Constructors

            // Конструктор элемента:
            //  value    - содержимое элемента
            //  priority - приоритет элемента
            // Экземпляры создаются через SMPLModel
            internal SmplQueueElement(object token, int priority)
            {
                Value = token;
                Priority = priority;
            }

        #endregion

    }
}
