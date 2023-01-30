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
        NotEvaluates,
        ArrayAll,
        ArrayAny,
        NotArrayAll,
        NotArrayAny
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

    public enum ComplexFilters
    {
        RulesEngine
    }
}
