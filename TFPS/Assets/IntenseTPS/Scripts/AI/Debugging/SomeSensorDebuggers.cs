using Sensors;
using UnityEngine;

public class SomeSensorDebuggers : MonoBehaviour
{
    public enum DebugSensorType
    {
        RandPositionAroundFinder,
        CanFireNMPosFinder
    }

    private bool debug = false;
    private ShooterBehaviour sb;
    public DebugSensorType sensorType;

    private void Start()
    {
        sb = GetComponent<ShooterBehaviour>();
    }

    private void Update()
    {
        if (!debug || !sb || sb.ai == null)
            return;
        switch (sensorType)
        {
            case DebugSensorType.RandPositionAroundFinder:
                SensorRandPosAroundGtor sensor = sb.ai.GetSensor<SensorRandPosAroundGtor>();
                if (sensor != null)
                {
                    sensor.RequestAllInfo(transform.position);
                }

                break;

            case DebugSensorType.CanFireNMPosFinder:
                SensorCanFireNMPositionFinder sensor2 = sb.ai.GetSensor<SensorCanFireNMPositionFinder>();
                if (sensor2 != null)
                {
                    sensor2.RequestAllInfo(sb.ai);
                }
                break;

            default:
                break;
        }
    }
}