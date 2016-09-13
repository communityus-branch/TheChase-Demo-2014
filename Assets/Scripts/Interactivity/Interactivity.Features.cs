using UnityEngine;
using System.Collections;
using System;

public partial class Interactivity
{
	private FeatureDemonstration[] features = null;
	private FeatureDemonstration currentFeature = null;
	private int featureCounter = 0;	//	zero == null == main demoloop
	private bool isTransitioning = false;

	private void setDescription( string str )
	{
		var go = GameObject.Find( "featureDesc" );
		if( go )
			go.GetComponent<GUIText>().text = str;
		else
			Debug.LogWarning( "Can't find 'featureDesc'" );
	}
	
	private void setFeature( int index )
	{
		if( index == 0 )
			switchToFeature( null, Vector2.zero );
		else
		{
			var f = features[ index-1 ];
			switchToFeature( f, Vector2.zero );
		}
	}

	protected void leftFeature()
	{
		if (!allowFeatureDemonstration)
			return;

		if( isTransitioning == false )
		{
			featureCounter--;
			if( featureCounter < 0 )
				featureCounter = features.Length;

			setFeature( featureCounter );
		}
	}

	protected void rightFeature()
	{
		if (!allowFeatureDemonstration)
			return;

		if( isTransitioning == false )
		{
			featureCounter++;
			if( featureCounter > features.Length )
				featureCounter = 0;

			setFeature( featureCounter );
		}
	}
	
	private void setupSwiper()
	{
		var swipe = GetComponent<Swiper2>() as Swiper2;
		if( swipe == null )
		{
			Debug.LogWarning( "Unable to find Swiper2" );
			return;
		}

		swipe.OnSwipeLeft += () =>	{	leftFeature();	};
		swipe.OnSwipeRight += () =>	{	rightFeature();	};
		//swipe.OnSwipeDrag += (v) =>	{	debugText.text = string.Format( "Drag {0}", v.magnitude );	};
	}

	//	feature.Running will enable/disable cameras, it occurs halfway between the transition(when it's black).
	private void switchToFeature( FeatureDemonstration feature, Vector2 transitionDirection )
	{
		if (!allowFeatureDemonstration && feature != null)
			return;

		if( currentFeature == feature || isTransitioning == true )
			return;

		allowAiming = false;

		//	If there is a current feature active, tell it to hide itself. It has half the transition time to do so.
		if( currentFeature != null )
			currentFeature.Hide( transitionDuration * 0.5f );

		isTransitioning = true;
		GetComponent<CameraFade>().Transition( transitionDuration, 0.5f,
		() =>	//	@0.5
		{
			bool doTransition = false;
			if( currentFeature != null )
			{
				doTransition = true;
				currentFeature.Running = false;
				currentFeature = null;
			}

			if( feature != null )
			{
				BulletTime.allowCameraTriggers = false;

				//	Tell the new feature to show itself. It has half the transition time to do so.
				feature.Running = true;
				feature.Show( transitionDuration * 0.5f, doTransition );
				setDescription( feature.Description );
				currentFeature = feature;
			}
			else
			{
				setDescription( "" );
				BulletTime.allowCameraTriggers = true;
			}
			cinematron.featureMode = currentFeature != null;
		},
		() =>	//	on complete.
		{
			isTransitioning = false;
			allowAiming = currentFeature == null;
		});
	}
	
	private void setupFeatures()
	{
		features = GameObject.FindObjectsOfType( typeof( FeatureDemonstration ) ) as FeatureDemonstration[];
		setupSwiper();
	}
}