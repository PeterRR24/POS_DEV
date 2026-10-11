using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RR.Utility
{
    public class RRSerialport
    {
        StringBuilder sb;
        Task newTask = null;
        CustomAutoResetEvent stopWaitHandle = null;
        private delegate void SetTextDeleg(string text);
        private string _responseData = String.Empty;
        public static int Timeout;
        public string ProcessRequest(SerialportRequest request)
        {
            sb = new StringBuilder();
            _responseData = string.Empty;
            SerialPort _serialPort = new SerialPort();

            Parity selectedParity;
            selectedParity = (Parity)Enum.Parse(typeof(Parity), request.Parity);
            StopBits selectedStopBits;
            selectedStopBits = (StopBits)Enum.Parse(typeof(StopBits), request.StopBit);
            Timeout = request.Timeout;
            _serialPort = new SerialPort(request.ComPort, Convert.ToInt32(request.BaudRate), selectedParity, 8, selectedStopBits);
            
            if (!(_serialPort.IsOpen))
                _serialPort.Open();
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            _serialPort.Write(request.requestToProcess);
            StartNewTask(ReceiveComplete);
            return _responseData;
        }
        public void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int intBuffer;
            intBuffer = sp.BytesToRead;
            byte[] byteBuffer = new byte[intBuffer];
            sp.Read(byteBuffer, 0, intBuffer);
            sb.Append(Encoding.Default.GetString(byteBuffer));
            _responseData = sb.ToString().Trim();
            ReceiveComplete();
        }
        private void ReceiveComplete()
        {
            try
            {
                if (_responseData.Contains("</CONNFROMPOS>") || _responseData.Contains("</FROMPOS>"))
                     stopWaitHandle.Set();
            }
            catch (Exception ex)
            {
                stopWaitHandle.Set();
            }
        }
        public void StartNewTask(Action action)
        {
            if (newTask != null)
            {
                newTask.Dispose();
            }
            newTask = new Task(action);
            newTask.Start();
            stopWaitHandle = new CustomAutoResetEvent(false);
            stopWaitHandle.WaitOne();
        }
        class CustomAutoResetEvent
        {
            AutoResetEvent stopWaitHandle = null;
            public CustomAutoResetEvent(bool initialState)
            {
                stopWaitHandle = new AutoResetEvent(initialState);
            }

            public bool WaitOne()
            {
                return stopWaitHandle.WaitOne(Timeout); //wait for 30 seconds to receive response 30000
            }

            public bool Set()
            {
                bool result = false;
                if (stopWaitHandle != null)
                {
                    result = stopWaitHandle.Set();
                    stopWaitHandle.Close();
                    stopWaitHandle.Dispose();
                    stopWaitHandle = null;
                    return result;
                }
                else
                    return result;
            }
        }
    }

    public class SerialportRequest
    {
        public string ComPort { get; set; }
        public string BaudRate { get; set; }
        public string Parity { get; set; }
        public string StopBit { get; set; }
        public string requestToProcess { get; set; }
        public int Timeout { get; set; }
    }




}




