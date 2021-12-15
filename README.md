# Feature Flighting Management System
Feature toggles is a technique using which you can turn "on" or "off" a feature without a complete system deployment. It's a great technique for controlled feature rollout, conducting A/B tests and implementing Canary cohorts.
This is a centralized feature flighting management system which leverages Azure App Configuration's Feature Management.

## Capabilities
1. REST APIs for evaluating Feature Flags
2. Support for multi-tenancy (without additional infrastructure)
3. Ring-based feature rollout
4. REST APIs for managing feature flightings (CRUD)
5. Integration with Graph API for evaluating flags based on Groups

## References
- ### [Control Ring Rollout](https://www.devcompost.com/post/control-rollout-1)
- ### [Designing a Centralized Feature flighting system](https://www.devcompost.com/post/control-rollout-2)
- ### [Official Documentation](https://github.com/microsoft/FeatureFlightingManagement/wiki)

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
