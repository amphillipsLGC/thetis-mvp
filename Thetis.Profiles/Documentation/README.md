# Thetis.Profiles

Thetis.Profiles is a C# library for managing clinical narrative profiles and their associated data requirements. It provides a structured way to define profiles, owners, and rules for data access or validation.

## Features

- Define clinical narrative profiles with metadata and ownership
- Specify data requirements for each profile
- Create rules for data requirements using various operators

## Entity Overview

- **Profile**: Represents a clinical narrative profile, including name, description, visibility, owner, and data requirements.
- **ProfileOwner**: Contains user information for the profile owner.
- **DataRequirement**: Specifies a resource type and a set of rules.
- **DataRequirementRule**: Defines a property, operator, and value for data validation.
- **RuleOperator**: Enumeration of supported rule operators (Equals, NotEquals, Contains, etc.).

## Database Migrations

To set up the database for Thetis.Profiles, you need to run the Entity Framework Core migrations. Ensure you have the necessary packages installed and then execute the following command in your terminal 
from the Thetis.Web project directory:

```shell
dotnet ef migrations add Initial -c ProfileDbContext -p ../Thetis.Profiles/Thetis.Profiles.csproj -s ./Thetis.Web.csproj -o Data/Migrations
```

To apply the migrations to your database, run:

```shell
dotnet ef database update -c ProfileDbContext
```

## Example Usage

```csharp
var profile = new Profile
{
    Id = Guid.NewGuid(),
    UserId = userId,
    Name = "Sample Profile",
    IsPublic = true,
    Owner = new ProfileOwner { UserId = userId, FirstName = "John", LastName = "Doe" },
    DataRequirements = new List<DataRequirement>
    {
        new DataRequirement
        {
            Id = Guid.NewGuid(),
            ResourceType = "Document",
            Rules = new List<DataRequirementRule>
            {
                new DataRequirementRule
                {
                    PropertyName = "Status",
                    Operator = RuleOperator.Equals,
                    OperatorValue = "Approved"
                }
            }
        }
    }
};