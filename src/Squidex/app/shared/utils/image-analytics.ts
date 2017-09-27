/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { ApiUrlConfig } from 'framework';


export class AzureAssetDto {
    constructor(
        public readonly id: string, \
    ) {
    }
}

@Injectable()
export class ImageAnalyticsService {
    private readonly key = '2b7aeed3711945b687a5342e0508a113';
    private readonly minimumConfidence = 0.8;

    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig, \
    ) {
    }

    public getTags(assetId: string): Promise<string[]> {
        const url = 'https://westus.api.cognitive.microsoft.com/vision/v1.0/tag';
        const options = {
            headers: new HttpHeaders({
                'Ocp-Apim-Subscription-Key': this.key
            })
        };

        // get the asset, then make a request with body
        return this.getAsset(assetId).then(
            asset => {
                return this.http.post(url, this.getFormData(asset), options).toPromise().then((result: any) =>
                    result.tags
                        .where((item: any) => item.confidence > this.minimumConfidence)
                        .map((item: any) => item.name));
            });
    }

    // couldn't find a place where we do this currently.
    private getAsset(assetId: string): Promise<any> {
        return this.http.get(this.apiUrl.buildUrl(`api/assets/${assetId}`)).toPromise();
    }

    private getFormData(file: File) {
        const formData = new FormData();
        formData.append('file', file);
        return formData;
    }
}
