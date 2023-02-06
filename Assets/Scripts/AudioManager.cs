using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
	// Audio players components.
	public static AudioSource audioSource;

	private static AudioClip[] hitClips;
	private static AudioClip[] deflectionSound;
	private static AudioClip swordSwing;
	private static AudioClip shieldHit;
	private static AudioClip gunfire;
	private static AudioClip gunHit;

	// Random pitch adjustment range.
	public static float LowPitchRange = .8f;
	public static float HighPitchRange = 1.2f;
	// Singleton instance.
	public static AudioManager Instance = null;

	// Initialize the singleton instance.
	private void Awake()
	{
		// If there is not already an instance of SoundManager, set it to this.
		if (Instance == null)
		{
			Instance = this;
		}
		//If an instance already exists, destroy whatever this object is to enforce the singleton.
		else if (Instance != this)
		{
			Destroy(gameObject);
		}
		//Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
		DontDestroyOnLoad(gameObject);
	}

	private void Start()
	{
		audioSource = this.GetComponent<AudioSource>();
		hitClips = Resources.LoadAll<AudioClip>("Audio/hitSounds");
		deflectionSound = Resources.LoadAll<AudioClip>("Audio/deflectionSounds");

		gunHit = Resources.Load<AudioClip>("Audio/gunHit");
		swordSwing = Resources.Load<AudioClip>("Audio/swordSwing");
		shieldHit = Resources.Load<AudioClip>("Audio/ShieldHit");
		gunfire = Resources.Load<AudioClip>("Audio/gunfire");

		Debug.Log(hitClips.Length);
		Debug.Log(deflectionSound.Length);
	}
	public static void playSwingSound()
	{
		audioSource.clip = swordSwing;
		audioSource.volume = 0.3f;
		audioSource.Play();
	}

	public static void playShieldHit()
	{
		audioSource.PlayOneShot(shieldHit);
	}

	public static void playGunfire()
	{
		audioSource.PlayOneShot(gunfire);
	}

	public static void playGunHit()
	{
		audioSource.PlayOneShot(gunHit);
	}


	// Play a random clip from an array, and randomize the pitch slightly.
	public static void RandomHitSound()
	{
		int randomIndex = Random.Range(0, hitClips.Length);
		float randomPitch = Random.Range(LowPitchRange, HighPitchRange);
		audioSource.pitch = randomPitch;
		audioSource.clip = hitClips[randomIndex];
		audioSource.Play();
	}

	public static void RandomDeflectSound()
	{
		int randomIndex = Random.Range(0, deflectionSound.Length);
		float randomPitch = Random.Range(LowPitchRange, HighPitchRange);
		audioSource.pitch = randomPitch;
		audioSource.clip = deflectionSound[randomIndex];
		audioSource.Play();
	}

}