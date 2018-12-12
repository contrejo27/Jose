/* DataManager.cs
 * 
 * Used to take a CSV data file (Comma-Seperated-Values, a common export type from Databases, Google Sheets, and Excel)
 * and return a CSVData Scriptable Object that makes it easy to peruse its data
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System;

namespace BrandXR
{
    public class CSVDataManager: MonoBehaviour
    {
        
        private Action<CSVData> OnSuccess;
        private Action OnFailed;

        //----------------------------------------------------//
        /// <summary>
        /// Finds a CSV data file in your local storage and returns a CSVData object, which you can use to peruse the data easily
        /// </summary>
        /// <param name="path">The path to the file in local storage or on the web</param>
        /// <param name="OnSuccess">The method you want to be called when a CSV file is successfully created</param>
        /// <param name="OnFailed">The method you want to be called when a CSV file fails to load</param>
        /// <returns>CSVData - The CSVData object containing all of the information about the CSV data file</returns>
        public void CreateCSVData( string path, string fileName, Action<CSVData> OnSuccess, Action OnFailed )
        //----------------------------------------------------//
        {
            this.OnSuccess = OnSuccess;
            this.OnFailed = OnFailed;

            //If the user did not provide a name for the CSV file, set it to the product name of the project
            if( fileName == "" )
            {
                fileName = Application.productName;
            }

            //Locate the CSV file at the path
            WWWHelper.instance.GetCSV( path, true, FindCSVSuccess, FindCSVFailed, WWWHelper.LocationType.Web, fileName );
            
        } //END CreateCSVData

        //-----------------------------------------------//
        private void FindCSVSuccess( string csvText )
        //-----------------------------------------------//
        {

            if( csvText != null && csvText != "" )
            {
                //Store the CSV data file in our local storage
                string csvPath = Application.persistentDataPath + "/" ;

                //Create the scriptable object that will store the data of the CSV file
                CSVData csvData = ScriptableObject.CreateInstance<CSVData>();

                //Store the original data
                csvData.originalCSVText = csvText;
                csvData.rawCSV_original = CSVReader.SplitCsvGrid( csvText );

                //Grab the headers from the CSV data
                csvData.headers = CSVReader.GetHeaders( csvData.rawCSV_original );

                //Setup the columns from the headers
                csvData.AddColumnsFromHeaders();

                csvData.InitLength();

                if( OnSuccess != null )
                {
                    OnSuccess.Invoke( csvData );
                }

            }
            else
            {
                FindCSVFailed();
            }

        } //END FindCSVSuccess

        //-----------------------------------------------//
        private void FindCSVFailed()
        //-----------------------------------------------//
        {

            Debug.LogError( "CSVDataManager.cs FindCSVFailed() Unable to locate CSV file in either local or web" );

            if( OnFailed != null )
            {
                OnFailed.Invoke();
            }

        } //END FindCSVFailed

        //-----------------------------------------------//
        /// <summary>
        /// Reads the CSV file at the given local path and returns its data as a 2D string array
        /// </summary>
        /// <param name="localFilePath">The location of the CSVData at a local file path</param>
        /// <returns>Returns a 2D string of the file data </returns>
        private string[,] ReadCSV( string localFilePath )
        //-----------------------------------------------//
        {
            string fileText = System.IO.File.ReadAllText( localFilePath );

            string[,] fileData = CSVReader.SplitCsvGrid( fileText );
            
            return fileData;

        } //END ReadCSV



    } //END namespace

} //END BrandXR