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

import { ApiUrlConfig, HTTP } from 'framework';

export class UsersDto {
    constructor(
        public readonly total: number,
        public readonly items: UserDto[]
    ) {
    }
}

export class UserDto {
    constructor(
        public readonly id: string,
        public readonly email: string,
        public readonly displayName: string,
        public readonly pictureUrl: string | null,
        public readonly isLocked: boolean,
        public readonly firstName: string,
        public readonly lastName: string,
        public readonly isAdministrator: boolean | false
    ) {
    }

    public update(email: string, displayName: string, firstName: string, lastName: string, isAdministrator: boolean): UserDto {
        return new UserDto(this.id, email, displayName, this.pictureUrl, this.isLocked, firstName, lastName, this.isAdministrator);
    }

    public lock(): UserDto {
        return new UserDto(this.id, this.email, this.displayName, this.pictureUrl, true, this.firstName, this.lastName, this.isAdministrator);
    }

    public unlock(): UserDto {
        return new UserDto(this.id, this.email, this.displayName, this.pictureUrl, false, this.firstName, this.lastName, this.isAdministrator);
    }
}

export class CreateUserDto {
    constructor(
        public readonly email: string,
        public readonly displayName: string,
        public readonly password: string,
        public readonly firstName: string,
        public readonly lastName: string,
        public readonly isAdministrator: boolean | false
    ) {
    }
}

export class UpdateUserDto {
    constructor(
        public readonly email: string,
        public readonly displayName: string,
        public readonly password: string,
        public readonly firstName: string,
        public readonly lastName: string,
        public readonly isAdministrator: boolean | false
    ) {
    }
}

@Injectable()
export class UsersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getUsers(query?: string): Observable<UserDto[]> {
        const url = this.apiUrl.buildUrl(`api/users?query=${query || ''}`);

        return HTTP.getVersioned(this.http, url)
            .map(response => {
                const items: any[] = response;

                return items.map(item => {
                    return new UserDto(
                        item.id,
                        item.email,
                        item.displayName,
                        item.pictureUrl,
                        item.isLocked,
                        item.firstName,
                        item.lastName,
                        item.isAdministrator);
                });
            })
            .pretifyError('Failed to load users. Please reload.');
    }

    public getUser(id: string): Observable<UserDto> {
        const url = this.apiUrl.buildUrl(`api/users/${id}`);

        return HTTP.getVersioned(this.http, url)
            .map(response => {
                return new UserDto(
                    response.id,
                    response.email,
                    response.displayName,
                    response.pictureUrl,
                    response.isLocked,
                    response.firstName,
                    response.lastName,
                    response.isAdministrator);
            })
            .pretifyError('Failed to load user. Please reload.');
    }
}

@Injectable()
export class UserManagementService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getUsers(take: number, skip: number, query?: string): Observable<UsersDto> {
        const url = this.apiUrl.buildUrl(`api/user-management?take=${take}&skip=${skip}&query=${query || ''}`);

        return HTTP.getVersioned(this.http, url)
            .map(response => {
                const items: any[] = response.items;

                const users = items.map(item => {
                    return new UserDto(
                        item.id,
                        item.email,
                        item.displayName,
                        item.pictureUrl,
                        item.isLocked,
                        item.firstName,
                        item.lastName,
                        item.isAdministrator);
                });

                return new UsersDto(response.total, users);
            })
            .pretifyError('Failed to load users. Please reload.');
    }

    public getUser(id: string): Observable<UserDto> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}`);

        return HTTP.getVersioned(this.http, url)
            .map(response => {
                return new UserDto(
                    response.id,
                    response.email,
                    response.displayName,
                    response.pictureUrl,
                    response.isLocked,
                    response.firstName,
                    response.lastName,
                    response.isAdministrator);
            })
            .pretifyError('Failed to load user. Please reload.');
    }

    public postUser(dto: CreateUserDto): Observable<UserDto> {
        const url = this.apiUrl.buildUrl('api/user-management');

        return HTTP.postVersioned(this.http, url, dto)
            .map(response => {
                return new UserDto(response.id, dto.email, dto.displayName, response.pictureUrl, false, dto.firstName, dto.lastName, response.isAdministrator);
            })
            .pretifyError('Failed to create user. Please reload.');
    }

    public putUser(id: string, dto: UpdateUserDto): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}`);

        return HTTP.putVersioned(this.http, url, dto)
            .pretifyError('Failed to update user. Please reload.');
    }

    public lockUser(id: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}/lock`);

        return HTTP.putVersioned(this.http, url, {})
            .pretifyError('Failed to load users. Please retry.');
    }

    public unlockUser(id: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}/unlock`);

        return HTTP.putVersioned(this.http, url, {})
            .pretifyError('Failed to load users. Please retry.');
    }
}
