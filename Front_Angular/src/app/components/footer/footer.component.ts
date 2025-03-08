import { Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss']
})
export class FooterComponent implements OnInit {
  @Input() companyName: string = 'FutureApp';
  currentYear: number = new Date().getFullYear();

  constructor() { }

  ngOnInit(): void {
  }
}