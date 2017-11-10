#region

using UnityEngine;

#endregion

public class DayNightCycle : MonoBehaviour
{
    private float hourOfDay = 9f;
    public float timeSpeed = 0.6f;

    public float HourOfDay
    {
        get { return transform.rotation.eulerAngles.x / 360f * 24f; }
        set
        {
            Quaternion q = transform.rotation;
            q.eulerAngles = new Vector3(value / 24f * 360f, q.eulerAngles.y, q.eulerAngles.z);
            transform.rotation = q;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(Time.deltaTime * Time.timeScale * timeSpeed, 0f, 0f);
    }
}