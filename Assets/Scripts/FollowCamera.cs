using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class FollowCamera : MonoBehaviour
{
    public GameObject waterHigh;
    public Transform mainCamera;
    public Transform TargetPos;
    public Transform player;
    public float Damping;
    public float aimSpeed;
    public float zoom;
    private float currentZoom;
    private Vector3 point;

    private Vector3 m_CurrentVelocity;
    Vector3 m_DampedPos;
    private float newDamping;

    public Volume UnderwaterVolume;
    private WaterManager waterManager;

    private float waterHeightAtCamera;
    void Start()
    {
        currentZoom = zoom;
        waterManager = GetComponent<WaterManager>();
    }

    void OnEnable()
    {
        if (TargetPos != null)
            m_DampedPos = mainCamera.position;

        newDamping = 0.5f;
    }
    float UnderwaterTween() {
        float diff = mainCamera.position.y - waterHeightAtCamera;
        return -Mathf.Clamp(diff, -10, 0)/10;
    }

    void Update()
    {
        waterHeightAtCamera = waterManager.WaterHeightAtPosition(mainCamera.position);
        currentZoom = zoom;
        newDamping -= Time.deltaTime;
        if (newDamping < Damping + 0.1f)
        {
            newDamping = Damping;
        }
        UnderwaterVolume.GetComponent<Volume>().weight = UnderwaterTween();

        Transform waterTran = waterHigh.GetComponent<Transform>();
        if (waterHeightAtCamera<0) {
            Vector3 ls = waterTran.localScale;
            waterTran.localScale = new Vector3(1,-1,1);
        } else {
            waterTran.localScale = new Vector3(1,1,1);
        }
        if (TargetPos != null)
        {

            //rotation
            Vector3 positionDirection = player.position - TargetPos.position;
            positionDirection.Normalize();
            point = TargetPos.position - (positionDirection * currentZoom);

            Vector3 desiredRot = new Vector3(player.rotation.eulerAngles.x, player.rotation.eulerAngles.y, 0f);
            Quaternion desiredFinal = Quaternion.Euler(desiredRot.x, desiredRot.y, desiredRot.z);

            transform.rotation = Quaternion.Slerp(transform.rotation, desiredFinal, aimSpeed * Time.deltaTime); //looks where player is looking
            //transform.rotation = Quaternion.Slerp(transform.rotation, player.rotation, aimSpeed * Time.deltaTime); // Also turns in z

            //movement
            var pos = point;
            m_DampedPos = Damping < 0.01f
                ? pos : Vector3.SmoothDamp(m_DampedPos, pos, ref m_CurrentVelocity, newDamping);
            pos = m_DampedPos;

            transform.position = pos;
        }
    }
}
