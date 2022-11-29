namespace BottApp.Database.User;

public class ProfilePatch
{
    public readonly string FirstName;


    public ProfilePatch(string firstName)
    {
        FirstName = firstName;
    }
}