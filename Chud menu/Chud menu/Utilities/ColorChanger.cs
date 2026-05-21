using UnityEngine;

namespace MalachiTemp.Utilities
{
    public class TimedBehaviour : MonoBehaviour
    {
        public virtual void Start()
        {
            startTime = Time.time;
        }

        public virtual void Update()
        {
            if (!complete)
            {
                progress = Mathf.Clamp((Time.time - startTime) / duration, 0f, 1f);
                if (Time.time - startTime > duration)
                {
                    if (loop)
                        OnLoop();
                    else
                        complete = true;
                }
            }
        }

        public virtual void OnLoop()
        {
            startTime = Time.time;
        }

        public bool complete = false;
        public bool loop = true;
        public float progress = 0f;
        protected bool paused = false;
        protected float startTime;
        public float duration = 2f;
    }

    public class ColorChanger : TimedBehaviour
    {
        public override void Start()
        {
            base.Start();
            if (GetComponent<Renderer>() != null)
                gameObjectRenderer = GetComponent<Renderer>();
        }

        public override void Update()
        {
            base.Update();
            if (colors != null)
            {
                if (timeBased)
                    color = colors.Evaluate(progress);
                gameObjectRenderer.material.color = color;
                gameObjectRenderer.material.SetColor("_EmissionColor", color);
            }
        }

        public Renderer gameObjectRenderer;
        public Gradient colors = null;
        public Color color;
        public bool timeBased = true;
    }
}
