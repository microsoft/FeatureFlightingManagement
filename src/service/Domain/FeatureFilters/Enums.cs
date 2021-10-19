namespace Microsoft.FeatureFlighting.Domain.FeatureFilters
{
    public enum Operator
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        In,
        NotIn,
        MemberOfCustomGroup,
        NotMemberOfCustomGroup,
        MemberOfSecurityGroup,
        NotMemberOfSecurityGroup
    }

    public enum Filters
    {
        Alias,
        RoleGroup,
        Date,
        Country,
        Region,
        Role,
        UserUpn,
        Generic
    }
}
