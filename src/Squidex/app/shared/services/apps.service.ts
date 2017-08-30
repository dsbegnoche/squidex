/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

// import { HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import {
    ApiUrlConfig,
    DateTime,
    HTTP,
    PermissionEnum
} from 'framework';

export class AppDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly permission: PermissionEnum,
        public readonly created: DateTime,
        public readonly lastModified: DateTime
    ) {
    }
}

export class CreateAppDto {
    constructor(
        public readonly name: string
    ) {
    }
}

@Injectable()
export class AppsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getApps(): Observable<AppDto[]> {
        const url = this.apiUrl.buildUrl('/api/apps');

        return HTTP.getVersioned(this.http, url)
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new AppDto(
                            item.id,
                            item.name,
                            item.permission,
                            DateTime.parseISO(item.created),
                            DateTime.parseISO(item.lastModified));
                    });
                })
                .pretifyError('Failed to load apps. Please reload.');
    }

    public postApp(dto: CreateAppDto, now?: DateTime): Observable<AppDto> {
        const url = this.apiUrl.buildUrl('api/apps');

        return HTTP.postVersioned(this.http, url, dto)
                .map(response => {
                    now = now || DateTime.now();

                    return new AppDto(response.id, dto.name, PermissionEnum.Owner, now, now);
                })
                .pretifyError('Failed to create app. Please reload.');
    }

    public deleteApp(appName: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}`);

        return HTTP.deleteVersioned(this.http, url)
            .map((response) => {
                return appName;
            })
            .pretifyError('Failed to delete app. Please reload.');
    }
}
