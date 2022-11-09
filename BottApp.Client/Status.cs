using System.ComponentModel.DataAnnotations;

namespace BottApp.Client;

public sealed class Status
{
    [Required]
    public ErrorCode Code;


    public static Status Ok()
    {
        return new Status()
        {
            Code = ErrorCode.NoError
        };
    }
}