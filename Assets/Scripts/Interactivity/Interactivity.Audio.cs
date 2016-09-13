using UnityEngine;
using System.Collections;
using System;

public partial class Interactivity
{
	public float musicSpeedInFramesPerSecond { get { return cinematron? cinematron.musicSpeedInFramesPerSecond: BulletTime.frameRate; } }

	public AudioSource[] allAudios = null;	
	private void setupAudio()
	{
		allAudios = GameObject.FindObjectsOfType( typeof( AudioSource ) ) as AudioSource[];

		foreach( var a in allAudios )
		{
			if( a )
				a.pitch = cinematron.playbackSpeedInFramesPerSecond / cinematron.musicSpeedInFramesPerSecond;
		}
	}
	
	public void resyncMusic()
	{
		if( cinematron != null )
			cinematron.resyncMusic();
	}
	
	private bool interleaver = true;
	private void updateAudio()
	{
		if( interleaver == true && scrubbing == false )
			resyncMusic();

		interleaver = !interleaver;
	}
}