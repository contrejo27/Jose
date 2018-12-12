using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrandXR
{
    public class ComponentHelper
    {
        //Used when adding a component to a gameObject by string
        //Finds the first instance of the script in all assemblies
        //https://forum.unity.com/threads/addcomponent-string-still-needed.317042/
        //EX: gameObject.AddComponent(TypeUtil.FindType("Rigidbody"));
        public static System.Type FindType( string typeName, bool useFullName = false, bool ignoreCase = false )
        {
            if( string.IsNullOrEmpty( typeName ) ) return null;

            StringComparison e = ( ignoreCase ) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if( useFullName )
            {
                foreach( var assemb in System.AppDomain.CurrentDomain.GetAssemblies() )
                {
                    foreach( var t in assemb.GetTypes() )
                    {
                        if( string.Equals( t.FullName, typeName, e ) ) return t;
                    }
                }
            }
            else
            {
                foreach( var assemb in System.AppDomain.CurrentDomain.GetAssemblies() )
                {
                    foreach( var t in assemb.GetTypes() )
                    {
                        if( string.Equals( t.FullName, typeName, e ) ) return t;
                    }
                }
            }
            return null;
        }

    } //END ComponentHelper

} //END Namespace