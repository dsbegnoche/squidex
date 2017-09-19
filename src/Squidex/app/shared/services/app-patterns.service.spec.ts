/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    ApiUrlConfig,
    AppPatternsService,
    AppPatternsDto,
    AppPatternsSuggestionDto,
    Version
} from './../';

describe('AppPatternsService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                AppPatternsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get patterns',
        inject([AppPatternsService, HttpTestingController], (patternService: AppPatternsService, httpMock: HttpTestingController) => {

        let settings1: AppPatternsDto | null = null;
        let settings2: AppPatternsDto | null = null;

        patternService.getPatterns('my-app').subscribe(result => {
            settings1 = result;
        });

        const response: AppPatternsDto = { regexSuggestions: [] };

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/patterns');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush(response);

        patternService.getPatterns('my-app').subscribe(result => {
            settings2 = result;
        });

        expect(settings1).toEqual(response);
        expect(settings2).toEqual(response);
    }));

    it('should return default patterns when error occurs',
        inject([AppPatternsService, HttpTestingController], (patternService: AppPatternsService, httpMock: HttpTestingController) => {

        let settings: AppPatternsDto | null = null;

        patternService.getPatterns('my-app').subscribe(result => {
            settings = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/patterns');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.error(new ErrorEvent('500'));

        expect(settings).toBeDefined();
        expect(settings!.regexSuggestions).toEqual([]);
    }));

    it('should make post request to add pattern',
        inject([AppPatternsService, HttpTestingController], (patternService: AppPatternsService, httpMock: HttpTestingController) => {

            let returnValue: AppPatternsSuggestionDto | null = null;
            const pattern1: AppPatternsSuggestionDto = new AppPatternsSuggestionDto('pattern1', '[0-9]', 'message');

            patternService.postPattern('my-app', pattern1, new Version()).subscribe(result => {
                returnValue = result;
            });

            const response: AppPatternsSuggestionDto = pattern1;

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/patterns');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('E-Tag')).toBeNull();

            req.flush(pattern1);
            expect(returnValue).toEqual(response);
        }));

    it('should make delete request to remove pattern',
        inject([AppPatternsService, HttpTestingController], (patternService: AppPatternsService, httpMock: HttpTestingController) => {

            patternService.deletePattern('my-app', 'pattern1', new Version('1')).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/patterns/pattern1');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toEqual(new Version('1').value);

            req.flush({});
        }));
});