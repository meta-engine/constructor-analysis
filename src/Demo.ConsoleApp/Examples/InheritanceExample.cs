namespace Demo.ConsoleApp.Examples;

public class BaseEntity
{
    public Guid Id { get; }
    public string Name { get; }

    protected BaseEntity(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}

public class User : BaseEntity
{
    public string Email { get; }
    public string Username { get; }

    public User(string email, Guid userId, string userName)
        : base(userId, userName)
    {
        Email = email;
        Username = userName;
    }
}

