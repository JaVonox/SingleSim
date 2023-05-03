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


    public AudioSource outsideAmbientSource;
    public List<AudioClip> commonOutsideAudio;
    public List<AudioClip> rareOutsideAudio;

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

        if (dtTime > 0.1f)
        {
            if (beepsEnabled) //Beep randomness
            {
                if (Random.Range(0.001f, 1.0f) < 0.1f)
                {
                    serverBeep.Play();
                }
            }

            if(outsideAmbientSource.isPlaying == false && Random.Range(0f,1.0f) < 0.001f) //Outside Ambience randomness. default 0.001f
            {
                if(Random.Range(0f,1.0f) < 0.01f) //Check for rare audio cue. default 0.01f
                {
                    outsideAmbientSource.clip = rareOutsideAudio[Random.Range(0, rareOutsideAudio.Count)];
                    outsideAmbientSource.volume = Random.Range(0.1f, 0.5f);
                    outsideAmbientSource.Play();
                }
                else
                {
                    outsideAmbientSource.clip = commonOutsideAudio[Random.Range(0, commonOutsideAudio.Count)];
                    outsideAmbientSource.volume = Random.Range(0.1f, 0.5f);
                    outsideAmbientSource.Play();
                }
            }

            dtTime = 0;
        }

    }
}
