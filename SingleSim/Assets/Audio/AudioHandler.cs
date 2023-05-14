using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ServerAudioState
{
    AmbientFan,
    Scanner,
    Decoding,
    ScanAndDecode,
}
public class AudioHandler : MonoBehaviour
{
    public AudioSource serverAmbient;
    public AudioSource serverProcessing;
    public AudioSource serverScanning;
    public AudioSource serverBeep;
    public AudioSource laptopEmail;

    public AudioSource outsideAmbience;
    public AudioSource outsideNoises;
    public List<AudioClip> commonOutsideAudio;
    public List<AudioClip> rareOutsideAudio;

    private float dtTime;

    private static ServerAudioState currentState;
    public static bool needsUpdate; //When the update method needs to switch the current audio behaviour

    public static bool beepsEnabled;
    public static void CallNewAudioState()
    {
        if((Gameplay.scannerState == "idle" || Gameplay.scannerState == "finished") && (Gameplay.decoderState == "idle" || Gameplay.decoderState == "finished"))
        {
            currentState = ServerAudioState.AmbientFan;
            needsUpdate = true;
        }
        else if(Gameplay.scannerState == "scanning" && Gameplay.decoderState == "decoding")
        {
            currentState = ServerAudioState.ScanAndDecode;
            needsUpdate = true;
        }
        else if(Gameplay.scannerState == "scanning")
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
                serverScanning.enabled = false;
                serverBeep.enabled = false;
                beepsEnabled = false;
                break;
            case ServerAudioState.Scanner:
                serverAmbient.enabled = true;
                serverProcessing.enabled = false;
                serverScanning.enabled = true;
                serverBeep.enabled = false;
                beepsEnabled = false;
                break;
            case ServerAudioState.Decoding:
                serverAmbient.enabled = true;
                serverProcessing.enabled = true;
                serverScanning.enabled = false;
                serverBeep.enabled = true;
                beepsEnabled = true;
                break;
            case ServerAudioState.ScanAndDecode:
                serverAmbient.enabled = true;
                serverProcessing.enabled = true;
                serverScanning.enabled = true;
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
        normalSoundRefs.Clear();
        weirdSoundRefs.Clear();

        normalSoundRefs.Add("crow1", 0);
        normalSoundRefs.Add("crow2", 1);

        weirdSoundRefs.Add("weird1", 0);
        weirdSoundRefs.Add("weird2", 1);

        Setup();
    }

    public static void Setup()
    {
        needsUpdate = true;
        beepsEnabled = false;
        forcePlay = false;
        forcedSound = "";
        forcedVolume = 0f;
    }

    private static bool forcePlay = false;
    private static bool forceEmailPlay = false;
    private static string forcedSound = "";
    private static float forcedVolume = 0f;
    private static Dictionary<string, int> normalSoundRefs = new Dictionary<string, int>();
    private static Dictionary<string, int> weirdSoundRefs = new Dictionary<string, int>();
    public static bool ConsolePlaySound(string soundName, float volume)
    {
        if(normalSoundRefs.ContainsKey(soundName) || weirdSoundRefs.ContainsKey(soundName))
        {
            forcePlay = true;
            forcedSound = soundName;
            forcedVolume = volume;
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void EmailNotification()
    {
        forceEmailPlay = true;
    }
    private void ConsoleSoundPlayer()
    {
        outsideNoises.Stop();
        int soundID = -1;
        bool isNormal = true;

        if (normalSoundRefs.ContainsKey(forcedSound)) { soundID = normalSoundRefs[forcedSound]; isNormal = true; }
        else if (weirdSoundRefs.ContainsKey(forcedSound)) { soundID = weirdSoundRefs[forcedSound]; isNormal = false; }
        else { Debug.LogError("invalid sound requested"); }

        if(isNormal)
        {
            outsideNoises.clip = commonOutsideAudio[soundID];
            outsideNoises.volume = Mathf.Min(forcedVolume * Movement.volume,1.0f);
            outsideNoises.Play();
        }
        else
        {
            outsideNoises.clip = rareOutsideAudio[soundID];
            outsideNoises.volume = Mathf.Min(forcedVolume * Movement.volume,1.0f);
            outsideNoises.Play();
        }

        forcePlay = false;
        forcedSound = "";
        forcedVolume = 0;
    }
    // Update is called once per frame
    void Update()
    {
        if (needsUpdate) { UpdateAudio(); } //When an audio source update is requested, force an update

        if (forcePlay) //console command to force play a sound
        {
            ConsoleSoundPlayer();
        }

        if (forceEmailPlay) //When an email is recieved play the email notification sound
        {
            laptopEmail.Play();
            forceEmailPlay = false;
        }


        dtTime += Time.deltaTime;

        if (dtTime > 0.1f)
        {
            outsideAmbience.volume = Movement.volume * 0.2f;
            serverAmbient.volume = Movement.volume * 0.8f;
            serverProcessing.volume = Movement.volume * 0.8f;
            serverScanning.volume = Movement.volume;
            laptopEmail.volume = Movement.volume * 0.5f;

            if (beepsEnabled) //Beep randomness
            {
                if (Random.Range(0.001f, 1.0f) < 0.1f)
                {
                    serverBeep.volume = Movement.volume * 0.1f;
                    serverBeep.Play();
                }
            }

            if(outsideNoises.isPlaying == false && Random.Range(0, 2000) == 69 && Gameplay.storyState >= 6) //Outside noises randomness. only occurs if the player has left the tutorial
            {
                if(Random.Range(0,200) == 69) //Check for rare audio cue
                {
                    outsideNoises.clip = rareOutsideAudio[Random.Range(0, rareOutsideAudio.Count)];
                    outsideNoises.volume = Mathf.Min(1.0f * Movement.volume,1.0f);
                    outsideNoises.Play();
                }
                else
                {
                    outsideNoises.clip = commonOutsideAudio[Random.Range(0, commonOutsideAudio.Count)];
                    outsideNoises.volume = Mathf.Min(Random.Range(0.1f, 1.0f) * Movement.volume, 1.0f);
                    outsideNoises.Play();
                }
            }

            dtTime = 0;
        }

    }
}
