namespace Player
{
    public class FirstPersonCameraParams : CameraModderParamsBase
    {
        public static FirstPersonCameraParams operator +(FirstPersonCameraParams modifier1ReturnedAddTo, FirstPersonCameraParams modifier2AddThis)
        {
            return modifier1ReturnedAddTo;
        }
    }
}