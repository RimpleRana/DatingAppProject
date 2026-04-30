import { Component, inject, OnInit } from '@angular/core';
import { ApiError } from '../../../types/error';

@Component({
  selector: 'app-server-error',
  imports: [],
  templateUrl: './server-error.html',
  styleUrl: './server-error.css',
})
export class ServerError implements OnInit {
  protected error: ApiError | null = null;  
  protected showDetails = false;

  ngOnInit() {
    this.error = history.state?.error ?? null;  
  }

  detailsToggle() {
    this.showDetails = !this.showDetails;
  }
}
