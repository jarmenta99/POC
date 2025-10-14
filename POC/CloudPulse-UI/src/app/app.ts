import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from './api';
import { TopicsFilterComponent } from './topics-filter/topics-filter';
import { MessagesTableComponent } from './messages-table/messages-table';

@Component({
  selector: 'app-root',
  imports: [CommonModule, MatProgressSpinnerModule, TopicsFilterComponent, MessagesTableComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App {
  messages: any[] = [];
  title = 'CloudPulse';
  isLoadingMessages: boolean = false;
  isDeadLetterMode: boolean = false;

  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  onFilter(filter: any) {
    // Validate that required fields are present
    if (!filter.azureEnvironment || !filter.topicName || !filter.subscriptionName) {
      console.error('Missing required filter parameters:', filter);
      alert('Missing required parameters: ' + JSON.stringify(filter));
      return;
    }
    
    // Store the dead letter mode for the table
    this.isDeadLetterMode = filter.deadLetter || false;
    
    const requestParams = {
      AzureEnvironment: filter.azureEnvironment,
      TopicName: filter.topicName,
      SubscriptionName: filter.subscriptionName,
      MaxMessages: 2752, // Max allowed by API
      DeadLetter: filter.deadLetter || false
    };
    
    console.log('✅ About to call API with params:', requestParams);
    this.isLoadingMessages = true;
    this.cdr.detectChanges(); // Update UI to show loading state
    
    this.api.getMessages(requestParams).subscribe({
      next: (data) => {
        console.log('✅ Messages received successfully:', data);
        this.messages = Array.isArray(data) ? data : [data];
        this.isLoadingMessages = false;
        this.cdr.detectChanges(); // Manually trigger change detection
      },
      error: (error) => {
        console.error('❌ Error fetching messages:', error);
        console.error('Error status:', error.status);
        console.error('Error details:', error.error);
        
        this.isLoadingMessages = false;
        this.cdr.detectChanges(); // Manually trigger change detection
      }
    });
  }
}