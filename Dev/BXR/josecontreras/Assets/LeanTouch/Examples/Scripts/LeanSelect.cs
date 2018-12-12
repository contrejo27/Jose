using UnityEngine;
using System.Collections.Generic;

namespace Lean.Touch
{
	// This component allows you to select LeanSelectable components
	public class LeanSelect : MonoBehaviour
	{
		public enum SelectType
		{
			Raycast3D,
			Overlap2D,
			CanvasUI
		}

		public enum SearchType
		{
			GetComponent,
			GetComponentInParent,
			GetComponentInChildren
		}

		public enum ReselectType
		{
			KeepSelected,
			Deselect,
			DeselectAndSelect,
			SelectAgain
		}

		public static List<LeanSelect> Instances = new List<LeanSelect>();

		public SelectType SelectUsing;

		[Tooltip("The layers you want the raycast/overlap to hit")]
		public LayerMask LayerMask = Physics.DefaultRaycastLayers;

		[Tooltip("The camera used to calculate the ray (None = MainCamera)")]
		public Camera Camera;

		[Tooltip("The maximum number of selectables that can be selected at the same time (0 = Unlimited)")]
		public int MaxSelectables;

		[Tooltip("How should the candidate GameObjects be searched for the LeanSelectable component?")]
		public SearchType Search = SearchType.GetComponentInParent;

		[Tooltip("If you select an already selected selectable, what should happen?")]
		public ReselectType Reselect;

		[Tooltip("Automatically deselect everything if nothing was selected?")]
		public bool AutoDeselect;

        

        // NOTE: This must be called from somewhere
        public void SelectStartScreenPosition(LeanFinger finger)
		{
			SelectScreenPosition(finger, finger.StartScreenPosition);
		}

		// NOTE: This must be called from somewhere
		public void SelectScreenPosition(LeanFinger finger)
		{
			SelectScreenPosition(finger, finger.ScreenPosition);
		}

		// NOTE: This must be called from somewhere
		public void SelectScreenPosition(LeanFinger finger, Vector2 screenPosition)
		{
            
            // Stores the component we hit (Collider or Collider2D)
            var component = default(Component);
            
            switch (SelectUsing)
			{
				case SelectType.Raycast3D:
				{
					// Make sure the camera exists
					var camera = LeanTouch.GetCamera(Camera, gameObject);

					if (camera != null)
					{
						var ray = camera.ScreenPointToRay(screenPosition);
						var hit = default(RaycastHit);

						if (Physics.Raycast(ray, out hit, float.PositiveInfinity, LayerMask) == true)
						{
                            //Debug.Log( "SelectScreenPosition() finger = " + finger + ", screenPosition = " + screenPosition + ", hit.collider = " + hit.collider.gameObject.name );
                            component = hit.collider;
						}
					}
				}
				break;

				case SelectType.Overlap2D:
				{
					// Make sure the camera exists
					var camera = LeanTouch.GetCamera(Camera, gameObject);

					if (camera != null)
					{
						var point = camera.ScreenToWorldPoint(screenPosition);

						component = Physics2D.OverlapPoint(point, LayerMask);
					}
				}
				break;

				case SelectType.CanvasUI:
				{
					var results = LeanTouch.RaycastGui(screenPosition, LayerMask);

					if (results != null && results.Count > 0)
					{
						component = results[0].gameObject.transform;
					}
				}
				break;
			}

			// Select the component
			Select(finger, component);
		}

		public void Select(LeanFinger finger, Component component)
		{
            // Stores the selectable we will search for
            var selectable = default(LeanSelectable);

			// Was a collider found?
			if (component != null)
			{
                if( Search == SearchType.GetComponent )
                {
                    if( component.GetComponent<LeanSelectable>() != null )
                    {
                        selectable = component.GetComponent<LeanSelectable>();
                        //Debug.Log( "Select() finger = " + finger + ", Found LeanSelectable on gameObject.name = " + selectable.gameObject.name );
                    }
                    else
                    {
                        //Debug.Log( "Select() finger = " + finger + ", LeanSelectable does not exist on gameObject.name = "+ component.gameObject.name );
                        selectable = null;
                    }
                }
                else if( Search == SearchType.GetComponentInParent )
                {
                    if( component.GetComponentInParent<LeanSelectable>() != null )
                    {
                        selectable = component.GetComponentInParent<LeanSelectable>();
                        //Debug.Log( "Select() finger = " + finger + ", Found LeanSelectable on gameObject.name = " + selectable.gameObject.name );
                    }
                    else
                    {
                        //Debug.Log( "Select() finger = " + finger + ", LeanSelectable does not exist on gameObject.name = " + component.gameObject.name );
                        selectable = null;
                    }
                }
                else if( Search == SearchType.GetComponentInChildren )
                {
                    if( component.GetComponentInChildren<LeanSelectable>() != null )
                    {
                        selectable = component.GetComponentInChildren<LeanSelectable>();
                        //Debug.Log( "Select() finger = " + finger + ", Found LeanSelectable on gameObject.name = " + selectable.gameObject.name );
                    }
                    else
                    {
                        //Debug.Log( "Select() finger = " + finger + ", LeanSelectable does not exist on gameObject.name = " + component.gameObject.name );
                        selectable = null;
                    }
                }
			}

			// Select the selectable
			Select(finger, selectable);
		}

		public void Select(LeanFinger finger, LeanSelectable selectable)
		{
            //Debug.Log( "Select() finger = " + finger + ", selectable = " + selectable );

			// Something was selected?
			if (selectable != null && selectable.isActiveAndEnabled == true)
			{
				if (selectable.HideWithFinger == true)
				{
					for (var i = LeanSelectable.Instances.Count - 1; i >= 0; i--)
					{
						var instance = LeanSelectable.Instances[i];

						if (instance.HideWithFinger == true && instance.IsSelected == true)
						{
							return;
						}
					}
				}

				// Did we select a new LeanSelectable?
				if (selectable.IsSelected == false)
				{
					// Deselect some if we have too many
					if (MaxSelectables > 0)
					{
						LeanSelectable.Cull(MaxSelectables - 1);
					}

					// Select
					selectable.Select(finger);
				}
				// Did we reselect the current LeanSelectable?
				else
				{
					switch (Reselect)
					{
						case ReselectType.Deselect:
						{
							selectable.Deselect();
						}
						break;

						case ReselectType.DeselectAndSelect:
						{
							selectable.Deselect();
							selectable.Select(finger);
						}
						break;

						case ReselectType.SelectAgain:
						{
							selectable.Select(finger);
						}
						break;
					}
				}
			}
			// Nothing was selected?
			else
			{
				// Deselect?
				if (AutoDeselect == true)
				{
					DeselectAll();
				}
			}
		}

		[ContextMenu("Deselect All")]
		public void DeselectAll()
		{
			LeanSelectable.DeselectAll();
		}

		protected virtual void OnEnable()
		{
			if (Instances.Count > 0)
			{
				Debug.LogWarning("Your scene already contains a LeanSelect component, using more than once at once may cause selection issues", Instances[0]);
			}

			Instances.Add(this);
		}

		protected virtual void OnDisable()
		{
			Instances.Remove(this);
		}
	}
}