using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SphereController : MonoBehaviour
{
    public float speed = 5f;
    public Transform fencingTarget;
    public Transform shootingTarget;
    public Transform quitTarget;
    public Transform resumeTarget;
    public Transform mainMenuTarget;

    public Slider Slider;
    public List<ButtonTarget> buttonTargets = new List<ButtonTarget>();

    public class ButtonTarget
    {
        public Transform target;
        public Action action;
        public bool isHovering;
        public float value;

        public ButtonTarget(Transform t, Action a, bool ih)
        {
            target = t;
            action = a;
            isHovering = ih;
            value = 0;
        }
    }

    private void Awake()
    { 
        if (fencingTarget)
        {
            buttonTargets.Add(new ButtonTarget(fencingTarget, () => SceneManager.LoadScene(1), false));
        }

        if (shootingTarget)
        {
            buttonTargets.Add(new ButtonTarget(shootingTarget, () => SceneManager.LoadScene(2), false));
        }

        if (quitTarget)
        {
            buttonTargets.Add(new ButtonTarget(quitTarget, () => Application.Quit(), false));
        }

        if (resumeTarget)
        {
            buttonTargets.Add(new ButtonTarget(resumeTarget, () => GameManager.instance.Continue(), false));
        }

        if (mainMenuTarget)
        {
            buttonTargets.Add(new ButtonTarget(mainMenuTarget, () => SceneManager.LoadScene(0), false));
        }
    }

    public void SetHandPosition(Vector3 pos)
    {
        transform.position = pos;

        bool isNotHoveringAtAll = true;
        foreach (ButtonTarget bt in buttonTargets)
        {
            if (Vector2.Distance(transform.position, bt.target.position) < 1f)
            {
                bt.isHovering = true;
                isNotHoveringAtAll = false;
            }
            else
            {
                bt.isHovering = false;
                bt.value = 0;
            }

            if (bt.isHovering)
            {
                bt.value += Time.unscaledDeltaTime / 1.5f;
                Slider.value = bt.value;

                if (Slider.value >= 1f)
                {
                    bt.action();
                }
            }
            else
            {
                bt.value = 0;
            }
        }

        if (isNotHoveringAtAll) Slider.value = 0;
    }
}


