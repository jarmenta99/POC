import { Component, Input, OnChanges, SimpleChanges, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-messages-table',
  imports: [
    CommonModule, 
    FormsModule,
    MatTableModule, 
    MatPaginatorModule, 
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatExpansionModule,
    MatCardModule,
    MatSelectModule,
    MatTooltipModule
  ],
  templateUrl: './messages-table.html',
  styleUrls: ['./messages-table.scss']
})
export class MessagesTableComponent implements OnChanges, AfterViewInit {
  @Input() messages: any[] = [];
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  dataSource = new MatTableDataSource<any>();
  displayedColumns: string[] = ['MessageId', 'Message', 'actions'];
  
  // Filtering
  filterText: string = '';
  selectedProperties: string[] = [];
  availableProperties: string[] = [];
  
  // JSON view
  expandedElement: any | null = null;
  
  constructor() {
    console.log('MessagesTableComponent constructor called');
    // Page size options will be set on the paginator
  }

  ngOnChanges(changes: SimpleChanges) {
    console.log('MessagesTableComponent ngOnChanges called with:', changes);
    if (changes['messages']) {
      console.log('Messages changed:', this.messages);
      console.log('Messages length:', this.messages?.length);
      this.updateDataSource();
      this.extractAvailableProperties();
    }
  }

  ngAfterViewInit() {
    console.log('MessagesTableComponent ngAfterViewInit called');
    console.log('Paginator available:', !!this.paginator);
    console.log('Sort available:', !!this.sort);
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    
    // Custom filter predicate for property filtering
    this.dataSource.filterPredicate = (data: any, filter: string) => {
      if (!filter) return true;
      
      // If specific properties are selected, only search in those
      if (this.selectedProperties.length > 0) {
        return this.selectedProperties.some(prop => {
          const value = this.getNestedProperty(data, prop);
          return value && value.toString().toLowerCase().includes(filter.toLowerCase());
        });
      }
      
      // Otherwise search in all properties
      return JSON.stringify(data).toLowerCase().includes(filter.toLowerCase());
    };
  }

  private updateDataSource() {
    console.log('Updating data source with messages:', this.messages);
    this.dataSource.data = this.messages;
    console.log('DataSource.data after update:', this.dataSource.data);
    console.log('DataSource.data.length:', this.dataSource.data.length);
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
  }

  private extractAvailableProperties() {
    const properties = new Set<string>();
    
    this.messages.forEach(message => {
      this.extractPropertiesRecursive(message, '', properties);
    });
    
    this.availableProperties = Array.from(properties).sort();
  }

  private extractPropertiesRecursive(obj: any, prefix: string, properties: Set<string>) {
    if (obj && typeof obj === 'object') {
      Object.keys(obj).forEach(key => {
        const fullPath = prefix ? `${prefix}.${key}` : key;
        properties.add(fullPath);
        
        if (typeof obj[key] === 'object' && obj[key] !== null) {
          this.extractPropertiesRecursive(obj[key], fullPath, properties);
        }
      });
    }
  }

  private getNestedProperty(obj: any, path: string): any {
    return path.split('.').reduce((o, p) => o && o[p], obj);
  }

  applyFilter() {
    this.dataSource.filter = this.filterText.trim();
    
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  clearFilter() {
    this.filterText = '';
    this.selectedProperties = [];
    this.dataSource.filter = '';
  }

  toggleExpansion(element: any) {
    this.expandedElement = this.expandedElement === element ? null : element;
  }

  formatJson(obj: any): string {
    try {
      return JSON.stringify(obj, null, 2);
    } catch (error) {
      return 'Invalid JSON';
    }
  }

  stringifyJson(obj: any): string {
    try {
      return JSON.stringify(obj);
    } catch (error) {
      return String(obj);
    }
  }

  isJsonString(str: string): boolean {
    try {
      JSON.parse(str);
      return true;
    } catch (error) {
      return false;
    }
  }

  parseMessage(message: string): any {
    try {
      return JSON.parse(message);
    } catch (error) {
      return { rawMessage: message };
    }
  }

  copyToClipboard(text: string) {
    navigator.clipboard.writeText(text).then(() => {
      console.log('Copied to clipboard');
    });
  }
}