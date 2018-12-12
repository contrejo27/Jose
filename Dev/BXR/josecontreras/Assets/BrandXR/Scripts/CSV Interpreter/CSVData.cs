/* CSVData.cs
 * 
 * A serializable data object containing information about a CSV file.
 * Instantiated and used by CSVDataManager.cs
 */
  
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BrandXR
{
    public class CSVData: SerializedScriptableObject
    {
        [HideInInspector]
        public string originalCSVText = "";

        [HideInInspector]
        public string[,] rawCSV_original;

        [HideInInspector]
        public string[,] rawCSV_temp;

        public List<string> headers;

        public int columnLength;

        [SerializeField]
        public Dictionary<string, DataColumn> dataColumns = new Dictionary<string, DataColumn>();



        //-------------------------------//
        /// <summary>
        /// Finds how many columns there are in the CSV file.
        /// </summary>
        public void InitLength()
        //-------------------------------//
        {

            if( headers == null || ( headers != null && headers.Count == 0 ) )
            {
                Debug.LogError( "CSVData.cs InitLength() Unable to Initialize the column length, the headers list is null or empty" );
                return;
            }

            if( dataColumns == null || ( dataColumns != null && dataColumns.Count == 0 ) )
            {
                Debug.LogError( "CSVData.cs InitLength() Unable to Initialize the column length, the dataColumns dictionary is null or empty" );
                return;
            }

            columnLength = dataColumns[ headers[ 0 ] ].data.Length;

        } //END InitLength



        //---------------------------------------------------------------//
        /// <summary>
        /// Creates a column out of the array of string data
        /// </summary>
        /// <param name="label">What is the name for this column of data (aka header)</param>
        /// <param name="data">The array of string data</param>
        /// <returns></returns>
        public DataColumn CreateColumn( string label, string[] data )
        //---------------------------------------------------------------//
        {

            if( label == "" )
            {
                Debug.LogError( "CSVData.cs CreateColumn() Unable to create column, the string label is empty" );
                return null;
            }

            if( data == null || ( data != null && data.Length == 0 ) )
            {
                Debug.LogError( "CSVData.cs CreateColumn() Unable to create column, the 2D string array of data is null or empty" );
                return null;
            }

            DataColumn column = ScriptableObject.CreateInstance<DataColumn>();

            column.Initialize( data[ 0 ] );

            string[] dataNoHeader = new string[ data.Length - 1 ];

            for( int i = 0; i < data.Length - 1; i++ )
            {
                if( label != data[ i + 1 ] )
                {
                    //Debug.Log( "CSVData.cs CreateColumn() label( " + label + " ) != data[ " + i + " + 1 ](" + data[ i + 1 ] + ")" );
                    dataNoHeader[ i ] = data[ i + 1 ];
                }
                else
                {
                    //Debug.Log( "CSVData.cs CreateColumn() label( " + label + " ) == data[ " + i + " + 1 ](" + data[ i + 1 ] + ")" );
                }
            }

            column.data = dataNoHeader;

            return column;

        } //END CreateColumn


        //-----------------------------------------------------//
        /// <summary>
        /// Add a 2D array of all of the string data contained in a column to the dataColumns dictionary, stored with the label set as the dictionary key
        /// </summary>
        /// <param name="label">The dictionary key and header name for this data</param>
        /// <param name="data">The 2D string array of column data</param>
        public void AddColumn( string label, string[] data )
        //-----------------------------------------------------//
        {

            if( label == "" )
            {
                Debug.LogError( "CSVData.cs AddColumn() Failed to create column, the passed in label is empty" );
                return;
            }

            if( data == null || ( data != null && data.Length == 0 ) )
            {
                Debug.LogError( "CSVData.cs AddColumn() Failed to create column, the 2D array of string data is null or empty" );
            }

            //If the dataColumns dictionary is null, create a new dictionary
            if( dataColumns == null ) { dataColumns = new Dictionary<string, DataColumn>(); }

            DataColumn column = CreateColumn( label, data );

            if( column != null )
            {
                /*
                foreach( string s in data )
                {
                    Debug.Log( "CSVData.cs AddColumn() label = " + label + ", data = " + s );
                }
                */

                dataColumns.Add( label, column );
            }
            else
            {
                Debug.LogError( "CSVData.cs AddColumn() Failed to create a column for the passed in label = " + label + ", and data.Length = " + data.Length );
                return;
            }

        } //END AddColumn


        //--------------------------------------------//
        /// <summary>
        /// Based on the CSV file headers, seperates all the columns by label and adds them to the dataColumn dictionary
        /// </summary>
        public void AddColumnsFromHeaders()
        //--------------------------------------------//
        {
            
            if( headers == null || ( headers != null && headers.Count == 0 ) )
            {
                Debug.LogError( "CSVData.cs AddColumnsFromHeaders() Unable to iterate through list of headers because the list is null" );
                return;
            }

            if( headers != null && headers.Count > 0 )
            {
                foreach( string s in headers )
                {
                    if( s != null && s != "" )
                    {
                        AddColumn( s, FindColumn( s ) );
                    }
                }
            }
            
        } //END AddColumnsFromHeaders

        //--------------------------------------------//
        /// <summary>
        /// Returns a column of data as a string array based on the name of the header you pass in
        /// </summary>
        /// <param name="header">The name of the header you would like to find the column data for (as a string array)</param>
        /// <returns>The 2D string array containing the data for the column</returns>
        public string[] FindColumn( string header )
        //--------------------------------------------//
        {

            if( header == null || header == "" )
            {
                Debug.LogError( "CSVData.cs FindColumn() unable to return a column of string data for the given header, because the header string is empty" );
                return null;
            }

            if( rawCSV_original == null || ( rawCSV_original != null && rawCSV_original.Length == 0 ) )
            {
                Debug.LogError( "CSVData.cs FindColumn() unable to locate the column array of string data, the rawCSV_Original data string is empty" );
                return null;
            }

            string[] data = new string[ rawCSV_original.GetLength( 1 ) - 1 ];

            if( data != null && data.Length > 0 )
            {
                for( int i = 0; i < rawCSV_original.GetLength( 0 ); i++ )
                {
                    if( rawCSV_original[ i, 0 ] == header )
                    {
                        for( int j = 0; j < rawCSV_original.GetLength( 1 ) - 1; j++ )
                        {
                            data[ j ] = rawCSV_original[ i, j ];
                        }

                        return data;
                    }
                }
            }
            

            Debug.LogError( "CSVData.cs FindColumn() The column header " + header + " could not be found." );
            return null;

        } //END FindColumn


        //------------------------------------//
        /// <summary>
        /// Used to recreate the CSV Data string from the data we have in this CSVData scriptable object
        /// </summary>
        /// <returns>The string of CSVData rebuilt from the column and row information in this CSVData scriptable object</returns>
        public string RebuildString()
        //------------------------------------//
        {

            if( headers == null || ( headers != null && headers.Count == 0 ) )
            {
                Debug.LogError( "CSVData.cs RebuildString() Unable to rebuild the data string, the list of headers is null or empty" );
                return null;
            }

            if( dataColumns == null || ( dataColumns != null && dataColumns.Count == 0 ) )
            {
                Debug.LogError( "CSVData.cs RebuildString() Unable to rebuild the data string, the dictionary of dataColumn is null or empty" );
            }

            string temp = "";

            //Add the headers to the string
            if( headers != null && headers.Count > 0 )
            {
                for( int i = 0; i < headers.Count; i++ )
                {
                    if( headers[i] != null && headers[i] != "" )
                    {
                        if( i == 0 )
                        {
                            temp += headers[ i ];
                        }
                        else
                        {
                            temp += "," + headers[ i ];
                        }
                    }
                }
            }
            
            //Add the newline to the string after the header
            temp += "\n";

            //Add the rows for this column
            if( dataColumns != null && dataColumns.Count > 0 )
            {
                //For all of the columns...
                for( int j = 0; j < dataColumns[ headers[ 0 ] ].data.Length; j++ )
                {
                    
                    if( headers != null && headers.Count > 0 )
                    {
                        for( int i = 0; i < headers.Count; i++ )
                        {
                            //Get the data for the column
                            string[] tempColumn = dataColumns[ headers[ i ] ].data;

                            //Add the row data
                            if( i == 0 )
                            {
                                temp += tempColumn[ j ];
                            }
                            else
                            {
                                temp += "," + tempColumn[ j ];
                            }
                        }
                    }
                    

                    temp += "\n";
                }
            }
            
            return temp;

        } //END RebuildString

        //------------------------------------------------//
        /// <summary>
        /// Returns a column of data for the header you pass in
        /// </summary>
        /// <param name="header">The name of the header you want to grab the column data for</param>
        /// <returns>A DataColumn object used to peruse one column's worth of data</returns>
        public DataColumn GetColumn( string header )
        //------------------------------------------------//
        {
            return dataColumns[ header ];

        } //END GetColumn



    } //END class

} //END namespace