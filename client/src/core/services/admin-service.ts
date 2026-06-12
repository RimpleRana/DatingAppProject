import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { User } from '../../types/user';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;
  
  getUserWithRoles() {
    return this.http.get<User[]>(this.baseUrl + 'admin/user-with-roles');
  }

  updateUserRoles(userId: string, roles: string[]) {
    return this.http.post<string[]>(this.baseUrl + 'admin/edit-roles/' + userId + '?roles=' + roles, {})
  }
}
