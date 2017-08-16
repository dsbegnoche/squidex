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

import {
    ApiUrlConfig,
    HTTP
} from 'framework';

export class CpProductsDto {
    constructor(
        public readonly id: string,
        public readonly name: string
    ) {
    }
}

@Injectable()
export class CpProductsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getPoducts(): Observable<CpProductsDto[]> {
        const url = this.apiUrl.buildUrl('/api/cptoolbar/products');
        console.log('afterURL');

        return HTTP.getVersioned(this.http, url)
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new CpProductsDto(
                            item.id,
                            item.name);
                    });
                })
                .pretifyError('Failed to load products. Please reload.');
    }
}