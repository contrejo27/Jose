/* CSVReader.cs
 * 
 * Given a string of csv text, splits the text and returns a 2D string array
 * 
 * USAGE: CSVReader.SplitCsvGrid(textString)
 * 
 * Adapted from Unify code
 * http://wiki.unity3d.com/index.php/CSVReader
 * 
 * CSVReader by Dock. (24/8/11)
 * http://starfruitgames.com
 * 
 */

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace BrandXR
{
    public class CSVReader: MonoBehaviour
    {


        //The CSV text file downloaded from the web or located in local storage
        public TextAsset csvFile;



        //------------------------------//
        public void Start()
        //------------------------------//
        {
            string[,] grid = SplitCsvGrid( csvFile.text );

            Debug.Log( "size = " + ( 1 + grid.GetUpperBound( 0 ) ) + "," + ( 1 + grid.GetUpperBound( 1 ) ) );

            DebugOutputGrid( grid );

        } //END Start


        //--------------------------------------------------------//
        /// <summary>
        /// Outputs the content of a 2D array, useful for checking the importer
        /// </summary>
        /// <param name="grid"></param>
        static public void DebugOutputGrid( string[,] grid )
        //--------------------------------------------------------//
        {
            string textOutput = "";

            for( int y = 0; y < grid.GetUpperBound( 1 ); y++ )
            {
                for( int x = 0; x < grid.GetUpperBound( 0 ); x++ )
                {

                    textOutput += grid[ x, y ];
                    textOutput += "|";
                }

                textOutput += "\n";
            }

            Debug.Log( textOutput );

        } //END DebugOutputGrid


        //-----------------------------------------------------------//
        /// <summary>
        /// Splits a CSV file into a 2D string array
        /// </summary>
        /// <param name="csvText"></param>
        /// <returns></returns>
        static public string[,] SplitCsvGrid( string csvText )
        //-----------------------------------------------------------//
        {
            //Split the CSV file into lines based on where the "Newline" exists
            string[] lines = csvText.Split( "\n"[ 0 ] );

            //Finds the max width of row
            int width = 0;
            int height = 0;

            for( int i = 0; i < lines.Length; i++ )
            {
                string[] row = SplitCsvLine( lines[ i ] );

                if( row != null && row.Length > 0 )
                {
                    width = Mathf.Max( width, row.Length );
                }

                //If there is data for this row, count it as an actual line for our 'height'
                if( row != null && row.Length > 0 )
                {
                    height++;
                }

                /*
                for( int j = 0; j < row.Length; j++ )
                {
                    Debug.Log( "CSVReader.cs SplitCsvGrid() Split Line[" + i + "], row[" + j + "] = " + row[j] );
                }
                */
            }

            //Debug.Log( "CSVReader.cs SplitCsvGrid() Found line Width = " + width + ", Height = " + height );

            //Creates new 2D string grid to output to
            string[,] outputGrid = new string[ width + 1, height + 1 ];

            for( int y = 0; y < lines.Length; y++ )
            {
                string[] row = SplitCsvLine( lines[ y ] );

                for( int x = 0; x < row.Length; x++ )
                {
                    outputGrid[ x, y ] = row[ x ];
                    
                    //Debug.Log( "Adding outputGrid[ " + x + ", " + y + " ] = " + outputGrid[ x, y ] );

                    // This line was to replace "" with " in my output.
                    // Include or edit it as you wish.
                    //outputGrid[ x, y ] = outputGrid[ x, y ].Replace( "\"\"", "\"" );
                }
            }
            
            return outputGrid;

        } //END SplitCsvGrid

        //-----------------------------------------------------//
        public static List<string> GetHeaders( string[,] grid )
        //-----------------------------------------------------//
        {

            List<string> headers = new List<string>();

            if( grid != null && grid.Length > 0 )
            {
                //Debug.Log( "----------------------------------------------------------------\n----------------------------------------------------------------" );
                //Debug.Log( "GetHeaders() grid.GetUpperBound(1)[y] = " + grid.GetUpperBound(1) + ", grid.GetUpperBound(0)[x] = " + grid.GetUpperBound(0) );

                for( int y = 0; y < grid.GetUpperBound( 1 ); y++ )
                {
                    for( int x = 0; x < grid.GetUpperBound( 0 ); x++ )
                    {
                        //Debug.Log( "GetHeaders() grid[ " + x + ", " + y + " ] = " + grid[x,y] );

                        //if this is the first line in the grid
                        if( y == 0 )
                        {
                            //Debug.Log( "GetHeaders() header.add( grid[ " + x + ", " + y + " ](" + grid[ x, y ] + ") )" );
                            headers.Add( grid[x,y] );
                        }
                    }
                }
            }

            return headers;

        } //END GetHeaders

        //-----------------------------------------------------//
        /// <summary>
        /// Splits a CSV row
        /// </summary>
        /// <param name="line">The line of CSV text to parse</param>
        /// <returns></returns>
        static public string[] SplitCsvLine( string line )
        //-----------------------------------------------------//
        {
            string pattern = @"
            # Match one value in valid CSV string.
            (?!\s*$)                                      # Don't match empty last value.
            \s*                                           # Strip whitespace before value.
            (?:                                           # Group for value alternatives.
            '(?<val>[^'\\]*(?:\\[\S\s][^'\\]*)*)'         # Either $1: Single quoted string,
            | ""(?<val>[^""\\]*(?:\\[\S\s][^""\\]*)*)""   # or $2: Double quoted string,
            | (?<val>[^,'""\s\\]*(?:\s+[^,'""\s\\]+)*)    # or $3: Non-comma, non-quote stuff.
            )                                             # End group of value alternatives.
            \s*                                           # Strip whitespace after value.
            (?:,|$)                                       # Field ends on comma or EOS.
            ";

            string[] values = ( from Match m in Regex.Matches( line, pattern,
                 RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline )
                                select m.Groups[ 1 ].Value ).ToArray();

            return values;
            
        } //END SplitCsvLine


    } //END Class

} //END Namespace