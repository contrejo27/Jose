﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace BrandXR
{
    class MathHelper
    {

        // maps a value from one range of values to another range of values
        // see http://openframeworks.cc/documentation/math/ofMath/#show_ofMap for more
        // For between functionality, val = between, inputMin = min, inputMax = max, outputMin = 0, outputMax = 1
        // For percentage functionality, val = percentage, inputMin = 0, inputMax = 1, outputMin = minRatio, outputMax = maxRatio
        //----------------------------------------------------------------------------------------------------------------------------//
        public static float Map( float val, float inputMin, float inputMax, float outputMin, float outputMax )
        //----------------------------------------------------------------------------------------------------------------------------//
        {
            return ( val - inputMin ) / ( inputMax - inputMin ) * ( outputMax - outputMin ) + outputMin;

        } //END Map


        /// Returns 0 if @between is less then @min and 1 if @between is greater then @max
        // Assumes min < max
        //Returns how close a value is to the max as a percentage
        //Example: if Max was 2 and min was 0, and between was 1, the percentage returned would be .5
        //----------------------------------------------------------------------------------------------------------------------------//
        public static float GetPercentageBetween( float max, float min, float between )
        //----------------------------------------------------------------------------------------------------------------------------//
        {
            return ( Mathf.Clamp( between, min, max ) - min ) / ( max - min );

        } //END GetPercentageBetween


        /// assumes percentage is between 0 and 1
        /// assumes minRatio is less then maxRatio
        //----------------------------------------------------------------------------------------------------------------------------//
        public static float GetRatioBetweenTwoNumbers( float percentage, float minRatio, float maxRatio )
        //----------------------------------------------------------------------------------------------------------------------------//
        {
            //Returns how far a percentage is between the two ratios.
            //Example, a percentage of .5 would return 1 if the minRatio and maxRatio were 0 and 2
            return ( maxRatio - minRatio ) * percentage + minRatio;

        } //END GetRatioBetweenTwoNumbers

        //----------------------------------------------------------------------------------------------------------------------------//
        public static int GetClosestElementInList( float target, List<float> collection )
        //----------------------------------------------------------------------------------------------------------------------------//
        {

            int closestElement = -99;
            float closestDifference = Mathf.Infinity;
            float difference = Mathf.Infinity;

            for( int i = 0; i < collection.Count; i++ )
            {
                difference = Math.Abs( collection[ i ] - target );

                if( difference < closestDifference )
                {
                    closestElement = i;
                    closestDifference = difference;
                }
            }

            return closestElement;

        } //END GetClosestElementInList






        public enum Bounds
        {
            INCLUSIVE_INCLUSIVE,
            INCLUSIVE_EXCLUSIVE,
            EXCLUSIVE_INCLUSIVE,
            EXCLUSIVE_EXCLUSIVE
        }

        //-------------------------------------------------------------------------------------//
        //Convenience method from https://stackoverflow.com/questions/3188672/how-to-elegantly-check-if-a-number-is-within-a-range
        public static bool IsBetween( int theNumber, int low, int high, Bounds boundDef )
        //-------------------------------------------------------------------------------------//
        {
            bool result;
            switch( boundDef )
            {
                case Bounds.INCLUSIVE_INCLUSIVE:
                result = ( ( low <= theNumber ) && ( theNumber <= high ) );
                break;
                case Bounds.INCLUSIVE_EXCLUSIVE:
                result = ( ( low <= theNumber ) && ( theNumber < high ) );
                break;
                case Bounds.EXCLUSIVE_INCLUSIVE:
                result = ( ( low < theNumber ) && ( theNumber <= high ) );
                break;
                case Bounds.EXCLUSIVE_EXCLUSIVE:
                result = ( ( low < theNumber ) && ( theNumber < high ) );
                break;
                default:
                throw new System.ArgumentException( "Invalid boundary definition argument" );
            }
            return result;

        } //END IsBetween

        //-------------------------------------------------------------------------------------//
        //Convenience method from https://stackoverflow.com/questions/3188672/how-to-elegantly-check-if-a-number-is-within-a-range
        public static bool IsBetween( long theNumber, int low, int high, Bounds boundDef )
        //-------------------------------------------------------------------------------------//
        {
            bool result;
            switch( boundDef )
            {
                case Bounds.INCLUSIVE_INCLUSIVE:
                result = ( ( low <= theNumber ) && ( theNumber <= high ) );
                break;
                case Bounds.INCLUSIVE_EXCLUSIVE:
                result = ( ( low <= theNumber ) && ( theNumber < high ) );
                break;
                case Bounds.EXCLUSIVE_INCLUSIVE:
                result = ( ( low < theNumber ) && ( theNumber <= high ) );
                break;
                case Bounds.EXCLUSIVE_EXCLUSIVE:
                result = ( ( low < theNumber ) && ( theNumber < high ) );
                break;
                default:
                throw new System.ArgumentException( "Invalid boundary definition argument" );
            }
            return result;

        } //END IsBetween


    } //END Class

} //END Namespace