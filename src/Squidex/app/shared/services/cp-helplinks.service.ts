/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import {
    ApiUrlConfig,
    HTTP
    } from 'framework';

export class CpHelpLinksDto {
    constructor(
        public readonly id: string,
        public readonly title: string,
        public readonly url: string
    ) {
    }
}

@Injectable()
export class CpHelpLinksService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getHelplinks(): Observable<CpHelpLinksDto[]> {
        const url = this.apiUrl.buildUrl('/api/cptoolbar/helplinks');

        return HTTP.getVersioned<any>(this.http, url)
            .map(response => {
                const body = response.payload.body;

                const items: any[] = body;

                return items.map(item => {
                    return new CpHelpLinksDto(
                        item.id,
                        item.title,
                        item.url);
                });
            })
            .pretifyError('Failed to load help links. Please reload.');
    }

    public getResetPasswordUrl(): Observable<CpHelpLinksDto[]> {
        const url = this.apiUrl.buildUrl('/api/cptoolbar/reset-password');

        return HTTP.getVersioned<any>(this.http, url)
            .map(response => {
                const body = response.payload.body;

                const items: any[] = body;

                return items.map(item => {
                    return new CpHelpLinksDto(
                        item.id,
                        item.title,
                        item.url);
                });
            })
            .pretifyError('Failed to load reset password link. Please reload.');
    }
}