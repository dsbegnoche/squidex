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
    public productCount = 0;
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
        this.productCount = this.getProducts();
    }

    public getProducts() {
        this.productsSubscription =
            this.productService.getProducts().subscribe(products => {
                this.products = products;
                this.productCount = products.length;
            });

        return this.products.length;
    }
}