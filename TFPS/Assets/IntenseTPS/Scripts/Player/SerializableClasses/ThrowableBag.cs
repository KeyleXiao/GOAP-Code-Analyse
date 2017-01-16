namespace Player
{
    [System.Serializable]
    public class ThrowableBag
    {
        public Exploder throwablePrefab;
        public int haveCount;
        public int maxCarryCount;

        public ThrowableBag(Exploder _throwablePrefab, int _haveCount, int _maxCarryCount)
        {
            throwablePrefab = _throwablePrefab;
            haveCount = _haveCount;
            maxCarryCount = _maxCarryCount;
        }
    }
}