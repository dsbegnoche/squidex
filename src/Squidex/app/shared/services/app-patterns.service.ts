/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import { ApiUrlConfig,
    HTTP,
    Version
    } from 'framework';

export interface AppPatternsDto {
    regexSuggestions: AppPatternsSuggestionDto[];
}

export class AppPatternsSuggestionDto {
    public name: string;
    public pattern: string;
    public defaultMessage: string;

    constructor(name: string, pattern: string, message: string) {
        this.name = name;
        this.pattern = pattern;
        this.defaultMessage = message;
    }
}

@Injectable()
export class AppPatternsService {
    private settings: AppPatternsDto;

    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getPatterns(appName: string): Observable<AppPatternsDto> {
        if (this.settings) {
            return Observable.of(this.settings);
        } else {
            const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns`);

            return this.http.get<AppPatternsDto>(url)
                .catch(error => {
                    return Observable.of({ regexSuggestions: [] });
                })
                .do(settings => {
                    this.settings = settings;
                });
        }
    }

    public postPattern(appName: string, dto: AppPatternsSuggestionDto, version: Version): Observable<AppPatternsSuggestionDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns`);

        return HTTP.postVersioned(this.http, url, dto, version)
            .map(response => {
                return new AppPatternsSuggestionDto(
                    response.name || response.id,
                    response.pattern,
                    response.defaultMessage);
            })
            .pretifyError('Failed to add pattern. Please reload.');
    }

    public deletePattern(appName: string, name: string, version: Version): Observable<AppPatternsSuggestionDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/patterns/${name}`);

        return HTTP.deleteVersioned(this.http, url, version)
            .pretifyError('Failed to revoke client. Please reload.');
    }
}