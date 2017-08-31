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

export class CpProductsDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly description: string,
        public readonly logoUri: string,
        public readonly logoutUri: string,
        public readonly productUri: string,
        public readonly currentProduct: string,
        public readonly isProductHidden: string
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

    public getProducts(): Observable<CpProductsDto[]> {
        const url = this.apiUrl.buildUrl('/api/cptoolbar/products');
        return HTTP.getVersioned(this.http, url)
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new CpProductsDto(
                            item.id,
                            item.name,
                            item.description,
                            item.logoUri,
                            item.logoutUri,
                            item.productUri,
                            item.currentProduct,
                            item.isProductHidden);
                    });
                })
                .pretifyError('Failed to load products. Please reload.');
    }
}