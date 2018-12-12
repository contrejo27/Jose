using Sirenix.OdinInspector;

namespace BrandXR
{
    public class DataColumn: SerializedScriptableObject
    {

        public string label = "";
        public string header;
        public string[] data;

        //-------------------------------------------------------//
        public void Initialize( string headerText )
        //-------------------------------------------------------//
        {
            header = headerText;

        } //END Initialize

        //-------------------------------------------------------//
        public string GetRow( int row )
        //-------------------------------------------------------//
        {
            return data[ row ];

        } //END GetRow

        //----------------------------------------------------------------------------//
        public string[] GetRows( int startRow, int endRow, int sampling = 1 )
        //----------------------------------------------------------------------------//
        {
            string[] set = new string[ ( ( endRow - startRow ) / sampling ) + 1 ];

            int samplingCount = startRow;
            for( int i = 0; samplingCount < endRow; i++ )
            {
                set[ i ] = data[ samplingCount ];
                samplingCount += sampling;
            }

            return set;

        } //END GetRows

        //----------------------------------------------------//
        public void SetRow( int row, string updatedData )
        //----------------------------------------------------//
        {
            data[ row ] = updatedData;

        } //END SetRow

    } //END Class

} //END Namespace 