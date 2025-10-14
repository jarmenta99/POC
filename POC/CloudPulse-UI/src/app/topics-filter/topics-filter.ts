import { Component, EventEmitter, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { ApiService } from '../api';

@Component({
  selector: 'app-topics-filter',
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatSelectModule, MatButtonModule],
  templateUrl: './topics-filter.html',
  styleUrls: ['./topics-filter.scss']
})
export class TopicsFilterComponent implements OnInit {
  topics: any[] = [];
  selectedTopic: string = '';
  selectedSubscription: string = '';
  environment: string = 'Qa';

  @Output() filterChanged = new EventEmitter<any>();

  constructor(private api: ApiService) {}

  ngOnInit() {
    console.log('TopicsFilterComponent initialized');
    console.log('Calling getTopics with environment:', this.environment);
    
    this.api.getTopics(this.environment).subscribe({
      next: (data) => {
        console.log('Topics API response:', data);
        this.topics = data.topics || data.Topics || data;
        console.log('Processed topics array:', this.topics);
        
        if (!this.topics || this.topics.length === 0) {
          console.warn('No topics found in response');
        }
      },
      error: (error) => {
        console.error('Error loading topics:', error);
        console.error('Error status:', error.status);
        console.error('Error message:', error.message);
        console.error('Error details:', error.error);
        
        // Fallback: Use mock data for development
        console.log('Using mock data for development');
        this.topics = [
          {
            TopicName: "feedtransactionmanager",
            Subscriptions: [
              { SubscriptionName: "crewqualsync" },
              { SubscriptionName: "test-subscription" }
            ]
          },
          {
            TopicName: "user-events",
            Subscriptions: [
              { SubscriptionName: "user-events-sub1" },
              { SubscriptionName: "user-events-sub2" }
            ]
          }
        ];
      }
    });
  }

  onSubmit() {
    console.log('=== GET MESSAGES BUTTON CLICKED ===');
    console.log('Selected Topic:', this.selectedTopic);
    console.log('Selected Subscription:', this.selectedSubscription);
    console.log('Environment:', this.environment);
    console.log('Topics array:', this.topics);
    
    if (!this.selectedTopic || this.selectedTopic === '') {
      console.warn('No topic selected - selectedTopic is:', this.selectedTopic);
      alert('Please select a topic first');
      return;
    }
    
    if (!this.selectedSubscription || this.selectedSubscription === '') {
      console.warn('No subscription selected - selectedSubscription is:', this.selectedSubscription);
      alert('Please select a subscription first');
      return;
    }
    
    const filterData = {
      topicName: this.selectedTopic,
      subscriptionName: this.selectedSubscription,
      azureEnvironment: this.environment
    };
    
    console.log('✅ Validation passed! Emitting filter data:', filterData);
    this.filterChanged.emit(filterData);
    console.log('✅ Event emitted successfully');
  }

  getSubscriptionsForSelectedTopic() {
    const selectedTopicObj = this.topics.find(t => t.TopicName === this.selectedTopic);
    return selectedTopicObj?.Subscriptions || [];
  }
}