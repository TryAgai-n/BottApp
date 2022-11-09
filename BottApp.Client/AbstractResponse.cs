using System.ComponentModel.DataAnnotations;

namespace BottApp.Client;

public abstract class AbstractResponse
{
    [Required]
    public Status Status;


    protected AbstractResponse()
    {
        Status = Status.Ok();
    }
}