namespace Information
{
    public class InformationReceivedDamage : InformationP
    {
        public Attribute<float> damage;

        public InformationReceivedDamage(
            float _damage,
            float _damageConfidence
            ) : base()
        {
            damage.Set(_damage, _damageConfidence);
        }
    }
}