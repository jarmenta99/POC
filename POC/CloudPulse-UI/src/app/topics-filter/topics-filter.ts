import { Component, EventEmitter, Output, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { Subject, takeUntil } from 'rxjs';
import { ApiService } from '../api';

@Component({
  selector: 'app-topics-filter',
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatSelectModule, MatButtonModule, MatProgressSpinnerModule, MatIconModule],
  templateUrl: './topics-filter.html',
  styleUrls: ['./topics-filter.scss']
})
export class TopicsFilterComponent implements OnInit, OnDestroy {
  topics: any[] = [];
  selectedTopic: string = '';
  selectedSubscription: string = '';
  environment: string = 'Qa';
  isDeadLetter: boolean = false;
  isLoadingTopics: boolean = false;
  
  private destroy$ = new Subject<void>();

  @Output() filterChanged = new EventEmitter<any>();


  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    console.log('TopicsFilterComponent initialized');
    this.loadTopics();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadTopics() {
    console.log('Loading topics for environment:', this.environment);
    console.log('Setting isLoadingTopics to true');
    this.isLoadingTopics = true;
    this.cdr.detectChanges(); // Force change detection
    
    this.api.getTopics(this.environment)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
      next: (data) => {
        console.log('Topics API response:', data);
        this.topics = data.topics || data.Topics || data;
        console.log('Processed topics array:', this.topics);
        
        if (!this.topics || this.topics.length === 0) {
          console.warn('No topics found in response');
        }
        
        // Reset selections when topics change
        this.selectedTopic = '';
        this.selectedSubscription = '';
        
        console.log('Setting isLoadingTopics to false');
        this.isLoadingTopics = false;
        this.cdr.detectChanges(); // Force change detection
        console.log('Loading state after reset:', this.isLoadingTopics);
      },
      error: (error) => {
        console.error('Error loading topics:', error);
        console.error('Error status:', error.status);
        console.error('Error message:', error.message);
        console.error('Error details:', error.error);
        
        // Clear topics on error
        this.topics = [];
        this.selectedTopic = '';
        this.selectedSubscription = '';
        
        console.log('Setting isLoadingTopics to false (error case)');
        this.isLoadingTopics = false;
        this.cdr.detectChanges(); // Force change detection
        console.log('Loading state after error reset:', this.isLoadingTopics);
      }
    });
  }

  onEnvironmentChange() {
    console.log('Environment changed to:', this.environment);
    this.loadTopics();
  }

  onTopicChange() {
    console.log('Topic changed to:', this.selectedTopic);
    // Reset subscription when topic changes
    this.selectedSubscription = '';
  }

  onSubmit() {
    console.log('=== GET MESSAGES BUTTON CLICKED ===');
    console.log('Selected Topic:', this.selectedTopic);
    console.log('Selected Subscription:', this.selectedSubscription);
    console.log('Environment:', this.environment);
    console.log('deadLetter:', this.isDeadLetter);
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
      azureEnvironment: this.environment,
      deadLetter: this.isDeadLetter
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