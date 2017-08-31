/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    ApiUrlConfig,
    CpProductsService,
    CpProductsDto,
    fadeAnimation,
    ModalView
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
    public modalMenu = new ModalView(false, true);

    public products: CpProductsDto[] = [];
    public productUrl = this.apiUrl.buildUrl('/api/cptoolbar/products');

    constructor(
        private readonly productService: CpProductsService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public ngOnDestroy() {
        this.products = [];
    }

    public ngOnInit() {
        this.productsSubscription =
            this.productService.getProducts().retryWhen(err => {
                return err.delay(1000);
            }).takeLast(1).subscribe(products => {
                this.products = products;
            });
    }
}