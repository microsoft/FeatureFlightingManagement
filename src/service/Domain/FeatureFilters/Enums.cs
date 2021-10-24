namespace Microsoft.FeatureFlighting.Core.FeatureFilters
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
        NotMemberOfSecurityGroup,
        Evaluates,
        NotEvaluates
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
        RulesEngine,
        Generic
    }
}
