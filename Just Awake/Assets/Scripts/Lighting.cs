using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lighting : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] GameObject directionalLight;
    Light lightComponent;
    float directionalLightAngleRotateSpeed = 0.005f;

    void Awake() {
        lightComponent = directionalLight.GetComponent<Light>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float lightIntensity = 2f + 2f/50000f*Player.transform.position.y;
        if (lightIntensity < 0f) lightIntensity = 0f;
        lightComponent.intensity = lightIntensity;
    }

    void FixedUpdate()
    {
        float directionalLightAngleX = directionalLight.transform.eulerAngles.x < 180 ? directionalLight.transform.eulerAngles.x + 360f : directionalLight.transform.eulerAngles.x;
        directionalLightAngleX += directionalLightAngleRotateSpeed;
        if((directionalLightAngleX > 449f) || (directionalLightAngleX < 271f))
            directionalLightAngleRotateSpeed *= -1;
        if(directionalLightAngleRotateSpeed < 0f) directionalLightAngleX = 540 - directionalLightAngleX;
        directionalLight.transform.rotation = Quaternion.Euler(directionalLightAngleX, -30f, 0f);
    }
}
