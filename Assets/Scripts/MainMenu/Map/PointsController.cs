using UnityEngine;
using System;

public class PointsController : MonoBehaviour
{
    [SerializeField]
    public Point[] points;

    private string _activetedPoints;

    public void SetNullPointIndex() 
    {
        ScensVar.PointIndex = -1;
    }

    public void SetPointIndex(Point point) 
    {
        for (int i = 0; i < points.Length; i++)
        {
            if(point == points[i]) 
            {
                ScensVar.PointIndex = i;
                return;
            }
        }
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("Time")) 
        {
            DateTime oldTime = DateTime.Parse(PlayerPrefs.GetString("Time"));
            Debug.Log(oldTime);
            if(DateTime.Now.Day != oldTime.Day) 
            {
                PlayerPrefs.DeleteKey("Points");
            }
        }

        PlayerPrefs.SetString("Time", DateTime.Now.ToString());

        if (PlayerPrefs.HasKey("Points")) {
            _activetedPoints = PlayerPrefs.GetString("Points");
        }
        else
        {
            _activetedPoints = "";

            for (int i = 0; i < points.Length; i++)
            {
                _activetedPoints += "0";
            }

            PlayerPrefs.SetString("Points", _activetedPoints);
        }

        PlayerPrefs.Save();

        for (int i = 0; i < points.Length; i++)
        {
            Debug.Log(_activetedPoints[i]);
            if (_activetedPoints[i] == '1')
            {
                points[i].SetPointActiv(false);
            }
        }
    }
}
