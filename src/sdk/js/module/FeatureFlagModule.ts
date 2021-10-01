import {FeatureFlagServiceProvider} from '../service/FeatureFlagServiceProvider'
import {FeatureFlagDirective} from '../directive/FeatureFlagDirective'
import * as angular from 'angular'
export class FeatureFlagAngular1Module {
        constructor() {
        }
        private static _moduleName = "Microsoft.PS.Flighting";
        static registerModule() {
                angular.module(this._moduleName, [])
                        .provider('FeatureFlagService', FeatureFlagServiceProvider)
                        .directive('featureFlag', ['FeatureFlagService', FeatureFlagDirective.featureFlag]);
        }
       

        static get FlightingNg1ModuleName() {
                return this._moduleName;
        }


}
            