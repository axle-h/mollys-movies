import { Component, OnInit } from '@angular/core';
import {
  LiveTransmissionStatus,
  MovieTorrentService,
  TransmissionContext,
  TransmissionService,
  TransmissionStatusCode,
} from '../../api';
import { map, mergeAll } from 'rxjs/operators';
import { from, of } from 'rxjs';

interface TransmissionContextAndStatus extends TransmissionContext {
  liveStatus: LiveTransmissionStatus;
}

@Component({
  selector: 'mm-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss'],
})
export class ListComponent implements OnInit {
  page?: number;
  limit?: number;
  count?: number;
  data?: TransmissionContextAndStatus[] | null;

  constructor(
    private readonly transmissionService: TransmissionService,
    private readonly movieTorrentService: MovieTorrentService,
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.transmissionService.getAllContexts({ page: this.page, limit: this.limit }).subscribe(x => {
      this.page = x.page;
      this.limit = x.limit;
      this.count = x.count;
      this.data = x.data as TransmissionContextAndStatus[];
      const started =
        this.data && this.data.filter(c => c.status === TransmissionStatusCode.Started);
      if (!started || started.length === 0) {
        return of(x);
      }
      const $liveStatuses = started.map(c =>
        this.movieTorrentService
          .getLiveTransmissionStatus({ movieId: c.movieId, torrentId: c.torrentId })
          .pipe(map(s => ({ context: c, status: s }))),
      );
      from($liveStatuses)
        .pipe(mergeAll())
        .subscribe(({ context, status }) => (context.liveStatus = status));
    });
  }

  getStatusIcon(context: TransmissionContext) {
    switch (context.status) {
      case TransmissionStatusCode.Complete:
        return 'fas fa-check text-success';
      case TransmissionStatusCode.Downloaded:
        return 'fas fa-cog fa-spin text-secondary';
      case TransmissionStatusCode.Started:
        return 'fas fa-sync fa-spin';
    }
  }
}
