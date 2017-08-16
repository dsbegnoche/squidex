/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    ApiUrlConfig,
    CpProductsService,
    fadeAnimation,
    CpProductsDto
} from 'shared';

@Component({
    selector: 'cp-products-menu',
    templateUrl: './cp-products-menu.component.html',
    animations: [
        fadeAnimation
    ]
})
export class CpProductsMenuComponent implements OnDestroy, OnInit {
    public productsSubscription: Subscription;

    public products: CpProductsDto[] = [];
    public productUrl = this.apiUrl.buildUrl('/api/cptoolbar/products');

    constructor(
        private readonly productService: CpProductsService,
        private readonly apiUrl: ApiUrlConfig
    ) {
        console.log(this.productUrl);
    }

    public ngOnDestroy() {
        console.log('destroy');
    }

    public ngOnInit() {
        this.productsSubscription =
            this.productService.getPoducts().subscribe(products => {
                this.products = products;
            });
    }
}