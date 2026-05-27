import { Component } from '@angular/core';
import { ConcertsComponent } from './concerts/concerts';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ConcertsComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {}