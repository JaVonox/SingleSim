using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ServerAudioState
{
    AmbientFan,
    Scanner,
    Decoding
}
public class AudioHandler : MonoBehaviour
{
    public AudioSource serverAmbient;
    public AudioSource serverProcessing;
    public AudioSource serverBeep;

    private float dtTime;

    private static ServerAudioState currentState;
    public static bool needsUpdate = true; //When the update method needs to switch the current audio behaviour

    public static bool beepsEnabled;
    public static void CallNewAudioState()
    {
        if((Gameplay.scannerState == "idle" || Gameplay.scannerState == "finished") && (Gameplay.decoderState == "idle" || Gameplay.decoderState == "finished"))
        {
            currentState = ServerAudioState.AmbientFan;
            needsUpdate = true;
        }
        else if(Gameplay.scannerState == "scanning" && Gameplay.decoderState != "decoding")
        {
            currentState = ServerAudioState.Scanner;
            needsUpdate = true;
        }
        else if(Gameplay.decoderState == "decoding")
        {
            currentState = ServerAudioState.Decoding;
            needsUpdate = true;
        }
    }
    private void UpdateAudio() //Called when the needsupdate is set to true
    {
        switch(currentState)
        {
            case ServerAudioState.AmbientFan:
                serverAmbient.enabled = true;
                serverProcessing.enabled = false;
                serverBeep.enabled = false;
                beepsEnabled = false;
                break;
            case ServerAudioState.Scanner:
                serverAmbient.enabled = true;
                serverProcessing.enabled = true;
                serverBeep.enabled = false;
                beepsEnabled = false;
                break;
            case ServerAudioState.Decoding:
                serverAmbient.enabled = true;
                serverProcessing.enabled = true;
                serverBeep.enabled = true;
                beepsEnabled = true;
                break;
            default:
                Debug.LogError("Invalid audio state");
                break;
        }

        needsUpdate = false;
        dtTime = 0.0f;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (needsUpdate) { UpdateAudio(); } //When an audio source update is requested, force an update

        dtTime += Time.deltaTime;

        if(beepsEnabled)
        {
            if (dtTime > 0.1f)
            {
                if (Random.Range(0.001f, 1.0f) < 0.1f)
                {
                    serverBeep.Play();
                }
                dtTime = 0;
            }
        }
    }
}
