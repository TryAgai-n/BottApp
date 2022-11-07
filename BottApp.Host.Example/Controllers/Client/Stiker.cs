

namespace BottApp
{
    public class Stiker
    {
        public string Description { get; set; }
        public string Stiker_ID { get; set; }

        public Stiker(string _description, string _stiker_ID)
        {
            Description = _description;
            Stiker_ID = _stiker_ID;
        }
    }
}
