import { Pipe, PipeTransform } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { AuthenticationService } from '../services/authentication.service';

@Pipe({
    name: 'authImage'
})
export class AuthPipe implements PipeTransform {

    constructor(
        private http: HttpClient,
        private auth: AuthenticationService, // our service that provides us with the authorization token
    ) {}

    async transform(src: string): Promise<string> {
        const token = this.auth.currentUserValue.token;
        const headers = new HttpHeaders({ Authorization: `Bearer ${token}`});
        const imageBlob = await this.http.get(src, {headers, responseType: 'blob'}).toPromise();

        const reader = new FileReader();
        return new Promise((resolve, reject) => {
            reader.onloadend = () => resolve(reader.result as string);
            reader.readAsDataURL(imageBlob);
        });
    }

}
