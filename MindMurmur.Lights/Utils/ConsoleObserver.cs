﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMurmur.Lights.Utils
{

    public class ConsoleObserver<T> : IObserver<T>
    {
        private readonly string _name;

        public ConsoleObserver(string name = "")
        {
            _name = name;
        }

        public void OnNext(T value)
        {
            Console.WriteLine("{0} - OnNext({1})", _name, value);
        }

        public void OnError(Exception error)
        {
            Console.WriteLine("{0} - OnError:", _name);
            Console.WriteLine("\t {0}", error);
        }

        public void OnCompleted()
        {
            Console.WriteLine("{0} - OnCompleted()", _name);
        }
    }
}
