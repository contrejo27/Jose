namespace BrandXR
{
    public class Pointer<T>
    {
        //Use this class as a way to pass an reference variable through a coroutine
        //(normally a reference variable cannot be used with a Coroutine, use this "Pointer" class to get around this limitation)

        public T value;
    }
}