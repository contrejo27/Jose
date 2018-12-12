using UnityEngine;
using UnityEngine.UI;

namespace BrandXR
{
    public class UITextField: MonoBehaviour, ICanvasRaycastFilter
    {

        public Text textField;

        //----------------------------------//
        public void Awake()
        //----------------------------------//
        {

            if( textField == null && transform.GetComponent<Text>() != null )
            {
                textField = transform.GetComponent<Text>();
            }

        } //END Awake

        public string text
        {
            get
            {
                if( textField != null ) { return textField.text; }
                else return "";
            }
            set
            {
                if( textField != null ) { textField.text = value; }
            }
        }

        public Color color
        {
            get
            {
                if( textField != null ) { return textField.color; }
                else return Color.white;
            }
            set
            {
                if( textField != null ) { textField.color = value; }
            }
        }

        public bool IsRaycastLocationValid( Vector2 screenPoint, Camera eventCamera )
        {
            return false;
        }

    } //END Class

} //END Namespace