namespace BottApp.Database.User;

public class Profile
{

    public string? FirstName { get; }
    public string? LastName { get; }
    
    
    public Profile(string? firstName, string? lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    
    
    public string GetFullName => LastName + ", " + FirstName;
    
}