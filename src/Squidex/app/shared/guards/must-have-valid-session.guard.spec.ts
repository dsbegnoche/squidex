// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

import { IMock, Mock } from 'typemoq';
import { Observable } from 'rxjs';

import { AuthService } from 'shared';

import { MustHaveValidSessionGuard } from './must-have-valid-session.guard';

describe('MustHaveValidSessionGuard', () => {
    let authService: IMock<AuthService>;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
    });

    it('should navigate to login page if session not valid', (done) => {
        authService.setup(x => x.isValidSession())
            .returns(() => Observable.of(false));

        const guard = new MustHaveValidSessionGuard(authService.object);

        guard.canActivate(<any>{}, <any>{})
            .subscribe(result => {
                expect(result).toBeFalsy();

                done();
            });
    });

    it('should return true if session is valid', (done) => {
        authService.setup(x => x.isValidSession())
            .returns(() => Observable.of(true));

        const guard = new MustHaveValidSessionGuard(authService.object);

        guard.canActivate(<any>{}, <any>{})
            .subscribe(result => {
                expect(result).toBeTruthy();

                done();
            });
    });
});