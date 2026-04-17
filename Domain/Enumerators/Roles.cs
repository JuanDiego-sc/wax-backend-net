namespace Domain.Enumerators;

public static class Roles
{
    public const string Admin =  "Admin";
    public const string Enrolled = "Enrolled";
    public const string Registered = "Registered";
    

    public static readonly IReadOnlyList<string> All = [Admin, Enrolled, Registered];
    
}