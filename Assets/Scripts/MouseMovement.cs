using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    public enum ControlMode
    {
        Keyboard,
        Arduino
    }

    private const int ReceiverCount = 5;
    private const int MaxSerialMessagesPerFrame = 128;

    [Header("控制方式")]
    [Tooltip("可以在运行前或运行时切换。Arduino 模式会占用串口，切回键盘后会释放串口。")]
    [SerializeField] private ControlMode controlMode = ControlMode.Keyboard;

    [Header("键盘控制")]
    [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode moveRightKey = KeyCode.D;

    [Header("车道移动")]
    public float laneWidth = 1f;
    public float moveSpeed = 5f;

    [Header("Arduino 串口")]
    [SerializeField] private string portName = "COM3";
    [SerializeField] private int baudRate = 115200;
    [SerializeField, Min(100)] private int reconnectDelayMilliseconds = 1000;
    [SerializeField] private bool dtrEnable = true;
    [SerializeField] private bool logArduinoMessages;

    [Header("Arduino 运行状态")]
    [Tooltip("运行时在 Inspector 中查看串口连接结果。")]
    [SerializeField] private string arduinoConnectionStatus = "键盘模式（串口未启用）";
    [Tooltip("-1 = 未知，0 = 检测到红外线，1 = 未检测到红外线。数组下标 0-4 对应 1-5 号接收器。")]
    [SerializeField] private int[] receiverStates = { -1, -1, -1, -1, -1 };

    private int currentLane;
    private float targetX;
    private bool isChangingLane;

    private ArduinoSerialReader arduinoReader;
    private ControlMode activeControlMode;
    private bool controlModeInitialized;
    private string activePortName;
    private int activeBaudRate;
    private int activeReconnectDelay;
    private bool activeDtrEnable;

    public ControlMode CurrentControlMode => controlMode;
    public bool IsChangingLane => isChangingLane;
    public string ArduinoConnectionStatus => arduinoConnectionStatus;

    private void OnEnable()
    {
        targetX = transform.position.x;
        EnsureReceiverStateArray();
        EnsureArduinoReader();
        ApplyControlMode();
    }

    private void Update()
    {
        if (!controlModeInitialized || activeControlMode != controlMode || ArduinoConfigurationChanged())
        {
            ApplyControlMode();
        }

        DrainArduinoStatusMessages();

        if (controlMode == ControlMode.Keyboard)
        {
            UpdateKeyboardInput();
        }
        else
        {
            UpdateArduinoInput();
        }

        UpdateLaneMovement();
    }

    private void OnDisable()
    {
        StopArduinoReader();
        controlModeInitialized = false;
    }

    private void OnDestroy()
    {
        if (arduinoReader == null)
        {
            return;
        }

        arduinoReader.Dispose();
        arduinoReader = null;
    }

    public void SetControlMode(ControlMode mode)
    {
        controlMode = mode;

        if (isActiveAndEnabled)
        {
            ApplyControlMode();
        }
    }

    public void SetLane(int lane)
    {
        int newLane = Mathf.Clamp(lane, -2, 2);

        if (newLane == currentLane && Mathf.Abs(transform.position.x - targetX) < 0.001f)
        {
            return;
        }

        currentLane = newLane;
        targetX = currentLane * laneWidth;
        isChangingLane = Mathf.Abs(transform.position.x - targetX) >= 0.001f;
    }

    public int GetReceiverState(int receiverNumber)
    {
        if (receiverNumber < 1 || receiverNumber > ReceiverCount)
        {
            return -1;
        }

        EnsureReceiverStateArray();
        return receiverStates[receiverNumber - 1];
    }

    private void UpdateKeyboardInput()
    {
        if (isChangingLane)
        {
            return;
        }

        if (Input.GetKeyDown(moveLeftKey))
        {
            ChangeLane(-1);
        }
        else if (Input.GetKeyDown(moveRightKey))
        {
            ChangeLane(1);
        }
    }

    private void UpdateArduinoInput()
    {
        int processedMessageCount = 0;

        while (processedMessageCount < MaxSerialMessagesPerFrame &&
               arduinoReader.TryDequeueMessage(out string message))
        {
            processedMessageCount++;
            ProcessArduinoMessage(message);
        }
    }

    private void ProcessArduinoMessage(string rawMessage)
    {
        string message = rawMessage.Trim();

        if (message.Length != 2 ||
            message[0] < '1' || message[0] > '5' ||
            message[1] < '0' || message[1] > '1')
        {
            if (logArduinoMessages)
            {
                Debug.LogWarning($"Arduino 消息格式无效：'{message}'，期望格式为 [1-5][0-1]。", this);
            }

            return;
        }

        int receiverIndex = message[0] - '1';
        int newState = message[1] - '0';

        if (receiverStates[receiverIndex] == newState)
        {
            return;
        }

        receiverStates[receiverIndex] = newState;

        if (logArduinoMessages)
        {
            Debug.Log($"Arduino: {receiverIndex + 1} 号接收器变为 {newState}。", this);
        }

        // 低电平有效：状态变为 0 时，移动到该接收器对应的绝对车道。
        // 接收器 1-5 对应车道 -2 到 2。状态变为 1 时只记录，不改变车道。
        if (newState == 0)
        {
            SetLane(receiverIndex - 2);
        }
    }

    private void UpdateLaneMovement()
    {
        if (!isChangingLane)
        {
            return;
        }

        Vector3 position = transform.position;
        position.x = Mathf.MoveTowards(position.x, targetX, moveSpeed * Time.deltaTime);
        transform.position = position;

        if (Mathf.Abs(transform.position.x - targetX) >= 0.001f)
        {
            return;
        }

        Vector3 finalPosition = transform.position;
        finalPosition.x = targetX;
        transform.position = finalPosition;
        isChangingLane = false;
    }

    private void ChangeLane(int direction)
    {
        SetLane(currentLane + direction);
    }

    private void ApplyControlMode()
    {
        EnsureArduinoReader();
        activeControlMode = controlMode;
        controlModeInitialized = true;

        activePortName = portName;
        activeBaudRate = baudRate;
        activeReconnectDelay = reconnectDelayMilliseconds;
        activeDtrEnable = dtrEnable;

        StopArduinoReader();

        if (controlMode == ControlMode.Keyboard)
        {
            arduinoConnectionStatus = "键盘模式（Arduino 串口未启用）";
            return;
        }

        ResetReceiverStates();
        arduinoConnectionStatus = $"正在连接 {portName}...";
        arduinoReader.Start(
            portName,
            Mathf.Max(1, baudRate),
            Mathf.Max(100, reconnectDelayMilliseconds),
            dtrEnable);
    }

    private bool ArduinoConfigurationChanged()
    {
        return controlMode == ControlMode.Arduino &&
               (activePortName != portName ||
                activeBaudRate != baudRate ||
                activeReconnectDelay != reconnectDelayMilliseconds ||
                activeDtrEnable != dtrEnable);
    }

    private void DrainArduinoStatusMessages()
    {
        if (arduinoReader == null)
        {
            return;
        }

        while (arduinoReader.TryDequeueStatus(out string status))
        {
            arduinoConnectionStatus = status;

            if (logArduinoMessages)
            {
                Debug.Log($"Arduino: {status}", this);
            }
        }
    }

    private void EnsureArduinoReader()
    {
        if (arduinoReader == null)
        {
            arduinoReader = new ArduinoSerialReader();
        }
    }

    private void StopArduinoReader()
    {
        if (arduinoReader != null)
        {
            arduinoReader.Stop();
        }
    }

    private void EnsureReceiverStateArray()
    {
        if (receiverStates == null || receiverStates.Length != ReceiverCount)
        {
            receiverStates = new[] { -1, -1, -1, -1, -1 };
        }
    }

    private void ResetReceiverStates()
    {
        EnsureReceiverStateArray();

        for (int i = 0; i < receiverStates.Length; i++)
        {
            receiverStates[i] = -1;
        }
    }
}
