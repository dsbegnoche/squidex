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
    AuthService,
    CpHelpLinksService,
    CpHelpLinksDto,
    fadeAnimation,
    ModalView
} from 'shared';

@Component({
    selector: 'cp-profile-menu',
    templateUrl: './cp-profile-menu.component.html',
    animations: [
        fadeAnimation
    ]
})
export class CpProfileMenuComponent implements OnDestroy, OnInit {
    private authenticationSubscription: Subscription;
    private helplinksSubscription: Subscription;

    public modalMenu = new ModalView(false, true);

    public profileDisplayName = '';
    public profileId = '';

    public isAdmin = false;

    public profileUrl = this.apiUrl.buildUrl('/identity-server/account/profile');
    public resetPasswordLink: CpHelpLinksDto[] = [];
    public resetPasswordUrl = '';
    public resetPasswordReturnUrl = window.location.href;

    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig,
        private readonly helplinksService: CpHelpLinksService
    ) {
    }

    public ngOnDestroy() {
        this.authenticationSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.authenticationSubscription =
            this.authService.userChanges.filter(user => !!user)
                .subscribe(user => {
                    this.profileId = user.id;
                    this.profileDisplayName = user.displayName;

                    this.isAdmin = user.isAdmin;
                });

        this.helplinksSubscription = this.helplinksService.getResetPasswordUrl().subscribe(resetPasswordLink => {
            this.resetPasswordLink = resetPasswordLink;
            this.resetPasswordUrl = resetPasswordLink[0].url + this.resetPasswordReturnUrl;
        });
    }

    public logout() {
        this.authService.logoutRedirect();
    }
}