/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import { ApiUrlConfig } from 'framework';

export interface UISettingsDto {
    regexSuggestions: UIRegexSuggestionDto[];
}

export class UIRegexSuggestionDto {
    public name: string;
    public pattern: string;
    public message: string;

    constructor(name: string, pattern: string, message: string) {
        this.name = name;
        this.pattern = pattern;
        this.message = message;
    }
}

@Injectable()
export class UIService {
    private settings: UISettingsDto;

    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getSettings(appName: string): Observable<UISettingsDto> {
        if (this.settings) {
            return Observable.of(this.settings);
        } else {
            const url = this.apiUrl.buildUrl(`api/ui/${appName}/settings`);

            return this.http.get<UISettingsDto>(url)
                .catch(error => {
                    return Observable.of({ regexSuggestions: [] });
                })
                .do(settings => {
                    this.settings = settings;
                });
        }
    }
}