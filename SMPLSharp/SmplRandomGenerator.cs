﻿// Содержание:
// - class SmplRandomGenerator
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMPLSharp.Utils
{
    // Random Number Generator
    public class SmplRandomGenerator
    {
        #region Public Properties
        
            // Текущее значение Seed
            public uint Seed
            {
                get;
                set;
            }

            // Первоначальное значение Seed
            public uint StartSeed
            {
                get;
                private set;
            }

        #endregion

        #region Constructors

            // Конструктор генератора, задающий seed от текущего времени 
            public SmplRandomGenerator()
            {
                Random r = new Random();
                Seed = StartSeed = (uint) r.Next();
            }

            // Конструкутор генератора с заданным seed
            public SmplRandomGenerator(uint seed)
            {
                Seed = StartSeed = seed;
            }

        #endregion

        #region Public Methods
        
            // Генерирует число по равномерному распределению 
            // в диапозоне [0, a] включительно
            public int IRandom(int a)
            {
                Seed = Seed * 1664525 + 1013904223;
                return (int)(Seed % (uint)(a + 1));
            }

            // Генерирует число по равномерному распределению 
            // в диапозоне [a, b] включительно
            public int IRandom(int a, int b)
            {
                Seed = Seed * 1664525 + 1013904223;
                return (int)(Seed % (uint)(b - a + 1)) + a;
            }

            // Генерирует число по отрицательному экспоненциальному распределению
            // со средней точкой m
            public int NegExp(int m)
            {
                Seed = Seed * 1664525 + 1013904223;
                float rnd = Seed / (float)uint.MaxValue;
                return (int) Math.Round(-m  * Math.Log(rnd) + 0.5);
            }
            
            // Генерирует число по распределению Пуассона
            // со параметром lambda
            public int Poisson(double lambda) {
                double L = Math.Exp(-lambda);
                double p = 1.0;
                int k = 0;
                Random r = new Random();
                do {
                    k++;
                    Seed = Seed * 1664525 + 1013904223;
                    float rnd = Seed / (float)uint.MaxValue;
                    p *= rnd;
                } while (p > L);
                return k - 1;
            }

        #endregion

    }
}