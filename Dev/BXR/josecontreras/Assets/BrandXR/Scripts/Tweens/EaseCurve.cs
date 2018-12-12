/**
 * TweenHelper
 * Adapted from David Darren's "PennerDoubleAnimation" project available on Github
 * which code'rized many famous ease equations from Robert Penner, Lee Brimelow, and Zeh Fernando
 * Notice from original GitHub release below...
 * 
 * Animates the value of a double property between two target values using 
 * Robert Penner's easing equations for interpolation over a specified Duration.
 *
 * @author		Darren David darren-code@lookorfeel.com
 * @version		1.0
 *
 * Credit/Thanks:
 * Robert Penner - The easing equations we all know and love 
 *   (http://robertpenner.com/easing/) [See License.txt for license info]
 * 
 * Lee Brimelow - initial port of Penner's equations to WPF 
 *   (http://thewpfblog.com/?p=12)
 * 
 * Zeh Fernando - additional equations (out/in) from 
 *   caurina.transitions.Tweener (http://code.google.com/p/tweener/)
 *   [See License.txt for license info]
 */

using UnityEngine;
using System;

namespace BrandXR
{
    public class EaseCurve : MonoBehaviour
    {
        private static bool curvesCreated = false;

        //Singleton behavior
        private static EaseCurve _instance;

