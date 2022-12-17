using Auxiliary;

namespace Infuse.Data
{
    public class InfuseUser : BsonModel
    {
        private int _tshockId;
        public int TShockId
        {
            get
                => _tshockId;
            set
            {
                _ = this.SaveAsync(x => x.TShockId, value);
                _tshockId = value;
            }
        }

        private int[] _buffs = Array.Empty<int>();
        public int[] Buffs
        {
            get
                => _buffs;
            set
            {
                _ = this.SaveAsync(x => x.Buffs, value);
                _buffs = value;
            }
        }
    }
}
