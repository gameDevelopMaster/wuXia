using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundEffect : MonoBehaviour 
{
	public static string[] Explosion = new string[]{"explosion"};
	public static string[] Drop = new string[]{"drop"};
	public static string[] Combo = new string[]{"sweet","excellent"};
	public static string[] GameOver = new string[]{"game_over"};
	static SoundEffect _instance;
	float timeOfEffect;
    public static AudioSource audioSource = new AudioSource();
	public static SoundEffect Instance
	{
		get
		{
            if (_instance == null)
            {
                GameObject obj = GameObject.Find("SoundEffect");
                if (obj == null)
                {
                    obj = (GameObject)Instantiate(Resources.Load("Prefabs/SoundEffect"));
					obj.transform.parent = Camera.main.transform;
					obj.transform.localPosition = new Vector3(0,0,0);
                }
                _instance = obj.GetComponent<SoundEffect>();
            }
			_instance.Init();
            return _instance;
        }
	}
	void Init()
	{
		audioSource = gameObject.GetComponent<AudioSource>();
	}
	// Use this for initialization
    //void Start ()
    //{
    //}
	public void PlaySound(string[] soundNames,int index)
	{
		string filePath = "Sounds/"+soundNames[index];
		audioSource.PlayOneShot(Resources.Load(filePath) as AudioClip);
	}
	public void PlaySoundRandom(string[] soundNames)
	{
		int rnd = Random.Range(0,soundNames.Length);
		PlaySound(soundNames,rnd);
	}
	public void Stop()
	{
		//if(audioSource.clip == sound && audioSource.isPlaying)
		audioSource.Stop();
	}
	public static void TurnOnTurnOffSound(bool sound, float volume)
	{
		if(audioSource == null)
			return;
		if(audioSource.isPlaying)
		{
			audioSource.volume = volume/10;
			if(!sound)
				audioSource.Pause();
			
			else
				return;
		}
	}
}
