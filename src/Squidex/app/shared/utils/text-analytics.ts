/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

let uuidv4 = require('uuid/v4');

export class AzureDocumentDto {
    constructor(
        public readonly id: string,
        public readonly language: string,
        public readonly text: string
    ) {
    }
}

export class AzureAnalyticsDto {
    constructor(
        public readonly documents: AzureDocumentDto[]
    ) {
    }
}

@Injectable()
export class TextAnalyticsService {
    private readonly key = '489ad04269e24d9481ac546f4c027e67';
    private body: AzureAnalyticsDto;
    private documents: AzureDocumentDto[] = [];

    constructor(
        private readonly http: HttpClient
    ) {
    }

    public getKeyPhrases(bodyText: string): Observable<string[]> {
        const url = `https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases`;
        const options = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json', 'Ocp-Apim-Subscription-Key': this.key
            })
        };

        this.documents[0] = new AzureDocumentDto(uuidv4(), 'en', bodyText);
        this.body = new AzureAnalyticsDto(this.documents);

        return this.http.post(url, this.body, options)
            .map((response: any) => {
                let result: string[] = [];

                result = response.documents[0]!.keyPhrases;

                return result;
            })
            .catch(error => Observable.of([]));
    }
}