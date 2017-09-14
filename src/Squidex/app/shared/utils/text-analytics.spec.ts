/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    ApiUrlConfig,
    TextAnalyticsService
} from './../';

describe('TextAnalyticsService', () => {

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                TextAnalyticsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make a post request with correct key',
        inject([TextAnalyticsService, HttpTestingController],
            (textAnalyticsService: TextAnalyticsService, httpMock: HttpTestingController) => {

                let tags: string[];

                textAnalyticsService.getKeyPhrases('only article will return')
                    .then((result: string[]) => {
                        tags = result;
                    });

                const req = httpMock.expectOne('https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases');

                expect(req.request.method).toEqual('POST');
                expect(req.request.headers.get('Ocp-Apim-Subscription-Key')).toEqual('489ad04269e24d9481ac546f4c027e67');

                req.flush({
                    'documents': [
                        {
                            'keyPhrases': [
                                'article'
                            ],
                            'id': 'id'
                        }
                    ],
                    'errors': []
                });
            }));
});