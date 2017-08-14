/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { IMock, Mock } from 'typemoq';
import { Observable } from 'rxjs';

import { AuthService } from 'shared';

import { MustBeAuthenticatedGuard } from './must-be-authenticated.guard';

describe('MustBeAuthenticatedGuard', () => {
    let authService: IMock<AuthService>;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
    });

    it('should navigate to default page if not authenticated', (done) => {
        authService.setup(x => x.userChanges)
            .returns(() => Observable.of(null));

        const guard = new MustBeAuthenticatedGuard(authService.object);

        guard.canActivate(<any>{}, <any>{})
            .subscribe(result => {
                expect(result).toBeFalsy();

                done();
            });
    });

    it('should return true if authenticated', (done) => {
        authService.setup(x => x.userChanges)
            .returns(() => Observable.of(<any>{}));

        const guard = new MustBeAuthenticatedGuard(authService.object);

        guard.canActivate(<any>{}, <any>{})
            .subscribe(result => {
                expect(result).toBeTruthy();

                done();
            });
    });
});