using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakObstacle : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D other){
        if (other.gameObject.tag == "Boom")
        {
            // TODO: 播放动画
            Destroy(gameObject);
        }
    }
}
