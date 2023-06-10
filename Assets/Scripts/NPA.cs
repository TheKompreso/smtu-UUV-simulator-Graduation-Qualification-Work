using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;

public class NPA : MonoBehaviour
{
    [Serializable]
    public class Clock
    {
        public int hours;
        public int minutes;
        public int seconds;
        public int miliseconds;
    }
    private Clock time = new();
    private Clock EndTime = new();
    private List<float> batteryStats = new();
    public static NPA Instance { get; private set; }
    [Header("Object on scene")]
    public TMP_Text OutputText;
    public Transform cable;
    [Header("NPA Options")]
    [Range(1.0f, 15.0f)]
    public float speedMax;
    public static float speed;
    private float battery;
    [Range(10.0f, 12000.0f)]
    public float batteryStart;
    [Range(10.0f, 12000.0f)]
    public float batteryMax;
    [Range(1, 1200)]
    public float basicChargeConsumption;
    [Range(1, 1200)]
    public float motorChargeConsumption;
    private float chargeUpdate;
    [Range(50.0f, 500.0f)]
    public float maxImmersionDepth;
    [Range(50.0f, 500.0f)]
    public float cableLength;
    private void Awake()
    {
        Instance = this;
        MoveEngine.immersionDepth = -maxImmersionDepth;
    }
    private void Start()
    {
        battery = batteryStart;
        chargeUpdate = (basicChargeConsumption / 60 / 60) * Time.deltaTime;

    }
    void Update()
    {
        OutputText.text = $"Время: {time.hours.ToString("D2")}:{time.minutes.ToString("D2")}:{time.seconds.ToString("D2")}\n" +
            $"Заряд батареи: {string.Format("{0:0.00}", battery / batteryMax * 100)}% (выключится в {EndTime.hours.ToString("D2")}:{EndTime.minutes.ToString("D2")}:{EndTime.seconds.ToString("D2")})\n" +
            $"Скорость: {string.Format("{0:0.00}", speed)} м/с\n" +
            $"Глубина: {string.Format("{0:0.00}", this.transform.position.y)} м.\n" +
            $"Запас хода(кабель): {string.Format("{0:0.00}", cableLength - Vector3.Distance(new Vector3(transform.position.x, transform.position.y, transform.position.z), new Vector3(cable.position.x, cable.position.y, cable.position.z)))} м.";
    }
    private void FixedUpdate()
    {
        battery -= chargeUpdate;
        KeyboardController.Instance.boardPower = 1315 * speedMax;
        ClockUpdate();
    }
    public static void moveChargeConsuption(float power)
    {
        Instance.battery -= (Instance.motorChargeConsumption / 60 / 60) * Time.deltaTime * power;
    }
    void ClockUpdate()
    {
        time.miliseconds++;
        if (time.miliseconds == 50) { time.seconds++; time.miliseconds = 0; CalcEndTime(); }
        if (time.seconds == 60) { time.minutes++; time.seconds = 0; }
        if (time.minutes == 60) { time.hours++; time.minutes = 0; }
        if (time.hours == 24) time.hours = 0;
    }
    void CalcEndTime()
    {
        batteryStats.Add(batteryStart - battery);
        batteryStart = battery;

        int stats = (int)(battery / CalcList());
        EndTime.seconds = time.seconds + stats;
        EndTime.minutes = time.minutes;
        EndTime.hours = time.hours;

        while (EndTime.seconds >= 60)
        {
            EndTime.minutes++;
            EndTime.seconds -= 60;
        }

        while (EndTime.minutes >= 60)
        {
            EndTime.hours++;
            EndTime.minutes -= 60;
        }

        while (EndTime.hours >= 24)
            EndTime.hours -= 24;

        if (!Directory.Exists("Stats")) 
        {
            Directory.CreateDirectory("Stats");
            Debug.LogError("Ошибка при запуске - требуется перезапустить программу для корректного сохранения данных симуляции, иначе данные будут не полные!");
        }
        FileAddLine("Stats/EndTime.txt", $"{stats}{Environment.NewLine}");
        FileAddLine("Stats/Battery.txt", $"{battery}{Environment.NewLine}");
        FileAddLine("Stats/Speed.txt", $"{speed}{Environment.NewLine}");
        FileAddLine("Stats/Deep.txt", $"{this.transform.position.y}{Environment.NewLine}");
    }

    void FileAddLine(string path, string text)
    {
        if (!File.Exists(path))
            File.Create(path);
        File.AppendAllText(path, text);
    }

    float CalcList()
    {
        float sum = 0;

        int count = batteryStats.Count - 1;
        int endcount = count - 45;
        if (endcount < 0) endcount = 0;

        for (int i = count; i >= endcount; i--)
        {
            sum += batteryStats[i];
        }
        sum /= (count + 1 - endcount);
        return sum;
    }
}
