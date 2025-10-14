import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ApiService } from './api';
import { TopicsFilterComponent } from './topics-filter/topics-filter';
import { MessagesTableComponent } from './messages-table/messages-table';

@Component({
  selector: 'app-root',
  imports: [TopicsFilterComponent, MessagesTableComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App {
  messages: any[] = [];
  title = 'CloudPulse-UI';

  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  onFilter(filter: any) {
    console.log('=== APP COMPONENT RECEIVED FILTER EVENT ===');
    console.log('Filter received:', filter);
    
    // Validate that required fields are present
    if (!filter.azureEnvironment || !filter.topicName || !filter.subscriptionName) {
      console.error('Missing required filter parameters:', filter);
      alert('Missing required parameters: ' + JSON.stringify(filter));
      return;
    }
    
    const requestParams = {
      AzureEnvironment: filter.azureEnvironment,
      TopicName: filter.topicName,
      SubscriptionName: filter.subscriptionName,
      MaxMessages: 10,
      DeadLetter: false
    };
    
    console.log('✅ About to call API with params:', requestParams);
    
    this.api.getMessages(requestParams).subscribe({
      next: (data) => {
        console.log('✅ Messages received successfully:', data);
        this.messages = Array.isArray(data) ? data : [data];
        this.cdr.detectChanges(); // Manually trigger change detection
      },
      error: (error) => {
        console.error('❌ Error fetching messages:', error);
        console.error('Error status:', error.status);
        console.error('Error details:', error.error);
        
        // For testing, set some mock messages
        console.log('Setting mock messages for testing...');
        this.messages = [
          { MessageId: 'mock-1', Message: 'Test message 1', Timestamp: new Date().toISOString() },
          { MessageId: 'mock-2', Message: 'Test message 2', Timestamp: new Date().toISOString() }
        ];
        this.cdr.detectChanges(); // Manually trigger change detection
      }
    });
  }
}