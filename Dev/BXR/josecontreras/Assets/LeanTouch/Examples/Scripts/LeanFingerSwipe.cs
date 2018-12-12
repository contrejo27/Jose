using UnityEngine;
using UnityEngine.Events;

namespace Lean.Touch
{
	// This script fires events if a finger has been held for a certain amount of time without moving
	public class LeanFingerSwipe : MonoBehaviour
	{
		public enum ClampType
		{
			None,
			Normalize,
			Direction4,
			ScaledDelta
		}

		// Event signature
		[System.Serializable] public class FingerEvent : UnityEvent<LeanFinger> {}
		[System.Serializable] public class Vector2Event : UnityEvent<Vector2> {}

		[Tooltip("Ignore fingers with StartedOverGui?")]
		public bool IgnoreStartedOverGui = true;

		[Tooltip("Ignore fingers with IsOverGui?")]
		public bool IgnoreIsOverGui;

		[Tooltip("Do nothing if this LeanSelectable isn't selected?")]
		public LeanSelectable RequiredSelectable;

		[Tooltip("Must the swipe be in a specific direction?")]
		public bool CheckAngle;

		[Tooltip("The required angle of the swipe in degrees, where 0 is up, and 90 is right")]
		public float Angle;

		[Tooltip("The left/right tolerance of the swipe angle in degrees")]
		public float AngleThreshold = 90.0f;

		[Tooltip("Should the swipe delta be modified before use?")]
		public ClampType Clamp;

		[Tooltip("The swipe delta multiplier, useful if you're using a Clamp mode")]
		public float Multiplier = 1.0f;

		// Called on the first frame the conditions are met
		public FingerEvent OnSwipe;

		public Vector2Event OnSwipeDelta;

        public bool enableAtStart = false;

#if UNITY_EDITOR
        /*
		protected virtual void Reset()
		{
			Start();
		}
        */
#endif

		protected bool CheckSwipe(LeanFinger finger, Vector2 swipeDelta)
		{
            //Debug.Log( "swipeDelta = " + swipeDelta );

            //Check if the screen position hasn't moved much, if it hasn't, cancel the swipe
            if( Mathf.Abs( swipeDelta.x ) < 5f && Mathf.Abs( swipeDelta.y ) < 5f )
            {
                //Debug.Log( "Swipe Canceled... ScreenPosition = " + finger.ScreenPosition + ", LastScreenPosition = " + finger.LastScreenPosition );
                return false;
            }

            //Print out the snapshots of the finger movement that occured before the drag ended
            /*
            if( finger.Snapshots != null && finger.Snapshots.Count > 0 )
            {
                for( int i = 0; i < finger.Snapshots.Count; i++ )
                {
                    Debug.Log( "Snapshot[" + i + "] Age =" + finger.Snapshots[i].Age + ", pos = " + finger.Snapshots[i].ScreenPosition );
                }
            }
            */

			// Invalid angle?
			if (CheckAngle == true)
			{
				var angle = Mathf.Atan2(swipeDelta.x, swipeDelta.y) * Mathf.Rad2Deg;
				var delta = Mathf.DeltaAngle(angle, Angle);

				if (delta < AngleThreshold * -0.5f || delta >= AngleThreshold * 0.5f)
				{
					return false;
				}
			}

			// Clamp delta?
			switch (Clamp)
			{
				case ClampType.Normalize:
				{
					swipeDelta = swipeDelta.normalized;
				}
				break;

				case ClampType.Direction4:
				{
					if (swipeDelta.x < -Mathf.Abs(swipeDelta.y)) swipeDelta = -Vector2.right;
					if (swipeDelta.x >  Mathf.Abs(swipeDelta.y)) swipeDelta =  Vector2.right;
					if (swipeDelta.y < -Mathf.Abs(swipeDelta.x)) swipeDelta = -Vector2.up;
					if (swipeDelta.y >  Mathf.Abs(swipeDelta.x)) swipeDelta =  Vector2.up;
				}
				break;

				case ClampType.ScaledDelta:
				{
					swipeDelta *= LeanTouch.ScalingFactor;
				}
				break;
			}

			// Call event
			if (OnSwipe != null)
			{
				OnSwipe.Invoke(finger);
			}

			if (OnSwipeDelta != null)
			{
                //Debug.Log( "LeanFingerSwipe.cs CheckSwipe() OnSwipeDelta.Invoke( " + ( swipeDelta * Multiplier ) + " )" );
				OnSwipeDelta.Invoke(swipeDelta * Multiplier);
			}
            else
            {
                //Debug.Log( "LeanFingerSwipe.cs CheckSwipe() OnSwipDelta has no listeners" );
            }

			return true;
		}

		protected virtual void OnEnable()
		{
            // Hook events
            if( enableAtStart )
            LeanTouch.OnFingerSwipe += FingerSwipe;
		}

        /*
		protected virtual void Start()
		{
			if (RequiredSelectable == null)
			{
				RequiredSelectable = GetComponent<LeanSelectable>();
			}
		}
        */

		protected virtual void OnDisable()
		{
			// Unhook events
			if( enableAtStart ) LeanTouch.OnFingerSwipe -= FingerSwipe;
		}

		public void FingerSwipe(LeanFinger finger)
		{

			// Ignore?
			if (IgnoreStartedOverGui == true && finger.StartedOverGui == true)
			{
				return;
			}

			if (IgnoreIsOverGui == true && finger.IsOverGui == true)
			{
				return;
			}

			if (RequiredSelectable != null && RequiredSelectable.IsSelected == false)
			{
                //Debug.Log( "LeanFingerSwipe.cs FingerSwipe() cancelling swipe... RequiredSelectable.IsSelected = " + RequiredSelectable.IsSelected );
                return;
			}
            else
            {
                //Debug.Log( "LeanFingerSwipe.cs FingerSwipe() about to call CheckSwipe()" );
            }

            // Perform final swipe check and fire event
            //CheckSwipe(finger, finger.SwipeScreenDelta); //Original line
            CheckSwipe( finger, finger.GetSnapshotScreenDelta( .25f ) );
		}
	}
}