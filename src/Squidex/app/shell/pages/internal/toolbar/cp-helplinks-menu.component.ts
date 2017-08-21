/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    ApiUrlConfig,
    CpHelpLinksService,
    CpHelpLinksDto,
    fadeAnimation,
    ModalView
    } from 'shared';

@Component({
    selector: 'cp-helplinks-menu',
    templateUrl: './cp-helplinks-menu.component.html',
    animations: [
        fadeAnimation
    ]
})
export class CpHelpLinksMenuComponent implements OnDestroy, OnInit {
    public helplinksSubscription: Subscription;
    public modalMenu = new ModalView(false, true);

    public helplinks: CpHelpLinksDto[] = [];
    public helplinksUrl = this.apiUrl.buildUrl('/api/cptoolbar/helplinks');

    constructor(
        private readonly helplinksService: CpHelpLinksService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public ngOnDestroy() {
        this.helplinks = [];
    }

    public ngOnInit() {
        this.helplinksSubscription =
            this.helplinksService.getHelplinks().subscribe(helplinks => {
                this.helplinks = helplinks;
            });
    }
}