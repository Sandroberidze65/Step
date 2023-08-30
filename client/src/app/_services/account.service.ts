import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { User } from '../_models/user';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private curentUserSource = new BehaviorSubject<User | null>(null);
  currentUser$ = this.curentUserSource.asObservable();

  constructor(private http: HttpClient ) { }

  login (model: any){
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map((responce: User)=> {
        const user = responce;
        if(user){
          this.setCurrentUser(user);
        }
      })
    )
  }

  register(model: any){
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map(user => {
        if(user){
          this.setCurrentUser(user);
        }
      }) 
    )
  }

  setCurrentUser(user: User){
    localStorage.setItem('user', JSON.stringify(user));
    this.curentUserSource.next(user);
  }

  logout(){
    localStorage.removeItem('user');
    this.curentUserSource.next(null);
  }
}
