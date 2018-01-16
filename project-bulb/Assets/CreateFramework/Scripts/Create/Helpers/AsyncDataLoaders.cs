using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;

namespace Create.Helpers
{
    public class AsyncCoroutineAction : CustomYieldInstruction
    {
        private bool _isDone;
        public override bool keepWaiting
        {
            get
            {
                return !_isDone;
            }
        }

        private Action<object> _action;

        public AsyncCoroutineAction(Action<object> action)
        {
            _action = action;
            ThreadPool.QueueUserWorkItem(new WaitCallback(AwaitAction));
        }

        private void AwaitAction(object o)
        {
            try
            {
                _action(o);
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("Exception occurred during async task: {0}\r\n{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                _isDone = true;
            }
        }
    }

    public class AsyncXmlParser<T> : CustomYieldInstruction
    {
        private bool _isDone;
        public override bool keepWaiting
        {
            get
            {
                return !_isDone;
            }
        }

        public T Result
        {
            get; private set;
        }

        private string _path;

        public AsyncXmlParser(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarningFormat("[{0}] Data file {1} not found!", GetType(), path);
                return;
            }

            _path = path;
            WaitCallback action = new WaitCallback(RunParse);
            ThreadPool.QueueUserWorkItem(action);
        }

        private void RunParse(object param)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var stream = File.OpenRead(_path))
                {
                    Result = (T)serializer.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("[{0}] Error while parsing {1} : {2}\r\n{3}", GetType(), _path, ex.Message, ex.StackTrace);
            }
            finally
            {
                _isDone = true;
            }
        }
    }

    public class AsyncDataContractSerializerParser<T> : CustomYieldInstruction
    {
        private bool _isDone;
        public override bool keepWaiting
        {
            get
            {
                return !_isDone;
            }
        }

        public T Result
        {
            get; private set;
        }

        private string _path;
        private List<Type> _knownTypes;

        public AsyncDataContractSerializerParser(string path, List<Type> knownTypes = null)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarningFormat("[{0}] Data file {1} not found!", GetType(), path);
                return;
            }

            _path = path;
            _knownTypes = knownTypes;
            WaitCallback action = new WaitCallback(RunParse);
            ThreadPool.QueueUserWorkItem(action);
        }

        private void RunParse(object param)
        {
            try
            {
                var serializer = new DataContractSerializer(typeof(T), _knownTypes);
                using (var stream = File.OpenRead(_path))
                {
                    Result = (T)serializer.ReadObject(stream);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("[{0}] Error while parsing {1} : {2}\r\n{3}", GetType(), _path, ex.Message, ex.StackTrace);
            }
            finally
            {
                _isDone = true;
            }
        }
    }

    public class AsyncCsvLoader : CustomYieldInstruction
    {
        private bool _isDone;
        public override bool keepWaiting
        {
            get
            {
                return !_isDone;
            }
        }

        public List<string[]> Result
        {
            get; private set;
        }

        private string _path;
        private char _splitChar;

        public AsyncCsvLoader(string path, char splitChar = ';')
        {
            if (!File.Exists(path))
            {
                Debug.LogWarningFormat("[{0}] Csv file {1} not found!", GetType(), path);
                return;
            }

            _path = path;
            _splitChar = splitChar;
            WaitCallback action = new WaitCallback(RunLoad);
            ThreadPool.QueueUserWorkItem(action);
        }

        private void RunLoad(object param)
        {
            try
            {
                string[] lines = File.ReadAllLines(_path);
                Result = new List<string[]>(lines.Length);
                foreach (var line in lines)
                {
                    Result.Add(line.Split(new char[] { _splitChar }));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("[{0}] Error while loading csv {1} : {2}\r\n{3}", GetType(), _path, ex.Message, ex.StackTrace);
            }
            finally
            {
                _isDone = true;
            }
        }
    }
}