        //--------------------------------------------//
        public static EaseCurve instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<EaseCurve>() == null ) { PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_EaseCurve ); }
                    _instance = GameObject.FindObjectOfType<EaseCurve>();

                    _instance.CreateCurves();
                }

                return _instance;
            }

        } //END Instance

        public enum EaseType
        {
            Linear,
            EaseInOut,
            ExpoEaseOut,
            ExpoEaseIn,
            ExpoEaseInOut,
            ExpoEaseOutIn,
            CircEaseOut,
            CircEaseIn,
            CircEaseInOut,
            CircEaseOutIn,
            QuadEaseOut,
            QuadEaseIn,
            QuadEaseInOut,
            QuadEaseOutIn,
            SineEaseOut,
            SineEaseIn,
            SineEaseInOut,
            SineEaseOutIn,
            CubicEaseOut,
            CubicEaseIn,
            CubicEaseInOut,
            CubicEaseOutIn,
            QuartEaseOut,
            QuartEaseIn,
            QuartEaseInOut,
            QuartEaseOutIn,
            QuintEaseOut,
            QuintEaseIn,
            QuintEaseInOut,
            QuintEaseOutIn,
            ElasticEaseOut,
            ElasticEaseIn,
            ElasticEaseInOut,
            ElasticEaseOutIn,
            BounceEaseOut,
            BounceEaseIn,
            BounceEaseInOut,
            BounceEaseOutIn,
            BackEaseOut,
            BackEaseIn,
            BackEaseInOut,
            BackEaseOutIn
        }

        public static AnimationCurve Linear; //get { return GetEaseCurve( EaseType.Linear ); }
        public static AnimationCurve EaseInOut;
        public static AnimationCurve ExpoEaseOut; //{ get { return GetEaseCurve( EaseType.ExpoEaseOut ); } }
        public static AnimationCurve ExpoEaseIn; //{ get { return GetEaseCurve( EaseType.ExpoEaseIn ); } }
        public static AnimationCurve ExpoEaseInOut; //{ get { return GetEaseCurve( EaseType.ExpoEaseInOut ); } }
        public static AnimationCurve ExpoEaseOutIn; // { get { return GetEaseCurve( EaseType.ExpoEaseOutIn ); } }
        public static AnimationCurve CircEaseOut; // { get { return GetEaseCurve( EaseType.CircEaseOut ); } }
        public static AnimationCurve CircEaseIn; // { get { return GetEaseCurve( EaseType.CircEaseIn ); } }
        public static AnimationCurve CircEaseInOut; // { get { return GetEaseCurve( EaseType.CircEaseInOut ); } }
        public static AnimationCurve CircEaseOutIn; // { get { return GetEaseCurve( EaseType.CircEaseOutIn ); } }
        public static AnimationCurve QuadEaseOut; // { get { return GetEaseCurve( EaseType.QuadEaseOut ); } }
        public static AnimationCurve QuadEaseIn; // { get { return GetEaseCurve( EaseType.QuadEaseIn ); } }
        public static AnimationCurve QuadEaseInOut; // { get { return GetEaseCurve( EaseType.QuadEaseInOut ); } }
        public static AnimationCurve QuadEaseOutIn; // { get { return GetEaseCurve( EaseType.QuadEaseOutIn ); } }
        public static AnimationCurve SineEaseOut; // { get { return GetEaseCurve( EaseType.SineEaseOut ); } }
        public static AnimationCurve SineEaseIn; // { get { return GetEaseCurve( EaseType.SineEaseIn ); } }
        public static AnimationCurve SineEaseInOut; // { get { return GetEaseCurve( EaseType.SineEaseInOut ); } }
        public static AnimationCurve SineEaseOutIn; // { get { return GetEaseCurve( EaseType.SineEaseOutIn ); } }
        public static AnimationCurve CubicEaseOut; // { get { return GetEaseCurve( EaseType.CubicEaseOut ); } }
        public static AnimationCurve CubicEaseIn; // { get { return GetEaseCurve( EaseType.CubicEaseIn ); } }
        public static AnimationCurve CubicEaseInOut; // { get { return GetEaseCurve( EaseType.CubicEaseInOut ); } }
        public static AnimationCurve CubicEaseOutIn; // { get { return GetEaseCurve( EaseType.CubicEaseOutIn ); } }
        public static AnimationCurve QuartEaseOut; // { get { return GetEaseCurve( EaseType.QuartEaseOut ); } }
        public static AnimationCurve QuartEaseIn; // { get { return GetEaseCurve( EaseType.QuartEaseIn ); } }
        public static AnimationCurve QuartEaseInOut; // { get { return GetEaseCurve( EaseType.QuartEaseInOut ); } }
        public static AnimationCurve QuartEaseOutIn; // { get { return GetEaseCurve( EaseType.QuartEaseOutIn ); } }
        public static AnimationCurve QuintEaseOut; // { get { return GetEaseCurve( EaseType.QuintEaseOut ); } }
        public static AnimationCurve QuintEaseIn; // { get { return GetEaseCurve( EaseType.QuintEaseIn ); } }
        public static AnimationCurve QuintEaseInOut; // { get { return GetEaseCurve( EaseType.QuintEaseInOut ); } }
        public static AnimationCurve QuintEaseOutIn; // { get { return GetEaseCurve( EaseType.QuintEaseOutIn ); } }
        public static AnimationCurve ElasticEaseOut; // { get { return GetEaseCurve( EaseType.ElasticEaseOut ); } }
        public static AnimationCurve ElasticEaseIn; // { get { return GetEaseCurve( EaseType.ElasticEaseIn ); } }
        public static AnimationCurve ElasticEaseInOut; // { get { return GetEaseCurve( EaseType.ElasticEaseInOut ); } }
        public static AnimationCurve ElasticEaseOutIn; // { get { return GetEaseCurve( EaseType.ElasticEaseOutIn ); } }
        public static AnimationCurve BounceEaseOut; // { get { return GetEaseCurve( EaseType.BounceEaseOutIn ); } }
        public static AnimationCurve BounceEaseIn; // { get { return GetEaseCurve( EaseType.BounceEaseIn ); } }
        public static AnimationCurve BounceEaseInOut; // { get { return GetEaseCurve( EaseType.BounceEaseInOut ); } }
        public static AnimationCurve BounceEaseOutIn; // { get { return GetEaseCurve( EaseType.BounceEaseOutIn ); } }
        public static AnimationCurve BackEaseOut; // { get { return GetEaseCurve( EaseType.BackEaseOut ); } }
        public static AnimationCurve BackEaseIn; // { get { return GetEaseCurve( EaseType.BackEaseIn ); } }
        public static AnimationCurve BackEaseInOut; // { get { return GetEaseCurve( EaseType.BackEaseInOut ); } }
        public static AnimationCurve BackEaseOutIn; // { get { return GetEaseCurve( EaseType.BackEaseOutIn ); } }

        //--------------------------------------------------------------//
        public void Awake()
        //--------------------------------------------------------------//
        {
            CreateCurves();
            
        } //END Awake

        //----------------------------------------------------//
        private void CreateCurves()
        //----------------------------------------------------//
        {
            if( !curvesCreated )
            {
                curvesCreated = true;

                Linear = AnimationCurve.Linear( 0f, 0f, 1f, 1f );
                EaseInOut = AnimationCurve.EaseInOut( 0f, 0f, 1f, 1f );
                ExpoEaseOut = _GetEaseCurve( EaseType.ExpoEaseOut );
                ExpoEaseIn = _GetEaseCurve( EaseType.ExpoEaseIn );
                ExpoEaseInOut = _GetEaseCurve( EaseType.ExpoEaseInOut );
                ExpoEaseOutIn = _GetEaseCurve( EaseType.ExpoEaseOutIn );
                CircEaseOut = _GetEaseCurve( EaseType.CircEaseOut );
                CircEaseIn = _GetEaseCurve( EaseType.CircEaseIn );
                CircEaseInOut = _GetEaseCurve( EaseType.CircEaseInOut );
                CircEaseOutIn = _GetEaseCurve( EaseType.CircEaseOutIn );
                QuadEaseOut = _GetEaseCurve( EaseType.QuadEaseOut );
                QuadEaseIn = _GetEaseCurve( EaseType.QuadEaseIn );
                QuadEaseInOut = _GetEaseCurve( EaseType.QuadEaseInOut );
                QuadEaseOutIn = _GetEaseCurve( EaseType.QuadEaseOutIn );
                SineEaseOut = _GetEaseCurve( EaseType.SineEaseOut );
                SineEaseIn = _GetEaseCurve( EaseType.SineEaseIn );
                SineEaseInOut = _GetEaseCurve( EaseType.SineEaseInOut );
                SineEaseOutIn = _GetEaseCurve( EaseType.SineEaseOutIn );
                CubicEaseOut = _GetEaseCurve( EaseType.CubicEaseOut );
                CubicEaseIn = _GetEaseCurve( EaseType.CubicEaseIn );
                CubicEaseInOut = _GetEaseCurve( EaseType.CubicEaseInOut );
                CubicEaseOutIn = _GetEaseCurve( EaseType.CubicEaseOutIn );
                QuartEaseOut = _GetEaseCurve( EaseType.QuartEaseOut );
                QuartEaseIn = _GetEaseCurve( EaseType.QuartEaseIn );
                QuartEaseInOut = _GetEaseCurve( EaseType.QuartEaseInOut );
                QuartEaseOutIn = _GetEaseCurve( EaseType.QuartEaseOutIn );
                QuintEaseOut = _GetEaseCurve( EaseType.QuintEaseOut );
                QuintEaseIn = _GetEaseCurve( EaseType.QuintEaseIn );
                QuintEaseInOut = _GetEaseCurve( EaseType.QuintEaseInOut );
                QuintEaseOutIn = _GetEaseCurve( EaseType.QuintEaseOutIn );
                ElasticEaseOut = _GetEaseCurve( EaseType.ElasticEaseOut );
                ElasticEaseIn = _GetEaseCurve( EaseType.ElasticEaseIn );
                ElasticEaseInOut = _GetEaseCurve( EaseType.ElasticEaseInOut );
                ElasticEaseOutIn = _GetEaseCurve( EaseType.ElasticEaseOutIn );
                BounceEaseOut = _GetEaseCurve( EaseType.BounceEaseOut );
                BounceEaseIn = _GetEaseCurve( EaseType.BounceEaseIn );
                BounceEaseInOut = _GetEaseCurve( EaseType.BounceEaseInOut );
                BounceEaseOutIn = _GetEaseCurve( EaseType.BounceEaseOutIn );
                BackEaseOut = _GetEaseCurve( EaseType.BackEaseOut );
                BackEaseIn = _GetEaseCurve( EaseType.BackEaseIn );
                BackEaseInOut = _GetEaseCurve( EaseType.BackEaseInOut );
                BackEaseOutIn = _GetEaseCurve( EaseType.BackEaseOutIn );
            }

        } //END CreateCurves

        //----------------------------------------------------//
        public static AnimationCurve GetEaseCurve( EaseType type )
        //----------------------------------------------------//
        {
            //Make sure the easing curves exist to be referenced
            instance.CreateCurves();

            if( type == EaseType.Linear ) { return Linear; }

            else if( type == EaseType.SineEaseIn ) { return SineEaseIn; }
            else if( type == EaseType.QuadEaseIn ) { return QuadEaseIn; }
            else if( type == EaseType.CubicEaseIn ) { return CubicEaseIn; }
            else if( type == EaseType.QuartEaseIn ) { return QuartEaseIn; }
            else if( type == EaseType.QuintEaseIn ) { return QuintEaseIn; }
            else if( type == EaseType.ExpoEaseIn ) { return ExpoEaseIn; }
            else if( type == EaseType.CircEaseIn ) { return CircEaseIn; }
            else if( type == EaseType.BackEaseIn ) { return BackEaseIn; }
            else if( type == EaseType.ElasticEaseIn ) { return ElasticEaseIn; }
            else if( type == EaseType.BounceEaseIn ) { return BounceEaseIn; }

            else if( type == EaseType.SineEaseOut ) { return SineEaseOut; }
            else if( type == EaseType.QuadEaseOut ) { return QuadEaseOut; }
            else if( type == EaseType.CubicEaseOut ) { return CubicEaseOut; }
            else if( type == EaseType.QuartEaseOut ) { return QuartEaseOut; }
            else if( type == EaseType.QuintEaseOut ) { return QuintEaseOut; }
            else if( type == EaseType.ExpoEaseOut ) { return ExpoEaseOut; }
            else if( type == EaseType.CircEaseOut ) { return CircEaseOut; }
            else if( type == EaseType.BackEaseOut ) { return BackEaseOut; }
            else if( type == EaseType.ElasticEaseOut ) { return ElasticEaseOut; }
            else if( type == EaseType.BounceEaseOut ) { return BounceEaseOut; }

            else if( type == EaseType.SineEaseInOut ) { return SineEaseInOut; }
            else if( type == EaseType.QuadEaseInOut ) { return QuadEaseInOut; }
            else if( type == EaseType.CubicEaseInOut ) { return CubicEaseInOut; }
            else if( type == EaseType.QuartEaseInOut ) { return QuartEaseInOut; }
            else if( type == EaseType.QuintEaseInOut ) { return QuintEaseInOut; }
            else if( type == EaseType.ExpoEaseInOut ) { return ExpoEaseInOut; }
            else if( type == EaseType.CircEaseInOut ) { return CircEaseInOut; }
            else if( type == EaseType.BackEaseInOut ) { return BackEaseInOut; }
            else if( type == EaseType.ElasticEaseInOut ) { return ElasticEaseInOut; }
            else if( type == EaseType.BounceEaseInOut ) { return BounceEaseInOut; }

            else if( type == EaseType.SineEaseOutIn ) { return SineEaseOutIn; }
            else if( type == EaseType.QuadEaseOutIn ) { return QuadEaseOutIn; }
            else if( type == EaseType.CubicEaseOutIn ) { return CubicEaseOutIn; }
            else if( type == EaseType.QuartEaseOutIn ) { return QuartEaseOutIn; }
            else if( type == EaseType.QuintEaseOutIn ) { return QuintEaseOutIn; }
            else if( type == EaseType.ExpoEaseOutIn ) { return ExpoEaseOutIn; }
            else if( type == EaseType.CircEaseOutIn ) { return CircEaseOutIn; }
            else if( type == EaseType.BackEaseOutIn ) { return BackEaseOutIn; }
            else if( type == EaseType.ElasticEaseOutIn ) { return ElasticEaseOutIn; }
            else if( type == EaseType.BounceEaseOutIn ) { return BounceEaseOutIn; }

            return Linear;

        } //END GetEaseCurve

        //--------------------------------------------------------------//
        private static AnimationCurve _GetEaseCurve( EaseType type )
        //--------------------------------------------------------------//
        {
            
            var curve = new AnimationCurve();

            int resolution = GetResolution( type );
            
            for( var i = 0; i < resolution; ++i )
            {
                var time = i / ( resolution - 1f );
                var value = (float)GetEquation( type, time );
                var key = new Keyframe( time, value );

                //Debug.Log( "GetEaseCurve() type = " + type + ", resolution = " + resolution + ", time = " + time + ", value = " + value + ", key = " + key );
                curve.AddKey( key );
            }
            
            for( var i = 0; i < resolution; ++i )
            {
                curve.SmoothTangents( i, 0f );
            }

            return curve;

        } //END GetEaseCurve

        //----------------------------------------------------//
        private static int GetResolution( EaseType type )
        //----------------------------------------------------//
        {

            if( type == EaseType.Linear ) { return 2; }

            else if( type == EaseType.SineEaseIn ) { return 15; }
            else if( type == EaseType.QuadEaseIn ) { return 15; }
            else if( type == EaseType.CubicEaseIn ) { return 15; }
            else if( type == EaseType.QuartEaseIn ) { return 15; }
            else if( type == EaseType.QuintEaseIn ) { return 15; }
            else if( type == EaseType.ExpoEaseIn ) { return 15; }
            else if( type == EaseType.CircEaseIn ) { return 15; }
            else if( type == EaseType.BackEaseIn ) { return 30; }
            else if( type == EaseType.ElasticEaseIn ) { return 30; }
            else if( type == EaseType.BounceEaseIn ) { return 30; }

            else if( type == EaseType.SineEaseOut ) { return 15; }
            else if( type == EaseType.QuadEaseOut ) { return 15; }
            else if( type == EaseType.CubicEaseOut ) { return 15; }
            else if( type == EaseType.QuartEaseOut ) { return 15; }
            else if( type == EaseType.QuintEaseOut ) { return 15; }
            else if( type == EaseType.ExpoEaseOut ) { return 15; }
            else if( type == EaseType.CircEaseOut ) { return 15; }
            else if( type == EaseType.BackEaseOut ) { return 30; }
            else if( type == EaseType.ElasticEaseOut ) { return 30; }
            else if( type == EaseType.BounceEaseOut ) { return 30; }

            else if( type == EaseType.SineEaseInOut ) { return 15; }
            else if( type == EaseType.QuadEaseInOut ) { return 15; }
            else if( type == EaseType.CubicEaseInOut ) { return 15; }
            else if( type == EaseType.QuartEaseInOut ) { return 15; }
            else if( type == EaseType.QuintEaseInOut ) { return 15; }
            else if( type == EaseType.ExpoEaseInOut ) { return 15; }
            else if( type == EaseType.CircEaseInOut ) { return 15; }
            else if( type == EaseType.BackEaseInOut ) { return 30; }
            else if( type == EaseType.ElasticEaseInOut ) { return 30; }
            else if( type == EaseType.BounceEaseInOut ) { return 30; }

            else if( type == EaseType.SineEaseOutIn ) { return 15; }
            else if( type == EaseType.QuadEaseOutIn ) { return 15; }
            else if( type == EaseType.CubicEaseOutIn ) { return 15; }
            else if( type == EaseType.QuartEaseOutIn ) { return 15; }
            else if( type == EaseType.QuintEaseOutIn ) { return 15; }
            else if( type == EaseType.ExpoEaseOutIn ) { return 15; }
            else if( type == EaseType.CircEaseOutIn ) { return 15; }
            else if( type == EaseType.BackEaseOutIn ) { return 30; }
            else if( type == EaseType.ElasticEaseOutIn ) { return 30; }
            else if( type == EaseType.BounceEaseOutIn ) { return 30; }

            return 2;

        } //END GetResolution

        //----------------------------------------------------//
        private static double GetEquation( EaseType type, float time )
        //----------------------------------------------------//
        {
            
            //Debug.Log( "GetEquation() type = " + type + ", time = " + time + ", ExpoEaseInOut = " + _ExpoEaseInOut( time, 0.0, 1.0, 1.0 ) );

            if( type == EaseType.Linear ) { return _Linear( 2, 0.0, 1.0, 1.0 ); }

            else if( type == EaseType.SineEaseIn ) { return _SineEaseIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.QuadEaseIn ) { return _QuadEaseIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.CubicEaseIn ) { return _CubicEaseIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.QuartEaseIn ) { return _QuartEaseIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.QuintEaseIn ) { return _QuintEaseIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.ExpoEaseIn ) { return _ExpoEaseIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.CircEaseIn ) { return _CircEaseIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.BackEaseIn ) { return _BackEaseIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.ElasticEaseIn ) { return _ElasticEaseIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.BounceEaseIn ) { return _BounceEaseIn( time, 0.0, 1.0, 1.0 ); }

            else if( type == EaseType.SineEaseOut ) { return _SineEaseOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.QuadEaseOut ) { return _QuadEaseOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.CubicEaseOut ) { return _CubicEaseOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.QuartEaseOut ) { return _QuartEaseOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.QuintEaseOut ) { return _QuintEaseOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.ExpoEaseOut ) { return _ExpoEaseOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.CircEaseOut ) { return _CircEaseOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.BackEaseOut ) { return _BackEaseOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.ElasticEaseOut ) { return _ElasticEaseOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.BounceEaseOut ) { return _BounceEaseOut( time, 0.0, 1.0, 1.0 ); }

            else if( type == EaseType.SineEaseInOut ) { return _SineEaseInOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.QuadEaseInOut ) { return _QuadEaseInOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.CubicEaseInOut ) { return _CubicEaseInOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.QuartEaseInOut ) { return _QuartEaseInOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.QuintEaseInOut ) { return _QuintEaseInOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.ExpoEaseInOut ) { return _ExpoEaseInOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.CircEaseInOut ) { return _CircEaseInOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.BackEaseInOut ) { return _BackEaseInOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.ElasticEaseInOut ) { return _ElasticEaseInOut( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.BounceEaseInOut ) { return _BounceEaseInOut( time, 0.0, 1.0, 1.0 ); }

            else if( type == EaseType.SineEaseOutIn ) { return _SineEaseOutIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.QuadEaseOutIn ) { return _QuadEaseOutIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.CubicEaseOutIn ) { return _CubicEaseOutIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.QuartEaseOutIn ) { return _QuartEaseOutIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.QuintEaseOutIn ) { return _QuintEaseOutIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.ExpoEaseOutIn ) { return _ExpoEaseOutIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.CircEaseOutIn ) { return _CircEaseOutIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.BackEaseOutIn ) { return _BackEaseOutIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.ElasticEaseOutIn ) { return _ElasticEaseOutIn( time, 0.0, 1.0, 1.0 ); }
            else if( type == EaseType.BounceEaseOutIn ) { return _BounceEaseOutIn( time, 0.0, 1.0, 1.0 ); }

            return _Linear( time, 0.0, 1.0, 1.0 );

        } //END GetEquation




        #region Equations

        #region Linear

        /// <summary>
        /// Easing equation function for a simple linear tweening, with no easing.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _Linear( double t, double b, double c, double d )
        {
            return c * t / d + b;
        }

        #endregion

        #region Expo

        /// <summary>
        /// Easing equation function for an exponential (2^t) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _ExpoEaseOut( double t, double b, double c, double d )
        {
            return ( t == d ) ? b + c : c * ( -Math.Pow( 2, -10 * t / d ) + 1 ) + b;
        }

        /// <summary>
        /// Easing equation function for an exponential (2^t) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _ExpoEaseIn( double t, double b, double c, double d )
        {
            return ( t == 0 ) ? b : c * Math.Pow( 2, 10 * ( t / d - 1 ) ) + b;
        }

        /// <summary>
        /// Easing equation function for an exponential (2^t) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _ExpoEaseInOut( double t, double b, double c, double d )
        {
            if( t == 0 )
                return b;

            if( t == d )
                return b + c;

            if( ( t /= d / 2 ) < 1 )
                return c / 2 * Math.Pow( 2, 10 * ( t - 1 ) ) + b;

            return c / 2 * ( -Math.Pow( 2, -10 * --t ) + 2 ) + b;
        }

        /// <summary>
        /// Easing equation function for an exponential (2^t) easing out/in: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _ExpoEaseOutIn( double t, double b, double c, double d )
        {
            if( t < d / 2 )
                return _ExpoEaseOut( t * 2, b, c / 2, d );

            return _ExpoEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
        }

        #endregion

        #region Circular

        /// <summary>
        /// Easing equation function for a circular (sqrt(1-t^2)) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _CircEaseOut( double t, double b, double c, double d )
        {
            return c * Math.Sqrt( 1 - ( t = t / d - 1 ) * t ) + b;
        }

        /// <summary>
        /// Easing equation function for a circular (sqrt(1-t^2)) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _CircEaseIn( double t, double b, double c, double d )
        {
            return -c * ( Math.Sqrt( 1 - ( t /= d ) * t ) - 1 ) + b;
        }

        /// <summary>
        /// Easing equation function for a circular (sqrt(1-t^2)) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _CircEaseInOut( double t, double b, double c, double d )
        {
            if( ( t /= d / 2 ) < 1 )
                return -c / 2 * ( Math.Sqrt( 1 - t * t ) - 1 ) + b;

            return c / 2 * ( Math.Sqrt( 1 - ( t -= 2 ) * t ) + 1 ) + b;
        }

        /// <summary>
        /// Easing equation function for a circular (sqrt(1-t^2)) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _CircEaseOutIn( double t, double b, double c, double d )
        {
            if( t < d / 2 )
                return _CircEaseOut( t * 2, b, c / 2, d );

            return _CircEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
        }

        #endregion

        #region Quad

        /// <summary>
        /// Easing equation function for a quadratic (t^2) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _QuadEaseOut( double t, double b, double c, double d )
        {
            return -c * ( t /= d ) * ( t - 2 ) + b;
        }

        /// <summary>
        /// Easing equation function for a quadratic (t^2) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _QuadEaseIn( double t, double b, double c, double d )
        {
            return c * ( t /= d ) * t + b;
        }

        /// <summary>
        /// Easing equation function for a quadratic (t^2) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _QuadEaseInOut( double t, double b, double c, double d )
        {
            if( ( t /= d / 2 ) < 1 )
                return c / 2 * t * t + b;

            return -c / 2 * ( ( --t ) * ( t - 2 ) - 1 ) + b;
        }

        /// <summary>
        /// Easing equation function for a quadratic (t^2) easing out/in: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _QuadEaseOutIn( double t, double b, double c, double d )
        {
            if( t < d / 2 )
                return _QuadEaseOut( t * 2, b, c / 2, d );

            return _QuadEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
        }

        #endregion

        #region Sine

        /// <summary>
        /// Easing equation function for a sinusoidal (sin(t)) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _SineEaseOut( double t, double b, double c, double d )
        {
            return c * Math.Sin( t / d * ( Math.PI / 2 ) ) + b;
        }

        /// <summary>
        /// Easing equation function for a sinusoidal (sin(t)) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _SineEaseIn( double t, double b, double c, double d )
        {
            return -c * Math.Cos( t / d * ( Math.PI / 2 ) ) + c + b;
        }

        /// <summary>
        /// Easing equation function for a sinusoidal (sin(t)) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _SineEaseInOut( double t, double b, double c, double d )
        {
            if( ( t /= d / 2 ) < 1 )
                return c / 2 * ( Math.Sin( Math.PI * t / 2 ) ) + b;

            return -c / 2 * ( Math.Cos( Math.PI * --t / 2 ) - 2 ) + b;
        }

        /// <summary>
        /// Easing equation function for a sinusoidal (sin(t)) easing in/out: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _SineEaseOutIn( double t, double b, double c, double d )
        {
            if( t < d / 2 )
                return _SineEaseOut( t * 2, b, c / 2, d );

            return _SineEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
        }

        #endregion

        #region Cubic

        /// <summary>
        /// Easing equation function for a cubic (t^3) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _CubicEaseOut( double t, double b, double c, double d )
        {
            return c * ( ( t = t / d - 1 ) * t * t + 1 ) + b;
        }

        /// <summary>
        /// Easing equation function for a cubic (t^3) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _CubicEaseIn( double t, double b, double c, double d )
        {
            return c * ( t /= d ) * t * t + b;
        }

        /// <summary>
        /// Easing equation function for a cubic (t^3) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _CubicEaseInOut( double t, double b, double c, double d )
        {
            if( ( t /= d / 2 ) < 1 )
                return c / 2 * t * t * t + b;

            return c / 2 * ( ( t -= 2 ) * t * t + 2 ) + b;
        }

        /// <summary>
        /// Easing equation function for a cubic (t^3) easing out/in: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _CubicEaseOutIn( double t, double b, double c, double d )
        {
            if( t < d / 2 )
                return _CubicEaseOut( t * 2, b, c / 2, d );

            return _CubicEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
        }

        #endregion

        #region Quartic

        /// <summary>
        /// Easing equation function for a quartic (t^4) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _QuartEaseOut( double t, double b, double c, double d )
        {
            return -c * ( ( t = t / d - 1 ) * t * t * t - 1 ) + b;
        }

        /// <summary>
        /// Easing equation function for a quartic (t^4) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _QuartEaseIn( double t, double b, double c, double d )
        {
            return c * ( t /= d ) * t * t * t + b;
        }

        /// <summary>
        /// Easing equation function for a quartic (t^4) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _QuartEaseInOut( double t, double b, double c, double d )
        {
            if( ( t /= d / 2 ) < 1 )
                return c / 2 * t * t * t * t + b;

            return -c / 2 * ( ( t -= 2 ) * t * t * t - 2 ) + b;
        }

        /// <summary>
        /// Easing equation function for a quartic (t^4) easing out/in: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _QuartEaseOutIn( double t, double b, double c, double d )
        {
            if( t < d / 2 )
                return _QuartEaseOut( t * 2, b, c / 2, d );

            return _QuartEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
        }

        #endregion

        #region Quintic

        /// <summary>
        /// Easing equation function for a quintic (t^5) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _QuintEaseOut( double t, double b, double c, double d )
        {
            return c * ( ( t = t / d - 1 ) * t * t * t * t + 1 ) + b;
        }

        /// <summary>
        /// Easing equation function for a quintic (t^5) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _QuintEaseIn( double t, double b, double c, double d )
        {
            return c * ( t /= d ) * t * t * t * t + b;
        }

        /// <summary>
        /// Easing equation function for a quintic (t^5) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _QuintEaseInOut( double t, double b, double c, double d )
        {
            if( ( t /= d / 2 ) < 1 )
                return c / 2 * t * t * t * t * t + b;
            return c / 2 * ( ( t -= 2 ) * t * t * t * t + 2 ) + b;
        }

        /// <summary>
        /// Easing equation function for a quintic (t^5) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _QuintEaseOutIn( double t, double b, double c, double d )
        {
            if( t < d / 2 )
                return _QuintEaseOut( t * 2, b, c / 2, d );
            return _QuintEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
        }

        #endregion

        #region Elastic

        /// <summary>
        /// Easing equation function for an elastic (exponentially decaying sine wave) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _ElasticEaseOut( double t, double b, double c, double d )
        {
            if( ( t /= d ) == 1 )
                return b + c;

            double p = d * .3;
            double s = p / 4;

            return ( c * Math.Pow( 2, -10 * t ) * Math.Sin( ( t * d - s ) * ( 2 * Math.PI ) / p ) + c + b );
        }

        /// <summary>
        /// Easing equation function for an elastic (exponentially decaying sine wave) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _ElasticEaseIn( double t, double b, double c, double d )
        {
            if( ( t /= d ) == 1 )
                return b + c;

            double p = d * .3;
            double s = p / 4;

            return -( c * Math.Pow( 2, 10 * ( t -= 1 ) ) * Math.Sin( ( t * d - s ) * ( 2 * Math.PI ) / p ) ) + b;
        }

        /// <summary>
        /// Easing equation function for an elastic (exponentially decaying sine wave) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _ElasticEaseInOut( double t, double b, double c, double d )
        {
            if( ( t /= d / 2 ) == 2 )
                return b + c;

            double p = d * ( .3 * 1.5 );
            double s = p / 4;

            if( t < 1 )
                return -.5 * ( c * Math.Pow( 2, 10 * ( t -= 1 ) ) * Math.Sin( ( t * d - s ) * ( 2 * Math.PI ) / p ) ) + b;
            return c * Math.Pow( 2, -10 * ( t -= 1 ) ) * Math.Sin( ( t * d - s ) * ( 2 * Math.PI ) / p ) * .5 + c + b;
        }

        /// <summary>
        /// Easing equation function for an elastic (exponentially decaying sine wave) easing out/in: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _ElasticEaseOutIn( double t, double b, double c, double d )
        {
            if( t < d / 2 )
                return _ElasticEaseOut( t * 2, b, c / 2, d );
            return _ElasticEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
        }

        #endregion

        #region Bounce

        /// <summary>
        /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _BounceEaseOut( double t, double b, double c, double d )
        {
            if( ( t /= d ) < ( 1 / 2.75 ) )
                return c * ( 7.5625 * t * t ) + b;
            else if( t < ( 2 / 2.75 ) )
                return c * ( 7.5625 * ( t -= ( 1.5 / 2.75 ) ) * t + .75 ) + b;
            else if( t < ( 2.5 / 2.75 ) )
                return c * ( 7.5625 * ( t -= ( 2.25 / 2.75 ) ) * t + .9375 ) + b;
            else
                return c * ( 7.5625 * ( t -= ( 2.625 / 2.75 ) ) * t + .984375 ) + b;
        }

        /// <summary>
        /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _BounceEaseIn( double t, double b, double c, double d )
        {
            return c - _BounceEaseOut( d - t, 0, c, d ) + b;
        }

        /// <summary>
        /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _BounceEaseInOut( double t, double b, double c, double d )
        {
            if( t < d / 2 )
                return _BounceEaseIn( t * 2, 0, c, d ) * .5 + b;
            else
                return _BounceEaseOut( t * 2 - d, 0, c, d ) * .5 + c * .5 + b;
        }

        /// <summary>
        /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out/in: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _BounceEaseOutIn( double t, double b, double c, double d )
        {
            if( t < d / 2 )
                return _BounceEaseOut( t * 2, b, c / 2, d );
            return _BounceEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
        }

        #endregion

        #region Back

        /// <summary>
        /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _BackEaseOut( double t, double b, double c, double d )
        {
            return c * ( ( t = t / d - 1 ) * t * ( ( 1.70158 + 1 ) * t + 1.70158 ) + 1 ) + b;
        }

        /// <summary>
        /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _BackEaseIn( double t, double b, double c, double d )
        {
            return c * ( t /= d ) * t * ( ( 1.70158 + 1 ) * t - 1.70158 ) + b;
        }

        /// <summary>
        /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _BackEaseInOut( double t, double b, double c, double d )
        {
            double s = 1.70158;
            if( ( t /= d / 2 ) < 1 )
                return c / 2 * ( t * t * ( ( ( s *= ( 1.525 ) ) + 1 ) * t - s ) ) + b;
            return c / 2 * ( ( t -= 2 ) * t * ( ( ( s *= ( 1.525 ) ) + 1 ) * t + s ) + 2 ) + b;
        }

        /// <summary>
        /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out/in: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        private static double _BackEaseOutIn( double t, double b, double c, double d )
        {
            if( t < d / 2 )
                return _BackEaseOut( t * 2, b, c / 2, d );
            return _BackEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
        }

        #endregion

        #endregion

    }
} //END Namespace