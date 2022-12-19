using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class LinesChanger : MonoBehaviour
{
    private VisualEffect ve;
    [SerializeField] private float lerpedColor1;
    [SerializeField] private float lerpedColor2;
    [SerializeField] private float colorMin = 0.3f;
    [SerializeField] private float colorMax = 1.3f;
    [SerializeField] private float colorChangeSpeed = 0.1f;
    public float m_colorTemp;

    // Start is called before the first frame update
    void Start()
    {
        ve = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        lerpedColor1 = Mathf.Lerp(colorMin, colorMax, Mathf.PingPong(Time.time * colorChangeSpeed, m_colorTemp));
        //lerpedColor2 = Mathf.Lerp(colorMax, colorMin, Mathf.PingPong(Time.time * colorChangeSpeed, colorMax));

        if (lerpedColor1 >= m_colorTemp)
        {
            m_colorTemp = colorMin;
            colorMin = colorMax;
            colorMax = m_colorTemp;
        }
        
        ve.SetFloat("Color1", lerpedColor1);
        //ve.SetFloat("Color2", lerpedColor2);
    }
}