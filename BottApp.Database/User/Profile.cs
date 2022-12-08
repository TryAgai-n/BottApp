namespace BottApp.Database.User;

public class Profile
{
    public Profile(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
    }
    private readonly string _firstName;
    private readonly string _lastName;

}