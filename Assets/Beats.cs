using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;

public class Beats : MonoBehaviour
{
    public static Beats Single { get; private set; }

    private AudioSource _source;
    public float BPM;

    private float dt = float.PositiveInfinity;
    private float lastTimestamp = 0.0f;

    public event Action OnBeet;

    // Start is called before the first frame update
    void Start()
    {       
        dt = 1.0f / BPM * 60.0f; //seconds beteen beets. 
        _source = GetComponent<AudioSource>();

        Single = this;

    }

    // Update is called once per frame
    void Update()
    {
        var currentTime = _source.time;
        if (_source.isPlaying && currentTime >= NextTimestamp()) {
            lastTimestamp = NextTimestamp();
            OnBeet?.Invoke();
        }
    }
    private float NextTimestamp() {
        if (_source.time < lastTimestamp - dt) {
            return 0;
        } else {
            return lastTimestamp + dt;
        }
    }

    //gets a value between 0.0 / 1.0 of position between beets. 0 and 1 is on beat
    public static float GetPulseSaw() {
        if (Single?._source == null) { return 1;}
        return (Single._source.time - Single.lastTimestamp) / Single.dt;
    }

    //gets a value between 0.0 / 1.0 of position between beets. 1 is on beat 0 is upbeat
    public static float GetPulseTriangle() {    
        var value = Single?._source == null ? 1f : Single.GetPulseTriangleInternal();

        //if (value > 1 || value < 0) {
        //    Debug.Log("somthng is wrong");
        //    Debug.Log("BPM: " + Single?.BPM);
        //    Debug.Log("dt: " + Single?.dt);
        //    Debug.Log("lastTimestamp: " + Single?.lastTimestamp);
        //    Debug.Log("souce time: " + Single._source.time);
        //    Debug.Log("timeLeft" + (Single.NextTimestamp() - Single._source.time));
        //    Debug.Log("upBeatTime" + (Single.dt / 2));
        //    Debug.Log("boom value:" + value);
        //    
        //    //throw new Exception("boom value:" + value);
        //}
        return value > 1.0f ? 1.0f: value; // tmp fix , todo  se comment on timeLeft
    }
    private float GetPulseTriangleInternal() {    
        var timeLeft = NextTimestamp() - _source.time; // can be negative if executed before update. todo: cap at 1 or update or something
        var upBeatTime = dt / 2;
        if (timeLeft <= upBeatTime) { // towards upbeat
            return 1 - (timeLeft / upBeatTime);
        } else { // away from upbeat
            return (timeLeft - upBeatTime) / upBeatTime;
        }
    }

}
