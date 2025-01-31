using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal; 

public class DayCycle : MonoBehaviour
{
    public enum State { DAY, NIGHT }
    public enum Day { ONE = 1, TWO = 2, THREE = 3, FOUR = 4 }

    [Header("Settings")]
    [SerializeField] private float tickPerSec;
    [SerializeField, Tooltip("In ticks")] private float dayDuration;
    [SerializeField, Tooltip("In ticks")] private float nightDuration;

    [field: Header("Monitor")]
    [field: SerializeField] public State CurrentState { get; private set; }
    [SerializeField] private float stateProgress;
    [field: SerializeField] public Day CurrentDay { get; private set; }

    // Modules
    private SceneManager sceneManager;
    private DayCycleUI UI;

    public delegate void HalfDay();
    public static event HalfDay OnHalfDay;
    private bool halfDayRaised = false;

    public delegate void DayEvent();
    public static event DayEvent OnDay;

    public delegate void NightEvent();
    public static event NightEvent OnNight;

    ////===맹 추가
    [Header("Lighting Settings")]
    [SerializeField] private Light2D globalLight; 
    

    // ===============================================================================================================

    private void Awake()
    {
        CurrentDay = Day.ONE;
        CurrentState = State.DAY;

        sceneManager = FindObjectOfType<SceneManager>();
        UI = FindObjectOfType<DayCycleUI>();
        ////=== 맹추가
        GameObject lightObj = GameObject.Find("Light2D_NightShadow"); //
        if (lightObj != null)
        {
            globalLight = lightObj.GetComponent<Light2D>();
        }
    }

    private void Start()
    {
        StartCoroutine(UpdateRoutine());
    }

    private IEnumerator UpdateRoutine()
    {
        var elapsed = 0;

        while (true)
        {
            if (CurrentState == State.DAY)
            {
                stateProgress = elapsed / dayDuration;

                if (elapsed >= dayDuration * 0.5f && !halfDayRaised)
                {
                    OnHalfDay?.Invoke();
                    halfDayRaised = true;
                }

                if (elapsed >= dayDuration)
                {
                    CurrentState = State.NIGHT;
                    OnNight?.Invoke();

                    elapsed = 0;

                    OnHalfDay?.Invoke();
                    halfDayRaised = false;
                }
            } 
            else
            {
                stateProgress = elapsed / nightDuration;

                if (elapsed >= nightDuration * 0.5f && !halfDayRaised)
                {
                    OnHalfDay?.Invoke();
                    halfDayRaised = true;
                }

                if (elapsed >= nightDuration)
                {
                    if (CurrentDay != Day.FOUR)
                    {
                        CurrentState = State.DAY;
                        OnDay?.Invoke();

                        elapsed = 0;

                        CurrentDay++;
                        UI.UpdateUI((int)CurrentDay);

                        OnHalfDay?.Invoke();
                        halfDayRaised = false;

                        /////
                        sendSignalToLight2D(); // 밤이 시작되면 조명 변경
                    }
                    else
                    {
                        RaiseWin();
                        yield break;
                    }
                }
            }

            elapsed++;
            yield return new WaitForSeconds(1 / tickPerSec);
        }
    }

    private void RaiseWin()
    {
        sceneManager.LoadEnding();
    }

     //// -- 맹 추가 코드 Global light 2D 변경
     public void sendSignalToLight2D()
    {
      //  Debug.Log("시그널보내~");
        if (globalLight != null)
        {
            if (CurrentState == State.DAY)
            {
                globalLight.intensity = 1.0f;  // 낮에는 밝게
                globalLight.color = Color.white; 
            }
            else
            {
                globalLight.intensity =1f;  // 밤에는 어둡게
                 globalLight.color = Color.black; 
                //globalLight.color = new Color(0.1f, 0.1f, 0.2f); // 어두운 파란색
            }
        }
        else
        {
            Debug.LogWarning("Global Light 2D가 설정되지 않았습니다!");
        }
    }

   
    
    
}
