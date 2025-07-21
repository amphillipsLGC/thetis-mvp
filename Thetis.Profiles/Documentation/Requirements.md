# Thetis.Profiles Requirements

This document outlines the core requirements and entity relationships for the `Thetis.Profiles` library.

## Profile

- Represents a clinical narrative profile.
- Can be **private** or **public**.
- Must have an **owner** (`ProfileOwner`).
- Contains one or more **data requirements**.

## ProfileOwner

- Stores user information for the profile owner.
- Linked to a single profile.

## DataRequirement

- Defines a requirement for a specific resource type.
- Associated with one profile.
- Contains one or more **rules**.

## DataRequirementRule

- Specifies a property, operator, and value for validation.
- Linked to a single data requirement.

## RuleOperator

- Enumeration of supported operators (e.g., `Equals`, `NotEquals`, `Contains`).

---

### Example Entity Relationship

- **Profile** (1) --- (1) **ProfileOwner**
- **Profile** (1) --- (N) **DataRequirement**
- **DataRequirement** (1) --- (N) **DataRequirementRule**

---
