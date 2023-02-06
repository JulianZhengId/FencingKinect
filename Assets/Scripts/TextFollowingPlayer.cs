using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFollowingPlayer : MonoBehaviour
{
    public Transform player;
    [SerializeField] private Vector3 gap = new Vector3(0, 1f, 0);

    // Update is called once per frame
    void Update()
    {
        transform.position = player.position + gap;
    }
}
