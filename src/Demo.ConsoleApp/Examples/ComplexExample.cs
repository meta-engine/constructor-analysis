namespace Demo.ConsoleApp.Examples;

public enum Role
{
    Admin,
    User,
    Guest
}

public class Person
{
    public string FirstName { get; }
    public string LastName { get; }
    public Role Role { get; }

    protected Person(string firstName, string lastName, Role role)
    {
        FirstName = firstName;
        LastName = lastName;
        Role = role;
    }
}

public class Employee : Person
{
    public int EmployeeId { get; }
    public string Department { get; }
    public Role AccessLevel { get; }

    public Employee(int employeeId, string firstName, string lastName, Role userRole, string department)
        : base(firstName, lastName, userRole)
    {
        EmployeeId = employeeId;
        Department = department;
        AccessLevel = userRole;
    }
}

