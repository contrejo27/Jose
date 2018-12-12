using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BrandXR
{
    public class UITextFieldManager: MonoBehaviour
    {

        public List<UITextField> textFields;

        //----------------------------------//
        public void Awake()
        //----------------------------------//
        {

            if( textFields == null && transform.GetComponent<UITextField>() != null )
            {
                textFields.Add( transform.GetComponent<UITextField>() );
            }

        } //END Awake

        //----------------------------------//
        public void SetText( string text )
        //----------------------------------//
        {

            foreach( UITextField ui in textFields )
            {
                ui.text = text;
            }

        } //END SetText

        //----------------------------------//
        public void SetText( int element, string text )
        //----------------------------------//
        {

            if( textFields.Count > element )
            {
                textFields[ element ].text = text;
            }

        } //END SetText

        //----------------------------------//
        public string GetText( int element )
        //----------------------------------//
        {

            if( textFields.Count > element )
            {
                return textFields[ element ].text;
            }
            else
            {
                return "";
            }

        } //END GetText

        //----------------------------------//
        public Color GetColor( int element )
        //----------------------------------//
        {

            if( textFields.Count > element )
            {
                return textFields[ element ].color;
            }
            else
            {
                return Color.white;
            }

        } //END GetText

    } //END Class

} //END Namespace