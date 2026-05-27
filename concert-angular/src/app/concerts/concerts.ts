import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-concerts',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './concerts.html',
  styleUrl: './concerts.css'
})
export class ConcertsComponent implements OnInit {
  concerts: any[] = [];
  loading = true;
  error = '';

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.http.get<any[]>('https://localhost:7283/api/concerts').subscribe({
      next: (data) => {
        this.concerts = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = 'Error: ' + err.message;
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}