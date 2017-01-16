namespace Player
{
    [System.Serializable]
    public class AmmoBag
    {
        public ProjectileBase projectilePrefab;
        public int haveCount;
        public int maxCarryCount;

        public AmmoBag(ProjectileBase _projectilePrefab, int _haveCount, int _maxCarryCount)
        {
            projectilePrefab = _projectilePrefab;
            haveCount = _haveCount;
            maxCarryCount = _maxCarryCount;
        }
    }
}