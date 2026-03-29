namespace Domain.Enumerators;

public static class Roles
{
    public const string Admin =  "Admin";
    public const string Member = "Member";

    public static readonly IReadOnlyList<string> All = [Admin, Member];
}