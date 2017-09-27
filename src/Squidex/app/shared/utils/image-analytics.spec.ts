/*
 * CivicPlus implementation of Squidex Headless CMS
 */
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    ApiUrlConfig,
    ImageTagService
} from './../';

describe('ImageTagService',
    () => {
        beforeEach(() => {
            TestBed.configureTestingModule({
                imports: [
                    HttpClientTestingModule
                ],
                providers: [
                    ImageTagService,
                    {
                        provide: ApiUrlConfig,
                        useValue: new ApiUrlConfig('http://testspec.local')
                    }
                ]
            });
        });

        afterEach(inject([HttpTestingController],
            (httpMock: HttpTestingController) => {
                httpMock.verify();
            }));

        it('Should request and sort tags for an asset',
            inject([ImageTagService, HttpTestingController],
                (imageTagService: ImageTagService, httpMock: HttpTestingController) => {
                    let tags: string[];
                    let assetId = 'fakeId';

                    imageTagService.getTags(assetId)
                        .then((result: string[]) => {
                            tags = result;

                            // tags with lower confidence level should be removed
                            expect(tags).toEqual(['testtag']);
                        });

                    // get the asset file
                    const reqA = httpMock.expectOne(
                        `http://testspec.local/api/assets/${assetId}`);

                    expect(reqA.request.method).toEqual('GET');
                    reqA.flush({ 'file': 'totally a file' });

                    // get the tags
                    const req = httpMock.expectOne(
                        'https://westus.api.cognitive.microsoft.com/vision/v1.0/tag');

                    expect(req.request.method).toEqual('POST');
                    expect(req.request.headers.get('Ocp-Apim-Subscription-Key'))
                        .toEqual('2b7aeed3711945b687a5342e0508a113');

                    req.flush({
                        'tags': [
                            {
                                'name': 'testtag',
                                'confidence': 0.9853853583335876
                            },
                            {
                                'name': 'testtag2',
                                'confidence': 0.7736158967018127
                            }
                        ],
                        'requestId': 'b8e9860d-ad08-4c05-af6c-edf5f754636d',
                        'metadata': {
                            'width': 3264,
                            'height': 1836,
                            'format': 'Jpeg'
                        }
                    });

                }));
    });
