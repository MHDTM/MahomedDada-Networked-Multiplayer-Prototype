using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CameraFollowLocal : MonoBehaviour
{
    public float smoothTime = 0.2f;
    private Vector3 velocity;
    private Transform target;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        TryFindLocalPlayer();
    }

    void TryFindLocalPlayer()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null)
        {
            var p = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (p != null) target = p.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) TryFindLocalPlayer();
        if (target == null) return;

        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, -10f);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}