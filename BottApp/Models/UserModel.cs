namespace BottApp.Models
{
    public class UserModel
    {
        public string Name { get; }
        public long Id { get; }
        public string Phone { get;}
        public bool isSendContact { get; }

        public UserModel(string _name, string _phone, long _id, bool _isSendContact)
        {
            Name = _name;
            Phone = _phone;
            Id = _id;
            isSendContact = _isSendContact;
        }
    }
}
