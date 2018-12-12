using UnityEngine;
using UnityEngine.Events;

public class UISelectEventListener : MonoBehaviour
{
    /*
    public UnityEvent OnLookAt;
    public UnityEvent OnLookAway;
    public UnityEvent OnSelect;

    //----------------------------------//
    void OnEnable()
    //----------------------------------//
    {
        Select.OnLookAt += CallOnLookAt;      //Hover Over
        Select.OnLookAway += CallOnLookAway;  //Hover Exit
        Select.OnSelectPass += CallOnSelect;  //When the screen is tapped while hovering over this Selectable object

        //When the radial UI (aka hover selector) fills up all the way
        GameManager.instance.hoverManager.hoverSelector.OnSelectEvent += CallOnSelect;

    } //END OnEnable

    //----------------------------------//
    void OnDisable()
    //----------------------------------//
    {
        Select.OnLookAt -= CallOnLookAt;
        Select.OnLookAway -= CallOnLookAway;
        Select.OnSelectPass -= CallOnSelect;

        if( GameManager.instance != null && GameManager.instance.hoverManager != null && GameManager.instance.hoverManager.hoverSelector != null )
        {
            GameManager.instance.hoverManager.hoverSelector.OnSelectEvent -= CallOnSelect;
        }

    } //END OnDisable



    //----------------------------------//
    void CallOnLookAt( HoverSelector.SelectionEvents SelectionEvent )
    //----------------------------------//
    {
        CallOnLookAt();

    } //END CallOnLookAt

    //----------------------------------//
    void CallOnLookAt()
    //----------------------------------//
    {
        //Debug.Log( "CallOnLookAt( " + ( Select.instance.GetLastHit().transform == this.transform ).ToString() + " )" );
        if( Select.instance.GetLastHit().transform == this.transform )
        {
            //Turn off the cursor
            Select.instance.cursor.SetActive( false );

            if( OnLookAt != null )
            {
                OnLookAt.Invoke();
            }
        }

    } //END CallOnLookAt



    //----------------------------------//
    void CallOnLookAway( HoverSelector.SelectionEvents SelectionEvent )
    //----------------------------------//
    {
        CallOnLookAway();

    } //END CallOnLookAway

    //----------------------------------//
    void CallOnLookAway()
    //----------------------------------//
    {
        if( Select.instance.GetLastHit().transform == this.transform )
        {
            //Turn off the cursor
            Select.instance.cursor.SetActive( false );

            if( OnLookAway != null )
            {
                OnLookAway.Invoke();
            }
        }

    } //END CallOnLookAway



    //----------------------------------//
    void CallOnSelect( HoverSelector.SelectionEvents SelectionEvent )
    //----------------------------------//
    {
        CallOnSelect();

    } //END CallOnSelect

    //----------------------------------//
    void CallOnSelect()
    //----------------------------------//
    {
        if( Select.instance.GetLastHit().transform == this.transform )
        {
            //Turn off the cursor
            Select.instance.cursor.SetActive( false );

            if( OnSelect != null )
            {
                OnSelect.Invoke();
            }
        }

    } //END CallOnSelect
    */


} //END Class