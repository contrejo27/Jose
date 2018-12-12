using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class bxrTweenFloat: bxrTween
    {
        
        private List<Type> validTypes = new List<Type>
        { typeof( UnityEngine.Object ) };
        
        [Tooltip( "Any Unity Object" )]
        public UnityEngine.Object uObject;
        
        public enum StartValueType
        {
            UseCurrentValue,
            SetValue
        }
        [Tooltip( "What value should this tween start from?" )]
        public StartValueType startValueType = StartValueType.UseCurrentValue;

        [ShowIf( "startValueType", StartValueType.SetValue )]
        public float startValue = 0f;
        [Tooltip( "What do you want to tween to?" )]
        public float endValue = 0f;

        [Tooltip("Name of the field you would like to tween on the Component/Object")]
        public string fieldName = "";
        private FieldInfo fieldInfo; //Reference to the Field on the Component/Object
        private PropertyInfo propertyInfo; //For referencing variables that are actually calls to methods masquerading as fields

        //--------------------------------------------//
        public override void Start()
        //--------------------------------------------//
        {

            if( playOnStart )
            {
                Play();
            }

        } //END Start


        //--------------------------------------------//
        public override TweenType GetTweenType()
        //--------------------------------------------//
        {

            return TweenType.Position;

        } //END GetTweenType

        //----------------------------//
        public override void Play()
        //----------------------------//
        {

            //If Play() is called from this inherited class, we need to make sure our values are set up before attempting to tween
            Play( uObject, fieldName, GetStartValue(), endValue, easeCurve, length, delay, loop, onCompleteOrLoop );

        } //END Play

        //--------------------------------------------//
        private float GetStartValue()
        //--------------------------------------------//
        {
            //Set the fieldInfo and propertyInfo values to false, we'll be checking their null status later to determine where to get/set the variable from
            fieldInfo = null;
            propertyInfo = null;

            //Make sure the field exists first before returning it!
            FieldInfo info = uObject.GetType().GetField( fieldName );

            if( info != null )
            {
                //We found the field, but make sure we can cast it as the type we need
                var cast = info.GetValue( uObject ) as float?;

                if( cast != null )
                {
                    //if( showDebug ) { Debug.Log( "bxrTweenFloat.cs GetStartValue() Found " + fieldName + ", value = " + (float)cast + " within Object fields" ); }

                    //Store the fieldInfo for the value we located
                    fieldInfo = info;

                    //Depending on user settings, we may want to get the "start" value from the current object, or use the passed in values
                    if( startValueType == StartValueType.UseCurrentValue )
                    {
                        return (float)cast;
                    }
                    else
                    {
                        //else, return set value
                        return this.startValue;
                    }
                    
                }
                else
                {
                    //if( showDebug ) { Debug.Log( "bxrTweenFloat.cs GetStartValue() we found " + fieldName + ", but were unable to cast it to the type needed" ); }
                }
            }
            else
            {
                //if( showDebug ) { Debug.Log( "bxrTweenFloat.cs GetStartValue() unable to find any fields with the fieldName = " + fieldName + ", about to begin searching properties instead" ); }
            }

            //So the field didn't exist as a standard field variable, let's check
            //the properties of this Object to see if we can find it
            PropertyInfo[] pInfos = uObject.GetType().GetProperties();

            foreach( PropertyInfo pInfo in pInfos )
            {
                //Check if the name is the same as the field we're looking for
                if( pInfo.Name == fieldName && pInfo.CanRead && pInfo.CanWrite )
                {
                    //The object 'Looks' correct, but let's cast it as the type we need 
                    //and see if that is successful before declaring mission complete
                    var cast = pInfo.GetValue( uObject, null ) as float?; //Use the '?' at the end of the value type to allow primitives and other 'non-nullables' to also have a 'null' value type

                    if( cast != null )
                    {
                        //if( showDebug ) { Debug.Log( "Found " + fieldName + ", value = " + (float)cast + " within Object properties" ); }
                        propertyInfo = pInfo;

                        //Depending on user settings, we may want to get the "start" value from the current object, or use the passed in values
                        if( startValueType == StartValueType.UseCurrentValue )
                        {
                            return (float)cast;
                        }
                        else
                        {
                            //else, return set value
                            return this.startValue;
                        }
                    }
                    else
                    {
                        //if( showDebug ) { Debug.Log( "Found " + fieldName + ", but failed to cast it into the type we need..." ); }
                    }
                }
                else
                {
                    //if( showDebug ) { Debug.Log( "GetStartValue() searching properties... property.Name( " + pInfo.Name + " ) != fieldName(" + fieldName + ")" ); }
                }
            }

            

            //else, return set value
            return this.startValue;

        } //END GetStartValue



        //--------------------------------------------//
        public bool IsValidComponent<T>( T checkThis )
        //--------------------------------------------//
        {

            foreach( Type type in validTypes )
            {
                //Debug.Log( "checkThis.GetType().Name = " + ( checkThis.GetType().Name ) + ", type.Name = " + type + " ... IsSubclass? = " + checkThis.GetType().IsSubclassOf( type ) );

                if( checkThis.GetType().IsSubclassOf( type ) )
                {
                    return true;
                }
            }

            return false;

        } //END IsValidComponent
        

        //--------------------------------------------//
        private void SetComponentToValue<T>( T Component, string fieldName ) where T : UnityEngine.Object
        //--------------------------------------------//
        {

            this.uObject = Component as UnityEngine.Object;

            //We set the fieldInfo or propertyInfo values in the GetStartValue() function instead of here.
            this.fieldName = fieldName;
            GetStartValue();

            //if( showDebug ) { Debug.Log( "SetComponentToValue() Component = " + Component + ", fieldName = " + fieldName + ", fieldInfo = " + fieldInfo + ", propertyInfo = " + propertyInfo ); }

        } //END SetComponentToValue

        //-------------------------------------------//
        public float GetDefaultStartValue<T>( T component, string fieldName )
        //-------------------------------------------//
        {

            if( IsValidComponent( component ) )
            {
                //if( showDebug ) Debug.Log( "GetDefaultStartValue() IsValid = true" );
                this.fieldName = fieldName;
                return GetStartValue();
            }
            
            return 0f;

        } //END GetDefaultStartValue

        //--------------------------------------------//
        public void Play<T>( T tweenThis, string fieldName, float startValue, float endValue, AnimationCurve easeCurve, float length, float delay, bool loop, UnityEvent onCompleteOrLoop ) where T : UnityEngine.Object
        //--------------------------------------------//
        {

            //Setup the object and its values for the tween
            if( tweenThis != null && IsValidComponent( tweenThis ) )
            {
                SetComponentToValue( tweenThis, fieldName );
                
                this.startValue = startValue;
                this.endValue = endValue;

                Setup( easeCurve, length, delay, loop, onCompleteOrLoop );
                
                //Now that the values are setup, call the Play() from the base class to begin the tween
                base.Play();
            }
            else
            {
                if( showDebug ) { Debug.Log( "bxrTweenFloat.cs Play() Unable to Play Tween, tweenThis variable is null or of the incorrect type" ); }
            }

        } //END Play


        //----------------------------//
        protected override void SetNewValues( float timer )
        //----------------------------//
        {
            

            //Perform tween logic every coroutine update
            if( fieldInfo != null )
            {
                fieldInfo.SetValue( this.uObject, Mathf.Lerp( startValue, endValue, easeCurve.Evaluate( timer / length ) ) );
            }
            else if( propertyInfo != null )
            {
                propertyInfo.SetValue( this.uObject, Mathf.Lerp( startValue, endValue, easeCurve.Evaluate( timer / length ) ), null );
            }

        } //END SetNewValue

        //-----------------------//
        protected override void PrepareForLoop()
        //-----------------------//
        {

            //Flip the start and end values to prepare to loop back to the start
            float temp = endValue;

            endValue = startValue;
            startValue = temp;

        } //END PrepareValuesForLoop

        
        //--------------------------------------------//
        public override UnityEngine.Component GetLinkedComponent()
        //--------------------------------------------//
        {

            if( uObject != null && uObject is Component )
            {
                //Debug.Log( "bxrTweenFloat.cs GetLinkedComponent() found uObject that is component... uObject = " + uObject.name );
                return uObject as UnityEngine.Component;
            }

            return null;

        } //END GetLinkedComponent
        
        //--------------------------------------------//
        public override UnityEngine.Component GetLinkedComponent( string fieldName )
        //--------------------------------------------//
        {

            if( uObject != null && this.fieldName == fieldName && uObject is Component )
            {
                return uObject as UnityEngine.Component;
            }

            return null;

        } //END GetLinkedObject
        

        //--------------------------------------------//
        public override UnityEngine.Object GetLinkedObject()
        //--------------------------------------------//
        {

            if( uObject != null ) { return uObject as UnityEngine.Object; }

            return null;

        } //END GetLinkedObject

        //--------------------------------------------//
        public override UnityEngine.Object GetLinkedObject( string fieldName )
        //--------------------------------------------//
        {

            if( uObject != null && this.fieldName == fieldName )
            {
                return uObject as UnityEngine.Object;
            }

            return null;

        } //END GetLinkedObject




    } //END Class

} //END Namespace