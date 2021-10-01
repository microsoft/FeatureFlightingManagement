import { IFeatureFlagConfiguration } from '../interface/IFeatureFlagConfiguration'
import * as angular from 'angular'
export class FeatureFlagService {
    static $inject = ['$http'];
    private endpoint: string;
    private q: angular.IQService;
    constructor(private $http: angular.IHttpService, featureFlagContext: IFeatureFlagConfiguration, public context: any, $q: angular.IQService) {
        this.endpoint = featureFlagContext.ServiceEndpoint;
        this.q = $q;
    }

    public getFeatureFlags(appName, env, features: string[], params = {}, cacheType = 'noCache'): angular.IPromise<any> {
        if (features === undefined || features === null || features.length < 1)
            throw new Error("features parameter is empty");
        var deferred = this.q.defer();
        var FeatureFlagsResponse = this.getCachedFlagResponse(appName, features, params,cacheType);
        var FeatureFlagsResult = FeatureFlagsResponse["FeatureFlagsResult"];
        var evaluateFeatures = FeatureFlagsResponse["Features"];
        if (evaluateFeatures.length !== 0) {
            this.getResponseFromAPI(appName, env, evaluateFeatures, params).then((response) => {
                var resultArray = Object.entries(response);
                resultArray.forEach(([key, value]) => {
                    this.storeResponseinCache(appName, key, value, params,cacheType);
                    FeatureFlagsResult[key] = value;
                });
                deferred.resolve(FeatureFlagsResult);
            }, (error: any): any => {
                console.error(error);
                deferred.reject(error);
            });
        }
        else
            deferred.resolve(FeatureFlagsResult);

        return deferred.promise;
    }
    private getCacheKey(appName,key,params){
        
        if(params== null || Object.keys(params).length === 0)
         return("FEATUREFLAG:" + appName.toUpperCase() + ":" + key.toUpperCase());

        return("FEATUREFLAG:" + appName.toUpperCase() + ":" + key.toUpperCase()+":"+JSON.stringify(params));
       
    }
    private storeResponseinCache(appName, key, value,params, cacheType) {
        var index = this.getCacheKey(appName,key,params);
         if (cacheType === 'sessionStorage')
            sessionStorage[index] = value;
        else if (cacheType === 'localStorage')
            localStorage[index] = value;
    }
    private getResponseFromAPI(appName, env, features: string[], params = {}): angular.IPromise<any> {
        var url = this.endpoint + '/Evaluate?featureNames=' + features.join();
        var config: angular.IRequestShortcutConfig;
        if (params || this.context) {
            params = params || {};
            angular.extend(params, this.context);
            config = {
                headers: {
                    'X-FlightContext': JSON.stringify(params),
                    'X-Application': appName,
                    'X-Environment': env
                }
            }
        }

        var promise = this.$http.get(url, config)
            .then((response: any): angular.IPromise<any> => {
                return response.data;
            }, (error: any): any => {
                console.error(error);
            });
        return promise;
    }
    private getCachedFlagResponse(appName, features, params,cacheType): any {
        var FlagNotCached = new Array();
        var FeatureFlagsResult = {};
        features.forEach(flag => {
            let index = this.getCacheKey(appName,flag,params);
              if (cacheType === 'sessionStorage') {
                if (sessionStorage[index] !== undefined && sessionStorage[index] !== null)
                    FeatureFlagsResult[flag] = (sessionStorage[index].toUpperCase()==="TRUE");
                else
                    FlagNotCached.push(flag);
            }
            else if (cacheType === 'localStorage') {
               if (localStorage[index] !== undefined && localStorage[index] !== null) {
                    FeatureFlagsResult[flag] = (localStorage[index].toUpperCase()==="TRUE");
                }
                else
                    FlagNotCached.push(flag);
            }
            else {
                FlagNotCached.push(flag);
            }
        });
        
        return { 'Features': FlagNotCached, 'FeatureFlagsResult': FeatureFlagsResult };
    

    }

}

export class FeatureFlagServiceProvider implements angular.IServiceProvider {
    private _featureFlagConfig: IFeatureFlagConfiguration;
    private _context: any;

    constructor() {
        this.$get.$inject = ['$http', '$q']
    }

    $get($http: angular.IHttpService, $q: angular.IQService) {
        return new FeatureFlagService($http, this._featureFlagConfig, this._context, $q);
    }

    public configure(featureFlagConfig: IFeatureFlagConfiguration, context: any): void {
        this._featureFlagConfig = featureFlagConfig;
        this._context = context;
    }
}

