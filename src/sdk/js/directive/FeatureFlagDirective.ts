'use strict'
export class FeatureFlagDirective {
    static featureFlag(featureFlagService): angular.IDirective {
        return {
            restrict: 'AE',
            scope: {
                feature: '@',
                hiddenIfActive: '=?',
                appName: '@',
                env: '@',
                params: '=?'
            },
            transclude: true,
            template: '<div ng-if="enabled" ng-transclude></div>',
            link: (scope: any) => {
                var params = scope.params || {};

                scope.active = false;
                scope.enabled = !scope.hiddenIfActive;

                featureFlagService.getFeatureFlags(scope.appName, scope.env, [scope.feature], params)
                    .then((response: any): void => {
                        scope.active = response[scope.feature];
                    });

                scope.$watch('active', () => {
                    scope.enabled = (scope.active && !scope.hiddenIfActive) || (!scope.active && scope.hiddenIfActive);

                })
            }
        }
    }
}
