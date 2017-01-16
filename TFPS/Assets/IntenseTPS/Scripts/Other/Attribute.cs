using UnityEngine;

/// <summary>
/// This is used to hold a type with its confidence value
/// </summary>
public struct Attribute<TType>
{
    private TType value;
    private float confidence;

    public void Set(TType _value, float _confidence)
    {
        Value = _value;
        Confidence = _confidence;
    }

    public void Set(Attribute<TType> att)
    {
        Value = att.value;
        Confidence = att.confidence;
    }

    public TType Value
    {
        get
        {
            return this.value;
        }
        private set
        {
            this.value = value;
        }
    }

    public float Confidence
    {
        get
        {
            return this.confidence;
        }
        private set
        {
            value = Mathf.Clamp01(value);
            this.confidence = value;
        }
    }
}