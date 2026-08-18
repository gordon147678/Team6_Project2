using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;
using System.Threading;

/// <summary>
/// 只负责在后台线程读取 Arduino 串口。
/// Unity 对象的任何修改都由主线程中的 MouseMovement 执行。
/// </summary>
internal sealed class ArduinoSerialReader : IDisposable
{
    private const int ReadTimeoutMilliseconds = 100;

    private readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();
    private readonly ConcurrentQueue<string> statuses = new ConcurrentQueue<string>();
    private readonly ManualResetEvent stopSignal = new ManualResetEvent(false);
    private readonly object portLock = new object();

    private Thread readerThread;
    private SerialPort serialPort;
    private volatile bool shouldStop;

    private string portName;
    private int baudRate;
    private int reconnectDelayMilliseconds;
    private bool dtrEnable;
    private string lastStatus;
    private bool disposed;

    public void Start(string newPortName, int newBaudRate, int newReconnectDelayMilliseconds, bool newDtrEnable)
    {
        ThrowIfDisposed();

        if (readerThread != null && readerThread.IsAlive)
        {
            return;
        }

        portName = newPortName.Trim();
        baudRate = newBaudRate;
        reconnectDelayMilliseconds = newReconnectDelayMilliseconds;
        dtrEnable = newDtrEnable;

        ClearQueue(messages);
        ClearQueue(statuses);
        lastStatus = null;
        shouldStop = false;
        stopSignal.Reset();

        readerThread = new Thread(ReadLoop)
        {
            IsBackground = true,
            Name = "Arduino Serial Reader"
        };
        readerThread.Start();
    }

    public void Stop()
    {
        if (readerThread == null)
        {
            return;
        }

        shouldStop = true;
        stopSignal.Set();
        CloseCurrentPort();

        if (readerThread.IsAlive)
        {
            readerThread.Join(1000);
        }

        readerThread = null;
    }

    public bool TryDequeueMessage(out string message)
    {
        return messages.TryDequeue(out message);
    }

    public bool TryDequeueStatus(out string status)
    {
        return statuses.TryDequeue(out status);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        Stop();
        stopSignal.Dispose();
        disposed = true;
    }

    private void ReadLoop()
    {
        while (!shouldStop)
        {
            SerialPort openedPort = null;

            try
            {
                if (string.IsNullOrWhiteSpace(portName))
                {
                    ReportStatus("串口名为空，请在 Inspector 中填写，例如 COM3。");
                    WaitBeforeReconnect();
                    continue;
                }

                openedPort = new SerialPort(portName, baudRate)
                {
                    Encoding = Encoding.ASCII,
                    NewLine = "\n",
                    ReadTimeout = ReadTimeoutMilliseconds,
                    DtrEnable = dtrEnable,
                    RtsEnable = false,
                    Handshake = Handshake.None
                };

                openedPort.Open();

                lock (portLock)
                {
                    serialPort = openedPort;
                }

                ReportStatus($"已连接 {portName}（{baudRate} baud）");

                while (!shouldStop && openedPort.IsOpen)
                {
                    try
                    {
                        string line = openedPort.ReadLine();

                        if (line != null)
                        {
                            messages.Enqueue(line);
                        }
                    }
                    catch (TimeoutException)
                    {
                        // 超时用来定期检查停止信号，不是错误。
                    }
                }
            }
            catch (Exception exception)
            {
                if (!shouldStop)
                {
                    ReportStatus($"连接 {portName} 失败：{exception.Message}");
                }
            }
            finally
            {
                lock (portLock)
                {
                    if (ReferenceEquals(serialPort, openedPort))
                    {
                        serialPort = null;
                    }
                }

                ClosePort(openedPort);
            }

            if (!shouldStop)
            {
                WaitBeforeReconnect();
            }
        }
    }

    private void WaitBeforeReconnect()
    {
        stopSignal.WaitOne(reconnectDelayMilliseconds);
    }

    private void ReportStatus(string status)
    {
        if (status == lastStatus)
        {
            return;
        }

        lastStatus = status;
        statuses.Enqueue(status);
    }

    private void CloseCurrentPort()
    {
        SerialPort currentPort;

        lock (portLock)
        {
            currentPort = serialPort;
            serialPort = null;
        }

        ClosePort(currentPort);
    }

    private static void ClosePort(SerialPort port)
    {
        if (port == null)
        {
            return;
        }

        try
        {
            if (port.IsOpen)
            {
                port.Close();
            }
        }
        catch
        {
            // 关闭时的错误不应阻止 Unity 退出或切回键盘模式。
        }
        finally
        {
            port.Dispose();
        }
    }

    private static void ClearQueue(ConcurrentQueue<string> queue)
    {
        while (queue.TryDequeue(out _))
        {
        }
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(ArduinoSerialReader));
        }
    }
}
