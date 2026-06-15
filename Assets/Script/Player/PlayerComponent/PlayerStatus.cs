namespace Script.Player.PlayerComponent
{
    public class PlayerStatus : CharacterStatus
    {
        private PlayerController _player;

        // TODO: 数据注入
        public void Initialize(PlayerController player)
        {
            _player = player;
        }

        protected override void OnDeath()
        {
            if (_player != null)
            {
                _player.Die();
            }
        }
    }
}