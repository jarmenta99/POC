import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';

@Component({
  selector: 'app-messages-table',
  imports: [CommonModule, MatTableModule],
  templateUrl: './messages-table.html',
  styleUrls: ['./messages-table.scss']
})
export class MessagesTableComponent {
  @Input() messages: any[] = [];
}